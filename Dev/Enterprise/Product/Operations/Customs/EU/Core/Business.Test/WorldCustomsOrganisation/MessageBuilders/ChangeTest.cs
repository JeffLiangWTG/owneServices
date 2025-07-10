using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Customs.Shared.WorldCustomsOrganisation;
using CargoWise.Types;
using NUnit.Framework;
using Constants = Enterprise.Customs.EU.WorldCustomsOrganisation.Constants;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing;

[TestedType(typeof(Change))]
public class ChangeTest : DifferenceAbstractTest<Change>
{
	protected override Change CreateConstructor()
	{
		var comparisonXml = "<root><elements><element><value>123</value></element></elements></root>";
		var newXml = "<root><elements><element><value>abc</value></element></elements></root>";
		var element = XElement.Parse(@"<xmldiff><node match=""1""><node match=""1""><change match=""1"">abc</change></node></node></xmldiff>");
		var changeNodes = element.Descendants(AmendmentMessageHelper.Change);
		var node = changeNodes.First();
		var parents = node.AncestorsAndSelf(AmendmentMessageHelper.Node).Select(x => new Node { Index = x.Attribute(AmendmentMessageHelper.Match).Value, NodeName = x.Name.LocalName }).Reverse();
		return new ChangeTestHelper(PointerParser, comparisonXml, newXml, parents, node.Value);
	}

	protected override ZString TypeOfAmendment => Constants.AmendmentType.Change;
	protected override Type GenericClassType => typeof(Change);
	protected override ZString XPath => "//*[1]/*[1]";
	protected override ZString FullXPathWithNodeNames => "//*[local-name() = 'root'][1]/*[local-name() = 'elements'][1]";
	protected override ZString[] GetHierarchyStrings() => ((ChangeTestHelper)Constructor).GetHierarchyStrings();
	protected override ZString HierarchyStrings => "root[1]//elements[1]";
	protected override ZString GetHierarchyString() => ((ChangeTestHelper)Constructor).GetHierarchyString();
	protected override ZString HierarchyString => "root[1]//elements[1]";
	protected override string FullPointersFromNames => "c";
	protected override ZString LastPointer => "root[1]/";
}
