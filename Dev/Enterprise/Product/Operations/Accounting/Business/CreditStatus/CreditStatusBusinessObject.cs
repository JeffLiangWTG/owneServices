using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CreditStatus
{
	public class CreditStatusBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CreditStatusBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
			Argument.NotNull(factory, "factory");

			AgingDateTypeInfo.ValueChanged += delegate { SetInfoValuesOfInvoiceAging(); };
			AgingDateType = AccountingConfigurationRegistry.Instance.AgingOptionReceivables.Value;
		}

		public abstract class Schema
		{
			public const string OrganisationPK = "OrganisationPK";
			public const string CurrentDisbursementOutstanding = "CurrentDisbursementOutstanding";
			public const string FirstAgeingDisbursementOutstanding = "FirstAgeingDisbursementOutstanding";
			public const string SecondAgeingDisbursementOutstanding = "SecondAgeingDisbursementOutstanding";
			public const string ThirdAgeingDisbursementOutstanding = "ThirdAgeingDisbursementOutstanding";
			public const string TotalDisbursementOutstanding = "TotalDisbursementOutstanding";
			public const string OnePeriodTotal = "OnePeriodTotal";
			public const string TwoPeriodTotal = "TwoPeriodTotal";
			public const string ThreePeriodTotal = "ThreePeriodTotal";
			public const string TotalOutstandingAmount = "TotalOutstandingAmount";
			public const string CurrentTotal = "CurrentTotal";
			public const string OM_ARTreatDisbursementsAsStandardValue = "OM_ARTreatDisbursementsAsStandardValue";
			public const string LastPurchaseSale = "LastPurchaseSale";
			public const string LastPaymentReceipt = "LastPaymentReceipt";
			public const string LastPaymentReceiptDate = "LastPaymentReceiptDate";
			public const string MTD_PTDSales = "MTD_PTDSales";
			public const string YTDPurchaseSales = "YTDPurchaseSales";
			public const string LYRPurchaseSales = "LYRPurchaseSales";
			public const string CreditBalance = "CreditBalance";
			public const string CreditLimit = "CreditLimit";
			public const string StandardPaymentTermDays = "StandardPaymentTermDays";
			public const string StandardPaymentTerms = "StandardPaymentTerms";
			public const string DisbursementPaymentTermDays = "DisbursementPaymentTermDays";
			public const string DisbursementPaymentTerms = "DisbursementPaymentTerms";
			public const string CurrentStandardOutstanding = "CurrentStandardOutstanding";
			public const string FirstAgeingStandardOutstanding = "FirstAgeingStandardOutstanding";
			public const string SecondAgeingStandardOutstanding = "SecondAgeingStandardOutstanding";
			public const string ThirdAgeingStandardOutstanding = "ThirdAgeingStandardOutstanding";
			public const string TotalStandardOutstanding = "TotalStandardOutstanding";
			public const string CurrentBatchedTotal = "CurrentBatchedTotal";
			public const string OnePeriodBatchedTotal = "OnePeriodBatchedTotal";
			public const string TwoPeriodBatchedTotal = "TwoPeriodBatchedTotal";
			public const string ThreePeriodBatchedTotal = "ThreePeriodBatchedTotal";
			public const string BatchedOutstandingAmountTotal = "BatchedOutstandingAmountTotal";
			public const string AverageDaysFromInvoiceDateToFullyPaidDate = "AverageDaysFromInvoiceDateToFullyPaidDate";
			public const string AverageDaysFromDueDateToFullyPaidDate = "AverageDaysFromDueDateToFullyPaidDate";
			public const string DisbursementAverageDaysFromDueDateToFullyPaid = "DisbursementAverageDaysFromDueDateToFullyPaid";
			public const string StandardAverageDaysFromDueDateToFullyPaid = "StandardAverageDaysFromDueDateToFullyPaid";
			public const string StandardOverdueAmount = "StandardOverdueAmount";
			public const string DisbursementOverdueAmount = "DisbursementOverdueAmount";
			public const string TotalOverdueAmount = "TotalOverdueAmount";
			public const string IsCreditOnHold = "IsCreditOnHold";
		}

		#region OrganisationPK

		[List("Headers")]
		public ZGuid OrganisationPK
		{
			get { return organisationPK; }
			set
			{
				SetNonPersistentPropertyValue(OrganisationPKInfo, ref organisationPK, value);
				ResetInformationalFields();
			}
		}
		ZGuid organisationPK;

		public ZPropertyInfo OrganisationPKInfo
		{
			get { return GetZPropertyInfo(Schema.OrganisationPK); }
		}

		public void ValidateOrganisationPK()
		{
			OrganisationPKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(OrganisationPKInfo, Headers);
		}

		public virtual OrgHeaderCollection Headers
		{
			get { return FindboxLookupCollections.GetDebtorCollection(Factory); }
		}

		#endregion

		void ResetInformationalFields()
		{
			if (OrganisationPK.IsValid)
			{
				SetInfoValues();
			}
			else
			{
				ClearInfoValues();
			}
		}

		void SetInfoValues()
		{
			var orgPK = organisationPK.ToGuid();

			LastPurchaseSale = CreditStatusData.GetLastSale(orgPK);
			LastPaymentReceipt = CreditStatusData.GetLastReceipt(orgPK);
			LastPaymentReceiptDate = CreditStatusData.GetLastReceiptDate(orgPK);
			if (!LastPaymentReceiptDate.IsValid)
			{
				LastPaymentReceiptDate = ZDateTime.Empty;
			}

			MTD_PTDSales = CreditStatusData.GetPTDSales(orgPK);
			YTDPurchaseSales = CreditStatusData.GetYTDSales(orgPK);
			LYRPurchaseSales = CreditStatusData.GetLYRSales(orgPK);

			StandardPaymentTerms = CreditStatusData.GetStandardInvoiceTerms(orgPK);
			DisbursementPaymentTerms = CreditStatusData.GetDisbursementInvoiceTerms(orgPK);

			SetInfoValuesOfInvoiceAging();

			if (Organisation != null)
			{
				OM_ARTreatDisbursementsAsStandardValue = Organisation.MiscServ.OM_ARTreatDisbursementsAsStandardValue;
			}

			AverageDaysFromDueDateToFullyPaidDate = CreditStatusData.GetARAverageDaysFromDueDateToFullyPaidDate(orgPK);
			AverageDaysFromInvoiceDateToFullyPaidDate = CreditStatusData.GetARAverageDaysFromInvoiceDateToFullyPaidDate(orgPK);
			DisbursementAverageDaysFromDueDateToFullyPaid = CreditStatusData.GetDisbursementARAverageDaysFromDueDateToFullyPaidDate(orgPK);
			StandardAverageDaysFromDueDateToFullyPaid = CreditStatusData.GetStandardARAverageDaysFromDueDateToFullyPaidDate(orgPK);

			SetUnpostedRevenueFields(orgPK);

			StandardOverdueAmount = CreditStatusData.GetStandardAROverdueAmount(orgPK);
			DisbursementOverdueAmount = CreditStatusData.GetDisbursementAROverdueAmount(orgPK);
			TotalOverdueAmount = StandardOverdueAmount + DisbursementOverdueAmount;

			if (SettlementGroup != null && (UseSettlementGroupCreditLimit || !string.IsNullOrEmpty(SettlementGroupString)))
			{
				CreditLimit = CreditStatusData.GetARCreditLimit(SettlementGroup.PK.ToGuid());
				CreditBalance = CreditStatusData.GetSettlementGroupARCreditBalance(SettlementGroup);
			}
			else
			{
				CreditLimit = CreditStatusData.GetARCreditLimit(orgPK);
				CreditBalance = CreditStatusData.CalculatedCreditBalance(CreditLimit, PostedRevenue, RecognisedWIP, UnrecognisedWIP, Claim);
			}
		}

		public void SetInfoValuesOfInvoiceAging()
		{
			if (OrganisationPK != Guid.Empty)
			{
				var orgPK = OrganisationPK.ToGuid();
				CurrentStandardOutstanding = CreditStatusData.GetCurrentARAgeOutStandingAmount(orgPK, AgingDateType);
				FirstAgeingStandardOutstanding = CreditStatusData.GetSingleARAgeOutStandingAmount(orgPK, AgingDateType);
				SecondAgeingStandardOutstanding = CreditStatusData.GetTwoARAgeOutstandingAmount(orgPK, AgingDateType);
				ThirdAgeingStandardOutstanding = CreditStatusData.GetThreeARAgeOutstandingAmount(orgPK, AgingDateType);
				TotalStandardOutstanding = CurrentStandardOutstanding + FirstAgeingStandardOutstanding + SecondAgeingStandardOutstanding + ThirdAgeingStandardOutstanding;

				CurrentDisbursementOutstanding = CreditStatusData.GetDSBCurrentAgeOutstandingAmount(orgPK, AgingDateType);
				FirstAgeingDisbursementOutstanding = CreditStatusData.GetDSBSingleAgeOutstandingAmount(orgPK, AgingDateType);
				SecondAgeingDisbursementOutstanding = CreditStatusData.GetDSBTwoAgeOutstandingAmount(orgPK, AgingDateType);
				ThirdAgeingDisbursementOutstanding = CreditStatusData.GetDSBThreeAgeOutstandingAmount(orgPK, AgingDateType);
				TotalDisbursementOutstanding = CurrentDisbursementOutstanding + FirstAgeingDisbursementOutstanding + SecondAgeingDisbursementOutstanding + ThirdAgeingDisbursementOutstanding;

				CurrentTotal = CurrentStandardOutstanding + CurrentDisbursementOutstanding;
				OnePeriodTotal = FirstAgeingDisbursementOutstanding + FirstAgeingStandardOutstanding;
				TwoPeriodTotal = SecondAgeingDisbursementOutstanding + SecondAgeingStandardOutstanding;
				ThreePeriodTotal = ThirdAgeingDisbursementOutstanding + ThirdAgeingStandardOutstanding;
				TotalOutstandingAmount = TotalDisbursementOutstanding + TotalStandardOutstanding;

				CurrentBatchedTotal = CreditStatusData.GetBatchedCurrentAgeOutstandingAmount(orgPK, AgingDateType);
				OnePeriodBatchedTotal = CreditStatusData.GetBatchedSingleAgeOutstandingAmount(orgPK, AgingDateType);
				TwoPeriodBatchedTotal = CreditStatusData.GetBatchedTwoAgeOutstandingAmount(orgPK, AgingDateType);
				ThreePeriodBatchedTotal = CreditStatusData.GetBatchedThreeAgeOutstandingAmount(orgPK, AgingDateType);
				BatchedOutstandingAmountTotal = CurrentBatchedTotal + OnePeriodBatchedTotal + TwoPeriodBatchedTotal + ThreePeriodBatchedTotal;
			}
		}

		void ClearInfoValues()
		{
			LastPurchaseSale = 0.0M;
			LastPaymentReceipt = 0.0M;
			LastPaymentReceiptDate = ZDateTime.Empty;
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

			CurrentDisbursementOutstanding = 0.0M;
			FirstAgeingDisbursementOutstanding = 0.0M;
			SecondAgeingDisbursementOutstanding = 0.0M;
			ThirdAgeingDisbursementOutstanding = 0.0M;
			TotalDisbursementOutstanding = 0.0M;

			CurrentTotal = 0.0M;
			OnePeriodTotal = 0.0M;
			TwoPeriodTotal = 0.0M;
			ThreePeriodTotal = 0.0M;
			TotalOutstandingAmount = 0.0M;

			CurrentBatchedTotal = 0.0M;
			OnePeriodBatchedTotal = 0.0M;
			TwoPeriodBatchedTotal = 0.0M;
			ThreePeriodBatchedTotal = 0.0M;
			BatchedOutstandingAmountTotal = 0.0M;

			OM_ARTreatDisbursementsAsStandardValue = 0.0M;

			AverageDaysFromDueDateToFullyPaidDate = 0;
			AverageDaysFromInvoiceDateToFullyPaidDate = 0;
			DisbursementAverageDaysFromDueDateToFullyPaid = 0;
			StandardAverageDaysFromDueDateToFullyPaid = 0;

			PostedRevenue = 0.0M;
			Claim = 0.0M;
			UnrecognisedWIP = 0.0M;
			RecognisedWIP = 0.0M;
			SumOfTotalWIPAndRevenue = 0.0M;

			ClearSettlementGroupStringCache();

			StandardOverdueAmount = 0.0m;
			DisbursementOverdueAmount = 0.0m;
			TotalOverdueAmount = 0.0m;
		}

		void SetUnpostedRevenueFields(Guid organisationPK)
		{
			var unpostedRevenueFieldValuesTuple = CreditStatusData.GetUnpostedRevenueFieldValues(organisationPK);
			PostedRevenue = unpostedRevenueFieldValuesTuple.PostedRevenue;
			Claim = unpostedRevenueFieldValuesTuple.Claim;
			UnrecognisedWIP = unpostedRevenueFieldValuesTuple.UnrecognisedWIP;
			RecognisedWIP = unpostedRevenueFieldValuesTuple.RecognisedWIP;
			SumOfTotalWIPAndRevenue = UnrecognisedWIP + RecognisedWIP + PostedRevenue;
		}

		ARAPDataAccessor CreditStatusData
		{
			get
			{
				if (creditStatusData == null)
				{
					creditStatusData = new ARAPDataAccessor();
				}
				return creditStatusData;
			}
		}
		ARAPDataAccessor creditStatusData;

		#region Aging Date Type

		[List("AgingOptionsList")]
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

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateOrganisationPK();
		}

		#region Properties

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal StandardOverdueAmount
		{
			get { return fStandardOverdueAmount; }
			private set { SetNonPersistentPropertyValue(StandardOverdueAmountInfo, ref fStandardOverdueAmount, value); }
		}

		ZDecimal fStandardOverdueAmount;

		public ZPropertyInfo StandardOverdueAmountInfo
		{
			get { return GetZPropertyInfo(nameof(StandardOverdueAmount)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal DisbursementOverdueAmount
		{
			get { return fDisbursementOverdueAmount; }
			private set { SetNonPersistentPropertyValue(DisbursementOverdueAmountInfo, ref fDisbursementOverdueAmount, value); }
		}

		ZDecimal fDisbursementOverdueAmount;

		public ZPropertyInfo DisbursementOverdueAmountInfo
		{
			get { return GetZPropertyInfo(nameof(DisbursementOverdueAmount)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalOverdueAmount
		{
			get { return fTotalOverdueAmount; }
			private set { SetNonPersistentPropertyValue(TotalOverdueAmountInfo, ref fTotalOverdueAmount, value); }
		}

		ZDecimal fTotalOverdueAmount;

		public ZPropertyInfo TotalOverdueAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalOverdueAmount)); }
		}

		public ZBool IsCreditOnHold
		{
			get
			{
				return Organisation != null && Organisation.CompanyData != null ? Organisation.CompanyData.OB_AROnCreditHold : ZBool.False;
			}
		}

		public ZPropertyInfo IsCreditOnHoldInfo
		{
			get { return GetZPropertyInfo(nameof(IsCreditOnHold)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal CurrentDisbursementOutstanding
		{
			get { return currentDisbursementOutstanding; }
			private set { SetNonPersistentPropertyValue(CurrentDisbursementOutstandingInfo, ref currentDisbursementOutstanding, value); }
		}

		ZDecimal currentDisbursementOutstanding;

		public ZPropertyInfo CurrentDisbursementOutstandingInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentDisbursementOutstanding); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal FirstAgeingDisbursementOutstanding
		{
			get { return firstAgeingDisbursementOutstanding; }
			private set { SetNonPersistentPropertyValue(FirstAgeingDisbursementOutstandingInfo, ref firstAgeingDisbursementOutstanding, value); }
		}

		ZDecimal firstAgeingDisbursementOutstanding;

		public ZPropertyInfo FirstAgeingDisbursementOutstandingInfo
		{
			get { return GetZPropertyInfo(Schema.FirstAgeingDisbursementOutstanding); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal SecondAgeingDisbursementOutstanding
		{
			get { return secondAgeingDisbursementOutstanding; }
			private set { SetNonPersistentPropertyValue(SecondAgeingDisbursementOutstandingInfo, ref secondAgeingDisbursementOutstanding, value); }
		}

		ZDecimal secondAgeingDisbursementOutstanding;

		public ZPropertyInfo SecondAgeingDisbursementOutstandingInfo
		{
			get { return GetZPropertyInfo(Schema.SecondAgeingDisbursementOutstanding); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal ThirdAgeingDisbursementOutstanding
		{
			get { return thirdAgeingDisbursementOutstanding; }
			private set { SetNonPersistentPropertyValue(ThirdAgeingDisbursementOutstandingInfo, ref thirdAgeingDisbursementOutstanding, value); }
		}

		ZDecimal thirdAgeingDisbursementOutstanding;

		public ZPropertyInfo ThirdAgeingDisbursementOutstandingInfo
		{
			get { return GetZPropertyInfo(Schema.ThirdAgeingDisbursementOutstanding); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalDisbursementOutstanding
		{
			get { return totalDisbursementOutstanding; }
			private set { SetNonPersistentPropertyValue(TotalDisbursementOutstandingInfo, ref totalDisbursementOutstanding, value); }
		}

		ZDecimal totalDisbursementOutstanding;

		public ZPropertyInfo TotalDisbursementOutstandingInfo
		{
			get { return GetZPropertyInfo(Schema.TotalDisbursementOutstanding); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal OnePeriodTotal
		{
			get { return onePeriodTotal; }
			private set { SetNonPersistentPropertyValue(OnePeriodTotalInfo, ref onePeriodTotal, value); }
		}

		ZDecimal onePeriodTotal;

		public ZPropertyInfo OnePeriodTotalInfo
		{
			get { return GetZPropertyInfo(Schema.OnePeriodTotal); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TwoPeriodTotal
		{
			get { return twoPeriodTotal; }
			private set { SetNonPersistentPropertyValue(TwoPeriodTotalInfo, ref twoPeriodTotal, value); }
		}

		ZDecimal twoPeriodTotal;

		public ZPropertyInfo TwoPeriodTotalInfo
		{
			get { return GetZPropertyInfo(Schema.TwoPeriodTotal); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal ThreePeriodTotal
		{
			get { return threePeriodTotal; }
			private set { SetNonPersistentPropertyValue(ThreePeriodTotalInfo, ref threePeriodTotal, value); }
		}

		ZDecimal threePeriodTotal;

		public ZPropertyInfo ThreePeriodTotalInfo
		{
			get { return GetZPropertyInfo(Schema.ThreePeriodTotal); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalOutstandingAmount
		{
			get { return totalOutstandingAmount; }
			private set { SetNonPersistentPropertyValue(TotalOutstandingAmountInfo, ref totalOutstandingAmount, value); }
		}

		ZDecimal totalOutstandingAmount;

		public ZPropertyInfo TotalOutstandingAmountInfo
		{
			get { return GetZPropertyInfo(Schema.TotalOutstandingAmount); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal CurrentTotal
		{
			get { return currentTotal; }
			private set { SetNonPersistentPropertyValue(CurrentTotalInfo, ref currentTotal, value); }
		}

		ZDecimal currentTotal;

		public ZPropertyInfo CurrentTotalInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentTotal); }
		}

		public ZDecimal OM_ARTreatDisbursementsAsStandardValue
		{
			get { return oM_ARTreatDisbursementsAsStandardValue; }
			private set { SetNonPersistentPropertyValue(OM_ARTreatDisbursementsAsStandardValueInfo, ref oM_ARTreatDisbursementsAsStandardValue, value); }
		}

		ZDecimal oM_ARTreatDisbursementsAsStandardValue;

		public ZPropertyInfo OM_ARTreatDisbursementsAsStandardValueInfo
		{
			get { return GetZPropertyInfo(Schema.OM_ARTreatDisbursementsAsStandardValue); }
		}

		public ZDecimal LastPurchaseSale
		{
			get { return lastPurchaseSale; }
			protected set { SetNonPersistentPropertyValue(LastPurchaseSaleInfo, ref lastPurchaseSale, value); }
		}

		ZDecimal lastPurchaseSale;

		public ZPropertyInfo LastPurchaseSaleInfo
		{
			get { return GetZPropertyInfo(Schema.LastPurchaseSale); }
		}

		public ZDecimal LastPaymentReceipt
		{
			get { return lastPaymentReceipt; }
			protected set { SetNonPersistentPropertyValue(LastPaymentReceiptInfo, ref lastPaymentReceipt, value); }
		}

		ZDecimal lastPaymentReceipt;

		public ZPropertyInfo LastPaymentReceiptInfo
		{
			get { return GetZPropertyInfo(Schema.LastPaymentReceipt); }
		}

		public ZDateTime LastPaymentReceiptDate
		{
			get { return lastPaymentReceiptDate; }
			protected set { SetNonPersistentPropertyValue(LastPaymentReceiptDateInfo, ref lastPaymentReceiptDate, value); }
		}

		ZDateTime lastPaymentReceiptDate;

		public ZPropertyInfo LastPaymentReceiptDateInfo
		{
			get { return GetZPropertyInfo(Schema.LastPaymentReceiptDate); }
		}

		public ZDecimal MTD_PTDSales
		{
			get { return mTD_PTDSales; }
			protected set { SetNonPersistentPropertyValue(MTD_PTDSalesInfo, ref mTD_PTDSales, value); }
		}

		ZDecimal mTD_PTDSales;

		public ZPropertyInfo MTD_PTDSalesInfo
		{
			get { return GetZPropertyInfo(Schema.MTD_PTDSales); }
		}

		public ZDecimal YTDPurchaseSales
		{
			get { return yTDPurchaseSales; }
			protected set { SetNonPersistentPropertyValue(YTDPurchaseSalesInfo, ref yTDPurchaseSales, value); }
		}

		ZDecimal yTDPurchaseSales;

		public ZPropertyInfo YTDPurchaseSalesInfo
		{
			get { return GetZPropertyInfo(Schema.YTDPurchaseSales); }
		}

		public ZDecimal LYRPurchaseSales
		{
			get { return lyrPurchaseSales; }
			protected set { SetNonPersistentPropertyValue(LYRPurchaseSalesInfo, ref lyrPurchaseSales, value); }
		}

		ZDecimal lyrPurchaseSales;

		public ZPropertyInfo LYRPurchaseSalesInfo
		{
			get { return GetZPropertyInfo(Schema.LYRPurchaseSales); }
		}

		public ZDecimal CreditBalance
		{
			get { return creditBalance; }
			protected set { SetNonPersistentPropertyValue(CreditBalanceInfo, ref creditBalance, value); }
		}

		ZDecimal creditBalance;

		public ZPropertyInfo CreditBalanceInfo
		{
			get { return GetZPropertyInfo(Schema.CreditBalance); }
		}

		public ZDecimal CreditLimit
		{
			get { return creditLimit; }
			protected set { SetNonPersistentPropertyValue(CreditLimitInfo, ref creditLimit, value); }
		}

		ZDecimal creditLimit;

		public ZPropertyInfo CreditLimitInfo
		{
			get { return GetZPropertyInfo(Schema.CreditLimit); }
		}

		public ZString StandardPaymentTerms
		{
			get { return standardPaymentTerms; }
			protected set { SetNonPersistentPropertyValue(StandardPaymentTermsInfo, ref standardPaymentTerms, value); }
		}

		ZString standardPaymentTerms;

		public ZPropertyInfo StandardPaymentTermsInfo
		{
			get { return GetZPropertyInfo(Schema.StandardPaymentTerms); }
		}

		public ZString DisbursementPaymentTerms
		{
			get { return disbursementPaymentTerms; }
			protected set { SetNonPersistentPropertyValue(DisbursementPaymentTermsInfo, ref disbursementPaymentTerms, value); }
		}

		ZString disbursementPaymentTerms;

		public ZPropertyInfo DisbursementPaymentTermsInfo
		{
			get { return GetZPropertyInfo(Schema.DisbursementPaymentTerms); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal CurrentStandardOutstanding
		{
			get { return currentStandardOutstanding; }
			protected set { SetNonPersistentPropertyValue(CurrentStandardOutstandingInfo, ref currentStandardOutstanding, value); }
		}

		ZDecimal currentStandardOutstanding;

		public ZPropertyInfo CurrentStandardOutstandingInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentStandardOutstanding); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal FirstAgeingStandardOutstanding
		{
			get { return firstAgeingStandardOutstanding; }
			protected set { SetNonPersistentPropertyValue(FirstAgeingStandardOutstandingInfo, ref firstAgeingStandardOutstanding, value); }
		}

		ZDecimal firstAgeingStandardOutstanding;

		public ZPropertyInfo FirstAgeingStandardOutstandingInfo
		{
			get { return GetZPropertyInfo(Schema.FirstAgeingStandardOutstanding); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal SecondAgeingStandardOutstanding
		{
			get { return secondAgeingStandardOutstanding; }
			protected set { SetNonPersistentPropertyValue(SecondAgeingStandardOutstandingInfo, ref secondAgeingStandardOutstanding, value); }
		}

		ZDecimal secondAgeingStandardOutstanding;

		public ZPropertyInfo SecondAgeingStandardOutstandingInfo
		{
			get { return GetZPropertyInfo(Schema.SecondAgeingStandardOutstanding); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal ThirdAgeingStandardOutstanding
		{
			get { return thirdAgeingStandardOutstanding; }
			protected set { SetNonPersistentPropertyValue(ThirdAgeingStandardOutstandingInfo, ref thirdAgeingStandardOutstanding, value); }
		}

		ZDecimal thirdAgeingStandardOutstanding;

		public ZPropertyInfo ThirdAgeingStandardOutstandingInfo
		{
			get { return GetZPropertyInfo(Schema.ThirdAgeingStandardOutstanding); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalStandardOutstanding
		{
			get { return totalStandardOutstanding; }
			protected set { SetNonPersistentPropertyValue(TotalStandardOutstandingInfo, ref totalStandardOutstanding, value); }
		}

		ZDecimal totalStandardOutstanding;

		public ZPropertyInfo TotalStandardOutstandingInfo
		{
			get { return GetZPropertyInfo(Schema.TotalStandardOutstanding); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal CurrentBatchedTotal
		{
			get { return fCurrentBatchedTotal; }
			protected set { SetNonPersistentPropertyValue(CurrentBatchedTotalInfo, ref fCurrentBatchedTotal, value); }
		}

		ZDecimal fCurrentBatchedTotal;

		public ZPropertyInfo CurrentBatchedTotalInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentBatchedTotal); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal OnePeriodBatchedTotal
		{
			get { return fOnePeriodBatchedTotal; }
			protected set { SetNonPersistentPropertyValue(OnePeriodBatchedTotalInfo, ref fOnePeriodBatchedTotal, value); }
		}

		ZDecimal fOnePeriodBatchedTotal;

		public ZPropertyInfo OnePeriodBatchedTotalInfo
		{
			get { return GetZPropertyInfo(Schema.OnePeriodBatchedTotal); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TwoPeriodBatchedTotal
		{
			get { return fTwoPeriodBatchedTotal; }
			protected set { SetNonPersistentPropertyValue(TwoPeriodBatchedTotalInfo, ref fTwoPeriodBatchedTotal, value); }
		}

		ZDecimal fTwoPeriodBatchedTotal;

		public ZPropertyInfo TwoPeriodBatchedTotalInfo
		{
			get { return GetZPropertyInfo(Schema.TwoPeriodBatchedTotal); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal ThreePeriodBatchedTotal
		{
			get { return fThreePeriodBatchedTotal; }
			protected set { SetNonPersistentPropertyValue(ThreePeriodBatchedTotalInfo, ref fThreePeriodBatchedTotal, value); }
		}

		ZDecimal fThreePeriodBatchedTotal;

		public ZPropertyInfo ThreePeriodBatchedTotalInfo
		{
			get { return GetZPropertyInfo(Schema.ThreePeriodBatchedTotal); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal BatchedOutstandingAmountTotal
		{
			get { return fBatchedOutstandingAmountTotal; }
			protected set { SetNonPersistentPropertyValue(BatchedOutstandingAmountTotalInfo, ref fBatchedOutstandingAmountTotal, value); }
		}

		ZDecimal fBatchedOutstandingAmountTotal;

		public ZPropertyInfo BatchedOutstandingAmountTotalInfo
		{
			get { return GetZPropertyInfo(Schema.BatchedOutstandingAmountTotal); }
		}

		public ZInt AverageDaysFromInvoiceDateToFullyPaidDate
		{
			get { return averageDaysFromInvoiceDateToFullyPaidDate; }
			protected set { SetNonPersistentPropertyValue(AverageDaysFromInvoiceDateToFullyPaidDateInfo, ref averageDaysFromInvoiceDateToFullyPaidDate, value); }
		}
		ZInt averageDaysFromInvoiceDateToFullyPaidDate;

		public ZPropertyInfo AverageDaysFromInvoiceDateToFullyPaidDateInfo
		{
			get { return GetZPropertyInfo(Schema.AverageDaysFromInvoiceDateToFullyPaidDate); }
		}

		public ZInt AverageDaysFromDueDateToFullyPaidDate
		{
			get { return averageDaysFromDueDateToFullyPaidDate; }
			protected set { SetNonPersistentPropertyValue(AverageDaysFromDueDateToFullyPaidDateInfo, ref averageDaysFromDueDateToFullyPaidDate, value); }
		}
		ZInt averageDaysFromDueDateToFullyPaidDate;

		public ZPropertyInfo AverageDaysFromDueDateToFullyPaidDateInfo
		{
			get { return GetZPropertyInfo(Schema.AverageDaysFromDueDateToFullyPaidDate); }
		}

		public ZInt DisbursementAverageDaysFromDueDateToFullyPaid
		{
			get { return disbursementAverageDaysFromDueDateToFullyPaid; }
			protected set { SetNonPersistentPropertyValue(DisbursementAverageDaysFromDueDateToFullyPaidInfo, ref disbursementAverageDaysFromDueDateToFullyPaid, value); }
		}
		ZInt disbursementAverageDaysFromDueDateToFullyPaid;

		public ZPropertyInfo DisbursementAverageDaysFromDueDateToFullyPaidInfo
		{
			get { return GetZPropertyInfo(Schema.DisbursementAverageDaysFromDueDateToFullyPaid); }
		}

		public ZInt StandardAverageDaysFromDueDateToFullyPaid
		{
			get { return standardAverageDaysFromDueDateToFullyPaid; }
			protected set { SetNonPersistentPropertyValue(StandardAverageDaysFromDueDateToFullyPaidInfo, ref standardAverageDaysFromDueDateToFullyPaid, value); }
		}
		ZInt standardAverageDaysFromDueDateToFullyPaid;

		public ZPropertyInfo StandardAverageDaysFromDueDateToFullyPaidInfo
		{
			get { return GetZPropertyInfo(Schema.StandardAverageDaysFromDueDateToFullyPaid); }
		}

		public ZDecimal PostedRevenue
		{
			get { return fPostedRevenue; }
			private set { SetNonPersistentPropertyValue(PostedRevenueInfo, ref fPostedRevenue, value); }
		}

		ZDecimal fPostedRevenue;

		public ZPropertyInfo PostedRevenueInfo
		{
			get { return GetZPropertyInfo(nameof(PostedRevenue)); }
		}

		public ZDecimal Claim
		{
			get { return fClaim; }
			private set { SetNonPersistentPropertyValue(ClaimInfo, ref fClaim, value); }
		}

		ZDecimal fClaim;

		public ZPropertyInfo ClaimInfo
		{
			get { return GetZPropertyInfo(nameof(Claim)); }
		}

		public ZDecimal UnrecognisedWIP
		{
			get { return fUnrecognisedWIP; }
			private set { SetNonPersistentPropertyValue(UnrecognisedWIPInfo, ref fUnrecognisedWIP, value); }
		}

		ZDecimal fUnrecognisedWIP;

		public ZPropertyInfo UnrecognisedWIPInfo
		{
			get { return GetZPropertyInfo(nameof(UnrecognisedWIP)); }
		}

		public ZDecimal RecognisedWIP
		{
			get { return fRecognisedWIP; }
			private set { SetNonPersistentPropertyValue(RecognisedWIPInfo, ref fRecognisedWIP, value); }
		}

		ZDecimal fRecognisedWIP;

		public ZPropertyInfo RecognisedWIPInfo
		{
			get { return GetZPropertyInfo(nameof(RecognisedWIP)); }
		}

		public ZDecimal SumOfTotalWIPAndRevenue
		{
			get { return fSumOfTotalWIPAndRevenue; }
			private set { SetNonPersistentPropertyValue(SumOfTotalWIPAndRevenueInfo, ref fSumOfTotalWIPAndRevenue, value); }
		}

		ZDecimal fSumOfTotalWIPAndRevenue;

		public ZPropertyInfo SumOfTotalWIPAndRevenueInfo
		{
			get { return GetZPropertyInfo(nameof(SumOfTotalWIPAndRevenue)); }
		}

		public OrgHeader Organisation
		{
			get
			{
				return Factory.Load<OrgHeader>(OrganisationPK);
			}
		}

		OrgHeader SettlementGroup
		{
			get
			{
				OrgHeader result = null;
				if (Organisation == null)
				{
					result = null;
				}
				else if (Organisation.ARSettlementGroupPK.IsValid)
				{
					var settlementGroupOrg = Factory.Load<OrgHeader>(Organisation.ARSettlementGroupPK);
					if (settlementGroupOrg != null)
					{
						result = settlementGroupOrg;
					}
				}
				else
				{
					result = Organisation;
				}

				return result;
			}
		}

		public ZString SettlementGroupCode
		{
			get
			{
				return SettlementGroup == null ? ZString.Empty : SettlementGroup.OH_Code;
			}
		}

		public ZPropertyInfo SettlementGroupCodeInfo
		{
			get { return GetZPropertyInfo(nameof(SettlementGroupCode)); }
		}

		public ZBool UseSettlementGroupCreditLimit
		{
			get
			{
				return Organisation != null && Organisation.CompanyData != null ? Organisation.CompanyData.OB_ARUseSettlementGroupCreditLimit : ZBool.False;
			}
		}

		public ZPropertyInfo UseSettlementGroupCreditLimitInfo
		{
			get { return GetZPropertyInfo(nameof(UseSettlementGroupCreditLimit)); }
		}

		public ZString SettlementGroupString
		{
			get
			{
				return Factory.GetCachedValue(GetSettlementGroupStringCachingKey(), delegate
				{
					var query = new ZQuery(OrgRelatedPartySchema.PR_OH_RelatedParty, OrganisationPK);
					query.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, SQLComparisonOperator.NotEqual, OrganisationPK);
					query.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ARSettlementGroup);
					query.AddToFilter(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);

					var use = Factory.ExistsInDatabase(OrgRelatedPartySchema.Constants.TableName, query);

					return use ? OtherDebtorsUsingCurrentSettlementGroupString : ZString.Empty;
				});
			}
		}

		string GetSettlementGroupStringCachingKey()
		{
			return "SettlementGroupString: " + OrganisationPK;
		}

		void ClearSettlementGroupStringCache()
		{
			Factory.ClearCachedValue<ZString>(GetSettlementGroupStringCachingKey());
		}

		public ZPropertyInfo SettlementGroupStringInfo
		{
			get { return GetZPropertyInfo(nameof(SettlementGroupString)); }
		}

		ZString OtherDebtorsUsingCurrentSettlementGroupString
		{
			get
			{
				return Res.GetString("7164DE64-412A-45BF-8663-612A87D13EAE", "Other debtors are using the credit limit of this Settlement Group.");
			}
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList AgingOptionsList
		{
			get
			{
				if (fAgingOptionsList == null)
				{
					fAgingOptionsList = AccountingConstants.AgingOptions.CodeList;
				}
				return fAgingOptionsList;
			}
		}
		CodeDescriptionPairList fAgingOptionsList;

		#endregion
	}
}
