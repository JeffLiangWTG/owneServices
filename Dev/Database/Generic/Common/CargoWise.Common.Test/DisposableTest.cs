using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class DisposableTest : TestCase
	{
		public void TestIsDisposed()
		{
			// Arrange
			var disposable = new DisposableObject();
			Assert(!disposable.IsDisposed);
			// Act
			disposable.Dispose();
			// Assert
			Assert(disposable.IsDisposed);
		}

		public void TestHookedUpToLeakListener()
		{
			using (var listenedDisposable = new DisposableObject())
			{
				Assert("Should be registered", DisposableLeakListener.Instance.IsRegistered(listenedDisposable));
				using (var unlistenedDisposable = new DisposableObjectWithoutLeakListener())
				{
					Assert("Should not be registered", !DisposableLeakListener.Instance.IsRegistered(unlistenedDisposable));
				}
			}
		}
	}
}