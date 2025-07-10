using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.DataTransfer.Native.Business.Xsd.Type;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DataTransfer.Native.Business.Xsd
{
	public class NativeXsdGenerator : NativeXsd
	{
		public NativeXsdGenerator()
		{
			entityGenerator = new EntityDefinitionXsdGenerator();
		}
		readonly EntityDefinitionXsdGenerator entityGenerator;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "schema elemant")]
		public XElement Generate(IEntitySetDefinition infoProvider)
		{
			var schemaElement = GenerateSchemaElement();

			schemaElement.Add(new XElement(xs + Tag.Include, new XAttribute(Tag.SchemaLocation, "Native.xsd")));

			var rootElement = new XElement(xs + Tag.Element, new XAttribute(Tag.ElementName, infoProvider.Name), new XAttribute(Tag.AttributeType, infoProvider.Name + "Data"));

			schemaElement.Add(rootElement);
			schemaElement.Add(GenerateTopLevelElement(infoProvider));
			schemaElement.Add(GenerateRootEntityXsd(infoProvider));
			AddAdditionalTopLevelElements(schemaElement);

			return schemaElement;
		}

		/// <summary>
		/// Creates additional top level complexTypes which are referenced
		/// by properties inside the Element being generated.
		/// </summary>
		void AddAdditionalTopLevelElements(XElement schemaElement)
		{
			entityGenerator.GetNewTopLevelTypes.OrderBy(t => (string)t.Attribute(Tag.AttributeName) ?? string.Empty).ForEach(t => schemaElement.Add(t));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "schema elemant")]
		internal XElement GenerateSchemaElement()
		{
			var schemaElement = new XElement(xs + Tag.Schema);
			schemaElement.Add(new XAttribute(Tag.TargetNamespace, ReferenceDataXMLForSerialize.Instance.NameSpace));
			schemaElement.Add(new XAttribute(Tag.Version, NativeXmlInfo.Version_2011_11));
			schemaElement.Add(new XAttribute(Tag.ElementFormDefault, "qualified"));
			schemaElement.Add(new XAttribute(Tag.Xmlns, ReferenceDataXMLForSerialize.Instance.NameSpace));
			schemaElement.Add(new XAttribute(XNamespace.Xmlns + "xs", xs.NamespaceName));
			return schemaElement;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "schema elemant")]
		internal XElement GenerateTopLevelElement(IEntitySetDefinition infoProvider)
		{
			var allElement = new XElement(xs + Tag.All);
			allElement.Add(new XElement(xs + Tag.Element, new XAttribute(Tag.ElementName, infoProvider.Root.EntityName), new XAttribute(Tag.AttributeType, "Native" + infoProvider.Name)));

			var topLevelElement = new XElement(xs + Tag.ComplexType, new XAttribute(Tag.AttributeName, infoProvider.Name + "Data"));
			topLevelElement.Add(allElement);
			topLevelElement.Add(new XElement(xs + Tag.Attribute, new XAttribute(Tag.AttributeName, Tag.Version), new XAttribute(Tag.AttributeType, new XsToken().Name)));
			return topLevelElement;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "schema elemant")]
		internal XElement GenerateRootEntityXsd(IEntitySetDefinition infoProvider)
		{
			var rootEntityXsd = GenerateComplexTypeFor(infoProvider.Root);
			rootEntityXsd.Add(new XAttribute(Tag.AttributeName, "Native" + infoProvider.Name));
			return rootEntityXsd;
		}

		XElement GenerateComplexTypeFor(IEntityDefinition entityDefinition)
		{
			var element = entityGenerator.Generate(entityDefinition);
			GenerateAssociations(element, entityDefinition);
			return element;
		}

		#region Serialize Association Elements

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		internal void GenerateAssociations(XElement element, IEntityDefinition entityDef)
		{
			var entityElement = element.Elements(xs + Tag.All).First();
			entityElement.Add(GenerateBelongsToEntityElement(entityDef));
			entityElement.Add(GenerateHasManyEntityElements(entityDef));
			entityElement.Add(GenerateHasOneEntityElement(entityDef));

			var entityDefinition = entityDef as EntityDefinition;
			if (entityDefinition != null && entityDefinition.CandidateKeyProperties.Any())
			{
				var comments = entityDefinition.CandidateKeyProperties.OrderBy(property => property.PropertyName).Select(property =>
				{
					var foreignKey = property.ColumnDef as DB.Keys.ForeignKey;
					return foreignKey == null ? property.PropertyName : foreignKey.ReferenceTable.Name + "." + foreignKey.ReferenceColumnDef.HumanName;
				});

				entityElement.AddFirst(new XComment(" Candidate Key: " + string.Join(" + ", comments) + " "));
			}
		}

		internal IEnumerable<XElement> GenerateBelongsToEntityElement(IEntityDefinition entityDefinition)
		{
			var belongsTo = entityDefinition.BelongsTo().Where(e => !e.Equals(entityDefinition.MainAssociationMate)).OrderBy(e => e.EntityName);
			foreach (var definition in belongsTo)
			{
				yield return GenerateRelativeElement(definition);
			}
		}

		internal IEnumerable<XElement> GenerateHasManyEntityElements(IEntityDefinition entityDefinition)
		{
			var hasMany = entityDefinition.HasMany().Where(e => !e.Equals(entityDefinition.MainAssociationMate)).OrderBy(e => e.EntityName);
			foreach (var definition in hasMany)
			{
				yield return GenerateHasManyElement(definition);
			}
		}

		internal IEnumerable<XElement> GenerateHasOneEntityElement(IEntityDefinition entityDefinition)
		{
			var hasOne = entityDefinition.HasOne().Where(e => !e.Equals(entityDefinition.MainAssociationMate)).OrderBy(e => e.EntityName);
			foreach (var definition in hasOne)
			{
				yield return GenerateRelativeElement(definition);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "schema elemant")]
		internal XElement GenerateHasManyElement(IEntityDefinition entityDefinition)
		{
			var complexType = new XElement(xs + Tag.ComplexType,
					new XElement(xs + Tag.Sequence,
							new XElement(xs + Tag.Element,
									new XAttribute(Tag.ElementName, entityDefinition.EntityName),
									new XAttribute(Tag.MinOccurs, "0"),
									new XAttribute(Tag.MaxOccurs, "unbounded"),
									GenerateComplexTypeFor(entityDefinition))
				));

			var collectionElement = new XElement(xs + Tag.Element,
					new XAttribute(Tag.ElementName, entityDefinition.EntityName + "Collection"),
					new XAttribute(Tag.MinOccurs, "0"),
					complexType
				);

			AddTableNameAttributeIfRequired(entityDefinition, collectionElement, complexType);

			return collectionElement;
		}

		internal XElement GenerateRelativeElement(IEntityDefinition entityDefinition)
		{
			var element = new XElement(xs + Tag.Element,
					new XAttribute(Tag.ElementName, entityDefinition.EntityName),
					new XAttribute(Tag.MinOccurs, "0"));

			var complexType = GenerateComplexTypeFor(entityDefinition);
			AddTableNameAttributeIfRequired(entityDefinition, element, complexType);

			element.Add(complexType);
			return element;
		}

		static void AddTableNameAttributeIfRequired(IEntityDefinition entityDefinition, XElement element, XElement complexType)
		{
			if (entityDefinition.EntityName != entityDefinition.TableName && !element.Attributes(TagName.TableName).Any())
			{
				complexType.Add(new XElement(xs + Tag.Attribute,
												new XAttribute(Tag.AttributeName, TagName.TableName),
												new XAttribute(Tag.AttributeType, new XsString(0).Name)));
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Contains a raw XML Schema")]
		public string GenerateNativeRootSchema()
		{
			#region const string nativeSchemaText

			const string nativeSchemaText = @"<xs:schema targetNamespace=""{0}"" version=""{1}"" elementFormDefault=""qualified"" xmlns=""{0}"" xmlns:uv=""{6}"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:import schemaLocation=""{5}"" namespace=""{6}"" />

  <xs:element name=""{2}"">
    <xs:complexType>
      <xs:sequence>
        <xs:element name=""{3}"">
          <xs:complexType>
            <xs:sequence>
              <xs:element name=""OwnerCode"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""EnableCodeMapping"" type=""xs:boolean"" minOccurs=""0"" />
              <xs:element name=""DataContext"" type=""uv:DataContext"" minOccurs=""0"" />
              <xs:element name=""MessageNumberCollection"" minOccurs=""0"">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name=""MessageNumber"" minOccurs=""0"" maxOccurs=""unbounded"" type=""uv:MessageNumber"" />
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:sequence>
          </xs:complexType>
        </xs:element>
        <xs:element name=""{4}"">
          <xs:complexType>
            <xs:sequence>
              <xs:any minOccurs=""1"" maxOccurs=""unbounded"" processContents=""skip""/>
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:sequence>
      <xs:attribute name=""version"" type=""xs:token"" />
    </xs:complexType>
  </xs:element>

  <xs:simpleType name=""Action"">
    <xs:restriction base=""xs:string"">
      <xs:enumeration value=""INSERT"" />
      <xs:enumeration value=""UPDATE"" />
      <xs:enumeration value=""MERGE"" />
      <xs:enumeration value=""DELETE"" />
    </xs:restriction>
  </xs:simpleType>

<!-- Organisation address, simplified-->

  <xs:complexType name=""NativeOrganizationAddress"">
    <xs:all>
      <xs:element name=""OrganizationCode"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""12"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Address1"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""50"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Address2"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""50"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""City"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""25"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""CompanyName"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""100"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""AddressShortCode"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""25"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Country"" minOccurs=""0"">
		<xs:complexType>
            <xs:all>
			 <xs:element name=""Code"">
				<xs:simpleType>
					<xs:restriction base=""xs:string"">
						<xs:maxLength value=""2"" />
					</xs:restriction>
				</xs:simpleType>
			  </xs:element>
              <xs:element type=""xs:string"" name=""Name""  minOccurs=""0""/>
            </xs:all>
          </xs:complexType>
	  </xs:element>
      <xs:element name=""Email"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""60"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Fax"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""20"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
     <xs:element name=""Phone"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""20"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Port"" minOccurs=""0"">
		<xs:complexType>
            <xs:all>
               <xs:element name=""Code"">
				 <xs:simpleType>
					<xs:restriction base=""xs:string"">
					<xs:maxLength value=""5"" />
					</xs:restriction>
				 </xs:simpleType>
				</xs:element>
              <xs:element type=""xs:string"" name=""Name""  minOccurs=""0""/>
            </xs:all>
          </xs:complexType>
	 </xs:element>
      <xs:element name=""Postcode"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""10"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""State"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""25"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""AddressType"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""40"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>

	    <xs:element name=""RegistrationNumberCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""RegistrationNumber"" minOccurs=""0"" maxOccurs=""unbounded"">
				<xs:complexType>
				 <xs:all>
				  <xs:element name=""Type"" minOccurs=""1"">
				  <xs:complexType>
					<xs:all>
					  <xs:element name=""Code"" minOccurs=""1"">
						<xs:simpleType>
						  <xs:restriction base=""xs:string"">
							<xs:maxLength value=""3"" />
						  </xs:restriction>
						</xs:simpleType>
					  </xs:element>
					  <xs:element name=""Description"" minOccurs=""0"">
						<xs:simpleType>
						  <xs:restriction base=""xs:string"">
							<xs:maxLength value=""35"" />
						  </xs:restriction>
						</xs:simpleType>
					  </xs:element>
					</xs:all>
				  </xs:complexType>
				  </xs:element>
				  <xs:element name=""CountryOfIssue"" minOccurs=""0"">
					  <xs:complexType  >
						<xs:all>
						  <xs:element name=""Code"" minOccurs=""1"">
							<xs:simpleType>
							  <xs:restriction base=""xs:string"">
								<xs:maxLength value=""2"" />
							  </xs:restriction>
							</xs:simpleType>
						  </xs:element>
						  <xs:element name=""Name"" minOccurs=""0"">
							<xs:simpleType>
							  <xs:restriction base=""xs:string"">
								<xs:maxLength value=""35"" />
							  </xs:restriction>
							</xs:simpleType>
						  </xs:element>
						</xs:all>
					  </xs:complexType>
				  </xs:element>
				  <xs:element name=""Value"" minOccurs=""1"">
					<xs:simpleType>
					  <xs:restriction base=""xs:string"">
						<xs:maxLength value=""35"" />
					  </xs:restriction>
					</xs:simpleType>
				  </xs:element>
				</xs:all>
			  </xs:complexType>
			</xs:element>
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:all>
  </xs:complexType>
<!-- end address -->

  <xs:complexType name=""CustomValues"">
    <xs:sequence maxOccurs=""unbounded"">
      <xs:choice>
        <xs:element name=""Boolean"">
          <xs:complexType>
            <xs:simpleContent>
              <xs:extension base=""xs:boolean"">
                <xs:attribute name=""Name"" type=""xs:string"" use=""required""/>
              </xs:extension>
            </xs:simpleContent>
          </xs:complexType>
        </xs:element>
        <xs:element name=""DateTime"">
          <xs:complexType>
            <xs:simpleContent>
              <xs:extension base=""xs:dateTime"">
                <xs:attribute name=""Name"" type=""xs:string"" use=""required""/>
              </xs:extension>
            </xs:simpleContent>
          </xs:complexType>
        </xs:element>
        <xs:element name=""Integer"">
          <xs:complexType>
            <xs:simpleContent>
              <xs:extension base=""xs:integer"">
                <xs:attribute name=""Name"" type=""xs:string"" use=""required""/>
              </xs:extension>
            </xs:simpleContent>
          </xs:complexType>
        </xs:element>
        <xs:element name=""String"">
          <xs:complexType>
            <xs:simpleContent>
              <xs:extension base=""xs:string"">
                <xs:attribute name=""Name"" type=""xs:string"" use=""required""/>
              </xs:extension>
            </xs:simpleContent>
          </xs:complexType>
        </xs:element>
        <xs:element name=""Time"">
          <xs:complexType>
            <xs:simpleContent>
              <xs:extension base=""xs:time"">
                <xs:attribute name=""Name"" type=""xs:string"" use=""required""/>
              </xs:extension>
            </xs:simpleContent>
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:sequence>
  </xs:complexType>
  <xs:simpleType name=""emptiableDateTime"">
	<xs:union memberTypes=""xs:dateTime emptyString""/>
  </xs:simpleType>
  <xs:simpleType name=""emptyString"">
	<xs:restriction base=""xs:string"">
		<xs:length value=""0""/>
	</xs:restriction>
  </xs:simpleType>
  <xs:simpleType name=""emptiableTime"">
	<xs:union memberTypes=""xs:time emptyString""/>
  </xs:simpleType>
</xs:schema>";

			#endregion

			var schemaInfo = SchemaVersionManager.Instance;
			return string.Format(nativeSchemaText
					, schemaInfo.Namespace
					, schemaInfo.Version
					, NativeXmlInfo.RootElementName
					, NativeXmlInfo.HeaderElementName
					, NativeXmlInfo.BodyElementName
					, UniversalXmlInfo.CommonSchemaName
					, schemaInfo.UniversalNamespace
					);
		}

		public string GenerateNativeRequestSchema()
		{
			var schemaInfo = SchemaVersionManager.Instance;

			#region nativeRequestSchemaText

			var nativeSchemaText =
				$@"<xs:schema targetNamespace=""http://www.cargowise.com/Schemas/Native"" version=""{schemaInfo.Version}"" elementFormDefault=""qualified"" xmlns=""http://www.cargowise.com/Schemas/Native"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""Native"">
    <xs:complexType>
      <xs:sequence>
        <xs:element name=""{NativeXmlInfo.BodyElementName}"">
          <xs:complexType>
            <xs:sequence>
              <xs:choice>
                <xs:element name=""AcceptabilityBand"" type=""CriteriaData""/>
                <xs:element name=""Airline"" type=""CriteriaData""/>
                <xs:element name=""BMSystem"" type=""CriteriaData""/>
                <xs:element name=""CommodityCode"" type=""CriteriaData""/>
                <xs:element name=""Company"" type=""CriteriaData""/>
                <xs:element name=""Container"" type=""CriteriaData""/>
                <xs:element name=""Country"" type=""CriteriaData""/>
                <xs:element name=""CurrencyExchangeRate"" type=""CriteriaData""/>
                <xs:element name=""CusStatement"" type=""CriteriaData""/>
                <xs:element name=""DangerousGood"" type=""CriteriaData""/>
                <xs:element name=""Organization"" type=""CriteriaData""/>
                <xs:element name=""Product"" type=""CriteriaData""/>
                <xs:element name=""Rate"" type=""CriteriaData""/>
                <xs:element name=""ServiceLevel"" type=""CriteriaData""/>
                <xs:element name=""Staff"" type=""CriteriaData""/>
                <xs:element name=""Tag"" type=""CriteriaData""/>
                <xs:element name=""TagRule"" type=""CriteriaData""/>
                <xs:element name=""UNLOCO"" type=""CriteriaData""/>
                <xs:element name=""Vessel"" type=""CriteriaData""/>
                <xs:element name=""WorkflowTemplate"" type=""CriteriaData""/>
              </xs:choice>
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:sequence>
    </xs:complexType>
  </xs:element>

  <xs:complexType name=""CriteriaData"">
    <xs:sequence>
      <xs:element name=""CriteriaGroup"" type=""CriteriaGroupType"" maxOccurs=""unbounded"" />
	</xs:sequence>
  </xs:complexType>

  <xs:complexType name=""CriteriaGroupType"">
    <xs:sequence>
      <xs:element type=""CriteriaType"" name=""Criteria"" maxOccurs=""unbounded"" />
    </xs:sequence>
    <xs:attribute type=""TypeEnum"" name=""Type"" use=""required"" />
  </xs:complexType>

  <xs:complexType name=""CriteriaType"">
    <xs:simpleContent>
      <xs:extension base=""xs:string"">
        <xs:attribute type=""xs:string"" name=""Entity"" use=""required"" />
        <xs:attribute type=""xs:string"" name=""FieldName"" use=""required"" />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:simpleType name=""TypeEnum"">
    <xs:restriction base=""xs:string"">
      <xs:enumeration value=""Key"" />
      <xs:enumeration value=""Partial"" />
    </xs:restriction>
  </xs:simpleType>
</xs:schema>";

			#endregion

			return nativeSchemaText;
		}
	}
}
