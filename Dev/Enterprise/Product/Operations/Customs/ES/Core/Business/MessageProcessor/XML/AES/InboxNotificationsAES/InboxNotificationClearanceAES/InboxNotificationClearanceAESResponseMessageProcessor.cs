using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaLevanteExporV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public class InboxNotificationClearanceAESResponseMessageProcessor : AESCommonInboxNotificationResponseMessageProcessor<ComunicaLevanteExporV1Sal>
	{
		public InboxNotificationClearanceAESResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Inbox Notification Export Clearance Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameComunicaLevanteExporV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ExportClearanceCommunication };

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(ComunicaLevanteExporV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new InboxNotificationClearanceAESMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(ComunicaLevanteExporV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			var correctResponseData = response.DatosComunicacion;
			if (correctResponseData != null)
			{
				entryHeader.CH_EntryReleaseDate = correctResponseData.FechaLevante;
				entryHeader.ZG_CSVT2L = correctResponseData.CsvDocumentoT2L;
				var csvClearance = correctResponseData.CsvLevanteExportacion;
				SetCSVClearanceAndTriggerDocumentRequest(entryHeader, message, csvClearance);
			}

			SetEntryStatus(entryHeader);

			return ZString.Empty;
		}

		void SetEntryStatus(CusEntryHeader entryHeader)
		{
			entryHeader.CH_EntryStatus = entryHeader.EntryInstruction?.IsSubStyleBOrC == true
				? EntryStatusCodes.ClearedWithPendingComplementaryDeclarations
				: EntryStatusCodes.Cleared;
		}

		protected override CommonDocumentRequest<CusEntryHeader> GetNewDocumentRequest(CusEntryHeader businessObject, ZString certName, EDIMessage message) => new ExportAESDocumentRequest(businessObject, certName);

		const string XsdSchemaNameComunicaLevanteExporV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.ComunicaLevanteExporV1Sal.xsd";
	}
}
