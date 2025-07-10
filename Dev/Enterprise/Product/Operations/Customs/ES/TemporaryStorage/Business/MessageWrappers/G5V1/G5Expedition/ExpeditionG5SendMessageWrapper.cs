using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class ExpeditionG5SendMessageWrapper : G5CommonSendMessageWrapper, IExpeditionG5MessageDataProvider
	{
		public ExpeditionG5SendMessageWrapper(TemporaryStorageHeader tempHeader, ICertificateProvider certificateData) : base(tempHeader, certificateData)
		{
		}
	}
}
