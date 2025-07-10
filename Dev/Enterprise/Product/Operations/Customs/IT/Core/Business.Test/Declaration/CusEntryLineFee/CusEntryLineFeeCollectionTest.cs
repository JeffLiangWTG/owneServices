using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLineFeeCollection))]
abstract class CusEntryLineFeeCollectionTest : EU.Business.Declaration.Testing.CusEntryLineFeeCollectionTest
{
	public void TestAllowSort()
	{
		var entryLineFeeCollectionForTest = new CusEntryLineFeeCollectionForTest(Factory.New<CusEntryLine>(), Factory);
		AssertEquals("Sorting for EntryLine Fee is disabled,AllowSort", false, entryLineFeeCollectionForTest.AllowSortExposed);
	}

	public void TestApplyCustomsCompliantSort()
	{
		var entryLine = Factory.New<CusEntryLine>();
		SetUpFees(entryLine.Fees, "407", "406", "405", "927", "911", "201", "165", "116", "A35", "A30", "A20", "A10", "A00");
		AssertEquals("[PRE-CONDITION] entryLine.Fees count", 13, entryLine.Fees.Count);
		AssertArrayEqualsByElements("[PRE-CONDITION] Fees order", new ZString[] { "407", "406", "405", "927", "911", "201", "165", "116", "A35", "A30", "A20", "A10", "A00" }, entryLine.Fees.Cast<CusEntryLineFee>().Select(x => x.CF_ChargeType).ToArray());

		entryLine.Fees.ApplyCustomsCompliantSort();
		AssertArrayEqualsByElements("Fees ordered after ApplyCustomsCompliantSort", new ZString[] { "A00", "A10", "A20", "A30", "A35", "116", "165", "201", "911", "927", "405", "406", "407" }, entryLine.Fees.Cast<CusEntryLineFee>().Select(x => x.CF_ChargeType).ToArray());
	}

	public void TestElementsAreLoadedInOrder()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		SetUpFees(entryLine.Fees, "407", "406", "405", "927", "911", "201", "165", "116", "A35", "A30", "A20", "A10", "A00");

		var entryLineFeeCollection = new CusEntryLineFeeCollection(entryLine, Factory);
		entryLineFeeCollection.Load();

		AssertArrayEqualsByElements("Fees are loaded in order", new ZString[] { "A00", "A10", "A20", "A30", "A35", "116", "165", "201", "911", "927", "405", "406", "407" }, entryLineFeeCollection.Cast<CusEntryLineFee>().Select(x => x.CF_ChargeType).ToArray());
	}

	protected void SetUpFees(CusEntryLineFeeCollection lineFeeCollection, params ZString[] rateCodesToAdd)
	{
		foreach (var rateCode in rateCodesToAdd)
		{
			lineFeeCollection.AddOrUpdate(rateCode, 0m);
		}
	}

	public void TestCalculateVatFee()
	{
		var refDataHelper = new UniversalReferenceTestDataHelper(Factory);

		var eunDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		refDataHelper.CreateNewOrGetExistingDataGrouping("IT", parent: eunDataGrouping);

		var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var ordVat = refDataHelper.CreateTaxOrFee("ORD", 0.2m, currentCountryCode, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

		var secRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "SEC", description: "Security Deposit");
		var excRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "IT", rateType: "EXC", description: "Excise");
		var moeRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "MOE", description: "Miscellaneous Not VATable");

		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "SEC", secRateType.PK);
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "131", excRateType.PK);
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "217", moeRateType.PK);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine = entryLine.Declaration.InvoiceLines[0];
		invoiceLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;
		entryLine.InvoiceLines.Add(invoiceLine);

		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "131", "%", 10m, 53m, 5.3m, "", false);
		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "217", "%", 20m, 40m, 8.0m, "", false);
		var vatCalculator = new EntryLineVatCalculator(entryLine);
		var calculatedVat = vatCalculator.CalculateVatFee();
		AssertNotNull(nameof(calculatedVat), calculatedVat);

		CombineAssertions("Calculated VAT", () =>
		{
			AssertEquals("Rate", ordVat.ZZF_Value, calculatedVat.Rate);
			AssertEquals("BaseValue", 5.3m, calculatedVat.BaseValue);
			AssertEquals("Amount", 1.06m, calculatedVat.Amount);
			AssertEquals("MethodOfCalculation", "%", calculatedVat.MethodOfCalculation);
			AssertEquals("AdjustedRate", ordVat.ZZF_Value * 100, calculatedVat.AdjustedRate);
		});

		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "SEC", "%", 50m, 60m, 30m, "", true);
		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "SEC", "%", 15m, 80m, 12m, "", false);
		calculatedVat = vatCalculator.CalculateVatFee();
		CombineAssertions("Calculated VAT", () =>
		{
			AssertEquals("Rate", ordVat.ZZF_Value, calculatedVat.Rate);
			AssertEquals("BaseValue", 17.3m, calculatedVat.BaseValue);
			AssertEquals("Amount", 3.46m, calculatedVat.Amount);
			AssertEquals("MethodOfCalculation", "%", calculatedVat.MethodOfCalculation);
			AssertEquals("AdjustedRate", ordVat.ZZF_Value * 100, calculatedVat.AdjustedRate);
		});
	}

	public void TestCalculateVatFeeWithITMiscellanousCusRefTypes()
	{
		var refDataHelper = new UniversalReferenceTestDataHelper(Factory);

		var eunDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		refDataHelper.CreateNewOrGetExistingDataGrouping("IT", parent: eunDataGrouping);

		var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var ordVat = refDataHelper.CreateTaxOrFee("ORD", 0.2m, currentCountryCode, ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

		var secRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "SEC", description: "Security Deposit");
		var mieRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "IT", rateType: "MIE", description: "Miscellaneous Import Export");
		var mnbRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "IT", rateType: "MNB", description: "Miscellaneous Not VATable Import Export");

		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "SEC", secRateType.PK);
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "131", mieRateType.PK);
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "217", mnbRateType.PK);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine = entryLine.Declaration.InvoiceLines[0];
		invoiceLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;
		entryLine.InvoiceLines.Add(invoiceLine);

		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "131", "%", 10m, 53m, 5.3m, "", false);
		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "217", "%", 20m, 40m, 8.0m, "", false);
		var vatCalculator = new EntryLineVatCalculator(entryLine);
		var calculatedVat = vatCalculator.CalculateVatFee();
		AssertNotNull(nameof(calculatedVat), calculatedVat);

		CombineAssertions("Calculated VAT", () =>
		{
			AssertEquals("Rate", ordVat.ZZF_Value, calculatedVat.Rate);
			AssertEquals("BaseValue", 5.3m, calculatedVat.BaseValue);
			AssertEquals("Amount", 1.06m, calculatedVat.Amount);
			AssertEquals("MethodOfCalculation", "%", calculatedVat.MethodOfCalculation);
			AssertEquals("AdjustedRate", ordVat.ZZF_Value * 100, calculatedVat.AdjustedRate);
		});

		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "SEC", "%", 50m, 60m, 30m, "", true);
		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "SEC", "%", 15m, 80m, 12m, "", false);
		calculatedVat = vatCalculator.CalculateVatFee();
		CombineAssertions("Calculated VAT", () =>
		{
			AssertEquals("Rate", ordVat.ZZF_Value, calculatedVat.Rate);
			AssertEquals("BaseValue", 17.3m, calculatedVat.BaseValue);
			AssertEquals("Amount", 3.46m, calculatedVat.Amount);
			AssertEquals("MethodOfCalculation", "%", calculatedVat.MethodOfCalculation);
			AssertEquals("AdjustedRate", ordVat.ZZF_Value * 100, calculatedVat.AdjustedRate);
		});
	}

	public void TestGetTotalAmount()
	{
		var entryLineFeeCollection = (CusEntryLineFeeCollection)GetCollectionToTest();
		entryLineFeeCollection.AddOrUpdate("A00", 100m).CF_MethodOfPayment = "A";
		entryLineFeeCollection.AddOrUpdate("A10", 20m).CF_MethodOfPayment = "A";
		entryLineFeeCollection.AddOrUpdate("A20", 11.22m).CF_MethodOfPayment = "B";

		AssertEquals("CF_MethodOfPayment = 'A'", 120m, entryLineFeeCollection.GetTotalAmount("A"));
		AssertEquals("CF_MethodOfPayment = 'B'", 11.22m, entryLineFeeCollection.GetTotalAmount("B"));
		AssertEquals("CF_MethodOfPayment = 'Z'", 0m, entryLineFeeCollection.GetTotalAmount("Z"));
	}

	public void TestAllowNew()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entryHeader.MergedLines.AddNew();
		var entryLineFeeCollectionForTest = new CusEntryLineFeeCollectionForTest(entryLine, Factory);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			entryInstruction.CEI_Style = ExportUCC6DeclarationTypeList.Codes.DichiarazioneRiesportazioneB1;
			AssertEquals("AllowNew", true, entryLineFeeCollectionForTest.AllowNew);

			entryInstruction.CEI_Style = ExportUCC6DeclarationTypeList.Codes.DichiarazionePerTerritoriFiscaliSpecialiB4;
			AssertEquals("AllowNew", false, entryLineFeeCollectionForTest.AllowNew);

			entryInstruction.CEI_Style = ExportUCC6DeclarationTypeList.Codes.RegimeSpecialeDichiarazioneB2;
			AssertEquals("AllowNew", true, entryLineFeeCollectionForTest.AllowNew);

			entryInstruction.CEI_Style = ExportUCC6DeclarationTypeList.Codes.NotificaDiPresentazioneDelleMerciC2;
			AssertEquals("AllowNew", false, entryLineFeeCollectionForTest.AllowNew);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("AllowNew", true, entryLineFeeCollectionForTest.AllowNew);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			entryInstruction.CEI_Style = ExportUCC6DeclarationTypeList.Codes.DichiarazionePerTerritoriFiscaliSpecialiB4;
			AssertEquals("AllowNew", true, entryLineFeeCollectionForTest.AllowNew);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("AllowNew", true, entryLineFeeCollectionForTest.AllowNew);
		}
	}

	public void TestHasExcludeActionForGivenFeeType()
	{
		const string testRateCode = "ABC";

		var feeCollection = (CusEntryLineFeeCollection)GetCollectionToTest();
		AssertEquals("[PRE-CONDITION]", false, feeCollection.HasExcludeActionForGivenFeeType(testRateCode));

		var sysFee = feeCollection.AddNew();
		sysFee.CF_ChargeType = testRateCode;
		sysFee.CF_RateOverrideReasonCode = ZString.Empty;
		AssertEquals("With Single Record having CF_RateOverrideReasonCode=EMPTY", false, feeCollection.HasExcludeActionForGivenFeeType(testRateCode));

		var addFee = feeCollection.AddNew();
		addFee.CF_ChargeType = testRateCode;
		addFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
		AssertEquals("With Second Record having CF_RateOverrideReasonCode=ADD", false, feeCollection.HasExcludeActionForGivenFeeType(testRateCode));

		var excFee = feeCollection.AddNew();
		excFee.CF_ChargeType = testRateCode;
		excFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Exclude;
		AssertEquals("With third record having CF_RateOverrideReasonCode=EXC", true, feeCollection.HasExcludeActionForGivenFeeType(testRateCode));
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		return new CusEntryLineFeeCollection(entryLine, Factory);
	}
}

internal class CusEntryLineFeeCollectionForTest : CusEntryLineFeeCollection
{
	public CusEntryLineFeeCollectionForTest(CusEntryLine entryLine, BusinessObjectFactory factory) : base(entryLine, factory)
	{
	}

	public bool AllowSortExposed => AllowSort;
}
