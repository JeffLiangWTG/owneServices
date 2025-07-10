using CargoWise.Integration;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.DataTransfer.Native.ServiceTasks
{
	class NativeMessageProcessingManager : IMessageProcessingManager
	{
		readonly MessageProcessingManagerCore messageProcessingManagerCore;
		public NativeMessageProcessingManager(ISimpleLogger taskLogger)
		{
			this.messageProcessingManagerCore = new MessageProcessingManagerCore(
				new XmlSessionTracker(taskLogger),
				GetMessageProcessor,
				(_) => new NativeFactoryLocator());
		}

		public IXmlSessionTracker Logger => messageProcessingManagerCore.Logger;

		public IDataWritingManager OutboundSessionTracker
		{
			set => messageProcessingManagerCore.OutboundSessionTracker = value;
		}

		public IXmlSessionTracker Process(IEDIMessage message, ITopLevelDataObject topLevelDataObject = null, bool recordBillingInformation = true, ICodeMappingManager mapper = null, IDelayedTransactionManager transaction = null)
		{
			return messageProcessingManagerCore.Process(message, topLevelDataObject, recordBillingInformation, mapper, transaction);
		}

		IDataMessageProcessor GetMessageProcessor(string messageSubType)
		{
			return new NativeDataMessageProcessor(messageProcessingManagerCore.Logger);
		}

		class NativeFactoryLocator : IEDIMessageUniversalObjectFactoryLocator
		{
			public IUniversalObjectFactory GetFactory(IEDIMessage message, IXmlSessionTracker logger)
			{
				return new UniversalObjectFactory(message.Factory);
			}
		}
	}
}
