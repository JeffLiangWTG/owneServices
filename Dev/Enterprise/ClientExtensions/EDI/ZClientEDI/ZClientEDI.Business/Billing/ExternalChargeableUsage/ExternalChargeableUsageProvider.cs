using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Integration;

namespace ZClientEDI.Business.Billing
{
	public abstract class ExternalChargeableUsageProvider
	{
		public virtual void BulkCopyUsages(BillingPeriod billingPeriod, ILogger logger)
		{
			using (var externalConnection = GetNewConnection())
			using (var usages = GetChargeableUsages(externalConnection, billingPeriod, logger))
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			using (var bulkCopy = extraConnection.GetSqlBulkCopy())
			{
				bulkCopy.BulkCopyTimeout = 0;
				bulkCopy.BatchSize = 100000;
				bulkCopy.DestinationTableName = usages.TableName;

				foreach (DataColumn column in usages.Columns)
				{
					bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
				}

				bulkCopy.WriteToServer(usages);
			}
		}

		public void LoadRawUsage(BillingLoadRawUsageContext context, Action<IDataReader> readerAction)
		{
			using (var externalConnection = GetNewConnection())
			using (var command = GetRawUsageCommand(externalConnection, context))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					readerAction(reader);
				}
			}
		}

		protected abstract DbConnection GetNewConnection();
		protected abstract DataTable GetChargeableUsages(DbConnection connection, BillingPeriod billingPeriod, ILogger logger);
		protected abstract DbCommand GetRawUsageCommand(DbConnection connection, BillingLoadRawUsageContext context);
	}
}
