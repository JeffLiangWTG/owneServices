using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Customs.Shared.WorldCustomsOrganisation;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using NUnit.Framework;
using static Enterprise.Customs.EU.WorldCustomsOrganisation.Constants;
using AmendmentMessageHelper = Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.AmendmentMessageHelper;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(FecAmendmentChange))]
	class FecAmendmentChangeTest : DifferenceAbstractTest<FecAmendmentChange>
	{
		protected override FecAmendmentChange CreateConstructor()
		{
			var comparisonXml = "<root><elements><element><value>123</value></element></elements></root>";
			var newXml = "<root><elements><element><value>abc</value></element></elements></root>";
			var element = XElement.Parse(@"<xmldiff><node match=""1""><node match=""1""><change match=""1"">abc</change></node></node></xmldiff>");
			var changeNodes = element.Descendants(AmendmentMessageHelper.Change);
			var node = changeNodes.First();
			var parents = node.AncestorsAndSelf(AmendmentMessageHelper.Node).Select(x => new Node { Index = x.Attribute(AmendmentMessageHelper.Match).Value, NodeName = x.Name.LocalName }).Reverse();
			return new FecAmendmentChangeTestHelper(PointerParser, comparisonXml, newXml, parents, node.Value);
		}

		protected override ZString TypeOfAmendment => AmendmentType.Change;
		protected override Type GenericClassType => typeof(FecAmendmentChange);
		protected override ZString XPath => "//*[1]/*[1]";
		protected override ZString FullXPathWithNodeNames => "//*[local-name() = 'root'][1]/*[local-name() = 'elements'][1]";
		protected override ZString[] GetHierarchyStrings() => ((FecAmendmentChangeTestHelper)Constructor).GetHierarchyStrings();
		protected override ZString HierarchyStrings => "Declaration[1]//GoodsShipment[1]//GovernmentAgencyGoodsItem[1]//Commodity[1]//GoodsMeasure[1]//GrossMassMeasure";
		protected override ZString GetHierarchyString() => ((FecAmendmentChangeTestHelper)Constructor).GetHierarchyString();
		protected override ZString HierarchyString => "Declaration[1]//GoodsShipment[1]//GovernmentAgencyGoodsItem[1]//Commodity[1]//GoodsMeasure[1]//GrossMassMeasure";
		protected override string FullPointersFromNames => "c";
		protected override ZString LastPointer => "Declaration[1]//GoodsShipment[1]//GovernmentAgencyGoodsItem[1]//Commodity[1]//GoodsMeasure[1]/";
	}

	internal class FecAmendmentChangeTestHelper : FecAmendmentChange
	{
		public FecAmendmentChangeTestHelper(IPointerParser pointerParser, ZString comparisonXml, ZString newXml, IEnumerable<Node> nodeTree, ZString newValue) : base(pointerParser, comparisonXml, newXml, nodeTree, newValue)
		{
		}

		public new ZString[] GetHierarchyStrings() => base.GetHierarchyStrings();
		public new ZString GetHierarchyString() => base.GetHierarchyString();
	}
}
