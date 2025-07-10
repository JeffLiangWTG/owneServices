using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

class ImportH1CommonLineWrapperTest : WrapperHelperTest<ImportH1CommonLineWrapper>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown("Constructor Throws Exception if entryLine is null", typeof(ArgumentNullException),
			"Value cannot be null.\r\nParameter name: entryLine", () => GetWrapper(null));
		});
	}

	public void TestDeclarationGoodsItemNumber()
	{
		entryLine.CL_LineNumber = 3;
		AssertEquals("Expected filled SequenceNumber", "3", wrapper.DeclarationGoodsItemNumber);
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
		invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = GetWrapper(entryLine);
	}
	CusEntryLine entryLine;
	ImportH1CommonLineWrapper wrapper;

	ImportH1CommonLineWrapper GetWrapper(CusEntryLine entryLine) => new ImportH1CommonLineWrapper(entryLine);

	protected override ImportH1CommonLineWrapper GetProvider() => wrapper;
}
