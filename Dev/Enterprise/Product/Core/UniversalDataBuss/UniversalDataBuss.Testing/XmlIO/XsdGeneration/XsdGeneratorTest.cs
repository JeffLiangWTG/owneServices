using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.TestDataObjects;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XsdGeneration.Testing
{
	class XsdGeneratorTest : TestCaseWithFactory
	{
		public void TestGenerateUberCommonSchemaWorks_2011_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				AssertEquals("Precondition: Got the right namespace for this test", UniversalXmlInfo.Namespace_2011_11, SchemaVersionManager.Current.Namespace);
				var generator = new UniversalXsdGenerator();
				AssertMultilineASCIIEquals("Checking XSD Content", ExpectedCommonXsdFromUberDataObjects_2011_11.Trim(), generator.GetXsdOutput(typeof(UberShipment).Assembly));
			}
		}

		public void TestGenerateUberCommonSchemaWorks_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				AssertEquals("Precondition: Got the right namespace for this test", UniversalXmlInfo.Namespace_2012_11, SchemaVersionManager.Current.Namespace);
				var generator = new UniversalXsdGenerator();
				AssertMultilineASCIIEquals("Checking XSD Content", ExpectedCommonXsdFromUberDataObjects_2012_11.Trim(), generator.GetXsdOutput(typeof(UberShipment).Assembly));
			}
		}

		public void TestGenerateUberShipmentSchemaWorks_2011_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				AssertEquals("Precondition: Got the right namespace for this test", UniversalXmlInfo.Namespace_2011_11, SchemaVersionManager.Current.Namespace);
				var generator = ObjectFactory.Get<IUniversalXsdGenerator>();
				AssertMultilineASCIIEquals("Checking XSD Content", ExpectedXsdFromUberDataObjects_2011_11.Trim(), generator.GetXsdOutput(typeof(UberShipment)));
			}
		}

		public void TestGenerateUberShipmentSchemaWorks_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				AssertEquals("Precondition: Got the right namespace for this test", UniversalXmlInfo.Namespace_2012_11, SchemaVersionManager.Current.Namespace);
				var generator = ObjectFactory.Get<IUniversalXsdGenerator>();
				AssertMultilineASCIIEquals("Checking XSD Content", ExpectedXsdFromUberDataObjects_2012_11.Trim(), generator.GetXsdOutput(typeof(UberShipment)));
			}
		}

		public void TestGenerateWithTypeInternallyDefinedMultipleTimesFails()
		{
			AssertExceptionThrown(typeof(XmlProcessingException)
				, "Error processing Type [BogusDataObject_InternalType] - XsdSchemaAttribute should define this Type as 'Outer', not 'Inner' as it is used multiple times in this Schema."
				, delegate { new UniversalXsdGenerator().TestUnitTestBasedCheckingCodeForType(typeof(BogusDataObject_TypeInternallyDefinedMultipleTimes)); });
		}

		public void TestGenerateWithBadlyNamedCollectionFails()
		{
			AssertExceptionThrown(typeof(XmlProcessingException)
				, "Error processing Property [BogusDataObject_BadlyNamedCollection.Bogusses] - All List<DataObject> typed property names must end in 'Collection'."
				, delegate { new UniversalXsdGenerator().TestUnitTestBasedCheckingCodeForType(typeof(BogusDataObject_BadlyNamedCollection)); });
		}

		public void TestGenerateWithZStringWithNoMaxLengthAttributeFails()
		{
			AssertExceptionThrown(typeof(XmlProcessingException)
				, "Error processing Property [BogusDataObject_ZStringWithNoMaxLengthAttribute.Bill] - All ZString properties on DataObjects must have the MaxLengthAttribute applied."
				, delegate { new UniversalXsdGenerator().TestUnitTestBasedCheckingCodeForType(typeof(BogusDataObject_ZStringWithNoMaxLengthAttribute)); });
		}

		public void TestGenerateWithNoXsdSchemaAttributeFails()
		{
			AssertExceptionThrown(typeof(XmlProcessingException)
				, "Error processing Type [BogusDataObject_NoXsdSchemaAttribute] - All DataObjects must have the XsdSchemaAttribute applied."
				, delegate { new UniversalXsdGenerator().TestUnitTestBasedCheckingCodeForType(typeof(BogusDataObject_NoXsdSchemaAttribute)); });
		}

		#region Implementation

		[XsdSchema(Placement.Outer)]
		class BogusDataObject_TypeInternallyDefinedMultipleTimes : IDataObject
		{
			public List<BogusDataObject_InternalType> BogusCollection { get; set; }
			public BogusDataObject_InternalType Bogus { get; set; }
		}

		[XsdSchema(Placement.Inner)]
		class BogusDataObject_InternalType : IDataObject
		{
			public ZInt? Beefy { get; set; }
		}

		[XsdSchema(Placement.Outer)]
		class BogusDataObject_BadlyNamedCollection : IDataObject
		{
			public List<BogusDataObject_BadlyNamedCollection> Bogusses { get; set; }
		}

		[XsdSchema(Placement.Outer)]
		class BogusDataObject_ZStringWithNoMaxLengthAttribute : IDataObject
		{
			public ZString? Bill { get; set; }
		}

		class BogusDataObject_NoXsdSchemaAttribute : IDataObject
		{
		}

		#endregion

		#region ExpectedXsdFromUberDataObjects_2011_11

		const string ExpectedXsdFromUberDataObjects_2011_11 = @"
<xs:schema targetNamespace=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"" elementFormDefault=""qualified"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:include schemaLocation=""UniversalCommon.xsd"" />

  <xs:element name=""UbiquitousShipment"" type=""UbiquitousShipmentData"" />

  <xs:complexType name=""UbiquitousShipmentData"">
    <xs:all>
      <xs:element name=""UberShipment"" type=""UberShipment""/>
    </xs:all>
    <xs:attribute name=""version"" type=""xs:token"" />
  </xs:complexType>

</xs:schema>
";

		#endregion

		#region ExpectedCommonXsdFromUberDataObjects_2011_11

		const string ExpectedCommonXsdFromUberDataObjects_2011_11 = @"
<xs:schema targetNamespace=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"" elementFormDefault=""qualified"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">

  <xs:complexType name=""UberCandidateKeyObject"">
    <xs:all>
      <xs:element name=""ReferenceData"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""25"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""EnumCandidateKey"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:enumeration value=""Candidate"" />
            <xs:enumeration value=""Key"" />
            <xs:enumeration value=""Type"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""MandatoryUNLOCOCandidateKey"" minOccurs=""1"" type=""UberUNLOCO"" />
      <xs:element name=""BooleanCandidateKey"" minOccurs=""0"" type=""xs:boolean"" />
      <xs:element name=""DateCandidateKey"" minOccurs=""0"" type=""emptiable_dateTime"" />
      <xs:element name=""DecimalCandidateKey"" minOccurs=""0"" type=""xs:decimal"" />
      <xs:element name=""IntCandidateKey"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""LongCandidateKey"" minOccurs=""0"" type=""xs:long"" />
      <xs:element name=""NonMandatoryUNLOCOCandidateKey"" minOccurs=""0"" type=""UberUNLOCO"" />
      <xs:element name=""ShortCandidateKey"" minOccurs=""0"" type=""xs:short"" />
      <xs:element name=""StringCandidateKey"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""2048"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""ZStringCandidateKey"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""1024"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>

      <xs:element name=""CandidateKeyObjectCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""CandidateKeyObject"" minOccurs=""0"" maxOccurs=""unbounded"" type=""UberCandidateKeyObject"" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberChargeCode"">
    <xs:all>
      <xs:element name=""Code"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""10"" />
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

  <xs:complexType name=""UberCodeDescriptionPair"">
    <xs:all>
      <xs:element name=""Code"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""50"" />
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

  <xs:complexType name=""UberCodeDescriptionPairAlwaysAttributes"">
    <xs:simpleContent>
      <xs:extension base=""string_maxLength50"">
        <xs:attribute name=""Description"" type=""string_maxLength50"" />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:complexType name=""UberCompany"">
    <xs:all>
      <xs:element name=""Code"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""3"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Country"" minOccurs=""0"" type=""UberCountry"" />
      <xs:element name=""Name"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""35"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberCountry"">
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

  <xs:complexType name=""UberCurrency"">
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

  <xs:complexType name=""UberDataContext"">
    <xs:all>
      <xs:element name=""Action"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:enumeration value=""Merge"" />
            <xs:enumeration value=""LinkOnly"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""ActionPurpose"" minOccurs=""0"" type=""UberCodeDescriptionPair"" />
      <xs:element name=""CodesMappedToTarget"" minOccurs=""0"" type=""xs:boolean"" />
      <xs:element name=""Company"" minOccurs=""0"" type=""UberCompany"" />
      <xs:element name=""DataProvider"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""50"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""DocumentaryOverride"" minOccurs=""0"" type=""UberDocumentaryOverride"" />
      <xs:element name=""EnterpriseID"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""3"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""EventType"" minOccurs=""0"" type=""UberCodeDescriptionPair"" />
      <xs:element name=""EventUser"" minOccurs=""0"" type=""UberStaff"" />
      <xs:element name=""ServerID"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""3"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Timestamp"" minOccurs=""0"" type=""xs:long"" />
      <xs:element name=""TriggerCount"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""TriggerDate"" minOccurs=""0"" type=""emptiable_dateTime"" />
      <xs:element name=""TriggerDescription"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""50"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""TriggerReference"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""2048"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""TriggerType"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:enumeration value=""Trigger"" />
            <xs:enumeration value=""Milestone"" />
            <xs:enumeration value=""Exception"" />
            <xs:enumeration value=""Manual"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>

      <xs:element name=""DataSourceCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""DataSource"" minOccurs=""0"" maxOccurs=""unbounded"" type=""UberDataSource"" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>

      <xs:element name=""DataTargetCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""DataTarget"" minOccurs=""0"" maxOccurs=""unbounded"" type=""UberDataTarget"" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberDataSource"">
    <xs:all>
      <xs:element name=""Type"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""35"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Key"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""35"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberDataTarget"">
    <xs:all>
      <xs:element name=""Type"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""35"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Key"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""35"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Owner"" minOccurs=""0"" type=""UberOrganization"" />
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberDocumentaryOverride"">
    <xs:all>
      <xs:element name=""DataVersion"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""DocumentName"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""256"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""IsSystemDefined"" minOccurs=""0"" type=""xs:boolean"" />
      <xs:element name=""Purpose"" minOccurs=""0"" type=""UberCodeDescriptionPair"" />
      <xs:element name=""SubmissionVersion"" minOccurs=""0"" type=""xs:int"" />
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberEventType"">
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

  <xs:complexType name=""UberIncoTerm"">
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

  <xs:complexType name=""UberOrder"">
    <xs:all>
      <xs:element name=""Warehouse"" minOccurs=""0"" type=""UberWarehouse"" />
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberOrganization"">
    <xs:all>
      <xs:element name=""CompanyName"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""50"" />
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
      <xs:element name=""Code"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""12"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Country"" minOccurs=""0"" type=""UberCountry"" />
      <xs:element name=""Phone"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""20"" />
          </xs:restriction>
        </xs:simpleType>
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
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberPackageType"">
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

  <xs:complexType name=""UberPackingUnit"">
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

  <xs:complexType name=""UberServiceLevel"">
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

  <xs:complexType name=""UberShipment"">
    <xs:all>
      <xs:element name=""ReferenceData"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""25"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""CandidateKeyField"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""ZZZMandatoryField"" minOccurs=""1"" type=""xs:decimal"" />
      <xs:element name=""ChargeCode"" minOccurs=""0"" type=""UberChargeCode"" />
      <xs:element name=""DataContext"" minOccurs=""0"" type=""UberDataContext"" />
      <xs:element name=""Destination"" minOccurs=""0"" type=""UberUNLOCO"" />
      <xs:element name=""EventType"" minOccurs=""0"" type=""UberEventType"" />
      <xs:element name=""FreightRate"" minOccurs=""0"" type=""xs:decimal"" />
      <xs:element name=""FreightRateCurrency"" minOccurs=""0"" type=""UberCurrency"" />
      <xs:element name=""JobType"" minOccurs=""0"" type=""UberCodeDescriptionPair"" />
      <xs:element name=""Origin"" minOccurs=""0"" type=""UberUNLOCO"" />
      <xs:element name=""OuterPacks"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""OuterPacksPackageType"" minOccurs=""0"" type=""UberPackageType"" />
      <xs:element name=""PackageCount"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""PackageUnit"" minOccurs=""0"" type=""UberPackingUnit"" />
      <xs:element name=""Pancake"" minOccurs=""0"">
        <xs:complexType>
          <xs:all>
            <xs:element name=""Type"" minOccurs=""1"">
              <xs:simpleType>
                <xs:restriction base=""xs:string"">
                  <xs:maxLength value=""35"" />
                </xs:restriction>
              </xs:simpleType>
            </xs:element>
            <xs:element name=""Calories"" minOccurs=""0"" type=""xs:int"" />
            <xs:element name=""CookedIn"" minOccurs=""0"">
              <xs:simpleType>
                <xs:restriction base=""xs:string"">
                  <xs:enumeration value=""FryingPan"" />
                  <xs:enumeration value=""Oven"" />
                  <xs:enumeration value=""Microwave"" />
                  <xs:enumeration value=""Griller"" />
                  <xs:enumeration value=""Toaster"" />
                </xs:restriction>
              </xs:simpleType>
            </xs:element>
            <xs:element name=""Description"" minOccurs=""0"">
              <xs:simpleType>
                <xs:restriction base=""xs:string"">
                  <xs:maxLength value=""80"" />
                </xs:restriction>
              </xs:simpleType>
            </xs:element>
            <xs:element name=""Diameter"" minOccurs=""0"" type=""xs:decimal"" />
            <xs:element name=""RecipeWritten"" minOccurs=""0"" type=""emptiable_dateTime"" />
          </xs:all>
        </xs:complexType>
      </xs:element>
      <xs:element name=""ServiceLevel"" minOccurs=""0"" type=""UberServiceLevel"" />
      <xs:element name=""ShipmentIncoTerm"" minOccurs=""0"" type=""UberIncoTerm"" />
      <xs:element name=""TransportMode"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:enumeration value=""Sea"" />
            <xs:enumeration value=""Air"" />
            <xs:enumeration value=""Road"" />
            <xs:enumeration value=""Rail"" />
            <xs:enumeration value=""Storage"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""UberBigField"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""2048"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""UberBoolean"" minOccurs=""0"" type=""xs:boolean"" />
      <xs:element name=""UberByteField"" minOccurs=""0"" type=""xs:unsignedByte"" />
      <xs:element name=""UberDate"" minOccurs=""0"" type=""emptiable_date"" />
      <xs:element name=""UberDateTime"" minOccurs=""0"" type=""emptiable_dateTime"" />
      <xs:element name=""UberDecimal"" minOccurs=""0"" type=""xs:decimal"" />
      <xs:element name=""UberDuration"" minOccurs=""0"" type=""xs:duration"" />
      <xs:element name=""UberInteger"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""UberLongInt"" minOccurs=""0"" type=""xs:long"" />
      <xs:element name=""UberMediumField"" minOccurs=""0"" type=""UberCodeDescriptionPairAlwaysAttributes"" />
      <xs:element name=""UberShortIntField"" minOccurs=""0"" type=""xs:short"" />
      <xs:element name=""UberSmallField"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""10"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Volume"" minOccurs=""0"" type=""xs:decimal"" />
      <xs:element name=""VolumeUnit"" minOccurs=""0"" type=""UberUnitOfVolume"" />
      <xs:element name=""Weight"" minOccurs=""0"" type=""xs:decimal"" />
      <xs:element name=""WeightUnit"" minOccurs=""0"" type=""UberUnitOfWeight"" />

      <xs:element name=""InnerRelation"" minOccurs=""0"">
        <xs:complexType>
          <xs:all>
            <xs:element name=""ExtraBoolean"" minOccurs=""0"" type=""xs:boolean"" />
            <xs:element name=""ExtraDateTime"" minOccurs=""0"" type=""emptiable_dateTime"" />
            <xs:element name=""ExtraDecimal"" minOccurs=""0"" type=""xs:decimal"" />
            <xs:element name=""ExtraField"" minOccurs=""0"">
              <xs:simpleType>
                <xs:restriction base=""xs:string"">
                  <xs:maxLength value=""50"" />
                </xs:restriction>
              </xs:simpleType>
            </xs:element>
            <xs:element name=""ExtraInteger"" minOccurs=""0"" type=""xs:int"" />
          </xs:all>
        </xs:complexType>
      </xs:element>

      <xs:element name=""Order"" minOccurs=""0"" type=""UberOrder"" />

      <xs:element name=""OrganizationCollection"" minOccurs=""1"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""Organization"" minOccurs=""1"" maxOccurs=""unbounded"" type=""UberOrganization"" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>

      <xs:element name=""ContainerCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""Container"" minOccurs=""0"" maxOccurs=""unbounded"">
              <xs:complexType>
                <xs:all>
                  <xs:element name=""ContainerNumber"" minOccurs=""0"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:maxLength value=""20"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""ContainerType"" minOccurs=""0"">
                    <xs:complexType>
                      <xs:all>
                        <xs:element name=""Code"" minOccurs=""1"">
                          <xs:simpleType>
                            <xs:restriction base=""xs:string"">
                              <xs:maxLength value=""4"" />
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
                        <xs:element name=""ISOCode"" minOccurs=""0"">
                          <xs:simpleType>
                            <xs:restriction base=""xs:string"">
                              <xs:maxLength value=""4"" />
                            </xs:restriction>
                          </xs:simpleType>
                        </xs:element>
                      </xs:all>
                    </xs:complexType>
                  </xs:element>
                </xs:all>
              </xs:complexType>
            </xs:element>
          </xs:sequence>
        </xs:complexType>
      </xs:element>

      <xs:element name=""DateCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""Date"" minOccurs=""0"" maxOccurs=""unbounded"">
              <xs:complexType>
                <xs:all>
                  <xs:element name=""Type"" minOccurs=""1"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:enumeration value=""AvailableExFactory"" />
                        <xs:enumeration value=""Pickup"" />
                        <xs:enumeration value=""Pack"" />
                        <xs:enumeration value=""Departure"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""IsEstimate"" minOccurs=""0"" type=""xs:boolean"" />
                  <xs:element name=""Value"" minOccurs=""1"" type=""emptiable_dateTime"" />
                </xs:all>
              </xs:complexType>
            </xs:element>
          </xs:sequence>
          <xs:attribute name=""Style"" type=""CollectionStyle"" />
        </xs:complexType>
      </xs:element>

      <xs:element name=""HeaderCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""Header"" minOccurs=""0"" maxOccurs=""unbounded"">
              <xs:complexType>
                <xs:all>
                  <xs:element name=""HeaderReference"" minOccurs=""1"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:maxLength value=""10"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""Destination"" minOccurs=""0"" type=""UberUNLOCO"" />
                  <xs:element name=""Exporter"" minOccurs=""0"" type=""UberOrganization"" />
                  <xs:element name=""Importer"" minOccurs=""0"" type=""UberOrganization"" />
                  <xs:element name=""InvoicedDate"" minOccurs=""0"" type=""emptiable_dateTime"" />
                  <xs:element name=""InvoicedValue"" minOccurs=""0"" type=""xs:decimal"" />
                  <xs:element name=""Origin"" minOccurs=""0"" type=""UberUNLOCO"" />

                  <xs:element name=""LineCollection"" minOccurs=""0"">
                    <xs:complexType>
                      <xs:sequence>
                        <xs:element name=""Line"" minOccurs=""0"" maxOccurs=""unbounded"">
                          <xs:complexType>
                            <xs:all>
                              <xs:element name=""LineNumber"" minOccurs=""1"" type=""xs:int"" />
                              <xs:element name=""InvoicedValue"" minOccurs=""0"" type=""xs:decimal"" />
                              <xs:element name=""PackageUnit"" minOccurs=""0"" type=""UberPackingUnit"" />
                              <xs:element name=""VolumeUnit"" minOccurs=""0"" type=""UberUnitOfVolume"" />
                              <xs:element name=""WeightUnit"" minOccurs=""0"" type=""UberUnitOfWeight"" />
                            </xs:all>
                          </xs:complexType>
                        </xs:element>
                      </xs:sequence>
                    </xs:complexType>
                  </xs:element>
                </xs:all>
              </xs:complexType>
            </xs:element>
          </xs:sequence>
        </xs:complexType>
      </xs:element>

      <xs:element name=""OrganizationAddressCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""OrganizationAddress"" minOccurs=""0"" maxOccurs=""unbounded"">
              <xs:complexType>
                <xs:all>
                  <xs:element name=""AddressType"" minOccurs=""0"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:maxLength value=""40"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""CompanyName"" minOccurs=""0"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:maxLength value=""256"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""Contact"" minOccurs=""0"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:maxLength value=""256"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""OrganizationCode"" minOccurs=""0"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:maxLength value=""12"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                </xs:all>
              </xs:complexType>
            </xs:element>
          </xs:sequence>
        </xs:complexType>
      </xs:element>

      <xs:element name=""ShipmentCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""Shipment"" minOccurs=""0"" maxOccurs=""unbounded"" type=""UberShipment"" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>

      <xs:element name=""XmlDateCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""XmlDate"" minOccurs=""0"" maxOccurs=""unbounded"">
              <xs:complexType>
                <xs:all>
                  <xs:element name=""Type"" minOccurs=""1"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:enumeration value=""Arrival"" />
                        <xs:enumeration value=""Departure"" />
                        <xs:enumeration value=""ShippedOnBoard"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""IsEstimate"" minOccurs=""0"" type=""xs:boolean"" />
                  <xs:element name=""Value"" minOccurs=""1"" type=""emptiable_dateTime"" />
                </xs:all>
              </xs:complexType>
            </xs:element>
          </xs:sequence>
          <xs:attribute name=""Style"" type=""CollectionStyle"" />
        </xs:complexType>
      </xs:element>
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberStaff"">
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

  <xs:complexType name=""UberUnitOfVolume"">
    <xs:all>
      <xs:element name=""Code"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""2"" />
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

  <xs:complexType name=""UberUnitOfWeight"">
    <xs:simpleContent>
      <xs:extension base=""string_maxLength2"">
        <xs:attribute name=""Description"" type=""string_maxLength35"" />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:complexType name=""UberUNLOCO"">
    <xs:all>
      <xs:element name=""Code"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""5"" />
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

  <xs:complexType name=""UberWarehouse"">
    <xs:all>
      <xs:element name=""Code"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""3"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Name"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""50"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberWorkflow"">
    <xs:all>
      <xs:element name=""ActionPurpose"" minOccurs=""0"" type=""UberCodeDescriptionPair"" />
      <xs:element name=""Company"" minOccurs=""0"" type=""UberCompany"" />
      <xs:element name=""EventType"" minOccurs=""0"" type=""UberCodeDescriptionPair"" />
      <xs:element name=""EventUser"" minOccurs=""0"" type=""UberStaff"" />
      <xs:element name=""TriggerCount"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""TriggerDate"" minOccurs=""0"" type=""emptiable_dateTime"" />
      <xs:element name=""TriggerDescription"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""50"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""TriggerReference"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""2048"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""TriggerType"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:enumeration value=""Trigger"" />
            <xs:enumeration value=""Milestone"" />
            <xs:enumeration value=""Exception"" />
            <xs:enumeration value=""Manual"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
    </xs:all>
  </xs:complexType>

  <xs:simpleType name=""CollectionStyle"">
    <xs:restriction base=""xs:string"">
      <xs:enumeration value=""Classic"" />
      <xs:enumeration value=""Modern"" />
      <xs:enumeration value=""Retro"" />
    </xs:restriction>
  </xs:simpleType>

  <xs:simpleType name=""CookerType"">
    <xs:restriction base=""xs:string"">
      <xs:enumeration value=""FryingPan"" />
      <xs:enumeration value=""Oven"" />
      <xs:enumeration value=""Microwave"" />
      <xs:enumeration value=""Griller"" />
      <xs:enumeration value=""Toaster"" />
    </xs:restriction>
  </xs:simpleType>

  <xs:simpleType name=""emptiable_date"">
    <xs:union memberTypes=""xs:date empty_string""/>
  </xs:simpleType>

  <xs:simpleType name=""emptiable_dateTime"">
    <xs:union memberTypes=""xs:dateTime empty_string""/>
  </xs:simpleType>

  <xs:simpleType name=""empty_string"">
    <xs:restriction base=""xs:string"">
      <xs:length value=""0""/>
    </xs:restriction>
  </xs:simpleType>

  <xs:simpleType name=""string_maxLength2"">
    <xs:restriction base=""xs:string"">
      <xs:maxLength value=""2"" />
    </xs:restriction>
  </xs:simpleType>

  <xs:simpleType name=""string_maxLength35"">
    <xs:restriction base=""xs:string"">
      <xs:maxLength value=""35"" />
    </xs:restriction>
  </xs:simpleType>

  <xs:simpleType name=""string_maxLength50"">
    <xs:restriction base=""xs:string"">
      <xs:maxLength value=""50"" />
    </xs:restriction>
  </xs:simpleType>

  <xs:simpleType name=""string_maxLength80"">
    <xs:restriction base=""xs:string"">
      <xs:maxLength value=""80"" />
    </xs:restriction>
  </xs:simpleType>

</xs:schema>
";

		#endregion

		#region ExpectedXsdFromUberDataObjects_2012_11

		const string ExpectedXsdFromUberDataObjects_2012_11 = @"
<xs:schema targetNamespace=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"" elementFormDefault=""qualified"" xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:include schemaLocation=""UniversalCommon.xsd"" />

  <xs:element name=""UbiquitousShipment"" type=""UbiquitousShipmentData"" />

  <xs:complexType name=""UbiquitousShipmentData"">
    <xs:all>
      <xs:element name=""UberShipment"" type=""UberShipment""/>
    </xs:all>
    <xs:attribute name=""version"" type=""xs:token"" />
  </xs:complexType>

</xs:schema>
		";

		#endregion

		#region ExpectedCommonXsdFromUberDataObjects_2012_11

		const string ExpectedCommonXsdFromUberDataObjects_2012_11 = @"
<xs:schema targetNamespace=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"" elementFormDefault=""qualified"" xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">

  <xs:complexType name=""UberCandidateKeyObject"">
    <xs:all>
      <xs:element name=""ReferenceData"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""25"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""EnumCandidateKey"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:enumeration value=""Candidate"" />
            <xs:enumeration value=""Key"" />
            <xs:enumeration value=""Type"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""MandatoryUNLOCOCandidateKey"" minOccurs=""1"" type=""UberUNLOCO"" />
      <xs:element name=""BooleanCandidateKey"" minOccurs=""0"" type=""xs:boolean"" />
      <xs:element name=""DateCandidateKey"" minOccurs=""0"" type=""emptiable_dateTime"" />
      <xs:element name=""DecimalCandidateKey"" minOccurs=""0"" type=""xs:decimal"" />
      <xs:element name=""IntCandidateKey"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""LongCandidateKey"" minOccurs=""0"" type=""xs:long"" />
      <xs:element name=""NonMandatoryUNLOCOCandidateKey"" minOccurs=""0"" type=""UberUNLOCO"" />
      <xs:element name=""ShortCandidateKey"" minOccurs=""0"" type=""xs:short"" />
      <xs:element name=""StringCandidateKey"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""2048"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""ZStringCandidateKey"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""1024"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>

      <xs:element name=""CandidateKeyObjectCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""CandidateKeyObject"" minOccurs=""0"" maxOccurs=""unbounded"" type=""UberCandidateKeyObject"" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberChargeCode"">
    <xs:all>
      <xs:element name=""Code"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""10"" />
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

  <xs:complexType name=""UberCodeDescriptionPair"">
    <xs:simpleContent>
      <xs:extension base=""string_maxLength50"">
        <xs:attribute name=""Description"" type=""string_maxLength35"" />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:complexType name=""UberCodeDescriptionPairAlwaysAttributes"">
    <xs:simpleContent>
      <xs:extension base=""string_maxLength50"">
        <xs:attribute name=""Description"" type=""string_maxLength50"" />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:complexType name=""UberCompany"">
    <xs:all>
      <xs:element name=""Code"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""3"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Country"" minOccurs=""0"" type=""UberCountry"" />
      <xs:element name=""Name"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""35"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberCountry"">
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

  <xs:complexType name=""UberCurrency"">
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

  <xs:complexType name=""UberDataContext"">
    <xs:all>
      <xs:element name=""Action"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:enumeration value=""Merge"" />
            <xs:enumeration value=""LinkOnly"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""DataSource"" minOccurs=""0"" type=""UberDataSource"" />
      <xs:element name=""DocumentaryOverride"" minOccurs=""0"" type=""UberDocumentaryOverride"" />
      <xs:element name=""Timestamp"" minOccurs=""0"" type=""xs:long"" />
      <xs:element name=""Workflow"" minOccurs=""0"" type=""UberWorkflow"" />

      <xs:element name=""DataTargetCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""DataTarget"" minOccurs=""0"" maxOccurs=""unbounded"" type=""UberDataTarget"" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberDataProvider"">
    <xs:simpleContent>
      <xs:extension base=""string_maxLength50"">
        <xs:attribute name=""Type"" type=""string_maxLength50"" />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:complexType name=""UberDataSource"">
    <xs:all>
      <xs:element name=""DataProvider"" minOccurs=""0"" type=""UberDataProvider"" />
      <xs:element name=""Key"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""20"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Type"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:enumeration value=""HireOnSight"" />
            <xs:enumeration value=""Good"" />
            <xs:enumeration value=""Bad"" />
            <xs:enumeration value=""DoNotEmploy"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberDataTarget"">
    <xs:all>
      <xs:element name=""Key"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""20"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Owner"" minOccurs=""0"" type=""UberOrganization"" />
      <xs:element name=""Type"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:enumeration value=""HireOnSight"" />
            <xs:enumeration value=""Good"" />
            <xs:enumeration value=""Bad"" />
            <xs:enumeration value=""DoNotEmploy"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberDocumentaryOverride"">
    <xs:all>
      <xs:element name=""DataVersion"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""DocumentName"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""256"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""IsSystemDefined"" minOccurs=""0"" type=""xs:boolean"" />
      <xs:element name=""Purpose"" minOccurs=""0"" type=""UberCodeDescriptionPair"" />
      <xs:element name=""SubmissionVersion"" minOccurs=""0"" type=""xs:int"" />
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberEventType"">
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

  <xs:complexType name=""UberIncoTerm"">
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

  <xs:complexType name=""UberOrder"">
    <xs:all>
      <xs:element name=""Warehouse"" minOccurs=""0"" type=""UberWarehouse"" />
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberOrganization"">
    <xs:all>
      <xs:element name=""CompanyName"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""50"" />
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
      <xs:element name=""Code"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""12"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Country"" minOccurs=""0"" type=""UberCountry"" />
      <xs:element name=""Phone"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""20"" />
          </xs:restriction>
        </xs:simpleType>
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
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberPackageType"">
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

  <xs:complexType name=""UberPackingUnit"">
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

  <xs:complexType name=""UberServiceLevel"">
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

  <xs:complexType name=""UberShipment"">
    <xs:all>
      <xs:element name=""ReferenceData"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""25"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""CandidateKeyField"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""ZZZMandatoryField"" minOccurs=""1"" type=""xs:decimal"" />
      <xs:element name=""ChargeCode"" minOccurs=""0"" type=""UberChargeCode"" />
      <xs:element name=""DataContext"" minOccurs=""0"" type=""UberDataContext"" />
      <xs:element name=""Destination"" minOccurs=""0"" type=""UberUNLOCO"" />
      <xs:element name=""EventType"" minOccurs=""0"" type=""UberEventType"" />
      <xs:element name=""FreightRate"" minOccurs=""0"" type=""xs:decimal"" />
      <xs:element name=""FreightRateCurrency"" minOccurs=""0"" type=""UberCurrency"" />
      <xs:element name=""JobType"" minOccurs=""0"" type=""UberCodeDescriptionPair"" />
      <xs:element name=""Origin"" minOccurs=""0"" type=""UberUNLOCO"" />
      <xs:element name=""OuterPacks"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""OuterPacksPackageType"" minOccurs=""0"" type=""UberPackageType"" />
      <xs:element name=""PackageCount"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""PackageUnit"" minOccurs=""0"" type=""UberPackingUnit"" />
      <xs:element name=""Pancake"" minOccurs=""0"">
        <xs:complexType>
          <xs:simpleContent>
            <xs:extension base=""string_maxLength35"">
              <xs:attribute name=""Calories"" type=""xs:int"" />
              <xs:attribute name=""CookedIn"" type=""CookerType"" />
              <xs:attribute name=""Description"" type=""string_maxLength80"" />
              <xs:attribute name=""Diameter"" type=""xs:decimal"" />
              <xs:attribute name=""RecipeWritten"" type=""emptiable_dateTime"" />
            </xs:extension>
          </xs:simpleContent>
        </xs:complexType>
      </xs:element>
      <xs:element name=""ServiceLevel"" minOccurs=""0"" type=""UberServiceLevel"" />
      <xs:element name=""ShipmentIncoTerm"" minOccurs=""0"" type=""UberIncoTerm"" />
      <xs:element name=""TransportMode"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:enumeration value=""Sea"" />
            <xs:enumeration value=""Air"" />
            <xs:enumeration value=""Road"" />
            <xs:enumeration value=""Rail"" />
            <xs:enumeration value=""Storage"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""UberBigField"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""2048"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""UberBoolean"" minOccurs=""0"" type=""xs:boolean"" />
      <xs:element name=""UberByteField"" minOccurs=""0"" type=""xs:unsignedByte"" />
      <xs:element name=""UberDate"" minOccurs=""0"" type=""emptiable_date"" />
      <xs:element name=""UberDateTime"" minOccurs=""0"" type=""emptiable_dateTime"" />
      <xs:element name=""UberDecimal"" minOccurs=""0"" type=""xs:decimal"" />
      <xs:element name=""UberDuration"" minOccurs=""0"" type=""xs:duration"" />
      <xs:element name=""UberInteger"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""UberLongInt"" minOccurs=""0"" type=""xs:long"" />
      <xs:element name=""UberMediumField"" minOccurs=""0"" type=""UberCodeDescriptionPairAlwaysAttributes"" />
      <xs:element name=""UberShortIntField"" minOccurs=""0"" type=""xs:short"" />
      <xs:element name=""UberSmallField"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""10"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Volume"" minOccurs=""0"" type=""xs:decimal"" />
      <xs:element name=""VolumeUnit"" minOccurs=""0"" type=""UberUnitOfVolume"" />
      <xs:element name=""Weight"" minOccurs=""0"" type=""xs:decimal"" />
      <xs:element name=""WeightUnit"" minOccurs=""0"" type=""UberUnitOfWeight"" />

      <xs:element name=""InnerRelation"" minOccurs=""0"">
        <xs:complexType>
          <xs:all>
            <xs:element name=""ExtraBoolean"" minOccurs=""0"" type=""xs:boolean"" />
            <xs:element name=""ExtraDateTime"" minOccurs=""0"" type=""emptiable_dateTime"" />
            <xs:element name=""ExtraDecimal"" minOccurs=""0"" type=""xs:decimal"" />
            <xs:element name=""ExtraField"" minOccurs=""0"">
              <xs:simpleType>
                <xs:restriction base=""xs:string"">
                  <xs:maxLength value=""50"" />
                </xs:restriction>
              </xs:simpleType>
            </xs:element>
            <xs:element name=""ExtraInteger"" minOccurs=""0"" type=""xs:int"" />
          </xs:all>
        </xs:complexType>
      </xs:element>

      <xs:element name=""Order"" minOccurs=""0"" type=""UberOrder"" />

      <xs:element name=""OrganizationCollection"" minOccurs=""1"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""Organization"" minOccurs=""1"" maxOccurs=""unbounded"" type=""UberOrganization"" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>

      <xs:element name=""ContainerCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""Container"" minOccurs=""0"" maxOccurs=""unbounded"">
              <xs:complexType>
                <xs:all>
                  <xs:element name=""ContainerNumber"" minOccurs=""0"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:maxLength value=""20"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""ContainerType"" minOccurs=""0"">
                    <xs:complexType>
                      <xs:all>
                        <xs:element name=""Code"" minOccurs=""1"">
                          <xs:simpleType>
                            <xs:restriction base=""xs:string"">
                              <xs:maxLength value=""4"" />
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
                        <xs:element name=""ISOCode"" minOccurs=""0"">
                          <xs:simpleType>
                            <xs:restriction base=""xs:string"">
                              <xs:maxLength value=""4"" />
                            </xs:restriction>
                          </xs:simpleType>
                        </xs:element>
                      </xs:all>
                    </xs:complexType>
                  </xs:element>
                </xs:all>
              </xs:complexType>
            </xs:element>
          </xs:sequence>
        </xs:complexType>
      </xs:element>

      <xs:element name=""DateCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""Date"" minOccurs=""0"" maxOccurs=""unbounded"">
              <xs:complexType>
                <xs:all>
                  <xs:element name=""Type"" minOccurs=""1"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:enumeration value=""AvailableExFactory"" />
                        <xs:enumeration value=""Pickup"" />
                        <xs:enumeration value=""Pack"" />
                        <xs:enumeration value=""Departure"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""IsEstimate"" minOccurs=""0"" type=""xs:boolean"" />
                  <xs:element name=""Value"" minOccurs=""1"" type=""emptiable_dateTime"" />
                </xs:all>
              </xs:complexType>
            </xs:element>
          </xs:sequence>
          <xs:attribute name=""Style"" type=""CollectionStyle"" />
        </xs:complexType>
      </xs:element>

      <xs:element name=""HeaderCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""Header"" minOccurs=""0"" maxOccurs=""unbounded"">
              <xs:complexType>
                <xs:all>
                  <xs:element name=""HeaderReference"" minOccurs=""1"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:maxLength value=""10"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""Destination"" minOccurs=""0"" type=""UberUNLOCO"" />
                  <xs:element name=""Exporter"" minOccurs=""0"" type=""UberOrganization"" />
                  <xs:element name=""Importer"" minOccurs=""0"" type=""UberOrganization"" />
                  <xs:element name=""InvoicedDate"" minOccurs=""0"" type=""emptiable_dateTime"" />
                  <xs:element name=""InvoicedValue"" minOccurs=""0"" type=""xs:decimal"" />
                  <xs:element name=""Origin"" minOccurs=""0"" type=""UberUNLOCO"" />

                  <xs:element name=""LineCollection"" minOccurs=""0"">
                    <xs:complexType>
                      <xs:sequence>
                        <xs:element name=""Line"" minOccurs=""0"" maxOccurs=""unbounded"">
                          <xs:complexType>
                            <xs:all>
                              <xs:element name=""LineNumber"" minOccurs=""1"" type=""xs:int"" />
                              <xs:element name=""InvoicedValue"" minOccurs=""0"" type=""xs:decimal"" />
                              <xs:element name=""PackageUnit"" minOccurs=""0"" type=""UberPackingUnit"" />
                              <xs:element name=""VolumeUnit"" minOccurs=""0"" type=""UberUnitOfVolume"" />
                              <xs:element name=""WeightUnit"" minOccurs=""0"" type=""UberUnitOfWeight"" />
                            </xs:all>
                          </xs:complexType>
                        </xs:element>
                      </xs:sequence>
                    </xs:complexType>
                  </xs:element>
                </xs:all>
              </xs:complexType>
            </xs:element>
          </xs:sequence>
        </xs:complexType>
      </xs:element>

      <xs:element name=""OrganizationAddressCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""OrganizationAddress"" minOccurs=""0"" maxOccurs=""unbounded"">
              <xs:complexType>
                <xs:all>
                  <xs:element name=""AddressType"" minOccurs=""0"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:maxLength value=""40"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""CompanyName"" minOccurs=""0"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:maxLength value=""256"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""Contact"" minOccurs=""0"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:maxLength value=""256"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""OrganizationCode"" minOccurs=""0"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:maxLength value=""12"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                </xs:all>
              </xs:complexType>
            </xs:element>
          </xs:sequence>
        </xs:complexType>
      </xs:element>

      <xs:element name=""ShipmentCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""Shipment"" minOccurs=""0"" maxOccurs=""unbounded"" type=""UberShipment"" />
          </xs:sequence>
        </xs:complexType>
      </xs:element>

      <xs:element name=""XmlDateCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""XmlDate"" minOccurs=""0"" maxOccurs=""unbounded"">
              <xs:complexType>
                <xs:all>
                  <xs:element name=""Type"" minOccurs=""1"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:enumeration value=""Arrival"" />
                        <xs:enumeration value=""Departure"" />
                        <xs:enumeration value=""ShippedOnBoard"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""IsEstimate"" minOccurs=""0"" type=""xs:boolean"" />
                  <xs:element name=""Value"" minOccurs=""1"" type=""emptiable_dateTime"" />
                </xs:all>
              </xs:complexType>
            </xs:element>
          </xs:sequence>
          <xs:attribute name=""Style"" type=""CollectionStyle"" />
        </xs:complexType>
      </xs:element>
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberStaff"">
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

  <xs:complexType name=""UberUnitOfVolume"">
    <xs:all>
      <xs:element name=""Code"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""2"" />
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

  <xs:complexType name=""UberUnitOfWeight"">
    <xs:simpleContent>
      <xs:extension base=""string_maxLength2"">
        <xs:attribute name=""Description"" type=""string_maxLength35"" />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:complexType name=""UberUNLOCO"">
    <xs:simpleContent>
      <xs:extension base=""string_maxLength5"">
        <xs:attribute name=""Name"" type=""string_maxLength35"" />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:complexType name=""UberWarehouse"">
    <xs:all>
      <xs:element name=""Code"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""3"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Name"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""50"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
    </xs:all>
  </xs:complexType>

  <xs:complexType name=""UberWorkflow"">
    <xs:all>
      <xs:element name=""ActionPurpose"" minOccurs=""0"" type=""UberCodeDescriptionPair"" />
      <xs:element name=""Company"" minOccurs=""0"" type=""UberCompany"" />
      <xs:element name=""EventType"" minOccurs=""0"" type=""UberCodeDescriptionPair"" />
      <xs:element name=""EventUser"" minOccurs=""0"" type=""UberStaff"" />
      <xs:element name=""TriggerCount"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""TriggerDate"" minOccurs=""0"" type=""emptiable_dateTime"" />
      <xs:element name=""TriggerDescription"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""50"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""TriggerReference"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""2048"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""TriggerType"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:enumeration value=""Trigger"" />
            <xs:enumeration value=""Milestone"" />
            <xs:enumeration value=""Exception"" />
            <xs:enumeration value=""Manual"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
    </xs:all>
  </xs:complexType>

  <xs:simpleType name=""CollectionStyle"">
    <xs:restriction base=""xs:string"">
      <xs:enumeration value=""Classic"" />
      <xs:enumeration value=""Modern"" />
      <xs:enumeration value=""Retro"" />
    </xs:restriction>
  </xs:simpleType>

  <xs:simpleType name=""CookerType"">
    <xs:restriction base=""xs:string"">
      <xs:enumeration value=""FryingPan"" />
      <xs:enumeration value=""Oven"" />
      <xs:enumeration value=""Microwave"" />
      <xs:enumeration value=""Griller"" />
      <xs:enumeration value=""Toaster"" />
    </xs:restriction>
  </xs:simpleType>

  <xs:simpleType name=""emptiable_date"">
    <xs:union memberTypes=""xs:date empty_string""/>
  </xs:simpleType>

  <xs:simpleType name=""emptiable_dateTime"">
    <xs:union memberTypes=""xs:dateTime empty_string""/>
  </xs:simpleType>

  <xs:simpleType name=""empty_string"">
    <xs:restriction base=""xs:string"">
      <xs:length value=""0""/>
    </xs:restriction>
  </xs:simpleType>

  <xs:simpleType name=""string_maxLength2"">
    <xs:restriction base=""xs:string"">
      <xs:maxLength value=""2"" />
    </xs:restriction>
  </xs:simpleType>

  <xs:simpleType name=""string_maxLength35"">
    <xs:restriction base=""xs:string"">
      <xs:maxLength value=""35"" />
    </xs:restriction>
  </xs:simpleType>

  <xs:simpleType name=""string_maxLength5"">
    <xs:restriction base=""xs:string"">
      <xs:maxLength value=""5"" />
    </xs:restriction>
  </xs:simpleType>

  <xs:simpleType name=""string_maxLength50"">
    <xs:restriction base=""xs:string"">
      <xs:maxLength value=""50"" />
    </xs:restriction>
  </xs:simpleType>

  <xs:simpleType name=""string_maxLength80"">
    <xs:restriction base=""xs:string"">
      <xs:maxLength value=""80"" />
    </xs:restriction>
  </xs:simpleType>

</xs:schema>
";

		#endregion
	}
}
