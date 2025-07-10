using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class CANPreDUAImportSendMessageWrapper : ImportCommonSendMessageWrapper, ICANPreDUAImportMessageDataProvider
	{
		public CANPreDUAImportSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
		}
	}
}
