using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocEdecCusEntryLine))]
sealed class DocEdecCusEntryLineTest : DocBaseCusEntryLineAbstractTest<CusEntryLine, DocEdecCusEntryLine>
{
	public void TestCommodityCode()
	{
		EntryLineInternal.RandomLine.JI_Tariff = "10001000000";
		AssertEquals("CommodityCode", "1000.1000", EntryLineWrapperInternal.CommodityCode);
	}

	public void TestStatisticalCode() => CombineAssertions(() =>
	{
		EntryLineInternal.RandomLine.JI_Tariff = "85078000911242";
		AssertEquals("StatisticalCode default", "242", EntryLineWrapperInternal.StatisticalCode);

		EntryLineInternal.RandomLine.JI_Procedure = ProcedureCodesEdec.ExemptFromDuty;
		EntryLineInternal.RandomLine.InAndOutwardProcessing.CSI_Status = InAndOutwardProcessingStatusCodes.RepairFalse;
		AssertEquals("StatisticalCode empty if EmptyFromDuty and not Repair", ZString.Empty, EntryLineWrapperInternal.StatisticalCode);
	});

	public void TestIsPreferentialTariff() => CombineAssertions(() =>
	{
		EntryLineInternal.RandomLine.JI_PrimaryPreference = PrimaryPreferenceCodes.NormalTariff;
		AssertEquals("Normal Tariff", false, EntryLineWrapperInternal.IsPreferentialTariff);

		EntryLineInternal.RandomLine.JI_PrimaryPreference = PrimaryPreferenceCodes.PreferentialTariff;
		AssertEquals("Preferential Tariff", true, EntryLineWrapperInternal.IsPreferentialTariff);
	});

	public void TestOverriddenRate() => CombineAssertions(() =>
	{
		EntryLineInternal.RandomLine.JI_RateOverride = true;
		EntryLineInternal.RandomLine.JI_OverriddenRate = 3.21m;
		AssertEquals("Rate overridden", "3.21", EntryLineWrapperInternal.OverriddenRate);

		EntryLineInternal.RandomLine.JI_OverriddenRate = 0m;
		AssertEquals("Rate overridden", "0", EntryLineWrapperInternal.OverriddenRate);

		EntryLineInternal.RandomLine.JI_RateOverride = false;
		AssertEquals("Rate not overridden", ZString.Empty, EntryLineWrapperInternal.OverriddenRate);
	});

	public void TestAdditionalUnit()
	{
		EntryLineInternal.RandomLine.JI_CustomsThirdQuantity = 4.56m;
		AssertEquals("NetWeight", 4.6m, EntryLineWrapperInternal.AdditionalUnit);
	}

	public void TestVATCode()
	{
		RefCusTaxOrFeeTestHelper.CreateRefCusTaxOrFeeList(Factory);
		EntryLineInternal.RandomLine.JI_Tariff = RefCusTaxOrFeeTestHelper.TariffWithSingleFee;
		AssertEquals("VATCode", 7.7m, EntryLineWrapperInternal.VATCode);
	}

	#region Implementation

	public override void TestDutyAmountRounded()
	{
		EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 12.3453M);
		AssertEquals("DutyAmount", 12.35M, EntryLineWrapperInternal.DutyAmountRounded);
	}

	public override void TestGSTVATAmountRounded()
	{
		EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.VAT, 12.3453M);
		AssertEquals("GSTVATAmount", 12.35M, EntryLineWrapperInternal.GSTVATAmountRounded);
	}

	public override void TestLinePriceInLocalCurrencyEqualsTheRelatedValueInBizObj()
	{
		Assert(true);
	}

	public override void TestLinePricesWithCurrency()
	{
		Assert(true);
	}

	protected override string TestingCountry
	{
		get { return Core.Constants.CountryCodes.Switzerland; }
	}

	protected override CusEntryLine GetNewEntryLine()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
		declaration.JE_ClusterKey = 1;
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;
		invoiceLine2.JI_CEI = entryInstruction.PK;
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine2.JI_CL = entryLine.PK;
		return entryLine;
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine1;
	JobComInvoiceLine invoiceLine2;
	CusEntryHeader entryHeader;

	protected override DocEdecCusEntryLine CreateEntryLineWrapper(Customs.Business.ICusEntryLine entryLineInternal)
	{
		return DocEdecCusEntryLine.New((CusEntryLine)entryLineInternal, Factory);
	}

	protected override DocEdecCusEntryLine DocEntryLineMergeOfTwoInvoiceLines
	{
		get { return CreateEntryLineWrapper(EntryLineMergeOfTwoInvoiceLines); }
	}

	#endregion
}

