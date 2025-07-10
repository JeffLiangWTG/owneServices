using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	class HeaderPresentationDateProviderTest : Customs.Business.Testing.DataProviderTestCase<HeaderPresentationDateProvider>
	{
		public void TestNew()
		{
			AssertNull("Argument == null", HeaderPresentationDateProvider.NewOrNull(null));
		}

		public void TestStartDateTime() => AssertEquals(new ZDateTime(2020, 2, 1, 3, 3, 0), dataProvider.StartDateTime);

		public void TestEndDateTime() => AssertEquals(new ZDateTime(2020, 3, 1, 3, 3, 0), dataProvider.EndDateTime);

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.ZG_PresentationStartDate = new ZDateTime(2020, 2, 1, 3, 3, 3, 3);
			declaration.ZG_PresentationEndDate = new ZDateTime(2020, 3, 1, 3, 3, 3, 3);
			dataProvider = HeaderPresentationDateProvider.NewOrNull(declaration);
		}
		IDateTimeRange dataProvider;

		protected override HeaderPresentationDateProvider GetProvider() => (HeaderPresentationDateProvider)dataProvider;
	}
}
