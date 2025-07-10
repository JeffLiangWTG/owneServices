using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageSending;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing
{
	[TestsSubclassesOf(typeof(AmendmentMessageHelper))]
	public abstract class AmendmentMessageHelperAbstractTest<T> : TestCaseWithFactory
		where T : AmendmentMessageHelper
	{
		protected abstract string InitialXML { get; }
		protected abstract string ExpectedXML { get; }
		protected abstract string InitialXMLForSendChanges { get; }
		protected abstract string NewXMLForSendChanges { get; }
		protected abstract string ExpectedXMLForSendChanges { get; }
		protected abstract T MessageHelper { get; }
		protected abstract WCOJobDeclarationMessageSendingObject MessageSendingObject { get; }

		List<XNamespace> namespacesToKeep => new List<XNamespace>();

		public void TestCanBeAmended()
		{
			var result = MessageHelper.CanBeAmended(MessageSendingObject);
			AssertEquals(true, result);
		}

		public void TestOrderAdditionalInformations()
		{
			var initialXML = LoadSampleUxml(InitialXML);
			var resultXML = XDocument.Parse(AmendmentMessageHelper.OrderAdditionalInformations(initialXML));
			var expectedXML = XDocument.Parse(LoadSampleUxml(ExpectedXML));

			AssertEquals(expectedXML.ToString(), resultXML.ToString());
		}

		internal string LoadSampleUxml(string key)
		{
			using (var stream = GetType().Assembly.GetManifestResourceStream(key))
			using (var sr = new StreamReader(stream))
			{
				return sr.ReadToEnd();
			}
		}

		public void TestGetDiffGram()
		{
			var helper = new AmendmentMessageHelperForTest(null);
			var initialXMl = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<TestData><Declaration><ID>TestDeclaration1</ID></Declaration></TestData>";
			var newXML = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<TestData><Declaration><ID>TestDeclaration2</ID></Declaration></TestData>";

			var diffGram = helper.GetDiffGramExposed(initialXMl, newXML, namespacesToKeep);

			AssertEquals("We should be comparing the XML's as if there was no xml version tag.", "<nodematch=\"1\">", diffGram.FirstNode.ToString().Replace(" ", "").Substring(0, 15));

			initialXMl = "<TestData><Declaration><ID>TestDeclaration1</ID></Declaration></TestData>";
			newXML = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<TestData><Declaration><ID>TestDeclaration2</ID></Declaration></TestData>";

			diffGram = helper.GetDiffGramExposed(initialXMl, newXML, namespacesToKeep);

			AssertEquals("We should be comparing the XML's as if there was no xml version tag (xml tag on new XML).", "<nodematch=\"1\">", diffGram.FirstNode.ToString().Replace(" ", "").Substring(0, 15));

			initialXMl = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<TestData><Declaration><ID>TestDeclaration1</ID></Declaration></TestData>";
			newXML = "<TestData><Declaration><ID>TestDeclaration2</ID></Declaration></TestData>";

			diffGram = helper.GetDiffGramExposed(initialXMl, newXML, namespacesToKeep);

			AssertEquals("We should be comparing the XML's as if there was no xml version tag (xml tag on original XML).", "<nodematch=\"1\">", diffGram.FirstNode.ToString().Replace(" ", "").Substring(0, 15));
		}

		public void TestProcessAddNode()
		{
			XElement amendmentXML = new XElement("Commodity");
			var initialXML = XDocument.Parse(@"<Commodity>
          <Description>PARTS OF RAT</Description>
          <Classification>
            <ID>42021219</ID>
            <IdentificationTypeCode>TSP</IdentificationTypeCode>
          </Classification>
          <Classification>
            <ID>00</ID>
            <IdentificationTypeCode>TRC</IdentificationTypeCode>
          </Classification>
          <DutyTaxFee>
            <DutyRegimeCode>100</DutyRegimeCode>
            <QuotaOrderID>123456</QuotaOrderID>
            <Payment>
              <MethodCode>E</MethodCode>
            </Payment>
          </DutyTaxFee>
          <GoodsMeasure>
            <GrossMassMeasure>0</GrossMassMeasure>
            <NetNetWeightMeasure>100</NetNetWeightMeasure>
            <TariffQuantity>0</TariffQuantity>
          </GoodsMeasure>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""GBP"">100.00</ItemChargeAmount>
          </InvoiceLine>
          <TransportEquipment>
            <SequenceNumeric>1</SequenceNumeric>
            <ID>CNT123</ID>
          </TransportEquipment>
          <TransportEquipment>
            <SequenceNumeric>2</SequenceNumeric>
            <ID>CNT456</ID>
          </TransportEquipment>
        </Commodity>");
			var expectedXML = XElement.Parse(@"<Commodity>
  <Classification />
  <Classification />
  <Classification>
    <ID>VATZ</ID>
    <IdentificationTypeCode>GN</IdentificationTypeCode>
  </Classification>
  <Classification>
    <ID>VATZ</ID>
    <IdentificationTypeCode>TRA</IdentificationTypeCode>
  </Classification>
</Commodity>");
			var xDiff = XElement.Parse(@"<xmldiff version=""1.0"" srcDocHash=""5927582294943938072"" options=""IgnoreComments IgnoreNamespaces IgnorePI IgnorePrefixes IgnoreWhitespace IgnoreXmlDecl IgnoreDtd"" fragments=""no"">
<node match=""1""/>
<add>
  <Classification>
    <ID>VATZ</ID>
    <IdentificationTypeCode>GN</IdentificationTypeCode>
  </Classification>
  <Classification>
    <ID>VATZ</ID>
    <IdentificationTypeCode>TRA</IdentificationTypeCode>
  </Classification>
</add>
</xmldiff>");

			var elementToAdd = xDiff.Element("add");
			var newXml = XDocument.Parse(expectedXML.ToString());

			var helper = new AmendmentMessageHelperForTest(null);
			helper.ProcessAddNodeExposed(amendmentXML, initialXML, newXml, elementToAdd, new List<AlwaysAdd>());
			AssertEquals("Only two empty Classification nodes should be added", expectedXML.Value, amendmentXML.Value);
		}

		public void TestAlwaysSend()
		{
			var helper = new AmendmentMessageHelperForTest(null);
			var objectToSend = MessageSendingObject;
			var diffGram = helper.GetDiffGramExposed(InitialXMLForSendChanges, NewXMLForSendChanges ?? ZString.Empty, namespacesToKeep);
			objectToSend.AmendmentDetails = new AmendmentDetails(diffGram, InitialXMLForSendChanges, NewXMLForSendChanges);
			var amendment = MessageHelper.GetAmendments(objectToSend);

			var expectedXmlDoc = XDocument.Parse(ExpectedXMLForSendChanges);
			var actualXmlDoc = amendment.Xml;

			AssertXMLEquals("The always send elements (if any) should be included in the generated message.", expectedXmlDoc.ToString(), actualXmlDoc.ToString());
		}

		public void TestRemoveNamespacesExcept_ShouldReturnNull_WhenElementIsNull()
		{
			XElement element = null;
			var namespacesToKeep = new List<XNamespace>();

			var result = AmendmentMessageHelper.RemoveNamespacesExcept(element, namespacesToKeep);

			AssertNull(result);
		}

		public void TestRemoveNamespacesExcept_ShouldRemoveAllNamespaces_WhenNoNamespacesToKeep()
		{
			var xml = "<root xmlns='http://example.com/ns1'><child xmlns='http://example.com/ns2'>Text</child></root>";
			XElement element = XElement.Parse(xml);
			var namespacesToKeep = new List<XNamespace>();

			var result = AmendmentMessageHelper.RemoveNamespacesExcept(element, namespacesToKeep);

			var expectedXml = @"<root>
  <child>Text</child>
</root>";
			AssertEquals(expectedXml, result.ToString());
		}

		public void TestRemoveNamespacesExcept_ShouldKeepSpecifiedNamespaces()
		{
			var xml = "<root xmlns='http://example.com/ns1'><child xmlns='http://example.com/ns2'>Text</child></root>";
			XElement element = XElement.Parse(xml);
			var namespacesToKeep = new List<XNamespace> { XNamespace.Get("http://example.com/ns2") };

			var result = AmendmentMessageHelper.RemoveNamespacesExcept(element, namespacesToKeep);

			var expectedXml = @"<root>
  <child xmlns=""http://example.com/ns2"">Text</child>
</root>";
			AssertEquals(expectedXml, result.ToString());
		}

		public void TestRemoveNamespacesExcept_ShouldRemoveNamespaceFromAttributes()
		{
			var xml = "<root xmlns='http://example.com/ns1' attr=\"value\"><child xmlns='http://example.com/ns2'>Text</child></root>";
			XElement element = XElement.Parse(xml);
			var namespacesToKeep = new List<XNamespace>();

			var result = AmendmentMessageHelper.RemoveNamespacesExcept(element, namespacesToKeep);
			var expectedXml = @"<root attr=""value"">
  <child>Text</child>
</root>";
			AssertEquals(expectedXml, result.ToString());
		}

		public void TestRemoveNamespacesExcept_ShouldNotModifyElementsWithoutNamespaces()
		{
			var xml = "<root><child>Text</child></root>";
			XElement element = XElement.Parse(xml);
			var namespacesToKeep = new List<XNamespace>();

			var result = AmendmentMessageHelper.RemoveNamespacesExcept(element, namespacesToKeep);

			var expectedXml = @"<root>
  <child>Text</child>
</root>";
			AssertEquals(expectedXml, result.ToString());
		}

		public void TestRemoveNamespacesExcept_ShouldHandleEmptyElementsProperly()
		{
			var xml = "<root xmlns='http://example.com/ns1'></root>";
			XElement element = XElement.Parse(xml);
			var namespacesToKeep = new List<XNamespace>();

			var result = AmendmentMessageHelper.RemoveNamespacesExcept(element, namespacesToKeep);

			var expectedXml = "<root />";
			AssertEquals(expectedXml, result.ToString());
		}

		public void TestRemoveNamespacesExcept_ShouldWorkForDeepNestedElements()
		{
			var xml = "<root xmlns='http://example.com/ns1'><child xmlns='http://example.com/ns2'><grandchild>Text</grandchild></child></root>";
			XElement element = XElement.Parse(xml);
			var namespacesToKeep = new List<XNamespace> { XNamespace.Get("http://example.com/ns2") };

			var result = AmendmentMessageHelper.RemoveNamespacesExcept(element, namespacesToKeep);

			var expectedXml = @"<root>
  <child xmlns=""http://example.com/ns2"">
    <grandchild>Text</grandchild>
  </child>
</root>";
			AssertEquals(expectedXml, result.ToString());
		}

		public void TestNamespacesToKeep_ShouldReturnEmptyListByDefault()
		{
			var helper = new AmendmentMessageHelperForTest(null);

			var result = helper.NamespacesToKeepExposed;

			AssertNotNull("NamespacesToKeep should return a non-null list.", result);
			AssertEquals("NamespacesToKeep should return an empty list by default.", 0, result.Count);
		}
	}
}
