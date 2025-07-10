using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Customs.Shared.MessageContracts;
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
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using CusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;
using CusEntryLineFee = Enterprise.Customs.ES.Business.Declaration.CusEntryLineFee;

namespace Enterprise.Customs.ES.Business
{
	public abstract class ImportGenericResponseMessageProcessor<TImportResponse> : XMLResponseMessageProcessor<TImportResponse, IMessagePrettyFormatter>
		where TImportResponse : class, ICommonServiceSegment, IImportCommonGeneric, IResponseCode
	{
		protected ImportGenericResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString AcceptedResponseCode => AcceptedMessageCode;

		protected virtual ZString GetEntryStatus(IImportCommon response, CusEntryHeader entryHeader, EDIMessage message, ImmutableHashSet<ZString> pdaRegisteredOperationCodes)
				=> pdaRegisteredOperationCodes.Contains(response.RegisteredOperationCode)
						? EntryStatusCodes.PreDeclarationAccepted
						: HasCSVClearance(response)
								? ShouldSetEntryStatusCLP(entryHeader) ? EntryStatusCodes.ClearedWithPendingComplementaryDeclarations : EntryStatusCodes.Cleared
								: entryHeader.MovementReferenceNumberEntryStatus == CircuitCodeList.Codes.YELLOW
										? EntryStatusCodes.ClearedWithPendingDocuments
										: EntryStatusCodes.CustomsDeclarationAccepted;

		protected ZBool HasCSVClearance(IImportCommonGeneric response) => !response.CSVClearance.IsEmpty();

		protected void SetCircuitIfNeeded(IImportCommon response, CusEntryHeader entryHeader, ImmutableHashSet<ZString> pdaRegisteredOperationCodes)
		{
			if (!pdaRegisteredOperationCodes.Contains(response.RegisteredOperationCode))
			{
				SetCircuit(response, entryHeader);
			}
		}

		protected void SetEntryStatus(IImportCommon response, CusEntryHeader entryHeader, EDIMessage message, ImmutableHashSet<ZString> pdaRegisteredOperationCodes, ZString entryStatus)
		{
			if (pdaRegisteredOperationCodes.Contains(response.RegisteredOperationCode))
			{
				entryHeader.CH_EntryStatus = entryStatus;

				var entryLineIsMissingPreviousDocuments = entryHeader.MergedLines.Cast<CusEntryLine>().Any(x => x.PreviousDocuments.IsNullOrEmpty());
				if (!entryLineIsMissingPreviousDocuments)
				{
					TriggerInboxRequest(entryHeader, message, new ZString[] { DeclarationMessageTypeList.Codes.InBoxNotificationForImport });
				}
			}
			else
			{
				SetEntryStatusAndCSVClearance(response, entryHeader, message, entryStatus);

				var logTypeCode = (NoResString)"TS Guarantee";
				var log = entryHeader.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.ErrorReport.Code && c.Parameters.GetValueOrDefault(EventReferenceParameters.Codes.Type) == logTypeCode).LastOrDefault();
				if (log != null)
				{
					Logger.LogError(log.Parameters.GetValueOrDefault(EventReferenceParameters.Codes.Reason));
				}
			}
		}

		protected void SetCircuit(IImportCommon response, CusEntryHeader entryHeader)
		{
			if (TryGetCircuitCode(response.Circuit, response.CircuitSpecified, out var entryStatus))
			{
				entryHeader.SetMovementReferenceNumberEntryStatus(entryStatus);
			}

			if (response.Circuit == CircuitoTd.V)
			{
				ZDateTime.TryParseExact(response.ReleaseDate + response.ReleaseTime, out var entryReleaseDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);
				entryHeader.CH_EntryReleaseDate = entryReleaseDate;
				entryHeader.ZG_CSVImportCertificate = response.CSVImportCertificate;
			}
		}

		protected virtual void SetEntryStatusAndCSVClearance(IImportCommon response, CusEntryHeader entryHeader, EDIMessage message, ZString entryStatus)
		{
			entryHeader.CH_EntryStatus = entryStatus;

			SetCSVClearanceAndTriggerDocumentRequest(entryHeader, message, response.CSVClearance);
		}

		protected virtual bool ShouldSetEntryStatusCLP(CusEntryHeader entryHeader) => entryHeader.EntryInstruction?.IsSubStyleBOrZ ?? false;

		protected void SetAcceptedDeclarationData(IImportCommon response, CusEntryHeader entryHeader, string entryStatus, bool isSimplified = false)
		{
			ZDateTime.TryParseExact(response.AcceptanceDate + response.AcceptanceTime, out var acceptanceDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);

			SetGenericAcceptedDeclarationData(response, entryHeader, acceptanceDate, entryStatus, isSimplified);

			var limitPaymentDateCorrect = ZDateTime.TryParseExact(response.LimitPaymentDate, out var limitPaymentDate, CustomsDateTimeExtension.DateFormat);
			entryHeader.ZG_LimitPaymentDate = limitPaymentDateCorrect && !limitPaymentDate.IsEmpty ? limitPaymentDate : ZDateTime.Empty;

			entryHeader.ZG_PaymentProofNumber = response.PaymentProofNumber;

			SetCircuitCan(response, entryHeader);

			entryHeader.ZG_ATCPaymentProofNumber = response.PaymentProofNumberCan;

			var limitPaymentCanDateCorrect = ZDateTime.TryParseExact(response.LimitPaymentDateCan, out var limitPaymentCanDate, CustomsDateTimeExtension.DateFormat);
			entryHeader.ZG_ATCLimitPaymentDate = limitPaymentCanDateCorrect && !limitPaymentCanDate.IsEmpty ? limitPaymentCanDate : ZDateTime.Empty;
		}

		protected void SetGenericAcceptedDeclarationData(IImportCommonGeneric response, CusEntryHeader entryHeader, ZDateTime acceptanceDate, string entryStatus, bool isSimplified = false, string administration = "")
		{
			SetMovementReferenceNumber(entryHeader, acceptanceDate);

			entryHeader.ZG_Parallel = (entryHeader.TotalAmount != response.TotalAmountToPay);

			if (entryStatus != EntryStatusCodes.PreDeclarationAccepted)
			{
				SetFees(response, entryHeader, administration);
			}

			SetDeferredMethodOfPaymentForVATFees(response, entryHeader);

			if (!isSimplified)
			{
				entryHeader.ZG_ExportMRN = response.ExportMRN;
			}
		}

		void SetFees(IImportCommonGeneric response, CusEntryHeader entryHeader, string administration)
		{
			if (response.Lines != null)
			{
				foreach (CusEntryLine entryLine in entryHeader.AllEntryLines)
				{
					var item = response.Lines.FirstOrDefault(x => x.LineNumber == entryLine.CL_LineNumber);
					if (item != null)
					{
						RemoveFeesBeforeAdding(entryLine, administration);
						if (item.Tributes != null)
						{
							foreach (var tax in item.Tributes)
							{
								var fee = entryLine.Fees.AddNew();
								SetFee(fee, tax, entryHeader.Factory);
							}
						}
					}
				}
			}
		}

		void SetFee(CusEntryLineFee fee, Cas47TributoLiquidadoTd tax, CargoWise.EntityFramework.BusinessObjectFactory factory)
		{
			fee.CF_ChargeType = tax.C47TributoClase;
			fee.CF_BaseValue = tax.C47TributoBaseImponible;
			fee.CF_Rate = tax.C47TributoTipoImpositivo;
			fee.MaxMin = tax.C47TributoIndicadorMaxMinNor;
			fee.CF_RateOverrideReasonCode = "OVR";
			var rateDuty = tax.C47TributoUnidadFiscal;
			if (rateDuty.IsNullOrEmpty() || rateDuty == "%")
			{
				fee.G4_RateDuty = "%";
			}
			else
			{
				fee.G4_RateDuty = new ZString(rateDuty).ConvertESToCargoWise(factory);
			}
			fee.CF_ChargeAmount = tax.C47TributoCuotaPaga;
		}

		protected virtual void RemoveFeesBeforeAdding(CusEntryLine entryLine, string administration)
		{
			entryLine.Fees.RemoveAndDeleteAll();
		}

		void SetDeferredMethodOfPaymentForVATFees(IImportCommonGeneric response, CusEntryHeader entryHeader)
		{
			if (response.TotalDeferredVAT > 0)
			{
				foreach (CusEntryLine entryLine in entryHeader.AllEntryLines)
				{
					foreach (CusEntryLineFee fee in entryLine.Fees)
					{
						if (fee.CF_MethodOfPayment != UniversalReferenceConstants.FeeMethodOfPayment.Deferred && fee.G4_Type == RefCusRateCodes.Vat)
						{
							fee.CF_MethodOfPayment = UniversalReferenceConstants.FeeMethodOfPayment.Deferred;
						}
					}
				}
			}
		}

		protected void SetCircuitCan(IImportCommon response, CusEntryHeader entryHeader)
		{
			if (TryGetCircuitCode(response.CircuitCan, response.CircuitCanSpecified, out var circuitCan))
			{
				entryHeader.SetCircuitCan(circuitCan);
			}
		}

		protected bool TryGetCircuitCode(CircuitoTd? responseCircuit, bool isCircuitSpecified, out string circuitCode)
		{
			circuitCode = responseCircuit switch
			{
				CircuitoTd.A => CircuitCodeList.Codes.YELLOW,
				_ => GetCircuitCodeFromText(isCircuitSpecified ? responseCircuit?.ToString() : ZString.Empty)
			};
			return !string.IsNullOrEmpty(circuitCode);
		}

		protected void ResetGuaranteesAmountAndAddTransactions(TImportResponse response, CusEntryHeader entryHeader)
							=> ResetGuaranteesAmountAndAddTransactionsForAcceptedDeclaration(response, entryHeader, TransactionCommentPrefix, GetDebtAmountArrayFromResponseGuarantee);

		ZDecimal[] GetDebtAmountArrayFromResponseGuarantee(TImportResponse response, ZString reference)
		{
			var debtList = new List<ZDecimal>();

			var guarantees = (response.GRNGuarantees ?? new Collection<GarantiaGrNutilizadaTd>()).ToList();
			var guaranteesCan = (GetResponseGuaranteesCan(response) ?? new Collection<GarantiaGrNutilizadaTd>()).ToList();
			guarantees.AddRange(guaranteesCan);

			var responseGuarantee = guarantees.FirstOrDefault(x => x.CBgarantiaGrn == reference);

			debtList.Add(responseGuarantee?.CBimporteReal ?? decimal.Zero);
			debtList.Add(responseGuarantee?.CBimporteRealSinDeterminar ?? decimal.Zero);
			debtList.Add(responseGuarantee?.CBimportePotencial ?? decimal.Zero);

			return debtList.ToArray();
		}

		protected virtual Collection<GarantiaGrNutilizadaTd> GetResponseGuaranteesCan(TImportResponse response) => null;

		protected void TriggerDocument031CaptureInGreenCircuitWithNoCsvClearance(CusEntryHeader entryHeader, EDIMessage message)
		{
			if (entryHeader.MovementReferenceNumberEntryStatus == MessageFunctionCodeList.Codes.GreenCircuit &&
				entryHeader.CSVClearance.IsEmpty)
			{
				TriggerMisingDocumentRequest(entryHeader, message);
			}
		}

		protected const string AcceptedMessageCode = "0";

		const string TransactionCommentPrefix = "IMP ";

		protected const string RegisteredOperationCodePDCAccepted = "0";
		protected const string RegisteredOperationCodeDSPAccepted = "1";
		protected const string RegisteredOperationCodePDSAccepted = "2";
		protected const string RegisteredOperationCodePDSModification = "3";
		protected const string RegisteredOperationCodeDSPAcceptedByComplementary = "5";

		protected override CommonDocumentRequest<CusEntryHeader> GetNewDocumentRequest(CusEntryHeader businessObject, ZString certName, EDIMessage message) => new ImportDocumentRequest(businessObject, certName);
	}
}
