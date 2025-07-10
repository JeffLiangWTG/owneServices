using Enterprise.Semaphores.Common;
using Enterprise.Semaphores.Common.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(UrlAuthenticationSemaphore))]
	sealed class UrlAuthenticationSemaphoreTest : SemaphoreTypeTestCase
	{
		public void TestLicenceLoginSemaphore()
		{
			AssertEquals("LockInfo", "UrlAuthentication:KEY", TestSemaphore.LockInfo);
		}

		protected override ISemaphoreType TestSemaphore
		{
			get { return new UrlAuthenticationSemaphore("KEY"); }
		}
	}
}
