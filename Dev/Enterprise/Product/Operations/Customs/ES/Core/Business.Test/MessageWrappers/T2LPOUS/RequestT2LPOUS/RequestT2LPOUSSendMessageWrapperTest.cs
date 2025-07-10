using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.ES.Business.Testing;

public class RequestT2LPOUSSendMessageWrapperTest : WrapperHelperTest<RequestT2LPOUSSendMessageWrapper>
{
	public void TestProofOperationInformationForT2LT2LF()
	{
		var proofOperationInformationForT2LT2LF = wrapper.ProofOperationInformationForT2LT2LF;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled ProofOperationInformationForT2LT2LF", proofOperationInformationForT2LT2LF);
			AssertSame("Cached ProofOperationInformationForT2LT2LF", wrapper.ProofOperationInformationForT2LT2LF, proofOperationInformationForT2LT2LF);
		});
	}

	public void TestPersonReqPres()
	{
		CombineAssertions(() =>
		{
			var orgAddress = Factory.New<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;
			declaration.JE_OH_Supplier = orgHeader.PK;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;

			var personReqPres = wrapper.PersonReqPres;

			AssertNotNull("Expected filled PersonReqPres", personReqPres);
			AssertSame("Cached PersonReqPres", wrapper.PersonReqPres, personReqPres);
			AssertNotNull("ContactPerson is filled if no representative", wrapper.PersonReqPres.ContactPerson);
			AssertEquals("PersonReqPres Contact Email is the one in Misc", "wisetech@wisetechglobal.com", personReqPres.ContactPerson.Email);

			var orgAddress2 = Factory.New<OrgAddress>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgAddress2.OA_OH = orgHeader2.PK;
			declaration.JE_OA_Representative = orgAddress2.PK;
			wrapper = GetWrapper(entryHeader);
			AssertNull("ContactPerson is not filled if has representative", wrapper.PersonReqPres.ContactPerson);
		});
	}

	public void TestRepresentative()
	{
		CombineAssertions(() =>
		{
			var orgAddress = Factory.New<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "DEF789012");
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			var orgAddress2 = Factory.New<OrgAddress>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "ABC123456");
			orgAddress2.OA_OH = orgHeader2.PK;
			orgAddress2.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

			declaration.JE_OA_DeclarantAddress = orgAddress.PK;

			wrapper = GetWrapper(entryHeader);
			var representative = wrapper.Representative;
			AssertNotNull("Expected filled Representative", representative);
			AssertEquals("Expected filled Representative with Declarant when Representative is empty", wrapper.Representative.Id, "DEF789012");
			AssertSame("Cached Representative", wrapper.Representative, representative);
			AssertEquals("Representative Contact Email is the one in Misc", "wisetech@wisetechglobal.com", representative.ContactPerson.Email);

			declaration.JE_OH_Supplier = orgHeader.PK;
			wrapper = GetWrapper(entryHeader);
			AssertNull("Expected null Representative when Declarant is the same as the Supplier", wrapper.Representative);

			declaration.JE_OA_Representative = orgAddress2.PK;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled Representative with Representative when Representative is not empty", wrapper.Representative.Id, "ABC123456");

			declaration.JE_OH_Supplier = orgHeader2.PK;
			wrapper = GetWrapper(entryHeader);
			AssertNull("Expected null Representative when Representative is the same as the Supplier", wrapper.Representative);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.InvoiceLines.AddNew();

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge failed", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];

		var contactEmail = "wisetech@wisetechglobal.com";
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(contactEmail))
		{
			wrapper = GetWrapper(entryHeader);
		}
	}
	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	RequestT2LPOUSSendMessageWrapper wrapper;

	RequestT2LPOUSSendMessageWrapper GetWrapper(CusEntryHeader entryHeader) => new RequestT2LPOUSSendMessageWrapper(entryHeader, Certificate);

	protected override RequestT2LPOUSSendMessageWrapper GetProvider() => wrapper;
}
