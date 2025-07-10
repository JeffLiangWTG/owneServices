using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class MPCIAttachmentMessageProviderTest : TestCaseWithFactory
{
	public void TestMessageId() => Assert(ZGuid.ParseSafe(AttachmentMessageProvider.MessageId).IsValid);

	public void TestSenderMPCIPartyID1() => AssertEquals("AABPVQA", AttachmentMessageProvider.SenderMPCIPartyID1);

	public void TestSenderMPCIPartyID2() => AssertEquals("AAALFQA", AttachmentMessageProvider.SenderMPCIPartyID2);

	public void TestSenderMPCIPartyID3() => AssertEquals("AAALFMM", AttachmentMessageProvider.SenderMPCIPartyID3);

	public void TestReferenceDocumentID() => AssertEquals("AABPVQA0000000000004", AttachmentMessageProvider.ReferenceDocumentID);

	public void TestSu() => AssertEquals("AABPVQA0000000000005", AttachmentMessageProvider.Su);

	public void TestProcessingPriority() => AssertEquals("A", AttachmentMessageProvider.ProcessingPriority);

	public void TestTestIndicator() => AssertEquals(0, AttachmentMessageProvider.TestIndicator);

	public void TestAttachmentFileType() => CombineAssertions(() =>
	{
		AssertEquals("PDF", AttachmentMessageProvider.AttachmentFileType);
		sendingObject.DocumentType = ".PDF";
		AssertEquals("PDF", new MPCIAttachmentMessageProvider(sendingObject).AttachmentFileType);
	});

	protected override void SetUp()
	{
		base.SetUp();
		var manifest = Factory.New<AsycudaManifestHeader>();
		var bill = manifest.Bills.AddNew();
		var cUSRES = bill.Messages.AddNew();
		SetMessage(cUSRES, "message1", AEConstants.Messaging.MessageTypes.CUSRES);
		cUSRES.EM_MessageText = "RFF+DM:message2::1'";
		var interchange = Factory.New<EDIInterchange>();
		interchange.EI_BodyText = @"UNB+UNOB:4::2:02+UAENAIC+AABPVQA::MPC1234:ABC4321+20240903:1322+132211B0000003'
UNH+132211H0000003+CUSRES:D:23A:UN'
GEI++RFI'
RFF+DM:AABPVQA0000000000004::002'
UNT+3+132211H0000003'
UNZ+5+132211B0000003'";
		cUSRES.EM_EI = interchange.PK;
		cUSRES.EM_MessageText = "BGM+716+AABPVQA0000000000005:002+9'";

		var interchange1 = Factory.New<EDIInterchange>();
		interchange1.EI_BodyText = "UNB+UNOB:4::2:02+AABPVQA::AAALFQA:AAALFMM+UAENAIC+20250425:0152+015251B0000116+++A+++0'";
		var cUSCAR = bill.Messages.AddNew();
		SetMessage(cUSCAR, "message2", AEConstants.Messaging.MessageTypes.CUSCAR);
		cUSCAR.EM_MessageText = "BGM+716+AABPVQA0000000000004:002+9'";
		cUSCAR.EM_EI = interchange1.PK;
		sendingObject = (SupportingDocSendingObject)manifest.GetSupportingDocSendingObject();
		var list = sendingObject.CUSRESList;
		sendingObject.CUSRES = "132211B0000003";
		sendingObject.DocumentType = "PDF";
	}

	void SetMessage(EDIMessage message, string messageNum, string msessageType)
	{
		message.EM_MessageNum = messageNum;
		message.EM_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		message.EM_MessageType = msessageType;
	}
	SupportingDocSendingObject sendingObject;
	MPCIAttachmentMessageProvider AttachmentMessageProvider => attachmentMessageProvider ??= new MPCIAttachmentMessageProvider(sendingObject);
	MPCIAttachmentMessageProvider attachmentMessageProvider;
}
