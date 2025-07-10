using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc022c;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5
{
	class CC022CProcessor : NctsBaseProcessor<Cc022CType>
	{
		public CC022CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "Notification To Amend Declaration";

		protected override ZString LRN => null;
		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override string GetMessageId(Cc022CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc022CType messageObject) => messageObject.MessageType.ToString();

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Departure;
	}
}
