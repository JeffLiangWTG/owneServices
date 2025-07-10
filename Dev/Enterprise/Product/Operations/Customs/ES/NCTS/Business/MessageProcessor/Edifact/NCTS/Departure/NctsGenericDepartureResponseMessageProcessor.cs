using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public abstract class NctsGenericDepartureResponseMessageProcessor : ESNCTSResponseMessageProcessor<INctsDepartureAndTIRResponseMessageProvider>
	{
		protected NctsGenericDepartureResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override sealed void ProcessMessageCore(EDIMessage message, NctsHeader linkedBusinessObject, INctsDepartureAndTIRResponseMessageProvider provider)
		{
			if (!linkedBusinessObject.IsDepartureMovement)
			{
				throw new InvalidOperationException(SetNctsMessageFailedLogDescription(linkedBusinessObject, "Departure"));
			}

			SetDepartureStatus(message, linkedBusinessObject, provider);

			var messagePrettyFormatter = new NctsDepartureMessagePrettyFormatter(provider);

			if (provider.DocumentMessageName == ES.Business.UniversalReferenceConstants.DeclarationResponseCode.Accepted)
			{
				SetClearanceCriteria(linkedBusinessObject, provider);
				SetPrintProcedure(linkedBusinessObject, provider);
				SetEntryNumbers(message, linkedBusinessObject, provider);

				message.EM_MessageInterpretation = messagePrettyFormatter.CreateMessageDetailsAccepted();
				SetMessageSubTypeAsAccepted(message);

				if (linkedBusinessObject.IsSafetyAndSecurityUncheckedWithExistingSecurityData)
				{
					DeleteSecurityData(linkedBusinessObject);
				}
			}
			else
			{
				message.EM_MessageInterpretation = messagePrettyFormatter.CreateMessageDetailsRejected();
				SetMessageSubTypeAsRejected(message);
			}

			SetMessageStatusAsReceived(message);
			SetCHStatusAsReceived(linkedBusinessObject);
		}

		void SetDepartureStatus(EDIMessage message, NctsHeader header, INctsDepartureAndTIRResponseMessageProvider response)
		{
			var (customsStatus, transactionStatus) = (string)response.MessageFunction switch
			{
				MessageFunctionCodeList.Codes.Rejected => (NctsTransitStatusList.Codes.DeclarationRejected, Customs.Business.PermitTransactionStatusList.Codes.Deleted),
				MessageFunctionCodeList.Codes.GreenCircuit => (NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, Customs.Business.PermitTransactionStatusList.Codes.Confirmed),
				MessageFunctionCodeList.Codes.RedCircuit or MessageFunctionCodeList.Codes.OrangeCircuit or MessageFunctionCodeList.Codes.WaitingForAdmission => (NctsTransitStatusList.Codes.GoodsNotReleasedForTransit, Customs.Business.PermitTransactionStatusList.Codes.Confirmed),
				_ => (null, null)
			};

			if (customsStatus is not null)
			{
				header.MovementHeader.BM_CustomsStatus = customsStatus;
			}

			if (transactionStatus is not null)
			{
				var updateTransactionAction =
					transactionStatus == Customs.Business.PermitTransactionStatusList.Codes.Confirmed
						? UpdateTransactionReference(response.RegistrationNumber)
						: null;
				UpdateGuaranteeTransactionsIfNeeded(message, transactionStatus, updateTransactionAction);
			}
		}

		void SetClearanceCriteria(NctsHeader header, INctsDepartureAndTIRResponseMessageProvider response)
		{
			if ((string)response.CustomsClearanceCriteria
				is ClearanceCriteriaCodeList.Codes.NormalProcedure
				or ClearanceCriteriaCodeList.Codes.SimplifiedProcedure
				or ClearanceCriteriaCodeList.Codes.SatisfactoryProcedure)
			{
				header.ESNctsHeader.CEN_ClearanceCriteria = response.CustomsClearanceCriteria;
			}
		}

		void SetPrintProcedure(NctsHeader header, INctsDepartureAndTIRResponseMessageProvider response)
		{
			if ((string)response.PrintActionRequired
				is TADPrintProcedureCodeList.Codes.OperatorMustPrintOutCopyAOfTad
				or TADPrintProcedureCodeList.Codes.OperatorMustPrintOutCopiesAAndBOfTad
				or TADPrintProcedureCodeList.Codes.CustomsMustPrintOutTad)
			{
				header.ESNctsHeader.CEN_TADPrintProcedure = response.PrintActionRequired;
			}
		}

		void SetEntryNumbers(EDIMessage message, NctsHeader header, INctsDepartureAndTIRResponseMessageProvider response)
		{
			var mrnCode = response.RegistrationNumber;
			if (!mrnCode.IsEmpty)
			{
				CreateOrUpdateCusEntryNumber(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, mrnCode, response.MessageFunction, response.AdmissionDate, ZDateTime.Empty);
				header.MovementHeader.BM_ValuationDate = header.MovementReferenceEntryNumber.CE_IssueDate;
			}

			var csvClearanceCode = response.CSVReleaseCode;
			if (!csvClearanceCode.IsEmpty && (response.MessageFunction == MessageFunctionCodeList.Codes.GreenCircuit))
			{
				CreateOrUpdateCusEntryNumber(header, CusEntryNumberTypes.Spain.ClearanceCSV, csvClearanceCode, ZString.Empty, response.CSVReleaseCreationDate, response.TransitMaxDate);

				TriggerMisingDocumentRequest(header, message);
			}
		}

		protected override INctsDepartureAndTIRResponseMessageProvider GetMessageProviderCore(EDIMessage message)
		{
			return message.EM_MessageText.Contains(EdifactCodes.UNHSegmentCode)
				? NCTSCUSRESV921ESMessageHelper.New(message)
				: (INctsDepartureAndTIRResponseMessageProvider)NCTSEdifactProcessorHelper.ProcessEdifactErrorResponse(message);
		}

		protected override CommonDocumentRequest<NctsHeader> GetNewDocumentRequest(NctsHeader businessObject, ZString certName, EDIMessage message) => new NCTSDepartureDocumentRequest(businessObject, certName);

		void DeleteSecurityData(NctsHeader header)
		{
			header.MovementHeader.BM_BTAIndicator = ZString.Empty;
			header.MovementHeader.BM_MethodOfPayment = ZString.Empty;
			header.MovementHeader.BM_AdditionalText = ZString.Empty;
			header.MovementHeader.BM_ConveyanceNumber = ZString.Empty;

			header.SecurityConsignor.Delete();
			header.SecurityConsignee.Delete();

			header.Itinerary.RemoveAndDeleteAll();

			header.PlaceOfUnloadingCode = ZString.Empty;
			header.BH_OH_Carrier = ZGuid.Empty;
			header.BH_UniqueVoyageIdentifier = ZString.Empty;
		}
	}
}
