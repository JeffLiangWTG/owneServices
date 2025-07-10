
using CargoWise.EntityFramework;
using CargoWise.Types;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManArrivalPortStatusCalculator : CMRStatusCalculator<CusSeaManArrivalPort>
	{
		public CusSeaManArrivalPortStatusCalculator(CusSeaManArrivalPort arrivalPort) : base(arrivalPort)
		{
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.ActualArrivalResponseStatus.CodeInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.SEAAAR };
	}
}
