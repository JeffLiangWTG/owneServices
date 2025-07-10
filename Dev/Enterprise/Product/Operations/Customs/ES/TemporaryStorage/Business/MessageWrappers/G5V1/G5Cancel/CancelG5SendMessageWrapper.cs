using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class CancelG5SendMessageWrapper : G5GenericSendMessageWrapper, IExpCancelG5MessageDataProvider
	{
		public CancelG5SendMessageWrapper(TemporaryStorageHeader tempHeader, ICertificateProvider certificateData) : base(tempHeader, certificateData)
		{
		}

		public ZString MRN => tempHeader.MRN;

		public IG5SimplifiedHeader Header => header ??= new G5SimplifiedHeaderWrapper(tempHeader);
		G5SimplifiedHeaderWrapper header;
	}
}
