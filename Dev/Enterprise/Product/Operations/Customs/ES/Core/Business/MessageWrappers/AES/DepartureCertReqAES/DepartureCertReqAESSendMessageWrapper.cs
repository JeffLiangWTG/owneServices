using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DepartureCertReqAESSendMessageWrapper : AESCommonSendMessageWrapper, IDepartureCertReqAESMessageDataProvider
	{
		public DepartureCertReqAESSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
		}

		public IAESCommonExportOperationMRN ExportOperation => exportOperation ?? (exportOperation = new AESCommonExportOperationMRNWrapper(entryHeader));
		AESCommonExportOperationMRNWrapper exportOperation;
	}
}
