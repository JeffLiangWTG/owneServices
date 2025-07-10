using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class ExpAmendmentG5SendMessageWrapper : G5CommonSendMessageWrapper, IExpAmendmentG5MessageDataProvider
	{
		public ExpAmendmentG5SendMessageWrapper(TemporaryStorageHeader tempHeader, ICertificateProvider certificateData) : base(tempHeader, certificateData)
		{
		}

		public ZString MRN => tempHeader.MRN;
	}
}
