using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public class KoreaSouthEDIInterchangeCreator : GlobalEDIInterchangeCreator
	{
		public KoreaSouthEDIInterchangeCreator(GlbCompany company, ICountryEInvoicingObjectFactory countryFactory) : base(company, countryFactory, ZString.Empty)
		{
		}

		protected override void PerformBeforeCreatingEDIMessageAndInterchange(TransactionBatchProcessContext batchProcessContext)
		{
			base.PerformBeforeCreatingEDIMessageAndInterchange(batchProcessContext);
			var geiProcessContext = batchProcessContext.As<GEIProcessContext>();
			if (geiProcessContext?.EInvoice?.Header?.ElectronicInvoiceBatchRequest != null)
			{
				geiProcessContext.EInvoice.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems = GetAdditionalHeaderDataItems(batchProcessContext);
			}
		}

		protected override void PerformAfterCreatingEDIMessageAndInterchangeSuccessfully(TransactionBatchProcessContext batchProcessContext)
		{
			base.PerformAfterCreatingEDIMessageAndInterchangeSuccessfully(batchProcessContext);

			if (batchProcessContext.Batch.AIB_Status == EInvoicingBatchState.Sent)
			{
				batchProcessContext.Batch.TransactionPivots[0].ParentTransactionHeader.AH_ComplianceDocumentDate = ZDate.Today;
			}
		}

		protected override void SendEmail(GEIProcessContext geiProcessContext, IEnumerable<ZString> errors)
		{
			if (errors.Count() > 1 || !LoggerWithGroupKey.TryDeserializeObject(errors.FirstOrDefault(), out var messagesWithKey))
			{
				base.SendEmail(geiProcessContext, errors);
				return;
			}

			var logger = geiProcessContext.Logger;
			logger.Log(LogType.Debug, "Attempting to send an email notification containing error details.");

			var transactionPKs = geiProcessContext.TransactionPKsReadyToSend;
			if (geiProcessContext.Interchanges?.Any() ?? false)
			{
				foreach (var interchange in geiProcessContext.Interchanges)
				{
					var messages = interchange.LoadMessages();
					foreach (var message in messages)
					{
						GetEmailNotificationCreator(message, geiProcessContext.Batch, transactionPKs, messagesWithKey, logger).SendEmail();
					}
				}
			}
			else
			{
				GetEmailNotificationCreator(ediMessage: null, geiProcessContext.Batch, transactionPKs, messagesWithKey, logger).SendEmail();
			}
		}

		GEIEmailNotificationCreator GetEmailNotificationCreator(EDIMessage ediMessage, AccEInvoicingBatch batch, IEnumerable<ZGuid> errorTransactionPKs, IDictionary<string, string[]> messagesWithKey, ILogger logger)
		{
			var batchErrorList = new List<ZString>();

			var factory = new BusinessObjectFactory();
			var errorTransactions = factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, errorTransactionPKs));
			var errorMessageMap = errorTransactions
				.ToDictionary(x => x.PK.ToString(), x => new List<ZString>());
			foreach (var kv in messagesWithKey)
			{
				if (errorMessageMap.ContainsKey(kv.Key))
				{
					errorMessageMap[kv.Key].AddRange(kv.Value.Select(x => (ZString)x));
				}
				else
				{
					var prefix = string.IsNullOrEmpty(kv.Key) ? "" : $"[{kv.Key}]";
					batchErrorList.AddRange(kv.Value.Select(message => (ZString)$"{prefix}{message}"));
				}
			}

			var incorrectSubjectBatchMessage = batchErrorList.Any()
				? batchErrorList.ToArray()
				: new ZString[] { (NoResString)"Following invoices were failed." };
			var incorrectSubjectBatch = IncorrectTransactionDetails.FromBizo(batch, incorrectSubjectBatchMessage);

			var incorrectTransactionDetails = errorTransactions
				.Select(x => IncorrectTransactionDetails.FromBizo(x, errorMessageMap[x.PK.ToString()]))
				.ToArray();

			return new GEIEmailNotificationCreator(
				ediMessage,
				incorrectSubjectBatch,
				incorrectTransactionDetails,
				CurrentCompany,
				logger
			);
		}

		GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection GetAdditionalHeaderDataItems(TransactionBatchProcessContext batchProcessContext)
		{
			var result = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection();
			var item = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItem
			{
				Key = ReadyKoreaConstants.SubmitID,
				Value = new KoreaSouthEInvoicingDataFinder().GetSubmitId(batchProcessContext.Batch)
			};
			result.Add(item);
			return result;
		}

		protected override IAccEInvoiceBatchToGEIConverter BatchToGEIConverter => new KoreaSouthTransactionBatchToGEIConverter(CountryFactory);
	}
}
