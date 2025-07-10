using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class BookingHeaderLineTest : TestCaseWithFactory
	{
		public void TestSetCountryOfDestination()
		{
			mapper.SetCountryOfDestination("HK");
			AssertEquals("HK", importer.CurrentCountryOfDestination);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<ExportCustomsManifestHeader>();
			importer = new EuroPacificManifestImporter(Factory);
			mapper = new BookingHeaderLine(importer, header, null);
		}

		ManifestImporter importer;
		BookingHeaderLine mapper;
		ExportCustomsManifestHeader header;
	}
}
