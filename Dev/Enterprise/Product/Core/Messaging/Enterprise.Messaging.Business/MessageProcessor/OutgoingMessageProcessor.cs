using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business.MessageProcessor
{
	public abstract class OutgoingMessageProcessor : BaseOutgoingMessageProcessor
	{
		protected OutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override ZQuery GetNewFilter()
		{
			var filter = base.GetNewFilter();
			filter.AddToFilter(EDIMessageSchema.EM_EI, null);
			return filter;
		}

		protected abstract InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages);

		protected override ZString PreProcessMessages(NonDependentEDIMessageCollection readyMessages)
		{
			ZString errorInPacking = ZString.Empty;

			var discardQueuedMessages = false;
			using (var provider = CreateNewInterchangeProvider(readyMessages))
			{
				if (provider == null)
				{
					errorInPacking = ErrorWhileCreatingInterchangeProvider;
					discardQueuedMessages = true;
				}
				else
				{
					var interchanges = provider.Interchanges;
					if (interchanges.Length == 0 && readyMessages.Count > 0)
					{
						errorInPacking = NoInterchangeHasBeenCreated;
						discardQueuedMessages = true;
					}
					errorInPacking += provider.GetErrorsOnDiscardedMessages();
				}
			}

			if (discardQueuedMessages)
			{
				readyMessages.Cast<EDIMessage>().Where(x => x.EM_Status == EDIMessage.Status.Queued).ForEach(x => x.EM_Status = EDIMessage.Status.Discarded);
			}
			return errorInPacking;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant")]
		const string ErrorWhileCreatingInterchangeProvider = "Error while creating Interchange Provider.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant")]
		const string NoInterchangeHasBeenCreated = "No Interchange has been created";
	}
}
