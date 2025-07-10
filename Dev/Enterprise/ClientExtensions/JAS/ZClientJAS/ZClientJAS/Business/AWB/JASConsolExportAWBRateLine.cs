using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.AWB
{
	public class JASConsolExportAWBRateLine : ConsolExportAWBRateLine
	{
		public JASConsolExportAWBRateLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Freight.Forwarding.AWB.Business.NatureAndQtyOfGoods GetNewNatureAndQtyOfGoodsText()
		{
			return new JASNatureAndQtyOfGoods(this);
		}
	}
}
