using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;

namespace Enterprise.DataTransfer.Native.Business.Xml.Deserializers
{
	class EntityXmlDeserializer
	{
		public EntityXmlDeserializer(XElement element, IEntityDefinition definition)
		{
			this.element = element ?? throw new ArgumentNullException(nameof(element));
			this.definition = definition;
		}
		readonly XElement element;
		readonly IEntityDefinition definition;

		public Guid ParseInternalPK()
		{
			if (element.Elements().Any())
			{
				var primaryKeyElementValue = element.Element(TagName.PrimaryKey)?.Value;

				if (Guid.TryParse(primaryKeyElementValue, out var guid))
				{
					return guid;
				}
				if (primaryKeyElementValue == null)
				{
					return Guid.Empty;
				}

				throw new NativeXMLUserVisibleException(message: FormattableString.Invariant($"Error Parsing InternalPK: Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx), but was '{primaryKeyElementValue}'."));
			}

			var elementValue = element.Value;
			if (Guid.TryParse(elementValue, out var pk))
			{
				return pk;
			}

			return Guid.Empty;
		}

		public string ParseAction()
		{
			var action = element.Attribute(TagName.Action);
			return action == null ? string.Empty : action.Value.ToUpper();
		}

		public IEnumerable<Property> ParsePropertyElements()
		{
			var addInfoTypeCode = ZString.Empty;
			var properties = new List<Property>();
			var propertyDefinitions = definition.PropertyDefinitions;

			var childElements = element.Elements();
			if (childElements.Any())
			{
				foreach (var childElement in childElements)
				{
					var propertyName = childElement.Name.LocalName;
					if (propertyName == "SystemLastEditTimeUtc" || propertyName == "SystemCreateTimeUtc")
					{
						continue;
					}
					if (definition.TableName == "CusAddInfo" && propertyName == "Type")
					{
						addInfoTypeCode = childElement.Value;
					}

					if (propertyName == "AddInfoCollection")
					{
						new AddInfoDeserialiser(childElement, properties, definition, addInfoTypeCode).Deserialise();
					}
					else
					{
						var def = propertyDefinitions.Find(propertyName);

						if (def != null)
						{
							var propertyValue = string.Empty;
							var value = childElement.Value; // Value gives the raw string value of the current element. XElement.ToString() converts /n or /r/n to System.NewLine which varies depending on environment settings.

							if (string.IsNullOrEmpty(value) && childElement.FirstNode is XNode containedNode)
							{
								value = containedNode.ToString(SaveOptions.DisableFormatting); // We have to use ToString() here as someone has put unescaped XML tags within an element that should only contain a string.
							}

							if (!string.IsNullOrEmpty(value))
							{
								propertyValue = value;
							}

							properties.Add(CreateProperty(childElement, propertyName, propertyValue));
						}
					}
				}
			}
			else
			{
				Guid pk = Guid.Empty;
				var elementValue = element.Value;
				if (!string.IsNullOrEmpty(elementValue) && Guid.TryParse(elementValue, out pk))
				{
					var property = new Property(definition.PropertyDefinitions[TagName.PrimaryKey]) { Value = elementValue };
					properties.Add(property);
				}
			}

			return properties;
		}

		Property CreateProperty(XElement childElement, string propertyName, string propertyValue)
		{
			var property = new Property(definition.PropertyDefinitions[propertyName]) { Value = propertyValue };
			foreach (var attr in childElement.Attributes())
			{
				property.AddAttributeValue(attr.Name.LocalName, attr.Value);
			}
			return property;
		}
	}
}
