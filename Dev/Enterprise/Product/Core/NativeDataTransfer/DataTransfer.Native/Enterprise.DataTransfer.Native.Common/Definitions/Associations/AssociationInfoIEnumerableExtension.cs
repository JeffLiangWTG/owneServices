using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.DB.Helpers;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.Common.Definitions.Associations
{
	static class AssociationInfoIEnumerableExtension
	{
		public static AssociationInfo FindAssociation(this IEnumerable<AssociationInfo> associations, EntityDefinition parentDefinition, EntityDefinition childDefinition)
		{
			var results = new List<AssociationInfo>();
			foreach (var association in associations)
			{
				if (association.ChildName != childDefinition.EntityName)
				{
					continue;
				}

				if (!association.ParentName.IsEmpty())
				{
					if (parentDefinition.EntityName == association.ParentName)
					{
						results.Add(association);
					}
				}
				else if (association.ParentKeys.Select(x => x.ParentKey)
									.Any(x => !x.IsEmpty() && parentDefinition.TablePrefix == ColumnNameHelper.GetParentTablePrefix(x)))
				{
					results.Add(association);
				}
			}

			if (results.Count == 0)
			{
				return null;
			}
			if (results.Count == 1)
			{
				return results[0];
			}
			throw new ApplicationException("More that one association with from:[" + parentDefinition.EntityName + "] and To:[" + childDefinition.EntityName + "], check something wrong with the definition file");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")] // REMOVE this under WI00139013. Please.
		public static EntityDefinition FindByName(this IEnumerable<EntityDefinition> definitions, string entityName)
		{
			var enumerator = definitions.Where(definition => definition.EntityName.Equals(entityName)).GetEnumerator();
			if (enumerator.MoveNext())
			{
				var result = enumerator.Current;
				if (enumerator.MoveNext())
				{
					throw new ApplicationException("Should only have one definition with EntityName:" + entityName + ". Please check your definition file");
				}
				return result;
			}
			throw new ApplicationException("Could not find definition with EntityName:" + entityName + ". Please check your definition file");
		}

		public static EntityDefinition FindEntityDefinition(this IEnumerable<EntityDefinition> definitions, string parentKey, string entityName)
		{
			if (parentKey.IsEmpty() && entityName.IsEmpty())
			{
				return null;
			}

			if (entityName.IsEmpty())
			{
				var tablePrefix = ColumnNameHelper.GetParentTablePrefix(parentKey);
				return definitions.FindDefinitionByTablePrefix(tablePrefix);
			}
			return definitions.FindByName(entityName);
		}

		public static EntityDefinition FindDefinitionByTablePrefix(this IEnumerable<EntityDefinition> definitions, string tablePrefix)
		{
			return definitions.First(definition => definition.TablePrefix == tablePrefix);
		}
	}
}
