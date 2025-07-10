using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.ExportMessageConstants;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AmendmentDUAExportSendMessageWrapper : DUAExportSendMessageWrapper
	{
		public AmendmentDUAExportSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData) { }

		protected override ZString MessageTypeCore => ExportDeclarationWrapperMessageTypeCodeList.ExportAmendmentDeclaration;

		protected override ZString LocalReferenceNumberCore => ESEDIMessage.ESExportAmendmentLocalReferenceNumberPlaceHolder;

		protected override ZString CustomsProcedureCategory5Core => entryHeader.MovementReferenceNumber;
	}
}
