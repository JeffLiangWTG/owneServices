using System;
using System.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	public class LicenceSegmentTest : TestCase
	{
		public void TestGetStringAttributeValueSafely()
		{
			XmlNode testNode = TestXmlNode;
			AssertEquals("No string value should be matched", String.Empty, LicenceSegment.GetStringAttributeValueSafely(testNode, "HALLABALLOZA"));
			AssertEquals("Expected String Value should match", expectedStringValue, LicenceSegment.GetStringAttributeValueSafely(testNode, "string"));
		}

		public void TestGetGuidAttributeValueSafely()
		{
			XmlNode testNode = TestXmlNode;
			AssertEquals("No Guid value should be matched", Guid.Empty, LicenceSegment.GetGuidAttributeValueSafely(testNode, "HALLABALLOZA"));
			AssertEquals("Expected Guid Value should match", expectedGuidValue, LicenceSegment.GetGuidAttributeValueSafely(testNode, "guid"));
		}

		public void TestGetBoolAttributeValueSafely()
		{
			XmlNode testNode = TestXmlNode;
			AssertEquals("No bool value should be matched", false, LicenceSegment.GetBoolAttributeValueSafely(testNode, "HALLABALLOZA"));
			AssertEquals("Expected bool Value should match", true, LicenceSegment.GetBoolAttributeValueSafely(testNode, "bool"));
		}

		XmlNode TestXmlNode
		{
			get
			{
				XmlDocument testDocument = new XmlDocument();
				XmlNode testNode = testDocument.CreateNode(XmlNodeType.Element, "TestElement", "");

				XmlAttribute stringAttribute = testDocument.CreateAttribute("string");
				stringAttribute.Value = expectedStringValue;
				testNode.Attributes.Append(stringAttribute);

				XmlAttribute guidAttribute = testDocument.CreateAttribute("guid");
				guidAttribute.Value = expectedGuidValue.ToString();
				testNode.Attributes.Append(guidAttribute);

				XmlAttribute boolAttribute = testDocument.CreateAttribute("bool");
				boolAttribute.Value = true.ToString();
				testNode.Attributes.Append(boolAttribute);

				return testNode;
			}
		}

		const string expectedStringValue = "Test String Value";
		readonly Guid expectedGuidValue = Guid.NewGuid();
	}
}
