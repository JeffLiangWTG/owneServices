using NUnit.Framework;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class SuspendChangeTrackingTest : TestCase
	{
		public void TestSuspendsTracking()
		{
			VisualiserDataSet dS = new VisualiserDataSet();
			AssertEquals(false, dS.fSuspendChangeTracking);
			using (dS.SuspendChangeTracking)
			{
				AssertEquals(true, dS.fSuspendChangeTracking);
			}
			AssertEquals(false, dS.fSuspendChangeTracking);
		}
	}
}
