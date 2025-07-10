using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DisposableManagerTest : TestCase
	{
		public void TestDispose()
		{
			bool isDisposed = false;
			var dispose = new DisposableAction(() => isDisposed = true);
			AssertEquals(false, isDisposed);

			using (var manager = new DisposableManager())
			{
				AssertEquals(dispose, manager.Subscribe(dispose));
				AssertEquals(false, isDisposed);
			}

			AssertEquals(true, isDisposed);
		}
	}
}
