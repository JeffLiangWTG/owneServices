using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Export.NotifPreDUAV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public class InboxNotificationExportResponseMessageProcessor : XMLResponseMessageProcessor<NotifPreDuav1Sal, IMessagePrettyFormatter>
	{
		public InboxNotificationExportResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Export inbox Notification Declaration Message Processor";
		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.InBoxNotificationForExport };

		protected override ZBool IsInboxDeclaration => true;

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameNotifPreDUAV1Sal;

		protected override ZString AcceptedResponseCode => InboxNotifExportResponseTypeCodeList.Codes.Admitted;

		protected override CusEntryHeader FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage)
		{
			return MessageProcessorHelper.GetRelevantBusinessObjectFromMRNCode<NotifPreDuav1Sal>(message, XsdSchemaEmbeddedResourceName, sentBusinessObjects);
		}

		protected override ZString ProcessAcceptedDeclaration(NotifPreDuav1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			var acceptanceResponse = response.Admision;

			entryHeader.ZG_CSVT2L = acceptanceResponse.CsvT2Lf;

			var circuit = GetCircuitCodeFromText(acceptanceResponse.CodigoRespuestaAeat);
			ZDateTime.TryParseExact(acceptanceResponse.FechaAdmision, out var acceptanceDate, CustomsDateTimeExtension.DateFormat);
			SetMovementReferenceNumber(entryHeader, acceptanceDate, circuit);

			ZDateTime.TryParseExact(acceptanceResponse.FechaHoraLevante, out var releaseDate, CustomsDateTimeExtension.DateTimeFormatLong);
			entryHeader.CH_EntryReleaseDate = releaseDate;

			entryHeader.SetCircuitCan(GetCircuitCodeFromText(acceptanceResponse.CodigoRespuestaAtc));
			SetClearanceResult(acceptanceResponse, entryHeader);
			SetEADPrintProcedure(acceptanceResponse, entryHeader);
			SetEntryStatus(acceptanceResponse, entryHeader, message);

			return ZString.Empty;
		}

		protected override void ProcessRejectedDeclaration(NotifPreDuav1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			ZString entryStatus = response.TipoRespuesta switch
			{
				InboxNotifExportResponseTypeCodeList.Codes.Rejected => EntryStatusCodes.PreDeclarationAccepted,
				InboxNotifExportResponseTypeCodeList.Codes.Cancelled => EntryStatusCodes.Cancelled,
				_ => ZString.Empty
			};

			if (!entryStatus.IsEmpty)
			{
				entryHeader.CH_EntryStatus = entryStatus;
			}
		}

		void SetEntryStatus(NotifPreDuav1SalAdmision acceptanceResponse, CusEntryHeader entryHeader, EDIMessage message)
		{
			ZString csvClearance = acceptanceResponse.CsvLevante;
			SetCSVClearanceAndTriggerDocumentRequest(entryHeader, message, csvClearance);

			entryHeader.CH_EntryStatus = csvClearance.IsEmpty
				? EntryStatusCodes.CustomsDeclarationAccepted
				: entryHeader.EntryInstruction?.IsSubStyleBOrC == true
					? EntryStatusCodes.ClearedWithPendingComplementaryDeclarations
					: EntryStatusCodes.Cleared;
		}

		void SetClearanceResult(NotifPreDuav1SalAdmision acceptanceResponse, CusEntryHeader entryHeader)
		{
			if (acceptanceResponse.CriterioDespacho
				is ClearanceResultCodeList.Codes.A1
				or ClearanceResultCodeList.Codes.A2)
			{
				entryHeader.ZG_ClearanceResult = acceptanceResponse.CriterioDespacho;
			}
		}

		void SetEADPrintProcedure(NotifPreDuav1SalAdmision acceptanceResponse, CusEntryHeader entryHeader)
		{
			if (acceptanceResponse.ImpresionDae
				is EADPrintProcedureCodeList.Codes._0NoEADPrint
				or EADPrintProcedureCodeList.Codes._1EADPrintedByCustomsAuthorities
				or EADPrintProcedureCodeList.Codes._2EADCanBePrintedByDeclarant)
			{
				entryHeader.EUH_EADPrintProcedure = acceptanceResponse.ImpresionDae;
			}
		}

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(NotifPreDuav1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new InboxNotificationExportMessagePrettyFormatter(response);

		protected override CommonDocumentRequest<CusEntryHeader> GetNewDocumentRequest(CusEntryHeader businessObject, ZString certName, EDIMessage message) => new ExportDocumentRequest(businessObject, certName);

		const string XsdSchemaNameNotifPreDUAV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.Export.Incoming.NotifPreDUAV1Sal.xsd";
	}
}
