using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions
{
	public class EntityDefinitionCollection : IEnumerable<EntityDefinition>
	{
		public EntityDefinitionCollection(IEnumerable<EntityDefinition> entities)
		{
			this.entities = entities.ToArray();
		}

		readonly EntityDefinition[] entities;

		public bool HasDefinition(string entityName)
		{
			return entities.Any(entity => entity.FullName == entityName);
		}

		public EntityDefinition FindDefinition(string entityName)
		{
			var results = entities.Where(entity => entity.FullName == entityName).ToArray();
			if (results.Length > 1)
			{
				throw new NativeXMLUserVisibleException("More than one entity found with name: " + entityName + ". Make sure you use Full Name for the Entity");
			}
			if (results.Length < 1)
			{
				throw new NativeXMLUserVisibleException("Could not found entity with name: " + entityName + ". Make sure you use Full Name for the Entity");
			}

			return results[0];
		}

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return entities.GetEnumerator();
		}

		#endregion

		#region IEnumerable<EntityDefinition> Members

		IEnumerator<EntityDefinition> IEnumerable<EntityDefinition>.GetEnumerator()
		{
			return ((IEnumerable<EntityDefinition>)entities).GetEnumerator();
		}

		#endregion
	}
}