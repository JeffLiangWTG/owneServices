using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing
{
	public class GovtComplianceInvoicePrintManager
	{
		public GovtComplianceInvoicePrintManager()
		{ }

#if DEBUG
		internal PrintTask lastPrintTaskForTesting;
		internal List<ZGuid> transactionsSentToPrinterForTesting;
#endif
		HashSet<ZGuid> transactionsWithoutPrinterQueue;

		public PrintTask PrintTaskForInvoicesWithoutPrinterSetup
		{
			get;
			private set;
		}

		public ZString LastGovernmentPrintTaskErrorMessage
		{
			get;
			private set;
		}

		public void PrintGovtComplianceInvoices(TransactionHeader[] transactions, ZString invoicePrintingOptionCode)
		{
			Dictionary<ZGuid, DocumentPack> orgDocPackHash = new Dictionary<ZGuid, DocumentPack>();
			List<ZGuid> transactionsWithPrinterAndPrinted = new List<ZGuid>();
			transactionsWithoutPrinterQueue = new HashSet<ZGuid>();
			PrintTaskForInvoicesWithoutPrinterSetup = null;

			foreach (TransactionHeader invoice in transactions)
			{
				bool sendDirectlyToPrinter = false;
				ZGuid printerPK = invoice.ComplianceSequencePrinterPK;

				// Always print Govt tax invoice
				GovtTaxInvoicePrintTask govtPrintTask = null;

				if (invoicePrintingOptionCode.IsEmpty)
				{
					invoicePrintingOptionCode = AccountingConfigurationRegistry.Instance.InvoicePrintingOption.Value;
				}

				var notifications = printerPK != ZGuid.Empty ? new NotificationBuffer() : null;

				try
				{
					if (invoicePrintingOptionCode == GovtTaxInvoicePrintTask.GovtTaxInvoice || invoicePrintingOptionCode == GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice)
					{
						sendDirectlyToPrinter = true;
						govtPrintTask = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(invoice.PK) { InvoicePrintingOptionCode = GovtTaxInvoicePrintTask.GovtTaxInvoice, CustomNotifications = notifications });
					}
					else if (invoicePrintingOptionCode == GovtTaxInvoicePrintTask.EnterpriseInvoice)
					{
						govtPrintTask = new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(invoice.PK) { InvoicePrintingOptionCode = GovtTaxInvoicePrintTask.EnterpriseInvoice, CustomNotifications = notifications });
					}
				}
				catch (ComplianceSequenceRelatedException ex)
				{
					LastGovernmentPrintTaskErrorMessage = ex.UserFriendlyMessage;
					continue;
				}

				if (invoicePrintingOptionCode != GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly && govtPrintTask.TaskCount > 0)
				{
					if (printerPK != ZGuid.Empty && sendDirectlyToPrinter)
					{
						SendGovtInvoiceDirectlyToPrinter(govtPrintTask, printerPK);

						if (notifications.HasErrors || notifications.HasWarnings)
						{
							throw new ZCannotSaveException(notifications.AsString, Res.GetString("2097a2f7-3676-47b6-ae38-3e3b0de51f4d", "Report Error"));
						}

						transactionsWithPrinterAndPrinted.Add(invoice.PK);
					}
					else // merge docpack based on org
					{
						transactionsWithoutPrinterQueue.Add(invoice.PK);
						DocumentPack pack = govtPrintTask.GetInvoicePacksForComplianceInvoiceManagerOnly((InvoicingBase)invoice).First();
						DocumentPack documentPackForOrg;
						if (orgDocPackHash.TryGetValue(invoice.AH_OH, out documentPackForOrg))
						{
							documentPackForOrg.AddRange(pack);
							documentPackForOrg.BuildConsolidatedEmailSubject(invoice.Header, ++documentPackForOrg.NumberOfDocumentNeedToBeConsolidated);
						}
						else
						{
							pack.NumberOfDocumentNeedToBeConsolidated = 1;
							orgDocPackHash.Add(invoice.AH_OH, pack);
						}
					}
				}

				// Add standard invoice if option code is BOTH
				if (invoicePrintingOptionCode == GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice)
				{
					transactionsWithoutPrinterQueue.Add(invoice.PK);
					InvoicePrintTask standardPrintTask = new InvoicePrintTask(new InvoicePrintTask.Configuration(invoice.PK));
					DocumentPack pack = standardPrintTask.GetInvoicePacksForComplianceInvoiceManagerOnly((InvoicingBase)invoice).First();
					DocumentPack documentPackForOrg;
					if (orgDocPackHash.TryGetValue(invoice.AH_OH, out documentPackForOrg))
					{
						documentPackForOrg.AddRange(pack);
						documentPackForOrg.BuildConsolidatedEmailSubject(invoice.Header, ++documentPackForOrg.NumberOfDocumentNeedToBeConsolidated);
					}
					else
					{
						pack.NumberOfDocumentNeedToBeConsolidated = 1;
						orgDocPackHash.Add(invoice.AH_OH, pack);
					}
				}
			}

			// merge and print all govt invoices which has no printer defined + standard invoices
			if (orgDocPackHash.Count > 0)
			{
				PrintTaskForInvoicesWithoutPrinterSetup = new PrintTask();
				PrintTaskForInvoicesWithoutPrinterSetup.DeliveryInstructionsDefaultPK = ClassAInvoicePreprintedMenuPK;
				PrintTaskForInvoicesWithoutPrinterSetup.AddRange(orgDocPackHash.Values);

#if DEBUG
				lastPrintTaskForTesting = PrintTaskForInvoicesWithoutPrinterSetup;
#endif
			}

#if DEBUG
			transactionsSentToPrinterForTesting = transactionsWithPrinterAndPrinted;
#endif

			// update the IsPrinted flag after printing to printer
			UpdateTransactionsAsPrinted(transactionsWithPrinterAndPrinted);
		}

		internal ZGuid ClassAInvoicePreprintedMenuPK
		{
			get { return new ZGuid("C27EDC55-EE66-4DE5-BBB2-B6BE131DDFBA"); }
		}

		void SendGovtInvoiceDirectlyToPrinter(GovtTaxInvoicePrintTask task, ZGuid printerPK)
		{
			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.Destination = DeliveryInstructionDestination.Print;
			instructions.PrinterDelivery.PrintQueuePK = printerPK;
			task.RunTaskWithInstructions(instructions);
		}

		void UpdateTransactionsAsPrinted(List<ZGuid> transactions)
		{
			BusinessObjectFactory factoryForUpdateOfPrintedFlag = new BusinessObjectFactory();
			var transactionsToUpdateInNewFactory = factoryForUpdateOfPrintedFlag.Load<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.PK, transactions.ToArray()));
			foreach (TransactionHeader transaction in transactionsToUpdateInNewFactory)
			{
				transaction.AH_InvoicePrinted = true;
			}
			factoryForUpdateOfPrintedFlag.Save();
		}

		public void UpdateTransactionsWithoutPrinterQueueAsPrinted()
		{
			UpdateTransactionsAsPrinted(transactionsWithoutPrinterQueue.ToList());
		}
	}
}
