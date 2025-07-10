using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.MessageProcessors
{
	public abstract class ApplicationTypeMessageProcessor
	{
		protected ApplicationTypeMessageProcessor(LoggingInformation logger)
		{
			this.Logger = logger;
		}

		public string MessageFriendlyName => MessageFriendlyNameCore;

		protected abstract string MessageFriendlyNameCore { get; }

		public string ApplicationCode => ApplicationCodeCore;

		protected abstract string ApplicationCodeCore { get; }

		public void ProcessMessage(EDIMessage message) => ProcessMessageCore(message);

		public void PreProcessMessage(EDIMessage message) => PreProcessMessageCore(message);

		protected virtual void PreProcessMessageCore(EDIMessage message)
		{
			message.EM_Status = EDIMessage.Status.PreProcessedOK;
		}

		public void PostProcessOnException(EDIMessage message) => PostProcessOnExceptionCore(message);

		protected virtual void PostProcessOnExceptionCore(EDIMessage message) { }

		public bool RequiresPreProcessing => RequiresPreProcessingCore;

		public IReadOnlyList<ZString> MessageTypesToInclude => MessageTypesToIncludeCore;

		public IReadOnlyList<ZString> MessageTypesToExclude => MessageTypesToExcludeCore;

		public IReadOnlyList<ZString> MessageSubTypesToInclude => MessageSubTypesToIncludeCore;

		public ZQuery MessageFilter
		{
			get
			{
				var result = MessageFilterCore;
				if (MessageTypesToInclude.Count > 0)
				{
					result.AddToFilter(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, MessageTypesToInclude);
				}

				if (MessageTypesToExclude.Count > 0)
				{
					result.AddToFilter(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.NotEqual, MessageTypesToExclude);
				}

				if (MessageSubTypesToInclude.Count > 0)
				{
					result.AddToFilter(EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.Equal, MessageSubTypesToInclude);
				}

				return result;
			}
		}

		protected abstract void ProcessMessageCore(EDIMessage message);

		protected virtual IReadOnlyList<ZString> MessageTypesToIncludeCore => System.Array.Empty<ZString>();

		protected virtual bool RequiresPreProcessingCore => false;

		protected virtual IReadOnlyList<ZString> MessageTypesToExcludeCore => System.Array.Empty<ZString>();

		protected virtual IReadOnlyList<ZString> MessageSubTypesToIncludeCore => System.Array.Empty<ZString>();

		protected virtual ZQuery MessageFilterCore
		{
			get { return new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCode); }
		}

		protected readonly LoggingInformation Logger;

		protected internal virtual void SetHeldUntilDate(EDIMessage message, IEnumerable<EDIMessage> unprocessedMessages)
		{
			message.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(2);
		}
	}
}
