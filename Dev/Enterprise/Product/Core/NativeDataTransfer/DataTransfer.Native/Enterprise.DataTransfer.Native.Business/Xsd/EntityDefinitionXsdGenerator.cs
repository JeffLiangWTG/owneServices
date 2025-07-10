using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;

namespace Enterprise.DataTransfer.Native.Business.Xsd
{
	/// <summary>
	/// Note: Generally a single instance of this generator is created for all
	/// the properties to be generated. Keep this in mind for any members.
	/// </summary>
	public class EntityDefinitionXsdGenerator : NativeXsd
	{
		public EntityDefinitionXsdGenerator()
		{
			propertyGenerator = new PropertyDefinitionXsdGenerator();
		}
		readonly PropertyDefinitionXsdGenerator propertyGenerator;

		public XElement Generate(IEntityDefinition entityDefinition)
		{
			var complexTypeElement = new XElement(xs + Tag.ComplexType);
			complexTypeElement.Add(GenerateSequenceElement(entityDefinition));
			complexTypeElement.Add(GenerateActionElement());
			return complexTypeElement;
		}

		/// <summary>
		/// Once the Generate method is called, there may be some additional
		/// types created which belong at the top level. These are returned by
		/// this method so that the caller can place them in the top level.
		/// </summary>
		public IEnumerable<XElement> GetNewTopLevelTypes => propertyGenerator.GetNewTopLevelTypes;

		#region Serialize All Element

		internal XElement GenerateSequenceElement(IEntityDefinition entityDefinition)
		{
			var allElement = new XElement(xs + Tag.All);
			allElement.Add(GenerateCustomValuesElements(entityDefinition));
			allElement.Add(GenerateElementForProperties(entityDefinition));
			return allElement;
		}

		internal XElement GenerateCustomValuesElements(IEntityDefinition entityDefinition)
		{
			if (entityDefinition.HasCustomColumns)
			{
				return new XElement(xs + Tag.Element, new XAttribute(Tag.ElementName, Tag.CustomValues), new XAttribute(Tag.ElementType, Tag.CustomValues), new XAttribute(Tag.MinOccurs, 0));
			}
			return null;
		}

		internal IEnumerable<XElement> GenerateElementForProperties(IEntityDefinition entityDefinition)
		{
			var propertyDefinitions = entityDefinition.PropertyDefinitions;
			return propertyDefinitions.OrderBy(propertyDef => propertyDef.PropertyName).Select(propertyDef => propertyGenerator.Generate(propertyDef, entityDefinition.EntitySetDefinition));
		}
		#endregion

		internal XElement GenerateActionElement()
		{
			return new XElement(xs + Tag.Attribute, new XAttribute(Tag.AttributeName, TagName.Action), new XAttribute(Tag.AttributeType, TagName.Action));
		}
	}
}
