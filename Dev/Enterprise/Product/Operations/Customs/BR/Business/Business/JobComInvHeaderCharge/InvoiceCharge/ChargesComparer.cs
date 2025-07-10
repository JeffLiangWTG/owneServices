using System.Collections;

namespace Enterprise.Customs.BR.Business
{
	public class ChargesComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			int result = 0;

			var xCharge = (Customs.Business.BaseJobComInvHeaderCharge)x;
			var yCharge = (Customs.Business.BaseJobComInvHeaderCharge)y;

			if (xCharge.J7_DistributeBy == ChargeDistributeByList.Codes.FOB)
			{
				result = yCharge.J7_DistributeBy == ChargeDistributeByList.Codes.FOB ? 0 : 1;
			}
			else if (yCharge.J7_DistributeBy == ChargeDistributeByList.Codes.FOB)
			{
				result = xCharge.J7_DistributeBy == ChargeDistributeByList.Codes.FOB ? 0 : -1;
			}

			return result;
		}
	}
}
