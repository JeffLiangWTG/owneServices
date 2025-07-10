using CargoWise.Common;
using Enterprise.DataTransfer.Native.Common.CodeMappings;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions
{
	public class EntitySetDefinition : IEntitySetDefinition
	{
		public EntitySetDefinition(string name, string noImportReason, CodeMappingCollection codeMappings, bool isCompact, bool useBatching)
		{
			Name = name;
			NoImportReason = noImportReason;
			CodeMappings = codeMappings;
			IsCompact = isCompact;
			UseBatching = useBatching;
		}

		public string Name { get; }

		public string NoImportReason { get; }

		public EntityDefinitionCollection Entities { get; private set; }

		public CodeMappingCollection CodeMappings { get; }

		public IEntityDefinition Root { get; private set; }

		public bool IsCompact { get; }

		public bool UseBatching { get; }

		internal void SetEntities(EntityDefinitionCollection entities, IEntityDefinition root)
		{
			if (Entities != null || Root != null)
			{
				ErrorReporter.ReportOnce("This setter should only ever be called once from EntitySetDefinitionBuilder");
			}

			Entities = entities;
			Root = root;
		}
	}
}
