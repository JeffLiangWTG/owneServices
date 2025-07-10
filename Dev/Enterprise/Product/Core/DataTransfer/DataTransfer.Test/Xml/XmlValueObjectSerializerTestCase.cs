using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	public class XmlValueObjectSerializerTestCase : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructor_ArgumentMustBeIValueObject()
		{
			new XmlValueObjectSerializer(typeof(string));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteCollectionToXml()
		{
			var dummy1 = Factory.New<TestImportingBizObj>();

			var adapter = new TestValueObjectDataAdapter();
			var serializer = new XmlValueObjectSerializer(typeof(TestValueObject));
			using (var stream = new MemoryStream())
			{
				var xmlWriter = new XmlTextWriter(stream, Encoding.ASCII);
				xmlWriter.Formatting = Formatting.Indented;
				serializer.WriteCollectionToXml(xmlWriter, adapter, new BusinessObject[] { dummy1, dummy1 }, new ValueObjectExportContext(new NotificationBuffer()));
				xmlWriter.Flush();
				stream.Flush();
				stream.Position = 0;

				var actualOutput = Encoding.UTF8.GetString(stream.ToArray());
				var expectedOutput = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\Xml\Testing\TestElements.xml"));
				AssertMultilineASCIIEquals("Should write multiple business objects properly", expectedOutput, actualOutput);
			}
		}

		#region TestWriteToXml

		public void TestWriteToXml_WithTextWriter()
		{
			var dummy = Factory.New<TestImportingBizObj>();
			var serializer = new XmlValueObjectSerializer(typeof(TestValueObject));
			var notify = new NotificationBuffer();
			var writer = new StringWriter();
			serializer.WriteToXml(writer, new TestValueObjectDataAdapter(), dummy, new ValueObjectExportContext(notify));

			writer.Flush();
			AssertEquals("Should have written xml", true, writer.GetStringBuilder().Length > 0);
		}

		public void TestWriteToXml_WithXmlWriter()
		{
			var dummy = Factory.New<TestImportingBizObj>();
			var serializer = new XmlValueObjectSerializer(typeof(TestValueObject));
			var notify = new NotificationBuffer();
			var writer = new StringWriter();
			var xmlWriter = new XmlTextWriter(writer);
			serializer.WriteToXml(xmlWriter, new TestValueObjectDataAdapter(), dummy, new ValueObjectExportContext(notify));

			xmlWriter.Flush();
			writer.Flush();
			AssertEquals("Should have written xml", true, writer.GetStringBuilder().Length > 0);
		}

		public void TestWriteToXml_WithInvalidValueObjectType()
		{
			var dummy = Factory.New<TestImportingBizObj>();
			var serializer = new XmlValueObjectSerializer(typeof(TestSomeRandomValueObject));
			var notify = new NotificationBuffer();
			var writer = new StringWriter();
			serializer.WriteToXml(writer, new TestValueObjectDataAdapter(), dummy, new ValueObjectExportContext(notify));

			writer.Flush();
			AssertEquals("Should have written xml", true, writer.GetStringBuilder().Length > 0);
		}

		#endregion

		#region TestDeserialiseFromXmlElement

		public void TestDeserialiseFromXmlElement_WithEDINamespace()
		{
			var document = new XmlDocument();
			document.LoadXml("<TestElement xmlns=\"http://www.edi.com.au/EnterpriseService/\"><Value>splaty</Value></TestElement>");
			var element = (XmlElement)document.FirstChild;

			var serializer = new XmlValueObjectSerializer(typeof(TestValueObject));
			var value = (TestValueObject)serializer.DeserialiseFromXmlElement(element);
			AssertEquals("splaty", value.Value);
		}

		#endregion

		#region TestDeserialiseThenSerialise

		public void TestDeserialiseThenSerialise_PreservesWhitespace()
		{
			const string elementValue = @"Line1
Line2

Line5";
			var rawXml = String.Format("<TestElement xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://www.edi.com.au/EnterpriseService/\"><Value>{0}</Value></TestElement>", elementValue);

			var document = new XmlDocument();
			document.LoadXml(rawXml);
			var element = (XmlElement)document.FirstChild;
			var serializer = new XmlValueObjectSerializer(typeof(TestValueObject));
			var deserialisedValue = (TestValueObject)serializer.DeserialiseFromXmlElement(element);
			AssertEquals("Expected preserved internal whitespace when deserialising.", elementValue, deserialisedValue.Value);

			var serialisedValue = serializer.SerialiseToXmlElement(deserialisedValue);
			AssertXMLEquals("Expected preserved internal whitespace when serialising.", rawXml, serialisedValue.OuterXml);
		}

		#endregion

		#region TestReadInterchangeOrCollectionFromXml

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadInterchangeOrCollectionFromXml_WithEDIInterchange()
		{
			var inputXmlContent = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\Xml\Testing\TestElementsWithinInterchange.xml"));
			var adapter = new TestValueObjectDataAdapter();
			adapter.EnableEDIInterchange = true;
			var serializer = new XmlValueObjectSerializer(typeof(TestValueObject));
			var reader = new XmlTextReader(new StringReader(inputXmlContent));

			var collection = new DummyBusinessObjectCollection(Factory);
			var notify = new NotificationBuffer();
			serializer.ReadInterchangeOrCollectionFromXml(reader, adapter, collection, null, notify);
			AssertEquals("Should have read 2 collection elements", 2, collection.Count);
			AssertEquals("There were errors: " + notify.AsString, false, notify.HasErrors);
			AssertNotNull("Expected EDI Interchange", adapter.LastPopulatedEDIInterchange);
			AssertEquals("Expected 2 EDIMessages", 2, adapter.LastPopulatedEDIInterchange.ContainedMessages.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadInterchangeOrCollectionFromXml_WithoutEDIInterchange()
		{
			var inputXmlContent = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\Xml\Testing\TestElementsWithinInterchange.xml"));
			var adapter = new TestValueObjectDataAdapter();
			adapter.EnableEDIInterchange = false;
			var serializer = new XmlValueObjectSerializer(typeof(TestValueObject));
			var reader = new XmlTextReader(new StringReader(inputXmlContent));

			var collection = new DummyBusinessObjectCollection(Factory);
			var notify = new NotificationBuffer();
			serializer.ReadInterchangeOrCollectionFromXml(reader, adapter, collection, null, notify);
			AssertEquals("Should have read 2 collection elements", 2, collection.Count);
			AssertEquals("There were errors: " + notify.AsString, false, notify.HasErrors);
			AssertNull("Expected no EDI Interchange", adapter.LastPopulatedEDIInterchange);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadInterchangeOrCollectionFromXml_ReadCollectionXml()
		{
			AssertReadInterchangeOrCollectionFromXml_ReadCollectionXml(@"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\Xml\Testing\TestElements.xml");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadInterchangeOrCollectionFromXml_ReadCollectionXmlWithNamespaces()
		{
			AssertReadInterchangeOrCollectionFromXml_ReadCollectionXml(@"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\Xml\Testing\TestElementsNS.xml");
		}

		void AssertReadInterchangeOrCollectionFromXml_ReadCollectionXml(string testFile)
		{
			var inputXmlContent = File.ReadAllText(Path.Combine(BaseSourcePath, testFile));
			var adapter = new TestValueObjectDataAdapter();
			var serializer = new XmlValueObjectSerializer(typeof(TestValueObject));
			var reader = new XmlTextReader(new StringReader(inputXmlContent));

			var collection = new DummyBusinessObjectCollection(Factory);
			var notify = new NotificationBuffer();
			serializer.ReadInterchangeOrCollectionFromXml(reader, adapter, collection, null, notify);
			AssertEquals("Should have read 2 collection elements", 2, collection.Count);
			AssertEquals("There were errors: " + notify.AsString, false, notify.HasErrors);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadInterchangeOrCollectionFromXml_ReadCollectionXmlWithoutNamespace()
		{
			var inputXmlContent = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\Xml\Testing\TestElementsWithNoNamespace.xml"));
			var adapter = new TestValueObjectDataAdapter();
			var serializer = new XmlValueObjectSerializer(typeof(TestValueObject));
			var reader = new XmlTextReader(new StringReader(inputXmlContent));

			var collection = new DummyBusinessObjectCollection(Factory);
			var notify = new NotificationBuffer();
			serializer.ReadInterchangeOrCollectionFromXml(reader, adapter, collection, null, notify);
			AssertEquals("Should have read 2 collection elements", 2, collection.Count);
			AssertEquals("There were errors: " + notify.AsString, false, notify.HasErrors);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadInterchangeOrCollectionFromXml_ReadXmlInterchange()
		{
			var inputXmlContent = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\Xml\Testing\TestElementsWithinInterchange.xml"));
			var adapter = new TestValueObjectDataAdapter();
			var serializer = new XmlValueObjectSerializer(typeof(TestValueObject));
			var reader = new XmlTextReader(new StringReader(inputXmlContent));

			var collection = new DummyBusinessObjectCollection(Factory);
			var notify = new NotificationBuffer();
			serializer.ReadInterchangeOrCollectionFromXml(reader, adapter, collection, null, notify);
			AssertEquals("Should have read 2 collection elements", 2, collection.Count);
			AssertEquals("There were errors: " + notify.AsString, false, notify.HasErrors);
		}

		#region TestReadInterchangeOrCollectionFromXml_ValidationWithInvalidInterchangePayload

		public void TestReadInterchangeOrCollectionFromXml_ValidationWithInvalidInterchangePayload()
		{
			AssertReadInterchangeOrCollectionFromXml_ValidationWithInvalidInterchangePayload(
				"<XmlInterchange><Payload><invalid >xml</Payload></XmlInterchange>".Replace("'", "\""),
				"element 'organisations' with namespace name '' was not found");
		}

		public void TestReadInterchangeOrCollectionFromXml_ValidationWithInvalidInterchangePayloadCollectionElement()
		{
			AssertReadInterchangeOrCollectionFromXml_ValidationWithInvalidInterchangePayload(
				"<XmlInterchange><Payload><invalid_root_collection></invalid_root_collection></Payload></XmlInterchange>".Replace("'", "\""),
				"element 'organisations' with namespace name '' was not found");

			SystemDataRegistry.Instance.XmlSchemaValidationStrict.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertReadInterchangeOrCollectionFromXml_ValidationWithInvalidInterchangePayload(
				"<XmlInterchange><Payload><Organisations><Organisation><invalid_node/></Organisation></Organisations></Payload></XmlInterchange>".Replace("'", "\""),
				"invalid child element",
				false);
		}

		void AssertReadInterchangeOrCollectionFromXml_ValidationWithInvalidInterchangePayload(string organisationInterchangeXml, string expectedErrorContainsText, bool expectValidationError = true)
		{
			var adapter = new OrganisationValueObjectDataAdapter();
			var serializer = new XmlValueObjectSerializer(typeof(Organisation));
			var reader = new XmlTextReader(new StringReader(organisationInterchangeXml));
			var collection = new DummyBusinessObjectCollection(Factory);

			var notify = new NotificationBuffer();
			serializer.ReadInterchangeOrCollectionFromXml(reader, adapter, collection, null, notify);
			var xmlNotification = notify.GetEventsByType(ErrorType.XmlSchemaValidation);
			var error = xmlNotification.Length > 0 ? xmlNotification[0].Message : "";
			AssertEquals(
				"The display message of the error should " + (!expectValidationError ? "not " : "") + "contain '" + expectedErrorContainsText + "'",
				expectValidationError, error.ToLower().IndexOf(expectedErrorContainsText) != -1);
		}

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadInterchangeOrCollectionFromXml_WithInvalidCollectionXml()
		{
			// note that this file doesn't conform to the schema on the data adapter
			var inputXmlContent = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\Xml\Legacy\TestFiles\SampleConsols.xml"));
			var adapter = new TestValueObjectDataAdapter();
			var serializer = new XmlValueObjectSerializer(typeof(TestValueObject));

			var reader = new XmlTextReader(new StringReader(inputXmlContent));
			var collection = new DummyBusinessObjectCollection(Factory);
			var notify = new NotificationBuffer();

			serializer.ReadInterchangeOrCollectionFromXml(reader, adapter, collection, null, notify);
			AssertEquals("Should have schema validation errors", true, notify.ContainsNotificationType(ErrorType.XmlSchemaValidation));
		}

		[ExpectNoExceptions]
		public void TestReadInterchangeOrCollectionFromXml_WithValidatingCollectionSchema()
		{
			var adapter = new OrganisationValueObjectDataAdapter();
			var serializer = new XmlValueObjectSerializer(typeof(TestValueObject));
			var reader = new XmlTextReader(new StringReader("<Organisations><Organisation /></Organisations>"));

			var collection = new DummyBusinessObjectCollection(Factory);
			var notify = new NotificationBuffer();
			serializer.ReadInterchangeOrCollectionFromXml(reader, adapter, collection, null, notify);
			AssertEquals(
				"Should have an xml validation error due to an invalid organisation node",
				true, notify.ContainsNotificationType(ErrorType.XmlSchemaValidation));
			AssertMultilineASCIIEquals("Fail to parse xml message", @"Error: Cannot parse XML on element 'Organisation'. Error Message:
There is an error in the XML document.
	<Organisation xmlns='http://www.edi.com.au/EnterpriseService/'> was not expected.", notify.AsString);
		}

		#endregion

		#region TestImportAndExportXmlData

		[ExpectException(typeof(ArgumentException))]
		public void TestImportXmlData_RootElementValidatedToBeConsistentWithDataAdapter()
		{
			var serialiser = new XmlValueObjectSerializer(typeof(SomeOtherTestObject));
			serialiser.ImportXmlData(new MemoryStream(), new TestValueObjectDataAdapter(), null, null, new NotificationBuffer());
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestExportXmlData_RootElementValidatedToBeConsistentWithDataAdapter()
		{
			var serialiser = new XmlValueObjectSerializer(typeof(SomeOtherTestObject));
			serialiser.ExportXmlData(new MemoryStream(), new TestValueObjectDataAdapter(), Array.Empty<BusinessObject>(), new ValueObjectExportContext(new NotificationBuffer()), "", "", "");
		}

		static class SomeOtherTestObject
		{ }

		[ExpectNoExceptions]
		public void TestImportAndExportXmlData()
		{
			var adapter = new OrganisationValueObjectDataAdapter();
			var serialiser = new XmlValueObjectSerializer(typeof(Organisation));
			var orgToImport = Factory.New<OrgHeader>();
			orgToImport.OH_Code = "orgcode";
			orgToImport.OH_FullName = "fullname";

			var notify = new NotificationBuffer();
			using (var stream = new MemoryStream())
			{
				serialiser.ExportXmlData(stream, adapter, new BusinessObject[] { orgToImport }, new ValueObjectExportContext(notify), "", "", "");
				stream.Flush();
				stream.Position = 0;

				var importedOrgs = new OrgHeaderCollection(Factory);
				serialiser.ImportXmlData(stream, adapter, importedOrgs, null, notify);

				AssertEquals("fullname", importedOrgs[0].OH_FullName);
			}
		}

		public void TestExportXmlData_PartyIDs()
		{
			var adapter = new OrganisationValueObjectDataAdapter();
			var serialiser = new XmlValueObjectSerializer(typeof(Organisation));
			var orgToImport = Factory.New<OrgHeader>();

			using (var stream = new MemoryStream())
			{
				var context = new ValueObjectExportContext(new NotificationBuffer());
				serialiser.ExportXmlData(stream, adapter, new BusinessObject[] { orgToImport }, context, "sender", "receiver", "EVT");
				stream.Flush();
				stream.Position = 0;
				using (var reader = new StreamReader(stream))
				{
					var contents = reader.ReadToEnd();
					AssertContains("<SenderCode>sender</SenderCode>", contents);
					AssertContains("<ReceiverCode>receiver</ReceiverCode>", contents);
					AssertContains("<Purpose>EVT</Purpose>", contents);
				}
				AssertEquals("EVT", context.ExportPurpose);
			}
		}

		public void TestImportXmlData_WithZeroLengthInput()
		{
			var notify = new NotificationBuffer();
			var adapter = new OrganisationValueObjectDataAdapter();
			var serialiser = new XmlValueObjectSerializer(typeof(Organisation));
			var importedOrgs = new OrgHeaderCollection(Factory);
			using (var stream = new MemoryStream(Array.Empty<byte>())) // zero length
			{
				serialiser.ImportXmlData(stream, adapter, importedOrgs, null, notify);
			}
			AssertEquals("Should have errors but no exception", true, notify.HasErrors);
		}

		public void TestSingleElementSchemaValidation()
		{
			var organisationXml = @"
<?xml version='1.0' encoding='utf-8'?>
<Organisation EDICode='orgcode' OwnerCode=''>
<OrganisationDetails>
  <Name>imported fullname</Name>
  <Addresses>
	<Address AddressType='ZZZ'>
	  <TelephoneNumbers />
	  <Sequence>1</Sequence>
	</Address>
  </Addresses>
</OrganisationDetails>
</Organisation>
".Replace("'", "\'");
			var adapter = new OrganisationValueObjectDataAdapter();
			var serialiser = new XmlValueObjectSerializer(typeof(Organisation));
			var importedOrgs = new OrgHeaderCollection(Factory);
			var notify = new NotificationBuffer();
			serialiser.ImportXmlData(new MemoryStream(Encoding.UTF8.GetBytes(organisationXml)), adapter, importedOrgs, null, notify);
			AssertEquals(0, importedOrgs.Count);
			var schemaValiationError = @"Error: Cannot parse XML on element 'AddressType'. Error Message:
There is an error in the XML document.
	Instance validation error: 'ZZZ' is not a valid value for OrgAddressAddressType.";
			AssertMultilineASCIIEquals("Schema Validation", schemaValiationError, notify.AsString);
		}

		public void TestImportXmlData_WithPrecedingAndTrailingWhitespaces()
		{
			var organisationXmlInterchange = @"

\t    \t \r
<?xml version='1.0' encoding='utf-8'?>
<XmlInterchange xmlns='http://www.edi.com.au/EnterpriseService/'>
  <Payload>
	<Organisations>
	  <Organisation EDICode='orgcode' OwnerCode=''>
		<OrganisationDetails>
		  <Name>imported fullname</Name>
		  <Addresses>
			<Address AddressType='MAIN'>
			  <TelephoneNumbers />
			  <Sequence>1</Sequence>
			</Address>
		  </Addresses>
		</OrganisationDetails>
	  </Organisation>
	</Organisations>
  </Payload>
</XmlInterchange>

\t    \t \r
".Replace("'", "\'").Replace("\\t", "\t").Replace("\\r", "\r");
			TestImportXmlData(organisationXmlInterchange, new NotificationBuffer());
		}

		[ExpectNoExceptions]
		public void TestImportXmlData_WithAnkh()
		{
			var organisationXmlInterchange = @"
<?xml version='1.0' encoding='utf-8'?>
<XmlInterchange xmlns='http://www.edi.com.au/EnterpriseService/'>
  <Payload>
	<Organisations>
	  <Organisation EDICode='orgcode' OwnerCode=''>
		<OrganisationDetails>
		  <Name>imported fullname&#xC;</Name>
		  <Addresses>
			<Address AddressType='MAIN'>
			  <TelephoneNumbers />
			  <Sequence>1</Sequence>
			</Address>
		  </Addresses>
		</OrganisationDetails>
	  </Organisation>
	</Organisations>
  </Payload>
</XmlInterchange>
".Replace("'", "\'").Replace("\\t", "\t").Replace("\\r", "\r");
			TestImportXmlData(organisationXmlInterchange, new NotificationBuffer());
		}

		public void TestImportXmlData_WithEDINamespace()
		{
			var organisationXmlInterchange = @"
<?xml version='1.0' encoding='utf-8'?>
<XmlInterchange xmlns='http://www.edi.com.au/EnterpriseService/'>
  <Payload>
	<Organisations>
	  <Organisation EDICode='orgcode' OwnerCode=''>
		<OrganisationDetails>
		  <Name>imported fullname</Name>
		  <Addresses>
			<Address AddressType='MAIN'>
			  <TelephoneNumbers />
			  <Sequence>1</Sequence>
			</Address>
		  </Addresses>
		</OrganisationDetails>
	  </Organisation>
	</Organisations>
  </Payload>
</XmlInterchange>
".Replace("'", "\'");
			TestImportXmlData(organisationXmlInterchange, new NotificationBuffer());
		}

		public void TestImportXmlData_WithSomeRandomNamespace()
		{
			var organisationXmlInterchange = @"
<?xml version='1.0' encoding='utf-8'?>
<XmlInterchange xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' Version='1'>
  <Payload>
	<Organisations>
	  <Organisation EDICode='orgcode' OwnerCode=''>
		<OrganisationDetails>
		  <Name>imported fullname</Name>
		  <Addresses>
			<Address AddressType='MAIN'>
			  <TelephoneNumbers />
			  <Sequence>1</Sequence>
			</Address>
		  </Addresses>
		</OrganisationDetails>
	  </Organisation>
	</Organisations>
  </Payload>
</XmlInterchange>
".Replace("'", "\'");
			TestImportXmlData(organisationXmlInterchange, new NotificationBuffer());
		}

		public void TestImportXmlData_WithoutNamespace()
		{
			var organisationXmlInterchange = @"
<?xml version='1.0' encoding='utf-8'?>
<XmlInterchange Version='1'>
  <Payload>
	<Organisations>
	  <Organisation EDICode='orgcode' OwnerCode=''>
		<OrganisationDetails>
		  <Name>imported fullname</Name>
		  <Addresses>
			<Address AddressType='MAIN'>
			  <TelephoneNumbers />
			  <Sequence>1</Sequence>
			</Address>
		  </Addresses>
		</OrganisationDetails>
	  </Organisation>
	</Organisations>
  </Payload>
</XmlInterchange>
".Replace("'", "\'");
			TestImportXmlData(organisationXmlInterchange, new NotificationBuffer());
		}

		void TestImportXmlData(string organisationXmlInterchange, INotifications notifications)
		{
			var adapter = new OrganisationValueObjectDataAdapter();
			var serialiser = new XmlValueObjectSerializer(typeof(Organisation));
			var importedOrgs = new OrgHeaderCollection(Factory);

			serialiser.ImportXmlData(new MemoryStream(Encoding.UTF8.GetBytes(organisationXmlInterchange)), adapter, importedOrgs, null, notifications);
			AssertEquals("OH_FullName from imported xml", "imported fullname", importedOrgs[0].OH_FullName);
		}

		public void TestExportXmlData_DoesntResultInMultipleNamespaceDeclarations()
		{
			var adapter = new OrganisationValueObjectDataAdapter();
			var serialiser = new XmlValueObjectSerializer(typeof(Organisation));
			var organisation = Factory.New<OrgHeader>();

			using (var memoryStream = new MemoryStream())
			{
				serialiser.ExportXmlData(memoryStream, adapter, new BusinessObject[] { organisation }, new ValueObjectExportContext(new NotificationBuffer()), "", "", "");
				memoryStream.Flush();

				var xmlStr = Encoding.UTF8.GetString(memoryStream.ToArray()).TrimWithUnicodeWhitespace();
				AssertEquals("Only 1 namespace declaration specified", 1, CountStrings(xmlStr, "http://www.edi.com.au/EnterpriseService/"));

				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(xmlStr);
				var xmlNodeList = xmlDoc.GetElementsByTagName("XmlInterchange");
				Assert(xmlStr, xmlNodeList != null);
				AssertEquals(xmlStr, 1, xmlNodeList.Count);

				var attributes = xmlNodeList[0].Attributes;
				AssertEquals(xmlStr, 4, attributes.Count);
				AssertEquals(xmlStr, "http://www.w3.org/2001/XMLSchema-instance", attributes["xmlns:xsi"].Value);
				AssertEquals(xmlStr, "http://www.w3.org/2001/XMLSchema", attributes["xmlns:xsd"].Value);
				AssertEquals(xmlStr, "1", attributes["Version"].Value);
				AssertEquals(xmlStr, "http://www.edi.com.au/EnterpriseService/", attributes["xmlns"].Value);

				AssertEquals("Contains payload element", true, xmlStr.Contains("<Payload>"));
				AssertEquals("Contains collection element without additional namespace", true, xmlStr.Contains("<Organisations>"));
			}
		}

		static int CountStrings(string str, string value)
		{
			var result = -1;
			var current = -1;
			do
			{
				current = str.IndexOf(value, current + 1);
				result++;
			}
			while (current != -1);
			return result;
		}

		#endregion

		#region SerialiseToXmlElement

		public void TestSerialiseToXmlElement()
		{
			var organisation = new Organisation();
			organisation.EDICode = "orgcode";
			organisation.OrganisationDetails.Name = "imported fullname";
			var address = organisation.OrganisationDetails.Addresses.AddNew();
			var capability = address.AddressCapabilities.AddNew();
			capability.AddressType = AddressCapabilityAddressType.MAIN;
			address.Sequence = 1;
			address.SequenceSpecified = true;

			var serialiser = new XmlValueObjectSerializer(typeof(Organisation));
			var orgXmlElement = serialiser.SerialiseToXmlElement(organisation);
			var resultValue = (Organisation)serialiser.DeserialiseFromXmlElement(orgXmlElement);
			AssertEquals(organisation.EDICode, resultValue.EDICode);
			AssertEquals(organisation.OrganisationDetails.Name, resultValue.OrganisationDetails.Name);
		}

		#endregion
	}
}
