using System.Collections.Generic;
using CargoWise.Customs.Shared.WorldCustomsOrganisation;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class FecAmendmentChange : Change
	{
		public FecAmendmentChange(IPointerParser pointerParser, ZString comparisonXml, ZString newXml, IEnumerable<Node> nodeTree, ZString newValue) : base(pointerParser, comparisonXml, newXml, nodeTree, newValue)
		{
		}

		protected override ZString GetHierarchyString() => "Declaration[1]//GoodsShipment[1]//GovernmentAgencyGoodsItem[1]//Commodity[1]//GoodsMeasure[1]//GrossMassMeasure";
	}
}
