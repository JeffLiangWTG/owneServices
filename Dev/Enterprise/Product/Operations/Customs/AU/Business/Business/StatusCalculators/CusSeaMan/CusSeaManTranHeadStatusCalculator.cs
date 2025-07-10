
using CargoWise.EntityFramework;
using CargoWise.Types;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManTranHeadStatusCalculator : CMRStatusCalculator<CusSeaManTranHead>
	{
		public CusSeaManTranHeadStatusCalculator(CusSeaManTranHead transportHeader) : base(transportHeader)
		{
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.ImpendingArrivalResponseStatus.CodeInfo;

		protected internal override ZString[] InterestedMessageTypes => new ZString[] { CMRMessage.CMRMessageTypes.SEAIAR };
	}
}
