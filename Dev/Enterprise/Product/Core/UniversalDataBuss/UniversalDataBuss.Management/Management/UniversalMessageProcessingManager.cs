using CargoWise.Common;
using CargoWise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management
{
	public sealed class UniversalMessageProcessingManager : IMessageProcessingManager
	{
		readonly IUniversalObjectFactory universalObjectFactory;
		readonly XmlSessionTracker xmlSessionTracker;

		public IXmlSessionTracker Logger => xmlSessionTracker;

		public IDataWritingManager OutboundSessionTracker { set => xmlSessionTracker.OutboundSessionTracker = value; }

		public UniversalMessageProcessingManager(IUniversalObjectFactory universalObjectFactory, IXmlSessionTracker xmlSessionTracker)
		{
			Argument.NotNull(xmlSessionTracker, nameof(xmlSessionTracker));
			this.xmlSessionTracker = (XmlSessionTracker)xmlSessionTracker;
			this.universalObjectFactory = universalObjectFactory;
		}

		public UniversalMessageProcessingManager(IUniversalObjectFactory universalObjectFactory, ISimpleLogger taskLogger)
			: this(universalObjectFactory, new XmlSessionTracker(taskLogger))
		{
		}

		public UniversalMessageProcessingManager(IXmlSessionTracker xmlSessionTracker)
			: this(null, xmlSessionTracker)
		{
		}

		public UniversalMessageProcessingManager(ISimpleLogger taskLogger)
			: this(null, new XmlSessionTracker(taskLogger))
		{
		}

		public IXmlSessionTracker Process(
			IEDIMessage message,
			ITopLevelDataObject topLevelDataObject = null,
			bool recordBillingInformation = true,
			ICodeMappingManager mapper = null,
			IDelayedTransactionManager transaction = null)
		{
			return new MessageProcessingManagerCore(
					this.xmlSessionTracker,
					UniversalMessageProcessingExtensions.GetMessageProcessor,
					GetFactoryLocator)
					.Process(message, topLevelDataObject, recordBillingInformation, mapper, transaction);
		}

		public MessageKeyProviderResult GetKeysForBlockingParallelImport(IEDIMessage message)
		{
			return ((IMessageKeyProvider)UniversalMessageProcessingExtensions.GetMessageProcessor(message.EM_MessageSubType)).GetKeysForBlockingParallelImport(universalObjectFactory ?? new UniversalObjectFactory(), message, xmlSessionTracker);
		}

		public static void LogFailedMessage(
			IEDIMessage isolatedMessage,
			MessageStatus status,
			IXmlSessionTracker logger)
		{
			MessageProcessingManagerCore.UpdateMessage(isolatedMessage, status, (XmlSessionTracker)logger);
			MessageProcessingManagerCore.FinalizeMessage(isolatedMessage, (XmlSessionTracker)logger);
		}

		IEDIMessageUniversalObjectFactoryLocator GetFactoryLocator(string messageSubType)
		{
			switch (messageSubType)
			{
				case EDIMessageSubTypeList.Codes.XmlUniversalEvent:
					return new EDIMessageUniversalObjectFactoryLocatorForUniversalEvent(universalObjectFactory);
				default:
					return new EDIMessageUniversalObjectFactoryLocator(universalObjectFactory);
			}
		}
	}
}
