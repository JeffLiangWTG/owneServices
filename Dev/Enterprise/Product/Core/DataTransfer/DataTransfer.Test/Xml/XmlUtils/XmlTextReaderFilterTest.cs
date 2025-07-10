using System;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	public class XmlTextReaderFilterTest : TestCase
	{
		#region Element Filtering

		public void TestElementsNotInSchemaAreFilteredOut()
		{
			AssertXmlRead(Reader, XmlNodeType.Element, "WithMandatoryElementsAndAttributes", false);

			AssertXmlRead(Reader, XmlNodeType.Element, "NestedElement", false);
			AssertXmlRead(Reader, XmlNodeType.Element, "MandatoryElement1", false);
			AssertXmlRead(Reader, XmlNodeType.Text, "", false);
			AssertXmlRead(Reader, XmlNodeType.EndElement, "MandatoryElement1", false);
			AssertXmlRead(Reader, XmlNodeType.Element, "EmptyElement1", true);
			AssertXmlRead(Reader, XmlNodeType.Element, "EmptyElement2", true);
			AssertXmlRead(Reader, XmlNodeType.EndElement, "NestedElement", false);

			AssertXmlRead(Reader, XmlNodeType.Element, "AnArray", false);
			AssertXmlRead(Reader, XmlNodeType.Element, "MandatoryElementInArray", true);
			AssertXmlRead(Reader, XmlNodeType.Element, "MandatoryElementInArray", false);
			AssertXmlRead(Reader, XmlNodeType.Text, "", false);
			AssertXmlRead(Reader, XmlNodeType.EndElement, "MandatoryElementInArray", false);
			AssertXmlRead(Reader, XmlNodeType.EndElement, "AnArray", false);

			AssertXmlRead(Reader, XmlNodeType.EndElement, "WithMandatoryElementsAndAttributes", false);
		}

		#endregion

		#region Attribute Filtering

		public void TestAttributeCount_WithAttributeFilter()
		{
			AssertXmlRead(Reader, XmlNodeType.Element, "WithMandatoryElementsAndAttributes", false);
			AssertEquals("Only 2 attributes should be returned because 2 were invalid", 2, Reader.AttributeCount);
		}

		public void TestHasAttributes()
		{
			AssertXmlRead(Reader, XmlNodeType.Element, "WithMandatoryElementsAndAttributes", false);
			AssertEquals("WithMandatoryElementsAndAttributes several attributes", true, Reader.HasAttributes);
			AssertXmlRead(Reader, XmlNodeType.Element, "NestedElement", false);
			AssertEquals("NestedElement has no attributes", false, Reader.HasAttributes);
		}

		public void TestGetAttribute_WithAttributeFilter()
		{
			AssertXmlRead(Reader, XmlNodeType.Element, "WithMandatoryElementsAndAttributes", false);
			for (int i = 0; i < 3; i++)
			{
				AssertEquals("Reader position should be at the element level now", "WithMandatoryElementsAndAttributes", Reader.LocalName);

				AssertEquals(XmlSchemaDefinitionsBase.EdiXmlNamespace, Reader.GetAttribute(0));
				AssertEquals("MandatoryAttribute1Value", Reader.GetAttribute(1));

				AssertEquals("Reader position should still be at the element level", "WithMandatoryElementsAndAttributes", Reader.LocalName);
			}
		}

		public void TestMoveToFirstAndNextAttribute_WithAttributeFilter2()
		{
			AssertXmlRead(Reader, XmlNodeType.Element, "WithMandatoryElementsAndAttributes", false);

			Reader.MoveToFirstAttribute();
			Reader.MoveToNextAttribute();

			AssertEquals("MandatoryAttribute1Value", Reader.GetAttribute(1));
			AssertEquals(XmlSchemaDefinitionsBase.EdiXmlNamespace, Reader.GetAttribute(0));

			AssertEquals("GetAttribute shouldnt have a side-effect when using MoveToFirstAttribute/MoveToNextAttribute", "MandatoryAttribute1", Reader.LocalName);
		}

		public void TestMoveToFirstAndNextAttribute_InCombinationWithGetAttribute()
		{
			AssertXmlRead(Reader, XmlNodeType.Element, "WithMandatoryElementsAndAttributes", false);
			for (int i = 0; i < 3; i++)
			{
				AssertEquals(true, Reader.MoveToFirstAttribute());
				Assert(Reader.IsNameSpaceAttribute());
				AssertEquals(true, Reader.MoveToNextAttribute());
				AssertEquals("MandatoryAttribute1", Reader.LocalName);
				AssertEquals(false, Reader.MoveToNextAttribute());
			}
		}

		public void TestMoveToFirstAttribute_WhenNoAttributesExist()
		{
			AssertXmlRead(Reader, XmlNodeType.Element, "WithMandatoryElementsAndAttributes", false);
			AssertXmlRead(Reader, XmlNodeType.Element, "NestedElement", false);

			AssertEquals("Reader position should be at the element", "NestedElement", Reader.LocalName);
			AssertEquals("No attributes exist, so false should be returned", false, Reader.MoveToFirstAttribute());
			AssertEquals("Reader position should still be at the element", "NestedElement", Reader.LocalName);
		}

		public void TestMoveToNextAttribute_WhenNoAttributesExist()
		{
			AssertXmlRead(Reader, XmlNodeType.Element, "WithMandatoryElementsAndAttributes", false);
			AssertXmlRead(Reader, XmlNodeType.Element, "NestedElement", false);

			AssertEquals("Reader position should be at the element", "NestedElement", Reader.LocalName);
			AssertEquals("No attributes exist, so false should be returned", false, Reader.MoveToNextAttribute());
			AssertEquals("Reader position should still be at the element", "NestedElement", Reader.LocalName);
		}

		public void TestMoveTo_EmptyElement_ThenEndElement()
		{
			// Testing moving across
			//     <EmptyElement2 />
			// </NestedElement>

			AssertXmlRead(Reader, XmlNodeType.Element, "WithMandatoryElementsAndAttributes", false);
			AssertXmlRead(Reader, XmlNodeType.Element, "NestedElement", false);
			AssertXmlRead(Reader, XmlNodeType.Element, "MandatoryElement1", false);
			Reader.Skip(); // skip MandatoryElement1

			Reader.MoveToElement();
			AssertEquals("Should be up to first EmptyElement1 for this test", "EmptyElement1", Reader.LocalName);
			AssertEquals("The EmptyElement1 has an attribute", true, Reader.HasAttributes);
			Reader.Skip(); // skip EmptyElement1

			Reader.MoveToElement();
			AssertEquals("Should be up to second EmptyElement2 for this test", "EmptyElement2", Reader.LocalName);
			AssertEquals("The EmptyElement2 also has an attribute (of a different name, if the navigator hasn't moved to the second EmptyElement2 it will think it should ignore the attribute)", true, Reader.HasAttributes);
			Reader.Read();
			Reader.MoveToContent();

			AssertXmlReadWithoutRead(Reader, XmlNodeType.EndElement, "NestedElement", false);
			AssertXmlRead(Reader, XmlNodeType.Element, "AnArray", false);
		}

		#endregion

		#region Unimplemented Overrides

		[ExpectException(typeof(NotImplementedException))]
		public void TestGetAttribute_NotImplemented()
		{
			Reader.GetAttribute("");
		}

		[ExpectException(typeof(NotImplementedException))]
		public void TestGetAttribute_NotImplemented2()
		{
			Reader.GetAttribute("", "");
		}

		[ExpectException(typeof(NotImplementedException))]
		public void TestMoveToAttribute_NotImplemented()
		{
			Reader.MoveToAttribute("");
		}

		[ExpectException(typeof(NotImplementedException))]
		public void TestMoveToAttribute_NotImplemented2()
		{
			Reader.MoveToAttribute("", "");
		}

		#endregion

		#region Implementation

		XmlTextReaderFilter Reader;

		protected override void SetUp()
		{
			base.SetUp();
			XmlParserContext context = new XmlParserContext(null, null, "", XmlSpace.None);
			XmlSchema schema = TestXmlSchemaDefinitions.Instance.SchemaWithMandatoryElementsAndAttributes;
			Reader = new XmlTextReaderFilter(schema, GetXML(), XmlNodeType.Document, context)
			{
				WhitespaceHandling = WhitespaceHandling.None
			};
		}

		protected virtual string GetXML()
		{
			return TestXml;
		}

		void AssertXmlRead(XmlReader reader, XmlNodeType expectedNodeType, string expectedName, bool expectedIsEmptyElement)
		{
			reader.Read();
			AssertXmlReadWithoutRead(reader, expectedNodeType, expectedName, expectedIsEmptyElement);
		}

		void AssertXmlReadWithoutRead(XmlReader reader, XmlNodeType expectedNodeType, string expectedName, bool expectedIsEmptyElement)
		{
			AssertEquals("Expected NodeType", expectedNodeType, reader.NodeType);
			AssertEquals("Expected Name", expectedName, reader.LocalName);
			AssertEquals("Expected IsEmptyElement", expectedIsEmptyElement, reader.IsEmptyElement);
		}

		const string TestXml =
			"<WithMandatoryElementsAndAttributes xmlns=\"http://www.edi.com.au/EnterpriseService/\" ExtraAttribute1=\"ExtraAttribute1Value\" MandatoryAttribute1=\"MandatoryAttribute1Value\" ExtraAttribute2=\"ExtraAttribute2Value\">\n" +
			"	<NestedElement>\n" +
			"		<ExtraElement1 />\n" +
			"		<ExtraElement2 />\n" +
			"		<MandatoryElement1>Value</MandatoryElement1>\n" +
			"		<ExtraElement3>Value</ExtraElement3>\n" +
			"		<ExtraElement4>Value</ExtraElement4>\n" +
			"       <EmptyElement1 SomeAttribute=\"x\" />\n" +
			"       <EmptyElement2 SomeOtherAttribute=\"x\" />\n" +
			"	</NestedElement>\n" +
			"	<AnArray>\n" +
			"		<MandatoryElementInArray MandatoryAttribute=\"x\"/>\n" +
			"		<ExtraElementInArray>Value</ExtraElementInArray>\n" +
			"		<ExtraElementInArray2 />\n" +
			"		<MandatoryElementInArray>Splaty</MandatoryElementInArray>\n" +
			"	</AnArray>\n" +
			"</WithMandatoryElementsAndAttributes>\n";

		#endregion
	}
}
