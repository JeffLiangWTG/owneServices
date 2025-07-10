using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.AWB
{
	public class JASShipmentExportAWBRateLine : ShipmentExportAWBRateLine
	{
		public JASShipmentExportAWBRateLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Freight.Forwarding.AWB.Business.NatureAndQtyOfGoods GetNewNatureAndQtyOfGoodsText()
		{
			return new JASNatureAndQtyOfGoods(this);
		}
	}
}
