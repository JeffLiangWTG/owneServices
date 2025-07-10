using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class T2LPOUSRequestProofOperationInformationForT2LT2LFWrapperTest : WrapperHelperTest<T2LPOUSRequestProofOperationInformationForT2LT2LFWrapper>
{
	public void TestLRN()
	{
		entryHeader.CH_BGMReference = "ES00001";
		AssertEquals("Expected filled LRN", "ES00001", wrapper.LRN);
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
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];

		wrapper = new T2LPOUSRequestProofOperationInformationForT2LT2LFWrapper(entryHeader);
	}
	CusEntryHeader entryHeader;
	T2LPOUSRequestProofOperationInformationForT2LT2LFWrapper wrapper;

	protected override T2LPOUSRequestProofOperationInformationForT2LT2LFWrapper GetProvider() => wrapper;
}
