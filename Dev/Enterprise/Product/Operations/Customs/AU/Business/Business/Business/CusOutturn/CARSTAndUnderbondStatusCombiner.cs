using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CARSTAndUnderbondStatusCombiner
	{
		public CARSTAndUnderbondStatusCombiner()
		{
		}

		public ZString GetCombinedStatus(ZString cARSTStatus, ZString underbondStatus)
		{
			if (cARSTStatus.IsEmpty && underbondStatus.IsEmpty)
			{
				return ZString.Empty;
			}
			else if (cARSTStatus.IsEmpty)
			{
				return underbondStatus;
			}
			else if (underbondStatus.IsEmpty)
			{
				return cARSTStatus;
			}
			else if (underbondStatus == CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived)
			{
				return underbondStatus;
			}
			else
			{
				return cARSTStatus;
			}
		}
	}
}
