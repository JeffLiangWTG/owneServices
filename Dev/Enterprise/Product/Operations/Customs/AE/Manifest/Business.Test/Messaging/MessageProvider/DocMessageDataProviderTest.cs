using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Business;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class DocMessageDataProviderTest : TestCaseWithFactory
{
	public void TestCUSCARMessageMatcher() => CombineAssertions(() =>
	{
		var message = "BGM+716+AABPVQA0000000000004:002+9'";
		var reference = new ReferenceElements();
		reference.ReferenceIdentifier = "AABPVQA0000000000004";
		reference.VersionIdentifier = "002";
		Assert("Don't match when reference is null", !DocMessageDataProvider.CUSCARMessageMatcher(message, null));
		Assert("Match", DocMessageDataProvider.CUSCARMessageMatcher(message, reference));
		reference.VersionIdentifier = "003";
		Assert("Don't match when version differnt", !DocMessageDataProvider.CUSCARMessageMatcher(message, reference));
	});

	public void TestIsRFICUSRESMessage() => CombineAssertions(() =>
	{
		var message = "GEI++RFI'";
		Assert("Match", DocMessageDataProvider.IsRFICUSRESMessage(message));
		message = "GEI++ABC'";
		Assert("Don't match", !DocMessageDataProvider.IsRFICUSRESMessage(message));
	});

	public void TestGetInterchangeControl() => CombineAssertions(() =>
	{
		var message = @"UNB+UNOB:4::2:02+UAENAIC+AABPVQA::MPC1234:ABC4321+20240903:1322+132211B0000003'
UNH+132211H0000003+CUSRES:D:23A:UN'
GEI++RFI'
RFF+DM:AABPVQA0000000000004::002'
UNT+3+132211H0000003'
UNZ+5+132211B0000003'";
		Assert("GetInterchangeControl", DocMessageDataProvider.GetInterchangeControl(message, out var controlReference, out var reference));
		AssertEquals("ControlReference", "132211B0000003", controlReference);
		AssertEquals("Reference", "AABPVQA0000000000004", reference.ReferenceIdentifier);
		AssertEquals("Version", "002", reference.VersionIdentifier);
	});

	public void TestSenderIdentification() => AssertEquals("SenderIdentification", "AABPVQA", Provider.SenderIdentification);

	public void TestSenderInternalIdentification() => AssertEquals("SenderInternalIdentification", "AAALFQA", Provider.SenderInternalIdentification);

	public void TestSenderInternalSubIdentification() => AssertEquals("SenderInternalSubIdentification", "AAALFMM", Provider.SenderInternalSubIdentification);

	public void TestReferenceDocumentID() => AssertEquals("ReferenceDocumentID", "AABPVQA0000000000004", Provider.ReferenceDocumentID);

	public void TestDocumentIdentifier() => AssertEquals("DocumentIdentifier", "AABPVQA0000000000005", Provider.DocumentIdentifier);

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
	DocMessageDataProvider Provider => provider ??= new DocMessageDataProvider(sendingObject);
	DocMessageDataProvider provider;
}
