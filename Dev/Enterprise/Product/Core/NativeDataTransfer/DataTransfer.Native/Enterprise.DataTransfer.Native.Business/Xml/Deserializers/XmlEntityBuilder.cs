using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.EntityBuilders;

namespace Enterprise.DataTransfer.Native.Business.Xml.Deserializers
{
	public class XmlEntityBuilder : EntityBuilder
	{
		public XmlEntityBuilder(XElement element, IEntityDefinition definition, AncillaryImportServices sessionServices)
		{
			this.definition = definition;
			Entity = new Entity(definition, sessionServices);
			parser = new EntityXmlDeserializer(element, definition);
		}
		readonly IEntityDefinition definition;
		readonly EntityXmlDeserializer parser;

		public override void BuildInternalPK()
		{
			Entity.InternalPK = parser.ParseInternalPK();
		}

		public override void BuildAction()
		{
			var action = parser.ParseAction();
			Entity.Action = action.ToEvent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public override void BuildProperties()
		{
			var errors = new List<string>();

			foreach (var property in parser.ParsePropertyElements())
			{
				var column = definition.PropertyDefinitions[property.Name];
				if (column != null)
				{
					var (success, error) = Entity.AddProperty(property);
					if (!success)
					{
						errors.Add(error);
					}
				}
			}

			if (errors.Count > 0)
			{
				var sb = new StringBuilder("Validation errors found in Native XML:\r\n");
				errors.ForEach(s => sb.AppendLine(s));
				throw new NativeXMLUserVisibleException(sb.ToString());
			}
		}
	}
}
