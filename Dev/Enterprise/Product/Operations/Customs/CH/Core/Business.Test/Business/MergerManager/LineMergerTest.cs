using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

public class LineMergerTest : Customs.Business.Testing.LineMergerTest
{
	protected override BaseJobDeclaration GetJobDeclaration() => Factory.New<JobDeclaration>();

	protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration) => new LineMerger((JobDeclaration)declaration);

	protected override Type ExpectedDutyCalculatorStrategyType => typeof(DutyCalculatorStrategy);

	protected override bool AllowDeleteEntryLineForRegistedEntry => false;

	public void TestRecalculateAdditionalTaxValueAfterMerge()
	{
		var declaration = CreateDeclaration();
		var invoiceLine = CreateInvoiceLine(declaration);
		var parentTariff = CreateTariffWithAdditionalTax(invoiceLine.JI_CountryOfOrigin, "0.04 * (VFD + DTY)");
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;
		invoiceLine.JI_Weight = 50m;

		CombineAssertions("Precondition before merge", () =>
		{
			AssertEquals("VFD", 100m, invoiceLine.JI_CustomsValue);
			AssertEquals("DTY", 0m, invoiceLine.JI_Calc_DutyAmount);
			AssertEquals("AdditionalTaxes Count", 1, invoiceLine.AdditionalTaxes.Count);
			AssertEquals("BZ_Value = 0.04 * (VFD + DTY)", 4m, invoiceLine.AdditionalTaxes[0].BZ_Value);
		});

		declaration.DoMerge();

		CombineAssertions("Precondition after merge", () =>
		{
			AssertEquals("VFD", 100m, invoiceLine.JI_CustomsValue);
			AssertEquals("DTY = 3.06 * 50", 163.12m, invoiceLine.JI_Calc_DutyAmount);
		});

		AssertEquals("BZ_Value = 0.04 * (100 + 153)", 10.12m, invoiceLine.AdditionalTaxes[0].BZ_Value);

		var additionalFee1 = invoiceLine.AdditionalFees.AddNew();
		additionalFee1.BZ_Tariff = "180";
		additionalFee1.BZ_Qty1 = 1;
		additionalFee1.BZ_ManualRate = 90;

		var additionalFee2 = invoiceLine.AdditionalFees.AddNew();
		additionalFee2.BZ_Tariff = "180";
		additionalFee2.BZ_Qty1 = 1;
		additionalFee2.BZ_ManualRate = 90;

		var additionalFee3 = invoiceLine.AdditionalFees.AddNew();
		additionalFee3.BZ_Tariff = "300";
		additionalFee3.BZ_Qty1 = 1;
		additionalFee3.BZ_ManualRate = 300;

		var surplusFee = invoiceLine.CusEntryLine.Fees.AddNew();
		surplusFee.CF_ChargeType = "333";
		surplusFee.CF_ChargeAmount = 333;

		declaration.DoMerge();

		var fee1 = invoiceLine.CusEntryLine.Fees.Cast<CusEntryLineFee>().Single(f => f.CF_ChargeType == "DTY");
		var fee2 = invoiceLine.CusEntryLine.Fees.Cast<CusEntryLineFee>().Single(f => f.CF_ChargeType == "180");
		var fee3 = invoiceLine.CusEntryLine.Fees.Cast<CusEntryLineFee>().Single(f => f.CF_ChargeType == "300");
		var fee4 = invoiceLine.CusEntryLine.Fees.Cast<CusEntryLineFee>().Single(f => f.CF_ChargeType == "660");

		CombineAssertions("check CusEntryLineFee", () =>
		{
			AssertEquals("4 CusEntryLineFee exists", 4, invoiceLine.CusEntryLine.Fees.Count);

			AssertNotNull("DTY exists", fee1);
			AssertEquals("DTY Amount 153", 153.0m, fee1.CF_ChargeAmount);

			AssertNotNull("Additional Fee 180 exists - grouped", fee2);
			AssertEquals("Additional Fee 180 Amount - Sum 180", 180.0m, fee2.CF_ChargeAmount);

			AssertNotNull("Additional Fee 300 exists", fee3);
			AssertEquals("Additional Fee 300 Amount - 300", 300.0m, fee3.CF_ChargeAmount);

			AssertNotNull("Additional Tax 660 exists", fee4);
			AssertEquals("Additional Tax 660 Amount - calculated", 10.12m, fee4.CF_ChargeAmount);

			AssertCollectionNotContains("extra/obsolete fee no longer exists", surplusFee, invoiceLine.CusEntryLine.Fees);
		});
	}

	public void TestDoMerge_ShouldCalculateAdditionalTaxBaseValue()
	{
		var declaration = CreateDeclaration();
		var invoiceLine = CreateInvoiceLine(declaration);
		var parentTariff = CreateTariffWithAdditionalTax(invoiceLine.JI_CountryOfOrigin);
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;
		invoiceLine.JI_Weight = 50m;

		CombineAssertions("Precondition before merge", () =>
		{
			AssertEquals("VFD", 100m, invoiceLine.JI_CustomsValue);
			AssertEquals("DTY", 0m, invoiceLine.JI_Calc_DutyAmount);
			AssertEquals("AdditionalTaxes Count", 1, invoiceLine.AdditionalTaxes.Count);
			AssertEquals("BZ_BaseValue = VFD + DTY", 100m, invoiceLine.AdditionalTaxes[0].BZ_BaseValue);
		});

		declaration.DoMerge();

		CombineAssertions("Precondition after merge", () =>
		{
			AssertEquals("VFD", 100m, invoiceLine.JI_CustomsValue);
			AssertEquals("DTY = 3.06 * 50", 153m, invoiceLine.JI_Calc_DutyAmount);
		});

		AssertEquals("BZ_BaseValue = VFD + DTY", 253m, invoiceLine.AdditionalTaxes[0].BZ_BaseValue);
	}

	public void TestDoMerge_EntryLineFeeVAT()
	{
		RefCusTaxOrFeeTestHelper.CreateRefCusTaxOrFeeList(Factory);
		var helper = new UniversalReferenceTestDataHelper(Factory);

		var declaration = CreateDeclaration();
		var invoiceLine = CreateInvoiceLine(declaration);
		var parentTariff = CreateTariffWithAdditionalTax(invoiceLine.JI_CountryOfOrigin);
		helper.CreateNewOrGetExistingVATApplicability(parentTariff, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.TaxCodes.StandardRate);
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;
		invoiceLine.JI_Weight = 50m;

		CombineAssertions("Precondition before merge", () =>
		{
			AssertEquals("VFD", 100m, invoiceLine.JI_CustomsValue);
			AssertEquals("DTY = 0", 0m, invoiceLine.JI_Calc_DutyAmount);
		});

		declaration.DoMerge();

		CombineAssertions("Precondition after merge", () =>
		{
			AssertEquals("VFD", 100m, invoiceLine.JI_CustomsValue);
			AssertEquals("DTY = 153", 153.00m, invoiceLine.JI_Calc_DutyAmount);
		});

		CombineAssertions("check VAT CusEntryLineFee", () =>
		{
			var vatFee = invoiceLine.CusEntryLine.Fees.Cast<CusEntryLineFee>().Single(f => f.CF_ChargeType == "VAT");
			AssertNotNull("VAT CusEntryLineFee exists", vatFee);
			AssertEquals("VAT Amount = (153 + 100) * StandardRate (ZZF_Value=0.077)", 19.5m, vatFee.CF_ChargeAmount);
			AssertEquals("VAT Amount on Invoice Line JI_Calc_GSTVATAmountIncludingWHEstimate", 19.5m, invoiceLine.JI_Calc_GSTVATAmountIncludingWHEstimate);
		});

		invoiceLine.JI_LinePrice = 218;
		declaration.DoMerge();

		CombineAssertions("recalculate VAT CusEntryLineFee", () =>
		{
			var vatFee = invoiceLine.CusEntryLine.Fees.Cast<CusEntryLineFee>().Single(f => f.CF_ChargeType == "VAT");
			AssertNotNull("VAT CusEntryLineFee exists", vatFee);
			AssertEquals("VAT Amount = (153 + 218) * StandardRate (ZZF_Value=0.077", 28.55m, vatFee.CF_ChargeAmount);
			AssertEquals("VAT Amount on Invoice Line JI_Calc_GSTVATAmountIncludingWHEstimate", 28.55m, invoiceLine.JI_Calc_GSTVATAmountIncludingWHEstimate);
		});

		invoiceLine.JI_LinePrice = 0;
		invoiceLine.InvoiceHeader.JZ_InvoiceAmount = 0;
		invoiceLine.JI_Tariff = string.Empty;
		declaration.DoMerge();

		AssertEquals("VAT CusEntryLineFee is set to zero", 0m, invoiceLine.CusEntryLine.Fees.Cast<CusEntryLineFee>().Single(f => f.CF_ChargeType == "VAT").CF_ChargeAmount);
	}

	TariffView CreateTariffWithAdditionalTax(string tradeGroupCountry, string additionalTaxFormula = null)
	{
		var tariffTestHelper = new RefCusTariffTestHelper(Factory);
		var parentTariff = tariffTestHelper.CreateImportTariffWithSingleRate("12345678000912");

		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		additionalTaxTariffHelper.CreateAdditionalTaxTariffWithRelationship(parentTariff, tariffCode: "660-001", rateFormula: additionalTaxFormula,
			tradeGroupCountry: tradeGroupCountry, excludedTradeGroupCountry: Core.Constants.CountryCodes.Australia);

		return parentTariff;
	}

	static JobComInvoiceLine CreateInvoiceLine(JobDeclaration declaration)
	{
		var invoiceLine = declaration.Invoices[0].InvoiceLines.AddNew();
		invoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;
		invoiceLine.JI_PrimaryPreference = PrimaryPreferenceCodes.PreferentialTariff;
		invoiceLine.JI_LinePrice = 100m;
		invoiceLine.JI_Weight = 50m;
		invoiceLine.JI_NetWeight = 50m;

		return invoiceLine;
	}

	JobDeclaration CreateDeclaration()
	{
		var declaration = TestJobDeclaration as JobDeclaration;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_InvoiceAmount = 100m;
		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Switzerland;

		return declaration;
	}
}
