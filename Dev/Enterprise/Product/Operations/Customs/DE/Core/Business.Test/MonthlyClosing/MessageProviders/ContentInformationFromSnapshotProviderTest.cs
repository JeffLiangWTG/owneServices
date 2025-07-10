using System;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	sealed class ContentInformationFromSnapshotProviderTest : Customs.Business.Testing.DataProviderTestCase<ContentInformationFromSnapshotProvider>
	{
		public void TestNew()
		{
			AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new ContentInformationFromSnapshotProvider(null));
		}

		public void TestContentType()
		{
			AssertEquals("A", dataProvider.ContentType);
		}

		public void TestDegreePercentage()
		{
			AssertEquals(123.45m, dataProvider.DegreePercentage);
		}

		protected override void SetUp()
		{
			base.SetUp();

			contentInformation = new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation() { DegreePercentage = 123.45m, Type = "A" };
			dataProvider = new ContentInformationFromSnapshotProvider(contentInformation);
		}

		IContentInformation dataProvider;
		DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation contentInformation;

		protected override ContentInformationFromSnapshotProvider GetProvider() => (ContentInformationFromSnapshotProvider)dataProvider;
	}
}
