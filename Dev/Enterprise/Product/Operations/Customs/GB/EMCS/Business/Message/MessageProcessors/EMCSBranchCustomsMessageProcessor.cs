using System;
using System.IO;
using System.Xml.Schema;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public abstract class EMCSBranchCustomsMessageProcessor<TEDIMessage, TDataProvider> : BranchCustomsApplicationTypeMessageProcessor where TEDIMessage : EMCSInboundEDIMessage
	{
		protected EMCSBranchCustomsMessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger)
		{
			this.xmlObjectType = Argument.NotNull(xmlObjectType, nameof(xmlObjectType));
		}

		protected readonly Type xmlObjectType;
		protected virtual bool MustHaveLinkedObject => true;
		protected virtual Type MessageInterpreterType => null;

		protected sealed override void PreProcessMessageCore(EDIMessage baseMessage)
		{
			var message = (TEDIMessage)baseMessage;
			if (message.EM_Status == EDIMessage.Status.Queued)
			{
				var linkedObject = GetLinkedObject(message.Factory, message);
				var messageStatus = EDIMessage.Status.PreProcessedOK;

				if (MustHaveLinkedObject && linkedObject == null)
				{
					Logger.LogError(Res.GetString("D1920C84-A6DD-4CDC-8452-11A34AAB189F", "Unable to find a linked business object for message (Number:{0}, Type:{1}); message status set to ERROR.", message.EM_MessageNum, message.EM_MessageType));
					messageStatus = EDIMessage.Status.Error;
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

		protected sealed override void ProcessMessageCore(EDIMessage baseMessage)
		{
			var message = (TEDIMessage)baseMessage;
			if (message.EM_Status == EDIMessage.Status.PreProcessedOK)
			{
				var provider = GetDataProvider(message);
				if (provider != null)
				{
					var messageInterpreterType = MessageInterpreterType;
					if (messageInterpreterType != null && messageInterpreterType.IsSubclassOfRawGeneric(typeof(EMCSInboundMessageInterpreter<>)) && !message.EM_MessageText.IsEmpty)
					{
						var inboundMessageInterpreter = (EMCSInboundMessageInterpreter<TDataProvider>)Activator.CreateInstance(messageInterpreterType, message, provider);
						message.EM_MessageInterpretation = inboundMessageInterpreter.GetInterpretation();
					}

					ProcessMessageCore(message.Factory, message, provider);
					message.EM_Status = EDIMessage.Status.ProcessedOK;
					if (message.EM_LinkedObject is EMCSJobDeclaration declaration)
					{
						SetDeclarationConcurrencyPolicy(declaration);
					}
				}
			}
		}

		protected abstract void ProcessMessageCore(BusinessObjectFactory factory, TEDIMessage message, TDataProvider provider);

		protected virtual void SetDeclarationConcurrencyPolicy(EMCSJobDeclaration declaration)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(declaration, nameof(declaration.JE_MessageStatus), ConcurrencyPolicy.Ignore);
		}

		protected EDIMessage FindOriginalOutgoingMessage(BusinessObjectFactory factory, ZString transactionId)
		{
			if (transactionId.IsEmpty)
			{
				return null;
			}

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationReference, transactionId);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCode);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Acknowledged);
			query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + OrderByClause.Descending;
			return factory.LoadTop1<EDIMessage>(query);
		}

		protected abstract ZGuid GetBranchPk(BusinessObject linkedObject);

		protected abstract BusinessObject GetLinkedObject(BusinessObjectFactory factory, TEDIMessage message);

		protected internal virtual TDataProvider GetDataProvider(TEDIMessage message, bool withSchemaValidation = false)
		{
			TDataProvider dataProvider = default;
			try
			{
				var customsData = EMCSCustomsData(message, withSchemaValidation);
				var xmlObject = customsData?.GetType().GetProperty(nameof(EMCSXmlObjectProvider<object>.Message))?.GetValue(customsData);
				if (xmlObject != null)
				{
					dataProvider = (TDataProvider)Activator.CreateInstance(DecideDataProviderType<TDataProvider>(xmlObjectType), xmlObject);
				}
			}
			catch (Exception ex) when (ex.InnerException is XmlSchemaValidationException || ex.InnerException is InvalidOperationException)
			{
				message.EM_Status = EDIMessage.Status.Failed;
			}

			return dataProvider;
		}

		protected virtual Type DecideDataProviderType<TTDataProvider>(Type xmlObjectType)
		{
			return typeof(TDataProvider);
		}

		protected internal object EMCSCustomsData(EDIMessage message, bool withSchemaValidation)
		{
			return message.Factory.GetCachedValue(string.Join("|", "EMCSGBCustomsData", message.PK, message.EM_ApplicationReference), delegate
			{
				object result = null;
				using (TextReader textReader = message.GetEM_MessageTextReader())
				{
					var type = typeof(EMCSXmlObjectProvider<>).MakeGenericType(xmlObjectType);
					result = Activator.CreateInstance(type, textReader, withSchemaValidation);
				}
				return result;
			});
		}
	}

	public sealed class EMCSXmlObjectProvider<T>
	{
		public EMCSXmlObjectProvider(TextReader textReader, bool withSchemaValidation)
		{
			xmlMessage = EMCSXmlObjectSerializer.Deserialize<T>(textReader, withSchemaValidation);
		}

		public T Message => xmlMessage;
		readonly T xmlMessage;
	}
}
