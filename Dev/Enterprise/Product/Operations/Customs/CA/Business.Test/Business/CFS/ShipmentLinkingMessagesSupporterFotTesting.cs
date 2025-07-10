using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ShipmentLinkingMessagesSupporterFotTesting : ShipmentLinkingMessagesSupporter
	{
		public ShipmentLinkingMessagesSupporterFotTesting(CommonShipment shipment)
			: base(shipment)
		{
		}

		public ZString CargoControlNumberChanged_Exposed;
		public ZInt ChangeTimes = 0;
		public EDIMessage[] MatchingRNSStatusMessages_Exposed;
		public EDIMessage[] MatchingForwardedManifestMessages_Exposed;

		protected override void OnCargoControlNumberChanged(ZString cargoControlNumber)
		{
			base.OnCargoControlNumberChanged(cargoControlNumber);

			ChangeTimes++;

			CargoControlNumberChanged_Exposed = ZString.Format("Current Cargo Control Number changed to {0}", cargoControlNumber);
			MatchingRNSStatusMessages_Exposed = GetMatchingRNSStatusMessagesByCCN(cargoControlNumber);
			MatchingForwardedManifestMessages_Exposed = GetMatchingForwardedManifestMessagesByCCN(cargoControlNumber);
		}
	}
}
