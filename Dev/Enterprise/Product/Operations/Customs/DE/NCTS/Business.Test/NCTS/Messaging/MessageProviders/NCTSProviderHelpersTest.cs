using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSProviderHelpersTest : TestCaseWithFactory
	{
		public void TestGetSecurity()
		{
			CombineAssertions(() =>
			{
				AssertEquals(NctsTypeOfSecurityList.Codes.NON, "0",
					NCTSProviderHelpers.GetSecurity(NctsTypeOfSecurityList.Codes.NON));

				AssertEquals(NctsTypeOfSecurityList.Codes.ENT, "1",
					NCTSProviderHelpers.GetSecurity(NctsTypeOfSecurityList.Codes.ENT));

				AssertEquals(NctsTypeOfSecurityList.Codes.EXI, "2",
					NCTSProviderHelpers.GetSecurity(NctsTypeOfSecurityList.Codes.EXI));

				AssertEquals(NctsTypeOfSecurityList.Codes.BTH, "3",
					NCTSProviderHelpers.GetSecurity(NctsTypeOfSecurityList.Codes.BTH));

				AssertEquals("Invalid", null, NCTSProviderHelpers.GetSecurity("INV"));
			});
		}

		public void TestFindGoodsItemWithMainPack()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			var cargoDesc = bill.GoodsItems.AddNew();
			cargoDesc.BY_LineNo = 1;
			var package = cargoDesc.Packages.AddNew();
			package.B5_UnitType = "BX";
			package.B5_UnitCount = 3;
			package.B5_MarksAndNumbers = "MarksAndNumbers";

			cargoDesc.BY_IsMainPack = true;
			var additionalCargoDesc = bill.GoodsItems.AddNew();
			additionalCargoDesc.BY_LineNo = 2;
			additionalCargoDesc.BY_IsMainPack = true;
			var additionalPackage = additionalCargoDesc.Packages.AddNew();
			additionalPackage.B5_UnitType = "BX";
			additionalPackage.B5_UnitCount = 5;
			additionalPackage.B5_MarksAndNumbers = "MarksAndNumbers2";

			var cargoDescByPack = bill.GoodsItems.AddNew();
			cargoDescByPack.BY_LineNo = 3;
			var packageByPack = cargoDescByPack.Packages.AddNew();
			packageByPack.B5_UnitType = "BX";
			packageByPack.B5_UnitCount = 0;
			packageByPack.B5_MarksAndNumbers = "MarksAndNumbers2";

			AssertEquals(2, NCTSProviderHelpers.FindGoodsItemWithMainPack(packageByPack));
		}

		public void TestIsDEMRN_Match()
		{
			AssertEquals(true, NCTSProviderHelpers.IsDEMRN("23DE1234567890"));
		}

		public void TestIsDEMRN_NotAtStart()
		{
			AssertEquals(false, NCTSProviderHelpers.IsDEMRN("BB23DE1234567890"));
		}

		public void TestIsDEMRN_NoDigits()
		{
			AssertEquals(false, NCTSProviderHelpers.IsDEMRN("2BDE1234567890"));
		}

		public void TestIsDEMRN_NoDE()
		{
			AssertEquals(false, NCTSProviderHelpers.IsDEMRN("23NL1234567890"));
		}

		public void TestIsDEMRN_NullInput()
		{
			AssertEquals(false, NCTSProviderHelpers.IsDEMRN(null));
		}

		public void TestUnloadedStatusInNewMis()
		{
			CombineAssertions(() =>
			{
				AssertEquals("NEW", true, NCTSProviderHelpers.UnloadedStatusInNewMis(NctsUnloadedStateList.Codes.NEW));
				AssertEquals("MIS", true, NCTSProviderHelpers.UnloadedStatusInNewMis(NctsUnloadedStateList.Codes.MIS));
				AssertEquals("DIF", false, NCTSProviderHelpers.UnloadedStatusInNewMis(NctsUnloadedStateList.Codes.DIF));
				AssertEquals("DEC", false, NCTSProviderHelpers.UnloadedStatusInNewMis(NctsUnloadedStateList.Codes.DEC));
			});
		}

		public void TestUnloadedStatusInNewMisDif()
		{
			CombineAssertions(() =>
			{
				AssertEquals("NEW", true, NCTSProviderHelpers.UnloadedStatusInNewMisDif(NctsUnloadedStateList.Codes.NEW));
				AssertEquals("MIS", true, NCTSProviderHelpers.UnloadedStatusInNewMisDif(NctsUnloadedStateList.Codes.MIS));
				AssertEquals("DIF", true, NCTSProviderHelpers.UnloadedStatusInNewMisDif(NctsUnloadedStateList.Codes.DIF));
				AssertEquals("DEC", false, NCTSProviderHelpers.UnloadedStatusInNewMisDif(NctsUnloadedStateList.Codes.DEC));
			});
		}

		public void TestUnloadedStatusInNewDif()
		{
			CombineAssertions(() =>
			{
				AssertEquals("NEW", true, NCTSProviderHelpers.UnloadedStatusInNewDif(NctsUnloadedStateList.Codes.NEW));
				AssertEquals("MIS", false, NCTSProviderHelpers.UnloadedStatusInNewDif(NctsUnloadedStateList.Codes.MIS));
				AssertEquals("DIF", true, NCTSProviderHelpers.UnloadedStatusInNewDif(NctsUnloadedStateList.Codes.DIF));
				AssertEquals("DEC", false, NCTSProviderHelpers.UnloadedStatusInNewDif(NctsUnloadedStateList.Codes.DEC));
			});
		}

		public void TestGetEffectiveExchangeRate()
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.CustomsRate;
			exchangeRate.RE_RX_NKExCurrency = "YSN";

			CombineAssertions(() =>
			{
				AssertNotNull("Valid", NCTSProviderHelpers.GetEffectiveExchangeRate("YSN", Factory));
				AssertNull("Invalid", NCTSProviderHelpers.GetEffectiveExchangeRate("SSS", Factory));
			});
		}
	}
}
