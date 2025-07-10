using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Customs.Shared.WorldCustomsOrganisation;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class NilAmendmentChange : Change
	{
		public NilAmendmentChange(IPointerParser pointerParser, ZString comparisonXml, ZString newXml, IEnumerable<Node> nodeTree, ZString newValue) : base(pointerParser, comparisonXml, newXml, nodeTree, newValue)
		{
			MasterUCRSequence = GetMucrSequence(comparisonXml);
		}

		int GetMucrSequence(ZString comparisonXML)
		{
			int index = 1;

			if (!comparisonXML.IsEmpty)
			{
				var element = XElement.Parse(comparisonXML);
				var goodsShipment = element.Descendants().FirstOrDefault(x => x.Name.LocalName == "Declaration")?.Elements()?.Where(x => x.Name.LocalName == "GoodsShipment") ?? Array.Empty<XElement>();
				var previousDocs = goodsShipment.Elements()?.Where(x => x.Name.LocalName == "PreviousDocument") ?? Array.Empty<XElement>();

				var masterUCRPreviousDoc = previousDocs.FirstOrDefault(x => x.Elements()?.FirstOrDefault(y => y.Name.LocalName == "TypeCode")?.Value == "MCR");
				if (masterUCRPreviousDoc != null)
				{
					index = previousDocs.ToList().IndexOf(masterUCRPreviousDoc) + 1;
				}
			}

			return index;
		}

		internal int MasterUCRSequence { get; }

		protected override ZString GetHierarchyString() => $"Declaration[1]//GoodsShipment[1]//PreviousDocument[{MasterUCRSequence}]//ID";
	}
}
