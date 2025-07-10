using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static CargoWise.EventReference.Constants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business
{
	public abstract class DVDCommonResponseMessageProcessor<TResponse, TPrettyMessage> : XMLResponseMessageProcessor<TResponse, TPrettyMessage>
		where TResponse : class, ICommonServiceSegment, IResponseCode, IMRNField
		where TPrettyMessage : IMessagePrettyFormatter
	{
		protected DVDCommonResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver) : base(logger, eDocsSaver)
		{
		}

		const string TransactionCommentPrefix = "DVD ";

		protected void SetEntryStatusAndTriggerInboxRequestIfNeededCommon(EDIMessage message, string operationCode, string csvClearance, CusEntryHeader entryHeader)
		{
			switch (operationCode)
			{
				case DVDResponseOperationCodeList.Codes._0DvdAccepted:
				case DVDResponseOperationCodeList.Codes._3DvdAcceptedByPreDeclarationModification:
					SetEntryHeaderCommon(csvClearance, entryHeader);
					break;
				case DVDResponseOperationCodeList.Codes._1PreDeclarationAccepted:
				case DVDResponseOperationCodeList.Codes._2PreDeclarationModification:
					entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

					TriggerDVDInboxRequest(message, entryHeader);
					break;
				default:
					break;
			}
			LoggerTSGuarantee(entryHeader);
		}

		protected void LoggerTSGuarantee(CusEntryHeader entryHeader)
		{
			var logTypeCode = (NoResString)"TS Guarantee";
			var log = entryHeader.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.ErrorReport.Code && c.Parameters.GetValueOrDefault(EventReferenceParameters.Codes.Type) == logTypeCode).LastOrDefault();
			if (log != null)
			{
				Logger.LogError(log.Parameters.GetValueOrDefault(EventReferenceParameters.Codes.Reason));
			}
		}

		protected void SetAcceptanceAndReleaseDataAndCircuitCan(CusEntryHeader entryHeader, string admisionDate, string clearanceDate, string csvClearance, EDIMessage message, TdCircuito? circuitAEAT, TdCircuito? circuitoATC)
		{
			SetAcceptanceAndReleaseData(admisionDate, clearanceDate, csvClearance, entryHeader, message, circuitAEAT);
			if (circuitoATC != null)
			{
				entryHeader.SetCircuitCan(GetCircuitCode(circuitoATC));
			}
		}

		protected void TriggerDVDInboxRequest(EDIMessage message, CusEntryHeader entryHeader) => TriggerInboxRequest(entryHeader, message, new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2 });

		protected void SetEntryHeaderCommon(string csvClearance, CusEntryHeader entryHeader)
		{
			if (string.IsNullOrEmpty(csvClearance))
			{
				entryHeader.CH_EntryStatus = entryHeader.EntryInstruction != null && entryHeader.EntryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.Z ? EntryStatusCodes.Cleared : EntryStatusCodes.CustomsDeclarationAccepted;
			}
			else
			{
				entryHeader.CH_EntryStatus = entryHeader.EntryInstruction != null && entryHeader.EntryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.B ? EntryStatusCodes.ClearedWithPendingComplementaryDeclarations : EntryStatusCodes.Cleared;
			}
		}

		protected void SetAcceptanceAndReleaseData(string admissionDateTime, string releaseDateTime, string csvClearance, CusEntryHeader entryHeader, EDIMessage message, TdCircuito? circuit = null)
		{
			ZDateTime.TryParseExact(admissionDateTime, out var acceptanceDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);
			SetMovementReferenceNumber(entryHeader, acceptanceDate, GetCircuitCode(circuit));

			ZDateTime.TryParseExact(releaseDateTime, out var entryReleaseDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);
			entryHeader.CH_EntryReleaseDate = entryReleaseDate;

			SetCSVClearanceAndTriggerDocumentRequest(entryHeader, message, csvClearance);
		}

		protected ZString GetCircuitCode(TdCircuito? circuitCode) => GetCircuitCodeFromText(circuitCode?.ToString());

		protected void ResetGuaranteesAmountAndAddTransactions(TResponse response, CusEntryHeader entryHeader)
							=> ResetGuaranteesAmountAndAddTransactionsForAcceptedDeclaration(response, entryHeader, TransactionCommentPrefix, GetDebtAmountArrayFromResponseGuarantee);

		ZDecimal[] GetDebtAmountArrayFromResponseGuarantee(TResponse response, ZString reference)
		{
			var debtList = new List<ZDecimal>();

			var guarantees = GetGuaranteesList(response);

			var responseGuarantee = guarantees.FirstOrDefault(x => x.CBgarantiaGrn == reference);

			debtList.Add(responseGuarantee?.CBimportePotencial ?? decimal.Zero);

			return debtList.ToArray();
		}

		protected virtual List<TdGarantiaGrNutilizada> GetGuaranteesList(TResponse response) => null;

		protected sealed override CommonDocumentRequest<CusEntryHeader> GetNewDocumentRequest(CusEntryHeader businessObject, ZString certName, EDIMessage message) => new DVDDocumentRequest(businessObject, certName);

		protected sealed override Dictionary<string, string> FileNamesDict(string mrn, string oldCSVClearance, EDIMessage message) => EDocHelper.FileNamesDictDVD(mrn, oldCSVClearance);
	}
}
