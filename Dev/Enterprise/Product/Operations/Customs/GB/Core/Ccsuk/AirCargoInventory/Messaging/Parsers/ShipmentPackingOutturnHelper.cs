using System.Linq;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class ExportShipmentPackingOutturnHelper
	{
		public ExportShipmentPackingOutturnHelper(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				shipment = declaration.Shipment;
			}
		}

		/// <summary>
		/// Reports packages received into facility as recorded on shipment's packing tab's outturn section.
		/// NB here outturn means turned out from deliverey van into facility.
		/// </summary>
		internal int PackagesReceived
		{
			get
			{
				var result = 0;
				if (shipment != null)
				{
					result = (from ForwardingPackLine packingLine in shipment.OuterPackLines select (int)packingLine.JL_Outturn).Sum();
				}
				return result;
			}
		}
		readonly ForwardingShipment shipment;
	}
}
