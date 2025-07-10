using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class TransactionMatchLink : AccTransactionMatchLink
	{
		public TransactionMatchLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void SetValues(TransactionHeader header)
		{
			AP_AH = header.PK;
			AP_Amount = header.AH_LocalTotal;
			AP_MatchDate = header.AH_PostDate;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TransactionMatchLinkFetchStrategy(this);
		}

		public static INumberFountainProxy MatchGroupNumberFountain
		{
			get { return Env.NumberFountains.MatchNo.GetTodaysPeriodFountain(); }
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();
			if (AP_MatchGroupNum.IsEmpty)
			{
				var matchLinkGroups = ((IBusinessObjectInternals)this).ParentCollections.OfType<TransactionMatchLinkGroup>();
				if (matchLinkGroups.Count() == 1)
				{
					TransactionMatchLinkGroup matchLinks = matchLinkGroups.First();
					string matchGroupNum = MatchGroupNumberFountain.GetNextFormatted(Factory);
					foreach (TransactionMatchLink matchLink in matchLinks)
					{
						matchLink.AP_MatchGroupNum = matchGroupNum;
					}
				}
			}
			if (!IsInDatabase && GlbCompany.CurrentCompany.GC_IsGSTCashBasis && cashBasisVATRecords.Count == 0)
			{
				bool createCashVATRecords = true;
#if DEBUG
				if (Globals.IsTest && SkipCashBasisVATCreationForTestOnly)
				{
					createCashVATRecords = false;
				}
#endif
				if (createCashVATRecords)
				{
					cashBasisVATRecords.AddRange(CashBasisVATManager.CreateRecords(this));
				}
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded)
			{
				cashBasisVATRecords.ForEach(item => item.Delete());
			}

			cashBasisVATRecords.Clear();
		}

		public void Unmatch()
		{
			if (MatchingTransaction is IMatching)
			{
				((IMatching)MatchingTransaction).Unmatch(AP_Amount, AP_OSAmount);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		public UnmatchingResult CanUnmatch
		{
			get
			{
				UnmatchingResult result = UnmatchingResult.DataError;
				if (MatchingTransaction is IMatching)
				{
					IMatching matchedTransaction = MatchingTransaction as IMatching;
					if (matchedTransaction.TransactionType == TransactionTypes.Payment && !CanUnmatchPayment(matchedTransaction.Ledger))
					{
						result = UnmatchingResult.ContainsPayment;
					}
					else if (matchedTransaction.TransactionType == TransactionTypes.Journal && !CanUnmatchJournal(matchedTransaction as Journal))
					{
						if (matchedTransaction.Ledger == LedgerTypes.AccountsReceivable)
						{
							result = UnmatchingResult.ContainsCashAdvanceARJournal;
						}
						else
						{
							result = UnmatchingResult.ContainsCashAdvanceAPJournal;
						}
					}
					else if (MatchingTransaction.Header != null && !MatchingTransaction.Header.OH_IsActive)
					{
						result = UnmatchingResult.DataErrorTransactionOrganisationIsInactive;
					}
					else
					{
						result = matchedTransaction.CanUnmatch(AP_Amount);
					}
				}
				else
				{
					ErrorReporter.ReportOnce(string.Format("Matching Transaction does not implement IMatching interface. Transaction Object Type: '{0}', PK: '{1}', AH_Ledger: '{2}', AH_TransactionType: '{3}', AH_TransactionCount: {4}.",
						MatchingTransaction.GetType(), MatchingTransaction.PK, MatchingTransaction.AH_Ledger, MatchingTransaction.AH_TransactionType, MatchingTransaction.AH_TransactionCount));
				}

				return result;
			}
		}

		bool CanUnmatchJournal(Journal journal)
		{
			bool result = true;

			var ledger = journal.AH_Ledger;
			var category = journal.AH_TransactionCategory;
			if ((ledger == LedgerTypes.AccountsReceivable || ledger == LedgerTypes.AccountsPayable)
				&& (category == Core.Constants.TransactionCategory.Codes.CashAdvanceReceived
						|| category == Core.Constants.TransactionCategory.Codes.CashAdvancePaid
						|| category == Core.Constants.TransactionCategory.Codes.CashAdvanceInvoice))
			{
				result = false;
			}
			return result;
		}

		protected bool CanUnmatchPayment(ZString legerType)
		{
			bool result = false;
			switch (legerType)
			{
				case ZArchitecture.Core.LedgerTypes.AccountsReceivable:
					result = Env.Security.ReceivablesAllowUnmatchingPaymentMatching.IsAllowed;
					break;
				case ZArchitecture.Core.LedgerTypes.AccountsPayable:
					result = Env.Security.PayablesAllowUnmatchingPaymentMatching.IsAllowed;
					break;
			}
			return result;
		}

		public override void Delete()
		{
			try
			{
#if DEBUG
				if (Globals.IsTest && SkipDeletingMatchLinksForTestOnly)
				{
					throw new CannotDeleteException("Could not delete match link.");
				}
#endif
				base.Delete();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(AP_AH
					, CriticalValidationInfoCollectorServiceKeyType.MatchLinkDeletionFailed
					, () => ex.GetExceptionMessageAndStackTrace()
					, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);

				throw;
			}
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal AP_GSTRealised
		{
			get => base.AP_GSTRealised;
			set => base.AP_GSTRealised = value;
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal AP_Amount
		{
			get => base.AP_Amount;
			set => base.AP_Amount = value;
		}

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public override ZDecimal AP_OSAmount
		{
			get => base.AP_OSAmount;
			set => base.AP_OSAmount = value;
		}

		public int OSCurrencyDecimals => TransactionHeader?.OSCurrencyDecimals ?? GlbCompany.CurrentCompany.GetLocalDecimals();

		public int LocalDecimals => TransactionHeader?.Company.GetLocalDecimals() ?? GlbCompany.CurrentCompany.GetLocalDecimals();

		public ZDecimal OSAmount
		{
			get
			{
				return TransactionMatchLinkOSAmountProvider.GetMatchLinkOSAmount(this);
			}
		}

		#region New Bound Properties

		public TransactionHeader MatchingTransaction
		{
			get
			{
				if (fMatchingTransaction == null)
				{
					fMatchingTransaction = Factory.Load<TransactionHeader>(AP_AH);
				}

				return fMatchingTransaction;
			}
		}

		TransactionHeader fMatchingTransaction;

		#endregion

		#region List Properties

		public TransactionHeaderCollection TransactionHeaders
		{
			get
			{
				if (fTransactionHeaders == null)
				{
					fTransactionHeaders = new TransactionHeaderCollection(Factory);
				}

				return fTransactionHeaders;
			}
		}

		TransactionHeaderCollection fTransactionHeaders;

		#endregion

		readonly List<AccCashBasisVAT> cashBasisVATRecords = new List<AccCashBasisVAT>();

#if DEBUG
		internal bool SkipCashBasisVATCreationForTestOnly;
		internal bool SkipDeletingMatchLinksForTestOnly;

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if ((kind & TestBusinessObjectKind.PopulateDependentCollections) != 0)
			{
				TransactionMatchLink matchLink1 = this;
				TransactionMatchLink matchLink2 = null;
				TransactionMatchLinkGroup matchLinkGroup = new TransactionMatchLinkGroup(Factory);
				matchLinkGroup.Add(matchLink1);
				matchLink1.AP_AH = Factory.NewWithValidTestData<APJournal>().PK;
				matchLink2 = matchLinkGroup.AddNew();
				matchLink2.AP_AH = Factory.NewWithValidTestData<ARJournal>().PK;
				matchLink1.AP_MatchGroupNum = "2";
				matchLink2.AP_MatchGroupNum = "3";
				TestObjectCreator.SetupMatchLinkMatchDate(matchLinkGroup);
			}
		}
#endif
	}
}
