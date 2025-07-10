using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.DataTransfer.DataExport.Testing
{
	public class PositivePayBatchNumberAllocatorTest : TestCaseWithFactory
	{
		public void TestGetNewBatchNumber()
		{
			PositivePayBatchNumberAllocator allocator = new PositivePayBatchNumberAllocator();
			AssertEquals("Should get a valid batch number back from allocator", 1000, allocator.GetNewBatchNumber());
			AssertEquals("Should get a valid batch number back from allocator", 1001, allocator.GetNewBatchNumber());
			AssertEquals("Should get a valid batch number back from allocator", 1002, allocator.GetNewBatchNumber());
		}
	}
}
