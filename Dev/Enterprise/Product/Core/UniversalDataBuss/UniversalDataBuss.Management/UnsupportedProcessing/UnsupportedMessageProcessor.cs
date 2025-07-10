using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management.UnsupportedProcessing
{
	class UnsupportedMessageProcessor : IDataMessageProcessor, IMessageKeyProvider
	{
		public MessageKeyProviderResult GetKeysForBlockingParallelImport(IUniversalObjectFactory factory, IEDIMessage message, IXmlSessionTracker logger, ITopLevelDataObject topLevelDataObject = null, bool recordBillingInformation = true, ICodeMappingManager codeMapper = null)
		{
			logger.LogBoth(LogType.Warning, Res.GetString("725B1D01-C462-4465-B661-C1463239ACCB", "Cannot process Message Sub Type [{0}]. Messages of Sub Type [{0}] can only be processed using eAdaptor HTTP+XML", message.EM_MessageSubType));
			return new MessageKeyProviderResult(MessageStatus.Rejected);
		}

		public MessageStatus Process(IUniversalObjectFactory factory, IEDIMessage message, IXmlSessionTracker logger, ITopLevelDataObject topLevelDataObject = null, bool recordBillingInformation = true, ICodeMappingManager codeMapper = null)
		{
			logger.LogBoth(LogType.Warning, Res.GetString("725B1D01-C462-4465-B661-C1463239ACCB", "Cannot process Message Sub Type [{0}]. Messages of Sub Type [{0}] can only be processed using eAdaptor HTTP+XML", message.EM_MessageSubType));
			return MessageStatus.Rejected;
		}
	}
}
