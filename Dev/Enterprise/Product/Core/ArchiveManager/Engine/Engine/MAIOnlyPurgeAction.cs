using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CargoWise.Data;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Engine
{
	public class MAIOnlyPurgeAction : PurgeAction
	{
		public MAIOnlyPurgeAction(IArchiveSet set, IArchiveLogger logger)
		: base(set, logger) { }

		#region IArchiveAction Members
		List<Guid> PKs { get; set; }

		#endregion
		protected override void LogPurgeDetails(Stopwatch sw)
		{
			var archiveItems = archiveSet.GetArchiveItems();
			var recordsToDelete = PKs.Count;

			logger?.LogInfo(archiveSet.SystemDescriptor.Code, $"Deleting {recordsToDelete} records from {archiveItems.FirstOrDefault().PKColumn.TableName}{Helpers.GetTimeTaken(sw)}");
		}

		protected override void SetupPurgeQuery()
		{
			var archiveItems = archiveSet.GetArchiveItems();
			var tableSchema = archiveItems.FirstOrDefault().PKColumn.TableSchema;
			AtLeastOneToDelete = false;

			PKs = new List<Guid>();
			foreach(var item in archiveItems)
			{
				PKs.Add(item.PK);
				AtLeastOneToDelete = true;
			}

			QueryBuilder = new StringBuilder("Declare @top bigint = 9223372036854775807" + System.Environment.NewLine);
			if (HasSelfReferences(tableSchema))
			{
				foreach (var columnName in GetSelfReferenceColumnNames(tableSchema))
				{
					_ = QueryBuilder.AppendLine($"DELETE TOP (@top) Child from	@tvp as tmp join {tableSchema.TableName} as Child on Child.{columnName} = tmp.Value where 1 = 1 AND Child.{columnName} IS NOT NULL OPTION (FORCE ORDER, OPTIMIZE FOR (@top = 1))");
				}

				_ = QueryBuilder.AppendLine((NoResString)"DELETE TOP (@top) " + tableSchema.TableName + (NoResString)" FROM @tvp AS tmp JOIN " + tableSchema.TableName + (NoResString)" ON tmp.Value = " + tableSchema.PK.Name + (NoResString)" OPTION (FORCE ORDER, OPTIMIZE FOR (@top = 1))");
			}
			else
			{
				_ = QueryBuilder.AppendLine((NoResString)"DELETE TOP (@top) " + tableSchema.TableName + (NoResString)" FROM @tvp AS tmp JOIN " + tableSchema.TableName + (NoResString)" ON tmp.Value = " + tableSchema.PK.Name + (NoResString)" OPTION (FORCE ORDER, OPTIMIZE FOR (@top = 1))");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Querying queue table for which no bizo exists")]
		protected override void ExecutePurgeQuery(StringBuilder query)
		{
			using (DbCommand command = Db.Connection.Command(query.ToString()))
			{
				command.AddTableValuedParameter((NoResString)"@tvp", "dbo.TVP_uniqueidentifier", PKs);
				_ = command.ExecuteNonQuery();
			}
		}
	}
}
