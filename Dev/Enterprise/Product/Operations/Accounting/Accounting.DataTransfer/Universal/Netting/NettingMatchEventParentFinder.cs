using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Netting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Accounting.Business.AccountingConstants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.DataTransfer.Universal.Netting
{
	public class NettingMatchEventParentFinder : EventParentFinder
	{
		public NettingMatchEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			INettingTransaction originalTransaction = null;

			try
			{
				if (IsNettingMatchEvent(xmlEvent))
				{
					var ledger = HelperMethods.GetValueFromContextCollection(xmlEvent, InvoiceForNettingInfoList.Ledger, throwExceptionWhenEmpty: true);
					var transactionType = HelperMethods.GetValueFromContextCollection(xmlEvent, InvoiceForNettingInfoList.TransactionType, throwExceptionWhenEmpty: true);

					if (ledger == LedgerTypes.AccountsReceivable)
					{
						var transactionNum = HelperMethods.GetValueFromContextCollection(xmlEvent, InvoiceForNettingInfoList.Number, throwExceptionWhenEmpty: true);
						var initiatingEHubId = HelperMethods.GetValueFromContextCollection(xmlEvent, InvoiceForNettingInfoList.InitiatingEHubId, throwExceptionWhenEmpty: true);

						originalTransaction = NettingHelper.GetNettingReceivableTransaction(transactionType, transactionNum, initiatingEHubId, factory);
					}
					else if (ledger == LedgerTypes.AccountsPayable)
					{
						var internalReferenceNum = HelperMethods.GetValueFromContextCollection(xmlEvent, InvoiceForNettingInfoList.InternalReferenceNumber, throwExceptionWhenEmpty: true);
						var receivingEHubId = HelperMethods.GetValueFromContextCollection(xmlEvent, InvoiceForNettingInfoList.InitiatingEHubId, throwExceptionWhenEmpty: true); //The issuer of the AP invoice is the recipient of the NettingReceivableTransaction/NettingPayableTransaction

						originalTransaction = NettingHelper.GetNettingPayableTransaction(transactionType, internalReferenceNum, receivingEHubId, factory);
					}
					else
					{
						throw new ArgumentException("Unsupported transaction");
					}

					if (originalTransaction == null)
					{
						var transactionNumber = HelperMethods.GetValueFromContextCollection(xmlEvent, InvoiceForNettingInfoList.Number, throwExceptionWhenEmpty: false);
						var internalReferenceNumber = HelperMethods.GetValueFromContextCollection(xmlEvent, InvoiceForNettingInfoList.InternalReferenceNumber, throwExceptionWhenEmpty: false);
						var transactionStatus = HelperMethods.GetValueFromContextCollection(xmlEvent, InvoiceForNettingInfoList.Status, throwExceptionWhenEmpty: false);
						var initiatingEHubId = HelperMethods.GetValueFromContextCollection(xmlEvent, InvoiceForNettingInfoList.InitiatingEHubId, throwExceptionWhenEmpty: false);
						var receivingEHubId = HelperMethods.GetValueFromContextCollection(xmlEvent, InvoiceForNettingInfoList.ReceivingEHubId, throwExceptionWhenEmpty: false);

						var logMessage = Res.GetString("10D53CF7-45AE-4D03-8C0B-A7243811C568",
							"Transaction not found in Netting to update.\r\nTransaction Number: {0}\r\nInternal Reference Number: {1}\r\nTransaction Status: {2}\r\nLedger: {3}\r\nTransaction Type: {4}\r\nSender: {5}\r\nReceiver: {6}",
							transactionNumber, internalReferenceNumber, transactionStatus, ledger, transactionType, initiatingEHubId, receivingEHubId);

						logger.Log(Enterprise.Integration.LogType.Warning, logMessage);
					}
				}
			}
			catch (Exception ex) when (ex is IncorrectDataSetupException || ex is ArgumentException)
			{
				logger.Log(Enterprise.Integration.LogType.Error, ex.Message);
				throw;
			}

			return originalTransaction != null ? new BusinessObject[] { originalTransaction as BusinessObject } : null;
		}

		static bool IsNettingMatchEvent(UniversalEvent xmlEvent)
		{
			return !string.IsNullOrWhiteSpace(HelperMethods.GetValueFromContextCollection(xmlEvent, InvoiceForNettingInfoList.Status));
		}
	}
}
