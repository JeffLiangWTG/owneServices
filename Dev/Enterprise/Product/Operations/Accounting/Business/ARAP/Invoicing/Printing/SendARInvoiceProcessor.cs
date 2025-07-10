using System;
using System.Globalization;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Accounting.Business.ARAP.Invoicing.Printing.InvoicePrintTask;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing
{
	class SendARInvoiceProcessor : IProcessor
	{
		public SendARInvoiceProcessor(InvoicingBase invoice)
		{
			this.invoice = invoice;
		}

		readonly InvoicingBase invoice;

		public void Process(INotifications notifications, CancellationToken unused
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			var canReprint = invoice.CheckCanPrintPostedInvoicingBase();
			if (!canReprint.Result)
			{
				notifications.AddWarning(String.Format(CultureInfo.InvariantCulture, (NoResString)"{0} {1} {2} was not sent. Error Details: {3}", invoice.AH_Ledger, invoice.AH_TransactionType, invoice.AH_TransactionNum, canReprint.ReasonForNotBeingAbleToPrint));
			}
			else
			{
				using (var task = new InvoicePrintTask(new Configuration(invoice) { Factory = invoice.Factory }))
				{
					var runSilentlyEmailOnlyResult = task.RunSilentlyEmailOnly(false);

					switch (runSilentlyEmailOnlyResult)
					{
						case RunTaskResult.Success:
							notifications.Add(NotificationSubscriberType.Info, (String.Format(CultureInfo.InvariantCulture, (NoResString)"AR {0} {1} was successfully sent", invoice.AH_TransactionType, invoice.AH_TransactionNum)));
							break;
						case RunTaskResult.NoDocumentPack:
							var errorMessage = @$"Unable to find AR {invoice.AH_TransactionType} {invoice.AH_TransactionNum} while running LWK Service Task.
This could be caused by incorrect settings of the workflow trigger’s running company.";
							notifications.AddWarning(errorMessage);
							break;
						default:
							notifications.AddWarning(String.Format(CultureInfo.InvariantCulture, (NoResString)"Not able to find contact information for {0}. AR {1} {2} was not sent", invoice.Header.OH_Code, invoice.AH_TransactionType, invoice.AH_TransactionNum));
							break;
					}
				}
			}
		}
	}
}
