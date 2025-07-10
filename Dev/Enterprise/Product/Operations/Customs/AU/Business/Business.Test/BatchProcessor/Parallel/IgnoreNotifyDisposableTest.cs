using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class IgnoreNotifyDisposableTest : TestCase
	{
		public void TestDispose()
		{
			foreach (var shouldNotify in new[] { false, true })
			{
				int disposeCount = 0;
				var notifyDisposable = new IgnoreNotifyDisposable(new DisposableAction(() => disposeCount++));
				AssertEquals(0, disposeCount);

				if (shouldNotify)
				{
					// notify should have no effect on the results
					notifyDisposable.Notify();
				}

				notifyDisposable.Dispose();
				AssertEquals(1, disposeCount);
				notifyDisposable.Dispose();
				AssertEquals(2, disposeCount);
			}
		}
	}
}
