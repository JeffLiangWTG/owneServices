using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public partial class GLJournalFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		public GLJournalFilterBusinessObject()
			: base()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddNumberFilter("Journal Number", AccTransactionHeaderSchema.AH_TransactionNum).MultilingualDescription = ResString.GetMultilingualString("Accounting|GLJournalFilter|JournalNumber", "Journal Number");

			ModuleFilter filter = filters.AddTextFilter("Journal Type", GetTransactionTypeQuery, TransactionTypeList);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLJournalFilter|JournalType", "Journal Type");

			ModulePeriodFilter postPeriodFilter = filters.AddPeriodFilter("Post Period", GetPostPeriodQuery);
			postPeriodFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLJournalFilter|PostPeriod", "Post Period");
			RemoveAllComparisonOperatorCodesExcept(postPeriodFilter, ModuleNumberFilter.ComparisonConstants.Exact);
			postPeriodFilter.PropertyValidation = ValidateAccountingPeriod;

			ModuleDateFilter postDateFilter = filters.AddDateFilter(AccountingUtils.DateFilterTypes.PostDate, GetAH_PostDateQuery);
			postDateFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLJournalFilter|PostDate", "Post Date");
			postDateFilter.Category = FilterCategories.NumbersAndReferences;

			ModulePeriodFilter reversePeriodFilter = filters.AddPeriodFilter("Reverse/End Period", GetAgePeriodQuery);
			reversePeriodFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLJournalFilter|ReverseEndPeriod", "Reverse/End Period");
			RemoveAllComparisonOperatorCodesExcept(reversePeriodFilter, ModuleNumberFilter.ComparisonConstants.Exact);
			reversePeriodFilter.PropertyValidation = ValidateAccountingPeriod;

			filters.AddTextFilter("Journal Description", AccTransactionHeaderSchema.AH_Desc).MultilingualDescription = ResString.GetMultilingualString("Accounting|GLJournalFilter|JournalDescription", "Journal Description");

			ModuleGuidFilter branchFilter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccTransactionHeaderSchema.AH_GB, AH_GBList);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLJournalFilter|Branch", "Branch");

			AddBranchManagementCodeFilter(filters);

			if (!Env.Security.GeneralLedgerViewingNonLoginBranchTransactions.IsAllowed)
			{
				branchFilter.DefaultProperty = GlbBranch.CurrentBranch.PK;
				branchFilter.Visibility = FilterVisibility.AlwaysVisible;
				branchFilter.ReadOnly = true;
			}

			filter = filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, AccTransactionHeaderSchema.AH_GE, AH_GEList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLJournalFilter|Department", "Department");

			filter = filters.AddNkFilter("Original Requester", GetOriginalRequesterQuery, ModuleIDs.GlbStaff, StaffList);
			filter.Category = FilterCategories.AuditInformation;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLJournalFilter|OriginalRequester", "Original Requester");

			filter = filters.AddNkFilter("Original Approver", GetOriginalApproverQuery, ModuleIDs.GlbStaff, StaffList);
			filter.Category = FilterCategories.AuditInformation;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLJournalFilter|OriginalApprover", "Original Approver");

			filter = filters.AddNkFilter("Last Requester", GetLastRequesterQuery, ModuleIDs.GlbStaff, StaffList);
			filter.Category = FilterCategories.AuditInformation;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLJournalFilter|LastRequester", "Last Requester");

			var dateUploadedFilter = filters.AddDateFilter(AccountingUtils.DateFilterTypes.DateUploaded, GetDateUploadedQuery);
			dateUploadedFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLJournalFilter|DateUploaded", "Date Uploaded");
			dateUploadedFilter.Category = FilterCategories.Dates;

			return filters;
		}

		GetNkQuery GetOriginalRequesterQuery
		{
			get
			{
				return GetRequesterOrApproverQueryGetter(GenApprovalRequestSchema.XP_SystemCreateUser,
					true,
					new ZQuery(GenApprovalRequestSchema.XP_ApprovalStatus, SQLComparisonOperator.NotEqual, Constants.GenApprovalRequestApprovalStatus.Cancelled));
			}
		}

		GetNkQuery GetOriginalApproverQuery
		{
			get
			{
				return GetRequesterOrApproverQueryGetter(GenApprovalRequestSchema.XP_GS_NKApprovingUser1,
					true,
					new ZQuery(GenApprovalRequestSchema.XP_ApprovalStatus, SQLComparisonOperator.Equal, Constants.GenApprovalRequestApprovalStatus.Posted));
			}
		}

		GetNkQuery GetLastRequesterQuery
		{
			get
			{
				return GetRequesterOrApproverQueryGetter(GenApprovalRequestSchema.XP_SystemCreateUser,
					false,
					new ZQuery(GenApprovalRequestSchema.XP_ApprovalStatus, SQLComparisonOperator.NotEqual, Constants.GenApprovalRequestApprovalStatus.Cancelled));
			}
		}

		GetNkQuery GetRequesterOrApproverQueryGetter(SchemaColumn columnSchema, bool isOriginalTarget, params ZQuery[] exceptionFilters)
		{
			return target =>
				{
					var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));

					var subQuery = new ZQuery(new GLJournalApprovalRequestCollection(Factory).CompleteFilter);
					subQuery.AddToFilter(GenApprovalRequestSchema.XP_ParentID, SQLComparisonOperator.Equal, AccTransactionHeaderSchema.PK);
					foreach (var exceptionFilter in exceptionFilters)
					{
						subQuery.AddToFilter(exceptionFilter);
					}

					var subSQL = String.Format(System.Globalization.CultureInfo.InvariantCulture,
@"AH_PK IN (
    SELECT XP_ParentID
    FROM
            (
                    SELECT XP_ParentID, {0}, ROW_NUMBER() OVER(PARTITION BY XP_ParentID ORDER BY XP_SystemCreateTimeUtc{1}) AS RowNum
                    FROM dbo.GenApprovalRequest
                    WHERE {2}
            ) T
    WHERE T.RowNum = 1
		AND T.{0} = '{3}'
)",
			  columnSchema.Name,
			  isOriginalTarget ? String.Empty : " DESC",
			  subQuery.LiteralTextSqlFormatted,
			  target);

					result.AddFilterAndZSQLParameterCollection(subSQL, null);

					return result;
				};
		}

		GlbStaffCollection StaffList
		{
			get
			{
				return new GlbStaffCollection(this.Factory);
			}
		}

		void RemoveAllComparisonOperatorCodesExcept(ModuleTextFilter filter, string keptCode)
		{
			int keptIndex = filter.ComparisonOperator_List.IndexOfCode(keptCode);
			for (int i = filter.ComparisonOperator_List.Count - 1; i >= 0; i--)
			{
				if (i != keptIndex)
				{
					filter.ComparisonOperator_List.RemoveAt(i);
				}
			}
		}

		#region Validation

		public void ValidateAccountingPeriod(ZPropertyInfo info)
		{
			var valueAsString = (ZString)info.Value;
			if (!valueAsString.IsEmpty)
			{
				if (ZInt.CanParse(valueAsString))
				{
					var period = ZInt.Parse(valueAsString);
					if (!new AccountingPeriodCalculator(Factory).IsPeriodValid(period))
					{
						info.AddError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(period));
					}
				}
				else
				{
					info.AddError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(valueAsString));
				}
			}
		}

		#endregion

		#region Filter Delegates

		ZQuery GetTransactionTypeQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetAgePeriodQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZInt period = ZInt.Parse(value);
			return GetPeriodFilter(period, AccTransactionHeaderSchema.AH_DueDate);
		}

		ZQuery GetPostPeriodQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZInt period = ZInt.Parse(value);
			return GetPeriodFilter(period, AccTransactionHeaderSchema.AH_PostDate);
		}

		ZQuery GetAH_PostDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZQuery filter = new ZQuery();
			AddDateTimeRange(filter, comparisonOperator, JoinCondition.And, AccTransactionHeaderSchema.AH_PostDate, date1, date2);
			return filter;
		}

		ZQuery GetDateUploadedQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var hasNoDateEntered = comparisonOperator == DateComparisonOperator.HasNoDateEntered;

			var subQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
			if (!hasNoDateEntered)
			{
				AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, StmALogSchema.SL_EventTime, date1, date2);
			}

			var subQuery2 = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code);
			subQuery.AddToFilter(subQuery2, JoinCondition.And);

			var genApprovalRequestQuery = new ZDBOnlySubQuery(typeof(GenApprovalRequest), GenApprovalRequestSchema.XP_ParentID, hasNoDateEntered);
			genApprovalRequestQuery.AddSubQuery(subQuery, JoinCondition.And);

			var glJournalQuery = new ZDBOnlyQuery(typeof(GLJournal));
			glJournalQuery.AddSubQuery(genApprovalRequestQuery, JoinCondition.Or);
			if (hasNoDateEntered)
			{
				var subQuery3 = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, true);
				var subQuery4 = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code);
				subQuery3.AddToFilter(subQuery4, JoinCondition.And);
				glJournalQuery.AddSubQuery(subQuery3, JoinCondition.And);
			}
			else
			{
				glJournalQuery.AddSubQuery(subQuery, JoinCondition.Or);
			}

			return glJournalQuery;
		}

		#endregion

		#region Lists

		protected CodeDescriptionPairList fTransactionTypeList;
		public CodeDescriptionPairList TransactionTypeList
		{
			get
			{
				if (fTransactionTypeList == null)
				{
					fTransactionTypeList = new CodeDescriptionPairList(OLookUpEditType.GLJournalTypes);
				}
				return fTransactionTypeList;
			}
		}

		protected GlbBranchCollection fAH_GBList;
		public GlbBranchCollection AH_GBList
		{
			get
			{
				if (fAH_GBList == null)
				{
					ZQuery filter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
					fAH_GBList = new GlbBranchCollection(Factory, filter);
				}
				return fAH_GBList;
			}
		}

		protected GlbDepartmentCollection fAH_GEList;
		public GlbDepartmentCollection AH_GEList
		{
			get
			{
				if (fAH_GEList == null)
				{
					fAH_GEList = new GlbDepartmentCollection(Factory);
				}
				return fAH_GEList;
			}
		}

		#endregion

		#region Implementation

		protected ZQuery GetPeriodFilter(int period, SchemaColumn dateField)
		{
			ZQuery query = new ZQuery();

			if (PeriodCalculator.IsPeriodValid(period))
			{
				ZDateTime periodStartDate = PeriodCalculator.GetFirstDayForPeriod(period);
				ZDateTime periodEndDate = PeriodCalculator.GetLastDayForPeriod(period);
				query.AddToFilter(dateField, SQLComparisonOperator.GreaterThanOrEqualTo, periodStartDate);
				query.AddToFilter(dateField, SQLComparisonOperator.LessThanOrEqualTo, periodEndDate);
			}

			return query;
		}

		AccountingPeriodCalculator fPeriodCalculator;
		protected AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Factory);
				}

				return fPeriodCalculator;
			}
		}

		#endregion
	}
}
