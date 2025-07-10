using System;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DataImportIndicatorServiceTest : TestCaseWithFactory
	{
		public void TestInstanceIsReused()
		{
			var instance1 = DataImportIndicatorService.GetInstance(Factory);
			AssertNotNull(instance1);

			var instance2 = DataImportIndicatorService.GetInstance(Factory);
			AssertSame(instance1, instance2);
		}

		public void TestFlagBelongsToFactory()
		{
			var factory1 = NewFactory();
			var factory2 = NewFactory();

			var instance1 = DataImportIndicatorService.GetInstance(factory1);
			var instance2 = DataImportIndicatorService.GetInstance(factory2);

			using (DataImportIndicatorService.StartDataImport(factory1))
			{
				AssertEquals(true, instance1.IsDataImportInProgress);
				AssertEquals(false, instance2.IsDataImportInProgress);
			}
		}

		public void TestIsDataImportInProgress()
		{
			var indicatorService = DataImportIndicatorService.GetInstance(Factory);
			AssertEquals(false, indicatorService.IsDataImportInProgress);

			using (DataImportIndicatorService.StartDataImport(Factory))
			{
				AssertEquals(true, indicatorService.IsDataImportInProgress);
				AssertEquals("Data Import already started.", AssertExceptionThrown<InvalidOperationException>(() => DataImportIndicatorService.StartDataImport(Factory)).Message);
				AssertEquals(true, indicatorService.IsDataImportInProgress);
			}

			AssertEquals(false, indicatorService.IsDataImportInProgress);
		}
	}
}
