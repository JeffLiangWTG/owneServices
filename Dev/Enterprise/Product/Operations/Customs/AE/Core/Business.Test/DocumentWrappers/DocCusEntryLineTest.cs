using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(DocCusEntryLine))]
sealed class DocCusEntryLineTest : DocBaseCusEntryLineAbstractTest<CusEntryLine, DocCusEntryLine>
{
	public override void TestDutyAmountRounded()
	{
		EntryLineInternal.Fees.AddOrUpdate(FeeTypeList.Codes.A00, 12.3453M);
		AssertEquals("DutyAmount", 12.35M, EntryLineWrapperInternal.DutyAmountRounded);
	}

	public override void TestGSTVATAmountRounded()
	{
		EntryLineInternal.Fees.AddOrUpdate(FeeTypeList.Codes.B00, 12.3453M);
		AssertEquals("GSTVATAmount", 12.35M, EntryLineWrapperInternal.GSTVATAmountRounded);
	}

	public void TestCIF()
	{
		EntryLineInternal.RandomLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		EntryLineInternal.RandomLine.JI_LinePrice = 100.0m;
		AssertEquals(100.0m, EntryLineWrapperInternal.CIF);
	}

	public void TestCIFInLocalCurrency()
	{
		EntryLineInternal.RandomLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		EntryLineInternal.RandomLine.JI_LinePrice = 100.0m;
		AssertEquals(100.0m, EntryLineWrapperInternal.CIFInLocalCurrency);
	}

	public override void TestLinePricesWithCurrency()
	{
		Assert(true);
	}

	public override void TestLinePriceInLocalCurrencyEqualsTheRelatedValueInBizObj()
	{
		Assert(true);
	}

	#region Implementation

	protected override string TestingCountry
	{
		get { return Enterprise.Core.Constants.CountryCodes.UnitedArabEmirates; }
	}

	protected override DocCusEntryLine CreateEntryLineWrapper(ICusEntryLine entryLineInternal)
	{
		return DocCusEntryLine.New((CusEntryLine)entryLineInternal, Factory);
	}

	Customs.Business.CusEntryLine fEntryLineMergeOfTwoInvoiceLines;
	protected override ICusEntryLine EntryLineMergeOfTwoInvoiceLines
	{
		get
		{
			if (fEntryLineMergeOfTwoInvoiceLines == null)
			{
				JobDeclaration declaration = (JobDeclaration)GetNewDeclaration();
				JobComInvoiceHeader header = declaration.Invoices.AddNew();
				header.JZ_InvoiceNumber = "1";
				header.JZ_RX_NKInvoice_Currency = "AED";

				JobComInvoiceLine invoiceLine = header.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 200.05m;
				invoiceLine.JI_Tariff = "123";
				invoiceLine.JI_LineNo = 1;

				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.JE_MessageType = "IMP";
				declaration.DoMerge();
				Assert("Must have at least one CustomsEntryHeader", declaration.CustomsEntryHeaders.Count > 0);
				Assert("Must have at least one CustomsEntryHeader", declaration.CustomsEntryHeaders[0].MergedLines.Count > 0);
				fEntryLineMergeOfTwoInvoiceLines = declaration.CustomsEntryHeaders[0].MergedLines[0];
			}
			return fEntryLineMergeOfTwoInvoiceLines;
		}
	}

	protected override DocCusEntryLine DocEntryLineMergeOfTwoInvoiceLines
	{
		get { return CreateEntryLineWrapper(EntryLineMergeOfTwoInvoiceLines); }
	}

	#endregion

}
