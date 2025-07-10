using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Chartera;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.CH.Business.CompanyPollingTransaction;

namespace Enterprise.Customs.CH.Business;

public class CharteraOutputDocumentSearchResultMessageProcessor : CharteraOutputGetMessageAcknowledgeMessageProcessor
{
	public CharteraOutputDocumentSearchResultMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Chartera Output Document Search Result Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchResult };

	protected internal override bool LinkMessageToCompany => true;

	protected override void ProcessMessageCore(GlbCompany company, CHEDIMessage incomingEdiMessage)
	{
		base.ProcessMessageCore(company, incomingEdiMessage);

		if (incomingEdiMessage.MessageDetail is ICharteraSearchResultDetail searchResultDetail)
		{
			var dsrTransaction = company.LoadTransactionBySearchProcessId(searchResultDetail.ProcessId);
			if (dsrTransaction != null)
			{
				dsrTransaction.CPT_Status = StatusCodes.Closed;
				dsrTransaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;

				var docTransactions = company.LoadTransactionsByDocumentId(searchResultDetail.Documents.Select(x => x.DocumentId).ToArray());
				var effectiveCount = 0;
				foreach (var document in searchResultDetail.Documents.Where(d => !docTransactions.Any(t => t.CPT_TransactionID == d.DocumentId)))
				{
					company.CreateDocumentDeliveryTransaction(ApplicationCode, document.DocumentId);
					effectiveCount++;
				}

				incomingEdiMessage.EM_Status = EDIMessage.Status.ProcessedOK;

				if (dsrTransaction.CPT_Reference == ReferenceCodes.Manual)
				{
					company.Logs.AddNew(Events.ManualDocumentSearch,
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, EventReferenceConstants.Types.Result),
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Requested, effectiveCount.ToStringInvariantCulture()),
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Count, searchResultDetail.Count.ToStringInvariantCulture()));
				}
			}
			else
			{
				incomingEdiMessage.EM_Status = EDIMessage.Status.Warning;
			}
		}
	}

	protected override void UpdateTransaction(CusPollingTransaction midTransaction)
	{
		midTransaction.CPT_Status = StatusCodes.Closed;
		midTransaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;
	}
}
