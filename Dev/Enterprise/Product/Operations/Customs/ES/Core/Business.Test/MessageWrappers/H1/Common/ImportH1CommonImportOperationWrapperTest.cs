using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

class ImportH1CommonImportOperationWrapperTest : WrapperHelperTest<ImportH1CommonImportOperationWrapper>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown("Constructor Throws Exception if entryHeader is null", typeof(ArgumentNullException),
			"Value cannot be null.\r\nParameter name: entryHeader", () => GetWrapper(null));

			AssertExceptionThrown("Constructor Throws Exception if jobDeclaration is null", typeof(ArgumentNullException),
			"Value cannot be null.\r\nParameter name: Declaration", () => GetWrapper(Factory.New<CusEntryHeader>()));
		});
	}

	public void TestLRN()
	{
		AssertEquals("Expected filled LRN with only entry ref num when no declarant with id is declared", "ES00001", wrapper.LRN);
	}

	public void TestDeclarationType()
	{
		declaration.JE_MessageSubType = "CO";
		AssertEquals("Expected filled DeclarationType", "CO", wrapper.DeclarationType);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "11";

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.CH_BGMReference = "ES00001";

		wrapper = GetWrapper(entryHeader);
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	ImportH1CommonImportOperationWrapper wrapper;

	ImportH1CommonImportOperationWrapper GetWrapper(CusEntryHeader entryHeader) => new ImportH1CommonImportOperationWrapper(entryHeader);

	protected override ImportH1CommonImportOperationWrapper GetProvider() => wrapper;
}
