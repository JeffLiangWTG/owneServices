using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Declaration;
using NUnit.Framework;
using JobDeclaration = Enterprise.Customs.BE.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class ExportOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<ExportOperationProvider>
{
	public void TestLRN()
	{
		entry.CH_BGMReference = "1480600000001";
		AssertEquals("1480600000001", provider.LRN);
	}

	public void TestMRN()
	{
		entry.MovementReferenceNumberSetter("22BEE00000000012J1");
		AssertEquals("22BEE00000000012J1", provider.MRN);
	}

	public void TestInvalidationReason()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty reason",string.Empty, provider.InvalidationReason);
			messageSendingAction.TypeOfEntry = BEExportEntryTypeList.Codes.CancellationRequest;
			messageSendingAction.Justification = "TestJustificationText";
			AssertEquals("Justification set as InvalidationReason for CancellationRequest", "TestJustificationText", provider.InvalidationReason);
			messageSendingAction.TypeOfEntry = BEExportEntryTypeList.Codes.ExportAmendment;
			AssertEquals("Empty reason for other requests", string.Empty, provider.InvalidationReason);
		});
	}

	[TestDate(2022, 8, 1, 1, 1, 1)]
	public void TestInvalidationRequestDateTime()
	{
		CombineAssertions(() =>
		{
			AssertEquals("value", new DateTime(2022, 8, 1, 1, 1, 1), provider.InvalidationRequestDateTime);
			AssertEquals("milliseconds", 0, Provider.InvalidationRequestDateTime.Millisecond);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, provider.InvalidationRequestDateTime.Kind);
		});
	}

	public void TestDeclarationType()
	{
		declaration.JE_MessageSubType = "EX";
		AssertEquals("EX", provider.DeclarationType);
	}

	public void TestAdditionalDeclarationType()
	{
		instruction.CEI_SubStyle = "H1";
		AssertEquals("H1", provider.AdditionalDeclarationType);
	}

	public void TestPresentationOfTheGoodsDateAndTime()
	{
		declaration.ZG_PresentationStartDate = new DateTime(2023, 1, 12, 10, 27, 11);

		CombineAssertions(() =>
		{
			AssertEquals("value", new DateTime(2023, 1, 12, 10, 27, 11), provider.PresentationOfTheGoodsDateAndTime);
			AssertEquals("milliseconds", 0, Provider.PresentationOfTheGoodsDateAndTime.Value.Millisecond);
			AssertEquals("DateTimeKind", DateTimeKind.Unspecified, provider.PresentationOfTheGoodsDateAndTime.Value.Kind);
		});
	}

	public void TestSecurity()
	{
		CombineAssertions(() =>
		{
			messageSendingAction.SecurityType = BEExportSecurityTypeList.Codes.NotUsed;
			AssertEquals(BEExportSecurityTypeList.Codes.NotUsed, provider.Security);
			messageSendingAction.SecurityType = BEExportSecurityTypeList.Codes.EXS;
			AssertEquals(BEExportSecurityTypeList.Codes.EXS, provider.Security);
			messageSendingAction.SecurityType = ZString.Empty;
			AssertNull(provider.Security);
		});
	}

	public void TestSpecificCircumstanceIndicator()
	{
		declaration.ZG_SpecificCircumstanceIndicator = "A";
		AssertEquals("A", provider.SpecificCircumstanceIndicator);
	}

	public void TestTotalAmountInvoiced()
	{
		invoiceHeader.JZ_InvoiceAmount = 123.45;
		AssertEquals(new decimal(123.45), provider.TotalAmountInvoiced);
	}

	public void TestInvoiceCurrency()
	{
		CombineAssertions(() =>
		{
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceHeader.JZ_InvoiceAmount = 123.45;
			AssertEquals("Invoice amount is filled, so should Invoice Currency", "EUR", provider.InvoiceCurrency);

			invoiceHeader.JZ_InvoiceAmount = 0.0;
			var lineMerger = new EU.Business.Declaration.LineMerger(declaration);
			lineMerger.DoMerge();
			provider = new ExportOperationProvider(messageSendingAction);

			AssertNull("No invoice amount is filled, so Invoice Currency should not be filled either", provider.InvoiceCurrency);
		});
	}

	protected override ExportOperationProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		instruction = declaration.CustomsEntryInstructions.AddNew();

		invoiceHeader = declaration.Invoices.AddNew();

		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;

		var lineMerger = new EU.Business.Declaration.LineMerger(declaration);
		lineMerger.DoMerge();
		entry = declaration.CustomsEntryHeaders.Single();

		messageSendingAction = new ExportEntryMessageSendingAction(entry) { TypeOfEntry = BEExportEntryTypeList.Codes.ExportDeclaration };

		provider = new ExportOperationProvider(messageSendingAction);
	}
	JobDeclaration declaration;
	CusEntryInstruction instruction;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoiceHeader;
	CusEntryHeader entry;
	ExportEntryMessageSendingAction messageSendingAction;
	ExportOperationProvider provider;
}
