using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EvvRequestSendingObject))]
sealed class EvvRequestSendingObjectTest : TestCaseWithFactory
{
	public void TestEntryHeader() => AssertSame(EntryHeader, SendingObject.MessageOwner);

	public void TestMessageSubTypeForEDIMessage() => AssertEquals("subType", SendingObject.MessageSubTypeForEDIMessage);

	public void TestDocumentType() => CombineAssertions(() =>
	{
		AssertDocumentType(MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, EvvDocumentType.Codes.TaxationDecisionCustomsDuties);
		AssertDocumentType(MessageSubTypeCodeList.Codes.TaxationDecisionVat, EvvDocumentType.Codes.TaxationDecisionVAT);
		AssertDocumentType(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties, EvvDocumentType.Codes.RefundCustomsDuties);
		AssertDocumentType(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat, EvvDocumentType.Codes.RefundVAT);

		void AssertDocumentType(string messageSubType, string expectedDocumentType)
		{
			var sendingObject = new EvvRequestSendingObject(EntryHeader, "123", 4, messageSubType);
			AssertEquals(messageSubType, expectedDocumentType, sendingObject.DocumentType);
		}
	});

	public void TestFactory() => AssertSame(Factory, SendingObject.Factory);

	public void TestApplicationCode() => AssertEquals(ApplicationCodes.CHCustomsEdec, SendingObject.ApplicationCode);

	public void TestMessageTypeForEDIMessage() => AssertEquals(MessageTypeCodeList.Codes.EVV, SendingObject.MessageTypeForEDIMessage);

	public void TestGetApplicationReference() => AssertEquals($"{SendingObject.Mrn}.{SendingObject.MrnVersion}", SendingObject.GetApplicationReference());

	public void TestGetGlbExternalPasswordPK() => AssertEquals(GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword.PK, SendingObject.GetCredentialPK());

	public void TestMrn() => AssertEquals("M100", SendingObject.Mrn);

	public void TestMrnVersion() => AssertEquals(2, SendingObject.MrnVersion);

	public void TestToMessageString() => CombineAssertions(() =>
	{
		GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "C100";
		var entryHeader = Factory.New<CusEntryHeader>();
		var sendingObject = new EvvRequestSendingObject(entryHeader, "M100", 2, MessageSubTypeCodeList.Codes.TaxationDecisionVat);
		var message = sendingObject.ToMessageString();

		Assert("requestorTraderIdentificationNumber", message.Contains("requestorTraderIdentificationNumber>C100<"));
		Assert("customsDeclarationNumber", message.Contains("customsDeclarationNumber>M100<"));
		Assert("customsDeclarationNumber", message.Contains("customsDeclarationVersion>2<"));
		Assert("documentType", message.Contains("documentType>taxationDecisionVAT<"));
	});

	CusEntryHeader EntryHeader => entryHeader ??= Factory.New<CusEntryHeader>();
	CusEntryHeader entryHeader;

	EvvRequestSendingObject SendingObject => sendingObject ??= new EvvRequestSendingObject(EntryHeader, "M100", 2, "subType");
	EvvRequestSendingObject sendingObject;
}
