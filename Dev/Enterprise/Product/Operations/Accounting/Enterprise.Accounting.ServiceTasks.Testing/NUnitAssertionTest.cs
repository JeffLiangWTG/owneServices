using NUnit.Framework;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	// If no test cases are found for NUnit then DAT will fail. This is purely
	// to satisfy DAT and allow NUnit assertions to be used.
	[TestFixture]
	public class NUnitAssertionTest
	{
		[Test]
		public void AllowNUnitModernAssertionsInNUnitCore()
		{
			Assert.Pass();
		}
	}
}
