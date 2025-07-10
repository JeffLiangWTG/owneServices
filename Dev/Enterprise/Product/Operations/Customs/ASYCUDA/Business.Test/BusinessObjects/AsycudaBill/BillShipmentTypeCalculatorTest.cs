using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using universalAlias = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class BillShipmentTypeCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry", universalAlias.RefDataGrouping.Codes.CommonDataGrouping);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice", universalAlias.RefDataGrouping.Codes.CommonDataGrouping);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes", universalAlias.RefDataGrouping.Codes.CommonDataGrouping);

			var fj = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "FJ", "Fiji", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(fj.PK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");
			var fjSuva = helper.CreateNewOrGetExistingCusCodeList("FJ", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "SUVA", "Suva", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(fjSuva.PK, "PORT", "FJSUV");
			var sb = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "SB", "Solomons", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sb.PK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");
			var sbHIRS = helper.CreateNewOrGetExistingCusCodeList("SB", universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "HIRS", "Honiara", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRS.PK, "SEA", "SBHIR");
			Factory.Save();

			CombineAssertions(() =>
			{
				foreach (ZString country in new[] { "SB", "FJ" })
				{
					Assert($"Precondition: {country} is supported country", country.IsSupportedCountries(Factory));
				}

				foreach (ZString country in new[] { "AU", "NZ" })
				{
					Assert($"Precondition: {country} is not supported country", !country.IsSupportedCountries(Factory));
				}
			});

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "SBAFT";
			header.AMA_RL_NKPortOfDischarge = "FJAQS";
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "SBAFT";
			bill.ABL_RL_NKFinalDestination = "FJAQS";
			var calculator = new BillShipmentTypeCalculator(bill);

			// Group a
			AssertEquals("Scenario 1", "EXP", calculator.CalculateShipmentTypeFor("SB"));
			AssertEquals("Scenario 1", "IMP", calculator.CalculateShipmentTypeFor("FJ"));

			bill.ABL_RL_NKFinalDestination = "NZAKL";
			AssertEquals("Scenario 2", "EXP", calculator.CalculateShipmentTypeFor("SB"));
			AssertEquals("Scenario 2", "TSS", calculator.CalculateShipmentTypeFor("FJ"));

			bill.ABL_RL_NKOrigin = "NZAKL";
			bill.ABL_RL_NKFinalDestination = "SBAFT";
			AssertEquals("Scenario 3", "TSS", calculator.CalculateShipmentTypeFor("SB"));
			AssertEquals("Scenario 3", "IMP", calculator.CalculateShipmentTypeFor("FJ"));

			bill.ABL_RL_NKFinalDestination = "AUBNE";
			AssertEquals("Scenario 4", "TSS", calculator.CalculateShipmentTypeFor("SB"));
			AssertEquals("Scenario 4", "TSS", calculator.CalculateShipmentTypeFor("FJ"));

			// Group b
			header.AMA_RL_NKPortOfLoading = "SBAFT";
			header.AMA_RL_NKPortOfDischarge = "NZAKL";
			bill.ABL_RL_NKOrigin = "SBAFT";
			bill.ABL_RL_NKFinalDestination = "FJAQS";
			AssertEquals("Scenario 5", "EXP", calculator.CalculateShipmentTypeFor("SB"));
			AssertEquals("Scenario 5", "", calculator.CalculateShipmentTypeFor("FJ"));

			bill.ABL_RL_NKFinalDestination = "NZAKL";
			AssertEquals("Scenario 6", "EXP", calculator.CalculateShipmentTypeFor("SB"));
			AssertEquals("Scenario 6", "", calculator.CalculateShipmentTypeFor("FJ"));
			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);
			var leg = consol.Transports.AddNew();
			leg.JW_RL_NKLoadPort = "FJAQS";
			AssertEquals("Scenario 6", "", calculator.CalculateShipmentTypeFor("FJ"));
			header.SetParent(null);

			bill.ABL_RL_NKOrigin = "ARBUE";
			bill.ABL_RL_NKFinalDestination = "FJAQS";
			AssertEquals("Scenario 7", "TSS", calculator.CalculateShipmentTypeFor("SB"));
			AssertEquals("Scenario 7", "", calculator.CalculateShipmentTypeFor("FJ"));

			bill.ABL_RL_NKFinalDestination = "NZAKL";
			AssertEquals("Scenario 8", "TSS", calculator.CalculateShipmentTypeFor("SB"));

			// Group C
			header.AMA_RL_NKPortOfLoading = "NZAKL";
			header.AMA_RL_NKPortOfDischarge = "SBAFT";
			bill.ABL_RL_NKOrigin = "FJAQS";
			bill.ABL_RL_NKFinalDestination = "SBAFT";
			AssertEquals("Scenario 9", "", calculator.CalculateShipmentTypeFor("FJ"));
			AssertEquals("Scenario 9", "IMP", calculator.CalculateShipmentTypeFor("SB"));

			bill.ABL_RL_NKOrigin = "FJAQS";
			bill.ABL_RL_NKFinalDestination = "NZAKL";
			AssertEquals("Scenario 10", "", calculator.CalculateShipmentTypeFor("FJ"));
			AssertEquals("Scenario 10", "TSS", calculator.CalculateShipmentTypeFor("SB"));

			bill.ABL_RL_NKOrigin = "NZAKL";
			bill.ABL_RL_NKFinalDestination = "FJAQS";
			AssertEquals("Scenario 11", "TSS", calculator.CalculateShipmentTypeFor("SB"));

			bill.ABL_RL_NKFinalDestination = "AUBNE";
			AssertEquals("Scenario 12", "TSS", calculator.CalculateShipmentTypeFor("SB"));
		}
	}
}
