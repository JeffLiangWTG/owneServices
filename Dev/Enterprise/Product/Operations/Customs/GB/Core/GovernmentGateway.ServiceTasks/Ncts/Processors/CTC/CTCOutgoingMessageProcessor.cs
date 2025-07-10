using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts
{
	public class CTCOutgoingMessageProcessor : OutgoingMessageProcessor
	{
		public CTCOutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override ZQuery MessageFilter => messageFilter ?? (messageFilter = GetMessageFilterQuery());
		ZQuery messageFilter;

		static ZQuery GetMessageFilterQuery()
		{
			var result = new ZQuery();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.GbCommonTransitConvention);
			return result;
		}

		protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
		{
			return new CTCInterchangeProvider(Logger, readyMessages);
		}

		protected override ZString PreProcessMessages(NonDependentEDIMessageCollection readyMessages)
		{
			foreach (EDIMessage message in readyMessages.ToArray())
			{
				if (!CheckForAccessToken(message))
				{
					readyMessages.Remove(message);
				}
			}

			return base.PreProcessMessages(readyMessages);
		}

		bool CheckForAccessToken(EDIMessage msg)
		{
			var validToken = true;

			if (msg.EM_LinkedObject is Business.NctsHeader header)
			{
				validToken = header.HasValidAccessToken();

				if (!validToken)
				{
					UpdateMessageHeldUntilDate(msg);
				}
			}

			return validToken;
		}

		void UpdateMessageHeldUntilDate(EDIMessage msg)
		{
			var held = msg.EM_HeldUntilDate.IsEmpty ? ZDateTime.UtcNow : msg.EM_HeldUntilDate;
			var created = msg.EM_SystemCreateTimeUtc;

			var timeDiff = held - created;

			if (timeDiff.TotalMinutes >= 60)
			{
				msg.EM_Status = EDIMessage.Status.Failed;
				Logger.LogError(System.FormattableString.Invariant($"Message sending failed: A valid access token was not found for message {msg.EM_MessageNum}"));
			}
			else
			{
				msg.EM_HeldUntilDate = held.AddMinutes(10);
				ServiceTaskHelper.NudgeServiceTaskTime(null, CTCMessageSenderServiceTask.Code, held.AddMinutes(10));
				Logger.LogWarning(System.FormattableString.Invariant($"A valid access token was not found for message {msg.EM_MessageNum}. Message will be held back until {msg.EM_HeldUntilDate}"));
			}
		}
	}
}
