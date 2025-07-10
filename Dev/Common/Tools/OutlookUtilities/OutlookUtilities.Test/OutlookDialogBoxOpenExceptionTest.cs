using NUnit.Framework;

namespace Enterprise.Interop.OutlookIntegration.Testing
{
	sealed class OutlookDialogBoxOpenExceptionTest : TestCase
	{
		public void TestExtendsOutlookOperationAbortedException()
		{
			AssertEquals(
				"Must extend " + nameof(OutlookOperationAbortedException) + " as other parts of the system catch this type. A dialog showing is almost always an error displayed to the user.",
				typeof(OutlookOperationAbortedException), typeof(OutlookDialogBoxOpenException).BaseType);
		}
	}
}
