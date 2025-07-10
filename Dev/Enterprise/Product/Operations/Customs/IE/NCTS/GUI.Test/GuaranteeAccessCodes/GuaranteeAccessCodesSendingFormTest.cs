using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.GUI.Testing
{
	[TestedType(typeof(GuaranteeAccessCodesSendingForm))]
	sealed class GuaranteeAccessCodesSendingFormTest : ZFormBasherTest
	{
		public void TestShowFormType()
		{
			GuaranteeAccessCodesSendingForm.ShowForm(Factory.NewWithValidTestData<CusGuaranteeHeader>());
			AssertType<GuaranteeAccessCodesSendingForm>("Dialog form type = GuaranteeAccessCodesSendingForm", ZFormModaliser.LastFormShownDialogForTest);
		}

		[RequiresSTA]
		public void TestSendButton()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var codeIEDUB100 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IEDUB100", "DUBLIN PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeIEDUB100.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "GUA");
			Factory.Save();

			using (var form = new GuaranteeAccessCodesSendingForm(sendingActionParent))
			using (cusGuaranteeHeader.Branch.SetAsTemporaryContext())
			{
				form.Show();
				var sendingAction = sendingActionParent.SendingObjectsCollection[0];
				sendingAction.OfficeOfGuarantee = "IEDUB100";
				sendingAction.CurrentCode = "QWE";
				sendingAction.NewAccessCode = "QAZ";
				sendingAction.MasterCode = "ZXC";
				CombineAssertions(() =>
				{
					var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
					sendButton.PerformClick();
					var lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals("Message count", 1, cusGuaranteeHeader.Messages.Count);
					AssertEquals("Application code", "IEN", cusGuaranteeHeader.Messages[0].EM_ApplicationCode);
					AssertEquals("Message Type", "026", cusGuaranteeHeader.Messages[0].EM_MessageType);
					AssertEquals("Direction", "TRX", cusGuaranteeHeader.Messages[0].EM_ReceiveTransmit);
					AssertEquals("Message has been queued/sent through Customs Guarantee Module.", lastMessage);
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new GuaranteeAccessCodesSendingForm(sendingActionParent);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testData = NctsTestDataProvider.CreateNctsGuaranteeWithCusGuaranteeHeader(Factory);
			cusGuaranteeHeader = testData.cusGuaranteeHeader;
			nctsGuarantee = testData.nctsGuarantee;
			sendingActionParent = new GuaranteeAccessCodesSendingActionParent(nctsGuarantee.CusGuarantee);
		}
		CusGuaranteeHeader cusGuaranteeHeader;
		NctsGuarantee nctsGuarantee;
		GuaranteeAccessCodesSendingActionParent sendingActionParent;
	}
}
