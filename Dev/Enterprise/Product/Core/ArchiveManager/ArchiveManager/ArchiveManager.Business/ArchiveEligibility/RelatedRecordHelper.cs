using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Engine;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.Rating.Business;

namespace Enterprise.ArchiveManager.Business.ArchiveEligibility
{
	public static class RelatedRecordHelper
	{
		public static IEnumerable<ArchiveItem> GetRelatedRecords(BusinessObject bizo, Dictionary<string, List<ArchiveableRelationship>> allSystemRelationships)
		{
			var recordsQueue = new Queue<ArchiveItem>([ArchiveItemHelper.FromBizo(bizo)]);
			var seenRecordsByTableCode = new Dictionary<string, HashSet<Guid>>();
			_ = seenRecordsByTableCode.GetOrAdd(bizo.TablePrefix).Add(bizo.PK.ToGuid());

			do
			{
				var currentItem = recordsQueue.Dequeue();
				var relationshipsForCurrentItem = allSystemRelationships.GetValueSafe(currentItem.PKColumn.TableName);

				if (relationshipsForCurrentItem is null)
				{
					continue;
				}

				var sqlForChildRecords = GetSqlForChildPKs(relationshipsForCurrentItem);
				var tvpParameters = GetTVPsForRelationships(relationshipsForCurrentItem, seenRecordsByTableCode);
				var childRecords = ExecuteSqlForChildPKs(sqlForChildRecords, currentItem, tvpParameters).ToArray();

				foreach (var child in childRecords)
				{
					if (seenRecordsByTableCode.GetOrAdd(child.TableCode).Add(child.PK))
					{
						yield return child;
						recordsQueue.Enqueue(child);
					}
				}
			} while (!recordsQueue.IsEmpty());
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "It is SQL")]
		static string GetSqlForChildPKs(List<ArchiveableRelationship> relationships)
		{
			return relationships
				.Select(GetSqlForSingleRelationship)
				.ToStringWithDelimiterBetweenStrings("\nUNION ALL\n");
		}

		static string GetSqlForSingleRelationship(ArchiveableRelationship relationship)
		{
			var childTableNameQuoted = relationship.ChildName.QuoteName('\'');
			var childPkColumnName = relationship.ChildPKColumn.Name.QuoteName();
			var isReversed = "CONVERT(BIT, " + (relationship.IsReversed ? "1" : "0") + ")";
			var childTableName = relationship.ChildName.QuoteName();
			var childFkColumnName = relationship.ChildFKColumn.Name.QuoteName();
			var parentPkReferencedByChild = relationship.ParentKeyColumnReferencedByChild.Name.QuoteName();
			var parentTableName = relationship.ParentName.QuoteName();
			var parentPkColumnName = relationship.ParentPKColumn.Name.QuoteName();
			var seenRecordsTVPName = SeenRecordsTVPNameForTableCode(relationship.ChildPKColumn.ColumnPrefix);

			return $@"
				SELECT
					{childTableNameQuoted} AS [child_table_name],
					{childPkColumnName} AS [child_pk],
					{isReversed} AS [is_reversed]
				FROM {childTableName}
				WHERE {childFkColumnName} IN (SELECT {parentPkReferencedByChild} FROM {parentTableName} WHERE {parentPkColumnName} = @ParentPk)
					AND {childPkColumnName} NOT IN (SELECT * FROM {seenRecordsTVPName})";
		}

		[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Cannot use factory here")]
		static IEnumerable<ArchiveItem> ExecuteSqlForChildPKs(string sql, ArchiveItem currentItem, Dictionary<string, Guid[]> tvpParameters)
		{
			var resolver = ObjectFactory.Get<IApplicationSchemaResolver>();

			using var cmd = Db.Connection.Command(sql);
			AddCommandParameters(cmd, currentItem, tvpParameters);
			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				var childTableName = reader.GetString(reader.GetOrdinal("child_table_name"));
				var childPk = reader.GetGuid(reader.GetOrdinal("child_pk"));
				var isReversed = reader.GetBoolean(reader.GetOrdinal("is_reversed"));
				var childPkColumn = resolver.GetTableSchema(childTableName).PK;

				yield return new ArchiveItem(childPkColumn, childPk, currentItem.PKColumn, currentItem.PK, isReversed, childPkColumn.ColumnPrefix);
			}
		}

		static void AddCommandParameters(DbCommand cmd, ArchiveItem currentItem, Dictionary<string, Guid[]> tvpParameters)
		{
			_ = cmd.AddParameter("@ParentPk", SqlDbType.UniqueIdentifier, currentItem.PK);

			foreach (var kvp in tvpParameters)
			{
				cmd.AddTableValuedParameter(kvp.Key, "dbo.TVP_uniqueidentifier", kvp.Value);
			}
		}

		[SuppressMessage("Style", "IDE0200:Remove unnecessary lambda expression", Justification = "Better clarity with full expression")]
		static Dictionary<string, Guid[]> GetTVPsForRelationships(List<ArchiveableRelationship> relationshipsForCurrentItem, Dictionary<string, HashSet<Guid>> seenRecordsByTableCode)
		{
			var distinctChildTableCodes = relationshipsForCurrentItem
				.Select(r => r.ChildPKColumn.ColumnPrefix)
				.Distinct();

			var tvpsForRelationships = distinctChildTableCodes.ToDictionary(
				tableCode => SeenRecordsTVPNameForTableCode(tableCode),
				tableCode => seenRecordsByTableCode.GetValueSafe(tableCode)?.ToArray() ?? []);

			return tvpsForRelationships;
		}

		static string SeenRecordsTVPNameForTableCode(string tableCode)
			=> $"@SeenRecordsFor{tableCode}";
	}
}
