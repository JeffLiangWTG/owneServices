using System;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class ExciseDutyFromSnapshotProviderTest : Customs.Business.Testing.DataProviderTestCase<ExciseDutyFromSnapshotProvider>
	{
		public void TestNew()
		{
			AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new ExciseDutyFromSnapshotProvider(null));
		}

		public void TestCode()
		{
			AssertEquals("ABC", dataProvider.Code);
		}

		public void TestDegreePercentage()
		{
			AssertEquals(123.45m, dataProvider.DegreePercentage);
		}

		public void TestValue()
		{
			AssertEquals(998877.66m, dataProvider.Value);
		}

		public void TestAmount()
		{
			CombineAssertions(() =>
			{
				AssertEquals(12345.78m, dataProvider.Amount.Quantity);
				AssertEquals("KGM", dataProvider.Amount.MeasurementUnit);
				AssertEquals(String.Empty, dataProvider.Amount.Qualifier);
			});
		}

		public void TestAmount_Null()
		{
			exciseDuty.Amount = null;
			AssertNull(dataProvider.Amount);
		}

		protected override void SetUp()
		{
			base.SetUp();

			exciseDuty = new DEMonthlyClosingEntryLineSnapshotExciseDuty()
			{
				Code = "ABC",
				Amount = new Amount() { Quantity = 12345.78m, MeasurementUnit = "KGM" },
				DegreePercentage = 123.45m,
				Value = 998877.66m
			};
			dataProvider = new ExciseDutyFromSnapshotProvider(exciseDuty);
		}

		IExciseDuty dataProvider;
		DEMonthlyClosingEntryLineSnapshotExciseDuty exciseDuty;

		protected override ExciseDutyFromSnapshotProvider GetProvider() => (ExciseDutyFromSnapshotProvider)dataProvider;
	}
}
