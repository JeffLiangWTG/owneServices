using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Common.CodeMappings;

namespace Enterprise.DataTransfer.Native.Common.Definitions
{
	class GlobalDefinitionBuilder
	{
		const string GlobalDefinitionLocation = "Enterprise.DataTransfer.Native.Common.Definitions.DefinitionFiles.GlobalDefinition.xml";

		public XElement LoadDefinition()
		{
			var assembly = typeof(GlobalDefinition).Assembly;
			var definitionStream = assembly.GetManifestResourceStream(GlobalDefinitionLocation);
			var reader = XmlReader.Create(definitionStream);
			return XElement.Load(reader);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "look like ")]
		public Dictionary<string, string> ParseTableMappings(XElement definitionElement)
		{
			var mappings = new Dictionary<string, string>();
			var mappingElements = definitionElement.Element("TableMappings");
			if (mappingElements != null)
			{
				foreach (var mappingElement in mappingElements.Elements("Mapping"))
				{
					var entitySetName = mappingElement.Attribute("EntitySet");
					var tableName = mappingElement.Attribute("Table");
					if (tableName != null && entitySetName != null)
					{
						mappings.Add(tableName.Value, entitySetName.Value);
					}
				}
			}
			return mappings;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "it maybe column names")]
		public IEnumerable<string> ParseExcludedProperties(XElement definitionElement)
		{
			var properties = new List<string>();
			var propertyElements = definitionElement.Element("ExcludedProperties");
			if (propertyElements != null)
			{
				foreach (var propertyElement in propertyElements.Elements("Property"))
				{
					properties.Add(propertyElement.Value);
				}
			}
			return properties;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "look like column name")]
		public IEnumerable<CodeMapping> ParseDefinitionElement(XElement definitionElement)
		{
			var mappingElements = definitionElement.Element("EDICodeMappings");
			var mappings = new List<CodeMapping>();
			foreach (var mappingElement in mappingElements.Elements("Mapping"))
			{
				var mapping = new CodeMapping();
				mapping.TableName = mappingElement.Attribute("Table").Value;
				mapping.Relationship = mappingElement.Attribute("Relationship").Value;
				mapping.PropertyName = mappingElement.Attribute("PropertyName").Value;
				mappings.Add(mapping);
			}
			return mappings;
		}
	}
}
