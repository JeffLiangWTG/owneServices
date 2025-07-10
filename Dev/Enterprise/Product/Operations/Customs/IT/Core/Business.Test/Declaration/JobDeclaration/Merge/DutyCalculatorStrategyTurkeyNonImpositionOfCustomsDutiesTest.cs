using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class DutyCalculatorStrategyTurkeyNonImpositionOfCustomsDutiesTest : TestCaseWithFactory
{
	public void TestCalculateA00_RateForCountryOfOriginZA_Preference100()
	{
		declaration.JE_GoodsOrigin = "TR";
		invoiceLine.JI_CountryOfOrigin = "ZA";
		invoiceLine.JI_PrimaryPreference = "100";

		DoMergeAndAssertSingleA00FeeAmount(expectedAmount: 250m);
	}

	public void TestCalculateA00_RateForGoodsOriginTR_Preference400()
	{
		declaration.JE_GoodsOrigin = "TR";
		invoiceLine.JI_CountryOfOrigin = "ZA";
		invoiceLine.JI_PrimaryPreference = "400";

		DoMergeAndAssertSingleA00FeeAmount(expectedAmount: 0m);
	}

	public void TestCalculateA00_RateForCountryOfOriginTR_Preference100()
	{
		declaration.JE_GoodsOrigin = "TR";
		invoiceLine.JI_CountryOfOrigin = "TR";
		invoiceLine.JI_PrimaryPreference = "100";

		DoMergeAndAssertSingleA00FeeAmount(expectedAmount: 500m);
	}

	public void TestCalculateA00_RateForCountryOfOriginTR_Preference400()
	{
		declaration.JE_GoodsOrigin = "TR";
		invoiceLine.JI_CountryOfOrigin = "TR";
		invoiceLine.JI_PrimaryPreference = "400";

		DoMergeAndAssertSingleA00FeeAmount(expectedAmount: 0m);
	}

	protected override void SetUp()
	{
		base.SetUp();

		SetupRatesAndTariff(Factory);

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = "EUR";
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Tariff = "4016999190";
		invoiceLine.JI_LinePrice = 10000;
		invoiceLine.JI_Weight = 10000;
		invoiceLine.JI_WeightUQ = "KG";
		invoiceLine.JI_NetWeight = 10000;
		invoiceLine.JI_NetWeightUQ = "KG";
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	JobComInvoiceLine invoiceLine;

	public static void SetupRatesAndTariff(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);

		var dataGroupingEUN = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping("IT", parent: dataGroupingEUN);
		helper.CreateNewOrGetExistingDataGrouping("ZA");
		helper.CreateNewOrGetExistingDataGrouping("TR");

		var tradeGroupZA = helper.CreateTradeGroup("EUN", "TGZA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.AddCountry(tradeGroupZA, "ZA");
		var tradeGroupTR = helper.CreateTradeGroup("EUN", "TGTR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.AddCountry(tradeGroupTR, "TR");

		var preference100 = helper.CreatePreferenceForCountry("100", "Normal Third Country Tariff Duty (Including Ceilings)", "EUN");
		var preference300 = helper.CreatePreferenceForCountry("300", "Tariff Preference Without Conditions Or Limits (Including Ceilings)", "EUN");
		var preference400 = helper.CreatePreferenceForCountry("400", "Non Imposition of Customs Duties under the Provisions of Customs Union Agreements Concluded By the Community", "EUN");

		var rateTypeDTY = helper.CreateCusRateType("EUN", "DTY");
		var rateCodeA00 = helper.CreateCusRateCode(factory, "A00", rateTypeDTY.PK);

		var tariffTypeIMP = helper.CreateNewOrGetExistingTariffType("EUN", "IMP");
		var tariff = helper.LoadOrCreateNewTariff("EUN", tariffTypeIMP.PK, "4016999190", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		var rateA00_100_ZA = helper.CreateRate(tariff, rateCodeA00.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "VFD * 0.025", dataGrouping: "EUN", preferencePk: preference100.PK);
		helper.CreateCusApplicability(rateA00_100_ZA, tradeGroupZA, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		var rateA00_100_TR = helper.CreateRate(tariff, rateCodeA00.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "VFD * 0.050", dataGrouping: "EUN", preferencePk: preference100.PK);
		helper.CreateCusApplicability(rateA00_100_TR, tradeGroupTR, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		var rateA00_300_TR = helper.CreateRate(tariff, rateCodeA00.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0", dataGrouping: "EUN", preferencePk: preference300.PK);
		helper.CreateCusApplicability(rateA00_300_TR, tradeGroupTR, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		var rateA00_400_TR = helper.CreateRate(tariff, rateCodeA00.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0", dataGrouping: "EUN", preferencePk: preference400.PK);
		helper.CreateCusApplicability(rateA00_400_TR, tradeGroupTR, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
	}

	void DoMergeAndAssertSingleA00FeeAmount(decimal expectedAmount)
	{
		declaration.DoMerge();

		AssertEquals("Entry Header Count", 1, declaration.CustomsEntryHeaders.Count);

		var entryHeader = declaration.CustomsEntryHeaders[0];
		AssertEquals("Entry Lines Count", 1, entryHeader.MergedLines.Count);

		var entryLine = entryHeader.MergedLines[0];
		AssertEquals("Entry Line Fees Count", 1, entryLine.Fees.Count);

		var fee = entryLine.Fees[0];
		AssertEquals("Fee Type", "A00", fee.CF_ChargeType);
		AssertEquals("Fee Amount", expectedAmount, fee.CF_ChargeAmount);
	}
}
