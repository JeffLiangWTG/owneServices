using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.ES.Business.Testing;

public class T2LPOUSRequestAndReceptionSendMessageWrapperTest : WrapperHelperTest<T2LPOUSRequestAndReceptionSendMessageWrapper>
{
	public void TestAuthorisation()
	{
		CombineAssertions(() =>
		{
			entryInstruction.ZG_RequestType = RequestTypeList.Codes.RegistrationRequest;
			wrapper = GetWrapper(entryHeader);
			AssertNull("Expected empty Authorisation when no authorisation declared in entryInstruction", wrapper.Authorisation);

			var auth1 = entryInstruction.CusAuthorizationUsages.AddNew();
			auth1.AGC_Code = "ACP";
			var auth2 = entryInstruction.CusAuthorizationUsages.AddNew();
			auth2.AGC_Code = "AAA";

			wrapper = GetWrapper(entryHeader);
			var authorisation = wrapper.Authorisation;
			AssertNotNull("Expected filled Authorisation with the correct authorisation from EntryInstruction", authorisation);
			AssertSame("Cached Authorisation", wrapper.Authorisation, authorisation);
			AssertEquals("Expected filled Authorisation with the correct Type mapped", "C511", authorisation.TypeOfAuthorisation);

			entryInstruction.ZG_RequestType = RequestTypeList.Codes.EndorsementRequest;
			wrapper = GetWrapper(entryHeader);
			AssertNull("Expected empty Authorisation when request type is not 02", wrapper.Authorisation);
		});
	}

	public void TestNullRepresentative()
	{
		declaration.JE_OA_Representative = ZGuid.Empty;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		AssertExceptionThrown<NullReferenceException>(() => wrapper.Representative.ToString());
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

			declaration.JE_OA_Representative = orgAddress2.PK;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled Representative with Representative when Representative is not empty", wrapper.Representative.Id, "ABC123456");
		});
	}

	public void TestGoodsShipment()
	{
		var goodsShipment = wrapper.GoodsShipment;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled GoodsShipment", goodsShipment);
			AssertSame("Cached GoodsShipment", wrapper.GoodsShipment, goodsShipment);
		});
	}

	public void TestNullPersonReqPres()
	{
		CombineAssertions(() =>
		{
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>(() => wrapper.PersonReqPres.ToString());
		});
	}

	public void TestSendEmailL()
	{
		AssertEquals("SendEmailL is implemented in each child class", "S", wrapper.SendEmailL);
	}

	public void TestSendEmailU()
	{
		AssertEquals("SendEmailU is implemented in each child class", "S", wrapper.SendEmailU);
	}

	public void TestSendEmailExp()
	{
		AssertEquals("SendEmailExp is implemented in each child class", "S", wrapper.SendEmailExp);
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
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
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
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;
	T2LPOUSRequestAndReceptionSendMessageWrapper wrapper;

	T2LPOUSRequestAndReceptionSendMessageWrapper GetWrapper(CusEntryHeader entryHeader) => new T2LPOUSRequestAndReceptionSendMessageWrapper(entryHeader, Certificate);

	protected override T2LPOUSRequestAndReceptionSendMessageWrapper GetProvider() => wrapper;
}
