using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Customs.Shared.WorldCustomsOrganisation;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Customs.EU.WorldCustomsOrganisation.Constants;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing
{
	[TestedType(typeof(Addition))]
	class AdditionTest : DifferenceAbstractTest<Addition>
	{
		protected override Addition CreateConstructor()
		{
			var comparisonXml = "<root><elements><element><value>123</value></element></elements></root>";
			var newXml = "<root><elements><element><value>123</value></element><element><value>abc</value></element></elements></root>";
			var element = XElement.Parse(@"<xmldiff><node match=""1""><node match=""1""><node match=""1"" /><add><element><value>abc</value></element></add></node></node></xmldiff>");
			var addNodes = element.Descendants(AmendmentMessageHelper.Add);
			var node = addNodes.First();
			var parents = node.AncestorsAndSelf(AmendmentMessageHelper.Node).Select(x => new Node { Index = x.Attribute(AmendmentMessageHelper.Match).Value, NodeName = x.Name.LocalName }).Reverse();
			return new AdditionTestHelper(PointerParser, comparisonXml, newXml, parents, node.Elements().ToArray());
		}

		protected override ZString TypeOfAmendment => AmendmentType.Addition;
		protected override ZString XPath => "//*[1]/*[1]";
		protected override Type GenericClassType => typeof(Addition);
		protected override ZString FullXPathWithNodeNames => "//*[local-name() = 'root'][1]/*[local-name() = 'elements'][1]";
		protected override ZString[] GetHierarchyStrings() => ((AdditionTestHelper)Constructor).GetHierarchyStrings();
		protected override ZString HierarchyStrings => "root[1]//elements[1]//element[2]//value";
		protected override ZString GetHierarchyString() => ((AdditionTestHelper)Constructor).GetHierarchyString();
		protected override ZString HierarchyString => "root[1]//elements[1]";
		protected override string FullPointersFromNames => "a";
		protected override ZString LastPointer => "root[1]//elements[1]//element[2]/";

		public void TestGetSequence()
		{
			var hierarchyString = GetHierarchyStrings().FirstOrDefault();
			AssertContains("Should contain element[2]", "element[2]", hierarchyString);
		}
	}

	[TestedType(typeof(Removal))]
	class RemovalTest : DifferenceAbstractTest<Removal>
	{
		protected override Removal CreateConstructor()
		{
			var comparisonXml = "<root><elements><element><value>123</value></element></elements></root>";
			var newXml = "<root><elements></elements></root>";
			var element = XElement.Parse(@"<xmldiff><node match=""1""><node match=""1""><remove match=""1"" /></node></node></xmldiff>");
			var removeNodes = element.Descendants(AmendmentMessageHelper.Remove);
			var node = removeNodes.First();
			var parents = node.AncestorsAndSelf(AmendmentMessageHelper.Node).Select(x => new Node { Index = x.Attribute(AmendmentMessageHelper.Match).Value, NodeName = x.Name.LocalName }).Reverse();
			var removeNodeValue = node.Attribute(AmendmentMessageHelper.Match).Value;
			return new RemovalTestHelper(PointerParser, comparisonXml, newXml, parents, removeNodeValue);
		}

		protected override ZString TypeOfAmendment => AmendmentType.Deletion;
		protected override Type GenericClassType => typeof(Removal);
		protected override ZString XPath => "//*[1]/*[1]";
		protected override ZString FullXPathWithNodeNames => "//*[local-name() = 'root'][1]/*[local-name() = 'elements'][1]";
		protected override ZString[] GetHierarchyStrings() => ((RemovalTestHelper)Constructor).GetHierarchyStrings();
		protected override ZString HierarchyStrings => "root[1]//elements[1]//element[1]";
		protected override ZString GetHierarchyString() => ((RemovalTestHelper)Constructor).GetHierarchyString();
		protected override ZString HierarchyString => "root[1]//elements[1]//element[1]";
		protected override string FullPointersFromNames => "c";
		protected override ZString LastPointer => "root[1]//elements[1]/";
	}

	internal class ChangeTestHelper : Change
	{
		public ChangeTestHelper(IPointerParser pointerParser, ZString comparisonXml, ZString newXml, IEnumerable<Node> nodeTree, ZString newValue) : base(pointerParser, comparisonXml, newXml, nodeTree, newValue)
		{
		}

		public new ZString[] GetHierarchyStrings() => base.GetHierarchyStrings();
		public new ZString GetHierarchyString() => base.GetHierarchyString();
	}
}
