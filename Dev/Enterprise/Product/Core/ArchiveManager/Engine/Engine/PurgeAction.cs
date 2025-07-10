using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Engine
{
	public class PurgeAction : IArchiveAction
	{
		public PurgeAction(IArchiveSet set, IArchiveLogger logger)
		{
			archiveSet = set;
			this.logger = logger;
		}

		protected readonly IArchiveSet archiveSet;
		protected readonly IArchiveLogger logger;

		#region IArchiveAction Members

		public ITransactionManager BeginTransactionWithManager()
			=> Db.Connection.BeginTransactionWithManager();

		public void Execute()
		{
			if (Db.Connection.IsInTransaction)
			{
				if (archiveSet.Count > 0)
				{
					SetupPurgeQuery();

					Stopwatch sw = null;
					if (IncludeTimeTakenInTheARCLogs)
					{
						sw = new Stopwatch();
						sw.Start();
					}

					lock (this)
					{
						ExecutePurgeQuery(QueryBuilder);
					}

					if (IncludeTimeTakenInTheARCLogs)
					{
						sw.Stop();
					}

					if (AtLeastOneToDelete)
					{
						if (IncludeTimeTakenInTheARCLogs)
						{
							archiveSet.TimeTakenToDelete = sw.ElapsedMilliseconds;
						}

						LogPurgeDetails(sw);
					}
				}
			}
			else
			{
				throw new InvalidOperationException("SaveInTransaction() must be called within a transaction");
			}
		}

		protected virtual void LogPurgeDetails(Stopwatch sw)
		{
			var recordsToDelete = ItemsDictionary.Values.Sum(x => x.Count);
			var mainArchiveItemKey = archiveSet.MainArchiveItemNK ?? archiveSet.MainArchiveItem.PK.ToString();

			if (!archiveSet.MainArchiveItem.Purgeable)
			{
				logger?.LogInfo(archiveSet.SystemDescriptor.Code, $"Deleting {recordsToDelete} related records from {archiveSet.MainArchiveItem.PKColumn.TableName} '{mainArchiveItemKey}'{Helpers.GetTimeTaken(sw)}");
			}
			else
			{
				logger?.LogInfo(archiveSet.SystemDescriptor.Code, $"Deleting {archiveSet.MainArchiveItem.PKColumn.TableName} '{mainArchiveItemKey}' and {recordsToDelete - 1} related records{Helpers.GetTimeTaken(sw)}");
			}
		}
		#endregion
		Dictionary<string, List<Guid>> TableToPKs { get; set; }
		protected StringBuilder QueryBuilder { get; set; }
		protected bool AtLeastOneToDelete { get; set; }
		Dictionary<string, List<IArchiveItem>> ItemsDictionary { get; set; }
		StringBuilder DeleteQuery(ITableSchema tableSchema, StringBuilder queryBuilder, Dictionary<string, List<IArchiveItem>> itemsDictionary)
		{
			var tvpName = $"@{tableSchema.TableName}TVP";
			if (HasSelfReferences(tableSchema))
			{
				foreach (var item in itemsDictionary[tableSchema.TableName])
				{
					if (!TableToPKs.TryGetValue(tableSchema.TableName, out var pkList))
					{
						TableToPKs[tableSchema.TableName] = [];
					}

					TableToPKs[tableSchema.TableName].Add(item.PK);
				}

				foreach (var columnName in GetSelfReferenceColumnNames(tableSchema))
				{
					_ = queryBuilder.AppendLine($"DELETE TOP (@top) Child from	{tvpName} as tmp join {tableSchema.TableName} as Child on Child.{columnName} = tmp.Value where 1 = 1 AND Child.{columnName} IS NOT NULL OPTION (FORCE ORDER, OPTIMIZE FOR (@top = 1))");
				}

				_ = queryBuilder.AppendLine((NoResString)$"DELETE TOP (@top) {tableSchema.TableName} FROM {tvpName} AS tmp JOIN {tableSchema.TableName} ON tmp.Value = {tableSchema.PK.Name} OPTION (FORCE ORDER, OPTIMIZE FOR (@top = 1))");
			}
			else
			{
				foreach (var item in itemsDictionary[tableSchema.TableName])
				{
					if (!TableToPKs.TryGetValue(tableSchema.TableName, out var pkList))
					{
						TableToPKs[tableSchema.TableName] = [];
					}

					TableToPKs[tableSchema.TableName].Add(item.PK);
				}

				_ = queryBuilder.AppendLine((NoResString)$"DELETE TOP (@top) {tableSchema.TableName} FROM {tvpName} AS tmp JOIN {tableSchema.TableName} ON tmp.Value = {tableSchema.PK.Name} OPTION (FORCE ORDER, OPTIMIZE FOR (@top = 1))");
			}
			return queryBuilder;
		}

		protected virtual void SetupPurgeQuery()
		{
			ItemsDictionary = new Dictionary<string, List<IArchiveItem>>();
			var tableSchemaList = new List<ITableSchema>();
			AtLeastOneToDelete = false;

			foreach (var item in archiveSet.GetArchiveItems())
			{
				if (item.Purgeable)
				{
					if (!ItemsDictionary.TryGetValue(item.PKColumn.TableName, out List<IArchiveItem> list))
					{
						list = new List<IArchiveItem>();
						ItemsDictionary.Add(item.PKColumn.TableName, list);
						tableSchemaList.Add(item.PKColumn.TableSchema);
					}

					list.Add(item);
					AtLeastOneToDelete = true;
				}
			}

			TableToPKs = new Dictionary<string, List<Guid>>();
			var sortedTableSchemas = ZTableSaveOrderComparer.Sort(tableSchemaList.ToArray());

			QueryBuilder = new StringBuilder($"Begin Try" + System.Environment.NewLine);
			_ = QueryBuilder.AppendLine("Declare @top bigint = 9223372036854775807");

			for (int index = sortedTableSchemas.Length - 1; index >= 0; index--)
			{
				var tableSchema = sortedTableSchemas[index];
				QueryBuilder = DeleteQuery(tableSchema, QueryBuilder, ItemsDictionary);
			}
			_ = QueryBuilder.AppendLine($"End Try");
			_ = QueryBuilder.AppendLine($"Begin Catch");
			_ = QueryBuilder.AppendLine($"	DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();");
			_ = QueryBuilder.AppendLine($"	RAISERROR(@ErrorMessage, 16, 1);");
			_ = QueryBuilder.Append($"End Catch");
		}

		public string GetPurgeQuery()
		{
			SetupPurgeQuery();
			return QueryBuilder.ToString();
		}

		/// <summary>
		/// Deletes records in archive set in an order that will not cause constraint violations.
		/// </summary>
		protected virtual void ExecutePurgeQuery(StringBuilder query)
		{
#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
			using (DbCommand command = Db.Connection.Command(query.ToString()))
			{
				foreach (var table in TableToPKs.Keys)
				{
					command.AddTableValuedParameter((NoResString)$"@{table}TVP", "dbo.TVP_uniqueidentifier", TableToPKs[table]);
				}

				_ = command.ExecuteNonQuery();
			}
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods
		}

		protected bool HasSelfReferences(ITableSchema tableSchema)
		{
			foreach (SchemaColumn column in tableSchema.All)
			{
				var fkPrefix = ZRowRelationshipManager.GetForeignKeyTablePrefix(column.Name);
				if (!string.IsNullOrEmpty(fkPrefix))
				{
					if (fkPrefix == CargoWise.Schema.Schema.GetPrefixFromColumnName(column.Name))
					{
						return true;
					}
				}
			}

			return false;
		}

		protected IEnumerable<string> GetSelfReferenceColumnNames(ITableSchema tableSchema)
		{
			foreach (SchemaColumn column in tableSchema.All)
			{
				string fkPrefix = ZRowRelationshipManager.GetForeignKeyTablePrefix(column.Name);
				if (!string.IsNullOrEmpty(fkPrefix))
				{
					if (fkPrefix == CargoWise.Schema.Schema.GetPrefixFromColumnName(column.Name))
					{
						yield return column.Name;
					}
				}
			}
		}

		bool IncludeTimeTakenInTheARCLogs
			=> SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.Value;
	}
}
