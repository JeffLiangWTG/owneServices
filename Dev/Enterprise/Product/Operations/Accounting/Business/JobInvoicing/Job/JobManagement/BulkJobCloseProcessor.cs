using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class BulkJobCloseProcessor : NonPersistentBusinessObject
	{
		#region Construction
		public BulkJobCloseProcessor()
			: this(null)
		{
		}
		public BulkJobCloseProcessor(Action<ZQuery> addAdditionalJobHeaderFilter)
			: base(new BusinessObjectFactory())
		{
			using (this.SuspendSettingHasChanges())
			{
				this.AddAdditionalJobHeaderFilter = addAdditionalJobHeaderFilter;
				JobCloseDate = ZDateTime.Today;
				JobOpenDateFilter.ModuleFilterChanged += Filter_ModuleFilterChanged;
				JobLastEditDateFilter.ModuleFilterChanged += Filter_ModuleFilterChanged;
				TotalNumberOfJobMessage = Res.GetString("0ee9486b-71cd-4e33-8ea2-0bfc5126f6bc", "<Please set required search conditions and click Find>");
			}
		}
		readonly Action<ZQuery> AddAdditionalJobHeaderFilter;

		void Filter_ModuleFilterChanged(object sender, EventArgs e)
		{
			JobPKs.Clear();
			TotalNumberOfJobMessage = Res.GetString("29805d1e-32aa-40ee-8463-3065ff2d47ee", "<One or more Search conditions have changed. Please Click Find>");
		}
		#endregion

		#region Filters

		#region Job Status

		[List("JobStatusList")]
		[MaxLength(JobHeader.Schema.JH_Status)]
		public ZString JobStatusFilter
		{
			get { return fJobStatusFilter; }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(JobStatusFilterInfo, value);

				if (fJobStatusFilter != value)
				{
					SetNonPersistentPropertyValue(JobStatusFilterInfo, ref fJobStatusFilter, value);
					Filter_ModuleFilterChanged(null, null);
					if (!IsValidationSuspended)
					{
						Validation.ValidateJobStatusFilter();
					}
				}
			}
		}

		ZString fJobStatusFilter;

		public ZPropertyInfo JobStatusFilterInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(JobStatusFilter));
			}
		}

		#endregion

		#region JobOpenDateFilter

		public ModuleDateFilter JobOpenDateFilter
		{
			get { return fdateFilter ?? (fdateFilter = new ModuleDateFilter("Job Open Date", GetJobOpenQuery, false)); }
		}

		ModuleDateFilter fdateFilter;
		#endregion

		#region JobLastEditDateFilter

		public ModuleDateFilter JobLastEditDateFilter
		{
			get { return jobLastEditDateFilter ?? (jobLastEditDateFilter = new ModuleDateFilter("Job Edited Last Time", GetJobLastEditedQuery, true)); }
		}

		ModuleDateFilter jobLastEditDateFilter;
		#endregion

		#endregion

		#region Properties

		public ZDateTime JobCloseDate
		{
			get { return fJobCloseDate; }
			set
			{
				SetNonPersistentPropertyValue(JobCloseDateInfo, ref fJobCloseDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJobCloseDate();
				}
			}
		}
		ZDateTime fJobCloseDate;
		public ZPropertyInfo JobCloseDateInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(JobCloseDate));
			}
		}

		public ZString TotalNumberOfJobMessage
		{
			get { return totalNumberOfJobMessage; }
			private set
			{
				SetNonPersistentPropertyValue(TotalNumberOfJobMessageInfo, ref totalNumberOfJobMessage, value);
			}
		}
		ZString totalNumberOfJobMessage;

		public ZPropertyInfo TotalNumberOfJobMessageInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(TotalNumberOfJobMessage));
			}
		}

		public List<ZGuid> JobPKs
		{
			get
			{
				if (jobPKs == null)
				{
					jobPKs = new List<ZGuid>();
				}
				return jobPKs;
			}
		}
		List<ZGuid> jobPKs;

		#endregion

		#region Methods

		public void Clear()
		{
			JobStatusFilter = "";
			JobOpenDateFilter.Clear();
			JobLastEditDateFilter.Clear();
			JobPKs.Clear();
		}

		public string Find()
		{
			string msg = Validation.Validate(false);

			if (string.IsNullOrWhiteSpace(msg))
			{
				JobPKs.Clear();

				var query = CreateQuery();
				var sql = ZString.Format(
					AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.Value
						? "SELECT {0} FROM {1} OUTER APPLY ShouldJobBeClosedByDsbBatch({0}) {2}"
						: "SELECT {0} FROM {1} {2}",
					JobHeader.Schema.PK, JobHeader.Schema.TableName, query.GetAsWhereClause(false));

				var selectedJobPKs = new DynamicBusinessObjectCollection(Factory);
				selectedJobPKs.Load(sql, query.Params);
				foreach (DynamicBusinessObject jobPK in selectedJobPKs)
				{
					JobPKs.Add(new ZGuid(jobPK[JobHeader.Schema.PK]));
				}

				TotalNumberOfJobMessage = Res.GetString("990ace5b-1b90-46e6-bcfa-652609eedcd8", @"Total Number of Job Found: {0}", JobPKs.Count);
			}
			return msg;
		}

		public int CloseJobInBulk()
		{
			int result = 0;
			using (var manager = Db.Connection.BeginTransactionWithManager()) // This is a call to a stored proc so had to avoid Factory
			{
				if (JobPKs.Count > 0)
				{
					string msg = Validation.Validate(true);
					if (string.IsNullOrWhiteSpace(msg))
					{
						using (var command = GetBulkJobClosureCommand())
						{
							command.ExecuteNonQuery();
							result = (int)command.GetParameterValue("@JobClosed");
						}
						manager.CommitTransaction();// This is a call to a stored proc so had to avoid Factory
					}
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DbCommand GetBulkJobClosureCommand()
		{
			var command = Db.Connection.Command("EXEC CloseJobInBulk @JobPKs, @JobCloseDate, @CurrentUserCode, @EnableBulkDisbursementJobsClosure, @JobClosed OUTPUT"); // This is a call to a stored proc, not something we're showing to the user.
			command.AddTableValuedParameter("@JobPKs", JobHeaderSchema.PK, JobPKs);
			command.AddParameter("@JobCloseDate", SqlDbType.DateTime, JobCloseDate.ToDateTime());
			command.AddParameter("@CurrentUserCode", SqlDbType.VarChar, Env.CurrentUser.Initials);
			command.AddParameter("@EnableBulkDisbursementJobsClosure", SqlDbType.Bit, AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.Value);
			command.AddOutputParameter("@JobClosed", SqlDbType.Int, 0, 0, 0, 0);
			return command;
		}

		ZQuery CreateQuery()
		{
			var query = new ZQuery();
			if (!string.IsNullOrWhiteSpace(JobStatusFilter))
			{
				query.AddToFilter(JobHeaderSchema.JH_Status, SQLComparisonOperator.Equal, JobStatusFilter);
			}
			else
			{
				query.AddToFilter(JobHeaderSchema.JH_Status, SQLComparisonOperator.NotEqual, JobHeaderStatus.Closed.Code);
			}
			query.AddToFilter(JobOpenDateFilter.Query);
			query.AddToFilter(JobLastEditDateFilter.Query);
			query.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.Equal, new ZGuid(Env.CurrentCompany.PK));
			query.AddToFilter(JobHeaderSchema.JH_IsActive, SQLComparisonOperator.Equal, true);

			if (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.Value)
			{
				query.AddFilterAndZSQLParameterCollection($"TargetGLAccount IS NULL", null);
			}

			AddAdditionalJobHeaderFilter?.Invoke(query);
			return query;
		}

		ZQuery GetJobOpenQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var query = new ZQuery();
			new EmptyFilterStripBusinessObject().AddDateTimeRangeFilter(query, comparisonOperator, JoinCondition.And, JobHeaderSchema.JH_A_JOP, fromDate, toDate);
			return query;
		}

		ZQuery GetJobLastEditedQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var query = new ZQuery();
			new EmptyFilterStripBusinessObject().AddDateTimeRangeFilter(query, comparisonOperator, JoinCondition.And, JobHeaderSchema.JH_SystemLastEditTimeUtc, fromDate, toDate, true);
			return query;
		}

		#endregion

		#region Lookups & Lists
		public CodeDescriptionPairList JobStatusList
		{
			get
			{
				if (jobStatusList == null)
				{
					jobStatusList = new JobHeaderStatusList();
					jobStatusList.RemoveCode(JobHeaderStatus.Closed);
				}
				return jobStatusList;
			}
		}
		CodeDescriptionPairList jobStatusList;
		#endregion

		#region Validation
		public BulkJobCloseProcessorValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected BulkJobCloseProcessorValidation GetNewValidation()
		{
			return new BulkJobCloseProcessorValidation(this);
		}
		#endregion

		public class EmptyFilterStripBusinessObject : FilterStripBusinessObject
		{
			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				return new ModuleFilterCollection();
			}

			public void AddDateTimeRangeFilter(ZQuery query, DateComparisonOperator comparisonOperator, JoinCondition joinCondition, SchemaDateTimeColumn column, ZDateTime lowerEmptyOk, ZDateTime upperEmptyOk, bool convertFromLocalToUtc = false)
			{
				AddDateTimeRange(query, comparisonOperator, joinCondition, column, lowerEmptyOk, upperEmptyOk, convertFromLocalToUtc);
			}
		}
	}

	#region Validation Class
	public class BulkJobCloseProcessorValidation : ZValidation
	{
		public BulkJobCloseProcessorValidation(BulkJobCloseProcessor parent)
			: base(parent)
		{
			if (Object.ReferenceEquals(parent, null))
			{
				throw new ArgumentNullException(nameof(parent));
			}
		}

		public void ValidateJobStatusFilter()
		{
			((IValidationInternals)this).Validate((base.ParentFilter as BulkJobCloseProcessor).JobStatusFilterInfo, GetJobStatusFilterValidationInvoker());
		}
		RunValidationInvoker GetJobStatusFilterValidationInvoker()
		{
			return delegate
			{
				CheckJobStatusFilter();
			};
		}
		protected void CheckJobStatusFilter()
		{
			MandatoryValidation.CheckEntered((base.ParentFilter as BulkJobCloseProcessor).JobStatusFilterInfo);
			ListValidation.ErrorIfInvalidCode((base.ParentFilter as BulkJobCloseProcessor).JobStatusFilterInfo, (base.ParentFilter as BulkJobCloseProcessor).JobStatusList);
		}

		public void ValidateJobCloseDate()
		{
			((IValidationInternals)this).Validate((base.ParentFilter as BulkJobCloseProcessor).JobCloseDateInfo, GetJobCloseDateValidationInvoker());
		}
		RunValidationInvoker GetJobCloseDateValidationInvoker()
		{
			return delegate
			{
				CheckJobCloseDate();
			};
		}
		protected void CheckJobCloseDate()
		{
			MandatoryValidation.CheckEntered((base.ParentFilter as BulkJobCloseProcessor).JobCloseDateInfo);

			if ((base.ParentFilter as BulkJobCloseProcessor).JobCloseDate.Date > ZDateTime.Today)
			{
				(base.ParentFilter as BulkJobCloseProcessor).JobCloseDateInfo.AddError(Res.GetString("e0a1305a-b83e-4ef7-b2d2-c3b7b5299987", "You cannot select a future date as a Job Close date"));
			}
			if ((base.ParentFilter as BulkJobCloseProcessor).JobCloseDate.Date < ZDateTime.Today && !Env.Security.BulkJobClosingInBackDate.IsAllowed)
			{
				(base.ParentFilter as BulkJobCloseProcessor).JobCloseDateInfo.AddError(Env.Security.BulkJobClosingInBackDate.ErrorMessageForNotAllowed);
			}

			TypeValidation.CheckValidZDateTimeAndRange((base.ParentFilter as BulkJobCloseProcessor).JobCloseDateInfo);
		}

		public string ValidateJobDateFilter()
		{
			string msg = string.Empty;
			if ((base.ParentFilter as BulkJobCloseProcessor).JobOpenDateFilter.IsEmpty && (base.ParentFilter as BulkJobCloseProcessor).JobLastEditDateFilter.IsEmpty)
			{
				msg = Res.GetString("567f48d4-8087-45a1-aa2a-131ab6ac05fd", @"You must fill at least one of the following filters
- Job Open Date
- Job Last Edit Time");
			}
			return msg;
		}

		public override Type AutoValidationType
		{
			get { return typeof(BulkJobCloseProcessorValidation); }
		}

		public string Validate(bool validateJobCloseDate)
		{
			var parent = (base.ParentFilter as BulkJobCloseProcessor);
			var msgBuilder = new ZStringBuilder();

			ValidateJobStatusFilter();
			if (parent.JobStatusFilterInfo.HasErrors())
			{
				msgBuilder.Append(parent.JobStatusFilterInfo.GetErrors().GetFirstMessage());
			}

			ValidateJobCloseDate();
			if (parent.JobCloseDateInfo.HasErrors())
			{
				msgBuilder.Append(parent.JobCloseDateInfo.GetErrors().GetFirstMessage());
			}

			msgBuilder.Append(ValidateJobDateFilter());

			return msgBuilder.ToStringWithNewLineBetweenAppends();
		}

		public override void ValidateAll()
		{
			throw new NotImplementedException();
		}
	}
	#endregion
}
