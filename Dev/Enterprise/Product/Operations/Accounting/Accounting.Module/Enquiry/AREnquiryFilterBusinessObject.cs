using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AREnquiryFilterBusinessObject : APEnquiryFilterBusinessObject
	{
		public AREnquiryFilterBusinessObject()
		{
			AgingDateType = AccountingConfigurationRegistry.Instance.AgingOptionReceivables.Value;
		}

		protected override ZBool IsPayableModule => false;

		#region Read Only Information

		public override void SetInfoValues()
		{
			LastPurchaseSale = ARAPEnquiryData.GetLastSale(OrganisationPK);
			LastPaymentReceipt = ARAPEnquiryData.GetLastReceipt(OrganisationPK);
			ZDateTime date = ARAPEnquiryData.GetLastReceiptDate(OrganisationPK);
			if (!date.IsValid)
			{
				date = ZDateTime.Empty;
			}
			LastPaymentReceiptDate = date.Date.ToString();

			MTD_PTDSales = ARAPEnquiryData.GetPTDSales(OrganisationPK);
			YTDPurchaseSales = ARAPEnquiryData.GetYTDSales(OrganisationPK);
			LYRPurchaseSales = ARAPEnquiryData.GetLYRSales(OrganisationPK);

			StandardPaymentTerms = ARAPEnquiryData.GetStandardInvoiceTerms(OrganisationPK);

			DisbursementPaymentTerms = ARAPEnquiryData.GetDisbursementInvoiceTerms(OrganisationPK);

			SetInfoValuesOfInvoiceAging();

			OrgHeader currentOrgHeader = Factory.Load<OrgHeader>(OrganisationPK);
			if (currentOrgHeader != null)
			{
				OM_ARTreatDisbursementsAsStandardValue = currentOrgHeader.MiscServ.OM_ARTreatDisbursementsAsStandardValue;

				if (currentOrgHeader.MiscServ.IsInvalidGlobalCreditCurrencyOrMissingExRate)
				{
					GlobalCreditAvailableInfo.AddWarning(currentOrgHeader.InvalidGlobalCreditCurrencyOrMissingExRateMessage);
				}
			}

			AverageDaysFromDueDateToFullyPaidDate = ARAPEnquiryData.GetARAverageDaysFromDueDateToFullyPaidDate(OrganisationPK);
			AverageDaysFromInvoiceDateToFullyPaidDate = ARAPEnquiryData.GetARAverageDaysFromInvoiceDateToFullyPaidDate(OrganisationPK);
			DisbursementAverageDaysFromDueDateToFullyPaid = ARAPEnquiryData.GetDisbursementARAverageDaysFromDueDateToFullyPaidDate(OrganisationPK);
			StandardAverageDaysFromDueDateToFullyPaid = ARAPEnquiryData.GetStandardARAverageDaysFromDueDateToFullyPaidDate(OrganisationPK);

			GlobalCreditGroup = ARAPEnquiryData.GetGlobalCreditGroupName(OrganisationPK);
			GlobalCreditLimit = ARAPEnquiryData.GetGlobalCreditLimit(OrganisationPK);
			GlobalCreditCurrency = ARAPEnquiryData.GetGlobalCreditCurrency(OrganisationPK);
			GlobalCreditAvailable = ARAPEnquiryData.GetGlobalCreditAvailable(OrganisationPK);

			SetUnpostedRevenueFields();

			StandardOverdueAmount = ARAPEnquiryData.GetStandardAROverdueAmount(OrganisationPK);
			DisbursementOverdueAmount = ARAPEnquiryData.GetDisbursementAROverdueAmount(OrganisationPK);
			TotalOverdueAmount = StandardOverdueAmount + DisbursementOverdueAmount;

			if (SettlementGroup != null && (UseSettlementGroupCreditLimit || !string.IsNullOrEmpty(SettlementGroupString)))
			{
				CreditLimit = ARAPEnquiryData.GetARCreditLimit(SettlementGroup.PK.ToGuid());
				CreditBalance = ARAPEnquiryData.GetSettlementGroupARCreditBalance(SettlementGroup);
			}
			else
			{
				CreditLimit = ARAPEnquiryData.GetARCreditLimit(OrganisationPK);
				CreditBalance = ARAPEnquiryData.CalculatedCreditBalance(CreditLimit, PostedRevenue, RecognisedWIP, UnrecognisedWIP, Claim);
			}
		}

		public override void SetInfoValuesOfInvoiceAging()
		{
			if (OrganisationPK != Guid.Empty)
			{
				CurrentStandardOutstanding = ARAPEnquiryData.GetCurrentARAgeOutStandingAmount(OrganisationPK, AgingDateType);
				FirstAgeingStandardOutstanding = ARAPEnquiryData.GetSingleARAgeOutStandingAmount(OrganisationPK, AgingDateType);
				SecondAgeingStandardOutstanding = ARAPEnquiryData.GetTwoARAgeOutstandingAmount(OrganisationPK, AgingDateType);
				ThirdAgeingStandardOutstanding = ARAPEnquiryData.GetThreeARAgeOutstandingAmount(OrganisationPK, AgingDateType);
				TotalStandardOutstanding = CurrentStandardOutstanding + FirstAgeingStandardOutstanding + SecondAgeingStandardOutstanding + ThirdAgeingStandardOutstanding;

				CurrentDisbursementOutstanding = ARAPEnquiryData.GetDSBCurrentAgeOutstandingAmount(OrganisationPK, AgingDateType);
				FirstAgeingDisbursementOutstanding = ARAPEnquiryData.GetDSBSingleAgeOutstandingAmount(OrganisationPK, AgingDateType);
				SecondAgeingDisbursementOutstanding = ARAPEnquiryData.GetDSBTwoAgeOutstandingAmount(OrganisationPK, AgingDateType);
				ThirdAgeingDisbursementOutstanding = ARAPEnquiryData.GetDSBThreeAgeOutstandingAmount(OrganisationPK, AgingDateType);
				TotalDisbursementOutstanding = CurrentDisbursementOutstanding + FirstAgeingDisbursementOutstanding + SecondAgeingDisbursementOutstanding + ThirdAgeingDisbursementOutstanding;

				CurrentTotal = CurrentStandardOutstanding + CurrentDisbursementOutstanding;
				OnePeriodTotal = FirstAgeingDisbursementOutstanding + FirstAgeingStandardOutstanding;
				TwoPeriodTotal = SecondAgeingDisbursementOutstanding + SecondAgeingStandardOutstanding;
				ThreePeriodTotal = ThirdAgeingDisbursementOutstanding + ThirdAgeingStandardOutstanding;
				TotalOutstandingAmount = TotalDisbursementOutstanding + TotalStandardOutstanding;

				CurrentBatchedTotal = ARAPEnquiryData.GetBatchedCurrentAgeOutstandingAmount(OrganisationPK, AgingDateType);
				OnePeriodBatchedTotal = ARAPEnquiryData.GetBatchedSingleAgeOutstandingAmount(OrganisationPK, AgingDateType);
				TwoPeriodBatchedTotal = ARAPEnquiryData.GetBatchedTwoAgeOutstandingAmount(OrganisationPK, AgingDateType);
				ThreePeriodBatchedTotal = ARAPEnquiryData.GetBatchedThreeAgeOutstandingAmount(OrganisationPK, AgingDateType);
				BatchedOutstandingAmountTotal = CurrentBatchedTotal + OnePeriodBatchedTotal + TwoPeriodBatchedTotal + ThreePeriodBatchedTotal;
			}
		}

		public override CodeDescriptionPairList HasRelatedClaimFilterOptionsList
		{
			get
			{
				if (fHasRelatedClaimFilterOptionsList == null)
				{
					fHasRelatedClaimFilterOptionsList = new CodeDescriptionPairList();

					fHasRelatedClaimFilterOptionsList.AddPair(HasRelatedARClaim, Res.GetString("3738d69c-d8c8-44dc-924d-d03cf2de65c3", "Only Transaction only with Related AR Claim"));
					fHasRelatedClaimFilterOptionsList.AddPair(HasNoRelatedARClaim, Res.GetString("95de0505-465c-4484-af71-b4d8a2e1ba65", "Only Transactions with NO Related AR Claim"));
				}

				return fHasRelatedClaimFilterOptionsList;
			}
		}

		protected override bool IsDebtor
		{
			get { return true; }
		}

		protected override void ClearInfoValues()
		{
			base.ClearInfoValues();

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

			PostedRevenue = 0.0M;
			Claim = 0.0M;
			UnrecognisedWIP = 0.0M;
			RecognisedWIP = 0.0M;
			SumOfTotalWIPAndRevenue = 0.0M;

			GlobalCreditGroup = ZString.Empty;
			GlobalCreditLimit = 0.0M;
			GlobalCreditCurrency = ZString.Empty;
			GlobalCreditAvailable = 0.0M;

			ClearSettlementGroupStringCache();

			StandardOverdueAmount = 0.0m;
			DisbursementOverdueAmount = 0.0m;
			TotalOverdueAmount = 0.0m;
		}

		void SetUnpostedRevenueFields()
		{
			var unpostedRevenueFieldValuesTuple = ARAPEnquiryData.GetUnpostedRevenueFieldValues(OrganisationPK);
			PostedRevenue = unpostedRevenueFieldValuesTuple.PostedRevenue;
			Claim = unpostedRevenueFieldValuesTuple.Claim;
			UnrecognisedWIP = unpostedRevenueFieldValuesTuple.UnrecognisedWIP;
			RecognisedWIP = unpostedRevenueFieldValuesTuple.RecognisedWIP;
			SumOfTotalWIPAndRevenue = UnrecognisedWIP + RecognisedWIP + PostedRevenue;
		}

		#region Properties

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
			get { return fCurrentDisbursementOutstanding; }
			private set { SetNonPersistentPropertyValue(CurrentDisbursementOutstandingInfo, ref fCurrentDisbursementOutstanding, value); }
		}

		ZDecimal fCurrentDisbursementOutstanding;

		public ZPropertyInfo CurrentDisbursementOutstandingInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentDisbursementOutstanding)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal FirstAgeingDisbursementOutstanding
		{
			get { return fFirstAgeingDisbursementOutstanding; }
			private set { SetNonPersistentPropertyValue(FirstAgeingDisbursementOutstandingInfo, ref fFirstAgeingDisbursementOutstanding, value); }
		}

		ZDecimal fFirstAgeingDisbursementOutstanding;

		public ZPropertyInfo FirstAgeingDisbursementOutstandingInfo
		{
			get { return GetZPropertyInfo(nameof(FirstAgeingDisbursementOutstanding)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal SecondAgeingDisbursementOutstanding
		{
			get { return fSecondAgeingDisbursementOutstanding; }
			private set { SetNonPersistentPropertyValue(SecondAgeingDisbursementOutstandingInfo, ref fSecondAgeingDisbursementOutstanding, value); }
		}

		ZDecimal fSecondAgeingDisbursementOutstanding;

		public ZPropertyInfo SecondAgeingDisbursementOutstandingInfo
		{
			get { return GetZPropertyInfo(nameof(SecondAgeingDisbursementOutstanding)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal ThirdAgeingDisbursementOutstanding
		{
			get { return fThirdAgeingDisbursementOutstanding; }
			private set { SetNonPersistentPropertyValue(ThirdAgeingDisbursementOutstandingInfo, ref fThirdAgeingDisbursementOutstanding, value); }
		}

		ZDecimal fThirdAgeingDisbursementOutstanding;

		public ZPropertyInfo ThirdAgeingDisbursementOutstandingInfo
		{
			get { return GetZPropertyInfo(nameof(ThirdAgeingDisbursementOutstanding)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalDisbursementOutstanding
		{
			get { return fTotalDisbursementOutstanding; }
			private set { SetNonPersistentPropertyValue(TotalDisbursementOutstandingInfo, ref fTotalDisbursementOutstanding, value); }
		}

		ZDecimal fTotalDisbursementOutstanding;

		public ZPropertyInfo TotalDisbursementOutstandingInfo
		{
			get { return GetZPropertyInfo(nameof(TotalDisbursementOutstanding)); }
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
			get { return GetZPropertyInfo(nameof(CurrentBatchedTotal)); }
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
			get { return GetZPropertyInfo(nameof(OnePeriodBatchedTotal)); }
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
			get { return GetZPropertyInfo(nameof(TwoPeriodBatchedTotal)); }
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
			get { return GetZPropertyInfo(nameof(ThreePeriodBatchedTotal)); }
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
			get { return GetZPropertyInfo(nameof(BatchedOutstandingAmountTotal)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal OnePeriodTotal
		{
			get { return fOnePeriodTotal; }
			private set { SetNonPersistentPropertyValue(OnePeriodTotalInfo, ref fOnePeriodTotal, value); }
		}

		ZDecimal fOnePeriodTotal;

		public ZPropertyInfo OnePeriodTotalInfo
		{
			get { return GetZPropertyInfo(nameof(OnePeriodTotal)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TwoPeriodTotal
		{
			get { return fTwoPeriodTotal; }
			private set { SetNonPersistentPropertyValue(TwoPeriodTotalInfo, ref fTwoPeriodTotal, value); }
		}

		ZDecimal fTwoPeriodTotal;

		public ZPropertyInfo TwoPeriodTotalInfo
		{
			get { return GetZPropertyInfo(nameof(TwoPeriodTotal)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal ThreePeriodTotal
		{
			get { return fThreePeriodTotal; }
			private set { SetNonPersistentPropertyValue(ThreePeriodTotalInfo, ref fThreePeriodTotal, value); }
		}

		ZDecimal fThreePeriodTotal;

		public ZPropertyInfo ThreePeriodTotalInfo
		{
			get { return GetZPropertyInfo(nameof(ThreePeriodTotal)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TotalOutstandingAmount
		{
			get { return fTotalOutstandingAmount; }
			private set { SetNonPersistentPropertyValue(TotalOutstandingAmountInfo, ref fTotalOutstandingAmount, value); }
		}

		ZDecimal fTotalOutstandingAmount;

		public ZPropertyInfo TotalOutstandingAmountInfo
		{
			get { return GetZPropertyInfo(nameof(TotalOutstandingAmount)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal CurrentTotal
		{
			get { return fCurrentTotal; }
			private set { SetNonPersistentPropertyValue(CurrentTotalInfo, ref fCurrentTotal, value); }
		}

		ZDecimal fCurrentTotal;

		public ZPropertyInfo CurrentTotalInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentTotal)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal OM_ARTreatDisbursementsAsStandardValue
		{
			get { return fOM_ARTreatDisbursementsAsStandardValue; }
			private set { SetNonPersistentPropertyValue(OM_ARTreatDisbursementsAsStandardValueInfo, ref fOM_ARTreatDisbursementsAsStandardValue, value); }
		}

		ZDecimal fOM_ARTreatDisbursementsAsStandardValue;

		public ZPropertyInfo OM_ARTreatDisbursementsAsStandardValueInfo
		{
			get { return GetZPropertyInfo(nameof(OM_ARTreatDisbursementsAsStandardValue)); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
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

		[DecimalPlaces(nameof(LocalDecimals))]
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

		[DecimalPlaces(nameof(LocalDecimals))]
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

		[DecimalPlaces(nameof(LocalDecimals))]
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

		[DecimalPlaces(nameof(LocalDecimals))]
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

		public int GlobalCreditCurrencyDecimals => RefCurrency.LoadFromCurrencyCode(Factory, GlobalCreditCurrency)?.Decimals ?? LocalDecimals;

		ZString fGlobalCreditGroup;

		public ZString GlobalCreditGroup
		{
			get { return fGlobalCreditGroup; }
			private set { SetNonPersistentPropertyValue(GlobalCreditGroupInfo, ref fGlobalCreditGroup, value); }
		}

		public ZPropertyInfo GlobalCreditGroupInfo
		{
			get { return GetZPropertyInfo(nameof(GlobalCreditGroup)); }
		}

		ZDecimal fGlobalCreditLimit;

		[DecimalPlaces(nameof(GlobalCreditCurrencyDecimals))]
		public ZDecimal GlobalCreditLimit
		{
			get { return fGlobalCreditLimit; }
			private set { SetNonPersistentPropertyValue(GlobalCreditLimitInfo, ref fGlobalCreditLimit, value); }
		}

		public ZPropertyInfo GlobalCreditLimitInfo
		{
			get { return GetZPropertyInfo(nameof(GlobalCreditLimit)); }
		}

		ZString fGlobalCreditCurrency;

		public ZString GlobalCreditCurrency
		{
			get { return fGlobalCreditCurrency; }
			private set { SetNonPersistentPropertyValue(GlobalCreditCurrencyInfo, ref fGlobalCreditCurrency, value); }
		}

		public ZPropertyInfo GlobalCreditCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(GlobalCreditCurrency)); }
		}

		ZDecimal fGlobalCreditAvailable;

		[DecimalPlaces(nameof(GlobalCreditCurrencyDecimals))]
		public ZDecimal GlobalCreditAvailable
		{
			get { return fGlobalCreditAvailable; }
			private set { SetNonPersistentPropertyValue(GlobalCreditAvailableInfo, ref fGlobalCreditAvailable, value); }
		}

		public ZPropertyInfo GlobalCreditAvailableInfo
		{
			get { return GetZPropertyInfo(nameof(GlobalCreditAvailable)); }
		}

		OrgHeader Organisation
		{
			get
			{ return Factory.Load<OrgHeader>(OrganisationPK); }
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
				return Res.GetString("224C34AB-6079-424F-8ABB-16C1AA762B91", "Other debtors are using the credit limit of this Settlement Group.");
			}
		}

		#endregion

		#endregion

		#region Lookups

		protected override OrgHeaderCollection GetOrganisationList()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return new OrgHeaderCollection(new BusinessObjectFactory(), query);
		}

		#endregion
	}
}
