using System;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Customs.ES.NCTS.Business;

public abstract class NCTS5CommonResponseMessageProcessor<TResponseProvider, TPrettyMessage> : ESNCTSResponseMessageProcessor<TResponseProvider>
	where TResponseProvider : class, ICommonServiceSegment, IResponseCode
	where TPrettyMessage : IMessagePrettyFormatter
{
	protected NCTS5CommonResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
	{
	}

	const string ResponseCodeP = "P";
	const string ResponseCodeL = "L";
	const string ResponseCodeB = "B";
	const string ResponseCodeG = "G";

	protected const string ResponseCodeR = "R";
	protected const string ResponseCodeS = "S";

	protected abstract ZString XsdSchemaEmbeddedResourceName { get; }

	protected virtual ZBool IsOnlyAcceptedDeclaration => false;
	protected virtual ZBool SetPhaseStatusTo015 => false;

	protected ZString AcceptedResponseCode => AESAndNCTS5ResponseTypeCodeList.Codes.AcceptedMessage;

	protected sealed override void ProcessMessageCore(EDIMessage message, NctsHeader linkedBusinessObject, TResponseProvider provider)
	{
		message.EM_MessageNum = ((ZString)provider.ServiceSegmentId).Left(AutoEDIMessage.Schema.EM_MessageNumMaxLength);

		var messagePrettyFormatter = GetNewMessagePrettyFormatter(provider, message, linkedBusinessObject);

		ProcessDeclarationCore(provider, message, linkedBusinessObject, messagePrettyFormatter);

		if (!IsAnnexMessage(message))
		{
			SetMessageStatusAsReceived(message);
			SetCHStatusAsReceived(linkedBusinessObject, messageStatus: ZString.Empty);
		}

		if (SetPhaseStatusTo015 && linkedBusinessObject.IsDepartureMovement)
		{
			linkedBusinessObject.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.Declaration;
		}
	}

	void ProcessDeclarationCore(TResponseProvider response, EDIMessage message, NctsHeader nctsHeader, TPrettyMessage messagePrettyFormatter)
	{
		RemoveCusPollingTransactionsIfNeeded(nctsHeader.Factory, MessageTypesToInclude, nctsHeader.MovementReferenceNumber);

		if (IsOnlyAcceptedDeclaration || response.ResponseCode == AcceptedResponseCode)
		{
			var extraDataFromProcessing = ProcessAcceptedDeclaration(response, message, nctsHeader);
			message.EM_MessageInterpretation = messagePrettyFormatter.CreateMessageDetailsAccepted(extraDataFromProcessing);
			SetMessageSubTypeAsAccepted(message);
		}
		else
		{
			ProcessRejectedDeclaration(response, message, nctsHeader);
			message.EM_MessageInterpretation = messagePrettyFormatter.CreateMessageDetailsRejected();
			SetMessageSubTypeAsRejected(message);
		}
	}

	protected override TResponseProvider GetMessageProviderCore(EDIMessage message)
	{
		return DeserializeMessage(message, XsdSchemaEmbeddedResourceName);
	}

	protected TResponseProvider DeserializeMessage(EDIMessage message, string xsdSchemaEmbeddedResourceName)
	{
		using (var textReader = message.GetEM_MessageTextReader())
		using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
		{
			return ESXmlObjectSerializer.DeserializeWithoutValidation<TResponseProvider>(xsdSchemaEmbeddedResourceName, bodyTextReader, isAES: false, isNCTS: true);
		}
	}

	protected abstract ZString ProcessAcceptedDeclaration(TResponseProvider response, EDIMessage message, NctsHeader nctsHeader);

	protected virtual void ProcessRejectedDeclaration(TResponseProvider response, EDIMessage message, NctsHeader nctsHeader) { }

	protected abstract TPrettyMessage GetNewMessagePrettyFormatter(TResponseProvider response, EDIMessage message, NctsHeader nctsHeader);

	protected void SetEntryNumbers(EDIMessage message, NctsHeader nctsHeader, ZString mrn, ZString expeditionCircuit, DateTime? admisionDate, ZString csvClearanceCode, ZString responseCode, DateTime? clearanceDate, DateTime? limitDateOfArrival, ZString summaryNumber, bool clearanceNoDependOfResponseCode = false)
	{
		var circuit = GetCircuitCodeFromText(expeditionCircuit);
		if (!mrn.IsEmpty)
		{
			var admisionDateCorrect = admisionDate == null ? ZDateTime.Empty : (ZDateTime)admisionDate;
			CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, mrn, circuit, admisionDateCorrect, ZDateTime.Empty, shouldChangeStatus: !circuit.IsEmpty, shouldChangeIssueDate: !admisionDateCorrect.IsEmpty);
		}

		if (!csvClearanceCode.IsEmpty && (responseCode == ResponseCodeL || clearanceNoDependOfResponseCode))
		{
			var limitDate = limitDateOfArrival == null ? ZDateTime.Empty : (ZDateTime)limitDateOfArrival;
			CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Spain.ClearanceCSV, csvClearanceCode, ZString.Empty, (ZDateTime)clearanceDate, limitDate);

			if (nctsHeader.IsDepartureMovement)
			{
				TriggerMisingDocumentRequest(nctsHeader, message);
			}
		}

		if (!summaryNumber.IsEmpty && responseCode == ResponseCodeR && circuit == MessageFunctionCodeList.Codes.GreenCircuit)
		{
			CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Spain.SummaryEntryNumber, summaryNumber, ZString.Empty, ZDateTime.Empty, ZDateTime.Empty);
		}
	}

	protected void SetDepartureStatus(NctsHeader nctsHeader, ZString responseCode)
	{
		ZString customsStatus = (string)responseCode switch
		{
			ResponseCodeP => ESNCTS5DepartureCustomsStatusList.Codes.PreLodged,
			ResponseCodeL => ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit,
			ResponseCodeB => ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl,
			ResponseCodeG => ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance,
			_ => ZString.Empty
		};

		if (!customsStatus.IsEmpty)
		{
			nctsHeader.MovementHeader.BM_CustomsStatus = customsStatus;
		}
	}

	protected void SetArrivalStatusCommon(NctsHeader nctsHeader, ZString responseStatus, ZString customsStatusWhenNotCompleted)
					=> nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = responseStatus == Ncts5TransitStatusList.Codes.Completed
																						? ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease
																						: customsStatusWhenNotCompleted;

	protected void SetReleaseDate(DateTime? releaseDate, NctsHeader nctsHeader)
	{
		if (releaseDate.HasValue)
		{
			nctsHeader.SummaryEntryNumber.CE_IssueDate = releaseDate.Value;
		}
	}

	protected void ProcessAcceptedDepartureAndNotifGoods(EDIMessage message, NctsHeader nctsHeader, ZString responseCode, ZString mrn, ZString expeditionCircuit, DateTime? admisionDate, ZString csvClearanceCode, DateTime? clearanceDate, DateTime? limitDateOfArrival)
	{
		if (!nctsHeader.IsDepartureMovement)
		{
			throw new InvalidOperationException(SetNctsMessageFailedLogDescription(nctsHeader, "Departure"));
		}

		SetEntryNumbers(message, nctsHeader, mrn, expeditionCircuit, admisionDate, csvClearanceCode, responseCode, clearanceDate, limitDateOfArrival, ZString.Empty);
		SetDepartureStatus(nctsHeader, responseCode);

		var logTypeCode = (NoResString)"TS Guarantee";
		var log = nctsHeader.MovementHeader.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.ErrorReport.Code && c.Parameters.GetValueOrDefault(EventReferenceParameters.Codes.Type) == logTypeCode).LastOrDefault();
		if (log != null)
		{
			Logger.LogError(log.Parameters.GetValueOrDefault(EventReferenceParameters.Codes.Reason));
		}

		UpdateGuaranteeTransactionsIfNeeded(message, Customs.Business.PermitTransactionStatusList.Codes.Confirmed, UpdateTransactionReference(mrn));
	}

	protected sealed override CommonDocumentRequest<NctsHeader> GetNewDocumentRequest(NctsHeader businessObject, ZString certName, EDIMessage message) => new NCTS5DepartureDocumentRequest(businessObject, certName);
}
