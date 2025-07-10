using Enterprise.BatchProcessor;
using Enterprise.Customs.JP.Common;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;

[assembly: UniversalCustomsInterchangeUnpacker(ApplicationCodeList.Codes.JPCustoms, typeof(JPCInterchangeUnpacker))]

namespace Enterprise.Customs.JP.Common;

sealed class JPCInterchangeUnpacker : IUniversalCustomsInterchangeUnpacker
{
	public IUniversalCustomsInterchangeUnpackerResult Unpack(Messaging.Business.EDIInterchange interchange, Messaging.Business.EDIInterchange outgoingInterchange, Messaging.Business.EDIMessage outgoingMessage, LoggingInformation logger)
	{
		var messageParent = outgoingMessage?.EM_LinkedObject;
		return interchange.EI_InterchangeType == JPMessageTypes.Codes.XER
				? ErrorInterchangeHandler.CreateXERMessage(interchange, messageParent)
				: NormalInterchangeHandler.CreateNACCSMessage(interchange, messageParent, logger);
	}

	ErrorInterchangeHandler ErrorInterchangeHandler => errorInterchangeHandler ??= new ErrorInterchangeHandler(ParentFinder);
	ErrorInterchangeHandler errorInterchangeHandler;

	NormalInterchangeHandler NormalInterchangeHandler => normalInterchangeHandler ??= new NormalInterchangeHandler(ParentFinder);
	NormalInterchangeHandler normalInterchangeHandler;

	NACCSMessageParentFinder ParentFinder => parentFinder ??= new NACCSMessageParentFinder();
	NACCSMessageParentFinder parentFinder;
}
