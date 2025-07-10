using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AmendmentAESSendMessageWrapper : DeclarationAESCommonSendMessageWrapper, IAmendmentAESMessageDataProvider
	{
		public AmendmentAESSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData, ZString securityCode, ZString messageSubType) : base(cusEntryHeader, certificateData, messageSubType)
		{
			this.securityCode = securityCode;
		}
		readonly string securityCode;

		public IAmendmentAESExportOperation ExportOperation => exportOperation ?? (exportOperation = new AmendmentAESExportOperationWrapper(entryHeader, securityCode));
		AmendmentAESExportOperationWrapper exportOperation;
	}
}
