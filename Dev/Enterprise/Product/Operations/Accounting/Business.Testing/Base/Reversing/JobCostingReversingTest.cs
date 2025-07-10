using System;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class JobCostingReversingTest : ReversingBaseTest
	{
		protected override Type GetTestingClassType()
		{
			return typeof(JobCostingReversing);
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = new TestIJobCostingTransaction();
		}

		protected override void SetupReversingIReversingInstance()
		{
			TestReversingIReversingInstance = new TestIJobCostingTransaction();
		}
	}
}