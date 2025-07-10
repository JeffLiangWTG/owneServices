using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class T2LPOUSCommonProofOperationInformationForT2LT2LFWrapperTest : WrapperHelperTest<T2LPOUSCommonProofOperationInformationForT2LT2LFWrapper>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown("Constructor Throws Exception if cusEntryHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","cusEntryHeader"), () => GetWrapper(null));

			var entryHeader = Factory.New<CusEntryHeader>();
			AssertExceptionThrown("Constructor Throws Exception if jobDeclaration is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","Declaration"), () => GetWrapper(entryHeader));
		});
	}

	public void TestDeclarationType()
	{
		declaration.ZG_CTStatusID = ExportCommunityTransitStatusList.Codes.T2LF;
		AssertEquals("Expected filled DeclarationType", "T2LF", wrapper.DeclarationType);
	}

	public void TestRequestedValidityOfTheProof()
	{
		CombineAssertions(() =>
		{
			AssertNull("Expected empty RequestedValidityOfTheProof when entryInstruction.ZG_NumberOfDays is 0 (<= 90)", wrapper.RequestedValidityOfTheProof);

			entryInstruction.ZG_NumberOfDays = 100;
			wrapper = GetWrapper(entryHeader);
			var requestedValidityOfTheProof = wrapper.RequestedValidityOfTheProof;
			AssertNotNull("Expected filled RequestedValidityOfTheProof", requestedValidityOfTheProof);
			AssertSame("Cached RequestedValidityOfTheProof", wrapper.RequestedValidityOfTheProof, requestedValidityOfTheProof);

			entryInstruction.ZG_NumberOfDays = 10;
			wrapper = GetWrapper(entryHeader);
			AssertNull("Expected empty RequestedValidityOfTheProof when entryInstruction.ZG_NumberOfDays is 10 (<= 90)", wrapper.RequestedValidityOfTheProof);
		});
	}

	public void TestRequestType()
	{
		entryInstruction.ZG_RequestType = RequestTypeList.Codes.RegistrationRequest;
		AssertEquals("Expected filled RequestType", "02", wrapper.RequestType);
	}

	public void TestNationalOnlyRequest()
	{
		CombineAssertions(() =>
		{
			entryInstruction.ZG_National = true;
			AssertEquals("Expected true NationalOnlyRequest when isReception is false and ZG_National is true", true, wrapper.NationalOnlyRequest);

			entryInstruction.ZG_National = false;
			AssertEquals("Expected false NationalOnlyRequest when isReception is false and ZG_National is false", false, wrapper.NationalOnlyRequest);

			wrapper = GetWrapper(entryHeader, isReception: true);
			AssertEquals("Expected true NationalOnlyRequest when isReception is true, even if ZG_National is false", true, wrapper.NationalOnlyRequest);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];

		wrapper = GetWrapper(entryHeader);
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;
	T2LPOUSCommonProofOperationInformationForT2LT2LFWrapper wrapper;

	T2LPOUSCommonProofOperationInformationForT2LT2LFWrapper GetWrapper(CusEntryHeader entryHeader, bool isReception = false) => new T2LPOUSCommonProofOperationInformationForT2LT2LFWrapper(entryHeader, isReception);

	protected override T2LPOUSCommonProofOperationInformationForT2LT2LFWrapper GetProvider() => wrapper;
}
