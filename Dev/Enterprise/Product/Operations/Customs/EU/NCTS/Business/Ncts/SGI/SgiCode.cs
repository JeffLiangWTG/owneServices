using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class SgiCode : ISgiCode
	{
		public SgiCode(ZString code, ZDecimal qty)
		{
			Code = code;
			Qty = qty;
		}

		public ZString Code { get; }
		public ZDecimal Qty { get; }
	}
}
