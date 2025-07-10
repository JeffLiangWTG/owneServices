using System;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders
{
	public class DefinitionFinder : IDefinitionFinder
	{
		public IDefinitionCache Cache { get; set; }

		#region IDefinitionFinder Members

		public bool HasDefinitionWithEntitySetName(string entitySetName)
		{
			return Cache.HasDefinition(entitySetName);
		}

		public bool HasDefinitionWithTopTableName(string topLevelTableName)
		{
			var entitySetName = FindEntitySetName(topLevelTableName);
			return HasDefinitionWithEntitySetName(entitySetName);
		}

		public EntitySetDefinition FindByEntitySetName(string entitySetName)
		{
			try
			{
				return Cache.Find(entitySetName);
			}
			catch (Exception ex)
			{
				throw new NativeXMLUserVisibleException(string.Format("Could not load EntitySet with Entity Set Name: {0} - {1}", entitySetName, ex.Message), ex);
			}
		}

		public EntitySetDefinition FindByTopTableName(string topLevelTableName)
		{
			try
			{
				var entitySetName = FindEntitySetName(topLevelTableName);
				return FindByEntitySetName(entitySetName);
			}
			catch (Exception ex)
			{
				throw new NativeXMLUserVisibleException(string.Format("Could not load EntitySet using Table Name: {0} - {1}", topLevelTableName, ex.Message), ex);
			}
		}

		public EntityDefinition FindDefinition(string entitySetName, string entityName)
		{
			return FindByEntitySetName(entitySetName).Entities.FindDefinition(entityName);
		}

		#endregion

		string FindEntitySetName(string topLevelTableName)
		{
			var mappings = GlobalDefinition.Instance.TableMapping;

			if (topLevelTableName.IsEmpty())
			{
				return string.Empty;
			}

			if (!mappings.ContainsKey(topLevelTableName))
			{
				return string.Empty;
			}

			return mappings[topLevelTableName];
		}
	}
}