using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Chartera.Outgoing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static CargoWise.EventReference.Constants;
using static Enterprise.Customs.Business.CusPollingTransaction;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;

namespace Enterprise.Customs.CH.Business;

public class CharteraOutputDocumentRejectionMessageProcessor : BaseMessageProcessor
{
	public CharteraOutputDocumentRejectionMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Chartera Output Document Rejection Message Processor";

	protected override string ApplicationCodeCore => ApplicationCodes.CHCustomsCharteraOutput;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.REQ };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.Rejected };

	protected override void ProcessMessageCore(CHEDIMessage incomingMessage)
	{
		var factory = incomingMessage.Factory;

		var outgoingMessage = factory.GetOutgoingMessageFromSessionId(incomingMessage.Interchange?.EI_SessionGUID ?? ZGuid.Empty);
		if (outgoingMessage != null)
		{
			incomingMessage.EM_LinkedObject = outgoingMessage.EM_LinkedObject;

			switch (outgoingMessage.EM_MessageSubType)
			{
				case MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest:
					ProcessDocumentSearchRejection(incomingMessage, outgoingMessage);
					break;
				case MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryRequest:
					ProcessDocumentDeliveryRejection(incomingMessage, outgoingMessage);
					break;
			}
		}
	}

	void ProcessDocumentSearchRejection(EDIMessage incomingMessage, EDIMessage outgoingMessage)
	{
		if (outgoingMessage.EM_LinkedObject is CusPollingTransaction transaction)
		{
			transaction.CPT_Status = StatusCodes.Error;
			transaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;

			AddMessageRejectedEvent(transaction.ParentObject as IStmALogParent, incomingMessage, outgoingMessage);
		}
	}

	void ProcessDocumentDeliveryRejection(EDIMessage incomingMessage, EDIMessage outgoingMessage)
	{
		if (outgoingMessage.EM_LinkedObject is GlbCompany company && outgoingMessage is CHEDIMessage message
			&& message.MessageDetail is IDocumentDeliveryRequest outgoingRequest && outgoingRequest.DocumentIds.Count > 0)
		{
			foreach (var transaction in company.LoadTransactionsByDocumentId(outgoingRequest.DocumentIds.ToArray()))
			{
				transaction.CPT_Status = StatusCodes.Error;
				transaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;
			}

			AddMessageRejectedEvent(company, incomingMessage, outgoingMessage);
		}
	}

	void AddMessageRejectedEvent(IStmALogParent logParent, EDIMessage incomingMessage, EDIMessage outgoingMessage)
	{
		logParent?.Logs.AddNew(Events.MessageRejected,
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, outgoingMessage.EM_MessageSubType),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.DocumentSource, ApplicationCode),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.InterchangeNumber, incomingMessage.Interchange.EI_InterchangeNum));
	}
}
