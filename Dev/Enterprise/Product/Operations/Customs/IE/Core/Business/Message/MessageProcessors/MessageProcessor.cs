using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public abstract class MessageProcessor<TEDIMessage, TDataProvider> : BranchCustomsApplicationTypeMessageProcessor
		where TEDIMessage : InboundEDIMessage
	{
		protected MessageProcessor(LoggingInformation logger, Type xmlObjectType)
			: base(logger)
		{
			this.xmlObjectType = Argument.NotNull(xmlObjectType, nameof(xmlObjectType));
		}

		protected readonly Type xmlObjectType;

		protected virtual bool MustHaveLinkedObject => true;

		protected sealed override void PreProcessMessageCore(Enterprise.Messaging.Business.EDIMessage baseMessage)
		{
			var message = (TEDIMessage)baseMessage;
			ReplaceXMLElementIfNeeded(message);
			if (message.EM_Status == EDIMessage.Status.Queued)
			{
				var linkedObject = GetLinkedObject(message.Factory, message);

				var messageStatus = EDIMessage.Status.PreProcessedOK;

				if (linkedObject == null)
				{
					if (MustHaveLinkedObject)
					{
						Logger.LogError(Res.GetString("F69832D1-BF93-408A-BD01-193F96F50CE4", "Unable to find a linked business object for message (Number:{0}, Type:{1}); message status set to ERROR.", message.EM_MessageNum, message.EM_MessageType));
						messageStatus = EDIMessage.Status.Error;
					}
				}
				else
				{
					message.EM_LinkedObject = linkedObject;
					var branchPk = GetBranchPk(linkedObject);
					if (branchPk.IsValid)
					{
						message.EM_GB = branchPk;
					}
				}
				message.EM_Status = messageStatus;
			}
		}

		protected sealed override void ProcessMessageCore(Enterprise.Messaging.Business.EDIMessage baseMessage)
		{
			var message = (TEDIMessage)baseMessage;
			ReplaceXMLElementIfNeeded(message);
			if (message.EM_Status == EDIMessage.Status.PreProcessedOK)
			{
				var provider = GetDataProvider(message);
				if (provider != null)
				{
					var interpreterType = MessageInterpreterType;
					if (interpreterType != null && interpreterType.IsSubclassOfRawGeneric(typeof(InboundMessageInterpreter<>)) && !message.EM_MessageText.IsEmpty && message.EM_LinkedObject != null)
					{
						var interpretater = (InboundMessageInterpreter<TDataProvider>)Activator.CreateInstance(interpreterType, message, provider);
						message.EM_MessageInterpretation = interpretater.GetInterpretation();
					}

					ProcessMessageCore(message.Factory, message, provider);
					message.EM_Status = EDIMessage.Status.ProcessedOK;
				}
			}
		}

		protected virtual Type MessageInterpreterType => null;

		protected abstract BusinessObject GetLinkedObject(BusinessObjectFactory factory, TEDIMessage message);

		protected abstract ZGuid GetBranchPk(BusinessObject linkedObject);

		protected abstract void ProcessMessageCore(BusinessObjectFactory factory, TEDIMessage message, TDataProvider provider);

		protected internal TDataProvider GetDataProvider(TEDIMessage message, bool withSchemaValidations = false) => message.GetDataProvider<TDataProvider>(xmlObjectType, () => message.EM_Status = EDIMessage.Status.Failed, withSchemaValidations);

		protected EDIMessage FindOriginalOutgoingMessage(BusinessObjectFactory factory, EDIMessage incomingMessage)
		{
			var cachedData = factory.GetCachedValue("MessageProcessingOriginalOutgoingMessages" + incomingMessage.EM_ApplicationCode, () => new Dictionary<EDIMessage, EDIMessage>());
			if (!cachedData.TryGetValue(incomingMessage, out var outgoingMessage))
			{
				outgoingMessage = FindOriginalOutgoingMessageCore(factory, incomingMessage);
				cachedData.Add(incomingMessage, outgoingMessage);
			}
			return outgoingMessage;
		}

		protected virtual EDIMessage FindOriginalOutgoingMessageCore(BusinessObjectFactory factory, EDIMessage incomingMessage) => InboundEDIMessage.GetOriginalMessage(factory, incomingMessage.EM_ApplicationCode, incomingMessage.EM_ApplicationReference) as EDIMessage;

		protected virtual void ReplaceXMLElementIfNeeded(EDIMessage message)
		{
		}
	}
}
