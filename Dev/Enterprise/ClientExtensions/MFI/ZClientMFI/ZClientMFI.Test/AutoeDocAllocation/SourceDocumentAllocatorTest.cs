using Enterprise.ZArchitecture;

namespace Enterprise.Client.MFI.AutoeDocAllocation.Testing
{
	public class SourceDocumentAllocatorTest : DocumentAllocatorTest
	{
		protected override DocumentAllocator GetDocumentAllocator()
		{
			return new SourceDocumentAllocator();
		}

		protected override void AssertSpecificAllocationResult(NotificationBuffer notify)
		{
			Assert("Allocation process should complete", notify.AsString.Contains("Attaching Documents completed"));
		}
	}
}
