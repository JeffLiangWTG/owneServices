using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(ManualCancelForm))]
	sealed class ManualCancelFormTest : ZFormBasherTest
	{
		[TestDate(2015, 12, 31, 16, 03, 00)]
		public void TestSaveButtonClick()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
				Factory.Save();

				var note = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.ManualCancel.Description).FirstOrDefault();
				if (note != null)
				{
					note.Delete();
					Factory.Save();
				}

				string expectedNoteText = string.Format(@"2015-12-23 15:02
TEST CANCEL REASON
{0}
2015-12-31 16:03", Env.CurrentUser.LoginName);
				using (var form = new ManualCancelForm(declaration, Factory))
				{
					form.BusinessEntity.ManualReleaseReason = "TEST CANCEL REASON";
					form.BusinessEntity.ManualReleaseDate = new ZDateTime(2015, 12, 23, 15, 02, 00);
					form.Show();
					form.SaveButton.PerformClick();
					note = declaration.Notes.FindByDescription(PredefinedNoteTypes.Instance.ManualCancel.Description).FirstOrDefault();
					AssertNotNull("CustomsManualStatus note should not be null", note);
					AssertEquals("Note Text", expectedNoteText, note.ST_NoteText);
				}
			}
		}

		public void TestFormText()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ManualCancelForm(declaration, Factory))
			{
				form.Show();
				AssertEquals("Manual Cancel", form.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestNoConcurrencyErrorWhenSaving()
		{
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			declaration.JE_CustomsOffice = "351";
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "00006789";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 10m;
			declaration.JE_DateOfArrival = new ZDateTime(2021, 04, 01, 12, 35, 35);
			Factory.Save();

			using (var form = new ManualCancelForm(declaration, Factory))
			{
				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var loadedEntryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
				loadedEntryHeader.CH_Status = MessageStatusList.Codes.AwaitingChange;
				newFactory.Save();

				form.BusinessEntity.ManualReleaseReason = "TEST CANCEL REASON";
				form.BusinessEntity.ManualReleaseDate = new ZDateTime(2021, 04, 09, 14, 02, 00);
				form.Show();
				form.SaveButton.PerformClick();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			}
		}

		protected override Form GetFormToBashCore() => new ManualCancelForm(Factory.New<JobDeclaration>(), Factory);
	}
}
