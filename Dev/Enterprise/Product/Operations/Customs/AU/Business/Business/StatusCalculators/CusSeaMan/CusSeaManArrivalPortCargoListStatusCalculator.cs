
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManArrivalPortCargoListStatusCalculator : CMRStatusCalculator<CusSeaManArrivalPort>
	{
		public CusSeaManArrivalPortCargoListStatusCalculator(CusSeaManArrivalPort arrivalPort) : base(arrivalPort)
		{
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.CargoListStatus.CodeInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.CARLST };
	}
}
