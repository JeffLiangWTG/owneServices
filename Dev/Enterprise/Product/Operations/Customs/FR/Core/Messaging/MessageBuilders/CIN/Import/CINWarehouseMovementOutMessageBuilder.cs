using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.CIN
{
	public class CINWarehouseMovementOutMessageBuilder : CINImportMessageBuilder
	{
		public CINWarehouseMovementOutMessageBuilder(ICINHeader header, string messageId) : base(header, messageId)
		{
		}

		protected override string MessageType => "WarehouseMovement-Out";
		protected override string MessageBodyTag => "WarehouseMovementOut";

		protected override ZString ToLocation => header.NewLocation.IsEmpty ? nOT_CIN_LOCATION : header.NewLocation;
		protected override ZString ToLocationLabel => header.NewLocation.IsEmpty ? nOT_CIN_LOCATION : header.NewLocation;

		protected override IEnumerable<XElement> GetMessageBodyElements
		{
			get
			{
				var elements = new List<XElement>
				{
					PopulateToElement(),
					PopulateDetailedGoods(AirWayBill),
					PopulateCustomsStatus()
				};

				var customsDocs = PopulateCustomsDocuments();
				if (customsDocs != null && customsDocs.Any())
				{
					elements.AddRange(customsDocs);
				}

				return elements.ToArray();
			}
		}
	}
}
