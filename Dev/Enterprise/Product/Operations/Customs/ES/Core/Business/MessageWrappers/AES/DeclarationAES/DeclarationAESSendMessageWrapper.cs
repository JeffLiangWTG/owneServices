using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationAESSendMessageWrapper : DeclarationAESCommonSendMessageWrapper, IDeclarationAESMessageDataProvider
	{
		public DeclarationAESSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData, ZString securityCode, ZString messageType, ZString messageSubType) : base(cusEntryHeader, certificateData, messageSubType)
		{
			this.securityCode = securityCode;
			this.messageType = Argument.NotNullOrEmpty(messageType, nameof(messageType));
		}
		readonly string securityCode;
		readonly string messageType;

		public IDeclarationAESExportOperation ExportOperation => exportOperation ?? (exportOperation = new DeclarationAESExportOperationWrapper(entryHeader, securityCode, messageType, IsComplementaryCWithMRN));
		DeclarationAESExportOperationWrapper exportOperation;
	}
}
