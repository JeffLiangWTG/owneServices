using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions.Builders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions
{
	class EntityInfoLoader
	{
		readonly XElement definitionData;
		readonly EntitySetDefinition info;

		public EntityInfoLoader(XElement definitionData, EntitySetDefinition info)
		{
			this.definitionData = definitionData;
			this.info = info;
		}

		internal IEnumerable<EntityDefinition> GetEntityInfos()
		{
			var entitiesElement = definitionData.Elements(TagName.Entities).First();
			var entityElements = entitiesElement.Elements(TagName.Entity);
			return entityElements.Select(BuildEntityInfo);
		}

		EntityDefinition BuildEntityInfo(XElement entityElement)
		{
			var definitionBuilder = new XmlEntityDefinitionBuilder(entityElement);
			var definition = definitionBuilder.Construct(info);
			return definition;
		}
	}
}
