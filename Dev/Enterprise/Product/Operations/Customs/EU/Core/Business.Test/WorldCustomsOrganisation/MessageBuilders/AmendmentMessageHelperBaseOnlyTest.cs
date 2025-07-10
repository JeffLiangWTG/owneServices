using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageSending;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing
{
	class AmendmentMessageHelperBaseOnlyTest : TestCaseWithFactory
	{
		List<XNamespace> namespacesToKeep => new List<XNamespace>();

		public void TestNoExceptionIfParameterIsNullForGetNodeHierarchyList()
		{
			AssertNoExceptionThrown(() => AmendmentMessageHelper.GetNodeHierarchyList(null));
		}

		public void TestGetAlwaysAddElementsFromDocument()
		{
			var alwaysAddElementNames = new List<string>();
			alwaysAddElementNames.Add("Element1");
			alwaysAddElementNames.Add("Element3");

			var newXmlDoc = new XDocument();
			var root = new XElement("Root");
			var declaration = new XElement("Declaration");
			var element1 = new XElement("Element1");
			declaration.Add(element1);
			var element2 = new XElement("Element2");
			declaration.Add(element2);
			var element3 = new XElement("Element3");
			declaration.Add(element3);
			root.Add(declaration);
			newXmlDoc.Add(root);

			var helper = new BaseAmendmentMessageHelperForTest(null);

			var alwaysAddElements = helper.GetAlwaysAddElementsFromDocumentExposed(alwaysAddElementNames, newXmlDoc, "Declaration");

			AssertEquals("There should be 2 elements in the list", 2, alwaysAddElements.Count);

			AssertEquals("Root name of first element", "Declaration", alwaysAddElements[0].RootName);
			AssertEquals("Name of first element", "Element1", alwaysAddElements[0].Name);
			AssertEquals("Index of first element", 1, alwaysAddElements[0].Index);
			AssertEquals("Node name of first element", "Element1", alwaysAddElements[0].Node.Name.LocalName);

			AssertEquals("Root name of second element", "Declaration", alwaysAddElements[1].RootName);
			AssertEquals("Name of second element", "Element3", alwaysAddElements[1].Name);
			AssertEquals("Index of second element", 3, alwaysAddElements[1].Index);
			AssertEquals("Node name of second element", "Element3", alwaysAddElements[1].Node.Name.LocalName);
		}

		public void TestGetDiffGram()
		{
			var xElement1 = new XElement("Importer", new XElement("Line", "UNIT 1A SOUTHAMPTON"));
			var address = "UNIT 1A  SOUTHAMPTON";
			var xElement2 = new XElement("Importer", new XElement("Line", address));

			var helper = new AmendmentMessageHelperForTest(null);
			var diffGram = helper.GetDiffGramExposed(xElement1.ToString(), xElement2.ToString(), namespacesToKeep);
			AssertNotContains("Prerequisite: XmlDiffOptions should not contain IgnoreWhitespace", "IgnoreWhitespace", diffGram.ToString());
			AssertXMLEquals("Comparing the XML's returns the difference", address, diffGram.Value);

			diffGram = helper.GetDiffGramExposed(xElement2.ToString(), xElement2.ToString(), namespacesToKeep);
			AssertXMLEquals("Comparing the XML's returns no difference", ZString.Empty, diffGram.Value);
		}

		public void TestAddEmptySiblingNodes()
		{
			var initialXml = @"<Root>
  <Declaration>
    <One />
    <Two>
      <Title>TitleOne</Title>
    </Two>
    <Two>
      <Title>TitleTwo</Title>
    </Two>
  </Declaration>
</Root>";
			var changedXml = initialXml.Replace("TitleOne", "TitleOneChanged");
			var expectedXml = @"<Declaration>
  <Two>
    <Title>TitleOneChanged</Title>
  </Two>
</Declaration>";
			RunTestAndAssertExpected("Changed Title of first Two", initialXml, changedXml, expectedXml);

			changedXml = initialXml.Replace("TitleTwo", "TitleTwoChanged");
			expectedXml = @"<Declaration>
  <Two />
  <Two>
    <Title>TitleTwoChanged</Title>
  </Two>
</Declaration>";
			RunTestAndAssertExpected("Changed Title of second Two", initialXml, changedXml, expectedXml);

			changedXml = @"<Root>
  <Declaration>
    <One>
      <Title>AddedTitleToOne</Title>
    </One>
    <Two>
      <Title>TitleOne</Title>
    </Two>
    <Two>
      <Title>TitleTwo</Title>
    </Two>
  </Declaration>
</Root>";
			expectedXml = @"<Declaration>
  <One>
    <Title>AddedTitleToOne</Title>
  </One>
</Declaration>";
			RunTestAndAssertExpected("Added Title to One", initialXml, changedXml, expectedXml);

			changedXml = @"<Root>
  <Declaration>
    <One />
    <One />
    <Two>
      <Title>TitleOne</Title>
    </Two>
    <Two>
      <Title>TitleTwo</Title>
    </Two>
  </Declaration>
</Root>";
			expectedXml = @"<Declaration>
  <One />
  <One />
</Declaration>";
			RunTestAndAssertExpected("Added a second One", initialXml, changedXml, expectedXml);

			changedXml = @"<Root>
  <Declaration>
    <One />
    <Two>
      <Title>TitleOne</Title>
    </Two>
    <Two>
      <Title>TitleTwo</Title>
    </Two>
    <Two>
      <Title>TitleThree</Title>
    </Two>
  </Declaration>
</Root>";
			expectedXml = @"<Declaration>
  <Two />
  <Two />
  <Two>
    <Title>TitleThree</Title>
  </Two>
</Declaration>";
			RunTestAndAssertExpected("Added a third Two", initialXml, changedXml, expectedXml);

			changedXml = changedXml.Replace("TitleOne", "ChangedTitleOne");
			expectedXml = @"<Declaration>
  <Two>
    <Title>ChangedTitleOne</Title>
  </Two>
  <Two />
  <Two>
    <Title>TitleThree</Title>
  </Two>
</Declaration>";
			RunTestAndAssertExpected("Changed title of first Two and added a third Two", initialXml, changedXml, expectedXml);
			
			initialXml = @"<Root>
  <Declaration>
    <One>
      <Two>
        <Title>TitleOne</Title>
      </Two>
      <Two>
        <Title>TitleTwo</Title>
      </Two>
    </One>
  </Declaration>
</Root>";

			changedXml = @"<Root>
  <Declaration>
    <One>
      <Two>
        <Title>TitleOne</Title>
      </Two>
      <Two>
        <Title>TitleTwo</Title>
        <SubTitle>SubTitle2</SubTitle>
      </Two>
    </One>
  </Declaration>
</Root>";
			expectedXml = @"<Declaration>
  <One>
    <Two />
    <Two>
      <SubTitle>SubTitle2</SubTitle>
    </Two>
  </One>
</Declaration>";
			RunTestAndAssertExpected("Added Subtitle to second Two", initialXml, changedXml, expectedXml);
		}

		void RunTestAndAssertExpected(string message, string initialXml, string changedXml, string expectedXml)
		{
			var helper = new AmendmentMessageHelperForTest(null);
			var objectToSend = new WCOJobDeclarationMessageSendingObject(Factory.NewWithValidTestData<CusEntryHeader>());
			var diffGram = helper.GetDiffGramExposed(initialXml, changedXml, namespacesToKeep);
			objectToSend.AmendmentDetails = new AmendmentDetails(diffGram, initialXml, changedXml);
			var amendment = helper.GetAmendments(objectToSend);
			var actualXml = amendment.Xml.ToString();
			AssertXMLEquals(message, expectedXml, actualXml);
		}
	}

	public class BaseAmendmentMessageHelperForTest : AmendmentMessageHelper
	{
		public BaseAmendmentMessageHelperForTest(IPointerParser pointerParser) : base(pointerParser)
		{
		}

		protected override EDIMessage CreateNewMessageForComparison(WCOJobDeclarationMessageSendingObject objectToSend)
		{
			throw new System.NotImplementedException();
		}

		public List<AlwaysAdd> GetAlwaysAddElementsFromDocumentExposed(List<string> alwaysAddElementNames, XDocument newXMLDoc, string rootName) => GetAlwaysAddElementsFromDocument(alwaysAddElementNames, newXMLDoc, rootName);
	}
}
