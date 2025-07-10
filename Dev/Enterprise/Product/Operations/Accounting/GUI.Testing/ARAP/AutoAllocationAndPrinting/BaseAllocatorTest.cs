using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Utility.Testing;

namespace Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting.Testing
{
	public abstract class BaseAllocatorTest : NonPersistentBusinessObjectTestCase
	{
		public abstract void TestGetFactoriesWithAllocationCodeToBeCalledOnSaving_Core();

		public abstract void TestCanContinueWithAllocation();

		public abstract void TestCanContinueWithPrinting();

		public abstract void TestMultipleFactorySavesWillNotCauseANewAllocation();

		#region Implementation

		protected TransactionAllocateTestHelper TestHelper
		{
			get
			{
				if (fTestHelper == null)
				{
					fTestHelper = new TransactionAllocateTestHelper(Factory);
				}
				return fTestHelper;
			}
		}
		TransactionAllocateTestHelper fTestHelper;

		#endregion
	}
}
