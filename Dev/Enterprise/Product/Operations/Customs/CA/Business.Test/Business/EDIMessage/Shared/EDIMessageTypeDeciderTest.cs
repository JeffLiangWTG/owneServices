using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CA.Business.Testing
{
	public class EDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestTypeDecider()
		{
			CheckMessage(typeof(SUPRPTMessage));
			CheckMessage(typeof(EX1STPMessage));
			CheckMessage(typeof(EDIReleaseMessage));
			CheckMessage(typeof(B3Message));
			CheckMessage(typeof(QueryMessage));
			CheckMessage(typeof(SyntaxErrorMessage));
			CheckMessage(typeof(RNSRequestMessage));
			CheckMessage(typeof(EDIMessage));
			CheckMessage(typeof(DLMMessage));
			CheckMessage(typeof(K84Message));
			CheckMessage(typeof(TCPMessage));
			CheckMessage(typeof(RSFMessage));
			CheckMessage(typeof(ACIHouseBillMessage));
			CheckMessage(typeof(ACIForwarderCloseMessage));
			CheckMessage(typeof(AVSQueryMessage));
			CheckMessage(typeof(IIDUniversalShipmentMessage));
			CheckMessage(typeof(CARMDailyNoticeMessage));
			CheckMessage(typeof(CARMStatementOfAccountMessage));
			var message = Factory.New<EDIMessage>();
			message.EM_MessageSubType = MessageTypeList.Codes.DataLoadingModule;
			AssertEquals(typeof(DLMMessage), decider.GetTypeForLoad(((INeedRow)message).Row, Factory));
			message = Factory.New<EDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
			AssertEquals(typeof(ARLMessage), decider.GetTypeForLoad(((INeedRow)message).Row, Factory));
			message = Factory.New<EDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = ARLMessageTypes.Codes.StatementOfAccount;
			AssertEquals(typeof(ARLMessage), decider.GetTypeForLoad(((INeedRow)message).Row, Factory));
			message = Factory.New<EDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = ARLMessageTypes.Codes.DailyNotice;
			AssertEquals(typeof(ARLMessage), decider.GetTypeForLoad(((INeedRow)message).Row, Factory));
			message = Factory.New<EDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			AssertEquals(typeof(UniversalEventMessage), decider.GetTypeForLoad(((INeedRow)message).Row, Factory));
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = UniversalEventMessageTypes.Codes.IIDResponses;
			AssertEquals(typeof(UniversalEventMessage), decider.GetTypeForLoad(((INeedRow)message).Row, Factory));
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = UniversalEventMessageTypes.Codes.D4Notices;
			AssertEquals(typeof(UniversalEventMessage), decider.GetTypeForLoad(((INeedRow)message).Row, Factory));
			message.EM_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			AssertEquals(typeof(CADMessage), decider.GetTypeForLoad(((INeedRow)message).Row, Factory));
			message.EM_MessageType = MessageTypeList.Codes.CARMDailyNotice;
			AssertEquals(typeof(CARMDailyNoticeMessage), decider.GetTypeForLoad(((INeedRow)message).Row, Factory));
			message.EM_MessageType = MessageTypeList.Codes.CARMStatementOfAccount;
			AssertEquals(typeof(CARMStatementOfAccountMessage), decider.GetTypeForLoad(((INeedRow)message).Row, Factory));
		}

		#region Implementation

		protected void CheckMessage(Type typeToCheck)
		{
			CheckMessage(typeToCheck, typeToCheck);
		}

		protected void CheckMessage(Type typeToCheck, Type typeExpected)
		{
			var message = Factory.New(typeToCheck);
			AssertEquals(typeExpected, decider.GetTypeForLoad(((INeedRow)message).Row, Factory));
		}

		protected override void SetUp()
		{
			base.SetUp();
			decider = new EDIMessageTypeDecider();
		}

		EDIMessageTypeDecider decider;
		#endregion

	}
}
