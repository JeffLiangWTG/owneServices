
using CargoWise.EntityFramework;
using CargoWise.Types;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLHeaderShipmentStatusCalculator : CusSeaManOBLHeaderStatusCalculator
	{
		public CusSeaManOBLHeaderShipmentStatusCalculator(BaseCusSeaManOBLHeader oceanBill) : base(oceanBill)
		{
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.BO_ShipmentStatusInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.CARST };
	}
}
