using Enterprise.ZArchitecture;

namespace Enterprise.Client.MFI.AutoeDocAllocation.Testing
{
	public class HoldDocumentAllocatorTest : DocumentAllocatorTest
	{
		protected override void AssertSpecificAllocationResult(NotificationBuffer notify)
		{
		}

		#region Implementation
		protected override DocumentAllocator GetDocumentAllocator()
		{
			return new HoldDocumentAllocator();
		}
		#endregion
	}
}
