using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(DocCusEntryHeader))]
sealed class DocCusEntryHeaderTest : DocBaseCusEntryHeaderAbstractTest<CusEntryHeader, DocCusEntryHeader>
{
	public void TestRemarks1()
	{
		JobComInvoiceHeader invoiceHeader = GetInvoiceHeaderForEntryHeader();
		AssertEquals("Remarks1:", ZString.Empty, entryHeaderWrapper.Remarks1);

		invoiceHeader.JZ_IncoTerm = "FOB";
		invoiceHeader.JZ_InvoiceAmount = 100.0m;
		JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines[0];
		invoiceLine.JI_LinePrice = 100.0m;
		AssertEquals("Remarks1:", "FOB 100.0 AUD", entryHeaderWrapper.Remarks1);

		invoiceHeader.JZ_IncoTerm = "CIF";
		AssertEquals("Remarks1:", "CIF 100.0 AUD", entryHeaderWrapper.Remarks1);
	}

	public void TestRemarks2()
	{
		JobComInvoiceHeader invoiceHeader = GetInvoiceHeaderForEntryHeader();
		AssertEquals("Remarks2:", "FRT 0 AUD", entryHeaderWrapper.Remarks2);

		invoiceHeader.JZ_IncoTerm = "CIF";
		AssertEquals("Remarks2 should be empty", ZString.Empty, entryHeaderWrapper.Remarks2);

		invoiceHeader.JZ_IncoTerm = "FOB";
		invoiceHeader.JZ_InvoiceAmount = 100.0m;
		JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines[0];
		invoiceLine.JI_LinePrice = 100.0m;

		Assert("Remarks2 should not be empty", entryHeaderWrapper.Remarks2.StartsWith("FRT"));
	}

	public void TestRemarksRoundForCurrency()
	{
		JobComInvoiceHeader invoiceHeader = GetInvoiceHeaderForEntryHeader();
		invoiceHeader.JZ_IncoTerm = "FOB";
		invoiceHeader.JZ_InvoiceAmount = 100.72m;
		AssertEquals("Remarks1:", "FOB 100.72 AUD", entryHeaderWrapper.Remarks1);
		AssertEquals("Remarks2:", "FRT 0.00 AUD", entryHeaderWrapper.Remarks2);

		var tNDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "TND");
		invoiceHeader.JZ_RX_NKInvoice_Currency = tNDCurrency.RX_Code;
		invoiceHeader.JZ_InvoiceAmount = 100.723m;
		AssertEquals("Remarks1:", "FOB 100.723 TND", entryHeaderWrapper.Remarks1);
		AssertEquals("Remarks2:", "FRT 0.000 TND", entryHeaderWrapper.Remarks2);

		var jPYCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "JPY");
		invoiceHeader.JZ_RX_NKInvoice_Currency = jPYCurrency.RX_Code;
		AssertEquals("Remarks1:", "FOB 101 JPY", entryHeaderWrapper.Remarks1);
		AssertEquals("Remarks2:", "FRT 0 JPY", entryHeaderWrapper.Remarks2);
	}

	public JobComInvoiceHeader GetInvoiceHeaderForEntryHeader()
	{
		var aUDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
		JobComInvoiceHeader invoiceHeader = entryHeader.Declaration.Invoices.AddNew();
		invoiceHeader.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
		invoiceHeader.JZ_IncoTerm = Constants.IncoTerms.FreeOnBoard;
		JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CL = entryHeader.MergedLines.AddNew().PK;
		return invoiceHeader;
	}

	public void TestEntryDate()
	{
		ZDateTime currentDate = ZDateTime.Today;
		EDIMessage message1 = entryHeader.Messages.AddNew();
		message1.EM_SystemCreateTimeUtc = currentDate.AddDays(-2);
		message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

		EDIMessage message2 = entryHeader.Messages.AddNew();
		message2.EM_SystemCreateTimeUtc = currentDate.AddDays(-1);
		message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message2.EM_MessageSubType = "C";

		EDIMessage message3 = entryHeader.Messages.AddNew();
		message3.EM_SystemCreateTimeUtc = currentDate;
		message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message3.EM_MessageSubType = "C";

		AssertEquals("EntryDate:", currentDate, entryHeaderWrapper.EntryDate);
	}

	public void TestNew()
	{
		AssertNull("Created with null", DocCusEntryHeader.New(null, Factory));
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertNotNull("Created with a valild object", DocCusEntryHeader.New(entryHeader, Factory));
	}

	public void TestDeclaration()
	{
		AssertNotNull("Declaration", entryHeader.Declaration);
		AssertEquals("Declaration is of type DocDeclaration", typeof(DocDeclaration), entryHeaderWrapper.Declaration.GetType());
	}

	public void TestEntryLinesCollection()
	{
		CusEntryLine line1 = entryHeader.MergedLines.AddNew();
		CusEntryLine line2 = entryHeader.MergedLines.AddNew();

		AssertEquals(2, entryHeaderWrapper.EntryLines.Count);
	}

	#region Implementation

	protected override string TestingCountry
	{
		get { return Enterprise.Core.Constants.CountryCodes.UnitedArabEmirates; }
	}

	CusEntryHeader entryHeader;
	DocCusEntryHeader entryHeaderWrapper;

	protected override void SetUp()
	{
		base.SetUp();
		entryHeader = EntryHeaderInternal;
		entryHeaderWrapper = EntryHeaderWrapperInternal;
	}

	protected override DocCusEntryHeader CreateEntryHeaderWrapper(CusEntryHeader entryHeader)
	{
		return DocCusEntryHeader.New(entryHeader, Factory);
	}

	#endregion
}
