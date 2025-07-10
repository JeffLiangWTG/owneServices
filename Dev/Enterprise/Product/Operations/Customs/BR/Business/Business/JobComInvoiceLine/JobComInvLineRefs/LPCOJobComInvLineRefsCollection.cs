using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class LPCOJobComInvLineRefsCollection : JobComInvLineRefsCollection<LPCOJobComInvLineRefs>
	{
		public LPCOJobComInvLineRefsCollection(BusinessObject parent) : base(parent, JobComInvLineRefsType.Codes.Lpco)
		{
		}
	}
}
