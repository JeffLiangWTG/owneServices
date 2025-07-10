using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusEntryLine))]
sealed class CusEntryLineTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
{
	protected override BusinessObject GetBusinessObjectForFetchForLoad()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		return declaration.CustomsEntryInstructions.AddNew();
	}

	public new void TestFormattedTariff()
	{
		var declaration = (JobDeclaration)ImportJobDeclaration;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		DoMerge(declaration);
		var entryLine = declaration.ActiveEntryHeaders[0].AllEntryLines[0];
		entryLine.CL_AdValoremTariff = "61101100000099";
		AssertEquals("Formatted tariff should have the following format: XXXX.XXXX XXX XXX", "6110.1100 000 099", entryLine.FormattedTariff);
	}

	public void TestGSTVATAmount()
	{
		var declaration = (JobDeclaration)ImportJobDeclaration;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		DoMerge(declaration);
		var entryLine = declaration.ActiveEntryHeaders[0].AllEntryLines[0];

		AddFee(entryLine.Fees, Core.Constants.Customs.CusEntryFeeTypes.VAT, 100);
		AddFee(entryLine.Fees, Core.Constants.Customs.CusEntryFeeTypes.VAT, 200);
		AddFee(entryLine.Fees, Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 50);

		AssertEquals("GSTVATAmount should return the SUM of Fees with ChargeType = VAT", 300m, entryLine.GSTVATAmount);
	}

	public void TestDutyAmount()
	{
		var declaration = (JobDeclaration)ImportJobDeclaration;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		DoMerge(declaration);
		var entryLine = declaration.ActiveEntryHeaders[0].AllEntryLines[0];

		AddFee(entryLine.Fees, Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 50);
		AddFee(entryLine.Fees, Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 100);
		AddFee(entryLine.Fees, Core.Constants.Customs.CusEntryFeeTypes.VAT, 200);

		AssertEquals("DutyAmount should return the SUM of Fees with ChargeType != VAT", 150m, entryLine.DutyAmount);
	}

	void AddFee(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> fees, ZString chargeType, ZDecimal chargeAmount)
	{
		var fee = fees.AddNew();
		fee.CF_ChargeType = chargeType;
		fee.CF_ChargeAmount = chargeAmount;
	}

	public void TestCL_StatisticalValue()
	{
		var declaration = (JobDeclaration)ImportJobDeclaration;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Switzerland;

		var line1 = invoice.InvoiceLines.AddNew();
		line1.JI_LinePrice = 50m;
		var charge1 = line1.Charges.AddNew();
		charge1.J7_ChargeType = "ADD";
		charge1.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
		charge1.J7_Amount = 10m;
		charge1.J7_IsStatisticalValueApplicable = true;

		var line2 = invoice.InvoiceLines.AddNew();
		line2.JI_LinePrice = 60m;
		var charge2 = line2.Charges.AddNew();
		charge2.J7_ChargeType = "ADD";
		charge2.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
		charge2.J7_Amount = 20m;
		charge2.J7_IsStatisticalValueApplicable = true;

		declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
		DoMerge(declaration);
		var entryLine = declaration.ActiveEntryHeaders[0].AllEntryLines[0];

		AssertEquals("StatisticalValue should return the SUM of JI_Calc_StatisticalValue", 140m, entryLine.CL_StatisticalValue);
	}

	public void TestResetTotalsAndCachedValues_JI_Calc_StatisticalValue()
	{
		var declaration = (JobDeclaration)ImportJobDeclaration;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Switzerland;

		var line1 = invoice.InvoiceLines.AddNew();
		line1.JI_LinePrice = 50m;
		var charge1 = line1.Charges.AddNew();
		charge1.J7_ChargeType = "ADD";
		charge1.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
		charge1.J7_Amount = 10m;
		charge1.J7_IsStatisticalValueApplicable = true;

		DoMerge(declaration);
		var entryLine = declaration.ActiveEntryHeaders[0].AllEntryLines[0];

		CombineAssertions(() =>
		{
			AssertEquals("StatisticalValue should be", 60m, entryLine.CL_StatisticalValue);
			entryLine.ResetTotalsAndCachedValues();
			AssertEquals("Reset StatisticalValue", 0m, entryLine.CL_StatisticalValue);
		});
	}

	public void TestCL_AdValoremTariffCaption() => AssertEquals("CL_AdValoremTariff Caption", "Tariff Code", DataBoundResourceStrings.GetDataForProperty(Factory.New<CusEntryLine>().CL_AdValoremTariffInfo).Caption);

	public void TestCL_DescriptionCaption() => AssertEquals("CL_Description Caption", "Description", DataBoundResourceStrings.GetDataForProperty(Factory.New<CusEntryLine>().CL_DescriptionInfo).Caption);

	public void TestCalcGrossWeight()
	{
		CombineAssertions("Import", () =>
		{
			var entryLine = GetMergeEntryLine((i, v) => i.JI_CustomsUnitQty = v, (i, v) => i.JI_CustomsQuantity = v, Core.Constants.Weight.Kilograms, 5.0111m, Core.Constants.Weight.Kilograms, 1.1102m, Core.Constants.Weight.Kilograms, 1.00m);
			AssertEquals(6.2m, entryLine.CalcGrossWeight);
			entryLine = GetMergeEntryLine((i, v) => i.JI_CustomsUnitQty = v, (i, v) => i.JI_CustomsQuantity = v, Core.Constants.Weight.Kilograms, 2.2214m, Core.Constants.Weight.Kilograms, 1.2403m, Core.Constants.Weight.Grams, 1000.00m);
			AssertEquals(3.5m, entryLine.CalcGrossWeight);
			AssertBaseCalcGrossWeight(entryLine);
		});

		CombineAssertions("Export", () =>
		{
			var entryLine = GetMergeEntryLine((i, v) => i.JI_CustomsUnitQty = v, (i, v) => i.JI_CustomsQuantity = v, Core.Constants.Weight.Kilograms, 5.0111m, Core.Constants.Weight.Kilograms, 1.1102m, Core.Constants.Weight.Kilograms, 1.00m, JobMessageTypeList.Codes.Export);
			AssertEquals(6.122m, entryLine.CalcGrossWeight);
			entryLine = GetMergeEntryLine((i, v) => i.JI_CustomsUnitQty = v, (i, v) => i.JI_CustomsQuantity = v, Core.Constants.Weight.Kilograms, 2.2214m, Core.Constants.Weight.Kilograms, 1.2403m, Core.Constants.Weight.Grams, 1000.00m, JobMessageTypeList.Codes.Export);
			AssertEquals(3.462m, entryLine.CalcGrossWeight);
			AssertBaseCalcGrossWeight(entryLine);
		});

		void AssertBaseCalcGrossWeight(CusEntryLine entryLine)
		{
			AssertEquals("CalcGrossWeight Caption", "Gross Weight", DataBoundResourceStrings.GetDataForProperty(entryLine.CalcGrossWeightInfo).Caption);
			AssertEquals(Core.Constants.Weight.Kilograms, entryLine.CalcGrossWeightUQ);
			AssertEquals(2, entryLine.CalcGrossWeightUQInfo.MaxLength);
		}
	}

	public void TestCalcNetWeight()
	{
		CombineAssertions(() =>
		{
			var entryLine = GetMergeEntryLine((i, v) => i.JI_CustomsSecondUnitQty = v, (i, v) => i.JI_CustomsSecondQuantity = v, Core.Constants.Weight.Kilograms, 5.0111m, Core.Constants.Weight.Kilograms, 1.1102m, Core.Constants.Weight.Kilograms, 1.00m);
			AssertEquals(6.122m, entryLine.CalcNetWeight);
			entryLine = GetMergeEntryLine((i, v) => i.JI_CustomsSecondUnitQty = v, (i, v) => i.JI_CustomsSecondQuantity = v, Core.Constants.Weight.Kilograms, 2.2214m, Core.Constants.Weight.Kilograms, 1.2403m, Core.Constants.Weight.Grams, 1000.00m);
			AssertEquals(3.462m, entryLine.CalcNetWeight);
			AssertEquals("CalcNetWeight Caption", "Net Weight", DataBoundResourceStrings.GetDataForProperty(entryLine.CalcNetWeightInfo).Caption);
			AssertEquals(Core.Constants.Weight.Kilograms, entryLine.CalcNetWeightUQ);
			AssertEquals(2, entryLine.CalcNetWeightUQInfo.MaxLength);
		});
	}

	public void TestCalcAdditionalQty()
	{
		CombineAssertions(() =>
		{
			var entryLine = GetMergeEntryLine((i, v) => i.JI_CustomsThirdUnitQty = v, (i, v) => i.JI_CustomsThirdQuantity = v, Core.Constants.Weight.Kilograms, 5.0111m, Core.Constants.Weight.Kilograms, 1.1102m, Core.Constants.Weight.Kilograms, 1.00m);
			AssertEquals(6.2m, entryLine.CalcAdditionalQty);
			entryLine = GetMergeEntryLine((i, v) => i.JI_CustomsThirdUnitQty = v, (i, v) => i.JI_CustomsThirdQuantity = v, Core.Constants.Weight.Kilograms, 2.2214m, Core.Constants.Weight.Kilograms, 1.2403m, Core.Constants.Weight.Grams, 1000.00m);
			AssertEquals(3.5m, entryLine.CalcAdditionalQty);
			AssertEquals("CalcAdditionalQty Caption", "Additional Qty", DataBoundResourceStrings.GetDataForProperty(entryLine.CalcAdditionalQtyInfo).Caption);
			AssertEquals(Core.Constants.Weight.Kilograms, entryLine.CalcAdditionalQtyUQ);
			AssertEquals(3, entryLine.CalcAdditionalQtyUQInfo.MaxLength);
		});
	}

	public void TestCalcCustomsNetWeight()
	{
		CombineAssertions(() =>
		{
			var entryLine = GetMergeEntryLine((i, v) => i.JI_WeightIncludingInnerPackageUQ = v, (i, v) => i.JI_WeightIncludingInnerPackage = v, Core.Constants.Weight.Grams, 5011.1m, Core.Constants.Weight.Kilograms, 1.1102m, Core.Constants.Weight.Kilograms, 1.00m);
			AssertEquals(6.2m, entryLine.CalcCustomsNetWeight);
			entryLine = GetMergeEntryLine((i, v) => i.JI_WeightIncludingInnerPackageUQ = v, (i, v) => i.JI_WeightIncludingInnerPackage = v, Core.Constants.Weight.Kilograms, 2.2214m, Core.Constants.Weight.Grams, 1240.3m, Core.Constants.Weight.Grams, 1000.00m);
			AssertEquals(3.5m, entryLine.CalcCustomsNetWeight);
			AssertEquals("CalcCustomsNetWeight Caption", "Customs Net Weight", DataBoundResourceStrings.GetDataForProperty(entryLine.CalcCustomsNetWeightInfo).Caption);
			AssertEquals(Core.Constants.Weight.Kilograms, entryLine.CalcCustomsNetWeightUQ);
			AssertEquals(2, entryLine.CalcCustomsNetWeightUQInfo.MaxLength);
		});
	}

	CusEntryLine GetMergeEntryLine(Action<JobComInvoiceLine, string> unitSetter, Action<JobComInvoiceLine, decimal> valueSetter, string unit1, decimal value1, string unit2, decimal value2, string interferenceUnit, decimal interferenceValue, string declarationType = JobMessageTypeList.Codes.Import)
	{
		var declaration = (JobDeclaration)ImportJobDeclaration;
		declaration.JE_MessageType = declarationType;

		var instruction1 = declaration.CustomsEntryInstructions.AddNew();
		var invoice1 = declaration.Invoices.AddNew();

		var line1 = invoice1.InvoiceLines.AddNew();
		line1.JI_CEI = instruction1.PK;
		unitSetter(line1, unit1);
		valueSetter(line1, value1);

		var line2 = invoice1.InvoiceLines.AddNew();
		line2.JI_CEI = instruction1.PK;
		unitSetter(line2, unit2);
		valueSetter(line2, value2);

		var instruction2 = declaration.CustomsEntryInstructions.AddNew();
		var invoice2 = declaration.Invoices.AddNew();

		var line3 = invoice2.InvoiceLines.AddNew();
		line3.JI_CEI = instruction2.PK;
		unitSetter(line3, interferenceUnit);
		valueSetter(line3, interferenceValue);

		declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
		DoMerge(declaration);

		return (CusEntryLine)line1.CusEntryLine;
	}

	public void TestSetStatusWhenInoviceLineIsRemovedAfterMerge()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

		var invoice = declaration.Invoices.AddNew();

		var line1 = invoice.JobComInvoiceLines.AddNew();
		line1.JI_Tariff = "12345678";
		line1.JI_Description = "description 1";
		var line2 = invoice.JobComInvoiceLines.AddNew();
		line2.JI_Tariff = "87654321";
		line2.JI_Description = "description 2";
		var line3 = invoice.JobComInvoiceLines.AddNew();
		line3.JI_Tariff = "97541357";
		line3.JI_Description = "description 3";

		DoMerge(declaration);
		var entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_Status = CHLogicalStatusList.Codes.Accepted;

		CombineAssertions(() =>
		{
			AssertEquals("There should be three Entry Lines present.", 3, declaration.ActiveEntryHeaders[0].AllEntryLines.Count);
			AssertEquals("Entry Line Number corresponds to the merged value.", (ZShort)1, declaration.ActiveEntryHeaders[0].AllEntryLines[0].CL_LineNumber);
			AssertEquals("Entry Line Number corresponds to the merged value.", (ZShort)2, declaration.ActiveEntryHeaders[0].AllEntryLines[1].CL_LineNumber);
			AssertEquals("Entry Line Number corresponds to the merged value.", (ZShort)3, declaration.ActiveEntryHeaders[0].AllEntryLines[2].CL_LineNumber);
			AssertEquals("Entry Line Status should be Active.", EntryLineStatusList.Codes.Active, declaration.ActiveEntryHeaders[0].AllEntryLines[2].CL_CustomsPostedStatus);
			var deletionPendingEntryLine = line3.CusEntryLine;

			line3.Delete();

			DoMerge(declaration);

			entryHeader.PendingDeletionEntryLines.Rebuild();
			AssertEquals("Entry Line Status should be Deletion Pending.", EntryLineStatusList.Codes.DeletePending, deletionPendingEntryLine.CL_CustomsPostedStatus);
			AssertEquals("Entry Line Status should be Deletion Pending.", true, entryHeader.PendingDeletionEntryLines.Contains(deletionPendingEntryLine));
			AssertEquals("There should be four Entry Lines present.", 3, entryHeader.AllEntryLines.Count);
			AssertEquals("Entry Line Number corresponds to the merged value.", (ZShort)1, entryHeader.AllEntryLines[0].CL_LineNumber);
			AssertEquals("Entry Line Number corresponds to the merged value.", (ZShort)2, entryHeader.AllEntryLines[1].CL_LineNumber);
			AssertEquals("Entry Line Number corresponds to the merged value.", (ZShort)3, entryHeader.AllEntryLines[2].CL_LineNumber);
		});
	}

	public void TestConfirmedDuty_Caption()
	{
		var entryHeader = ImportJobDeclaration.ActiveEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew() as CusEntryLine;

		AssertEquals("Caption", "Confirmed Duty", DataBoundResourceStrings.GetDataForProperty(entryLine.ConfirmedDutyInfo).Caption);
	}

	public void TestConfirmedDuty()
	{
		var entryHeader = ImportJobDeclaration.ActiveEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew() as CusEntryLine;
		PopulateCusEntryLineFees(entryLine);
		entryLine.ConfirmedFees.Load();

		AssertEquals("Sum of confirmed Duties", 50.0M, entryLine.ConfirmedDuty);
	}

	public void TestConfirmedVAT_Caption()
	{
		var entryHeader = ImportJobDeclaration.ActiveEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew() as CusEntryLine;

		AssertEquals("Caption", "Confirmed VAT", DataBoundResourceStrings.GetDataForProperty(entryLine.ConfirmedVATInfo).Caption);
	}

	public void TestConfirmedVAT()
	{
		var entryHeader = ImportJobDeclaration.ActiveEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew() as CusEntryLine;
		PopulateCusEntryLineFees(entryLine);
		entryLine.ConfirmedFees.Load();

		AssertEquals("Sum of confirmed VAT", 10.0M, entryLine.ConfirmedVAT);
	}

	void PopulateCusEntryLineFees(CusEntryLine entryLine)
	{
		PopulateFees(Core.Constants.Customs.CusEntryFeeTypes.VAT, 10.0M);
		PopulateFees(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 20.0M);
		PopulateFees("150", 30.0M);
		PopulateFees("XXX", 40.0M, string.Empty);

		void PopulateFees(ZString chargeType, ZDecimal chargeAmount, string source = CusEntryHeaderChargesSourceCodeList.Codes.CUS)
		{
			var newCharge = entryLine.Fees.AddNew();
			newCharge.CF_Source = source;
			newCharge.CF_ChargeType = chargeType;
			newCharge.CF_ChargeAmount = chargeAmount;
		}
	}

	protected override BaseJobDeclaration ImportJobDeclaration
	{
		get
		{
			var result = base.ImportJobDeclaration;
			result.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			return result;
		}
	}

	protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);

	protected override ZString ExpectedFallbackEntrylineDescription => "LINE";
}
