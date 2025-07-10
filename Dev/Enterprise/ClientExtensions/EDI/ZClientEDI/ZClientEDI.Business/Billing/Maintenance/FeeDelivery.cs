using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance
{
	public class FeeDelivery : MaintenanceCharge, IInvoiceUpdateNotifier
	{
		public FeeDelivery(BusinessObjectFactory factory,
			MaintenanceBillRecipient billRecipient,
			ClientLicenceFee fee,
			ZDateTime feeDate,
			ClientInvoiceDelivery invoiceDelivery,
			IEnumerable<ClientChargeableUsage> usages)
			: base(factory, billRecipient, invoiceDelivery)
		{
			using (SuspendSettingHasChanges())
			{
				this.Fee = fee;
				FeeDate = feeDate;

				if (fee != null)
				{
					if (usages == null)
					{
						usages = LoadExistingUsages(factory, billRecipient.DueDate, fee.PK);
					}
					SetExistingUsages(usages);
					CalculateExchangeRate();
					base.AddRowNotifications(this);
				}
			}
		}

		public ClientLicenceFee Fee { get; }
		public ZDateTime FeeDate { get; }

		public ZBool CanInvoice
		{
			get { return !HasRowMessageErrors && !IsInvoiced; }
		}

		public ZPropertyInfo CanInvoiceInfo
		{
			get { return GetZPropertyInfo(nameof(CanInvoice)); }
		}

		public ZBool CanForceInvoice
		{
			get { return !HasRowMessageErrors && IsInvoiced; }
		}

		public override ZString PriceCurrencyCode
		{
			get { return Fee.L8_RX_NKCurrency; }
		}

		#region Status

		public ZString StatusText
		{
			get
			{
				ZString result = "";
				CargoWise.ComponentModel.INotificationType severity = GetHighestSeverityNotificationType();

				if (IsInvoiced)
				{
					result = StatusMessages.Invoiced;
				}
				else if (HasErrors)
				{
					result = StatusMessages.Error;
				}
				else if (severity != null && (severity.Severity == NotificationType.MessageError.Severity || severity.Severity == NotificationType.Error.Severity))
				{
					result = StatusMessages.Error;
				}
				else if (severity != null && severity.Severity == NotificationType.Warning.Severity)
				{
					result = StatusMessages.Warning;
				}
				else
				{
					result = StatusMessages.Ready;
				}

				return result;
			}
		}

		public ZPropertyInfo StatusTextInfo
		{
			get { return this.GetZPropertyInfo(nameof(StatusText)); }
		}

		internal class StatusMessages
		{
			public const string Error = "1 - Error";
			public const string Warning = "2 - Warning";
			public const string NotBilled = "3 - Not Billable";
			public const string Invoiced = "4 - Invoiced";
			public const string Ready = "5 - Ready";
		}

		#endregion

		#region IsInvoiced Tracking

		public ZBool IsInvoiced
		{
			get
			{
				return Invoice != null && !Invoice.IsCancelled;
			}
		}

		public ARInvoice Invoice { get; private set; }
		ZGuid chargeableUsagePk;

		void SetExistingUsages(IEnumerable<ClientChargeableUsage> existingUsages)
		{
			if (existingUsages != null && existingUsages.Any())
			{
				var usage = existingUsages.First();
				Invoice = usage.Invoice;
				chargeableUsagePk = usage.PK;

				if (existingUsages.Count() > 1)
				{
					AddRowMessageError("Multiple previous usages found");
				}
			}
			else
			{
				Invoice = null;
				chargeableUsagePk = ZGuid.Empty;
			}
		}

		static ClientChargeableUsage[] LoadExistingUsages(BusinessObjectFactory usageFactory, ZDateTime periodStart, ZGuid feePk)
		{
			ClientChargeableUsage[] result = null;
			if (!periodStart.IsEmpty)
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(JoinCondition.And, ClientChargeableUsageSchema.U1_PeriodStart, periodStart);
				query.AddToFilter(JoinCondition.And, ClientChargeableUsageSchema.U1_Parent, feePk);
				query.AddToFilter(ClientChargeableUsageSchema.U1_Code, BillingConstants.BillingSystem.Fee);

				result = usageFactory.Load<ClientChargeableUsage>(query);
			}
			return result;
		}

		void UpdateUsage(ARInvoice invoice)
		{
			chargeableUsagePk = Enterprise.Client.EDI.Billing.Fee.FeeBill.CreateOrUpdateChargeableUsage(invoice, Fee, chargeableUsagePk, Recipient.DueDate, Recipient.Organisation.PK == Fee.L8_OH_RemitToOrg).PK;
			Invoice = invoice;
		}

		#endregion

		public virtual void PopulateInvoice(ARInvoice invoice, string description = null)
		{
			AccTaxRate taxId = InvoiceDelivery != null ? InvoiceDelivery.TaxId : null;

			var line = BillingInvoicingHelper.AddAmountLine(invoice,
				Fee.L8_Amount,
				Recipient.DateForExchangeRate,
				Fee.L8_ChargeCode,
				Fee.L8_RX_NKCurrency,
				taxId,
				description ?? Fee.L8_DescriptionMultilingual);
			var taxDate = Fee.CalculateTaxDate(FeeDate);
			if (!taxDate.Date.IsEmpty)
			{
				line.AL_TaxDate = taxDate.Date;
			}

			AddCurrencyExchangeLine(invoice, Fee.L8_Amount);

			UpdateUsage(invoice);
			RefreshBinding();
		}

		void IInvoiceUpdateNotifier.OnInvoiceChanged(ARInvoice newInvoice)
		{
			if (Invoice != null && newInvoice != null && Invoice.PK != newInvoice.PK)
			{
				UpdateUsage(newInvoice);
				RefreshBinding();
			}
		}
	}
}

