namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using CargoWise.Data;
	using CargoWise.Schema;
	using Enterprise.AuditDataServices.Subscription.Common;
	using Enterprise.Integration;

	public class GenericTestDataChangeSubscriber : ActualDataChangesAuditSubscriber
	{
		public GenericTestDataChangeSubscriber(string code, string description, ITableSchema sourceTable, SchemaColumn[] columns = null, bool notifyInsert = true, bool notifyUpdate = true, bool notifyDelete = true, Action<DataRow> customFilter = null)
		{
			this.code = code;
			this.description = description;
			this.sourceTable = sourceTable;
			this.columns = columns;
			this.notifyInsert = notifyInsert;
			this.notifyUpdate = notifyUpdate;
			this.notifyDelete = notifyDelete;
			this.customFilter = customFilter;
		}

		public bool MakeDbConnection { get; set; }

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
			if (MakeDbConnection)
			{
				DbRegistry.BiAnalysisServer.LoadValue(Db.Connection);
			}

			return true;
		}

		public override ITableSchema Table
		{
			get { return sourceTable; }
		}
		readonly ITableSchema sourceTable;

		public override IEnumerable<SchemaColumn> SpecificColumns
		{
			get { return columns; }
		}
		readonly SchemaColumn[] columns;

		public override bool NotifyInsert
		{
			get { return notifyInsert; }
		}

		readonly bool notifyInsert;

		public override bool NotifyUpdate
		{
			get { return notifyUpdate; }
		}

		readonly bool notifyUpdate;

		public override bool NotifyDelete
		{
			get { return notifyDelete; }
		}

		readonly bool notifyDelete;

		public override Action<DataRow> CustomFilter
		{
			get { return customFilter; }
		}

		readonly Action<DataRow> customFilter;

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			foreach (DataRow row in changeTable.Rows)
			{
				var rowVersion = (row.RowState == DataRowState.Deleted) ? DataRowVersion.Original : DataRowVersion.Current;
				string codeColumn = TablePrefix + "_Code";
				string descColumn = TablePrefix + "_Desc";

				var logText = new StringBuilder();

				logText.AppendFormat(CultureInfo.InvariantCulture,
					"{0}.{1} - {2} - PK [{3}] = ",
					string.Join("", ((byte[])row[AuditFieldNames.StartLsnFieldName, rowVersion]).Select(b => string.Format("{0:x2}", b))),
					string.Join("", ((byte[])row[AuditFieldNames.SeqValFieldName, rowVersion]).Select(b => string.Format("{0:x2}", b))),
					row.RowState.ToString().PadRight(8, ' '),
					row[TablePrefix + "_PK", rowVersion].ToString()
				);

				switch (row.RowState)
				{
					case DataRowState.Added:
					case DataRowState.Deleted:
						logText.AppendFormat(CultureInfo.InvariantCulture,
							"Code [{0}] - Desc [{1}]",
							row[codeColumn, rowVersion].ToString().Trim(),
							row[descColumn, rowVersion].ToString().Trim()
						);
						break;

					case DataRowState.Modified:
						logText.AppendFormat(CultureInfo.InvariantCulture,
							"Code [{0}]=>[{1}] - Desc [{2}]=>[{3}]",
							row[codeColumn, DataRowVersion.Original].ToString().Trim(),
							row[codeColumn].ToString().Trim(),
							row[descColumn, DataRowVersion.Original].ToString().Trim(),
							row[descColumn].ToString().Trim()
						);
						break;
				}

				logger.Log(LogType.Information, logText.ToString());
			}
		}

		string TablePrefix
		{
			get
			{
				return tablePrefix ?? (tablePrefix = columnPrefixRegex.Match(sourceTable.PK.Name).Groups["PREFIX"].Value);
			}
		}
		string tablePrefix;

		static readonly Regex columnPrefixRegex = new Regex(@"^(?<PREFIX>\w{2,3})_", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	}
}
