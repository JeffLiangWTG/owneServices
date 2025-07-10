using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

class RefCusTaxOrFeeTestHelper
{
	internal const string TariffWithSingleFee = "99999999";
	internal const string TariffWithSingleFee2 = "22222222";
	internal const string TariffWithMultipleFees = "11111111";
	internal const string InvalidTariff = "00000000";

	internal const string StandardVATRate81 = "81";

	const string Datagrouping = Core.Constants.CountryCodes.Switzerland;
	const string TaxOrFeeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;

	internal static void CreateRefCusTaxOrFeeList(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);

		helper.CreateRefCusTaxOrFeeType(TaxOrFeeType);
		factory.Save();

		helper.CreateTaxOrFee(UniversalReferenceConstants.TaxCodes.RelocationProcedure, 5, Datagrouping).ZZF_ZX0_NKTaxOrFeeType = TaxOrFeeType;
		helper.CreateTaxOrFee(UniversalReferenceConstants.TaxCodes.ProcessingTraffic, 10, Datagrouping).ZZF_ZX0_NKTaxOrFeeType = TaxOrFeeType;
		helper.CreateTaxOrFee(UniversalReferenceConstants.TaxCodes.DeferredTaxation, 15, Datagrouping).ZZF_ZX0_NKTaxOrFeeType = TaxOrFeeType;
		helper.CreateTaxOrFee(UniversalReferenceConstants.TaxCodes.StandardRate, 0.077, Datagrouping).ZZF_ZX0_NKTaxOrFeeType = TaxOrFeeType;
		helper.CreateTaxOrFee(UniversalReferenceConstants.TaxCodes.ReducedRate, 25, Datagrouping).ZZF_ZX0_NKTaxOrFeeType = TaxOrFeeType;
		helper.CreateTaxOrFee(UniversalReferenceConstants.TaxCodes.ExemptVat, 30, Datagrouping).ZZF_ZX0_NKTaxOrFeeType = TaxOrFeeType;

		helper.CreateNewOrGetExistingDataGrouping(Datagrouping);
		var tariffType = helper.CreateTariffType(Datagrouping, JobMessageTypeList.Codes.Import);
		factory.Save();

		var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Switzerland, tariffType.PK, TariffWithSingleFee, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingVATApplicability(tariff, Datagrouping, UniversalReferenceConstants.TaxCodes.StandardRate, startDate: new ZDateTime(2024, 01, 01), endDate: ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingVATApplicability(tariff, Datagrouping, UniversalReferenceConstants.TaxCodes.ReducedRate, startDate: ZDateTime.MinSmallDateTimeValue, endDate: new ZDateTime(2023, 12, 31));

		var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Switzerland, tariffType.PK, TariffWithMultipleFees, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingVATApplicability(tariff2, Datagrouping, UniversalReferenceConstants.TaxCodes.StandardRate);
		helper.CreateNewOrGetExistingVATApplicability(tariff2, Datagrouping, UniversalReferenceConstants.TaxCodes.ReducedRate);

		factory.Save();
	}
}
