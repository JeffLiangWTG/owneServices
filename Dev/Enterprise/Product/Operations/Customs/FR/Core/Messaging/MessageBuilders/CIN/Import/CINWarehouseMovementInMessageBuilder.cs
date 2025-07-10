using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.CIN
{
	public class CINWarehouseMovementInMessageBuilder : CINImportMessageBuilder
	{
		public CINWarehouseMovementInMessageBuilder(ICINHeader header, string messageId) : base(header, messageId)
		{
		}

		protected override string MessageType => "WarehouseMovement-In";
		protected override string MessageBodyTag => "WarehouseMovementIn";

		protected override ZString FromLocation => header.NewLocation.IsEmpty ? nOT_CIN_LOCATION : header.NewLocation;
		protected override ZString FromLocationLabel => header.NewLocation.IsEmpty ? nOT_CIN_LOCATION : header.NewLocation;

		protected override IEnumerable<XElement> GetMessageBodyElements => new XElement[]
		{
			PopulateFromElement(),
			PopulateDetailedGoods(AirWayBill),
			PopulateCustomsStatus()
		};
	}
}
