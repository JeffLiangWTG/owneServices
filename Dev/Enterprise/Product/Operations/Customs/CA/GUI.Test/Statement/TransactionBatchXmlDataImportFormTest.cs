using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(TransactionBatchXmlDataImportForm))]
	sealed class TransactionBatchXmlDataImportFormTest : ZFormBasherTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 01, 29, 05, 30, 15)]
		public void TestImportData()
		{
			using (var frm = new TransactionBatchXmlDataImportFormForTest())
			{
				frm.Show();
				Application.DoEvents();

				var files = frm.GetFilesForTest();

				var textBox = (TextBox)frm.Controls.Find("ProgressTextBox", true).First();
				Assert("Precondition", string.IsNullOrWhiteSpace(textBox.Text));

				var button = (ZButton)frm.Controls.Find("ImportXmlFilesButton", true).First();
				button.PerformClick();

				var actualLog = textBox.Text.Trim();

				AssertContains("[Success] Message Number:ARL18012905301500", actualLog);
				AssertContains("[Success] Message Number:ARL18012905301501", actualLog);
				AssertContains("[Success] Message Number:ARL18012905301502", actualLog);

				AssertContains("SoAUniversalTransactionBatch.xml", actualLog);
				AssertContains("SoAUniversalTransactionBatch2.xml", actualLog);
				AssertContains("SoAUniversalTransactionBatch3.xml", actualLog);

				var factory = new BusinessObjectFactory();

				var interchanges = factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_To, "ARLMSGTST"));
				AssertEquals("Should create 3 EDIInterchange from these files.", 3, interchanges.Length);

				foreach (var interchange in interchanges)
				{
					var interchangeNum = interchange.EI_InterchangeNum;

					CombineAssertions(() =>
					{
						Assert("EI_InterchangeNum", interchangeNum.StartsWith("ARL180129053015"));

						AssertEquals("EI_ReceiveTransmit of " + interchangeNum, interchange.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
						AssertEquals("EI_ApplicationCode of " + interchangeNum, interchange.EI_ApplicationCode, EDIInterchange.ApplicationCodes.CAIMP);
						AssertEquals("EI_ReceiveTransmit of " + interchangeNum, interchange.EI_GB, GlbBranch.CurrentBranch.PK);
						AssertEquals("EI_GB of " + interchangeNum, interchange.EI_To, "ARLMSGTST");
						AssertEquals("EI_From of " + interchangeNum, interchange.EI_From, GlbCompany.CurrentCompany.LicenceEnterpriseCode);
					});

					var index = int.Parse(interchangeNum.Right(1));
					var file = files[index];

					AssertEquals("Should create 1 EDIMessage.", 1, interchange.ContainedMessages.Count);

					var message = interchange.ContainedMessages[0];
					var messageNum = message.EM_MessageNum;

					CombineAssertions(() =>
					{
						AssertEquals("EM_MessageNum", interchangeNum, messageNum);

						AssertEquals("EM_GB of " + messageNum, message.EM_GB, GlbBranch.CurrentBranch.PK);
						AssertEquals("EM_GE of " + messageNum, message.EM_GE, GlbDepartment.CurrentDepartment.PK);
						AssertEquals("EM_ApplicationCode of " + messageNum, message.EM_ApplicationCode, EDIMessage.ApplicationCodes.UniversalDataMessaging);
						AssertEquals("EM_MessageType of " + messageNum, message.EM_MessageType, EDIMessageTypeList.Codes.XDC);
						AssertEquals("EM_MessageSubType of " + messageNum, message.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch);
						AssertEquals("EM_ReceiveTransmit of " + messageNum, message.EM_ReceiveTransmit, EDIInterchange.Direction.Receive);
						AssertEquals("EM_Status of " + messageNum, message.EM_Status, EDIMessageStatusList.Codes.Queued);
						AssertEquals("EM_EI of " + messageNum, message.EM_EI, interchange.PK);

						var expectedText = File.ReadAllText(file);
						AssertXMLEquals("EM_MessageNText of " + messageNum, expectedText, message.EM_MessageText);
					});
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestControlVisible()
		{
			using (var frm = new TransactionBatchXmlDataImportFormForTest())
			{
				frm.Show();
				Application.DoEvents();

				var importFromFileButton = frm.Controls.Find("ImportFromFileButton", true).First();
				Assert("Should be false as it's not used on this form.", !importFromFileButton.Visible);

				var onlySaveDataWhenNoRecordsHaveErrorsCheckBox = frm.Controls.Find("OnlySaveDataWhenNoRecordsHaveErrorsCheckBox", true).First();
				Assert("Should be false as it's not used on this form.", !onlySaveDataWhenNoRecordsHaveErrorsCheckBox.Visible);

				var progressTextBox = frm.Controls.Find("ProgressTextBox", true).First();
				Assert("Should be true as it's used on this form.", progressTextBox.Visible);

				var importXmlFilesButton = frm.Controls.Find("ImportXmlFilesButton", true).First();
				Assert("Should be true as it's used on this form.", importXmlFilesButton.Visible);
			}
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control) => control.Name == "ProgressTextBox";

		protected override Form GetFormToBashCore() => new TransactionBatchXmlDataImportForm();

		sealed class TransactionBatchXmlDataImportFormForTest : TransactionBatchXmlDataImportForm
		{
			public string[] GetFilesForTest() => GetFiles();

			protected override string[] GetFiles() => new[]
			{
				BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\SoAUniversalTransactionBatch.xml",
				BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\SoAUniversalTransactionBatch2.xml",
				BaseSourcePath + @"Enterprise\Product\Operations\Customs\CA\Business\MessageProcessors\TransactionBatchMessageProcessor\TestFiles\SoAUniversalTransactionBatch3.xml",
			};
		}
	}
}
