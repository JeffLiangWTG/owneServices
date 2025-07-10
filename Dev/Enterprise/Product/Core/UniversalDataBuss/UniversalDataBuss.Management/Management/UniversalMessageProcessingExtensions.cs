using System;
using CargoWise.Integration;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management
{
	public static class UniversalMessageProcessingExtensions
	{
		public static MessageStatus ProcessUniversalMessage(
			this IEDIMessage message,
			IUniversalObjectFactory universalObjectFactory,
			IXmlSessionTracker xmlSessionTracker,
			ITopLevelDataObject dataObject,
			ITopLevelDataObjectProcessor dataMessageProcessor,
			bool recordBillingInformation,
			ICodeMappingManager codeMapper,
			IDelayedTransactionManager transaction)
		{
			return MessageProcessingManagerCore.Process(
				universalObjectFactory,
				message,
				xmlSessionTracker,
				dataObject,
				dataMessageProcessor,
				recordBillingInformation,
				codeMapper,
				transaction);
		}

		public static IXmlSessionTracker ProcessUniversalMessage(
			this IEDIMessage message,
			IUniversalObjectFactory universalObjectFactory,
			IXmlSessionTracker xmlSessionTracker)
		{
			return new UniversalMessageProcessingManager(universalObjectFactory, xmlSessionTracker).Process(message);
		}

		public static IXmlSessionTracker ProcessUniversalMessage(
			this IEDIMessage message,
			IUniversalObjectFactory universalObjectFactory,
			ISimpleLogger logger)
		{
			return new UniversalMessageProcessingManager(universalObjectFactory, logger).Process(message);
		}

		public static IDataMessageFactory GetUniversalDataMessageFactory(this IEDIMessage message)
		{
			var messageSubType = message.EM_MessageSubType;
			var messageProcessor = GetMessageProcessor(messageSubType);
			var userContextExtractor = GetUserContextExtractor(messageSubType);
			switch (message.EM_MessageSubType)
			{
				case EDIMessageSubTypeList.Codes.XmlUniversalEvent:
					return new DataMessageFactory(
						messageProcessor,
						new TopLevelDataObjectFactory<DataObjects.Universal.Event>(),
						new EventProcessing.UniversalEventMessageProcessor(),
						userContextExtractor,
						new UserContextScopeManager());
				case EDIMessageSubTypeList.Codes.XmlUniversalShipment:
					return new DataMessageFactory(
						messageProcessor,
						new TopLevelDataObjectFactory<DataObjects.Universal.Shipment>(),
						new ShipmentProcessing.UniversalShipmentMessageProcessor(),
						userContextExtractor,
						new UserContextScopeManager());
				case EDIMessageSubTypeList.Codes.XmlUniversalSchedule:
					return new DataMessageFactory(
						messageProcessor,
						new TopLevelDataObjectFactory<DataObjects.Universal.Schedule>(),
						new ScheduleProcessing.UniversalScheduleMessageProcessor(),
						userContextExtractor,
						new UserContextScopeManager());
				case EDIMessageSubTypeList.Codes.XmlUniversalTransaction:
					return new DataMessageFactory(
						messageProcessor,
						new TopLevelDataObjectFactory<DataObjects.Accounting.TransactionInfo>(),
						new TransactionProcessing.UniversalTransactionMessageProcessor(),
						userContextExtractor,
						new UserContextScopeManager());
				case EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch:
					return new DataMessageFactory(
						messageProcessor,
						new TopLevelDataObjectFactory<DataObjects.Accounting.TransactionBatch>(),
						new TransactionProcessing.UniversalTransactionBatchMessageProcessor(),
						userContextExtractor,
						new UserContextScopeManager());
				case EDIMessageSubTypeList.Codes.XmlUniversalActivity:
					return new DataMessageFactory(
						messageProcessor,
						new TopLevelDataObjectFactory<DataObjects.Universal.Activity>(),
						new ActivityProcessing.UniversalActivityMessageProcessor(),
						userContextExtractor,
						new UserContextScopeManager());
				case EDIMessageSubTypeList.Codes.XmlUniversalDocumentRequest:
				case EDIMessageSubTypeList.Codes.XmlUniversalShipmentRequest:
				case EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatchRequest:
				case EDIMessageSubTypeList.Codes.XmlUniversalActivityRequest:
					throw new UnsupportedMessageTypeException();
				default:
					throw new InvalidMessageTypeException();
			}
		}

		public static bool TryGetUserContext(this IEDIMessage message, ITopLevelDataObject topLevelDataObject, IXmlSessionTracker logger, out IUserContext userContext)
		{
			if (!GetUserContextExtractor(message.EM_MessageSubType).TryGetUserContext(message, topLevelDataObject, logger, out userContext))
			{
				MessageProcessingManagerCore.UpdateMessage(message, MessageStatus.Rejected, logger);
				return false;
			}
			return true;
		}

		public static IDisposable SetUserContext(IUserContext userContext)
		{
			return new UserContextScopeManager().EnterUserContext(userContext);
		}

		public static bool ShouldSaveResultsFromUniversalXmlProcessing(this IEDIMessage message)
		{
			return message.EM_Status == EDIMessageStatusList.Codes.ProcessedOK || message.EM_Status == EDIMessageStatusList.Codes.Warning || message.EM_Status == EDIMessageStatusList.Codes.Linked;
		}

		internal static IDataMessageProcessor GetMessageProcessor(string messageSubType)
		{
			var userContextExtractor = GetUserContextExtractor(messageSubType);
			IDataMessageProcessor processor;
			switch (messageSubType)
			{
				case EDIMessageSubTypeList.Codes.XmlUniversalEvent:
					processor = new DataMessageProcessor<DataObjects.Universal.Event>(
						new EventProcessing.UniversalEventMessageProcessor(),
						new TopLevelDataObjectFactory<DataObjects.Universal.Event>(),
						new UserContextScopeManager(),
						userContextExtractor);
					break;
				case EDIMessageSubTypeList.Codes.XmlUniversalShipment:
					processor = new DataMessageProcessor<DataObjects.Universal.Shipment>(
						new ShipmentProcessing.UniversalShipmentMessageProcessor(),
						new TopLevelDataObjectFactory<DataObjects.Universal.Shipment>(),
						new UserContextScopeManager(),
						userContextExtractor);
					break;
				case EDIMessageSubTypeList.Codes.XmlUniversalSchedule:
					processor = new DataMessageProcessor<DataObjects.Universal.Schedule>(
						new ScheduleProcessing.UniversalScheduleMessageProcessor(),
						new TopLevelDataObjectFactory<DataObjects.Universal.Schedule>(),
						new UserContextScopeManager(),
						userContextExtractor);
					break;
				case EDIMessageSubTypeList.Codes.XmlUniversalTransaction:
					processor = new DataMessageProcessor<DataObjects.Accounting.TransactionInfo>(
						new TransactionProcessing.UniversalTransactionMessageProcessor(),
						new TopLevelDataObjectFactory<DataObjects.Accounting.TransactionInfo>(),
						new UserContextScopeManager(),
						userContextExtractor);
					break;
				case EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch:
					processor = new DataMessageProcessor<DataObjects.Accounting.TransactionBatch>(
						new TransactionProcessing.UniversalTransactionBatchMessageProcessor(),
						new TopLevelDataObjectFactory<DataObjects.Accounting.TransactionBatch>(),
						new UserContextScopeManager(),
						userContextExtractor);
					break;
				case EDIMessageSubTypeList.Codes.XmlUniversalActivity:
					processor = new DataMessageProcessor<DataObjects.Universal.Activity>(
						new ActivityProcessing.UniversalActivityMessageProcessor(),
						new TopLevelDataObjectFactory<DataObjects.Universal.Activity>(),
						new UserContextScopeManager(),
						userContextExtractor);
					break;
				case EDIMessageSubTypeList.Codes.XmlUniversalDocumentRequest:
				case EDIMessageSubTypeList.Codes.XmlUniversalShipmentRequest:
				case EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatchRequest:
				case EDIMessageSubTypeList.Codes.XmlUniversalActivityRequest:
					processor = new UnsupportedProcessing.UnsupportedMessageProcessor();
					break;
				default:
					processor = new UnsupportedProcessing.InvalidMessageProcessor();
					break;
			}

			return processor;
		}

		static IUserContextExtractor GetUserContextExtractor(string messageSubType)
		{
			switch (messageSubType)
			{
				case EDIMessageSubTypeList.Codes.XmlUniversalEvent:
					return new UserContextExtractor(new UniversalEventRecipientBranchLocator(), false);
				case EDIMessageSubTypeList.Codes.XmlUniversalShipment:
					return new UserContextExtractor(new ShipmentProcessing.UniversalShipmentBranchLocator(), true);
				default:
					return new UserContextExtractor(new RecipientBranchLocator(), true);
			}
		}

		class DataMessageFactory : IDataMessageFactory
		{
			public DataMessageFactory(
				IDataMessageProcessor dataMessageProcessor,
				ITopLevelDataObjectFactory topLevelDataObjectFactory,
				ITopLevelDataObjectProcessor topLevelDataObjectProcessor,
				IUserContextExtractor userContextExtractor,
				IUserContextScopeManager userContextScopeManager
			)
			{
				DataMessageProcessor = dataMessageProcessor;
				TopLevelDataObjectFactory = topLevelDataObjectFactory;
				TopLevelDataObjectProcessor = topLevelDataObjectProcessor;
				UserContextExtractor = userContextExtractor;
				UserContextScopeManager = userContextScopeManager;
			}

			public IDataMessageProcessor DataMessageProcessor { get; }
			public ITopLevelDataObjectFactory TopLevelDataObjectFactory { get; }
			public ITopLevelDataObjectProcessor TopLevelDataObjectProcessor { get; }
			public IUserContextExtractor UserContextExtractor { get; }
			public IUserContextScopeManager UserContextScopeManager { get; }
		}
	}
}
