using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class ReceptionG5SendMessageWrapper : G5CommonSendMessageWrapper, IReceptionG5MessageDataProvider
	{
		public ReceptionG5SendMessageWrapper(TemporaryStorageHeader tempHeader, ICertificateProvider certificateData) : base(tempHeader, certificateData)
		{
		}

		public ZString MRN => tempHeader.MRN;
	}
}
