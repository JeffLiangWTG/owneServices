using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions.Builders
{
	public class XmlEntityDefinitionBuilder
	{
		internal XmlEntityDefinitionBuilder(XElement entityDefinitionElement)
		{
			this.entityDefinitionElement = entityDefinitionElement;
		}

		readonly XElement entityDefinitionElement;

		public EntityDefinition Construct(EntitySetDefinition info)
		{
			var definition = new EntityDefinition(GetAttribute(TagName.EntityName), GetAttribute(TagName.Name), GetAttribute(TagName.TableSuffix),
				GetAttribute(TagName.Behaviour),
				GetAttribute(TagName.DateRangeStartField),
				GetAttribute(TagName.DateRangeEndField),
				GetAttribute(TagName.OptionalEntityCondition),
				GetValueFor(TagName.HasCustomColumns),
				GetValueFor(TagName.IsUpdateOrInsert),
				GetValueFor(TagName.IsExternal),
				GetValueFor(TagName.IsShowAll),
				GetValueFor(TagName.IncludeParentTableCode),
				GetValueFor(TagName.RequiresAdditionOfActionEqualsMerge),
				GetValuesUnder(TagName.ExcludedProperty),
				GetValuesUnder(TagName.ExcludedFromImportProperty),
				GetValuesUnder(TagName.ExcludedFromExportProperty),
				GetValuesUnder(TagName.IncludedProperty),
				GetValuesUnder(TagName.StripPropertyCRLF),
				GetValuesUnder(TagName.ExcludeFromStripProperty),
				GetValuesUnder(TagName.UniqueCriteria + "/" + TagName.ExcludedProperies, TagName.Property),
				info);

			return definition;
		}

		bool GetValueFor(string tagName, bool defaultValue = false)
		{
			bool result;
			if (!bool.TryParse(GetAttribute(tagName), out result))
			{
				result = defaultValue;
			}
			return result;
		}

		string GetAttribute(string tagName)
		{
			return (string)entityDefinitionElement.Attribute(tagName);
		}

		IEnumerable<string> GetValuesUnder(string tagName)
		{
			return GetValuesUnder(entityDefinitionElement, tagName);
		}

		IEnumerable<string> GetValuesUnder(string xPath, string tagName)
		{
			var element = entityDefinitionElement.XPathSelectElement(xPath);

			if (element == null)
			{
				return Enumerable.Empty<string>();
			}

			return GetValuesUnder(element, tagName);
		}

		IEnumerable<string> GetValuesUnder(XElement element, string tagName)
		{
			return from subElement in element.Elements(tagName)
						 select (string)subElement.Attribute(TagName.Name);
		}
	}
}
