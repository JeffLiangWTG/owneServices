using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance
{
	public abstract class MaintenanceCharge : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected MaintenanceCharge(BusinessObjectFactory factory, MaintenanceBillRecipient billRecipient, ClientInvoiceDelivery invoiceDelivery)
			: base(factory)
		{
			using (SuspendSettingHasChanges())
			{
				Recipient = billRecipient;
				InvoiceDelivery = invoiceDelivery;
				IsBilled = invoiceDelivery == null || invoiceDelivery.L9_IsBilled;
			}
		}

		public MaintenanceBillRecipient Recipient { get; private set; }
		public abstract ZString PriceCurrencyCode { get; }
		public ZDecimal ExchangeRate { get; private set; }
		public ZBool IsBilled { get; private set; }

		protected readonly ClientInvoiceDelivery InvoiceDelivery;

		protected void CalculateExchangeRate()
		{
			ExchangeRate = 0;
			if (Recipient != null && Recipient.InvoicingBranch != null)
			{
				ExchangeRate = BillingInvoicingHelper.GetExchangeRate(Recipient.DateForExchangeRate, PriceCurrencyCode, Recipient.InvoiceCurrencyCode, Recipient.InvoicingBranch);
			}
		}

		protected void AddCurrencyExchangeLine(ARInvoice invoice, ZDecimal amount)
		{
			if (string.Compare(PriceCurrencyCode, Recipient.InvoiceCurrencyCode, StringComparison.OrdinalIgnoreCase) != 0)
			{
				string exchangeNote = string.Format(CultureInfo.InvariantCulture, "Original currency amount {0} {1}",
					amount.ToString(BillingConstants.AmountDecimalFormat, CultureInfo.InvariantCulture),
					PriceCurrencyCode);
				BillingInvoicingHelper.AddCommentLine(invoice, exchangeNote);
			}
		}

		protected void AddRowNotifications(BusinessObject notificationOwner)
		{
			AddNotifications(notificationOwner.AddRowNotification, CargoWise.EntityFramework.NotificationType.MessageError);
		}

		protected void AddNotifications(INotifications notificationOwner)
		{
			AddNotifications(notificationOwner.Add, CargoWise.EntityFramework.NotificationType.MessageError);
		}

		delegate void NotificationAdder(INotification notification);

		static void AddNotif(NotificationAdder addNotification, INotificationType notification, string message)
		{
			addNotification(new Notification(notification, message));
		}

		void AddNotifications(NotificationAdder addNotification, INotificationType notification)
		{
			if (InvoiceDelivery == null)
			{
				AddNotif(addNotification, notification, "No Invoicing Delivery Instructions found");
			}
			else if (!IsBilled)
			{
				AddNotif(addNotification, notification, "Not billable");
			}
			else if (Recipient != null && Recipient.IsInvoicedByPartner)
			{
				AddNotif(addNotification, notification, "Partner billing not implemented");
			}
			else if (ExchangeRate == 0m && !PriceCurrencyCode.IsEmpty && InvoiceDelivery.L9_RX_NKInvoiceCurrency != PriceCurrencyCode)
			{
				AddNotif(addNotification, notification, "Exchange rate for today not found.");
			}
		}
	}
}

