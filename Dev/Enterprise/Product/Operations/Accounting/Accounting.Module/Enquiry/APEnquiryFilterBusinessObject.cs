using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Business.CreditStatus;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public partial class APEnquiryFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		public APEnquiryFilterBusinessObject()
		{
			AgingDateTypeInfo.ValueChanged += delegate { SetInfoValuesOfInvoiceAging(); };
			AgingDateType = AccountingConfigurationRegistry.Instance.AgingOptionPayables.Value;
		}

		#region Filters

		ModuleGuidFilter fOrganisationFilter;
		bool ModuleFiltersAreCreated { get; set; }
		protected virtual ZBool IsPayableModule => true;
		protected override void OnModuleFiltersCreated()
		{
			base.OnModuleFiltersCreated();
			ModuleFiltersAreCreated = true;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			fOrganisationFilter = CreateOrganizationFilter();
			filters.AddFilter(fOrganisationFilter);

			OrgWithAddressFilter orgWithAddressFilter = new OrgWithAddressFilter("Organization and Address", GetAPOrganizationAndAddressFilter, IsDebtor);
			orgWithAddressFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|APEnquiryFilter|OrganizationAndAddress", "Organization and Address");
			filters.AddCustomFilter(orgWithAddressFilter);

			AddBranchManagementCodeFilter(filters);

			var paymentStatusFilter = filters.AddTextFilter("Payment Status", GetPaymentStatusQuery, PaymentStatusList);
			paymentStatusFilter.Visibility = FilterVisibility.AlwaysVisible;
			paymentStatusFilter.Property = AccountingUtils.PaymentStatusTypes.Unpaid;
			paymentStatusFilter.PropertyInfo.ValueChanged += delegate { ResetInformationalFields(); };
			paymentStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|APEnquiryFilter|PaymentStatus", "Payment Status");

			var hasRelatedClaimFilter = filters.AddTextFilter("Has Related Claim", GetHasRelatedClaimQuery, HasRelatedClaimFilterOptionsList);
			hasRelatedClaimFilter.Category = FilterCategories.StatusAndFlags;
			hasRelatedClaimFilter.MultilingualDescription = ResString.GetMultilingualString("071ce275-7d9f-49fa-bef1-71b020a9f75d", "Has Related Claim");

			var referenceNumberFilter = filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.InvoiceRemittanceReference, GetInvoiceRemittanceReferenceQuery);
			referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|APEnquiryFilter|InvoiceRemittanceReference", "Invoice Remittance Reference");
			referenceNumberFilter.MaxLength = AccTransactionHeaderReferenceSchema.AH1_Reference.MaxLength;
			referenceNumberFilter.Category = FilterCategories.NumbersAndReferences;

			if (IsPayableModule)
			{
				AddNotionalWHTFilter(filters);
				AddServiceCodeFilter(filters);
			}

			AddMatchStatusAndReasonFilter(filters);

			return filters;
		}
			
		virtual protected bool IsDebtor
		{
			get { return false; }
		}

		ModuleGuidFilter CreateOrganizationFilter()
		{
			var organisationFilter = new ModuleGuidFilter("Organisation", ModuleIDs.Organisation, AccTransactionHeaderSchema.AH_OH, OrganisationList);
			organisationFilter.Visibility = FilterVisibility.AlwaysVisible;
			organisationFilter.PropertyInfo.ValueChanged += delegate { ResetInformationalFields(); };
			organisationFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|APEnquiryFilter|Organisation", "Organization");

			return organisationFilter;
		}

		#endregion

		#region Organisation

		public Guid OrganisationPK
		{
			get
			{
				if (OrganisationFilter.Property.IsValid && !OrganisationFilter.Property.IsEmpty)
				{
					return OrganisationFilter.Property.ToGuid();
				}
				else
				{
					return Guid.Empty;
				}
			}
		}

		public ModuleGuidFilter OrganisationFilter
		{
			get
			{
				if (ModuleFiltersAreCreated && this["Organisation"] != null)
				{
					return (ModuleGuidFilter)this["Organisation"];
				}
				else
				{
					return fOrganisationFilter ?? (fOrganisationFilter = CreateOrganizationFilter());
				}
			}
		}

		public void ResetInformationalFields()
		{
			ClearInfoValues();
			if (OrganisationPK != Guid.Empty)
			{
				SetInfoValues();
			}
		}

		#endregion

		#region Payment Status

		ZQuery GetPaymentStatusQuery(ZString paymentStatus)
		{
			return AccountingUtils.GetPaymentStatusFilter(paymentStatus);
		}

		#endregion

		#region Read Only Information

		#region Properties

		public ZString DisbursementPaymentTerms
		{
			get { return fDisbursementPaymentTerms; }
			protected set { SetNonPersistentPropertyValue(DisbursementPaymentTermsInfo, ref fDisbursementPaymentTerms, value); }
		}

		ZString fDisbursementPaymentTerms;

		public ZPropertyInfo DisbursementPaymentTermsInfo
		{
			get { return GetZPropertyInfo(nameof(DisbursementPaymentTerms)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal CreditBalance
		{
			get { return fCreditBalance; }
			protected set { SetNonPersistentPropertyValue(CreditBalanceInfo, ref fCreditBalance, value); }
		}

		ZDecimal fCreditBalance;

		public ZPropertyInfo CreditBalanceInfo
		{
			get { return GetZPropertyInfo(nameof(CreditBalance)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal CreditLimit
		{
			get { return fCreditLimit; }
			protected set { SetNonPersistentPropertyValue(CreditLimitInfo, ref fCreditLimit, value); }
		}

		ZDecimal fCreditLimit;

		public ZPropertyInfo CreditLimitInfo
		{
			get { return GetZPropertyInfo(nameof(CreditLimit)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal CurrentStandardOutstanding
		{
			get { return fCurrentStandardOutstanding; }
			protected set { SetNonPersistentPropertyValue(CurrentStandardOutstandingInfo, ref fCurrentStandardOutstanding, value); }
		}

		ZDecimal fCurrentStandardOutstanding;

		public ZPropertyInfo CurrentStandardOutstandingInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentStandardOutstanding)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal FirstAgeingStandardOutstanding
		{
			get { return fFirstAgeingStandardOutstanding; }
			protected set { SetNonPersistentPropertyValue(FirstAgeingStandardOutstandingInfo, ref fFirstAgeingStandardOutstanding, value); }
		}

		ZDecimal fFirstAgeingStandardOutstanding;

		public ZPropertyInfo FirstAgeingStandardOutstandingInfo
		{
			get { return GetZPropertyInfo(nameof(FirstAgeingStandardOutstanding)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal LastPaymentReceipt
		{
			get { return fLastPaymentReceipt; }
			protected set { SetNonPersistentPropertyValue(LastPaymentReceiptInfo, ref fLastPaymentReceipt, value); }
		}

		ZDecimal fLastPaymentReceipt;

		public ZPropertyInfo LastPaymentReceiptInfo
		{
			get { return GetZPropertyInfo(nameof(LastPaymentReceipt)); }
		}

		public ZString LastPaymentReceiptDate
		{
			get { return fLastPaymentReceiptDate; }
			protected set { SetNonPersistentPropertyValue(LastPaymentReceiptDateInfo, ref fLastPaymentReceiptDate, value); }
		}

		ZString fLastPaymentReceiptDate;

		public ZPropertyInfo LastPaymentReceiptDateInfo
		{
			get { return GetZPropertyInfo(nameof(LastPaymentReceiptDate)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal LastPurchaseSale
		{
			get { return fLastPurchaseSale; }
			protected set { SetNonPersistentPropertyValue(LastPurchaseSaleInfo, ref fLastPurchaseSale, value); }
		}

		ZDecimal fLastPurchaseSale;

		public ZPropertyInfo LastPurchaseSaleInfo
		{
			get { return GetZPropertyInfo(nameof(LastPurchaseSale)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal LYRPurchaseSales
		{
			get { return fLYRPurchaseSales; }
			protected set { SetNonPersistentPropertyValue(LYRPurchaseSalesInfo, ref fLYRPurchaseSales, value); }
		}

		ZDecimal fLYRPurchaseSales;

		public ZInt AverageDaysFromInvoiceDateToFullyPaidDate
		{
			get { return fAverageDaysFromInvoiceDateToFullyPaidDate; }
			protected set { SetNonPersistentPropertyValue(AverageDaysFromInvoiceDateToFullyPaidDateInfo, ref fAverageDaysFromInvoiceDateToFullyPaidDate, value); }
		}
		ZInt fAverageDaysFromInvoiceDateToFullyPaidDate;

		public ZPropertyInfo AverageDaysFromInvoiceDateToFullyPaidDateInfo
		{
			get { return GetZPropertyInfo(nameof(AverageDaysFromInvoiceDateToFullyPaidDate)); }
		}

		public ZInt AverageDaysFromDueDateToFullyPaidDate
		{
			get { return fAverageDaysFromDueDateToFullyPaidDate; }
			protected set { SetNonPersistentPropertyValue(AverageDaysFromDueDateToFullyPaidDateInfo, ref fAverageDaysFromDueDateToFullyPaidDate, value); }
		}
		ZInt fAverageDaysFromDueDateToFullyPaidDate;

		public ZPropertyInfo AverageDaysFromDueDateToFullyPaidDateInfo
		{
			get { return GetZPropertyInfo(nameof(AverageDaysFromDueDateToFullyPaidDate)); }
		}

		public ZInt DisbursementAverageDaysFromDueDateToFullyPaid
		{
			get { return fDisbursementAverageDaysFromDueDateToFullyPaid; }
			protected set { SetNonPersistentPropertyValue(DisbursementAverageDaysFromDueDateToFullyPaidInfo, ref fDisbursementAverageDaysFromDueDateToFullyPaid, value); }
		}
		ZInt fDisbursementAverageDaysFromDueDateToFullyPaid;

		public ZPropertyInfo DisbursementAverageDaysFromDueDateToFullyPaidInfo
		{
			get { return GetZPropertyInfo(nameof(DisbursementAverageDaysFromDueDateToFullyPaid)); }
		}

		public ZInt StandardAverageDaysFromDueDateToFullyPaid
		{
			get { return fStandardAverageDaysFromDueDateToFullyPaid; }
			protected set { SetNonPersistentPropertyValue(StandardAverageDaysFromDueDateToFullyPaidInfo, ref fStandardAverageDaysFromDueDateToFullyPaid, value); }
		}
		ZInt fStandardAverageDaysFromDueDateToFullyPaid;

		public ZPropertyInfo StandardAverageDaysFromDueDateToFullyPaidInfo
		{
			get { return GetZPropertyInfo(nameof(StandardAverageDaysFromDueDateToFullyPaid)); }
		}

		public ZPropertyInfo LYRPurchaseSalesInfo
		{
			get { return GetZPropertyInfo(nameof(LYRPurchaseSales)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal MTD_PTDSales
		{
			get { return fMTD_PTDSales; }
			protected set { SetNonPersistentPropertyValue(MTD_PTDSalesInfo, ref fMTD_PTDSales, value); }
		}

		ZDecimal fMTD_PTDSales;

		public ZPropertyInfo MTD_PTDSalesInfo
		{
			get { return GetZPropertyInfo(nameof(MTD_PTDSales)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal SecondAgeingStandardOutstanding
		{
			get { return fSecondAgeingStandardOutstanding; }
			protected set { SetNonPersistentPropertyValue(SecondAgeingStandardOutstandingInfo, ref fSecondAgeingStandardOutstanding, value); }
		}

		ZDecimal fSecondAgeingStandardOutstanding;

		public ZPropertyInfo SecondAgeingStandardOutstandingInfo
		{
			get { return GetZPropertyInfo(nameof(SecondAgeingStandardOutstanding)); }
		}

		public ZString StandardPaymentTerms
		{
			get { return fStandardPaymentTerms; }
			protected set { SetNonPersistentPropertyValue(StandardPaymentTermsInfo, ref fStandardPaymentTerms, value); }
		}

		ZString fStandardPaymentTerms;

		public ZPropertyInfo StandardPaymentTermsInfo
		{
			get { return GetZPropertyInfo(nameof(StandardPaymentTerms)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal ThirdAgeingStandardOutstanding
		{
			get { return fThirdAgeingStandardOutstanding; }
			protected set { SetNonPersistentPropertyValue(ThirdAgeingStandardOutstandingInfo, ref fThirdAgeingStandardOutstanding, value); }
		}

		ZDecimal fThirdAgeingStandardOutstanding;

		public ZPropertyInfo ThirdAgeingStandardOutstandingInfo
		{
			get { return GetZPropertyInfo(nameof(ThirdAgeingStandardOutstanding)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalStandardOutstanding
		{
			get { return fTotalStandardOutstanding; }
			protected set { SetNonPersistentPropertyValue(TotalStandardOutstandingInfo, ref fTotalStandardOutstanding, value); }
		}

		ZDecimal fTotalStandardOutstanding;

		public ZPropertyInfo TotalStandardOutstandingInfo
		{
			get { return GetZPropertyInfo(nameof(TotalStandardOutstanding)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal YTDPurchaseSales
		{
			get { return fYTDPurchaseSales; }
			protected set { SetNonPersistentPropertyValue(YTDPurchaseSalesInfo, ref fYTDPurchaseSales, value); }
		}

		ZDecimal fYTDPurchaseSales;

		public ZPropertyInfo YTDPurchaseSalesInfo
		{
			get { return GetZPropertyInfo(nameof(YTDPurchaseSales)); }
		}

		#endregion

		public virtual void SetInfoValues()
		{
			LastPurchaseSale = ARAPEnquiryData.GetLastPurchase(OrganisationPK);
			LastPaymentReceipt = ARAPEnquiryData.GetLastPayment(OrganisationPK);
			ZDateTime date = ARAPEnquiryData.GetLastPaymentDate(OrganisationPK);
			if (!date.IsValid)
			{
				date = ZDateTime.Empty;
			}
			LastPaymentReceiptDate = date.Date.ToString();

			MTD_PTDSales = ARAPEnquiryData.GetPTDPurchases(OrganisationPK);
			YTDPurchaseSales = ARAPEnquiryData.GetYTDPurchases(OrganisationPK);
			LYRPurchaseSales = ARAPEnquiryData.GetLYRPurchases(OrganisationPK);
			CreditLimit = ARAPEnquiryData.GetAPCreditLimit(OrganisationPK);
			CreditBalance = ARAPEnquiryData.GetAPOutstandingBalance(OrganisationPK);

			StandardPaymentTerms = ARAPEnquiryData.GetPaymentTerms(OrganisationPK);

			DisbursementPaymentTerms = ARAPEnquiryData.GetDisbursementInvoiceTerms(OrganisationPK);

			SetInfoValuesOfInvoiceAging();

			AverageDaysFromDueDateToFullyPaidDate = ARAPEnquiryData.GetAPAverageDaysFromDueDateToFullyPaidDate(OrganisationPK);
			AverageDaysFromInvoiceDateToFullyPaidDate = ARAPEnquiryData.GetAPAverageDaysFromInvoiceDateToFullyPaidDate(OrganisationPK);
			DisbursementAverageDaysFromDueDateToFullyPaid = ARAPEnquiryData.GetDisbursementAPAverageDaysFromDueDateToFullyPaidDate(OrganisationPK);
			StandardAverageDaysFromDueDateToFullyPaid = ARAPEnquiryData.GetStandardAPAverageDaysFromDueDateToFullyPaidDate(OrganisationPK);
		}

		public virtual void SetInfoValuesOfInvoiceAging()
		{
			if (OrganisationPK != Guid.Empty)
			{
				CurrentStandardOutstanding = ARAPEnquiryData.GetCurrentAPAgeOutStandingAmount(OrganisationPK, AgingDateType);
				FirstAgeingStandardOutstanding = ARAPEnquiryData.GetSingleAPAgeOutStandingAmount(OrganisationPK, AgingDateType);
				SecondAgeingStandardOutstanding = ARAPEnquiryData.GetTwoAPAgeOutstandingAmount(OrganisationPK, AgingDateType);
				ThirdAgeingStandardOutstanding = ARAPEnquiryData.GetThreeAPAgeOutstandingAmount(OrganisationPK, AgingDateType);
				TotalStandardOutstanding = CurrentStandardOutstanding + FirstAgeingStandardOutstanding + SecondAgeingStandardOutstanding + ThirdAgeingStandardOutstanding;
			}
		}

		protected virtual void ClearInfoValues()
		{
			LastPurchaseSale = 0.0M;
			LastPaymentReceipt = 0.0M;
			LastPaymentReceiptDate = ZString.Empty;
			MTD_PTDSales = 0.0M;
			YTDPurchaseSales = 0.0M;
			LYRPurchaseSales = 0.0M;
			CreditLimit = 0.0M;
			CreditBalance = 0.0M;
			StandardPaymentTerms = ZString.Empty;
			DisbursementPaymentTerms = ZString.Empty;
			CurrentStandardOutstanding = 0.0M;
			FirstAgeingStandardOutstanding = 0.0M;
			SecondAgeingStandardOutstanding = 0.0M;
			ThirdAgeingStandardOutstanding = 0.0M;
			TotalStandardOutstanding = 0.0M;

			AverageDaysFromDueDateToFullyPaidDate = 0;
			AverageDaysFromInvoiceDateToFullyPaidDate = 0;
			DisbursementAverageDaysFromDueDateToFullyPaid = 0;
			StandardAverageDaysFromDueDateToFullyPaid = 0;
		}

		protected ARAPDataAccessor ARAPEnquiryData
		{
			get
			{
				if (fARAPEnquiryData == null)
				{
					fARAPEnquiryData = new ARAPDataAccessor();
				}
				return fARAPEnquiryData;
			}
		}
		ARAPDataAccessor fARAPEnquiryData;

		#endregion

		#region Aging Date Type

		[List("AgingDateTypeList")]
		public ZString AgingDateType
		{
			get
			{
				return fAgingDateType;
			}
			set
			{
				SetNonPersistentPropertyValue(AgingDateTypeInfo, ref fAgingDateType, value);
				AgingDateTypeInfo.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(AgingDateTypeInfo);
			}
		}
		ZString fAgingDateType;

		public ZPropertyInfo AgingDateTypeInfo
		{
			get { return GetZPropertyInfo(nameof(AgingDateType)); }
		}

		#endregion

		#region Lookups

		protected OrgHeaderCollection OrganisationList
		{
			get
			{
				if (fOrganisationList == null)
				{
					fOrganisationList = GetOrganisationList();
				}
				return fOrganisationList;
			}
		}
		OrgHeaderCollection fOrganisationList;

		protected virtual OrgHeaderCollection GetOrganisationList()
		{
			return new CreditorCollection(new BusinessObjectFactory());
		}

		CodeDescriptionPairList PaymentStatusList
		{
			get
			{
				if (fPaymentStatusList == null)
				{
					fPaymentStatusList = new CodeDescriptionPairList();
					fPaymentStatusList.AddPair(AccountingUtils.PaymentStatusTypes.All, Res.GetString("Accounting|APEnquiryFilter|DisplayAllTransactions", "Display all transactions"));
					fPaymentStatusList.AddPair(AccountingUtils.PaymentStatusTypes.Unpaid, Res.GetString("Accounting|APEnquiryFilter|DisplayUnpaidTransactions", "Display unpaid transactions"));
					fPaymentStatusList.AddPair(AccountingUtils.PaymentStatusTypes.Paid, Res.GetString("Accounting|APEnquiryFilter|DisplayFullyPaidTransactions", "Display fully paid transactions"));
					fPaymentStatusList.AddPair(AccountingUtils.PaymentStatusTypes.PartPaid, Res.GetString("Accounting|APEnquiryFilter|DisplayPartiallyPaidTransactions", "Display partially paid transactions"));
				}
				return fPaymentStatusList;
			}
		}
		CodeDescriptionPairList fPaymentStatusList;

		public CodeDescriptionPairList AgingDateTypeList
		{
			get
			{
				if (fAgingDateTypeList == null)
				{
					fAgingDateTypeList = AccountingConstants.AgingOptions.CodeList;
				}
				return fAgingDateTypeList;
			}
		}
		CodeDescriptionPairList fAgingDateTypeList;

		#endregion
	}
}
