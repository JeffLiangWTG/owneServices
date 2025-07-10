using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.AWB
{
	public class JASShipmentExportAWBRateLineCollection : ShipmentExportAWBRateLineCollection
	{
		public JASShipmentExportAWBRateLineCollection(JASShipmentExportAWBHeader master)
			: base(master)
		{
		}

		public new JASShipmentExportAWBRateLine this[int index]
		{
			get { return (JASShipmentExportAWBRateLine)(Elements[index]); }
		}

		public new JASShipmentExportAWBRateLine AddNew()
		{
			return (JASShipmentExportAWBRateLine)base.AddNew();
		}
	}
}
