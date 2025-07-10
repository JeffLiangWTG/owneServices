using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.Accounting.DataTransfer.EInvoicing.KoreaSouth;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public class KoreaSouthEInvoicingEventMessageProcessor : EInvoicingEventMessageProcessor
	{
		protected override string LogErrorNotFoundKey => "TransactionNotFound"; // Error Message Key for Developers Only

		public KoreaSouthEInvoicingEventMessageProcessor(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch)
		: base(logger, message, universalEvent, invoiceBatch)
		{
			PivotsLazyLoading = new Lazy<(IReadOnlyDictionary<ZGuid, AccEInvoicingTransactionPivot>, IReadOnlyDictionary<ZGuid, AccEInvoicingTransactionPivot>)>(() => LoadRelatedPivotAtOnce());
			IncorrectTransactionDetailsList = new List<IncorrectTransactionDetails>();
		}

		public override void Process()
		{
			BatchErrorMessage = ZString.Empty;
			IncorrectTransactionDetailsList.Clear();

			if (invoiceBatch.AIB_Status != EInvoicingBatchState.Discarded)
			{
				if (SubmitBatch == null)
				{
					ReportAndLogError("MissingSubmitBatch", Res.GetString("D1A4F60A-700C-4D24-9D28-673CF57EDEC8", "Submission batch is not found."));
				}
				else if (!SubmitBatch.TransactionPivots.IsNullOrEmpty())
				{
					if (PivotsCacheSUB.Values.Any(x => x.AIP_Status != EInvoicingPivotState.Succeed && x.AIP_Status != EInvoicingPivotState.Failed))
					{
						LoadAllInovicesToCache(PivotsCacheSUB.Keys);
						if (universalEvent.EventType.Value == AutoEvents.InterchangeAcknowledgedCode)
						{
							ProcessEventMessage();
						}
						else
						{
							ProcessIRJEventMessage();
						}
					}
					else
					{
						logger.LogBoth(LogType.Warning, Res.GetString("821CC74B-AF09-4061-8691-EE791FF168E4", "No update performed due to transaction pivot having 'SUC' or 'FAL' status for invoice submit batch {0} in {1}.", SubmitBatch.AIB_BatchNumber, companyName));
					}
				}
				else
				{
					ReportAndLogError("PivotNotFound", Res.GetString("A8F5A997-E20E-43BF-B533-224072B375A3", "No update performed due to transaction pivot not found for invoice submit batch {0} in {1}.", SubmitBatch.AIB_BatchNumber, companyName));
				}
			}

			if (IncorrectTransactionDetailsList.Any())
			{
				SendErrorNotificationEmail(ZString.Empty);
			}
		}

		void ProcessEventMessage()
		{
			var statusCode = universalEvent.ContextCollection.FirstOrDefault(context => context.Type == EInvoicingKoreaSouthConstants.DataContext.KoreaStatusCode)?.Value
				?? string.Empty;
			switch (statusCode)
			{
				case EInvoicingKoreaSouthConstants.StatusCodes.QuerySuccess:
					QuerySucess();
					break;
				case EInvoicingKoreaSouthConstants.StatusCodes.QueryFail:
					BatchFail(universalEvent.EventParameters?.Reason ?? ZString.Empty);
					break;
				case EInvoicingKoreaSouthConstants.StatusCodes.SubmitFail:
					BatchFail(universalEvent.EventParameters?.Reason ?? ZString.Empty);
					break;
				case EInvoicingKoreaSouthConstants.StatusCodes.QueryProcessing:
					TryQueryStatusWithinMaxTimes(ReadyKoreaConstants.MaxQueryStatusTimes);
					break;
				case EInvoicingKoreaSouthConstants.StatusCodes.SubmitSuccess:
					SubmitSuccess();
					break;
				default:
					ReportAndLogError("UnknownStatusCode", Res.GetString("4B911778-6B00-4362-A615-218BF1052EE8", "No update performed due to invalid status code [{0}].", statusCode));
					break;
			}
		}

		void QuerySucess()
		{
			var validationDocumentMappings = universalEvent.ContextCollection.Where(context => context.Type == EInvoicingKoreaSouthConstants.DataContext.KoreaValidationDocumentStatus)
				.Select(x => new ValidationDocument(x.Value?.ToString()));
			var receiptID = universalEvent.ContextCollection.FirstOrDefault(context => context.Type == EInvoicingKoreaSouthConstants.DataContext.KoreaReceiptID)?.Value
				?? string.Empty;
			foreach (AccEInvoicingTransactionPivot pivot in SubmitBatch.TransactionPivots.ToArray())
			{
				var issueId = GetIssueId(pivot);
				var validationDocument = validationDocumentMappings.FirstOrDefault(x => x.IssueId == issueId);

				switch (validationDocument.StatusCode)
				{
					case ReadyKoreaConstants.ValidationDocumentStatusCodes.Success:
						Success(pivot, receiptID);
						break;
					case ReadyKoreaConstants.ValidationDocumentStatusCodes.Fail:
						IncorrectTransactionDetailsList.Add(IncorrectTransactionDetails.FromBizo(Factory.Load<InvoicingBase>(pivot.AIP_ParentID), new ZString[] { validationDocument.ErrorMessage }));
						Fail(pivot, validationDocument.ErrorMessage);
						break;
					default:
						var invoice = Factory.Load<InvoicingBase>(pivot.AIP_ParentID);

						var unknownInvoiceActionMsgForLog = Res.GetString("0F1096FB-0918-4D94-AC4D-6FDD268FBA95", "Invalid Issue-ID mapping result [{0}] for [{1}].", validationDocument.ContextValue, GetInvoiceInfo(invoice));
						logger.LogBoth(LogType.Warning, unknownInvoiceActionMsgForLog);

						var unknownInvoiceActionMsgForInvoice = (string.IsNullOrWhiteSpace(issueId) || issueId != validationDocument.IssueId)
							? Res.GetString("54F98C14-1086-46DF-B13A-1B250E6F3F7F", "Did not find Issue-ID mapping result.")
							: Res.GetString("A3BFEC1A-4FA7-4761-8CC9-FF3C61742CB6", "Invalid Issue-ID mapping result [{0}].", validationDocument.ContextValue);
						IncorrectTransactionDetailsList.Add(IncorrectTransactionDetails.FromBizo(invoice, new ZString[] { unknownInvoiceActionMsgForInvoice }));
						Fail(pivot, unknownInvoiceActionMsgForInvoice);
						break;
				}
			}

			SubmitBatch.ClearQueryTimes();
		}

		void Success(AccEInvoicingTransactionPivot pivot, string receiptID)
		{
			var submitPivot = FindSubmitPivoit(pivot);
			submitPivot.Batch.UpdateBatchAndPivotStatusAndErrorDescription(new[] { submitPivot.AIP_ParentID }, ZString.Empty, EInvoicingPivotState.Succeed, lastResponseReceivedUtc: LastResponseReceivedTime);

			var transaction = Factory.Load<InvoicingBase>(submitPivot.AIP_ParentID);
			AttachTaxInvoiceXMLToEDocs(transaction);

			var reference = transaction.Factory.New<AccTransactionHeaderReference>();
			reference.AH1_AH = transaction.PK;
			reference.AH1_Type = AccTransactionHeaderReferenceTypes.RED;
			reference.AH1_Reference = receiptID;

			TryRemoveQueryPivoit(pivot);
			UpdateEInvoicingPivotForLinkedTransaction(transaction);
		}

		void UpdateEInvoicingPivotForLinkedTransaction(InvoicingBase transaction)
		{
			var transactionTypeQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			transactionTypeQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
			transactionTypeQuery.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote);

			var transactionQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			transactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, transaction.PK);
			transactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, transaction.AH_GC);
			transactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			transactionQuery.AddToFilter(transactionTypeQuery);

			var pivotSubmitQuery = new ZDBOnlySubQuery(typeof(AccEInvoicingTransactionPivot), AccEInvoicingTransactionPivotSchema.AIP_ParentID);
			pivotSubmitQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.Submit);
			pivotSubmitQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Pending);
			transactionQuery.AddSubQuery(pivotSubmitQuery, JoinCondition.And);

			var linkedTransactions = transaction.Factory.Load<InvoicingBase>(transactionQuery);
			linkedTransactions.ForEach(x => x.EInvoicingTransactionPivotSubmitted.AIP_Status = EInvoicingPivotState.Queued);
		}

		void TryQueryStatusWithinMaxTimes(int maxTimes)
		{
			if (SubmitBatch.AIB_QueryTimes >= maxTimes)
			{
				BatchFail(Res.GetString("B0AA7F40-1396-4748-8372-C69E17A52248", "Exceeded the maximum times of automatic Query Status. Please retrieve the e-Invoice status manually via Action > Request e-Invoice Status, and take the appropriate action based on the status returned."));
			}
			else
			{
				Query();
				SubmitBatch.AIB_QueryTimes++;
			}
		}

		void SubmitSuccess()
		{
			foreach (AccEInvoicingTransactionPivot pivot in SubmitBatch.TransactionPivots)
			{
				pivot.AIP_Status = EInvoicingPivotState.Delivered;
			}
			Query();
		}

		void AttachTaxInvoiceXMLToEDocs(InvoicingBase transaction)
		{
			var issueIDFormInvoice = KoreaSouthEInvoicingHelper.GetIssueIDFromInvoice(transaction);
			DocTaxInvoiceFinder.GetXml(issueIDFormInvoice);

			if (DocTaxInvoiceFinder.XMLDocument != null)
			{
				var fileName = $"TaxInvoice_{issueIDFormInvoice}_{XmlToAdditionalInfoConverter.GetIssueDateTimeFromXML(DocTaxInvoiceFinder.XMLDocument)}.xml";
				transaction.DocManagerInfo.AddFileOrDocument(MessageEncoding.UTF8WithoutBOM.GetBytes(DocTaxInvoiceFinder.XMLStr), fileName, ReferenceTypes.Accounting);
				transaction.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
				CreateAccTransactionHeaderAuthorisationRecord(transaction, DocTaxInvoiceFinder.XMLStr);
			}
			else
			{
				ReportAndLogError("UnableToAttachTaxInvoiceXML", Res.GetString("6262BABC-FB4D-4E0C-89B9-F044EE12A1A9", "Unable to attach the tax invoice XML of Invoice [{0}] to eDoc.", transaction.AH_TransactionNum));
			}
		}

		void CreateAccTransactionHeaderAuthorisationRecord(InvoicingBase transaction, string xmlStr)
		{
			var authRecord = transaction.Factory.New<KoreaSouthAccTransactionHeaderAuthorisationRecord>();
			authRecord.AHF_ParentId = transaction.PK;
			authRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			authRecord.AHF_Number = new KoreaSouthEInvoicingDataFinder().GetSubmitId(SubmitBatch);
			authRecord.AHF_AuthorisationData = new ZBlob(MessageEncoding.UTF8WithoutBOM.GetBytes(xmlStr));
		}

		void Query()
		{
			var queryPivot = PivotsCacheSTA.Values.FirstOrDefault() ?? FindOrCreateQueryPivoit(PivotsCacheSUB.First().Value);
			queryPivot.AIP_Status = EInvoicingPivotState.Queued;
			queryPivot.AIP_LastResponseReceivedUtc = LastResponseReceivedTime;
		}

		void BatchFail(string batchErrorMessage)
		{
			IncorrectTransactionDetailsList.AddRange(PivotsCacheSUB.Keys.Select(invoicePK => IncorrectTransactionDetails.FromBizo(Factory.Load<InvoicingBase>(invoicePK), null)).ToArray());
			SubmitBatch.UpdateBatchAndPivotStatusAndErrorDescription(PivotsCacheSUB.Keys, batchErrorMessage, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
			PivotsCacheSTA.Values.ForEach(x => x.Delete());
			BatchErrorMessage = batchErrorMessage;
			SubmitBatch.ClearQueryTimes();
		}

		void Fail(AccEInvoicingTransactionPivot pivot, string errorMessage)
		{
			FindSubmitPivoit(pivot).Batch.UpdateBatchAndPivotStatusAndErrorDescription(new[] { pivot.AIP_ParentID }, errorMessage, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
			TryRemoveQueryPivoit(pivot);
		}

		void LoadAllInovicesToCache(IEnumerable<ZGuid> invoicePks)
		{
			Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, invoicePks));
		}

		ZString GetIssueId(AccEInvoicingTransactionPivot pivot)
		{
			var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_AH, pivot.AIP_ParentID)
				.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccTransactionHeaderReferenceTypes.KRI);

			return Factory.LoadTop1<AccTransactionHeaderReference>(query)?.AH1_Reference ?? null;
		}

		void ProcessIRJEventMessage()
		{
			var errorMessageforPivot = universalEvent.EventParameters?.Reason ?? ZString.Empty;
			switch (MessageSubType)
			{
				case KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest:
					BatchFail(errorMessageforPivot);
					break;
				case KoreaSouthEInvoiceAPICommandList.Codes.QueryInvoiceRequest:
					PivotsCacheSTA.ForEach(pivot =>
						pivot.Value.Batch.UpdateBatchAndPivotStatusAndErrorDescription(new[] { pivot.Value.AIP_ParentID }, errorMessageforPivot, EInvoicingPivotState.Queued, lastResponseReceivedUtc: LastResponseReceivedTime)
					);
					IncorrectTransactionDetailsList.AddRange(PivotsCacheSUB.Keys.Select(invoicePK => IncorrectTransactionDetails.FromBizo(Factory.Load<InvoicingBase>(invoicePK), null)).ToArray());
					BatchErrorMessage = $@"All invoices in this batch have re-queuqed automatically due to following reason:
{errorMessageforPivot}";
					break;
				default:
					break;
			}
		}

		AccEInvoicingTransactionPivot FindOrCreateQueryPivoit(AccEInvoicingTransactionPivot currentPivot)
			=> FindPivoit(currentPivot, EInvoicingPivotActionType.StatusCheck)
				?? currentPivot.ParentTransactionHeader.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck);

		AccEInvoicingTransactionPivot FindSubmitPivoit(AccEInvoicingTransactionPivot currentPivot)
			=> FindPivoit(currentPivot, EInvoicingPivotActionType.Submit);

		void TryRemoveQueryPivoit(AccEInvoicingTransactionPivot currentPivot)
			=> FindPivoit(currentPivot, EInvoicingPivotActionType.StatusCheck)?.Delete();

		AccEInvoicingTransactionPivot FindPivoit(AccEInvoicingTransactionPivot currentPivot, string actionType)
		{
			if (currentPivot.AIP_ActionType == actionType)
			{
				return currentPivot;
			}

			switch (actionType)
			{
				case EInvoicingPivotActionType.Submit:
					return PivotsCacheSUB.TryGetValue(currentPivot.AIP_ParentID, out var cachedSUB) ? cachedSUB : null;
				case EInvoicingPivotActionType.StatusCheck:
					return PivotsCacheSTA.TryGetValue(currentPivot.AIP_ParentID, out var cachedSTA) ? cachedSTA : null;
				default:
					return null;
			}
		}

		IReadOnlyDictionary<ZGuid, AccEInvoicingTransactionPivot> PivotsCacheSUB => PivotsLazyLoading.Value.pivotCacheSUB;
		IReadOnlyDictionary<ZGuid, AccEInvoicingTransactionPivot> PivotsCacheSTA => PivotsLazyLoading.Value.pivotsCacheSTA;

		Lazy<(IReadOnlyDictionary<ZGuid, AccEInvoicingTransactionPivot> pivotCacheSUB, IReadOnlyDictionary<ZGuid, AccEInvoicingTransactionPivot> pivotsCacheSTA)> PivotsLazyLoading { get; }

		(IReadOnlyDictionary<ZGuid, AccEInvoicingTransactionPivot> pivotCacheSUB, IReadOnlyDictionary<ZGuid, AccEInvoicingTransactionPivot> pivotsCacheSTA) LoadRelatedPivotAtOnce()
		{
			var relatedPivots = Factory.Load<AccEInvoicingTransactionPivot>(
				new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, SubmitBatch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().Select(x => x.AIP_ParentID).ToArray())
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, new[] { EInvoicingPivotActionType.Submit, EInvoicingPivotActionType.StatusCheck })
			);
			return (
				relatedPivots.Where(x => x.AIP_ActionType == EInvoicingPivotActionType.Submit).ToDictionary(x => x.AIP_ParentID, x => x),
				relatedPivots.Where(x => x.AIP_ActionType == EInvoicingPivotActionType.StatusCheck).ToDictionary(x => x.AIP_ParentID, x => x)
			);
		}
		AccEInvoicingBatch SubmitBatch => submitBatch ?? (submitBatch = GetSubmitBatch());
		AccEInvoicingBatch submitBatch;

		KoreaDocTaxInvoiceFinder DocTaxInvoiceFinder => docTaxInvoiceFinder ?? (docTaxInvoiceFinder = new KoreaDocTaxInvoiceFinder(SubmitBatch));
		KoreaDocTaxInvoiceFinder docTaxInvoiceFinder;

		AccEInvoicingBatch GetSubmitBatch()
		{
			if (MessageSubType == KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest)
			{
				return invoiceBatch;
			}

			var oneSubmitPivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(
				new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoiceBatch.TransactionPivots[0].AIP_ParentID)
					.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, new[] { EInvoicingPivotActionType.Submit })
			);
			return Factory.Load<AccEInvoicingBatch>(oneSubmitPivot.AIP_AIB);
		}

		ZString MessageSubType => messageSubType ?? (ZString)(messageSubType = universalEvent.EventParameters?.MessageSubType ?? ZString.Empty);
		ZString? messageSubType;

		string GetInvoiceInfo(InvoicingBase invoice)
		{
			return $"{invoice.AH_Ledger} {invoice.AH_TransactionType} {invoice.AH_TransactionNum}";
		}

		protected override bool ShouldSendEmail => IncorrectTransactionDetailsList.Any();

		protected override GEIEmailNotificationCreator GetEmailNotificationCreator(ZString errorMessage, ILogger logCollector)
		{
			return new GEIEmailNotificationCreator(ediMessage, IncorrectTransactionDetails.FromBizo(invoiceBatch, new ZString[] { BatchErrorMessage }), IncorrectTransactionDetailsList, invoiceBatch.Company, logCollector);
		}

		string BatchErrorMessage
		{
			get
			{
				return string.IsNullOrWhiteSpace(batchErrorMessage) ? (NoResString)"Following invoices were failed." : batchErrorMessage;
			}
			set
			{
				batchErrorMessage = value;
			}
		}
		string batchErrorMessage;

		BusinessObjectFactory Factory => invoiceBatch.Factory;

		List<IncorrectTransactionDetails> IncorrectTransactionDetailsList { get; }

		struct ValidationDocument
		{
			public ValidationDocument(ZString validationDocumentContextValue)
			{
				ContextValue = validationDocumentContextValue;

				var splitInfos = validationDocumentContextValue.Split(ReadyKoreaConstants.ValidationDocumentStatusInfoSplitter);
				IssueId = splitInfos[0];
				StatusCode = splitInfos.Length > 0 ? splitInfos[1] : ZString.Empty;
				ErrorMessage = splitInfos.Length > 1 ? splitInfos[2] : ZString.Empty;
			}

			public ZString ContextValue { get; }
			public ZString IssueId { get; }
			public ZString StatusCode { get; }
			public ZString ErrorMessage { get; }
		}
	}
}
