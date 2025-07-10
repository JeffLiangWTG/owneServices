using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ImportDocCas44PendV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business
{
	public class Box44ImportResponseMessageProcessor : ImportGenericResponseMessageProcessor<ImportDocCas44PendV1Sal>
	{
		public Box44ImportResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Box44 Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameImportDocCas44PendV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.Box44Documents };

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(ImportDocCas44PendV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
			=> new Box44ImportMessagePrettyFormatter(response, entryHeader);

		protected override ZString ProcessAcceptedDeclaration(ImportDocCas44PendV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			SetCircuit(response, entryHeader);
			var entryStatus = GetNewEntryStatus(response, entryHeader);
			SetAcceptedDeclarationData(response, entryHeader, entryStatus);
			SetEntryStatus(response, entryHeader, message, entryStatus);
			TriggerDocument031CaptureInGreenCircuitWithNoCsvClearance(entryHeader, message);

			ResetGuaranteesAmountAndAddTransactions(response, entryHeader);

			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				entryLine.ProcessBox44ImportEntryLineSupportingDocuments();
			}

			return ZString.Empty;
		}

		void SetEntryStatus(ImportDocCas44PendV1Sal response, CusEntryHeader entryHeader, EDIMessage message, ZString entryStatus)
		{
			SetReleaseData(response, entryHeader);

			entryHeader.CH_EntryStatus = entryStatus;

			SetCSVClearance(response, entryHeader, message);
		}

		ZString GetNewEntryStatus(ImportDocCas44PendV1Sal response, CusEntryHeader entryHeader)
		{
			var hasCsvClearance = response.LevanteDelDua;
			if (hasCsvClearance.Equals(IndicadorSiNoTd.S))
			{
				var entrySubStyleIsBOrCOrZ = entryHeader.EntryInstruction?.IsSubStyleBOrCOrZ ?? false;
				return entrySubStyleIsBOrCOrZ ? EntryStatusCodes.ClearedWithPendingComplementaryDeclarations : EntryStatusCodes.Cleared;
			}
			else if (entryHeader.MovementReferenceNumberEntryStatus == CircuitCodeList.Codes.YELLOW)
			{
				return EntryStatusCodes.ClearedWithPendingDocuments;
			}
			else
			{
				return EntryStatusCodes.CustomsDeclarationAccepted;
			}
		}

		void SetCSVClearance(ImportDocCas44PendV1Sal response, CusEntryHeader entryHeader, EDIMessage message)
		{
			ZString csvDelCertificadoDeImportacion = response.CsVdelCertificadoDeImportacion;

			entryHeader.ZG_CSVImportCertificate = !csvDelCertificadoDeImportacion.IsEmpty ? csvDelCertificadoDeImportacion : ZString.Empty;

			var csvClearance = response.CsVdeLevante;
			SetCSVClearanceAndTriggerDocumentRequest(entryHeader, message, csvClearance);
		}

		void SetReleaseData(ImportDocCas44PendV1Sal response, CusEntryHeader entryHeader)
		{
			ZDateTime.TryParseExact(response.FechaLevante, out var entryReleaseDate, CustomsDateTimeExtension.DateFormat);
			entryHeader.CH_EntryReleaseDate = entryReleaseDate;
		}

		void SetCircuit(ImportDocCas44PendV1Sal response, CusEntryHeader entryHeader)
		{
			if (TryGetCircuitCode(response.Circuito, response.CircuitoValueSpecified, out var entryStatus))
			{
				entryHeader.SetMovementReferenceNumberEntryStatus(entryStatus);
			}
		}

		void SetCircuitCan(IBox44Import response, CusEntryHeader entryHeader)
		{
			if (TryGetCircuitCode(response.CircuitCan, response.CircuitCanSpecified, out var circuitCan))
			{
				entryHeader.SetCircuitCan(circuitCan);
			}
		}

		void SetAcceptedDeclarationData(ImportDocCas44PendV1Sal response, CusEntryHeader entryHeader, ZString entryStatus)
		{
			ZDateTime.TryParseExact(response.FechaAdmision, out var acceptanceDate, CustomsDateTimeExtension.DateFormat);
			SetGenericAcceptedDeclarationData(response, entryHeader, acceptanceDate, entryStatus, false);

			var limitPaymentDateCorrect = ZDateTime.TryParseExact(response.FechaLimitePago, out var limitPaymentDate, CustomsDateTimeExtension.DateFormat);
			entryHeader.ZG_LimitPaymentDate = limitPaymentDateCorrect && !limitPaymentDate.IsEmpty ? limitPaymentDate : ZDateTime.Empty;

			SetCircuitCan(response, entryHeader);

			var limitPaymentCanDateCorrect = ZDateTime.TryParseExact(response.FechaLimitePagoAtc, out var limitPaymentCanDate, CustomsDateTimeExtension.DateFormat);
			entryHeader.ZG_ATCLimitPaymentDate = limitPaymentCanDateCorrect && !limitPaymentCanDate.IsEmpty ? limitPaymentCanDate : ZDateTime.Empty;
		}

		protected override Collection<GarantiaGrNutilizadaTd> GetResponseGuaranteesCan(ImportDocCas44PendV1Sal response) => ((IBox44Import)response).GRNGuaranteesCan;

		const string XsdSchemaNameImportDocCas44PendV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming.ImportDocCas44PendV1Sal.xsd";
	}
}
