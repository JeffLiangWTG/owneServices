using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class DepartureNCTS5SendMessageWrapper : DepartureAndAmendmentNCTS5CommonSendMessageWrapper, IDepartureNCTSMessageDataProvider
	{
		public DepartureNCTS5SendMessageWrapper(NctsHeader header, ICertificateProvider certificateData, ZString messageType)
			: base(header, certificateData, messageType)
		{
		}

		public IDepartureNCTSTransitOperation TransitOperation => transitOperation ?? (transitOperation = new DepartureNCTS5TransitOperationWrapper(nctsHeader, messageType));
		DepartureNCTS5TransitOperationWrapper transitOperation;
	}
}
