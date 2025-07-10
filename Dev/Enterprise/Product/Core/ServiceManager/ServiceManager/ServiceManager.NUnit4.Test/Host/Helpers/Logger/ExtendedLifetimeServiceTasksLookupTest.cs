using NUnit.Framework;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.Logger
{
	public class ExtendedLifetimeServiceTasksLookupTest
	{
		[Test]
		public void TestGetNumberOfLogFilesToPreserve()
		{
			var lookup = new ExtendedLifetimeServiceTasksLookup();
			Assert.That(lookup.GetNumberOfLogFilesToPreserve("LWK"), Is.EqualTo(7));
			Assert.That(lookup.GetNumberOfLogFilesToPreserve("HPQ"), Is.EqualTo(7));
			Assert.That(lookup.GetNumberOfLogFilesToPreserve("XXX"), Is.EqualTo(7));
			Assert.That(lookup.GetNumberOfLogFilesToPreserve("PFC"), Is.EqualTo(28));
			Assert.That(lookup.GetNumberOfLogFilesToPreserve("PML"), Is.EqualTo(60));
		}
	}
}
