using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionAgreementApprover
	{
		public const string AgreementLockPrefix = "CGN";

		#region New

		public static CommissionAgreementApprover New(BusinessObjectFactory factory)
		{
			var type = TypeDecider.GetTypeForBinding(typeof(CommissionAgreementApprover));
			var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(BusinessObjectFactory) }, null);

			return (CommissionAgreementApprover)constructor.Invoke(new object[] { factory });
		}

		#endregion

		#region Constructor

		protected CommissionAgreementApprover(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		protected readonly BusinessObjectFactory Factory;

		#endregion

		#region ApproveAndQueue

		public void ApproveAndQueue(CreateCommissionContext context, Progress progress)
		{
			CheckIsInTransaction();

			Approve(context, progress);
			QueueForCommissionsCalculation(context);
		}

		#endregion

		#region ApproveAndCreateCommissions

		public void ApproveAndCreateCommissions(CreateCommissionContext context, Progress progress)
		{
			CheckIsInTransaction();

			Approve(context, progress);
			CreateCommissions(context, progress);
		}

		#endregion

		#region CreateCommissions

		/// <summary>
		/// This method should only be called from CommissionGeneratorServiceTask or tests
		/// </summary>
		/// <param name="context"></param>
		/// <param name="progress"></param>
		public void CreateCommissions(CreateCommissionContext context, Progress progress)
		{
			CreateCommissionsCore(context, progress);
		}

		protected virtual void CreateCommissionsCore(CreateCommissionContext context, Progress progress)
		{
			CreateEffectiveDateCacheTable();
			CreateNonJobRelatedCommissions(context, progress);
			CreateJobCommissions(context, progress);

			var query = new ZQuery(OrgCommissionCalculationQueueSchema.CAQ_CA0, context.AgreementsBeingApproved.Select(a => a.PK));
			Factory.Load<OrgCommissionCalculationQueue>(query).DeleteAll();

			NotifyProgress(progress, SavingStatusMessage, 0);
			Factory.Save();
			NotifyProgress(progress, SavingStatusMessage, 100);
		}

		protected void CreateEffectiveDateCacheTable()
		{
			effectiveDateCacheTable = new DataTable();
			effectiveDateCacheTable.Columns.Add(TvpTriggerTypeEffectiveDate.Columns.TriggerType, typeof(string));
			effectiveDateCacheTable.Columns.Add(TvpTriggerTypeEffectiveDate.Columns.CustomerPk, typeof(Guid));
			effectiveDateCacheTable.Columns.Add(TvpTriggerTypeEffectiveDate.Columns.EffectiveDate, typeof(DateTime));
		}

		protected DataTable effectiveDateCacheTable;

		#region ApproveAgreements

		void Approve(CreateCommissionContext context, Progress progress)
		{
			CheckIsInTransaction();

			var processed = 0;

			var nonDrafts = context.AgreementsBeingApproved.Where(x => !x.IsDraft).ToArray();
			var locks = new List<SqlApplicationLock>();

			try
			{
				foreach (var nonDraft in nonDrafts)
				{
					CheckLock(nonDraft, locks);

					nonDraft.Approve(context.GetLogMessage());
				}

				Factory.Save();

				var drafts = context.AgreementsBeingApproved.Where(x => x.IsDraft).ToArray();
				var draftMainVersions = drafts.Select(x => x.MainVersion).ToArray();
				foreach (var draft in drafts)
				{
					CheckLock(draft.MainVersion, locks);

					NotifyProgress(progress, GetMergingAgreementsStatusMessage(processed + 1, drafts.Length), 100 * processed / drafts.Length);
					draft.ApproveDraft(context.GetLogMessage());

					Factory.Save();

					processed++;
				}

				NotifyProgress(progress, GetMergingAgreementsStatusMessage(drafts.Length, drafts.Length), 100);

				if (draftMainVersions.Any())
				{
					context.AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(nonDrafts.Union(draftMainVersions));
				}
			}
			finally
			{
				locks.ForEach(l => l.Dispose());
			}
		}

		protected virtual void QueueForCommissionsCalculation(CreateCommissionContext context)
		{
			context.AgreementsBeingApproved.ForEach(a =>
			{
				a.SendToCalculationQueue(context.FromDate, context.OverwriteOldValues);
			});

			Factory.Save();
		}

		static void CheckLock(OrgCommissionAgreement agreement, List<SqlApplicationLock> locks)
		{
			SqlApplicationLock sqlLock;
			if (!Db.Connection.TryGetLock(AgreementLockPrefix + agreement.PK, out sqlLock))
			{
				locks.ForEach(l => l.Dispose());
				throw new OrgCommissionAgreementBeingProcessedException();
			}

			locks.Add(sqlLock);
		}

		#endregion

		#region CreateNonJobRelatedCommissions

		void CreateNonJobRelatedCommissions(CreateCommissionContext context, Progress progress)
		{
			var filter = GetNonJobRelatedTransactionFilter(context);
			var nonJobRelatedTransactions = Factory.Load<TransactionHeader>(filter).OfType<ICommissionableTransaction>().ToArray();
			var processed = 0;

			ReversalTransactionCommissionCreator.AddReversalTransactionFetchHints(Factory, nonJobRelatedTransactions);

			foreach (var transaction in nonJobRelatedTransactions)
			{
				NotifyProgress(progress, GetCreatingNonJobRelatedCommissionStatusMessage(processed + 1, nonJobRelatedTransactions.Length), 100 * processed / nonJobRelatedTransactions.Length);
				NonJobRelatedTransactionCommissionCreator.New(transaction, effectiveDateCacheTable).CreateCommissions(context);
				processed++;
			}

			NotifyProgress(progress, GetCreatingNonJobRelatedCommissionStatusMessage(nonJobRelatedTransactions.Length, nonJobRelatedTransactions.Length), 100);
		}

		protected virtual ZDBOnlyQuery GetNonJobRelatedTransactionFilter(CreateCommissionContext context)
		{
			var filter = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			var sub1 = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			ZDBOnlySubQuery sub2 = null;

			InitNonJobRelatedSubQuery(sub1, context.FromDate);

			var applicableQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			if (context.OnlyCreateForAgreementsBeingApproved)
			{
				applicableQuery.AddToFilter(GetNonJobRelatedTransactionHasAgreementBeingApprovedFilter(context));
			}

			if (!context.OverwriteOldValues)
			{
				applicableQuery.AddSubQuery(GetNoExistingCommissionHeaderSubQuery(), JoinCondition.And);
			}
			else if (context.AgreementsBeingApproved != null)
			{
				sub2 = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);

				InitNonJobRelatedSubQuery(sub2, context.FromDate);

				var applicableQuery2 = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				applicableQuery2.AddSubQuery(GetHasExistingCommissionHeaderSubQuery(context.PksOfAgreementsBeingApproved), JoinCondition.Or);

				sub2.AddToFilter(applicableQuery2);
			}

			sub1.AddToFilter(applicableQuery);

			if (sub2 != null)
			{
				sub1.AddAsUnionQuery(sub2);
			}

			filter.AddSubQuery(sub1, JoinCondition.And);

			return filter;
		}

		void InitNonJobRelatedSubQuery(ZDBOnlySubQuery subQuery, ZDateTime fromDate)
		{
			subQuery.AddToFilter(NonJobRelatedTransactionCommissionCreator.GetNonJobRelatedTransactionsQuery());
			subQuery.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.GreaterThanOrEqualTo, fromDate);
		}

		protected virtual ZQuery GetNonJobRelatedTransactionHasAgreementBeingApprovedFilter(CreateCommissionContext context)
		{
			var allAllAllItemPath = new[] { Tuple.Create(ZBool.True, (ZString)OrgCommissionAgreementItemLookups.AllProductsCode), Tuple.Create(ZBool.True, (ZString)OrgCommissionAgreementItemLookups.AllServicesCode), Tuple.Create(ZBool.True, (ZString)OrgCommissionAgreementItemLookups.AllSubModulesCode) };
			var allAllAllAgreementsBeingApproved = context.AgreementsBeingApproved.Where(x => x.GetItem(allAllAllItemPath) != null);
			var customersOfAllAllAllAgreementsBeingApproved = allAllAllAgreementsBeingApproved.Select(x => x.CA0_OH_Customer).ToArray();
			return new ZQuery(AccTransactionHeaderSchema.AH_OH, customersOfAllAllAllAgreementsBeingApproved);
		}

#if DEBUG
		public ZQuery GetNonJobRelatedTransactionFilterForTesting(CreateCommissionContext context)
		{
			return GetNonJobRelatedTransactionFilter(context);
		}
#endif

		#endregion

		#region CreateJobCommissions

		void CreateJobCommissions(CreateCommissionContext context, Progress progress)
		{
			var filter = GetJobsFilter(context);
			var allJobs = Factory.Load<JobHeader>(filter);
			var jobsGroupedByBranch = allJobs.GroupBy(j => j.JH_GB);

			var processed = 0;
			var totalJobCount = allJobs.Length;

			foreach (var jobBranch in jobsGroupedByBranch)
			{
				using (DisposableEnvironment.ForBranch(jobBranch.Key.ToGuid()))
				{
					foreach (var job in jobBranch)
					{
						NotifyProgress(progress, GetCreatingJobCommissionStatusMessage(processed + 1, totalJobCount), 100 * processed / totalJobCount);
						new JobCommissionCreator(job, effectiveDateCacheTable).CreateCommissions(context);
						processed++;
					}
				}
			}

			NotifyProgress(progress, GetCreatingJobCommissionStatusMessage(totalJobCount, totalJobCount), 100);
		}

		protected ZQuery GetJobsFilter(CreateCommissionContext context)
		{
			var filter = new ZDBOnlyQuery(typeof(JobHeader));
			var containsJobTransactionOrJobApportionedLineQuery = new ZDBOnlyQuery(typeof(JobHeader));
			{
				var transactionFilter = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				transactionFilter.AddToFilter(JobRelatedTransactionCommissionCreator.GetIsCommissionableTransactionQuery());
				transactionFilter.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.GreaterThanOrEqualTo, context.FromDate);
				if (!context.OverwriteOldValues)
				{
					transactionFilter.AddSubQuery(GetNoExistingCommissionHeaderSubQuery(), JoinCondition.And);
				}

				var jobTransactionSubquery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.AH_JH);
				jobTransactionSubquery.AddToFilter(transactionFilter);
				containsJobTransactionOrJobApportionedLineQuery.AddSubQuery(jobTransactionSubquery, JoinCondition.Or);

				var jobApportionedLineSubquery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_JH);
				var jobApportionedLineHeaderSubquery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionLinesSchema.AL_AH);
				jobApportionedLineHeaderSubquery.AddToFilter(transactionFilter);
				jobApportionedLineSubquery.AddSubQuery(jobApportionedLineHeaderSubquery, JoinCondition.And);
				containsJobTransactionOrJobApportionedLineQuery.AddSubQuery(jobApportionedLineSubquery, JoinCondition.Or);

				filter.AddToFilter(containsJobTransactionOrJobApportionedLineQuery, JoinCondition.And);
			}

			if (context.OnlyCreateForAgreementsBeingApproved)
			{
				var customersOfAgreementsBeingApproved = context.AgreementsBeingApproved.Select(x => x.CA0_OH_Customer).ToArray();
				var jobCustomerAddressSubquery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
				jobCustomerAddressSubquery.AddToFilter(OrgAddressSchema.OA_OH, customersOfAgreementsBeingApproved);
				filter.AddSubQuery(jobCustomerAddressSubquery, JoinCondition.And);
			}

			if (context.OverwriteOldValues && context.AgreementsBeingApproved != null)
			{
				var hasTransactionWithExistingCommissionSubquery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.AH_JH);
				hasTransactionWithExistingCommissionSubquery.AddSubQuery(GetHasExistingCommissionHeaderSubQuery(context.PksOfAgreementsBeingApproved), JoinCondition.And);
				filter.AddSubQuery(hasTransactionWithExistingCommissionSubquery, JoinCondition.Or);
			}

			return filter;
		}

#if DEBUG
		public ZQuery GetJobsFilterForTesting(CreateCommissionContext context)
		{
			return GetJobsFilter(context);
		}
#endif

		#endregion

		protected static ZDBOnlySubQuery GetNoExistingCommissionHeaderSubQuery()
		{
			var result = new ZDBOnlySubQuery(typeof(AccCommissionHeader), AccCommissionHeaderSchema.CH0_AH_Source, true);
			result.AddToFilter(AccCommissionHeaderSchema.CH0_OverridenDateTimeUtc, null);
			return result;
		}

		protected static ZDBOnlySubQuery GetHasExistingCommissionHeaderSubQuery(ISet<ZGuid> pksOfAgreementsBeingApproved)
		{
			var result = new ZDBOnlySubQuery(typeof(AccCommissionHeader), AccCommissionHeaderSchema.CH0_AH_Source);
			result.AddToFilter(AccCommissionHeaderSchema.CH0_CA0, pksOfAgreementsBeingApproved);
			result.AddToFilter(AccCommissionHeaderSchema.CH0_OverridenDateTimeUtc, null);

			return result;
		}

		#endregion

		#region Progress

		protected void NotifyProgress(Progress progress, string status, int percentComplete)
		{
			if (progress != null)
			{
				progress(status, percentComplete);
			}
		}

		#endregion

		#region Common Methods

		void CheckIsInTransaction()
		{
			if (!((ITransactionParticipant)Factory).IsInTransaction)
			{
				throw new InvalidOperationException("This method can only be called within a transaction.");
			}
		}

		#endregion

		#region Messages

		protected string GetMergingAgreementsStatusMessage(int current, int total)
		{
			return Res.GetString("29b1620f-f06b-48d3-9a0d-9bfac231b0f4", "Merging Commission Agreements: {0} of {1}", current, total);
		}

		protected string GetCreatingNonJobRelatedCommissionStatusMessage(int current, int total)
		{
			return Res.GetString("cd21df9f-2a9e-4447-8f3a-47cbde89fce0", "Creating Non Job Related Commissions: {0} of {1}", current, total);
		}

		protected string GetCreatingJobCommissionStatusMessage(int current, int total)
		{
			return Res.GetString("d8fe3433-6484-4f52-a5a2-b720e2e529e5", "Creating Job Related Commissions: {0} of {1}", current, total);
		}

		protected string SavingStatusMessage
		{
			get { return Res.GetString("32374cb6-75e7-4272-9015-083edd0de0ad", "Saving.."); }
		}

		#endregion
	}
}
