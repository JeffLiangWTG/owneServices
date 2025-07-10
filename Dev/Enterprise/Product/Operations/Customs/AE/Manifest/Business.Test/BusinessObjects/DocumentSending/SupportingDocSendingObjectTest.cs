using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(SupportingDocSendingObject))]
sealed class SupportingDocSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestCUSRESAndCUSCAR() => CombineAssertions(() =>
	{
		AssertEquals("CUSRES", "132211B0000003", sendingObject.CUSRES);
		AssertEquals("CUSCAR", "AABPVQA0000000000004", sendingObject.CUSCAR);
	});

	public void TestCUSRESMessage() => AssertEquals(cUSRES, sendingObject.CUSRESMessage);

	public void TestCUSCARMessage() => AssertEquals(cUSCAR, sendingObject.CUSCARMessage);

	public void TestCUSRESList()
	{
		var list = sendingObject.CUSRESList;
		AssertEquals("132211B0000003", list.CodesAsString);
	}

	public void TestValidationType() => AssertType<SupportingDocSendingObjectValidation>(sendingObject.Validation);

	protected override BusinessObject GetNewBusinessObject()
	{
		var manifest = Factory.New<AsycudaManifestHeader>();
		return manifest.GetSupportingDocSendingObject();
	}

	protected override void SetUp()
	{
		base.SetUp();
		var manifest = Factory.New<AsycudaManifestHeader>();
		var bill = manifest.Bills.AddNew();
		cUSRES = bill.Messages.AddNew();
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
		cUSCAR = bill.Messages.AddNew();
		SetMessage(cUSCAR, "message2", AEConstants.Messaging.MessageTypes.CUSCAR);
		cUSCAR.EM_MessageText = "BGM+716+AABPVQA0000000000004:002+9'";
		var message3 = bill.Messages.AddNew();
		SetMessage(message3, "message3", AEConstants.Messaging.MessageTypes.CUSCAR);
		sendingObject = (SupportingDocSendingObject)manifest.GetSupportingDocSendingObject();
		var list = sendingObject.CUSRESList;
		sendingObject.CUSRES = "132211B0000003";
	}
	SupportingDocSendingObject sendingObject;
	EDIMessage cUSRES;
	EDIMessage cUSCAR;

	void SetMessage(EDIMessage message, string messageNum, string msessageType)
	{
		message.EM_MessageNum = messageNum;
		message.EM_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
		message.EM_MessageType = msessageType;
	}
}
