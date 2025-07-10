using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Chartera;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;

namespace Enterprise.Customs.CH.Business;

public class CharteraOutputDocumentDeliveryResultMessageProcessor : CharteraOutputGetMessageAcknowledgeMessageProcessor
{
	public CharteraOutputDocumentDeliveryResultMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Chartera Output Document Delivery Result Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryResult };

	protected override void ProcessMessageCore(GlbCompany company, CHEDIMessage incomingEdiMessage)
	{
		base.ProcessMessageCore(company, incomingEdiMessage);

		if (incomingEdiMessage.MessageDetail is IDocumentResponseDetail responseDetail)
		{
			if (IsKnownRequest(incomingEdiMessage, responseDetail.ProcessId))
			{
				var docTransaction = company.LoadTransactionByDocumentId(responseDetail.DocumentId);
				if (docTransaction != null && docTransaction.CPT_Status != StatusCodes.Closed)
				{
					docTransaction.CPT_Status = StatusCodes.Closed;
					docTransaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;

					var documentNotificationMessage = incomingEdiMessage.Factory.GetIncomingMessageFromApplicationReference(EDIMessage.ApplicationCodes.CHCustomsPassar, responseDetail.DocumentId, MessageTypeCodeList.Codes.MSG, MessageSubTypeCodeList.Codes.PassarDocumentNotification);

					BusinessObject parent;

					if (documentNotificationMessage != null)
					{
						parent = documentNotificationMessage.EM_LinkedObject;
					}
					else
					{
						parent = MessageProcessorHelper.FindCusEntryNumParent(company, responseDetail.SourceId);
					}

					if (parent != null)
					{
						incomingEdiMessage.EM_LinkedObject = parent;
						incomingEdiMessage.EM_Status = EDIMessage.Status.ProcessedOK;

						if (parent is IDocManagerSupport docManagerSupport)
						{
							docManagerSupport.DocManagerInfo.AddFileOrDocument(responseDetail.DocumentData.ToArray(), responseDetail.DocumentName, RefDocTypes.CustomsAuthority);
							docManagerSupport.DocManagerInfo.Save();
							var reference = CreateDocumentAllocatedEventReference(responseDetail.DocumentType);
							((EnterpriseBusinessObject)parent).Logs.AddNew(Events.DocumentAllocated, reference);
						}

						if (parent is IBranchProvider branchProvider)
						{
							incomingEdiMessage.EM_GB = incomingEdiMessage.Interchange.EI_GB = branchProvider.Branch.PK;
						}
					}
					else
					{
						incomingEdiMessage.EM_Status = EDIMessage.Status.Warning;
					}
				}
			}
			else
			{
				incomingEdiMessage.EM_Status = EDIMessage.Status.Warning;
			}
		}
	}
	
	ZString CreateDocumentAllocatedEventReference(string documentType)
	{
		var parameters = new Dictionary<string, string>
			{
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ExternalDocumentType, documentType },
			};
		return StmALog.GenerateEventReference(ZString.Empty, parameters);
	}

	protected override void UpdateTransaction(CusPollingTransaction midTransaction)
	{
		if (midTransaction.CPT_Status != StatusCodes.Closed)
		{
			midTransaction.CPT_Status = StatusCodes.Closed;
			midTransaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;
		}
	}

	bool IsKnownRequest(EDIMessage incomingMessage, string processId)
	{
		var query = new ZQuery();
		query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CHCustomsCharteraOutput);
		query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeCodeList.Codes.REQ);
		query.AddToFilter(EDIMessageSchema.EM_MessageSubType, MessageSubTypeCodeList.Codes.CharteraOutputDocumentDeliveryRequest);
		query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIInterchange.Direction.Transmit);
		query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, processId);
		return incomingMessage.Factory.LoadTop1<EDIMessage>(query) != null;
	}
}
