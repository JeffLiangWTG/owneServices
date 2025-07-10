using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.NCTS.Testing
{
	public class CtcMessageProcessorTestCase
	{
		public Action<NctsHeader> SetUpHeader { get; set; }
		public ZString IncomingMessageText { get; set; }
		public ZString MessageSubType { get; set; }
		public ZString OutgoingMessageSubType { get; set; }
		public ZString ExpectedNewMessageStatus { get; set; }
		public ZString ExpectedNewDeclarationStatus { get; set; }
		public ZString ExpectedNewArrivalStatus { get; set; }
		public ZString ExpectedNewDetailedStatus { get; set; }
		public ZString ExpectedNewMessageInterpretation { get; set; }
		public Action<NctsHeader> HeaderAssertion { get; set; }
		public Action<EDIMessage> MessageAssertion { get; set; }
	}
}
