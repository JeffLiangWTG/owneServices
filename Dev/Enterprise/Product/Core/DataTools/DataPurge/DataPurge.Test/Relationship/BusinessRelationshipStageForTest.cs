using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Schema;

namespace Enterprise.DataPurge
{
	public abstract class BusinessRelationshipStageForTest
	{
		public abstract void SetupRelationships();

		protected void AddRelationship(SchemaColumn parentKeyColumnReferencedByChild, SchemaColumn childFKColumn, string justificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords = null)
		{
			var parentTableName = parentKeyColumnReferencedByChild.TableName;
			var childTableName = childFKColumn.TableName;
			if (parentKeyColumnReferencedByChild.SqlDbType != childFKColumn.SqlDbType)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "parentKeyColumnReferencedByChild and childPKColumn must have the same SqlDbType, currently child '{0}' is {1} and parent '{2}' is {3}", childFKColumn.TableName, childFKColumn.SqlDbType, parentTableName, parentKeyColumnReferencedByChild.SqlDbType));
			}

			var relationship = new BusinessRelationship(parentTableName, parentKeyColumnReferencedByChild, childTableName, childFKColumn, justificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords);

			List<BusinessRelationship> relationshipList;
			if (RelationshipMap.TryGetValue(parentTableName, out var value))
			{
				relationshipList = value;
			}
			else
			{
				relationshipList = new List<BusinessRelationship>();
				RelationshipMap.Add(parentTableName, relationshipList);
			}

			if (relationshipList.Count == 0 || !relationshipList.Exists(delegate(BusinessRelationship r)
					{ return r.ParentTableName == parentTableName && r.ParentKeyColumnReferencedByChild == parentKeyColumnReferencedByChild && r.ChildTableName == childTableName && r.ChildFkColumn == childFKColumn; }))
			{
				relationshipList.Add(relationship);
			}
		}

		public Dictionary<string, List<BusinessRelationship>> RelationshipMap { get; } = new ();
	}
}
