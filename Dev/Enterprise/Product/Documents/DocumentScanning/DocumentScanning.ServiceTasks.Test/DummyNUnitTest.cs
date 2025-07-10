using NUnit.Framework;

namespace Enterprise.DocumentScanning.ServiceTasks.Test
{
	[TestFixture]
	public class DummyNUnitTest
	{
		[Test]
		public void AlwaysPasses()
		{
			Assert.Pass("This test always passes, it is just a dummy test to satisfy the requirement of having at least one NUnit test in the assembly.");
		}
	}
}
