using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Business.Xsd.Type;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.Business.Xsd
{
	/// <summary>
	/// Note: Generally a single instance of this generator is created for all
	/// the properties to be generated. Keep this in mind for any members.
	/// </summary>
	public class PropertyDefinitionXsdGenerator : NativeXsd
	{
		public XElement Generate(IPropertyDef propertyDef, IEntitySetDefinition entitySetDefinition)
		{
			if (PropertyShouldBeWrittenAsFancyAddInfo(propertyDef, entitySetDefinition))
			{
				return GetFancyAddInfoSchemaDefinition();
			}
			else
			{
				var propertyElement = new XElement(xs + Tag.Element);
				propertyElement.Add(GeneratePropertyNameAttribute(propertyDef));
				propertyElement.Add(GenerateMinOccurAttribute(propertyDef));
				propertyElement.Add(GeneratePropertyTypeElement(propertyDef, entitySetDefinition));

				return propertyElement;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		XElement GetFancyAddInfoSchemaDefinition()
		{
			// The add info collection is not tightly bound to the schema. It is wholly made-up, as far as Native is concerned. So load its definition from string.
			if (addInfoElementDefinition == null)
			{
				// Need all this instead of just XElement.Parse(addInfoSchema) otherwise the AddInfoCollection node will contain unnecessary xmlns:xs namespace definition
				var nameTable = new NameTable();
				var nameSpaceManager = new XmlNamespaceManager(nameTable);
				nameSpaceManager.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
				var parserContext = new XmlParserContext(null, nameSpaceManager, null, XmlSpace.None);
				var xmlTextReader = new XmlTextReader(addInfoSchema, XmlNodeType.Element, parserContext);
				addInfoElementDefinition = XElement.Load(xmlTextReader);
			}
			return addInfoElementDefinition;
		}

		XElement addInfoElementDefinition;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		internal static bool PropertyShouldBeWrittenAsFancyAddInfo(IPropertyDef propertyDef, IEntitySetDefinition entitySetDefinition)
		{
			return (propertyDef.PropertyName == "AddInfo" || propertyDef.PropertyName == "AddInfoData")
					&& propertyDef.ColumnDef.DataType != "xml"
					&& entitySetDefinition != null
					&& !ExcludedDeprecatedDefinitions.Any(n => n == entitySetDefinition.Name);
		}

		static XAttribute GeneratePropertyNameAttribute(IPropertyDef propertyDef)
		{
			var humanColumnName = propertyDef.PropertyName;
			return new XAttribute(Tag.ElementName, humanColumnName);
		}

		static XAttribute GenerateMinOccurAttribute(IPropertyDef propertyDef)
		{
			var column = propertyDef.ColumnDef;
			if (column.DoesNotRequireAValue)
			{
				return new XAttribute(Tag.MinOccurs, "0");
			}

			return null;
		}

		XObject GeneratePropertyTypeElement(IPropertyDef propertyDef, IEntitySetDefinition entitySetDefinition)
		{
			var type = propertyDef.GetXsdDataType();
			var codeMappings = entitySetDefinition?.CodeMappings[propertyDef.ColumnDef.Table.Name, propertyDef.PropertyName];

			if (codeMappings != null
				&& !codeMappings.RelationshipResolver.IsEmpty()
				&& type is INamedXsdDataType namedType
				&& !topLevelAttributes.Contains(propertyDef.PropertyName))
			{
				// Then we need to add things for the relationship attribute
				// to this property
				var referenceName = $"namedType_{propertyDef.PropertyName}";
				var relationshipAttribute = new XElement(
					xs + Tag.Attribute,
					new XAttribute(Tag.AttributeName, TagName.Relationship),
					new XAttribute(Tag.AttributeType, new XsString(0).Name)
				);

				(var reference, var namedElement) = namedType.ToXsdElementWithAttributes(referenceName, new[] { relationshipAttribute });
				AddForTopLevel(namedElement);
				topLevelAttributes.Add(propertyDef.PropertyName);
				return reference;
			}

			return type.ToXsdElement();
		}

		void AddForTopLevel(XElement namedElement)
		{
			topLevelElements.Add(namedElement);
		}

		public IEnumerable<XElement> GetNewTopLevelTypes => topLevelElements;
		readonly List<XElement> topLevelElements = new List<XElement>();

		readonly HashSet<string> topLevelAttributes = new HashSet<string>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "static readonly string")]
		static readonly ImmutableArray<string> ExcludedDeprecatedDefinitions = ImmutableArray.Create("Declaration", "Order", "Shipment");  // Add top-level definition names here if you want them to keep using the old-skool XX_AddInfo format (string) instead of the new fancy key-value pair version

		#region Add Info node schema element

		/// <summary>
		/// Don't touch this.  Talk to Daniel Clarke
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		const string addInfoSchema = @"
													 <xs:element name='AddInfoCollection'   minOccurs='0'>
														<xs:complexType>
														  <xs:sequence>
															<xs:element name='AddInfo' maxOccurs='unbounded' minOccurs='1'>
															  <xs:complexType>
																<xs:sequence>
																  <xs:element type='xs:string' name='Key'/>
																  <xs:element type='xs:string' name='Value'   minOccurs='0'/>
																  <xs:element name='OrganizationAddress' type='NativeOrganizationAddress' minOccurs='0'/>
																</xs:sequence>
															  </xs:complexType>
															</xs:element>
														  </xs:sequence>
														</xs:complexType>
													  </xs:element>
";
		#endregion
	}
}
