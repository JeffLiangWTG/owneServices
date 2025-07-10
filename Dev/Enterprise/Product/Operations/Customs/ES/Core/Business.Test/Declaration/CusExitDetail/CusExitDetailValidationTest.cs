using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class CusExitDetailValidationTest : EU.Business.Testing.CusExitDetailValidationTest
	{
		public new void TestCheckCED_Status()
		{
			var emptyError = "Please enter a value.";
			var lengthError = "Please enter a value.";

			var exitDetail = Factory.New<CusExitDetail>();
			CombineAssertions(() =>
			{
				exitDetail.CED_Status = ZString.Empty;
				AssertEquals(false, exitDetail.CED_StatusInfo.HasError(emptyError));
				AssertEquals(false, exitDetail.CED_StatusInfo.HasError(lengthError));

				exitDetail.CED_Status = "AA";
				AssertEquals(false, exitDetail.CED_StatusInfo.HasError(emptyError));
				AssertEquals(false, exitDetail.CED_StatusInfo.HasError(lengthError));

				exitDetail.CED_Status = "AAA";
				AssertEquals(false, exitDetail.CED_StatusInfo.HasError(emptyError));
				AssertEquals(false, exitDetail.CED_StatusInfo.HasError(lengthError));
			});
		}

		public new void TestCheckCED_ArrivalNotificationDate()
		{
			var exitDetail = Factory.New<CusExitDetail>();
			exitDetail.CED_ArrivalNotificationDate = ZDate.Today.AddDays(-2);
			AssertEquals("There should be no warning in ES when Arrival Notification Date is older than today", false, exitDetail.CED_ArrivalNotificationDateInfo.HasWarning("Notification Date is older than the current date"));
		}

		public void TestCheckCED_ArrivalNotificationPlace()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GoodsOfLocationType, "Location Codes");
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: grouping);

			helper.CreateCusCodeList("ES", RefCusCodeListTypes.Codes.GoodsOfLocationType, "01", "Test 1", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateCusCodeList("EUN", RefCusCodeListTypes.Codes.GoodsOfLocationType, "02", "Test 2", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var exitHeader = Factory.New<CusExitControlHeader>();
				exitHeader.CEH_ParentTableCode = "JE";
				exitHeader.CEH_ReferenceNumber = "123";
				var exitDetail = Factory.New<CusExitDetail>();
				exitDetail.CED_CEH = exitHeader.PK;
				exitDetail.CED_Status = "TTT";

				exitDetail.CED_ArrivalNotificationPlace = "";
				AssertNoMessageErrors(exitDetail.CED_ArrivalNotificationPlaceInfo);
				exitDetail.CED_ArrivalNotificationPlace = "01";
				AssertNoMessageErrors(exitDetail.CED_ArrivalNotificationPlaceInfo);
				exitDetail.CED_ArrivalNotificationPlace = "A";
				AssertHasMessageErrorContaining(exitDetail.CED_ArrivalNotificationPlaceInfo, ListValidation.InvalidCodeMessageError);
				exitDetail.CED_ArrivalNotificationPlace = "02";
				AssertHasMessageErrorContaining(exitDetail.CED_ArrivalNotificationPlaceInfo, ListValidation.InvalidCodeMessageError);
			}
		}
	}
}
