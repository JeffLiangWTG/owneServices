using NUnit.Framework;

namespace Enterprise.ActivityLogger.Test
{
	sealed class NativeMethodTester : TestCase
	{
		public void TestLoadFree()
		{
			SafeNativeMethods.LoadLibrary();
			SafeNativeMethods.FreeLibrary();
			AssertNoExceptionThrown(() => { });
		}

		public void TestDoubleLoad()
		{
			SafeNativeMethods.LoadLibrary();
			SafeNativeMethods.LoadLibrary();
			AssertNoExceptionThrown(() => { });
		}

		public void TestDoubleFree()
		{
			SafeNativeMethods.LoadLibrary();
			SafeNativeMethods.FreeLibrary();
			AssertNoExceptionThrown(() => { });
		}
		public void TestFreeFirst()
		{
			SafeNativeMethods.FreeLibrary();
			AssertNoExceptionThrown(() => { });
		}

		public void TestAttachDetach()
		{
			SafeNativeMethods.AttachHooks();
			SafeNativeMethods.DetachHooks();
			AssertNoExceptionThrown(() => { });
		}
	}
}
