using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public class TaxCoreEInvoicingEventMessageProcessor : EInvoicingEventMessageProcessor
	{
		public TaxCoreEInvoicingEventMessageProcessor(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch, ITaxCoreCountryEInvoicingResponseObjectFactory countryFactory)
			: base(logger, message, universalEvent, invoiceBatch)
		{
			CountryFactory = Argument.NotNull(countryFactory, nameof(countryFactory));
		}
		ITaxCoreCountryEInvoicingResponseObjectFactory CountryFactory { get; }

		List<InvoicingBase> Transactions
		{
			get
			{
				if (transactions == null && transactionPivot != null)
				{
					transactions = new List<InvoicingBase>();
					var transaction = invoiceBatch.Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, transactionPivot.AIP_ParentID));
					if (transaction != null)
					{
						transactions.Add(transaction);
					}
				}

				return transactions;
			}
		}
		List<InvoicingBase> transactions;
		AccEInvoicingTransactionPivot transactionPivot;

		public override void Process()
		{
			if (invoiceBatch.AIB_Status != Core.Constants.EInvoicingBatchState.Discarded)
			{
				transactionPivot = invoiceBatch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().FirstOrDefault();

				if (transactionPivot != null)
				{
					if (transactionPivot.AIP_Status != Core.Constants.EInvoicingPivotState.Succeed)
					{
						if (universalEvent.EventType.Value == AutoEvents.InterchangeAcknowledgedCode)
						{
							var messageSubType = universalEvent.EventParameters?.MessageSubType ?? ZString.Empty;
							if (messageSubType.IsEmpty)
							{
								ReportAndLogError("EmptyMessageSubType", Res.GetString("3029b75c-5a20-4ba2-ad93-186415549bbf", "No update performed due to universal event not containing message sub type for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
							}
							else
							{
								ProcessIAKEventMessage(messageSubType);
							}
						}
						else
						{
							ProcessIRJEventMessage();
						}
					}
					else
					{
						logger.LogBoth(LogType.Warning, Res.GetString("bdeff497-a255-4f0a-be38-78dc10f43743", "No update performed due to transaction pivot having 'SUC' status for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
					}
				}
				else
				{
					ReportAndLogError("PivotNotFound", Res.GetString("4a3bd231-08af-43e0-86d5-c2794324a5e5", "No update performed due to transaction pivot not found for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				}
			}
		}

		#region IRJ Event Message

		void ProcessIRJEventMessage(string fallbackReason = null)
		{
			var errorDescription = universalEvent.EventParameters?.Reason ?? fallbackReason ?? ZString.Empty;
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(Transactions.Select(x => x.PK), errorDescription, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
			SendErrorNotificationEmail(Res.GetString("1e9f63c2-0a34-44f5-be1b-ec0066073c69", "Message to {0} invoice authorization service was rejected for invoice batch {1} in {2} with the following reason: {3}", CountryFactory.CountryName, invoiceBatch.AIB_BatchNumber, companyName, universalEvent.EventParameters?.Reason ?? ZString.Empty));
		}

		#endregion

		#region IAK Event Message

		void ProcessIAKEventMessage(ZString messageType)
		{
			if (messageType == CountryFactory.ResponseMessageSubTypeCode)
			{
				ProcessInvoiceResponseMessage();
			}
			else
			{
				ReportAndLogError("UnknownMessageSubType", Res.GetString("d2aa393a-add5-4a59-9353-3518dd18aab4", "No update performed due to invalid message sub type [{0}] found for invoice batch {1} in {2}.", messageType, invoiceBatch.AIB_BatchNumber, companyName));
			}
		}

		void ProcessInvoiceResponseMessage()
		{
			if (universalEvent.ContextCollection != null)
			{
				var responseContext = universalEvent.ContextCollection.FirstOrDefault(x => x.Type == CountryFactory.ResponseMessageContextTypeCode);
				if (responseContext != null)
				{
					if (responseContext.Value.HasValue && !responseContext.Value.Value.IsEmpty)
					{
						ProcessInvoiceResponse(responseContext.Value.Value);
						AttachResponseMessageToeDocs();
					}
					else
					{
						ReportAndLogError("ResponseMessageContextHasNoValue", Res.GetString("87586912-6f5e-4c5b-896c-1a4bb80d85f6", "{0} Invoice Response was not processed due to Response Message Context value was empty for invoice batch {1} in {2}.", CountryFactory.CountryName, invoiceBatch.AIB_BatchNumber, companyName));
					}
				}
				else
				{
					ReportAndLogError("MissingResponseMessageContext", Res.GetString("be8690b6-90d2-42e5-b113-d35182c8ef2b", "{0} Invoice Response was not processed due to context collection not having Response Message Context for invoice batch {1} in {2}.", CountryFactory.CountryName, invoiceBatch.AIB_BatchNumber, companyName));
				}
			}
			else
			{
				ReportAndLogError("MissingContextCollection", Res.GetString("c7bce4fb-ecc5-4790-b14b-c9726ac7a727", "{0} Invoice Response was not processed due to universal event message not having context collection for invoice batch {1} in {2}.", CountryFactory.CountryName, invoiceBatch.AIB_BatchNumber, companyName));
			}
			transactionPivot.AIP_LastResponseReceivedUtc = LastResponseReceivedTime;
		}

		void ProcessInvoiceResponse(ZString response)
		{
			var eInvoiceResponse = ReadResponse(response);
			if (eInvoiceResponse != null)
			{
				var transaction = Transactions.FirstOrDefault();
				if (transaction != null)
				{
					var authorisationRecordCreator = CountryFactory.GetAuthorisationRecordCreator();
					authorisationRecordCreator.Create(invoiceBatch.Factory, eInvoiceResponse, transaction.PK);

					var checker = CountryFactory.GetEInvoiceChecker();
					if (checker.CheckAndApplyAuthorisation(invoiceBatch, eInvoiceResponse))
					{
						invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(Transactions.Select(x => x.PK), universalEvent.EventParameters?.Reason ?? ZString.Empty, EInvoicingPivotState.Succeed, lastResponseReceivedUtc: LastResponseReceivedTime); // TODO: Which status should we set is based on the processing results
					}
					else
					{
						invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(Transactions.Select(x => x.PK), universalEvent.EventParameters?.Reason ?? ZString.Empty, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
						ReportAndLogError("TaxCoreAuthorisationFailed", Res.GetString("f10a4af3-58b3-471a-86ad-19578dfe717b", "{0} government authorization failed for transaction {1} {2} {3} in {4}", CountryFactory.CountryName, transaction.AH_Ledger, transaction.AH_TransactionType, transaction.AH_TransactionNum, companyName));
					}
				}
				else
				{
					ReportAndLogError("TransactionNotFound", Res.GetString("01a89109-12e0-4e65-80e3-891b34af91ff", "Response Message was not processed due to related transaction was not found for pivot in invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				}
			}
			else
			{
				ProcessIRJEventMessage(Res.GetString("32991ad3-89d0-4dc8-b029-ea827f93fe50", "Unreadable response from web service"));
			}
		}

		TaxCoreEInvoiceResponse ReadResponse(ZString jsonResponse)
		{
			TaxCoreEInvoiceResponse eInvoiceResponse = null;
			try
			{
				var reader = CountryFactory.GetEInvoiceResponseReader();
				eInvoiceResponse = reader.Read(jsonResponse);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.LogBoth(LogType.Warning, Res.GetString("5cd51cf8-de8e-4f47-9482-a6c50eb1263e", "Reading {0} ended with an exception:\r\nException Type: {1}.\r\nException Details: {2}.", CountryFactory.ResponseName, ex.GetType(), ex.Message));
			}
			return eInvoiceResponse;
		}

		#endregion

		#region Helpers

		protected override bool ShouldSendEmail => !Transactions.IsNullOrEmpty();

		protected override string LogErrorNotFoundKey => "TransactionNotFound";

		protected override GEIEmailNotificationCreator GetEmailNotificationCreator(ZString errorMessage, ILogger logCollector)
		{
			return new GEIEmailNotificationCreator(ediMessage, Transactions.First(), new List<ZString>() { errorMessage }, logCollector);
		}

		void AttachResponseMessageToeDocs()
		{
			var responseMessageContext = universalEvent.ContextCollection.FirstOrDefault(x => x.Type == CountryFactory.ResponseMessageContextTypeCode);
			if (responseMessageContext != null)
			{
				if (responseMessageContext.Value.HasValue && !responseMessageContext.Value.Value.IsEmpty)
				{
					var transaction = Transactions.FirstOrDefault();
					if (transaction != null)
					{
						var fileName = universalEvent.EventParameters.MessageSubType.Value + "_" + ZDateTime.UtcNow.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture) + ".json";
						transaction.DocManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes(responseMessageContext.Value.Value.ToUTF8FromBase64()), fileName, Core.Constants.ReferenceTypes.Accounting); // TODO: What is proper encoding? 
						transaction.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
					}
					else
					{
						ReportAndLogError(LogErrorNotFoundKey, Res.GetString("452305bd-b74f-48a7-8c9d-c47b5ee90052", "{0} Response Message was not attached to eDocs due to related transaction was not found for pivot in invoice batch {1} in {2}.", CountryFactory.CountryName, invoiceBatch.AIB_BatchNumber, companyName));
					}
				}
				else
				{
					logger.LogBoth(LogType.Warning, Res.GetString("d89b8d64-49ef-47ff-b242-926a6cf47bc0", "{0} Response Message was not attached to eDocs due to Response Message Context value was empty for invoice batch {1} in {2}.", CountryFactory.CountryName, invoiceBatch.AIB_BatchNumber, companyName));
				}
			}
			else
			{
				logger.LogBoth(LogType.Warning, Res.GetString("6bf28b74-163c-4905-9833-774bcebebb72", "{0} Response Message was not attached to eDocs due to context collection not having Response Message Context for invoice batch {1} in {2}.", CountryFactory.CountryName, invoiceBatch.AIB_BatchNumber, companyName));
			}
		}

		#endregion
	}
}
