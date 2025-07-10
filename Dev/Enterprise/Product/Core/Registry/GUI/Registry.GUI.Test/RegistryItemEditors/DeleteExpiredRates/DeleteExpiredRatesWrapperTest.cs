using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DeleteExpiredRatesWrapper))]
	sealed class DeleteExpiredRatesWrapperTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestDeleteExpiredRatesWrapper()
		{
			var deleteExpiredRates = new DeleteExpiredRates();
			deleteExpiredRates.ExpiredRatesPeriodInYears = 1;
			deleteExpiredRates.BatchSize = 500;

			var deleteExpiredRatesWrapper = new DeleteExpiredRatesWrapper(deleteExpiredRates);
			AssertEquals(1, deleteExpiredRatesWrapper.ExpiredRatesPeriodInYears);
			AssertEquals(500, deleteExpiredRatesWrapper.BatchSize);

			deleteExpiredRatesWrapper.ExpiredRatesPeriodInYears = 2;
			deleteExpiredRatesWrapper.BatchSize = 100;
			AssertEquals(2, deleteExpiredRates.ExpiredRatesPeriodInYears);
			AssertEquals(100, deleteExpiredRates.BatchSize);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DeleteExpiredRatesWrapper(new DeleteExpiredRates { ExpiredRatesPeriodInYears = 1, BatchSize = 100 });
		}
	}
}
