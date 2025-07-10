using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	class ExitNotificationMessageSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckMessageType()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(messageSendingAction.MessageTypeInfo, "XX", ExitSummaryMessageTypeList.Codes.Notification);
		}

		public void TestCustomsOffice()
		{
			CombineAssertions(() =>
			{
				messageSendingAction.IntendedExitCustomsOffice = ZString.Empty;
				messageSendingAction.Redirection = false;
				AssertNoMessageErrorContaining("no message b/c no redirection", messageSendingAction.IntendedExitCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

				messageSendingAction.Redirection = true;
				AssertHasMessageErrorContaining("redirection, customs office required", messageSendingAction.IntendedExitCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

				messageSendingAction.IntendedExitCustomsOffice = "Office1";
				AssertNoMessageErrorContaining("no error not entered", messageSendingAction.IntendedExitCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("invalid office", messageSendingAction.IntendedExitCustomsOfficeInfo, ListValidation.InvalidCodeMessageError.ToString());

				messageSendingAction.IntendedExitCustomsOffice = "DE004324";
				AssertNoMessageErrorContaining("all correct, no error", messageSendingAction.IntendedExitCustomsOfficeInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			CreateCustomsOffice();

			var exitHeader = Factory.New<CusExitControlHeader>();
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			messageSendingAction = new ExitNotificationMessageSendingAction(exitDetail);
			messageSendingAction.MessageType = ExitSummaryMessageTypeList.Codes.Notification;
		}

		ExitNotificationMessageSendingAction messageSendingAction;

		void CreateCustomsOffice()
		{
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004323", "GERMAN OFFICE1", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CompetentAuthorityOfEnquiry);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004324", "GERMAN OFFICE2", ZDateTime.BrettsBirthday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			Factory.Save();
		}
	}
}
