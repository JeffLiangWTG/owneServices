using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance
{
	public class MaintenanceBillRecipient : BillRecipient
	{
		public MaintenanceBillRecipient(BusinessObjectFactory factory,
			ZGuid branchPK,
			EDIOrgHeader org,
			LicenceCompany licCompany,
			ClientLicenceBilling licBilling,
			ZDateTime dueDate,
			ZString currencyCode,
			ZDecimal localExchangeRate,
			ZDateTime dateForExchangeRate,
			EDIOrgHeader invoicingPartner)
			: base(factory, branchPK, org, licCompany, licBilling, currencyCode, localExchangeRate, dateForExchangeRate, invoicingPartner)
		{
			DueDate = dueDate;
		}

		public MaintenanceBillRecipient(BusinessObjectFactory factory, ZGuid branchPK, ZGuid orgPK, ZString currencyCode, ZDateTime dueDate, ZDateTime dateForExchangeRate)
			: base(factory, branchPK, orgPK, currencyCode, dateForExchangeRate)
		{
			DueDate = dueDate;
		}

		public MaintenanceBillRecipient(BusinessObjectFactory factory, ZGuid branchPK, ZGuid orgPK, ZString currencyCode, ZDateTime dateForExchangeRate)
			: this(factory, branchPK, orgPK, currencyCode, ZDateTime.Empty, dateForExchangeRate)
		{
		}

		public ZDateTime DueDate { get; private set; }

		public ZBool CanInvoice
		{
			get
			{
				return (Fees.Count > 0 || Bills.Count > 0)
					&& !Bills.Cast<MaintenanceBill>().Any(s => !s.CanInvoice)
					&& !Fees.Cast<FeeDelivery>().Any(s => !s.CanInvoice);
			}
		}

		public ZBool CanForceInvoice
		{
			get
			{
				return (Fees.Count > 0 || Bills.Count > 0)
					&& (Bills.Cast<MaintenanceBill>().Any(s => s.CanForceInvoice) || Fees.Cast<FeeDelivery>().Any(s => s.CanForceInvoice))
					&& (Bills.Cast<MaintenanceBill>().All(s => s.CanInvoice || s.CanForceInvoice))
					&& (Fees.Cast<FeeDelivery>().All(s => s.CanInvoice || s.CanForceInvoice));
			}
		}

		public IEnumerable<ARInvoice> Invoices =>
				Bills.OfType<MaintenanceBill>().Where(x => x.IsInvoiced).Select(x => x.Invoice)
					.Concat(Fees.OfType<FeeDelivery>().Where(x => x.IsInvoiced).Select(x => x.Invoice))
						.Distinct().OrderBy(x => x.AH_TransactionNum).ToArray();

		public ZString InvoiceNumbers => ZString.Join(", ", Invoices.Select(x => x.AH_TransactionNum).OrderBy(x => x).ToArray());

		public ZPropertyInfo InvoiceNumbersInfo => GetZPropertyInfo(nameof(InvoiceNumbers));

		public ZString StatusText
		{
			get
			{
				var feeStatus = Fees.Cast<FeeDelivery>().Select(s => s.StatusText.ToString()).Distinct();
				var billStatus = Bills.Cast<MaintenanceBill>().Select(s => s.StatusText.ToString()).Distinct();
				string status = string.Join(",", feeStatus.Union(billStatus).OrderBy(s => s).ToArray());
				return !string.IsNullOrEmpty(status) ? status : MaintenanceBill.StatusMessages.Ready;
			}
		}

		public ZPropertyInfo StatusTextInfo
		{
			get { return this.GetZPropertyInfo(nameof(StatusText)); }
		}

		/// <summary>
		/// If true, the invoice shows the amount for each module.
		/// If false, the invoice just shows the single total amount.
		/// </summary>
		public ZBool ShowPerModuleAmounts
		{
			get { return showPerModuleAmounts; }
			set { SetNonPersistentPropertyValue(ShowPerModuleAmountsInfo, ref showPerModuleAmounts, value); }
		}
		ZBool showPerModuleAmounts;

		public ZPropertyInfo ShowPerModuleAmountsInfo
		{
			get { return GetZPropertyInfo(nameof(ShowPerModuleAmounts)); }
		}

		public ZInt LicenceCount
		{
			get { return Bills.Count; }
		}

		public ZInt FeeCount
		{
			get { return Fees.Count; }
		}

		public ZDecimal TotalAmount { get { return TotalMaintenance + TotalFees; } }

		public ZDecimal TotalMaintenance { get { return totalMaintenance; } }
		ZDecimal totalMaintenance;

		public ZDecimal TotalFees { get { return totalFees; } }
		ZDecimal totalFees;

		public MaintenanceBill AddNewBill(LicenceHeader licHeader, ClientInvoiceDelivery invoiceDelivery, IPerDatabaseChargeDecider decider = null)
		{
			MaintenanceBill result = new MaintenanceBill(licHeader, this, invoiceDelivery, decider);
			Bills.Add(result);
			totalMaintenance += BillingInvoicingHelper.GetAmountInInvoiceCurrency(result.Maintenance + result.SurchargeAmount,
						DateForExchangeRate,
						result.PriceCurrencyCode,
						InvoiceCurrencyCode,
						InvoicingBranch,
						Factory);
			return result;
		}

		public void AddNewFee(ClientLicenceFee fee, ZDateTime feeDate, ClientInvoiceDelivery invoiceDelivery, IEnumerable<ClientChargeableUsage> usages)
		{
			FeeDelivery result = new FeeDelivery(Factory, this, fee, feeDate, invoiceDelivery, usages);
			totalFees += BillingInvoicingHelper.GetAmountInInvoiceCurrency(result.Fee.L8_Amount,
						DateForExchangeRate,
						result.PriceCurrencyCode,
						InvoiceCurrencyCode,
						InvoicingBranch,
						Factory);
			Fees.Add(result);
		}

		public virtual IEnumerable<ARInvoice> CreateInvoices(KeyValuePair<string, byte[]>[] attachments, IUSSalesTaxCalculator usSalesTaxCalculator = null)
		{
			if (!BranchPK.IsEmpty && BranchPK != Env.CurrentBranch.PK)
			{
				throw new InvalidOperationException("Current branch must be set to invoicing branch");
			}

			var invoice = CreateInvoiceForBranch(InvoicingBranch, ZDateTime.Empty, usSalesTaxCalculator: usSalesTaxCalculator);
			InvoiceUpdateNotifiers.Clear();
			PopulateMaintenance(invoice);
			PopulateFees(invoice);
			PopulateExchangeRates(invoice);
			var invoices = ProcessTax(invoice);

			if (attachments != null && attachments.Length != 0)
			{
				AddAttachments(invoice, attachments);
				BusinessObjectFactory.SaveTogether(invoice.Factory, invoice.DocManagerInfo.MasterFactory);
			}
			else
			{
				invoice.Factory.Save();
			}

			RefreshBinding();
			return invoices;
		}

		void PopulateMaintenance(ARInvoice invoice)
		{
			MaintenanceBill[] billGroup = Bills.Cast<MaintenanceBill>().OrderBy(s => s.OrgCode).ThenBy(s => s.LicHeader.Database.LD_LicenceType).ToArray();
			if (billGroup.Length > 0)
			{
				invoice.AH_Desc = MaintenanceBill.GetInvoiceDescription(billGroup);
				foreach (MaintenanceBill bill in billGroup)
				{
					if (invoice.Lines.Count > 0)
					{
						BillingInvoicingHelper.AddCommentLine(invoice, ".");
					}

					using (GetInvoiceUpdateNotifierAction(invoice, bill))
					{
						bill.PopulateInvoice(invoice);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		void PopulateFees(ARInvoice invoice)
		{
			if (Fees.Count > 0)
			{
				foreach (var feeGroup in Fees.Cast<FeeDelivery>()
					.GroupBy(s => new { L8_Type = s.Fee.L8_Type, L8_RenewalMonths = s.Fee.L8_RenewalMonths })
					.OrderBy(s => s.Key.L8_Type).ThenBy(s => s.Key.L8_RenewalMonths))
				{
					string feeDescription = EDIDataRegistry.Instance.LicenceFeeTypes.Value.GetDescriptionFromCode(feeGroup.Key.L8_Type);
					const string dateFormat = "dd-MMM-yyyy"; // Finance team want this format
					string dateText = DueDate.ToString(dateFormat, CultureInfo.InvariantCulture) + " to " +
						DueDate.AddMonths(feeGroup.Key.L8_RenewalMonths).AddDays(-1).ToString(dateFormat, CultureInfo.InvariantCulture);

					if (invoice.Lines.Count == 0)
					{
						invoice.AH_Desc = feeDescription + " - " + DueDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
					}

					foreach (var fee in feeGroup.OrderBy(s => s.Fee.Company.Header.OH_Code).ThenBy(s => s.Fee.L8_Order))
					{
						using (GetInvoiceUpdateNotifierAction(invoice, fee))
						{
							fee.PopulateInvoice(invoice, feeDescription + " " + fee.Fee.L8_DescriptionMultilingual + " [" + dateText + "]");
						}
					}
				}
			}
		}

		void PopulateExchangeRates(ARInvoice invoice)
		{
			Dictionary<ZString, ZDecimal> exchangeRates = new Dictionary<ZString, ZDecimal>();
			AddRates(Bills, exchangeRates);
			AddRates(Fees, exchangeRates);

			if (exchangeRates.Count > 0)
			{
				foreach (ZString currencyCode in exchangeRates.Keys.OrderBy(s => s))
				{
					string exchangeNote = string.Format(CultureInfo.InvariantCulture, "Exchange rate: 1 {0} = {1} {2}",
						currencyCode,
						exchangeRates[currencyCode].ToString(BillingConstants.ExchangeRateDecimalFormat, CultureInfo.InvariantCulture),
						InvoiceCurrencyCode);
					BillingInvoicingHelper.AddCommentLine(invoice, exchangeNote);
				}
			}
		}

		void AddRates(BusinessObjectCollection collection, Dictionary<ZString, ZDecimal> exchangeRates)
		{
			foreach (MaintenanceCharge charge in collection)
			{
				if (string.Compare(charge.PriceCurrencyCode, InvoiceCurrencyCode, StringComparison.OrdinalIgnoreCase) != 0 && !exchangeRates.ContainsKey(charge.PriceCurrencyCode))
				{
					exchangeRates.Add(charge.PriceCurrencyCode, charge.ExchangeRate);
				}
			}
		}

		void AddAttachments(IDocManagerSupport owner, KeyValuePair<string, byte[]>[] attachments)
		{
			foreach (var item in attachments)
			{
				owner.DocManagerInfo.AddFileOrDocument(item.Value, item.Key, EDIDataRegistry.Instance.InvoiceAttachmentDocType.Value);
			}
		}

		#region Bills

		public MaintenanceBillCollection Bills
		{
			get
			{
				if (bills == null)
				{
					bills = new MaintenanceBillCollection(Factory);
					RegisterEditableChildObject(fees);
				}
				return bills;
			}
		}
		MaintenanceBillCollection bills;

		#endregion

		#region Fees

		public FeeDeliveryCollection Fees
		{
			get
			{
				if (fees == null)
				{
					fees = new FeeDeliveryCollection(Factory);
					RegisterEditableChildObject(fees);
				}
				return fees;
			}
		}
		FeeDeliveryCollection fees;

		#endregion

		#region Accounting.TaxFramework 

		IEnumerable<ARInvoice> ProcessTax(ARInvoice invoice)
		{
			var processor = GetInvoiceTaxProcessor();
			processor.OnInvoiceSplit += (_, e) =>
			{
				foreach (var line in e.InvoiceLines)
				{
					if (InvoiceUpdateNotifiers.TryGetValue(line, out var notifier))
					{
						notifier.OnInvoiceChanged(e.NewInvoice);
					}
				}
			};
			return processor.Process(invoice);
		}

		DisposableAction GetInvoiceUpdateNotifierAction(ARInvoice invoice, IInvoiceUpdateNotifier updater)
		{
			var lines = invoice.Lines.ToArray<ARInvoiceLine>();
			return new DisposableAction(() =>
			{
				var newLines = invoice.Lines.ToArray<ARInvoiceLine>()
					.Except(lines).Where(x => !x.IsCommentCharge).ToArray();
				foreach (var line in newLines)
				{
					InvoiceUpdateNotifiers.Add(line, updater);
				}
			});
		}

		readonly Dictionary<ARInvoiceLine, IInvoiceUpdateNotifier> InvoiceUpdateNotifiers = new Dictionary<ARInvoiceLine, IInvoiceUpdateNotifier>();

		protected virtual EDIARInvoiceTaxProcessor GetInvoiceTaxProcessor() => new EDIARInvoiceTaxProcessor();

		#endregion
	}
}

