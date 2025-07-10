using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public sealed class CaptureMemoryDumpForDisposableLeakAttribute : TestSetupAttribute
	{
		public override void SetUp(TestCase testCase)
		{
			DisposableLeakListener.Instance.CaptureMemoryDump = true;
		}

		public override void TearDown(TestCase testCase)
		{
		}
	}
}
