using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class AmendmentNCTS5SendMessageWrapper : DepartureAndAmendmentNCTS5CommonSendMessageWrapper, IAmendmentNCTSMessageDataProvider
	{
		public AmendmentNCTS5SendMessageWrapper(NctsHeader header, ICertificateProvider certificateData, ZString messageType)
				: base(header, certificateData, messageType)
		{
		}

		public IAmendmentNCTSTransitOperation TransitOperation => transitOperation ?? (transitOperation = new AmendmentNCTS5TransitOperationWrapper(nctsHeader, messageType));
		AmendmentNCTS5TransitOperationWrapper transitOperation;
	}
}
