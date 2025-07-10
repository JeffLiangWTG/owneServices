using System;
using System.Collections;
using System.IO;
using System.Xml;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(XmlInterchange))]
	sealed class XmlInterchangeTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			XmlInterchange value = null;
			value = new XmlInterchangeCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestEmpty()
		{
			AssertEquals("XmlInterchange.Empty should have IsSpecified=false to indicate it is empty", false, XmlInterchange.Empty.IsSpecified);
		}

		[TestDate(2005, 11, 2)]
		public void TestNewPopulatedInterchange()
		{
			var interchange = XmlInterchange.NewPopulatedInterchange(Factory);
			AssertEquals("Version", "1", interchange.Version);

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var orgProxy = Factory.Load<OrgHeader>(Env.CurrentCompany.OrganisationPK);
			AssertEquals("EDIOrganisation", orgProxy.OH_Code, interchange.InterchangeInfo.EDIOrganisation.EDICode);
			AssertEquals("Source.CompanyCode", Env.CurrentCompany.Code, interchange.InterchangeInfo.Source.CompanyCode);
			AssertEquals("Source.EnterpriseCode", registrationKey.EnterpriseCode, interchange.InterchangeInfo.Source.EnterpriseCode);
			AssertEquals("Source.OriginServer", registrationKey.ServerCode, interchange.InterchangeInfo.Source.OriginServer);
			AssertEquals("Source.LoginName", GlbStaff.CurrentUser.GS_LoginName, interchange.InterchangeInfo.Source.LoginName);
			AssertEquals("Source.LoginUserEmailAddress", GlbStaff.CurrentUser.GS_EmailAddress, interchange.InterchangeInfo.Source.LoginUserEmailAddress);
			AssertEquals("Source.Date", ZDateTime.Now, interchange.InterchangeInfo.Date);
		}

		public void TestNewPopulatedInterchange_XmlType()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var interchange = XmlInterchange.NewPopulatedInterchange(Factory);
			AssertEquals("XmlType", XmlType.Verbose, interchange.InterchangeInfo.XmlType);

			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			interchange = XmlInterchange.NewPopulatedInterchange(Factory);
			AssertEquals("XmlType", XmlType.LightWeight, interchange.InterchangeInfo.XmlType);

			var context = new ValueObjectExportContext(new NotificationBuffer());
			context.SimplifiedXML = true;
			interchange = XmlInterchange.NewPopulatedInterchange(Factory, context);
			AssertEquals("XmlType", XmlType.LightWeight, interchange.InterchangeInfo.XmlType);

			context.SimplifiedXML = false;
			interchange = XmlInterchange.NewPopulatedInterchange(Factory, context);
			AssertEquals("XmlType", XmlType.Verbose, interchange.InterchangeInfo.XmlType);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadInterchangeOnly()
		{
			DoTestReadInterchangeOnly(Path.Combine(BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\IO\TestFiles\SampleForSkippingThePayload.xml", ""));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadInterchangeOnlyWithNamespacePrefix()
		{
			DoTestReadInterchangeOnly(Path.Combine(BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\IO\TestFiles\SampleForSkippingThePayloadWithNamespacePrefix.xml", ""));
		}

		void DoTestReadInterchangeOnly(string pathToFile)
		{
			AssertEquals("Precondition: the file's length should be greater than the reader's buffer capacity", true, new FileInfo(pathToFile).Length > 128 * 1024);
			using (var dataReader = new StreamReader(pathToFile))
			{
				var notify = new NotificationBuffer();
				var readInterchange = XmlInterchange.ReadInterchangeOnly(dataReader, notify);
				AssertNotNull(readInterchange);
				AssertEquals("Make sure we haven't read the Payload for performance reasons", true, dataReader.BaseStream.Position < 10000);
			}
		}

		#region TestDeserializeInterchangeAndPayload

		public void TestDeserializeInterchangeAndPayload_Collection()
		{
			var xml = @"<?xml version='1.0' encoding='utf-8'?>
<XmlInterchange xmlns='http://www.edi.com.au/EnterpriseService/'>
  <InterchangeInfo>
    <EDIOrganisation EDICode='ediorg' />
  </InterchangeInfo>
  <Payload>
    <TestElements>
      <TestElement>
        <Value>aaaa</Value>
      </TestElement>
      <TestElement>
        <Value>bbbb</Value>
      </TestElement>
    </TestElements>
  </Payload>
</XmlInterchange>".Replace("'", "\"");

			using (StringReader sr = new StringReader(xml))
			using (XmlReader reader = XmlReader.Create(sr))
			{
				XmlInterchange interchange;
				object result = XmlInterchange.DeserializeInterchangeAndPayload(reader,
					new XmlValueObjectSerializer(typeof(TestValueObject)), out interchange);

				AssertNotNull(interchange);
				AssertEquals("ediorg", interchange.InterchangeInfo.EDIOrganisation.EDICode);
				AssertNotNull(interchange.Payload);
				AssertNotNull(interchange.Payload.Data);

				IList payloadData = interchange.Payload.Data as IList;
				AssertNotNull(payloadData);
				AssertEquals(2, payloadData.Count);
				AssertEquals("aaaa", ((TestValueObject)payloadData[0]).Value);
				AssertEquals("bbbb", ((TestValueObject)payloadData[1]).Value);

				AssertNotNull(result);
				AssertSame(result, interchange.Payload.Data);
			}
		}

		public void TestDeserializeInterchangeAndPayload_Element()
		{
			var xml = @"<?xml version='1.0' encoding='utf-8'?>
<XmlInterchange xmlns='http://www.edi.com.au/EnterpriseService/'>
  <InterchangeInfo>
    <EDIOrganisation EDICode='ediorg' />
  </InterchangeInfo>
  <Payload>
    <TestElements>
      <TestElement>
        <Value>aaaa</Value>
      </TestElement>
      <TestElement>
        <Value>bbbb</Value>
      </TestElement>
    </TestElements>
  </Payload>
</XmlInterchange>".Replace("'", "\"");

			using (StringReader sr = new StringReader(xml))
			using (XmlReader reader = XmlReader.Create(sr))
			{
				XmlInterchange interchange;
				object result = XmlInterchange.DeserializeInterchangeAndPayload(reader,
					new XmlValueObjectSerializer(typeof(TestValueObject)), out interchange, true);

				AssertNotNull(interchange);
				AssertEquals("ediorg", interchange.InterchangeInfo.EDIOrganisation.EDICode);
				AssertNotNull(interchange.Payload);
				AssertNotNull(interchange.Payload.Data);
				AssertEquals(typeof(TestValueObject), interchange.Payload.Data.GetType());
				AssertEquals("aaaa", ((TestValueObject)interchange.Payload.Data).Value);

				AssertNotNull(result);
				AssertSame(result, interchange.Payload.Data);
			}
		}

		public void TestDeserializeInterchangeAndPayload_RootElement()
		{
			var xml = @"<?xml version='1.0' encoding='utf-8'?>
<XmlInterchange xmlns='http://www.edi.com.au/EnterpriseService/'>
  <InterchangeInfo>
    <EDIOrganisation EDICode='ediorg' />
  </InterchangeInfo>
  <Payload>
    <TestElement>
      <Value>aaaa</Value>
    </TestElement>
  </Payload>
</XmlInterchange>".Replace("'", "\"");

			using (StringReader sr = new StringReader(xml))
			using (XmlReader reader = XmlReader.Create(sr))
			{
				XmlInterchange interchange;
				object result = XmlInterchange.DeserializeInterchangeAndPayload(reader,
					new XmlValueObjectSerializer(typeof(TestValueObject)), out interchange);

				AssertNotNull(interchange);
				AssertEquals("ediorg", interchange.InterchangeInfo.EDIOrganisation.EDICode);
				AssertNotNull(interchange.Payload);
				AssertNotNull(interchange.Payload.Data);
				AssertEquals(typeof(TestValueObject), interchange.Payload.Data.GetType());
				AssertEquals("aaaa", ((TestValueObject)interchange.Payload.Data).Value);

				AssertNotNull(result);
				AssertSame(result, interchange.Payload.Data);
			}
		}

		public void TestSettingsAreNotStatic()
		{
			var myInterchange = XmlInterchange.Empty;
			myInterchange.ImportEDICode = true;
			var yourInterchange = XmlInterchange.Empty;
			AssertEquals(true, myInterchange.ImportEDICode);
			AssertEquals(false, yourInterchange.ImportEDICode);
		}

		#endregion
	}
}
