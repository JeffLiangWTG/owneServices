using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SummaryLineTest : TestCaseWithFactory
	{
		public void TestSetEmptyContainers()
		{
			mapper.SetEmptyContainers("12");
			AssertEquals((short)12, header.ED_NoOfEmptyContainers);
		}

		public void TestSetContainers()
		{
			mapper.SetContainers("12", " 15 ");
			AssertEquals((short)15, header.ED_NoOfContainer);
		}

		public void TestSetPackagesOverflow()
		{
			int shortMaxValue = short.MaxValue;
			mapper.SetPackages((shortMaxValue + 2).ToString());
			AssertEquals(shortMaxValue + 2, header.ED_NoOfPacks);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<ExportCustomsManifestHeader>();
			importer = new EuroPacificManifestImporter(Factory);
			mapper = new SummaryLine(importer, header, null);
		}

		ManifestImporter importer;
		SummaryLine mapper;
		ExportCustomsManifestHeader header;
	}
}
