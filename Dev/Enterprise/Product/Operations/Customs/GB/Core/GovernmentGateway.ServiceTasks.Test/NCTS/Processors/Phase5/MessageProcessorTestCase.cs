using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.NCTS.Testing;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	public class MessageProcessorTestCase : CtcMessageProcessorTestCase
	{
		public string LRN { get; set; }
		public string MRN { get; set; }
		public string CorrelationIdentifier { get; set; }

		public string InitialTransitStatus { get; set; }
		public string InitialPhase { get; set; }

		public string ExpectedPhase { get; set; }
		public string ExpectedMovementType { get; set; } = NctsMovementType.Codes.DepartureAndArrival;
		public string ExpectedAcceptMovementType { get; set; } = NctsMovementType.Codes.DepartureAndArrival;

		public Action<NctsHeader> SetupGuaranteesAndTransactions { get; set; }
		public Action<NctsHeader> GuaranteesAndTransactionsAssertion { get; set; }
		public Func<IDisposable> SetupRegistry { get; set; }
	}
}
