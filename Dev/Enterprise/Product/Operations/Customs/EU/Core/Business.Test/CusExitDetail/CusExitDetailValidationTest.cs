using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class CusExitDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCED_CustomsOffice()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, "Spain", eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Test123", "Test123", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			Factory.Save();

			CombineAssertions(() =>
			{
				var exitHeader = Factory.NewWithValidTestData<CusExitControlHeader>();
				var exitDetail = exitHeader.CusExitDetails.AddNew();
				exitDetail.CED_CustomsOffice = "Test";

				AssertHasMessageErrorContaining(exitDetail.CED_CustomsOfficeInfo, "The code you have selected is not in the list.");

				exitDetail.CED_CustomsOffice = "Test123";
				AssertNoMessageErrorContaining(exitDetail.CED_CustomsOfficeInfo, "The code you have selected is not in the list.");
			});
		}

		[TestDate(2020, 7, 1, 23, 59, 59)]
		public void TestCheckCED_ArrivalNotificationDate()
		{
			const string message = "Notification Date is older than the current date";
			var exitDetail = Factory.New<CusExitDetail>();
			CombineAssertions(() =>
			{
				exitDetail.CED_ArrivalNotificationDate = new ZDateTime(2020, 1, 1, 23, 41, 00);
				AssertHasWarning("CED_ArrivalNotificationDate earlier than today", exitDetail.CED_ArrivalNotificationDateInfo, message);

				exitDetail.CED_ArrivalNotificationDate = new ZDateTime(2020, 7, 1, 13, 21, 00);
				AssertNoWarning("CED_ArrivalNotificationDate is today", exitDetail.CED_ArrivalNotificationDateInfo, message);
			});
		}

		public void TestCheckCED_Status()
		{
			var exitDetail = Factory.New<CusExitDetail>();
			exitDetail.CED_Status = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertHasError(exitDetail.CED_StatusInfo, "Please enter a value.");
				AssertNoError(exitDetail.CED_StatusInfo, "Status length must be 3");

				exitDetail.CED_Status = "AA";
				AssertNoError(exitDetail.CED_StatusInfo, "Please enter a value.");
				AssertHasError(exitDetail.CED_StatusInfo, "Status length must be 3");

				exitDetail.CED_Status = "AAA";
				AssertNoError(exitDetail.CED_StatusInfo, "Please enter a value.");
				AssertNoError(exitDetail.CED_StatusInfo, "Status length must be 3");
			});
		}
	}
}
