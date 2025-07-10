using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Chartera.Outgoing;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Chartera;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class CharteraOutputErrorMessageProcessor : CharteraOutputGetMessageAcknowledgeMessageProcessor
{
	public CharteraOutputErrorMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Chartera Output Error Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.CharteraOutputError };

	protected internal override bool LinkMessageToCompany => true;

	protected override void ProcessMessageCore(GlbCompany company, CHEDIMessage incomingEdiMessage)
	{
		var factory = incomingEdiMessage.Factory;

		if (incomingEdiMessage.MessageDetail is IError error)
		{
			var outgoingRequestMessage = factory.GetOutgoingMessageFromApplicationReference(ApplicationCode, error.ProcessId, MessageTypeCodeList.Codes.REQ);
			if (outgoingRequestMessage != null)
			{
				incomingEdiMessage.EM_Status = EDIMessage.Status.ProcessedOK;
				switch (outgoingRequestMessage.EM_MessageSubType)
				{
					case MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest:
						ProcessDocumentSearchError(outgoingRequestMessage);
						break;
					case MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryRequest:
						ProcessDocumentDeliveryError(outgoingRequestMessage);
						break;
				}
			}
			else
			{
				incomingEdiMessage.EM_Status = EDIMessage.Status.Warning;
			}
		}
	}

	protected override void UpdateTransaction(CusPollingTransaction transaction)
	{
		transaction.CPT_Status = CompanyPollingTransaction.StatusCodes.Closed;
		transaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;
	}

	void ProcessDocumentSearchError(EDIMessage outgoingDocumentSearchRequestMessage)
	{
		if (outgoingDocumentSearchRequestMessage.EM_LinkedObject is CusPollingTransaction transaction)
		{
			transaction.CPT_Status = CompanyPollingTransaction.StatusCodes.Error;
			transaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;
			AddMessageRejectedEvent(transaction.ParentObject as IStmALogParent, outgoingDocumentSearchRequestMessage);
		}
	}

	void ProcessDocumentDeliveryError(EDIMessage outgoingDocumentDeliveryRequestMessage)
	{
		if (outgoingDocumentDeliveryRequestMessage.EM_LinkedObject is GlbCompany company && outgoingDocumentDeliveryRequestMessage is CHEDIMessage message
			&& message.MessageDetail is IDocumentDeliveryRequest outgoingRequest && outgoingRequest.DocumentIds.Count > 0)
		{
			foreach (var transaction in company.LoadTransactionsByDocumentId(outgoingRequest.DocumentIds.ToArray()))
			{
				transaction.CPT_Status = CompanyPollingTransaction.StatusCodes.Error;
				transaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;
			}
			AddMessageRejectedEvent(company, outgoingDocumentDeliveryRequestMessage);
		}
	}

	void AddMessageRejectedEvent(IStmALogParent logParent, EDIMessage outgoingMessage)
	{
		logParent?.Logs.AddNew(Events.ExternalValidationFailed,
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, outgoingMessage.EM_MessageSubType),
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.DocumentSource, ApplicationCode),
				new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.InterchangeNumber, outgoingMessage.Interchange.EI_InterchangeNum));
	}
}
