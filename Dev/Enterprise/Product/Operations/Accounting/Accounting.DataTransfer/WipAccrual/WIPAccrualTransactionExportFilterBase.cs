using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.WipsAndAccruals
{
	public abstract class WIPAccrualTransactionExportFilterBase : TransactionExportFilter
	{
		public WIPAccrualTransactionExportFilterBase(BusinessObjectFactory factory, TransactionExportFilterProvider filterProvider) : base(factory, filterProvider)
		{
		}

		protected override string GetTableName()
		{
			return AccTransactionLinesSchema.Constants.TableName;
		}

		protected override ZQuery CreateFilterForBatch()
		{
			StringCollectionX lineTypes = new StringCollectionX();

			if (ExportingNewBatch)
			{
				if (IncludeWIPs)
				{
					lineTypes.Add(TransactionLineTypes.WIP);
				}

				if (IncludeAccruals)
				{
					lineTypes.Add(TransactionLineTypes.Accrual);
				}
			}

			return CreateQueryForWIPAccruals(lineTypes);
		}

		protected abstract bool IncludeWIPs { get; }
		protected abstract bool IncludeAccruals { get; }
		protected abstract SchemaDateTimeColumn PostOrReverseDate { get; }

		#region Query Builders

		internal ZDBOnlyQuery CreateQueryForWIPAccruals(StringCollectionX lineTypes)
		{
			ZDBOnlyQuery dBOnlyQuery = CreateDefaultQuery();

			if (lineTypes.Count > 0)
			{
				dBOnlyQuery.AddToFilter(GetLineTypesFilter(lineTypes), JoinCondition.And);
			}

			if (ExportingNewBatch)
			{
				AddJobRelationshipSubQueryForWIPAccruals(dBOnlyQuery);
				AddDatesFilter(dBOnlyQuery, PostOrReverseDate);
				AddPeriodsFilter(dBOnlyQuery, PostOrReverseDate);
				AddOrganisationsSubQuery(dBOnlyQuery, AccTransactionLinesSchema.AL_OH, AccTransactionLinesSchema.AL_GB);
				AddBranchesSubQuery(dBOnlyQuery, AccTransactionLinesSchema.AL_GB);
				AddDepartmentsSubQuery(dBOnlyQuery, AccTransactionLinesSchema.AL_GE);
			}
			return dBOnlyQuery;
		}

		protected ZQuery GetLineTypesFilter(StringCollectionX lineTypes)
		{
			ZQuery result = new ZQuery();
			result.DefaultJoinCondition = JoinCondition.Or;
			foreach (string lineType in lineTypes)
			{
				result.AddToFilter(AccTransactionLinesSchema.AL_LineType, lineType);
			}
			return result;
		}

		protected void AddJobRelationshipSubQueryForWIPAccruals(ZDBOnlyQuery mainQuery)
		{
			if (FilterProvider.Jobs.Count > 0)
			{
				ZDBOnlySubQuery jobLinesSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionLinesSchema.AL_JH);
				jobLinesSubQuery.AddToFilter(AccTransactionLinesSchema.AL_JH, FilterProvider.Jobs.GetPKs());
				mainQuery.AddSubQuery(jobLinesSubQuery, JoinCondition.And);
			}

			if (FilterProvider.ExcludeJobRelatedTransactionsForAR && FilterProvider.ExcludeJobRelatedTransactionsForAP)
			{
				// DBOnlyQuery doesn't seem to work with NoResultQuery currently, so we do this instead
				mainQuery.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.PK, SQLComparisonOperator.Equal, ZGuid.Empty);
			}
			else if (FilterProvider.ExcludeJobRelatedTransactionsForAR)
			{
				ZQuery excludeARJobRelatedTransactions = new ZQuery(AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.NotEqual, ZGuid.Empty);
				excludeARJobRelatedTransactions.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.NotEqual, TransactionLineTypes.WIP);
				mainQuery.AddToFilter(excludeARJobRelatedTransactions, JoinCondition.And);
			}
			else if (FilterProvider.ExcludeJobRelatedTransactionsForAP)
			{
				ZQuery excludeAPJobRelatedTransactions = new ZQuery(AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.NotEqual, null);
				excludeAPJobRelatedTransactions.AddToFilter(JoinCondition.And, AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.NotEqual, TransactionLineTypes.Accrual);
				mainQuery.AddToFilter(excludeAPJobRelatedTransactions, JoinCondition.And);
			}
		}

		protected override void AddCompanyBranchFilter(ZDBOnlyQuery dBOnlyQuery)
		{
			dBOnlyQuery.AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);
		}

		#endregion
	}
}
