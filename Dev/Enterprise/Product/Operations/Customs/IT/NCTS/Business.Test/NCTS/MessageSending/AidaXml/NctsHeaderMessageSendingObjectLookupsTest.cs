using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class NctsHeaderMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCancellationReasonCodeList()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		sendingObject.MessageType = "CAN";
		AssertType<NctsPhase5CancellationReasonList>("When message type is cancellation, CancellationCodeReasonList", sendingObject.Lookups.ReasonList);
		AssertEquals("When message type is cancellation, CancellationCodeReasonList", "A, B, G, H", sendingObject.Lookups.ReasonList.CodesAsString);
	}

	public void TestAmendmentReasonCodeList()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		sendingObject.MessageType = "AMD";
		AssertType<NctsPhase5AmendmentReasonList>("When message type is amendment, CodeReasonList", sendingObject.Lookups.ReasonList);
		AssertEquals("When message type is Amendment, CodeReasonList", "A, C, D, E, F", sendingObject.Lookups.ReasonList.CodesAsString);
	}

	public void TestNewDeclarationReasonCodeList()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		sendingObject.MessageType = "NEW";
		AssertType<CodeDescriptionPairList>("When message type is New Declaration, CodeReasonList", sendingObject.Lookups.ReasonList);
		AssertEquals("When message type is not New Declaration, CodeReasonList", "", sendingObject.Lookups.ReasonList.CodesAsString);
	}

	public void TestReasonCodeListIsCached()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		sendingObject.MessageType = "CAN";
		var reasonCodeList = sendingObject.Lookups.ReasonList;
		AssertSame("Result is cached", reasonCodeList, sendingObject.Lookups.ReasonList);
	}

	public void TestCancellationLegislativeReferenceList()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		sendingObject.MessageType = "CAN";
		AssertType<NctsPhase5CancellationLegislativeReferenceList>("When message type is cancellation, LegislativeReferenceList", sendingObject.Lookups.LegislativeReferenceList);
		AssertEquals("When message type is cancellation, LegislativeReferenceList", "2, 3", sendingObject.Lookups.LegislativeReferenceList.CodesAsString);
	}

	public void TestAmendmentLegislativeReferenceList()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		sendingObject.MessageType = "AMD";
		AssertType<NctsPhase5AmendmentLegislativeReferenceList>("When message type is Amendment, LegislativeReferenceList", sendingObject.Lookups.LegislativeReferenceList);
		AssertEquals("When message type is Amendment, LegislativeReferenceList", "1", sendingObject.Lookups.LegislativeReferenceList.CodesAsString);
	}

	public void TestNewDeclarationLegislativeReferenceList()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		sendingObject.MessageType = "NEW";

		AssertType<CodeDescriptionPairList>("When message type is New Declaration, LegislativeReferenceList", sendingObject.Lookups.LegislativeReferenceList);
		AssertEquals("When message type is New Declaration, LegislativeReferenceList", "", sendingObject.Lookups.LegislativeReferenceList.CodesAsString);
	}

	public void TestCancellationLegislativeReferenceListIsCached()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		sendingObject.MessageType = "CAN";
		var referenceList = sendingObject.Lookups.LegislativeReferenceList;
		AssertSame("Result is cached", referenceList, sendingObject.Lookups.LegislativeReferenceList);
	}

	public void TestAmendmentLegislativeReferenceListIsCached()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		sendingObject.MessageType = "AMD";
		var referenceList = sendingObject.Lookups.LegislativeReferenceList;
		AssertSame("Result is cached", referenceList, sendingObject.Lookups.LegislativeReferenceList);
	}

	public void TestMessageSubTypeList()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		AssertEquals("MessageSubTypeList", "D1, D2", sendingObject.Lookups.MessageSubTypeList.CodesAsString);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
	}

	NctsHeader nctsHeader;
}
