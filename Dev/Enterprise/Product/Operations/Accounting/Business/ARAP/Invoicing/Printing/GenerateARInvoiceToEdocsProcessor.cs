using System;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing
{
	public class GenerateARInvoiceToEdocsProcessor : IProcessor
	{
		public GenerateARInvoiceToEdocsProcessor(InvoicingBase invoice)
		{
			this.invoice = invoice;
		}

		readonly InvoicingBase invoice;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "StmALog reference values should not be translated")]
		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			try
			{
				notifications.Add(NotificationSubscriberType.Info, (String.Format(CultureInfo.InvariantCulture, (NoResString)"Started attaching AR {0} {1} to EDocs Tab.", invoice.AH_TransactionType, invoice.AH_TransactionNum)));
				if (InvoicePrintTask.AttachARInvoiceToEdocs(invoice, notifications, token))
				{
					notifications.Add(NotificationSubscriberType.Info, (String.Format(CultureInfo.InvariantCulture, (NoResString)"Successfully attached AR {0} {1} to EDocs Tab.", invoice.AH_TransactionType, invoice.AH_TransactionNum)));
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				notifications.AddWarning(String.Format(CultureInfo.InvariantCulture, "Not able to generate PDF document due to {0}. AR {1} {2} was not attached to EDocs Tab.", e.Message, invoice.AH_TransactionType, invoice.AH_TransactionNum));
			}
		}
	}
}
