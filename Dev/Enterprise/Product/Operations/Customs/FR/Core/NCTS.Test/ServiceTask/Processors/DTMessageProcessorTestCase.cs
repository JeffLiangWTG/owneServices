using System;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.NCTS.ServiceTask.Testing
{
	class DTMessageProcessorTestCase
	{
		public Action<NctsHeader> SetUpHeader { get; set; }
		public ZString IncomingMessageText { get; set; }
		public ZString ExpectedNewMessageStatus { get; set; }
		public ZString ExpectedNewDepartureStatus { get; set; }
		public ZString ExpectedNewArrivalStatus { get; set; }
		public ZString ExpectedNewDetailedDepartureStatus { get; set; }
		public ZString ExpectedNewDetailedArrivalStatus { get; set; }
		public ZString ExpectedNewMessageInterpretation { get; set; }
		public Action<NctsHeader> HeaderAssertion { get; set; }
		public Action<EDIMessage> MessageAssertion { get; set; }
	}
}
