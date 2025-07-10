
using CargoWise.EntityFramework;
using CargoWise.Types;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusPartShipStatusCalculator : CMRStatusCalculator<CusPartShip>
	{
		public CusPartShipStatusCalculator(CusPartShip partShip) : base(partShip)
		{
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.CG_CustomsStatusInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.AIRCR, CMRMessage.CMRMessageTypes.CARST };
	}
}
