using CargoWise.Common;

namespace CargoWise.EntityFramework.Testing
{
	sealed class StressTestAttributeTest : TestCaseWithFactory
	{
		public void TestLargeNumberOfObjectsReportsError()
		{
			CreateLargeNumberOfDummyObjects();

			ZQuery filter = new ZQuery();
			filter.MaximumRows = BusinessObjectFactory.MaximumObjectsToCreateBeforeRaisingLargeLoadError + 1;
			Factory.Load(typeof(DummyBusinessObject), filter);
			AssertEquals("ExceededAllowableNewObjectCount", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		[StressTest]
		public void TestAttributeSuppressesError()
		{
			CreateLargeNumberOfDummyObjects();

			ZQuery filter = new ZQuery();
			filter.MaximumRows = BusinessObjectFactory.MaximumObjectsToCreateBeforeRaisingLargeLoadError;
			Factory.Load(typeof(DummyBusinessObject), filter);
			AssertEquals(true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
		}

		static void CreateLargeNumberOfDummyObjects()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			for (int i = 0; i < 1001; i++)
			{
				DummyBusinessObject dummy = newFactory.New<DummyBusinessObject>();
				dummy.Z0_VarCharMax = "Value" + i;
			}
			newFactory.Save();
		}
	}
}
