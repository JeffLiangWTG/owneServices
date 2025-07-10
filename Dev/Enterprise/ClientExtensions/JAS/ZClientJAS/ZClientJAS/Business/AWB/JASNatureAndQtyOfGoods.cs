using Enterprise.Client.JAS.Business.JXC.Export.Validations;
using Enterprise.Freight.Forwarding.AWB.Business;

namespace Enterprise.Client.JAS.Business.AWB
{
	public class JASNatureAndQtyOfGoods : NatureAndQtyOfGoods
	{
		public JASNatureAndQtyOfGoods(ExportAWBRateLine rateLine)
			: base(rateLine)
		{
		}

		protected override NatureAndQtyOfGoodsValidation GetValidation()
		{
			return new JXCNatureAndQtyOfGoodsValidation(this);
		}

		protected override CargoWise.Types.ZString HumanReadableNameCore
		{
			get
			{
				return "Nature and Quantity of Goods";
			}
		}
	}
}
