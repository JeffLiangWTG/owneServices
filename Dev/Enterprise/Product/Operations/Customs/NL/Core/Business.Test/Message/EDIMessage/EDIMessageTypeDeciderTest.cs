using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class EDIMessageTypeDeciderTest : TestCaseWithFactory
{
	public void TestGetTypeForBinding()
	{
		AssertNull(typeDecider.GetTypeForBinding());
	}

	public void TestGetTypeForNew()
	{
		AssertNull(typeDecider.GetTypeForNew());
	}

	public void TestGetTypeForLoad_Receive_DMS()
	{
		AssertGetTypeForLoad(typeof(NLEDIMessage), EDIMessage.Direction.Receive, NLEDIMessageTypes.Codes.DMS);
	}

	public void TestGetTypeForLoad_Receive_NCTS()
	{
		AssertGetTypeForLoad(typeof(NLEDIMessage), EDIMessage.Direction.Receive, NLEDIMessageTypes.Codes.NCT);
	}

	public void TestGetTypeForLoad_Receive_Unknown()
	{
		AssertGetTypeForLoad(typeof(NLEDIMessage), EDIMessage.Direction.Receive, "XX");
	}

	public void TestGetTypeForLoad_Transmit_DMS_CAN()
	{
		AssertGetTypeForLoad(typeof(CancelDeclarationEDIMessage), EDIMessage.Direction.Transmit, NLEDIMessageTypes.Codes.DMS, ExportSendMessageTypes.Codes.CAN);
	}

	public void TestGetTypeForLoad_Transmit_DMS_AMD()
	{
		AssertGetTypeForLoad(typeof(NLComparisonEDIMessage), EDIMessage.Direction.Transmit, NLEDIMessageTypes.Codes.DMS, ExportSendMessageTypes.Codes.AMD);
	}

	public void TestGetTypeForLoad_Transmit_DMS_CRI()
	{
		AssertGetTypeForLoad(typeof(NLComparisonEDIMessage), EDIMessage.Direction.Transmit, NLEDIMessageTypes.Codes.DMS, ImportSendMessageTypes.Codes.CRI);
	}

	public void TestGetTypeForLoad_Transmit_DMS_Unknown()
	{
		AssertGetTypeForLoad(typeof(NLEDIMessage), EDIMessage.Direction.Transmit, NLEDIMessageTypes.Codes.DMS, "XX");
	}

	void AssertGetTypeForLoad(Type type, string receiveTransmit, string messageType, string messageSubType = "")
	{
		var message = Factory.New<NLEDIMessage>();
		message.EM_ReceiveTransmit = receiveTransmit;
		message.EM_MessageType = messageType;
		message.EM_MessageSubType = messageSubType;
		var row = ((INeedRow)message).Row;
		AssertEquals($"{receiveTransmit} {messageType} {messageSubType}", type, typeDecider.GetTypeForLoad(row, Factory));
	}

	protected override void SetUp()
	{
		typeDecider = new EDIMessageTypeDecider();
	}

	TypeDecider typeDecider;
}
