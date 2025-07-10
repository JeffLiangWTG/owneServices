using System;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Aggregator
{
	public class SingleCompanyReAggregator : ReAggregator, ISingleCompanyReAggregator
	{
		public SingleCompanyReAggregator(ZGuid companyPK)
		{
			CompanyPK = companyPK;
		}
		readonly ZGuid CompanyPK;
		readonly string companyParameterName = "@CompanyPK";

		void ISingleCompanyReAggregator.ReAggregate()
		{
			ReAggregateCore();
		}

		#region SQL

		protected override ZString GetSqlForClearAggregateTable() => base.GetSqlForClearAggregateTable() + $" WHERE {AccGLAggregateSchema.Constants.AA_GC} = {companyParameterName}";

		protected override ZString GetSqlForReQueueCashVAT() => base.GetSqlForReQueueCashVAT() + $" AND {AccCashBasisVATSchema.Constants.YC_GC} = {companyParameterName}";

		protected override ZString GetSqlForReQueueTaxGLMovement()
		{
			return FormattableString.Invariant(
						$@"INSERT INTO {AccTaxGLMovementQueueSchema.Constants.SqlSchemaName}.{AccTaxGLMovementQueueSchema.Constants.TableName} ({AccTaxGLMovementQueueSchema.Constants.PK})
						SELECT {AccTaxGLMovementSchema.Constants.PK}
						FROM {AccTaxGLMovementSchema.Constants.SqlSchemaName}.{AccTaxGLMovementSchema.Constants.TableName}
						INNER JOIN {AccTaxTransactionSchema.Constants.SqlSchemaName}.{AccTaxTransactionSchema.Constants.TableName} ON {AccTaxTransactionSchema.Constants.PK} =  {AccTaxGLMovementSchema.Constants.ATM_ATT_TaxTransaction}
						LEFT JOIN {AccTaxGLMovementQueueSchema.Constants.SqlSchemaName}.{AccTaxGLMovementQueueSchema.Constants.TableName} ON {AccTaxGLMovementQueueSchema.Constants.PK} = {AccTaxGLMovementSchema.Constants.PK}
						WHERE {AccTaxGLMovementQueueSchema.Constants.PK} IS NULL AND {AccTaxTransactionSchema.Constants.ATT_GC} =  {companyParameterName}");
		}

		protected override ZString GetSqlForUpdateHeaderFlags() => base.GetSqlForUpdateHeaderFlags() + $" WHERE {AccTransactionHeaderSchema.Constants.AH_GC} = {companyParameterName}";

		protected override ZString GetSqlForUpdateLineFlags() => base.GetSqlForUpdateLineFlags() + $" AND {AccTransactionLinesSchema.Constants.AL_GC} = {companyParameterName}";

		protected override ZString AddAdditionalFiltersForReAggregateGLQuery() => $"AND {AccTransactionHeaderSchema.Constants.AH_GC} = {companyParameterName}";

		protected override void AddCommandParameters(DbCommand command) => command.AddParameter(companyParameterName, System.Data.SqlDbType.UniqueIdentifier, CompanyPK.ToGuid());

		#endregion
	}
}
