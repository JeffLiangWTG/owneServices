using Enterprise.ZArchitecture.Core;
namespace Enterprise.Registry.Business
{
	public static class SupplyChainSecurityOrganisationTypes
	{
		public const string Consignor = "CON";
		public const string LocalClient = "LOC";
		public const string ShipmentPickupTransportCompany = "TRS";
		public const string ShipmentPackingCFS = "CFS";
		public const string CoLoadMasterShipmentSendingForwarder = "FOR";
		public const string ConsolSendingAgent = "AGT";
		public const string ConsolAirline = "CAR";
		public const string ConsolTransportCompany = "TRC";
		public const string ConsolCFS = "CFC";
		public const string ConsolCoLoadWithOrganization = "COL";
		public const string ConsolForwarderYouHaveBorrowedMAWBStockFrom = "MAW";
		public const string ShipmentPickupFrom = "PCU";

		public static MultilingualString GetDescription(string code)
		{
			MultilingualString result = (NoResString)"";

			switch (code)
			{
				case Consignor:
					result = ResString.GetMultilingualString("182d9699-4fca-4b85-b687-0b2ff61f52c3", "Consignor");
					break;
				case LocalClient:
					result = ResString.GetMultilingualString("2accf0ac-a531-473c-b73c-3c124d2fa299", "Local Client");
					break;
				case ShipmentPickupTransportCompany:
					result = ResString.GetMultilingualString("78231c31-49a9-4e4d-8828-621035d64553", "Shipment Pickup Transport Company");
					break;
				case ShipmentPackingCFS:
					result = ResString.GetMultilingualString("9284c2cb-b5d9-45be-ac4a-6d8dfe19cdd4", "Shipment Packing CFS");
					break;
				case CoLoadMasterShipmentSendingForwarder:
					result = ResString.GetMultilingualString("b1a4dae0-8c26-49e4-a642-00e48873cb6a", "Co-Load Master Shipment Sending Forwarder");
					break;
				case ConsolSendingAgent:
					result = ResString.GetMultilingualString("438b866a-bdbe-4f6e-8151-4978cc747282", "Consol Sending Agent");
					break;
				case ConsolAirline:
					result = ResString.GetMultilingualString("d9740649-e656-4f2a-a6f1-e2d326e269f0", "Consol Airline");
					break;
				case ConsolTransportCompany:
					result = ResString.GetMultilingualString("b9ccedd0-8407-4251-985b-48f49323b2b7", "Consol Transport Company");
					break;
				case ConsolCFS:
					result = ResString.GetMultilingualString("d11bee22-9088-4268-8253-02d5974d6429", "Consol CFS");
					break;
				case ConsolCoLoadWithOrganization:
					result = ResString.GetMultilingualString("d65bf416-b231-4c38-98d4-189175409a5b", "Consol Co-Load With Organization");
					break;
				case ConsolForwarderYouHaveBorrowedMAWBStockFrom:
					result = ResString.GetMultilingualString("298df76b-0be7-4b0e-9ccd-88ea905f32b2", "Consol Forwarder you have borrowed MAWB Stock from");
					break;
				case ShipmentPickupFrom:
					result = ResString.GetMultilingualString("c3a6f97f-4072-404b-968f-2fba7a68fd47", "Shipment Pickup From");
					break;
			}

			return result;
		}
	}
}
