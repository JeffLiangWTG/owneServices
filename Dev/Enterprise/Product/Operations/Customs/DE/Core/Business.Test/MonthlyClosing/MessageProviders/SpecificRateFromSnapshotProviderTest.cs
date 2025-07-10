using System;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class SpecificRateFromSnapshotProviderTest : Customs.Business.Testing.DataProviderTestCase<SpecificRateFromSnapshotProvider>
	{
		public void TestNew()
		{
			AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new SpecificRateFromSnapshotProvider(null));
		}

		public void TestType()
		{
			AssertEquals("A", dataProvider.Type);
		}

		public void TestValue()
		{
			AssertEquals(123.45m, dataProvider.Value);
		}

		protected override void SetUp()
		{
			base.SetUp();

			specificRate = new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate() { Value = 123.45m, Type = "A" };
			dataProvider = new SpecificRateFromSnapshotProvider(specificRate);
		}

		IImportSpecificRate dataProvider;
		DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate specificRate;

		protected override SpecificRateFromSnapshotProvider GetProvider() => (SpecificRateFromSnapshotProvider)dataProvider;
	}
}
