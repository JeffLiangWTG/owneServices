using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Engine;

namespace Enterprise.AuditDataServices.ArchiveManager.Helpers
{
	[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "We are querying metadata")]
	public class DatabaseGraphTraverser
	{
		readonly IApplicationSchemaResolver resolver;
		readonly Lazy<HashSet<ArchiveableRelationship>> allRelationships;

		public DatabaseGraphTraverser()
		{
			resolver = ObjectFactory.Get<IApplicationSchemaResolver>();
			allRelationships = new(GetAllArchiveableRelationships);
		}

		public IEnumerable<ArchiveableRelationship> TraverseDatabaseToFindAllRelevantRelationships(IEnumerable<string> tableNamesToTraverse)
		{
			var alreadyVisitedNodes = new HashSet<string>();
			var toVisit = new Queue<string>();

			foreach (var table in tableNamesToTraverse)
			{
				if (resolver.GetTableSchema(table) != null)
				{
					toVisit.Enqueue(table);
					_ = alreadyVisitedNodes.Add(table);
				}
			}

			while (toVisit.Count != 0)
			{
				var currentTable = toVisit.Dequeue();
				var relationshipsForTable = allRelationships.Value.Where(r => r.ParentName == currentTable);

				foreach (var relationship in relationshipsForTable)
				{
					yield return relationship;

					if (alreadyVisitedNodes.Add(relationship.ChildName))
					{
						toVisit.Enqueue(relationship.ChildName);
					}
				}
			}
		}

		HashSet<ArchiveableRelationship> GetAllArchiveableRelationships()
		{
			var allRelationships = new HashSet<ArchiveableRelationship>();
			using var cmd = Db.Connection.Command(SqlForGettingAllArchiveableForeignKeyConstraints);
			using var reader = cmd.ExecuteReader();

			while (reader.Read())
			{
				var parentTable = reader.GetString(reader.GetOrdinal("ParentTableName"));
				var childTable = reader.GetString(reader.GetOrdinal("ChildTableName"));
				var foreignKeyColumn = reader.GetString(reader.GetOrdinal("ForeignKeyColumn"));

				var parentTableSchema = resolver.GetTableSchema(parentTable);
				var childTableSchema = resolver.GetTableSchema(childTable);

				if (parentTableSchema is null || childTableSchema is null)
				{
					continue;
				}

				var relationship = new ArchiveableRelationship(
					parentName: parentTable,
					parentPKColumn: parentTableSchema.PK,
					parentKeyColumnReferencedByChild: resolver.GetPkColumn(parentTable),
					childName: childTable,
					childPKColumn: childTableSchema.PK,
					childFKColumn: childTableSchema.GetSchemaColumn(foreignKeyColumn),
					isReversed: false);

				_ = allRelationships.Add(relationship);
			}

			return allRelationships;
		}

		const string SqlForGettingAllArchiveableForeignKeyConstraints = @"SELECT 
	object_name(fk.referenced_object_id) AS [ParentTableName],
	t.name AS [ChildTableName],
	c.name AS [ForeignKeyColumn],
	fks.delete_referential_action AS [DeleteAction]
FROM
	sys.foreign_key_columns AS fk
INNER JOIN 
	sys.tables AS t ON fk.parent_object_id = t.object_id
INNER JOIN 
	sys.columns AS c ON fk.parent_object_id = c.object_id AND fk.parent_column_id = c.column_id
INNER JOIN
	sys.foreign_keys AS fks ON fks.object_id = fk.constraint_object_id
WHERE 
	delete_referential_action_desc = 'NO_ACTION'";
	}
}
