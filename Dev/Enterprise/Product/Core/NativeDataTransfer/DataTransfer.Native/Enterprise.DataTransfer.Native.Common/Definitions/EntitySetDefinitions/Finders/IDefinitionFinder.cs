using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders
{
	public interface IDefinitionFinder
	{
		bool HasDefinitionWithEntitySetName(string entitySetName);
		bool HasDefinitionWithTopTableName(string topLevelTableName);
		EntitySetDefinition FindByEntitySetName(string entitySetName);
		EntitySetDefinition FindByTopTableName(string topLevelTableName);
		EntityDefinition FindDefinition(string entitySetName, string entityName);
	}
}