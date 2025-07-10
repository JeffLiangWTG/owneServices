using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management.UnsupportedProcessing
{
	class InvalidMessageProcessor : IDataMessageProcessor, IMessageKeyProvider
	{
		public MessageKeyProviderResult GetKeysForBlockingParallelImport(IUniversalObjectFactory factory, IEDIMessage message, IXmlSessionTracker logger, ITopLevelDataObject topLevelDataObject = null, bool recordBillingInformation = true, ICodeMappingManager codeMapper = null)
		{
			logger.LogBoth(LogType.Warning, Res.GetString("0F7507CA-D654-477F-B923-246B646B7145", "Invalid Message Sub Type [{0}]. Please check the message before trying again.", message.EM_MessageSubType));
			return new MessageKeyProviderResult(MessageStatus.Rejected);
		}

		public MessageStatus Process(IUniversalObjectFactory factory, IEDIMessage message, IXmlSessionTracker logger, ITopLevelDataObject topLevelDataObject = null, bool recordBillingInformation = true, ICodeMappingManager codeMapper = null)
		{
			logger.LogBoth(LogType.Warning, Res.GetString("0F7507CA-D654-477F-B923-246B646B7145", "Invalid Message Sub Type [{0}]. Please check the message before trying again.", message.EM_MessageSubType));
			return MessageStatus.Rejected;
		}
	}
}
