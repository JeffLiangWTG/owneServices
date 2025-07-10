using Enterprise.DataTransfer.Native.Common.CodeMappings;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions
{
	public interface IEntitySetDefinition
	{
		string Name { get; }

		string NoImportReason { get; }

		// Enable for processing descendant entities in batches for performance
		bool UseBatching { get; }

		IEntityDefinition Root { get; }

		EntityDefinitionCollection Entities { get; }

		CodeMappingCollection CodeMappings { get; }
	}
}