using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocShipmentConsol : DocBaseConsol
	{
		DocShipmentConsol(CommonConsol consol, BusinessObjectFactory factoryToWrap)
			: base(consol, factoryToWrap)
		{
		}

		public static new DocShipmentConsol New(CommonConsol consol, BusinessObjectFactory factoryToWrap)
		{
			if (consol == null)
			{
				return null;
			}
			else
			{
				return new DocShipmentConsol(consol, factoryToWrap);
			}
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		public override DocUNLOCO PortOfLoading
		{
			get { return DocUNLOCO.New(Factory, Consol.JK_RL_NKLoadPort); }
		}

		public override DocUNLOCO PortOfDischarge
		{
			get { return DocUNLOCO.New(Factory, Consol.JK_RL_NKDischargePort); }
		}

		public override ZDateTime ETA
		{
			get { return Consol.Transports.ArrivalTransport.JW_ETA; }
		}

		public override ZDateTime ETD
		{
			get { return Consol.Transports.DepartureTransport.JW_ETD; }
		}

		public override ZDateTime ATD
		{
			get { return Consol.Transports.DepartureTransport.JW_ATD; }
		}

		public override ZDateTime ATA
		{
			get { return Consol.Transports.ArrivalTransport.JW_ATA; }
		}

		public ZString AseanPointOfOriginForBOL
		{
			get
			{
				if (PortOfLoading != null && PortOfLoading.Code.StartsWith("HK"))
				{
					return PortOfLoading.PortName + "," + PortOfLoading.CountryName;
				}
				return ZString.Empty;
			}
		}

		public DocShipmentCollection Shipments
		{
			get
			{
				DocShipmentCollection coll = new DocShipmentCollection(Consol.Factory);
				foreach (CommonShipment shipment in Consol.Shipments)
				{
					coll.Add(DocShipment.New(shipment, Factory));
				}
				return coll;
			}
		}

		#region Forwarding Instruction

		public ZString Carrier
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsCoLoad && Creditor != null)
				{
					result = Creditor.PostalAddress;
				}
				else if (ShippingLine != null)
				{
					result = ShippingLine.PostalAddress;
				}

				return result;
			}
		}

		#endregion

	}
}
