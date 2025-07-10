using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer
{
	public abstract class TransactionExportFilter
	{
		public TransactionExportFilter(BusinessObjectFactory factory, TransactionExportFilterProvider filterProvider)
		{
			this.Factory = factory;
			this.FilterProvider = filterProvider;
		}

		public ZQuery Filter
		{
			get
			{
				ZQuery result;
				if (AtLeastOneTypeOfTransactionIsSelected || !ExportingNewBatch)
				{
					result = CreateFilterForBatch();
				}
				else
				{
					result = new ZQuery();
					result.IsNoResultQuery = true;
				}
				return result;
			}
		}

		protected internal ZDBOnlyQuery CreateNewQuery(ZString ledger, StringCollectionX tranTypes)
		{
			ZDBOnlyQuery dBOnlyQuery = CreateDefaultQuery();
			dBOnlyQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledger);

			if (tranTypes.Count > 0)
			{
				dBOnlyQuery.AddToFilter(GetTranTypesFilter(tranTypes), JoinCondition.And);
			}

			if (ExportingNewBatch)
			{
				AddJobRelationshipSubQueryForInvoices(dBOnlyQuery, ledger);
				AddDatesFilter(dBOnlyQuery, FromToDatesColumnToFilterOn);
				AddPeriodsFilter(dBOnlyQuery, AccTransactionHeaderSchema.AH_PostDate);
				AddTransactionNumbersFilter(dBOnlyQuery, AccTransactionHeaderSchema.AH_TransactionNum);
				AddOrganisationsSubQuery(dBOnlyQuery, AccTransactionHeaderSchema.AH_OH, AccTransactionHeaderSchema.AH_GB);
				AddBranchesSubQuery(dBOnlyQuery, AccTransactionHeaderSchema.AH_GB);
				AddDepartmentsSubQuery(dBOnlyQuery, AccTransactionHeaderSchema.AH_GE);
			}

			return dBOnlyQuery;
		}

		protected ZDBOnlyQuery CreateDefaultQuery()
		{
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(BusinessObjectType);
			dbOnlyQuery.DefaultJoinCondition = JoinCondition.And;
			AddAdditionalFilter(dbOnlyQuery);
			AddBatchNumberQuery(dbOnlyQuery);
			AddCompanyBranchFilter(dbOnlyQuery);
			return dbOnlyQuery;
		}

		void AddHighWaterMarkFilterIfApplicable(ZDBOnlyQuery dBOnlyQuery)
		{
			if (FilterProvider.UsingHighWaterMark)
			{
				dBOnlyQuery.AddToFilter(SystemLastEditTimeColumn, SQLComparisonOperator.GreaterThan, FilterProvider.HighWaterMark);
			}
		}

		protected virtual void AddAdditionalFilter(ZDBOnlyQuery dBOnlyQuery)
		{
		}

		protected virtual void AddCompanyBranchFilter(ZDBOnlyQuery dBOnlyQuery)
		{
			dBOnlyQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
		}

		protected virtual void AddJobRelationshipSubQueryForInvoices(ZDBOnlyQuery mainQuery, ZString ledger)
		{
			if (FilterProvider.Jobs.Count > 0 || FilterProvider.ExcludeJobRelatedTransactionsForAR || FilterProvider.ExcludeNonJobRelatedTransactionsForAR ||
				FilterProvider.ExcludeJobRelatedTransactionsForAP || FilterProvider.ExcludeNonJobRelatedTransactionsForAP)
			{
				ZDBOnlySubQuery invoiceLinesSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);

				if (FilterProvider.Jobs.Count > 0)
				{
					invoiceLinesSubQuery.AddToFilter(AccTransactionLinesSchema.AL_JH, FilterProvider.Jobs.GetPKs());
				}

				if (ledger == LedgerTypes.AccountsReceivable)
				{
					if (FilterProvider.ExcludeJobRelatedTransactionsForAR)
					{
						invoiceLinesSubQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, null);
					}

					if (FilterProvider.ExcludeNonJobRelatedTransactionsForAR)
					{
						invoiceLinesSubQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.NotEqual, null);
					}
				}
				else if (ledger == LedgerTypes.AccountsPayable)
				{
					if (FilterProvider.ExcludeJobRelatedTransactionsForAP)
					{
						invoiceLinesSubQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.Equal, null);
					}

					if (FilterProvider.ExcludeNonJobRelatedTransactionsForAP)
					{
						invoiceLinesSubQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.NotEqual, null);
					}
				}

				mainQuery.AddSubQuery(invoiceLinesSubQuery, JoinCondition.And);
			}
		}

		protected virtual void AddBatchNumberQuery(ZDBOnlyQuery dBOnlyQuery)
		{
			if (ExportingNewBatch)
			{
				ZDBOnlySubQuery genExportQuery = new ZDBOnlySubQuery(typeof(GenExportBatchSequence), GenExportBatchSequenceSchema.XB_ParentID, true);
				genExportQuery.AddToFilter(GenExportBatchSequenceSchema.XB_Type, GetGenExportBatchSequenceType());
				dBOnlyQuery.AddSubQuery(genExportQuery, JoinCondition.And);
				AddHighWaterMarkFilterIfApplicable(dBOnlyQuery);
			}
			else
			{
				ZDBOnlySubQuery genExportQuery = new ZDBOnlySubQuery(typeof(GenExportBatchSequence), GenExportBatchSequenceSchema.XB_ParentID);
				genExportQuery.AddToFilter(GenExportBatchSequenceSchema.XB_Type, GetGenExportBatchSequenceType());
				genExportQuery.AddToFilter(GenExportBatchSequenceSchema.XB_BatchNumber, BatchNumber);
				dBOnlyQuery.AddSubQuery(genExportQuery, JoinCondition.And);
			}
		}

		protected virtual string GetGenExportBatchSequenceType()
		{
			return Constants.DataExportBatchSubTypes.Codes.AccountingTransactionHeaderExport;
		}

		ZQuery GetTranTypesFilter(StringCollectionX tranTypes)
		{
			ZQuery result = new ZQuery();
			result.DefaultJoinCondition = JoinCondition.Or;

			foreach (ZString tranType in tranTypes)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, tranType);
			}
			return result;
		}

		protected virtual SchemaDateTimeColumn FromToDatesColumnToFilterOn
		{
			get { return AccTransactionHeaderSchema.AH_PostDate; }
		}

		public Type BusinessObjectType
		{
			get
			{
				return BusinessObjectTypeCore;
			}
		}

		public int NumberOfObjects
		{
			get
			{
				return Factory.GetDatabaseCount(BusinessObjectType, Filter);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Custom call to DB necessary since aggregator must issue command to DB directly, It's too expensive to use Factory in this case")]
		public List<ZGuid> GetFilterPks(bool getTop1)
		{
			var filter = this.Filter;
			string sqlQuery;
			List<ZGuid> pks = new List<ZGuid>();

			if (filter.IsNoResultQuery)
			{
				return pks;
			}

			sqlQuery = string.Format("SELECT {0} {1} FROM {2} {3}", getTop1 ? "Top 1" : string.Empty, ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(GetTableName()).Name, GetTableName(), filter.GetAsWhereClause(false).ToString());

			DbCommand cmd = Db.Connection.Command(sqlQuery);
			foreach (var param in filter.Params)
			{
				cmd.AddParameter(param);
			}

			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					pks.Add(reader.GetGuid(0));
				}
			}
			return pks;
		}

		protected virtual string GetTableName()
		{
			return AccTransactionHeaderSchema.Constants.TableName;
		}

		public ZInt BatchNumber
		{
			get { return FilterProvider.CurrentBatchNo; }
		}

		protected bool ExportingNewBatch
		{
			get { return BatchNumber == EmptyBatchNumber; }
		}

		protected abstract Type BusinessObjectTypeCore { get; }

		public abstract SchemaDateTimeColumn SystemLastEditTimeColumn { get; }
		protected abstract bool AtLeastOneTypeOfTransactionIsSelected { get; }

		protected abstract ZQuery CreateFilterForBatch();

		#region Implementation

		protected void AddDatesFilter(ZDBOnlyQuery mainQuery, SchemaDateTimeColumn postDateField)
		{
			if (!FilterProvider.DateFrom.IsEmpty && !FilterProvider.DateTo.IsEmpty)
			{
				mainQuery.AddToFilter(JoinCondition.And, postDateField, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, FilterProvider.DateFrom);
				mainQuery.AddToFilter(JoinCondition.And, postDateField, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, FilterProvider.DateTo);
			}
		}

		protected void AddPeriodsFilter(ZDBOnlyQuery mainQuery, SchemaDateTimeColumn postDateField)
		{
			if (FilterProvider.PeriodFrom > 0 && FilterProvider.PeriodTo > 0)
			{
				AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
				ZDateTime startDate = periodCalculator.GetFirstDayForPeriod(FilterProvider.PeriodFrom);
				ZDateTime endDate = periodCalculator.GetLastDayForPeriod(FilterProvider.PeriodTo);

				if (!startDate.IsEmpty && !endDate.IsEmpty)
				{
					mainQuery.AddToFilter(JoinCondition.And, postDateField, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, startDate);
					mainQuery.AddToFilter(JoinCondition.And, postDateField, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, endDate);
				}
			}
		}

		protected void AddTransactionNumbersFilter(ZDBOnlyQuery mainQuery,SchemaStringColumn transactionNumberField)
		{
			if (!FilterProvider.TransactionNumberFrom.IsEmpty)
			{
				mainQuery.AddToFilter(JoinCondition.And, transactionNumberField, SQLComparisonOperator.GreaterThanOrEqualTo, FilterProvider.TransactionNumberFrom);
			}
			if (!FilterProvider.TransactionNumberTo.IsEmpty)
			{
				mainQuery.AddToFilter(JoinCondition.And, transactionNumberField, SQLComparisonOperator.LessThanOrEqualTo, FilterProvider.TransactionNumberTo );
			}
		}

		protected virtual void AddOrganisationsSubQuery(ZDBOnlyQuery mainQuery, SchemaGuidColumn foreignKeyToMainTable, SchemaGuidColumn foreignKeyToBranch)
		{
			if (FilterProvider.Organisations.Count > 0)
			{
				ZDBOnlySubQuery organisationsSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), foreignKeyToMainTable);
				organisationsSubQuery.AddToFilter(OrgHeaderSchema.PK, FilterProvider.Organisations.GetPKs());
				mainQuery.AddSubQuery(organisationsSubQuery, JoinCondition.And);
			}
		}

		protected void AddBranchesSubQuery(ZDBOnlyQuery mainQuery, SchemaGuidColumn foreignKeyToMainTable)
		{
			if (FilterProvider.Branches.Count > 0)
			{
				ZDBOnlySubQuery branchesSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), foreignKeyToMainTable);
				branchesSubQuery.AddToFilter(GlbBranchSchema.PK, FilterProvider.Branches.GetPKs());
				mainQuery.AddSubQuery(branchesSubQuery, JoinCondition.And);
			}
		}

		protected void AddDepartmentsSubQuery(ZDBOnlyQuery mainQuery, SchemaGuidColumn foreignKeyToMainTable)
		{
			if (FilterProvider.Departments.Count > 0)
			{
				ZDBOnlySubQuery departmentsSubQuery = new ZDBOnlySubQuery(typeof(GlbDepartment), foreignKeyToMainTable);
				departmentsSubQuery.AddToFilter(GlbDepartmentSchema.PK, FilterProvider.Departments.GetPKs());
				mainQuery.AddSubQuery(departmentsSubQuery, JoinCondition.And);
			}
		}

		#endregion

		protected const int EmptyBatchNumber = 0;
		protected readonly BusinessObjectFactory Factory;
		protected readonly TransactionExportFilterProvider FilterProvider;
	}
}
