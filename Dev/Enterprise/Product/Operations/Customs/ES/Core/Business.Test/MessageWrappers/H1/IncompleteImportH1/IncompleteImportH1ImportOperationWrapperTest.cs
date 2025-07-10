using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

class IncompleteImportH1ImportOperationWrapperTest : WrapperHelperTest<IncompleteImportH1ImportOperationWrapper>
{
	public void TestCustomsRegistrationNumber()
	{
		AssertEquals("Expected filled MRN", "MRNNumber", wrapper.CustomsRegistrationNumber);
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
		AssertEquals("Merge done", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];
		entryHeader.MovementReferenceNumber = "MRNNumber";

		wrapper = new IncompleteImportH1ImportOperationWrapper(entryHeader);
	}
	CusEntryHeader entryHeader;
	IncompleteImportH1ImportOperationWrapper wrapper;

	protected override IncompleteImportH1ImportOperationWrapper GetProvider() => wrapper;
}
