using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CargoGroupLineTest : TestCaseWithFactory
	{
		public void TestSetContainers()
		{
			mapper.SetContainers("0000000001");
			AssertEquals((short)1, header.Lines[0].EL_NumberOfContainers);
		}

		public void TestNumberOfPackagesLimits()
		{
			int shortMaxValue = short.MaxValue;
			mapper.SetPackages((shortMaxValue + 2).ToString());
			AssertEquals(shortMaxValue + 2, header.Lines[0].EL_NumberOfPackages);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<ExportCustomsManifestHeader>();
			header.LoadOrCreateLineWithCANIntoCurrentManifestLine("12345");
			importer = new EuroPacificManifestImporter(Factory);
			mapper = new CargoGroupLine(importer, header, null);
		}

		ManifestImporter importer;
		CargoGroupLine mapper;
		ExportCustomsManifestHeader header;
	}
}
