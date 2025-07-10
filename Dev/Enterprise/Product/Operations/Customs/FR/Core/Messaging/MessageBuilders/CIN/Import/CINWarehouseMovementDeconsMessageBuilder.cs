using System.Collections.Generic;
using System.Xml.Linq;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.CIN
{
	public class CINWarehouseMovementDeconsMessageBuilder : CINImportMessageBuilder
	{
		public CINWarehouseMovementDeconsMessageBuilder(ICINHeader header, string messageId) : base(header, messageId)
		{
		}

		protected override string MessageType => "WarehouseMovement-Decons";

		protected override string MessageBodyTag => "WarehouseMovementDecons";

		protected override IEnumerable<XElement> GetMessageBodyElements => new XElement[]
		{
			PopulateFromGoods(AirWayBill),
			PopulateToGoods(OtherBills)
		};
	}
}
