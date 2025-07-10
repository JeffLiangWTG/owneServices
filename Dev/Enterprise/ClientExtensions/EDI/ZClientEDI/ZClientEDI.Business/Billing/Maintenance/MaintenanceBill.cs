using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance
{
	public class MaintenanceBill : MaintenanceCharge, IInvoiceUpdateNotifier
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MaintenanceBill(BusinessObjectFactory factory, LicenceHeader licHeader, MaintenanceBillRecipient billRecipient, ClientInvoiceDelivery invoiceDelivery, IPerDatabaseChargeDecider decider = null)
			: base(factory, billRecipient, invoiceDelivery)
		{
			using (SuspendSettingHasChanges())
			{
				this.licHeader = licHeader;
				this.decider = decider;
				if (licHeader != null)
				{
					this.licCompany = licHeader.Company;
					this.org = licCompany.Header;

#pragma warning disable
					((IBusinessObjectState)licHeader).UpdatedByDataRefreshIncludingChildren += new EventHandler(OnLicenceHeaderUpdated);
					((IBusinessObjectState)licCompany).UpdatedByDataRefreshIncludingChildren += new EventHandler(OnLicenceCompanyUpdated);
					if (billRecipient.LicCompany != null && billRecipient.LicCompany.PK != licCompany.PK)
					{
						((IBusinessObjectState)billRecipient.LicCompany).UpdatedByDataRefreshIncludingChildren += new EventHandler(OnLicenceCompanyUpdated);
					}
#pragma warning restore
				}

				taxId = invoiceDelivery != null ? invoiceDelivery.TaxId : null;
				UpdateAll();
			}
		}

		public MaintenanceBill(LicenceHeader licHeader, MaintenanceBillRecipient billRecipient, ClientInvoiceDelivery invoiceDelivery, IPerDatabaseChargeDecider decider = null)
			: this(licHeader.Factory, licHeader, billRecipient, invoiceDelivery, decider)
		{
		}

		public MaintenanceBill(LicenceHeader licHeader, MaintenanceBillRecipient billRecipient)
			: this(licHeader, billRecipient, null)
		{
		}

		readonly LicenceHeader licHeader;
		readonly LicenceCompany licCompany;
		readonly EDIOrgHeader org;
		readonly AccTaxRate taxId;
		readonly IPerDatabaseChargeDecider decider;
		MaintenancePrices maintenancePrices;

		#region Properties

		public ZBool CanInvoice
		{
			get { return IsBilled && !IsInvoiced && !CachedHasErrorsOrChanges; }
		}

		public ZPropertyInfo CanInvoiceInfo
		{
			get { return GetZPropertyInfo(nameof(CanInvoice)); }
		}

		public ZBool CanForceInvoice
		{
			get { return IsBilled && IsInvoiced && !CachedHasErrorsOrChanges; }
		}

		ZBool CachedHasErrorsOrChanges
		{
			get { return cachedHasErrorsOrChanges ?? (cachedHasErrorsOrChanges = (HasErrors || HasMessageErrors || IsChanged)).Value; }
		}
		ZBool? cachedHasErrorsOrChanges;

		void InvalidateCanInvoice()
		{
			cachedHasErrorsOrChanges = null;
			CanInvoiceInfo.RefreshBinding();
		}

		public ZBool HasSaveError
		{
			get { return licHeader != null && LicHeader.HasErrors; }
		}

		public AccTaxRate TaxId
		{
			get { return taxId; }
		}

		public ZString OverallSalesRep { get; private set; }
		public ZString RelationshipManager { get; private set; }

		public ZBool IsLiveLessThanOneYear
		{
			get
			{
				return licHeader != null
					&& !licHeader.LA_AgreedLiveDate.IsEmpty
					&& !licHeader.LA_ContractExpiryDate.IsEmpty
					&& (licHeader.LA_ContractExpiryDate - licHeader.LA_AgreedLiveDate).TotalDays <= 366;
			}
		}

		#region Status

		public ZString StatusText
		{
			get
			{
				ZString result = "";
				CargoWise.ComponentModel.INotificationType severity = StatusTextInfo.GetHighestSeverityNotificationType();

				if (IsInvoiced)
				{
					result = StatusMessages.Invoiced;
				}
				else if (!IsBilled)
				{
					result = StatusMessages.NotBilled;
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

		public ZBool IsChanged
		{
			get { return licHeader != null && licHeader.HasChanges; }
		}

		public ARInvoice Invoice { get; private set; }

		public ZBool IsInvoiced
		{
			get
			{
				return licHeader != null
					&& !LicHeader.LA_ContractRenewalIssued.IsEmpty
					&& LicHeader.LA_ContractRenewalIssued >= RenewalDate
					&& (Invoice == null || !Invoice.IsCancelled);
			}
		}

		public LicenceHeader LicHeader
		{
			get { return licHeader; }
		}

		public ZString EnterpriseCode
		{
			get { return licCompany != null ? licCompany.LicEnterprise.LE_EnterpriseCode : ZString.Empty; }
		}

		public ZString OrgCode
		{
			get { return org != null ? org.OH_Code : ZString.Empty; }
		}

		public ZString OrgName
		{
			get { return org != null ? org.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZString UNLOCO
		{
			get { return org != null ? org.OH_RL_NKClosestPort : ZString.Empty; }
		}

		public ZString PayingOrgCode
		{
			get { return Recipient.OrganisationCode != OrgCode ? Recipient.OrganisationCode : ZString.Empty; }
		}

		public ZString ClientSize
		{
			get { return org != null ? org.MiscServ.OM_CMClientSize : ZString.Empty; }
		}

		public ZString ServerCode
		{
			get { return licHeader != null ? licHeader.DatabaseCode : ZString.Empty; }
		}

		public static ZDateTime CalculateDueDate(LicenceHeader licHeader)
		{
			ZDateTime result;

			if (licHeader != null && !licHeader.LA_ContractExpiryDate.IsEmpty)
			{
				result = licHeader.LA_ContractExpiryDate.AddDays(1);
			}
			else
			{
				result = ZDateTime.Now.Date.ToZDateTime().AddMonths(1).AddDays(-1);
			}

			return result;
		}

		static public ZDateTime GetRenewalDate(LicenceHeader licHeader)
		{
			return licHeader != null && !licHeader.LA_ContractExpiryDate.IsEmpty
				? licHeader.LA_ContractExpiryDate.AddDays(1)
				: ZDateTime.Empty;
		}

		public ZDateTime RenewalDate
		{
			get { return GetRenewalDate(licHeader); }
		}

		public ZDateTime RenewalNoticeDate
		{
			get
			{
				ZDateTime result = RenewalDate;
				int months = RenewalMonths;
				return !result.IsEmpty ? result.AddMonths(months > 1 ? -2 : -1) : result;
			}
		}

		public ZInt RenewalMonths
		{
			get
			{
				return licHeader != null && licHeader.ReadonlyBilling != null ? licHeader.ReadonlyBilling.L0_RenewalMonths : (ZInt)0;
			}
		}

		public ZString InvoiceCurrencyCode
		{
			get { return Recipient != null ? Recipient.InvoiceCurrencyCode : ZString.Empty; }
		}

		public override ZString PriceCurrencyCode
		{
			get
			{
				if (IsFixedMaintenance)
				{
					return licHeader.ReadonlyBilling.L0_RX_NKFixedMaintenanceCurrency;
				}
				else
				{
					return Prices != null ? Prices.L6_RX_NKCurrency : ZString.Empty;
				}
			}
		}

		ClientLicencePriceHeader Prices;
		public ZDecimal PriceOldSeats { get { return maintenancePrices.PriceOldSeats; } }
		public ZDecimal PriceNewSeats { get { return maintenancePrices.PriceNewSeats; } }
		public ZDecimal Maintenance { get { return maintenancePrices.Maintenance; } }
		public ZDecimal SurchargeAmount { get { return maintenancePrices.SurchargeAmount; } }
		public ZDecimal FinalAmount { get { return maintenancePrices.FinalAmount; } }
		public ZString PriceListVersion { get { return Prices != null ? Prices.L6_PricelistVersion : ZString.Empty; } }
		public ZDecimal NewPercent
		{
			get { return maintenancePrices.NewPercent; }
			set { maintenancePrices.NewPercent = value; }
		}

		public MaintenanceModuleCollection ChargedModules
		{
			get { return maintenancePrices.Modules; }
		}

		ZDecimal LastMaintenanceAmount
		{
			get { return licHeader != null ? licHeader.Billing.L0_LastMaintenanceAmount : ZDecimal.Zero; }
		}

		public ZDecimal MonthlyMaintenance
		{
			get { return RenewalMonths != 0 ? Maintenance / RenewalMonths : 0; }
		}

		public ZDecimal MaintenanceIncrease
		{
			get { return Maintenance - LastMaintenanceAmount; }
		}

		public ZInt MaintenancePercentDecimals
		{
			get { return 2; }
		}

		public ZDecimal CombinedMaintenancePercent
		{
			get { return maintenancePrices.CombinedMaintenancePercent; }
		}

		public ZDecimal OldSeatPercentIncrease
		{
			get
			{
				decimal result = decimal.Zero;
				if (licHeader != null)
				{
					var billing = licHeader.ReadonlyBilling;
					if (billing != null && maintenancePrices.GetIsFirstRenewal())
					{
						result = CalcPercentChange(billing.L0_LastMaintenancePercent, billing.L0_NextMaintenancePercent);
					}
				}

				return result;
			}
			set
			{
				var billing = licHeader.Billing;
				billing.L0_NextMaintenancePercent = Utilities.Round((value + 100m) * billing.L0_LastMaintenancePercent / 100m, MaintenancePercentDecimals);
				if (billing.L0_NextMaintenancePercentInfo.HasChanges)
				{
					HasChanges = true;
				}
			}
		}

		public ZPropertyInfo OldSeatPercentIncreaseInfo
		{
			get
			{
				return licHeader != null && licHeader.ReadonlyBilling != null
					? GetWrappedZPropertyInfo(nameof(OldSeatPercentIncrease), x => licHeader.ReadonlyBilling.L0_NextMaintenancePercentInfo)
					: GetZPropertyInfo(nameof(OldSeatPercentIncrease));
			}
		}

		public ZDecimal NewSeatPercentIncrease
		{
			get
			{
				decimal result = decimal.Zero;
				if (licHeader != null)
				{
					var billing = licHeader.ReadonlyBilling;
					if (billing != null)
					{
						result = CalcPercentChange(billing.L0_LastNewSeatMaintenancePercent, billing.L0_NextNewSeatMaintenancePercent);
					}
				}

				return result;
			}
			set
			{
				var billing = licHeader.Billing;
				billing.L0_NextNewSeatMaintenancePercent = Utilities.Round((value + 100m) * billing.L0_LastNewSeatMaintenancePercent / 100m, MaintenancePercentDecimals);
				if (billing.L0_NextNewSeatMaintenancePercentInfo.HasChanges)
				{
					HasChanges = true;
				}
			}
		}

		public ZPropertyInfo NewSeatPercentIncreaseInfo
		{
			get
			{
				return licHeader != null && licHeader.ReadonlyBilling != null
					? GetWrappedZPropertyInfo(nameof(NewSeatPercentIncrease), x => licHeader.ReadonlyBilling.L0_NextNewSeatMaintenancePercentInfo)
					: GetZPropertyInfo(nameof(NewSeatPercentIncrease));
			}
		}

		public ZDecimal AllSeatPercentIncrease
		{
			get
			{
				return CalcPercentChange(maintenancePrices.MaintenanceAtLastPercentages, maintenancePrices.Maintenance);
			}
		}

		ZDecimal CalcPercentChange(ZDecimal from, ZDecimal to)
		{
			return from != ZDecimal.Zero ? (to * 100m) / from - 100m : decimal.Zero;
		}

		internal ZString Comment
		{
			get { return licHeader != null ? licHeader.Billing.L0_Comment : ZString.Empty; }
			set { licHeader.Billing.L0_Comment = value; }
		}

		#endregion

		#region DataRefreshBus Updates

		const string UpdateSyncError = "Settings have changed outside this screen. Generate Report must be done again.";

		void OnLicenceCompanyUpdated(object sender, EventArgs e)
		{
			AddRowError(UpdateSyncError);
		}

		void OnLicenceHeaderUpdated(object sender, EventArgs e)
		{
			UpdateAll();
		}

		void UpdateAll()
		{
			if (licHeader != null)
			{
				InvalidateCanInvoice();
				CalculateAll();
			}

			maintenancePrices = new MaintenancePrices(Factory, licHeader, Prices, RenewalDate, decider);

			if (licHeader != null)
			{
				ValidateAll();
			}
		}

		#endregion

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				InvalidateCanInvoice();
				RefreshBinding();
			}
		}

		#region CalculateAll

		void CalculateAll()
		{
			LoadExistingUsages();

			CalculatePriceList();
			CalculateExchangeRate();

			OverallSalesRep = org != null ? org.StaffAssignments.OverallSalesRep : ZString.Empty;

			RelationshipManager = org != null ? org.RelationshipManager : ZString.Empty;
		}

		ClientChargeableUsage[] existingUsages;

		void LoadExistingUsages()
		{
			existingUsages = licHeader != null ? LoadExistingUsages(Factory, RenewalDate, licHeader) : null;
			if (existingUsages != null
				&& existingUsages.Length > 0
				&& !existingUsages[0].U1_AH_Invoice.IsEmpty
				)
			{
				Invoice = existingUsages[0].Invoice;
#pragma warning disable
				((IBusinessObjectState)Invoice).UpdatedByDataRefreshIncludingChildren += new EventHandler(OnInvoiceUpdated);
#pragma warning restore
			}
		}

		void OnInvoiceUpdated(object sender, EventArgs e)
		{
			UpdateAll();
		}

		static ClientChargeableUsage[] LoadExistingUsages(BusinessObjectFactory usageFactory, ZDateTime periodStart, LicenceHeader licHeader)
		{
			ClientChargeableUsage[] result = null;
			if (!periodStart.IsEmpty)
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(JoinCondition.And, ClientChargeableUsageSchema.U1_PeriodStart, periodStart);
				query.AddToFilter(JoinCondition.And, ClientChargeableUsageSchema.U1_LD, licHeader.LA_LD);
				query.AddToFilter(JoinCondition.And, ClientChargeableUsageSchema.U1_LC, licHeader.LA_LC);
				query.AddToFilter(ClientChargeableUsageSchema.U1_Code, BillingConstants.BillingSystem.Maintenance);

				result = usageFactory.Load<ClientChargeableUsage>(query);
			}
			return result;
		}

		void CalculatePriceList()
		{
			Prices = licHeader.MaintenancePricesForDate(!RenewalDate.IsEmpty ? RenewalDate : ZDateTime.Now);
		}

		#endregion

		#region ValidateAll

		void ValidateAll()
		{
			ZPropertyInfo notificationOwner = StatusTextInfo;

			notificationOwner.ClearAllNotifications();

			if (Recipient != null && IsBilled)
			{
				Recipient.AddNotifications(notificationOwner, false);
			}

			if (!notificationOwner.HasNotifications())
			{
				base.AddNotifications(notificationOwner);
			}

			if (!notificationOwner.HasNotifications())
			{
				if (RenewalDate.IsEmpty)
				{
					notificationOwner.AddMessageError("No contract expiry date for " + OrgCode);
				}
				else if (IsFixedMaintenance && PriceCurrencyCode.IsEmpty)
				{
					notificationOwner.AddMessageError("No currency defined for fixed maintenance amount");
				}
				else if (!IsFixedMaintenance && Prices == null)
				{
					notificationOwner.AddMessageError("No pricelist found for " + OrgCode);
				}
				else if (Maintenance == 0m)
				{
					notificationOwner.AddMessageError("Maintenance is zero.");
				}
			}
		}

		public ZBool IsFixedMaintenance
		{
			get { return licHeader != null && licHeader.ReadonlyBilling != null && licHeader.ReadonlyBilling.L0_FixedMaintenanceAmount != 0; }
		}

		#endregion

		#region CreateInvoice

		const string InvoiceDescription = "ediEnterprise Application Services Renewal";

		public static ZString GetInvoiceDescription(MaintenanceBill[] billGroup)
		{
			if (billGroup.Length == 1 || !billGroup.Any(s => s.RenewalDate != billGroup[0].RenewalDate))
			{
				return Description(billGroup[0].RenewalDate);
			}
			else
			{
				return InvoiceDescription;
			}
		}

		static ZString Description(ZDateTime renewalDate)
		{
			return InvoiceDescription + " - " + renewalDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
		}

		ZString GetAmountLineDescription()
		{
			switch (LicHeader.Database.LD_LicenceType)
			{
				case DatabaseTypes.Codes.Test: return "Test Licence";
				case DatabaseTypes.Codes.Training: return "Training Licence";
				default: return InvoiceDescription;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public virtual void PopulateInvoice(ARInvoice invoice)
		{
			decimal osAmount = 0;
			if (!Recipient.ShowPerModuleAmounts)
			{
				ZDecimal amountInInvoiceCurrency = Recipient.AmountInInvoiceCurrency(Maintenance, PriceCurrencyCode);
				osAmount += amountInInvoiceCurrency;

				BillingInvoicingHelper.AddAmountLine(invoice,
					amountInInvoiceCurrency,
					"ANNMAINT",
					taxId,
					GetAmountLineDescription());
			}
			else
			{
				BillingInvoicingHelper.AddCommentLine(invoice, GetAmountLineDescription());
			}

			const string dateFormat = "d MMMM yyyy";
			string dateRangeText = RenewalDate.ToString(dateFormat, CultureInfo.InvariantCulture)
				+ " to "
				+ RenewalDate.AddMonths(RenewalMonths).AddDays(-1).ToString(dateFormat, CultureInfo.InvariantCulture);
			BillingInvoicingHelper.AddCommentLine(invoice, dateRangeText);
			BillingInvoicingHelper.AddCommentLine(invoice, "-".PadRight(dateRangeText.Length, '-'));

			BillingInvoicingHelper.AddCommentLine(invoice, OrgName);
			BillingInvoicingHelper.AddCommentLine(invoice, OrgCode + " Server " + ServerCode);
			if (!licHeader.Billing.L0_ClientRef.IsEmpty)
			{
				BillingInvoicingHelper.AddCommentLine(invoice, licHeader.Billing.L0_ClientRef);
			}

			AddCurrencyExchangeLine(invoice, Maintenance);

			foreach (var item in ChargedModules.Cast<MaintenanceModule>().OrderBy(s => s.Order))
			{
				if (item.TotalPrice != 0)
				{
					string description = ZString.Format("{0} ({1})", item.PriceItem.L7_DescriptionLocalized.Trim(), item.UserCount);
					if (!Recipient.ShowPerModuleAmounts)
					{
						BillingInvoicingHelper.AddCommentLine(invoice, description);
					}
					else
					{
						ZDecimal amountInInvoiceCurrency = Recipient.AmountInInvoiceCurrency(item.TotalMaintenance, PriceCurrencyCode);
						osAmount += amountInInvoiceCurrency;
						BillingInvoicingHelper.AddAmountLine(invoice,
							amountInInvoiceCurrency,
							"ANNMAINT", taxId, description);
					}
				}
			}

			if (SurchargeAmount != 0)
			{
				ZDecimal amountInInvoiceCurrency = Recipient.AmountInInvoiceCurrency(SurchargeAmount, PriceCurrencyCode);
				osAmount += amountInInvoiceCurrency;
				BillingInvoicingHelper.AddAmountLine(invoice,
					amountInInvoiceCurrency,
					"ANNMAINT",
					taxId,
					maintenancePrices.SurchangeDescription);
			}

			var salesTax = InvoiceDelivery?.SalesTaxChargeCode;
			if (osAmount != 0 && salesTax != null)
			{
				decimal rate = SystemBill.TaxGroup.GetSalesTaxPercentage(salesTax);
				ZDecimal taxAmount = Utilities.Round(osAmount * rate * 0.01m, BillingConstants.RoundingDecimals);
				if (taxAmount != 0)
				{
					BillingInvoicingHelper.AddAmountLine(invoice, taxAmount, salesTax.AC_Code, null,
						string.Concat(salesTax.AC_Desc, " (", salesTax.AC_Code, ')'));
				}
			}

			UpdateModuleRenewal(invoice);
			Invoice = invoice;
			InvalidateCanInvoice();
		}

		void UpdateModuleRenewal(ARInvoice invoice)
		{
			ClientChargeableUsage[] usagesInInvoiceFactory;

			if (existingUsages != null && existingUsages.Length > 0)
			{
				usagesInInvoiceFactory = LoadExistingUsages(invoice.Factory, RenewalDate, licHeader);
			}
			else
			{
				ClientChargeableUsage usage = invoice.Factory.New<ClientChargeableUsage>();
				usage.U1_Code = BillingConstants.BillingSystem.Maintenance;
				usage.U1_LD = licHeader.LA_LD;
				usage.U1_LC = licHeader.LA_LC;
				usage.U1_PeriodStart = RenewalDate;
				usagesInInvoiceFactory = new ClientChargeableUsage[] { usage };
			}

			foreach (ClientChargeableUsage usage in usagesInInvoiceFactory)
			{
				usage.U1_AH_Invoice = invoice.PK;
				usage.U1_UpdateTime = ZDateTime.Now;
				usage.U1_UnitCount = 1;
				usage.U1_InvoicedUnitCount = 1;
				usage.U1_UnitPrice = Maintenance;
			}

			existingUsages = usagesInInvoiceFactory;

			maintenancePrices.UpdateRenewal(invoice.Factory);
		}

		void IInvoiceUpdateNotifier.OnInvoiceChanged(ARInvoice newInvoice)
		{
			if (Invoice != null && newInvoice != null && Invoice.PK != newInvoice.PK)
			{
				foreach (var usage in existingUsages ?? Enumerable.Empty<ClientChargeableUsage>())
				{
					usage.U1_AH_Invoice = newInvoice.PK;
				}

				Invoice = newInvoice;
				InvalidateCanInvoice();
			}
		}

		#endregion
	}
}

