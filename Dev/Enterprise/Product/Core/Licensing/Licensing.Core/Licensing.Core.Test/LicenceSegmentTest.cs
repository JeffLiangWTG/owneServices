using System;
using System.Xml;
using NUnit.Framework;

namespace Enterprise.Licensing.Testing
{
	abstract class LicenceSegmentTest : TransactionedTestCase
	{
		public abstract void TestAddToLicenceNode();
		public abstract void TestLoadFromLicenceNode();

		public void TestGetStringAttributeValueSafely()
		{
			XmlNode testNode = TestXmlNode;
			AssertEquals("No string value should be matched", String.Empty, testDummy.StringAttributeValue(testNode, "HALLABALLOZA"));
			AssertEquals("Expected String Value should match", expectedStringValue, testDummy.StringAttributeValue(testNode, "string"));
		}

		public void TestGetGuidAttributeValueSafely()
		{
			XmlNode testNode = TestXmlNode;
			AssertEquals("No Guid value should be matched", Guid.Empty, testDummy.GuidAttributeValue(testNode, "HALLABALLOZA"));
			AssertEquals("Expected Guid Value should match", expectedGuidValue, testDummy.GuidAttributeValue(testNode, "guid"));
		}

		public void TestGetBoolAttributeValueSafely()
		{
			XmlNode testNode = TestXmlNode;
			AssertEquals("No bool value should be matched", false, testDummy.BoolAttributeValue(testNode, "HALLABALLOZA"));
			AssertEquals("Expected bool Value should match", true, testDummy.BoolAttributeValue(testNode, "bool"));
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

		readonly LicenceSegmentDummy testDummy = new LicenceSegmentDummy();
	}
}
