using NUnit.Framework;

// This is a dummy test to satisfy the requirement of having at least one NUnit test in the assembly and to avoid the following error:
/*
	System.AggregateException: One or more errors occurred. ---> Dat.ImplHostClient.ImplHostException: GetTests for scope net8.0 <assembly> failed
	System.InvalidOperationException: NUnit failed to load <assembly
*/
// This file can be removed once tests have been migrated to NUnit
namespace Enterprise.Build.Database.Script.Test
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
