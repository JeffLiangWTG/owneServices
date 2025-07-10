using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

class IncompleteImportH1LineWrapperTest : WrapperHelperTest<IncompleteImportH1LineWrapper>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			var entryLine = Factory.New<CusEntryLine>();
			AssertExceptionThrown("Constructor Throws Exception if InvoiceLines is null", typeof(ArgumentOutOfRangeException),
			"Value '0' cannot be less than or equal to 0.\r\nParameter name: InvoiceLines", () => GetWrapper(entryLine));
		});
	}

	public void TestProcedure()
	{
		var procedure = wrapper.Procedure;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled Procedure", procedure);
			AssertSame("Cached Procedure", wrapper.Procedure, procedure);
		});
	}

	public void TestCountryOfOrigin()
	{
		invoiceLine.JI_CountryOfOrigin = "ES";
		AssertEquals("Expected filled CountryOfOrigin", "ES", wrapper.CountryOfOrigin);
	}

	public void TestCommodity()
	{
		var commodity = wrapper.Commodity;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled Commodity", commodity);
			AssertSame("Cached Commodity", wrapper.Commodity, commodity);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		var entryHeader = declaration.CustomsEntryHeaders[0];
		var entryLine = entryHeader.MergedLines[0];

		wrapper = GetWrapper(entryLine);
	}
	JobComInvoiceLine invoiceLine;
	IncompleteImportH1LineWrapper wrapper;

	IncompleteImportH1LineWrapper GetWrapper(CusEntryLine entryLine) => new IncompleteImportH1LineWrapper(entryLine);

	protected override IncompleteImportH1LineWrapper GetProvider() => wrapper;
}
