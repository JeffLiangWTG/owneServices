using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Unmatching
{
	class UnmatchingRowFetchStrategy : BusinessObjectFetchStrategy
	{
		public UnmatchingRowFetchStrategy(UnmatchingRow row)
			: base(row)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(TransactionMatchLink), BusinessObject.PK);
		}
	}

	[ProvideMetaDataProperty("PropertyReadonlyness", MetaDataTypes.ReadOnly)]
	public partial class UnmatchingRow : NonPersistentBusinessObject, IObsoleteValidation
	{
		public abstract class Schema
		{
			public const string MatchGroupNum = "MatchGroupNum";
			public const string MatchDate = "MatchDate";
			public const string UnmatchDate = "UnmatchDate";
		}

		public UnmatchingRow(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void Initialize(UnmatchingRow copyFrom)
		{
			MatchGroupNum = copyFrom.MatchGroupNum;
			MatchDate = copyFrom.MatchDate;
		}

		public void Initialize(TransactionMatchLink copyFrom)
		{
			MatchGroupNum = copyFrom.AP_MatchGroupNum;
			MatchDate = copyFrom.AP_MatchDate;
		}

		protected override IBusinessObjectFetchStrategy GetFetchStrategy()
		{
			return new UnmatchingRowFetchStrategy(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			UnmatchDate = ZDateTime.Now;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateUnmatchDate();
		}

		#region Properties

		#region MatchGroupNum

		[MaxLength(20)]
		public ZString MatchGroupNum
		{
			get { return fMatchGroupNum; }
			set
			{
				CheckMaximumLength(MatchGroupNumInfo, value);
				fMatchGroupNum = value;
				MatchGroupNumInfo.RefreshBinding();
			}
		}

		ZString fMatchGroupNum;

		public ZPropertyInfo MatchGroupNumInfo
		{
			get { return GetZPropertyInfo(Schema.MatchGroupNum); }
		}

		#endregion

		#region MatchDate

		public ZDateTime MatchDate
		{
			get { return fMatchDate; }
			set
			{
				fMatchDate = value;
				MatchDateInfo.RefreshBinding();
				if (MatchDate > UnmatchDate)
				{
					UnmatchDate = MatchDate;
				}
			}
		}

		ZDateTime fMatchDate;

		public ZPropertyInfo MatchDateInfo
		{
			get { return GetZPropertyInfo(Schema.MatchDate); }
		}

		#endregion

		#region UnmatchDate

		public ZDateTime UnmatchDate
		{
			get { return unmatchDate; }
			set
			{
				SetNonPersistentPropertyValue(UnmatchDateInfo, ref unmatchDate, value);
				if (!IsValidationSuspended)
				{
					ValidateUnmatchDate();
				}
			}
		}

		ZDateTime EarlierOfUnmatchDateAndToday
		{
			get { return UnmatchDate >= ZDateTime.Today ? ZDateTime.Today : UnmatchDate; }
		}

		ZDateTime UnmatchDateForCashBasis
		{
			get
			{
				if (AccountingUtils.IsAllowFuturePostingRegistryEnabled && AccountingUtils.DoesUserHaveFuturePostingSecurity)
				{
					return UnmatchDate;
				}

				return EarlierOfUnmatchDateAndToday;
			}
		}

		ZDateTime unmatchDate;

		protected bool UnmatchDate_ReadOnly
		{
			get { return false; }
		}

		public ZPropertyInfo UnmatchDateInfo
		{
			get { return GetZPropertyInfo(Schema.UnmatchDate); }
		}

		bool AllowFutureUnmatchDate
		{
			get
			{
				bool applicableTransactionInMatching =
					MatchedTransactions.Any(b => MatchingBase.IsApplicableTransactionTypeForFuturePosting((IMatching)b));

				return
					applicableTransactionInMatching &&
					AccountingUtils.IsAllowFuturePostingRegistryEnabled &&
					AccountingUtils.DoesUserHaveFuturePostingSecurity;
			}
		}

		void ValidateUnmatchDate()
		{
			UnmatchDateInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(UnmatchDateInfo);

			TypeValidation.CheckValidSmallDateTime(UnmatchDateInfo);
			TypeValidation.CheckValidZDateTimeRange(UnmatchDateInfo);

			if (UnmatchDate.Date > ZDateTime.Today)
			{
				if (!AccountingUtils.IsAllowFuturePostingRegistryEnabled)
				{
					UnmatchDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);
				}
				else if (!AccountingUtils.DoesUserHaveFuturePostingSecurity)
				{
					UnmatchDateInfo.AddError(AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);
				}
			}
			if (UnmatchDate.Date < MatchDate.Date)
			{
				UnmatchDateInfo.AddError(Res.GetString("e58c38a5-d09c-444c-b286-67dda4098507", "The date should be greater then {0}.", MatchDateInfo.HumanReadableName));
			}

			PeriodValidation.CheckDateFallsIntoValidPeriod(UnmatchDateInfo);
		}

		public PeriodValidationProvider PeriodValidation
		{
			get { return periodValidation ?? (periodValidation = new PeriodValidationProvider(Factory)); }
		}
		PeriodValidationProvider periodValidation;

		public bool AllowBackPosting
		{
			get
			{
				return !(from TransactionMatchLink matchLink in MatchLinks
						 where matchLink.MatchingTransaction != null && !matchLink.MatchingTransaction.AllowBackPosting
						 select matchLink).Any();
			}
		}

		#endregion

		#region ReadOnly

		protected virtual bool GetPropertyReadonlyness(PropertyDescriptor property)
		{
			bool result = false;
			if (UseEditableFieldsForReadOnly && property.HasSetter())
			{
				result = !WritableProperties.Contains(property.Name);
			}
			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		List<string> WritableProperties
		{
			get
			{
				if (writableProperties == null)
				{
					writableProperties = new List<string>();
				}
				return writableProperties;
			}
		}

		public void AddWritableProperties(string[] list)
		{
			foreach (string line in list)
			{
				WritableProperties.Add(line);
			}
			UseEditableFieldsForReadOnly = true;

			RefreshBinding();
		}

		List<string> writableProperties;

		bool UseEditableFieldsForReadOnly;

		#endregion

		#endregion

		#region Unmatching

		public override void Delete()
		{
			UnmatchNonPaymentGroup();
			if (CanUnmatchThisMatchGroup == UnmatchingResult.Success)
			{
				MatchLinks.Factory.Save();
			}
			base.Delete();
		}

		/// <summary>
		/// Security Checkpoints required to unmatch. May be many depending on transactions in match group.
		/// </summary>
		public IEnumerable<SecurityCheckpoint> CheckpointsToUnmatch
			=> MatchedTransactions.OfType<IMiscellaneousTransaction>()
				.Select(mt => mt.CheckpointForUnmatch)
				.Where(cp => cp != null)
				.Distinct();

		public UnmatchingResult CanUnmatchThisMatchGroup
		{
			get
			{
				UnmatchingResult returnResult = UnmatchingResult.Success;

				foreach (TransactionMatchLink matchLink in MatchLinks)
				{
					UnmatchingResult currentLinkResult = matchLink.CanUnmatch;
					if (currentLinkResult == UnmatchingResult.ContainsPayment
						|| currentLinkResult == UnmatchingResult.ContainsCashAdvanceARJournal
						|| currentLinkResult == UnmatchingResult.ContainsCashAdvanceAPJournal)
					{
						returnResult = currentLinkResult;
					}
					else if (currentLinkResult > UnmatchingResult.DataError
						&& returnResult != UnmatchingResult.ContainsPayment
						&& returnResult != UnmatchingResult.ContainsCashAdvanceARJournal
						&& returnResult != UnmatchingResult.ContainsCashAdvanceAPJournal)
					{
						returnResult = currentLinkResult;
					}
				}

				return returnResult;
			}
		}

		public void UnmatchAnyGroup()
		{
			UnmatchCore();
		}

		public void UnmatchNonPaymentGroup()
		{
			if (CanUnmatchThisMatchGroup != UnmatchingResult.ContainsPayment)
			{
				UnmatchCore();
			}
		}

		public void UpdateUnmatchDateForAlreadyUnmatchedTransactions()
		{
			UnmatchedTransactions.ForEach(item => item.ChangeUnmatchDate(EarlierOfUnmatchDateAndToday));
			ReversedCashBasisVATRecords.ForEach(item => item.YC_PostDate = UnmatchDateForCashBasis);
		}

		// NOTE: UnmatchCore() does not check if the transactions are able to be unmatched.
		// Checking should be done prior to calling Unmatch() by inspecting CanUnmatchThisMatchGroup
		void UnmatchCore()
		{
			UnmatchedTransactions.Clear();
			ReversedCashBasisVATRecords.Clear();

			// Use this collection to mark which transactions are loaded
			TransactionHeaderCollection systemGeneratedTransactions = new TransactionHeaderCollection(Factory);
			foreach (TransactionMatchLink matchlink in MatchLinks)
			{
				if (matchlink.MatchingTransaction != null &&
					matchlink.MatchingTransaction.AreRelatedTransactionsCreatedByMatching &&
					matchlink.MatchingTransaction.GetTopLevelTransaction != null && matchlink.MatchingTransaction.RelatedTransactions.Count > 0)
				{
					if (!systemGeneratedTransactions.ContainsAll(matchlink.MatchingTransaction.RelatedTransactions) &&
						!systemGeneratedTransactions.Contains(matchlink.MatchingTransaction))
					{
						IMatching topLevelBizO = matchlink.MatchingTransaction.GetTopLevelTransaction;
						using (((BusinessObject)topLevelBizO).GetValidationSuspender())
						{
							topLevelBizO.Unmatch(matchlink.AP_Amount, matchlink.AP_OSAmount);
							topLevelBizO.ChangeUnmatchDate(EarlierOfUnmatchDateAndToday);
							UnmatchedTransactions.Add(topLevelBizO);
						}
						systemGeneratedTransactions.AddRange(matchlink.MatchingTransaction.RelatedTransactions);
						systemGeneratedTransactions.AddRange(matchlink.MatchingTransaction);
					}
				}
				else
				{
					using (matchlink.MatchingTransaction.GetValidationSuspender())
					{
						matchlink.Unmatch();
						IMatching iMatchingTransaction = matchlink.MatchingTransaction as IMatching;
						if (iMatchingTransaction != null)
						{
							iMatchingTransaction.ChangeUnmatchDate(EarlierOfUnmatchDateAndToday);
							UnmatchedTransactions.Add(iMatchingTransaction);
						}
					}
				}
#if DEBUG
				if (!Globals.IsTest || !IsSkipDeleteMatckLinkForTestOnly)
				{
#endif
					MatchLinksToDelete.Add(matchlink);
#if DEBUG
				}
#endif
			}

			ReversedCashBasisVATRecords.AddRange(CashBasisVATManager.ReverseRecords(MatchLinksToDelete, UnmatchDateForCashBasis));

			if (MatchLinks.Count > 0)
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(MatchLinks[0].MatchingTransaction.PK, CriticalValidationInfoCollectorServiceKeyType.MatchLinkDeletionInfo, () =>
				{
					var info = new ZStringBuilder();
					info.AppendLine(FormattableString.Invariant($"Current Factory Instance Number For MatchLink deleting:{Factory._Instance}"))
					.AppendLine(FormattableString.Invariant($"The MatchLink Group Number Is:{MatchLinks[0].AP_MatchGroupNum}"))
					.AppendLine(FormattableString.Invariant($"MatchLinks Collection info:"));
					MatchLinks.ForEach(x => info.AppendLine(FormattableString.Invariant($"Matchlink PK: {x.PK}  Matchlink Parent Transaction: {(x as TransactionMatchLink)?.TransactionHeader?.PK ?? ZGuid.Empty}  Is In MatchLinksToDelete: {MatchLinksToDelete.Any(y => y.PK == x.PK)}")));
					info.AppendLine(FormattableString.Invariant($"Stack Trace:"))
						.AppendLine(new StackTrace().ToString());
					return info.ToString();
				});
			}

			MatchLinksToDelete.RemoveAndDeleteAll();
		}

#if DEBUG
		public bool IsSkipDeleteMatckLinkForTestOnly;
#endif

		List<IMatching> UnmatchedTransactions
		{
			get { return unmatchedTransactions ?? (unmatchedTransactions = new List<IMatching>()); }
		}
		List<IMatching> unmatchedTransactions;

		List<AccCashBasisVAT> ReversedCashBasisVATRecords
		{
			get { return reversedCashBasisVATRecords ?? (reversedCashBasisVATRecords = new List<AccCashBasisVAT>()); }
		}
		List<AccCashBasisVAT> reversedCashBasisVATRecords;

		#endregion

		#region Lookups

		#region MatchLinks

		public TransactionMatchLinkCollectionForUnmatching MatchLinks
		{
			get
			{
				if (fMatchLinks == null)
				{
					fMatchLinks = new TransactionMatchLinkCollectionForUnmatching(Factory);
					foreach (ViewMatchGroup matchLink in MatchGroupsFromView)
					{
						// Load the bizo from the object in the view
						Factory.AddFetchHint(typeof(ViewMatchGroup), matchLink.PK);
						Factory.AddFetchHint(typeof(TransactionMatchLink), matchLink.PK);
					}

					foreach (ViewMatchGroup matchLink in MatchGroupsFromView)
					{
						// Load the bizo from the object in the view
						TransactionMatchLink transMatchLink = Factory.Load<TransactionMatchLink>(matchLink.PK);
						fMatchLinks.Add(transMatchLink);
						Factory.AddFetchHint(AccTransactionHeaderSchema.Constants.TableName, transMatchLink.AP_AH);
					}
				}

				return fMatchLinks;
			}
		}

		TransactionMatchLinkCollectionForUnmatching fMatchLinks;

		ViewMatchGroupCollection MatchGroupsFromView
		{
			get
			{
				ZQuery viewMatchLinkFilter = new ZQuery(ViewMatchGroupSchema.MG_GC, GlbCompany.CurrentCompany.PK);
				viewMatchLinkFilter.AddToFilter(JoinCondition.And, ViewMatchGroupSchema.MG_MatchGroupNum, SQLComparisonOperator.Equal, MatchGroupNum);

				ViewMatchGroupCollection viewMatchLinks = new ViewMatchGroupCollection(Factory, viewMatchLinkFilter);
				viewMatchLinks.Load();
				return viewMatchLinks;
			}
		}

		#endregion

		#region MatchedTransactions

		public TransactionHeaderCollection MatchedTransactions
		{
			get
			{
				if (fMatchedTransactions == null)
				{
					fMatchedTransactions = new TransactionHeaderCollection(Factory);
					fMatchedTransactions.LoadTransactionsFromMatchLinks(MatchLinks);
				}

				return fMatchedTransactions;
			}
		}

		TransactionHeaderCollection fMatchedTransactions;

		#endregion

		#region MatchLinksToDelete

		TransactionMatchLinkCollection MatchLinksToDelete
		{
			get
			{
				if (fMatchLinksToDelete == null)
				{
					fMatchLinksToDelete = new TransactionMatchLinkCollection(Factory);
				}

				return fMatchLinksToDelete;
			}
		}

		TransactionMatchLinkCollection fMatchLinksToDelete;

		#endregion
		#endregion
	}
}
