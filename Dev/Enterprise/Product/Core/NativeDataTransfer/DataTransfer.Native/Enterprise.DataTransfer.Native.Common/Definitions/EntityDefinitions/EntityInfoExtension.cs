using System.Collections.Generic;
using System.Linq;
using Enterprise.DataTransfer.Native.DB.Helpers;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions
{
	static class EntityInfoExtension
	{
		public static IEnumerable<AssociationInfo> FindParentAssociations(this EntityDefinition definition, IEnumerable<AssociationInfo> associations)
		{
			foreach (var association in associations)
			{
				if (definition.EntityName == association.ChildName)
				{
					yield return association;
				}
			}
		}

		public static IEnumerable<AssociationInfo> FindChildAssociations(this EntityDefinition definition, IEnumerable<AssociationInfo> associations)
		{
			foreach (var association in associations)
			{
				if (!association.ParentName.IsEmpty())
				{
					if (definition.EntityName == association.ParentName)
					{
						yield return association;
					}
				}
				else if (association.ParentKeys.Select(x => x.ParentKey)
							.Any(x => !x.IsEmpty() && definition.TablePrefix == ColumnNameHelper.GetParentTablePrefix(x)))
				{
					yield return association;
				}
			}
		}
	}
}
