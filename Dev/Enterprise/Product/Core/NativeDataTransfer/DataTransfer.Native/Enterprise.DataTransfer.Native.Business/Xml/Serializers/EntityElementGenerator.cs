using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Business.Xml.Serializers;
using Enterprise.DataTransfer.Native.Business.Xsd;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.CodeMappings;
using Enterprise.DataTransfer.Native.DB;

namespace Enterprise.DataTransfer.Native.Business.Xml
{
	public class EntityElementGenerator : IXmlGenerator<IEntity, XElement>
	{
		public EntityElementGenerator()
		{
			propertyGenerator = new PropertyElementGenerator();
			addInfoPropertyGenerator = new AddInfoPropertyElementGenerator();
			actionGenerator = new ActionAttributeGenerator();
		}
		readonly IXmlGenerator<Property, XElement> propertyGenerator;
		readonly IXmlGenerator<Property, XElement> addInfoPropertyGenerator;
		readonly IXmlGenerator<EntityAction, XAttribute> actionGenerator;

		public XElement Generate(IEntity entity)
		{
			var entityElement = new XElement(entity.EntityName);

			var actionAttribute = actionGenerator.Generate(entity.Action);
			entityElement.Add(actionAttribute);

			var propertyElements = GeneratePropertyElements(entity);
			entityElement.Add(propertyElements);

			return entityElement;
		}

		IEnumerable<XContainer> GeneratePropertyElements(IEntity entity)
		{
			bool isCompact = entity.Definition.EntitySetDefinition.IsCompact;
			foreach (var property in entity.Properties.Where(p => p.Definition.IsForExport))
			{
				var columnDef = isCompact ? property.Definition.ColumnDef : null;
				if (columnDef != null && (columnDef.Type == DB.ColumnType.PrimaryKey || HasDefaultValue(property, columnDef)))
				{
					continue;
				}

				if (property.Value is string)
				{
					property.Value = RemoveInvalidXmlChars((string)property.Value);
				}
				XElement propertyElement = PropertyDefinitionXsdGenerator.PropertyShouldBeWrittenAsFancyAddInfo(property.Definition, entity.Definition.EntitySetDefinition)
											? addInfoPropertyGenerator.Generate(property)
											: propertyGenerator.Generate(property);
				AddRelationshipAttribute(entity, property, propertyElement);

				yield return propertyElement;
			}
		}

		void AddRelationshipAttribute(IEntity entity, Property property, XElement propertyElement)
		{
			var codeMapping = EDICodeMapper.FindEntityCodeMapping(entity, property);

			if (codeMapping != null && !string.IsNullOrEmpty(codeMapping.RelationshipResolver))
			{
				XAttribute attribute = RelationshipResolver.CreateXAttribute(property, codeMapping.RelationshipResolver);

				if (attribute != null)
				{
					propertyElement.Add(attribute);
				}
			}
		}

		static bool HasDefaultValue(Property property, IColumnDef columnDef)
		{
			var defaultValue = columnDef.DefaultValue as string;
			var value = property.Value as string;

			if (value == null)
			{
				return true;
			}

			if (defaultValue == null)
			{
				return false;
			}

			return defaultValue == "('" + value + "')";
		}

		string RemoveInvalidXmlChars(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}

			var length = text.Length;
			StringBuilder stringBuilder = new StringBuilder(length);

			for (int i = 0; i < length; ++i)
			{
				if (XmlConvert.IsXmlChar(text[i]))
				{
					stringBuilder.Append(text[i]);
				}
				else if (i + 1 < length && XmlConvert.IsXmlSurrogatePair(text[i + 1], text[i]))
				{
					stringBuilder.Append(text[i]);
					stringBuilder.Append(text[i + 1]);
					++i;
				}
			}

			return stringBuilder.ToString();
		}
	}
}
