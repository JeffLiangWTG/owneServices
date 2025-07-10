using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class APPayment : Payment, IDocManagerSupport, IEDocsParsingSupport
	{
		public APPayment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_InvoiceAmount), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_OSTotal), ConcurrencyPolicy.Strict);
		}

		public delegate void HotChequeSelectedHandler(object sender, HotChequeLink link);
		public event HotChequeSelectedHandler DisplayHotCheques;

		public delegate void PaymentFieldsUneditableHandler(object sender, string message);
		public event PaymentFieldsUneditableHandler NotifyUserPaymentUneditable;

		public static ZString DefaultDescriptionForJobRelatedPayment
		{
			get
			{
				return (AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(LedgerTypes.AccountsPayable + TransactionTypes.Payment,
										 LedgerTypes.AccountsPayable + " " +
		 new CodeDescriptionPairList(OLookUpEditType.TransactionTypes).GetDescriptionFromCode(TransactionTypes.Payment)));
			}
		}

#if DEBUG
		public void SetValues_ForTestOnly(Job job, Charge charge, ZDateTime postingTime)
		{
			AH_Desc = DefaultDescriptionForJobRelatedPayment + " " + job.JH_JobNum;
			AH_NumberOfSupportingDocuments = AccountingConfigurationRegistry.Instance.GetVoucherNoOfAttchmentsFromCode(LedgerTypes.AccountsPayable + TransactionTypes.Payment, 1);
			AH_InvoiceDate = postingTime;
			AH_RX_NKTransactionCurrency = charge.JR_RX_NKCostCurrency;

			AH_PostDate = postingTime;
			AH_ReceiptType = charge.JR_PaymentType;

			AH_OH = charge.JR_OH_CostAccount;
			AH_AB = charge.JR_AB; // Note: Bank must be set before Exchange Rate because setting Bank sets Currency
			if (charge.ChequeBook != null)
			{
				this.ChequeBook = charge.ChequeBook.PK;
			}
			if (!charge.IsChequeNumberAutoAllocated)
			{
				AH_ChequeOrReference = charge.JR_ChequeNo; // Note: ChequeOrReference must be set after ReceiptType, BankAccount and ChequeBook
			}
			if (AH_RX_NKTransactionCurrency != AH_Calc_LocalRXCode)
			{
				AH_ExchangeRate = charge.JR_OSCostExRate; // Note: ExchangeRate should be set after Organisation
			}

			if (AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.Value)
			{
				AH_GB = GlbBranch.CurrentBranch.PK;
			}
			else
			{
				AH_GB = job.JH_GB;
			}
			AH_GE = job.JH_GE;

			AH_InvoiceApproved = ZBool.True;
			AH_FullyPaidDate = postingTime;

			RelatedCharges.Add(charge);
		}
#endif

		public void ImportSelectedHotCheque(AccHotCheque hotCheque)
		{
			if (hotCheque != null)
			{
				SetHotChequeInactiveWhenPosting(hotCheque);
				BeginImportingHotCheque();
				PopulateFieldsUsingHotCheque(hotCheque);
				FinishImportingHotCheque();
			}
		}

		public AccHotCheque ImportedHotCheque
		{
			get { return fImportedHotCheque; }
		}

		public bool IsHotChequeImported
		{
			get { return fImportedHotCheque != null; }
		}

		protected override ZPropertyInfo[] GetPropertiesWithStrictConcurrency()
		{
			var additionalPropertiesWithStrictConcurrency = new ZPropertyInfo[] { AH_InvoiceAmountInfo, AH_OSTotalInfo };
			return base.GetPropertiesWithStrictConcurrency().Concat(additionalPropertiesWithStrictConcurrency).ToArray();
		}

		#region Error Strings

		public static string AH_ABError
		{
			get { return Res.GetString("b5306186-1e55-42db-b922-bc45fa2c573b", "Entered bank must be the same as the bank on the imported hot check"); }
		}

		public static string AH_ChequeOrReferenceError
		{
			get { return Res.GetString("9998d625-91e3-4926-bca3-3990fcb491cb", "Entered reference number must be the same as the reference number on the imported hot check"); }
		}

		public static string ChequeBookError
		{
			get { return Res.GetString("5408c603-c0c7-4f45-9bd0-9dd0b5e4589d", "Entered check book must be the same as the check book on the imported hot check"); }
		}

		public static string AH_OSExTaxActualError
		{
			get { return Res.GetString("b5d858dd-427d-466d-8429-bb2e2611684b", "Entered amount must be the same as the amount used on the imported hot check"); }
		}

		public static string AH_ReceiptTypeError
		{
			get { return Res.GetString("0a95d46e-cc96-449e-92c5-d5ce1c969ee1", "Payment must be a check payment because hot check was imported"); }
		}

		public string GetAH_OSExTaxMaximumError(decimal maximumAmount)
		{
			return Res.GetString("05cdc74c-f0b8-4872-9770-bd869bfc0d53", "Entered amount must be less than or equal to the amount used on the imported hot check (") + Utilities.Round(maximumAmount, 2) + ")";
		}

		#endregion

		#region Business Object Override

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("08564fee-b62d-42b4-9bc6-e3cb6cb4752b", "Accounts Payable Payment"); }
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();
			if (IsHotChequeImported)
			{
				fImportedHotCheque.AQ_AH = this.PK;
			}
		}

		#endregion

		#region Property Override

		protected override ZGuid AH_OHCore
		{
			get { return base.AH_OHCore; }
			set
			{
				base.AH_OHCore = value;
				AccHotChequeCollection hotCheques = GetActiveHotCheques();
				if (hotCheques.Count > 0)
				{
					FireDisplayHotCheques(hotCheques);
				}
			}
		}

		public override ZString AH_ReceiptType
		{
			get { return base.AH_ReceiptType; }
			set
			{
				if (IsHotChequeImported && !fIsImportingHotCheque)
				{
					if (value != ReceiptTypes.Cheque)
					{
						FireNotifyUserPaymentUneditable(AH_ReceiptTypeError);
					}
					AH_ReceiptTypeInfo.RefreshBinding();
				}
				else
				{
					base.AH_ReceiptType = value;
				}
			}
		}

		public override ZGuid AH_AB
		{
			get { return base.AH_AB; }
			set
			{
				if (IsHotChequeImported && !fIsImportingHotCheque)
				{
					if (fImportedHotCheque.ChequeBook != null &&
						fImportedHotCheque.ChequeBook.BankAccount.PK != value)
					{
						FireNotifyUserPaymentUneditable(AH_ABError);
					}
					AH_ABInfo.RefreshBinding();
				}
				else
				{
					base.AH_AB = value;
				}
			}
		}

		public override ZString AH_ChequeOrReference
		{
			get { return base.AH_ChequeOrReference; }
			set
			{
				if (IsHotChequeImported && !fIsImportingHotCheque)
				{
					if (fImportedHotCheque.AQ_ChequeNumber != value)
					{
						FireNotifyUserPaymentUneditable(AH_ChequeOrReferenceError);
					}
					AH_ChequeOrReferenceInfo.RefreshBinding();
				}
				else
				{
					base.AH_ChequeOrReference = value;
				}
			}
		}

		public override ZGuid ChequeBook
		{
			get { return base.ChequeBook; }
			set
			{
				if (IsHotChequeImported && !fIsImportingHotCheque)
				{
					if (fImportedHotCheque.AQ_AK != value)
					{
						FireNotifyUserPaymentUneditable(ChequeBookError);
					}
					ChequeBookInfo.RefreshBinding();
				}
				else
				{
					base.ChequeBook = value;
				}
			}
		}

		public override ZDecimal AH_OSExTaxAmount
		{
			get { return base.AH_OSExTaxAmount; }
			set
			{
				if (IsHotChequeImported && !fIsImportingHotCheque)
				{
					if (fImportedHotCheque.AQ_ActualOrMaxIndicator == ActualOrMaxIndicator.Actual &&
						fImportedHotCheque.AQ_Amount != value)
					{
						FireNotifyUserPaymentUneditable(AH_OSExTaxActualError);
					}
					else if (fImportedHotCheque.AQ_ActualOrMaxIndicator == ActualOrMaxIndicator.Max)
					{
						if (fImportedHotCheque.AQ_Amount < value)
						{
							FireNotifyUserPaymentUneditable(GetAH_OSExTaxMaximumError(fImportedHotCheque.AQ_Amount));
						}
						else
						{
							base.AH_OSExTaxAmount = value;
						}
					}
					AH_OSExTaxAmountInfo.RefreshBinding();
				}
				else
				{
					base.AH_OSExTaxAmount = value;
				}
			}
		}

		public void UpdateChequeNumberForRelatedChargesAndJobConsolCost()
		{
			foreach (Charge listCharge in RelatedCharges)
			{
				listCharge.JR_ChequeNo = AH_ChequeOrReference;
			}
			SetChequeNumberOnRelatedJobConsolCost(AH_ChequeOrReference);
		}

		void SetChequeNumberOnRelatedJobConsolCost(ZString value)
		{
			if (RelatedConsolCostColleciton.Length > 0)
			{
				foreach (JobConsolCost relatedConsolCost in RelatedConsolCostColleciton)
				{
					relatedConsolCost.E6_ChequeOrReference = value;
				}
			}
		}

		JobConsolCost[] RelatedConsolCostColleciton
		{
			get
			{
				if (fRelatedConsolCostColleciton == null)
				{
					List<ZGuid> consolCostsPK = new List<ZGuid>();

					foreach (Charge listCharge in RelatedCharges)
					{
						if (listCharge.JR_E6.IsValid)
						{
							consolCostsPK.Add(listCharge.JR_E6);
						}
					}
					ZQuery filter = new ZQuery();
					if (consolCostsPK.Count == 0)
					{
						filter.IsNoResultQuery = true;
					}
					filter.AddToFilter(JobConsolCostSchema.PK, consolCostsPK);
					fRelatedConsolCostColleciton = Factory.Load<JobConsolCost>(filter);
				}
				return fRelatedConsolCostColleciton;
			}
		}
		JobConsolCost[] fRelatedConsolCostColleciton;

		#endregion

		#region IDocManagerSupport Members

		public override DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new PaymentDocManagerInfo(this, Constants.DocManagerCodes.APPayment)); }
		}
		PaymentDocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		#region Implementation

		protected override ZString Ledger
		{
			get { return LedgerTypes.AccountsPayable; }
		}

		protected override void SetDefaultBankAccount()
		{
			SetDefaultBankAccountAP();
		}

#if DEBUG
		public void SetAmounts_ForTestsOnly(Charge charge)
		{
			AH_InvoiceAmount += charge.JR_Calc_LocalCostAmtWithGST;
			AH_OSTotal += AH_RX_NKTransactionCurrency == AH_Calc_LocalRXCode ? charge.JR_Calc_LocalCostAmtWithGST : charge.JR_Calc_OSCostAmtWithGST;

			if (charge.ChequeBook != null && ChequeBook.IsEmpty)
			{
				ChequeBook = charge.ChequeBook.PK;
			}

			AddRelatedCharge(charge);
		}
#endif

		public override ZString ExchangeRateType
		{
			get { return Core.Constants.ExchangeRateTypes.Code.BuyRate; }
		}

		List<Charge> fRelatedCharges;
		protected List<Charge> RelatedCharges
		{
			get
			{
				if (fRelatedCharges == null)
				{
					fRelatedCharges = new List<Charge>();
				}

				return fRelatedCharges;
			}
		}

		public void AddRelatedCharge(Charge charge)
		{
			if (!RelatedCharges.Contains(charge))
			{
				RelatedCharges.Add(charge);
			}
		}

		protected override void UpdateChequeNumberOnRelatedObjects(string value)
		{
			base.UpdateChequeNumberOnRelatedObjects(value);

			UpdateChequeNumberForRelatedChargesAndJobConsolCost();
		}

		#region Unmatch

		protected override void UnmatchCore(ZDecimal matchLinkAmount, ZDecimal matchLinkOSAmount)
		{
			base.UnmatchCore(matchLinkAmount, matchLinkOSAmount);
			ClearPaymentDetailsOnThePaidItems();
		}

		#endregion

		#region ClearPaymentDetailsOnThePaidItems

		void ClearPaymentDetailsOnThePaidItems()
		{
			TransactionHeader[] paidItems = GetPaidItems();
			Charge[] charges = GetChargesOfPaidItems(paidItems);
			foreach (Charge aCharge in charges)
			{
				aCharge.ClearPaymentDetails();
			}
		}

		#endregion

		#region GetPaidItems

		TransactionHeader[] GetPaidItems()
		{
			List<ZGuid> pKs = new List<ZGuid>();

			BusinessObjectFactory tempFactory = new BusinessObjectFactory();
			ZQuery paymentMatchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, PK);
			TransactionMatchLink paymentMatchLink = tempFactory.LoadTop1<TransactionMatchLink>(paymentMatchLinkFilter);
			if (paymentMatchLink != null)
			{
				ZQuery matchLinksForPaidItemsFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_MatchGroupNum, paymentMatchLink.AP_MatchGroupNum);
				matchLinksForPaidItemsFilter.AddToFilter(AccTransactionMatchLinkSchema.AP_AH, SQLComparisonOperator.NotEqual, PK);
				TransactionMatchLink[] paidItems = tempFactory.Load<TransactionMatchLink>(matchLinksForPaidItemsFilter);

				foreach (TransactionMatchLink matchLink in paidItems)
				{
					if (!pKs.Contains(matchLink.AP_AH))
					{
						pKs.Add(matchLink.AP_AH);
					}
				}
			}

			if (pKs.Count > 0)
			{
				ZQuery paidSameCompany = new ZQuery(AccTransactionHeaderSchema.PK, pKs);
				paidSameCompany.AddToFilter(AccTransactionHeaderSchema.AH_GC, AH_GC);

				return Factory.Load<TransactionHeader>(paidSameCompany);
			}
			else
			{
				return System.Array.Empty<TransactionHeader>();
			}
		}

		#endregion

		#region GetChargesOfPaidItems

		Charge[] GetChargesOfPaidItems(TransactionHeader[] paidItems)
		{
			List<ZGuid> pKs = new List<ZGuid>();

			foreach (TransactionHeader paidItem in paidItems)
			{
				if (paidItem is APInvoice || paidItem is APCreditNote)
				{
					foreach (InvoicingLineBase line in ((InvoicingBase)paidItem).Lines)
					{
						if (!pKs.Contains(line.PK))
						{
							pKs.Add(line.PK);
						}
					}
				}
			}

			var result = new List<Charge>();
			if (pKs.Count > 0)
			{
				var chunkedPKs = AccountingUtils.ChunksOf(pKs, AccountingUtils.ChunkBatchSize);
				foreach (IList<ZGuid> groupOfPKs in chunkedPKs)
				{
					ZQuery chargesWithPaymentDetailsQuery = new ZQuery(JobChargeSchema.JR_AL_APLine, groupOfPKs);
					chargesWithPaymentDetailsQuery.AddToFilter(JobChargeSchema.JR_PaymentType, PaymentType);
					chargesWithPaymentDetailsQuery.AddToFilter(JobChargeSchema.JR_AB, AH_AB);
					chargesWithPaymentDetailsQuery.AddToFilter(JobChargeSchema.JR_AK, ChequeBook);
					chargesWithPaymentDetailsQuery.AddToFilter(JobChargeSchema.JR_ChequeNo, AH_ChequeOrReference);
					result.AddRange(Factory.Load<Charge>(chargesWithPaymentDetailsQuery));
				}
			}
			return result.ToArray();
		}

		#endregion

		#region Hot Cheques

		AccHotChequeCollection GetActiveHotCheques()
		{
			ZQuery query = new ZQuery(AccHotChequeSchema.AQ_OH, AH_OH);
			query.AddToFilter(AccHotChequeSchema.AQ_Cancelled, false);
			query.AddToFilter(AccHotChequeSchema.AQ_AH, SQLComparisonOperator.Equal, null);
			AccHotChequeCollection hotCheques = new AccHotChequeCollection(Factory, query);
			hotCheques.Load();
			return hotCheques;
		}

		public void SetHotChequeInactiveWhenPosting(AccHotCheque hotCheque)
		{
			fImportedHotCheque = hotCheque;
		}

		void PopulateFieldsUsingHotCheque(AccHotCheque hotCheque)
		{
			AH_ReceiptType = ReceiptTypes.Cheque;
			AH_AB = (hotCheque.ChequeBook != null && hotCheque.ChequeBook.BankAccount != null ? hotCheque.ChequeBook.BankAccount.PK : ZGuid.Empty);
			ChequeBook = hotCheque.AQ_AK;
			AH_ChequeOrReference = hotCheque.AQ_ChequeNumber;
			AH_OSExTaxAmount = hotCheque.AQ_Amount;
		}

		void FireDisplayHotCheques(AccHotChequeCollection hotCheques)
		{
			if (DisplayHotCheques != null)
			{
				HotChequeLink link = new HotChequeLink(hotCheques);
				DisplayHotCheques(this, link);
			}
		}

		void FireNotifyUserPaymentUneditable(string message)
		{
			if (NotifyUserPaymentUneditable != null && !fIsImportingHotCheque)
			{
				NotifyUserPaymentUneditable(this, message);
			}
		}

		void BeginImportingHotCheque()
		{
			fIsImportingHotCheque = true;
		}

		void FinishImportingHotCheque()
		{
			fIsImportingHotCheque = false;
		}

		bool fIsImportingHotCheque;

		AccHotCheque fImportedHotCheque;

		#endregion

		#endregion

		#region RelevantCAPJournals

		public List<Journal.Journal> RelatedCAPJOurnals { get; private set; }

		public void LoadCAPJournals()
		{
			var cashAdvanceRequestHeaderInfoLoader = new CashAdvanceRequestInfoByPaymentOrReceipt(this);
			RelatedCAPJOurnals = cashAdvanceRequestHeaderInfoLoader.CashAdvanceJournals;
		}

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			var checker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();

			if (checker.IsPayablesCashAdvanceFunctionalityEnabled &&
				!checker.IsManualSettingOfPayablesCashAdvanceRequestStatusToPaidAllowed &&
				(RelatedCAPJOurnals?.Any() ?? false) &&
				ReverseTransaction is TransactionHeader &&
				IsReversed)
			{
				var cahJournalReverser = new PaidCashAdvanceRequestReverser(this);
				foreach (var cahUpdaterJournal in RelatedCAPJOurnals.OfType<IJournalAssociatedToCashAdvanceRequest>())
				{
					cahUpdaterJournal.Accept(cahJournalReverser);
				}
			}

			base.OnFactorySavingBeforeTransactionCore();
		}
	}
}
