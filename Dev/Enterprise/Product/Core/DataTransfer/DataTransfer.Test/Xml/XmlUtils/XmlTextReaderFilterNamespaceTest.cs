namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class XmlTextReaderFilterNamespaceTest : XmlTextReaderFilterTest
	{
		// keep the following XML the same as the TestXml in the parent test class with the added namespace on every element
		const string TestXmlWithNamespace =
			"<test:WithMandatoryElementsAndAttributes xmlns:test=\"http://www.edi.com.au/EnterpriseService/\" ExtraAttribute1=\"ExtraAttribute1Value\" MandatoryAttribute1=\"MandatoryAttribute1Value\" ExtraAttribute2=\"ExtraAttribute2Value\">\n" +
			"	<test:NestedElement>\n" +
			"		<test:ExtraElement1 />\n" +
			"		<test:ExtraElement2 />\n" +
			"		<test:MandatoryElement1>Value</test:MandatoryElement1>\n" +
			"		<test:ExtraElement3>Value</test:ExtraElement3>\n" +
			"		<test:ExtraElement4>Value</test:ExtraElement4>\n" +
			"       <test:EmptyElement1 SomeAttribute=\"x\" />\n" +
			"       <test:EmptyElement2 SomeOtherAttribute=\"x\" />\n" +
			"	</test:NestedElement>\n" +
			"	<test:AnArray>\n" +
			"		<test:MandatoryElementInArray MandatoryAttribute=\"x\"/>\n" +
			"		<test:ExtraElementInArray>Value</test:ExtraElementInArray>\n" +
			"		<test:ExtraElementInArray2 />\n" +
			"		<test:MandatoryElementInArray>Splaty</test:MandatoryElementInArray>\n" +
			"	</test:AnArray>\n" +
			"</test:WithMandatoryElementsAndAttributes>\n";

		protected override string GetXML()
		{
			return TestXmlWithNamespace;
		}
	}
}
