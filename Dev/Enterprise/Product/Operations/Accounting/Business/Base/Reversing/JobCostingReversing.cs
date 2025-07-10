using Enterprise.Accounting.Business.Base.Interfaces;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public class JobCostingReversing : ReversingBase
	{
		public JobCostingReversing(IJobCosting jobCostingTransaction)
			: base(jobCostingTransaction)
		{
		}
	}
}
