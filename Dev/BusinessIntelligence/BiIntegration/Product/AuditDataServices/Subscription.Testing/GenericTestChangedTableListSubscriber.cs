namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System.Collections.Generic;
	using System.Globalization;
	using CargoWise.Schema;
	using Enterprise.AuditDataServices.Subscription.Common;
	using Enterprise.Integration;

	public class GenericTestChangedTableListSubscriber : ChangedTableListOnlyAuditSubscriber
	{
		public GenericTestChangedTableListSubscriber(string code, string description, ITableSchema[] tables)
		{
			this.code = code;
			this.description = description;
			this.tables = tables;
		}

		public override string Code
		{
			get { return code; }
		}
		readonly string code;

		public override string Description
		{
			get { return description; }
		}
		readonly string description;

		public override bool IsRequired()
		{
			return true;
		}

		public override IEnumerable<ITableSchema> SubscribedTables
		{
			get { return tables; }
		}
		readonly ITableSchema[] tables;

		public override void ProcessChanges(ILogger logger, IEnumerable<ITableSchema> changedTables)
		{
			foreach (var changedTable in changedTables)
			{
				logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}]", changedTable.SqlSchemaName, changedTable.TableName));
			}
		}
	}
}
