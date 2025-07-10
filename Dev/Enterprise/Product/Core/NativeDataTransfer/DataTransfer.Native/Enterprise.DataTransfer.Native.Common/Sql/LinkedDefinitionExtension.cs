using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Definitions;
using Enterprise.DataTransfer.Native.Common.Definitions.Associations;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.DB.Sql;
using Enterprise.DataTransfer.Native.Utils.Models;

namespace Enterprise.DataTransfer.Native.Common.Sql
{
	static class LinkedDefinitionExtension
	{
		public static Queue<Relation> GetRelations(this IEntityDefinition start, IEntityDefinition end)
		{
			var path = start.FindPath(end);
			return path.GetRelations();
		}

		public static Queue<Relation> GetRelations(this Stack<IEntityDefinition> path)
		{
			var relations = new Queue<Relation>();

			while (path.Count > 1)
			{
				var current = path.Pop();
				var next = path.Peek();
				var association = GetAssociation(current, next);
				if (association is ManyToManyAssociation)
				{
					relations.Enqueue(association.GetRelation(current));
					relations.Enqueue(association.GetRelation(next));
				}
				else
				{
					relations.Enqueue(association.GetRelation(current));
				}
			}
			return relations;
		}

		static AssociationDefinition GetAssociation(IEntityDefinition currentEntity, IEntityDefinition nextEntity)
		{
			AssociationDefinition association = null;
			if (currentEntity.IsParentOf(nextEntity))
			{
				association = currentEntity.AssociationCollection[nextEntity, currentEntity];
			}

			if (currentEntity.IsChildrenOf(nextEntity))
			{
				association = currentEntity.AssociationCollection[currentEntity, nextEntity];
			}
			return association;
		}
	}
}