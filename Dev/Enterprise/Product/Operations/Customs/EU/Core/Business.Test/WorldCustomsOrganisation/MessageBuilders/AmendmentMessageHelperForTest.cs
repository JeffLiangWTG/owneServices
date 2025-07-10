using System;
using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageSending;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing;

public class AmendmentMessageHelperForTest : AmendmentMessageHelper
{
	public AmendmentMessageHelperForTest(IPointerParser pointerParser) : base(pointerParser)
	{
	}

	protected override EDIMessage CreateNewMessageForComparison(WCOJobDeclarationMessageSendingObject objectToSend)
	{
		throw new NotImplementedException();
	}

	public XElement GetDiffGramExposed(ZString comparisonXml, ZString newXml, List<XNamespace> namespacesToKeep) => GetDiffGram(comparisonXml, newXml, namespacesToKeep);

	public void ProcessAddNodeExposed(XElement amendmentXML, XDocument oldXml, XDocument newXml, XElement element, List<AlwaysAdd> alwaysAddElements) => ProcessAddNode(amendmentXML, oldXml, element, newXml, alwaysAddElements);

	public List<AlwaysAdd> GetAlwaysAddElementsExposed(XDocument newXMLDoc) => GetAlwaysAddElements(newXMLDoc);

	public AmendmentObjectWrapper GetAmendmentsExposed(WCOJobDeclarationMessageSendingObject objectToSend) => GetAmendments(objectToSend);

	public List<XNamespace> NamespacesToKeepExposed => base.NamespacesToKeep;
}
