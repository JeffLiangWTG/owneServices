using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureCargoDesc))]
sealed class NctsDepartureCargoDescTest : EU.NCTS.Business.Testing.NctsDepartureCargoDescAbstractTest<NctsHeader>
{
	public void TestPreviousDocuments()
	{
		AssertType<NctsPreviousDocumentCollection<NctsPreviousDocument>>(nctsDepartureCargoDesc.PreviousDocuments);
	}

	public void TestAdditionalInfos()
	{
		CombineAssertions(() =>
		{
			AssertType<NctsAdditionalInfoCollection<NctsAdditionalInfo>>("Type", nctsDepartureCargoDesc.AdditionalInfos);
			AssertEquals("IsRegisteredEditableChildObject", true, nctsDepartureCargoDesc.IsRegisteredEditableChildObject(nctsDepartureCargoDesc.AdditionalInfos));
		});
	}

	public void TestCalculateVAT_NoTaxTypeSet()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, "Belgium", parentDataGrouping);
		var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
		tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
		Factory.Save();
		var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "0304798001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");

		var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.AddCountry(tradeGroup, "EU");

		helper.CreateTaxOrFee("VATS", 0.01m, Core.Constants.CountryCodes.Belgium, new ZDateTime(ZDateTime.MinSmallDateTimeValue), new ZDateTime(ZDateTime.MaxSmallDateTimeValue));
		helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Belgium, "VATS", "", new ZDateTime(ZDateTime.MinSmallDateTimeValue), new ZDateTime(ZDateTime.MaxSmallDateTimeValue));
		Factory.Save();

		nctsDepartureCargoDesc.BY_HarmonisedTariff = "0304798001";
		nctsDepartureCargoDesc.BY_MonetaryValue = 100.0m;
		nctsDepartureCargoDesc.BY_ZZF_NKTaxType = ZString.Empty;

		CombineAssertions(() =>
		{
			AssertEquals("VAT is calculated with 0 Rate when no Tax Type is set.", ZDecimal.Zero, nctsDepartureCargoDesc.VatAmount);

			nctsDepartureCargoDesc.BY_ZZF_NKTaxType = "VATS";

			AssertEquals("VAT is calculated from the Tax Type when set.", 1.0m, nctsDepartureCargoDesc.VatAmount);
		});
	}
	protected override BusinessObject GetBusinessObjectForFetchForLoad() => CreateNctsDepartureCargoDesc();

	protected override BusinessObject GetNewBusinessObject() => CreateNctsDepartureCargoDesc();

	protected override ZString CountryCode => Core.Constants.CountryCodes.Belgium;

	protected override void SetUp()
	{
		base.SetUp();
		nctsDepartureCargoDesc = CreateNctsDepartureCargoDesc();
	}
	NctsDepartureCargoDesc nctsDepartureCargoDesc;

	NctsDepartureCargoDesc CreateNctsDepartureCargoDesc()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var bill = nctsHeader.Bills.AddNew();
		return bill.GoodsItems.AddNew();
	}
}
