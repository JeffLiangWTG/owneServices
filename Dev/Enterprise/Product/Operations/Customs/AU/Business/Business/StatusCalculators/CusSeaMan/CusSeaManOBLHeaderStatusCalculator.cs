
using CargoWise.EntityFramework;
using CargoWise.Types;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLHeaderStatusCalculator : CMRStatusCalculator<BaseCusSeaManOBLHeader>
	{
		public CusSeaManOBLHeaderStatusCalculator(BaseCusSeaManOBLHeader oceanBill) : base(oceanBill)
		{
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.BO_MessageStatusInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.SEACR };
	}
}
