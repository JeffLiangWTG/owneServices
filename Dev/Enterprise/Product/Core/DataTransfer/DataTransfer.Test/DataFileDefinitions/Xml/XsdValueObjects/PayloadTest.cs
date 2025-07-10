using System;
using System.IO;
using System.Xml;
using Enterprise.DataTransfer.DataAdapters.Testing;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Payload))]
	sealed class PayloadTest : ValueObjectTestCase
	{
		public void TestSetBusinessObjectsSetsIsSpecified()
		{
			Payload payload = new Payload { IsSpecified = false };
			Assert("Initial state", !payload.IsSpecified);

			payload.Data = Array.Empty<object>();
			Assert("Is specified after data is set", payload.IsSpecified);
		}

		public void TestNoSerializationWithoutAdapter()
		{
			Payload payload =
				new Payload
				{
					Data = new object[] { Factory.NewWithValidTestData<TestImportingBizObj>() }
				};

			using (StringWriter sw = new StringWriter())
			{
				try
				{
					new XmlValueObjectSerializer(typeof(Payload)).Serialize(sw, payload);
					Fail("ArgumentNullException for DataAdapter should have been thrown here.");
				}
				catch (XmlException ex)
				{
					AssertEquals(typeof(ArgumentNullException), ex.InnerException.InnerException.GetType());
					AssertEquals("DataAdapter", ((ArgumentNullException)ex.InnerException.InnerException).ParamName);
				}
			}
		}

		public void TestSerialize()
		{
			const string expectedXml =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<Payload xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TestElements>
    <TestElement>
      <Value>splaty</Value>
    </TestElement>
  </TestElements>
</Payload>";

			Payload payload =
				new Payload
				{
					Data = new object[] { Factory.NewWithValidTestData<TestImportingBizObj>() },
					DataAdapter = new TestValueObjectDataAdapter()
				};

			using (StringWriter sw = new StringWriter())
			{
				new XmlValueObjectSerializer(typeof(Payload)).Serialize(sw, payload);
				AssertEquals(expectedXml, sw.ToString());
			}
		}

		public void TestSerializeWithValueObjectSerializer()
		{
			const string expectedXml =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<Payload xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <TestElement xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
    <Value>splaty</Value>
  </TestElement>
</Payload>";

			Payload payload =
				new Payload
				{
					Data = new TestValueObject { Value = "splaty" },
					ValueObjectSerializer = new XmlValueObjectSerializer(typeof(TestValueObject))
				};

			using (StringWriter sw = new StringWriter())
			{
				new XmlValueObjectSerializer(typeof(Payload)).Serialize(sw, payload);
				AssertXMLEquals(expectedXml, sw.ToString());
			}
		}

		public void TestDeserialize()
		{
			using (StringReader sr = new StringReader("<Payload />"))
			using (XmlReader xmlReader = XmlReader.Create(sr))
			{
				try
				{
					new XmlValueObjectSerializer(typeof(Payload)).Deserialize(xmlReader);
					Fail("NotImplementedException should have been thrown in Payload.ReadXml().");
				}
				catch (XmlException ex)
				{
					AssertEquals(typeof(NotSupportedException), ex.InnerException.InnerException.GetType());
				}
			}
		}

		public void TestGetOuterXml()
		{
			const string expectedXml = "<TestElements><TestElement><Value>splaty</Value></TestElement></TestElements>";

			Payload payload =
				new Payload
				{
					Data = new object[] { Factory.NewWithValidTestData<TestImportingBizObj>() },
					DataAdapter = new TestValueObjectDataAdapter()
				};

			AssertEquals(expectedXml, payload.GetOuterXml());
		}
	}
}
