using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class T2LPOUSReceptionProofOperationInformationForT2LT2LFWrapperTest : WrapperHelperTest<T2LPOUSReceptionProofOperationInformationForT2LT2LFWrapper>
{
	public void TestCodigoReferencia()
	{
		entryHeader.MovementReferenceNumber = "24ES123456789";
		AssertEquals("Expected filled CodigoReferencia", "24ES123456789", wrapper.CodigoReferencia);
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

		wrapper = new T2LPOUSReceptionProofOperationInformationForT2LT2LFWrapper(entryHeader);
	}
	CusEntryHeader entryHeader;
	T2LPOUSReceptionProofOperationInformationForT2LT2LFWrapper wrapper;

	protected override T2LPOUSReceptionProofOperationInformationForT2LT2LFWrapper GetProvider() => wrapper;
}
