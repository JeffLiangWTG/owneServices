using System;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BillRecipient : NonPersistentBusinessObject, IObsoleteValidation
	{
		BillRecipient(BusinessObjectFactory factory, ZGuid branchPK, ZString currencyCode, ZDateTime dateForExchangeRate)
			: base(factory)
		{
			if (!branchPK.IsEmpty && branchPK != Env.CurrentBranch.PK)
			{
				throw new InvalidOperationException("Current branch must be set to invoicing branch");
			}

			BranchPK = branchPK;
			InvoiceCurrencyCode = currencyCode;
			InvoiceCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, InvoiceCurrencyCode);
			DateForExchangeRate = dateForExchangeRate;
		}

		public BillRecipient(BusinessObjectFactory factory, ZGuid branchPK, ZGuid orgPK, ZString currencyCode, ZDateTime dateForExchangeRate)
			: this(factory, branchPK, currencyCode, dateForExchangeRate)
		{
			OrganisationPK = orgPK;
			Organisation = Factory.Load<EDIOrgHeader>(OrganisationPK);
			if (Organisation != null)
			{
				LicCompany = Organisation.LicCompany;
			}
			SelfBilling = LicCompany != null ? LicCompany.SelfBilling : null;

			CalculatePartner();
			CalculateExchangeRates();
		}

		public BillRecipient(BusinessObjectFactory factory,
			ZGuid branchPK,
			EDIOrgHeader org,
			LicenceCompany licCompany,
			ClientLicenceBilling licBilling,
			ZString currencyCode,
			ZDecimal localExchangeRate,
			ZDateTime dateForExchangeRate,
			EDIOrgHeader invoicingPartner)
			: this(factory, branchPK, currencyCode, dateForExchangeRate)
		{
			OrganisationPK = org?.PK ?? ZGuid.Empty;
			Organisation = org;
			LicCompany = licCompany;
			SelfBilling = licBilling;

			Partner = invoicingPartner;
			IsInvoicedByPartner = Partner != null;
			PartnerOrgPK = Partner != null ? Partner.PK : ZGuid.Empty;

			LocalExchangeRate = localExchangeRate;
		}

		public ZGuid BranchPK { get; private set; }
		public ZGuid OrganisationPK { get; private set; }
		public ZString InvoiceCurrencyCode { get; private set; }

		public EDIOrgHeader Organisation { get; private set; }
		public LicenceCompany LicCompany { get; private set; }
		protected ClientLicenceBilling SelfBilling { get; set; }
		RefCurrency InvoiceCurrency { get; set; }
		public ZBool IsInvoicedByPartner { get; private set; }
		public ZGuid PartnerOrgPK { get; private set; }
		public EDIOrgHeader Partner { get; private set; }
		public ZDateTime DateForExchangeRate { get; private set; }

		#region OverallSalesRep

		public ZString OverallSalesRep
		{
			get
			{
				if (!overallSalesRep.HasValue)
				{
					overallSalesRep = Organisation != null ? Organisation.StaffAssignments.OverallSalesRep : ZString.Empty;
				}

				return overallSalesRep.Value;
			}
		}
		ZString? overallSalesRep;

		#endregion

		public GlbBranch InvoicingBranch
		{
			get { return !BranchPK.IsEmpty ? Factory.Load<GlbBranch>(BranchPK) : null; }
		}

		public ZString InvoicingCountryCode
		{
			get { return InvoicingBranch != null ? InvoicingBranch.Company.GC_RN_NKCountryCode : ZString.Empty; }
		}

		public ZString MonthlyUsageInvoiceComment
		{
			get
			{
				ZString result = SelfBilling != null ? SelfBilling.L4_InvoiceCommentMultilingual : ZString.Empty;
				if (result.IsEmpty)
				{
					result = EDIDataRegistry.Instance.MonthlyUsageInvoiceComment.GetFallBackValueAtAllLevels(InvoicingBranch.GB_GC.ToGuid(), BranchPK.ToGuid(), Guid.Empty);
				}
				return result;
			}
		}

		public ZString ProcessingFeeDescription
		{
			get
			{
				return EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.Value.GetDescriptionFromCode(SelfBilling.L4_ProcessingFee);
			}
		}

		#region Organisation Information

		public ZString OrganisationCode
		{
			get { return Organisation != null ? Organisation.OH_Code : ZString.Empty; }
		}

		public ZString OrganisationName
		{
			get { return Organisation != null ? Organisation.OH_FullNameTruncated : ZString.Empty; }
		}

		public ZString OrganisationUNLOCO
		{
			get { return Organisation != null ? Organisation.OH_RL_NKClosestPort : ZString.Empty; }
		}

		public ZString EnterpriseCode
		{
			get { return Organisation != null && Organisation.LicEnterprise != null ? Organisation.LicEnterprise.LE_EnterpriseCode : ZString.Empty; }
		}

		#endregion

		void CalculatePartner()
		{
			ClientInvoiceDelivery[] billedDeliveries = LicCompany != null ? LicCompany.InvoiceDeliveries.Cast<ClientInvoiceDelivery>().Where(s => s.L9_IsBilled).ToArray() : null;
			ClientInvoiceDelivery clientInvoiceDelivery = billedDeliveries != null && billedDeliveries.Length == 1 ? billedDeliveries[0] : null;
			Partner = clientInvoiceDelivery != null ? clientInvoiceDelivery.Partner : null;
			IsInvoicedByPartner = Partner != null;
			PartnerOrgPK = Partner != null ? Partner.PK : ZGuid.Empty;
		}

		#region Exchange Rates/Currency Calculation

		void CalculateExchangeRates()
		{
			LocalExchangeRate = 1m;
			var branch = InvoicingBranch;
			var currency = InvoiceCurrency;
			if (branch != null && currency != null)
			{
				LocalExchangeRate = currency.GetRateForDate(Enterprise.ZArchitecture.Core.ExchangeRateType.Sell, DateForExchangeRate, 0);
			}
		}

		/// Exchange rate for invoicing branch currency to invoice currency
		public ZDecimal LocalExchangeRate { get; private set; }

		public ZDecimal AmountInInvoiceCurrency(ZDecimal amount, ZString sourceCurrencyCode)
		{
			if (sourceCurrencyCode != InvoiceCurrencyCode)
			{
				return BillingInvoicingHelper.GetAmountInInvoiceCurrency(amount, DateForExchangeRate, sourceCurrencyCode, InvoiceCurrencyCode, InvoicingBranch, Factory);
			}

			return amount;
		}

		#endregion

		public static class ValidationMessages
		{
			public const string NoLicence = "No licence found for this organization.";
			public const string NoBranch = "Invoice delivery instruction missing/imcomplete - branch is not defined.";
			public const string NoCurrency = "Invoicing currency is not defined.";
			public const string NoLocalExchangeRate = "Login company {0}: exchange rate for {1} not found for {2} to {3}.";
			public const string NotDebtor = "Organization is not of type Receivables. The A/R tab has not been configured.";
			public const string InactiveOrg = "Organization is inactive.";
		}

		public void AddRowNotifications(BusinessObject notificationOwner)
		{
			AddNotifications(notificationOwner.AddRowNotification, CargoWise.EntityFramework.NotificationType.Error);
		}

		public void AddNotifications(INotifications notificationOwner, bool mustHaveLicence = true)
		{
			AddNotifications(notificationOwner.Add, CargoWise.EntityFramework.NotificationType.MessageError, mustHaveLicence);
		}

		delegate void NotificationAdder(INotification notification);

		static void AddNotif(NotificationAdder addNotification, INotificationType notification, string message)
		{
			addNotification(new Notification(notification, message));
		}

		void AddNotifications(NotificationAdder addNotification, INotificationType notification, bool mustHaveLicence = true)
		{
			if (mustHaveLicence && LicCompany == null)
			{
				AddNotif(addNotification, notification, ValidationMessages.NoLicence);
			}
			else if (InvoicingBranch == null)
			{
				AddNotif(addNotification, notification, ValidationMessages.NoBranch);
			}
			else if (InvoiceCurrencyCode.IsEmpty)
			{
				AddNotif(addNotification, notification, ValidationMessages.NoCurrency);
			}
			else if (LocalExchangeRate == 0)
			{
				AddNotif(addNotification, notification, string.Format(CultureInfo.InvariantCulture, ValidationMessages.NoLocalExchangeRate, InvoicingBranch.Company.GC_Code, DateForExchangeRate.ToShortDateString(), InvoiceCurrencyCode, InvoicingBranch.Company.GC_RX_NKLocalCurrency));
			}
			else if (Organisation != null && !Organisation.OH_IsActive)
			{
				AddNotif(addNotification, notification, ValidationMessages.InactiveOrg);
			}
			else if (!IsInvoicedByPartner)
			{
				var companyDataList = (OrgCompanyData[])Organisation.CompanyDataCollection.Find(new ZQuery(OrgCompanyDataSchema.OB_GC, InvoicingBranch.Company.PK));
				if (companyDataList.Length == 0 || !companyDataList[0].OB_IsDebtor)
				{
					AddNotif(addNotification, notification, ValidationMessages.NotDebtor);
				}
			}
		}

		#region A/R Contact

		OrgContact Contact
		{
			get
			{
				if (!contactInitialized)
				{
					contact = FindRelatedReceivableContact();
					contactInitialized = true;
				}

				return contact;
			}
		}
		OrgContact contact;
		bool contactInitialized;

		public ZString ContactName
		{
			get { return Contact != null ? Contact.OC_ContactName : ZString.Empty; }
		}

		public ZString ContactEmail
		{
			get { return Contact != null ? Contact.OC_Email : ZString.Empty; }
		}

		OrgContact FindRelatedReceivableContact()
		{
			return ARContactHelper.FindRelatedReceivableContact(Organisation);
		}

		#endregion

		#region Create Invoice

		public delegate void InvoiceBuilder(ARInvoice invoice);

		protected ARInvoice CreateInvoice(ZDateTime postAndInvoiceDateOverride, InvoiceBuilder invoiceBuilder, IUSSalesTaxCalculator usSalesTaxCalculator = null)
		{
			using (BillingInvoicingHelper.BranchContext(BranchPK.ToGuid()))
			{
				return CreateInvoiceForBranch(InvoicingBranch, postAndInvoiceDateOverride, invoiceBuilder, usSalesTaxCalculator: usSalesTaxCalculator);
			}
		}

		protected EDIARInvoice CreateInvoiceForBranch(GlbBranch invoicingBranch, ZDateTime postAndInvoiceDateOverride, IUSSalesTaxCalculator usSalesTaxCalculator = null)
		{
			var factory = new BusinessObjectFactory();
			var invoice = factory.New<EDIARInvoice>();
			invoice.AH_OH = OrganisationPK;
			if (!postAndInvoiceDateOverride.IsEmpty)
			{
				invoice.AH_InvoiceDate = postAndInvoiceDateOverride;
				invoice.AH_PostDate = postAndInvoiceDateOverride;
			}
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
			invoice.AH_RX_NKTransactionCurrency = InvoiceCurrencyCode;
			invoice.AH_GB = invoicingBranch.PK;
			invoice.AH_GC = invoicingBranch.GB_GC;
			invoice.AH_ExchangeRate = LocalExchangeRate;
			invoice.USSalesTaxCalculator = usSalesTaxCalculator;
			return invoice;
		}

		ARInvoice CreateInvoiceForBranch(GlbBranch invoicingBranch, ZDateTime postAndInvoiceDateOverride, InvoiceBuilder invoiceBuilder, IUSSalesTaxCalculator usSalesTaxCalculator = null)
		{
			ARInvoice invoice = CreateInvoiceForBranch(invoicingBranch, postAndInvoiceDateOverride, usSalesTaxCalculator: usSalesTaxCalculator);
			invoiceBuilder(invoice);
			BusinessObjectFactory.SaveTogether(invoice.Factory, invoice.DocManagerInfo.MasterFactory);
			return invoice;
		}

		#endregion
	}
}

