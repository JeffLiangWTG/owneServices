using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ShipperDetailsLineTest : TestCaseWithFactory
	{
		public void TestSetOwnerName()
		{
			header.LoadOrCreateLineWithCANIntoCurrentManifestLine("12345");

			importer.NewLineAdded = false;
			mapper.SetOwnerName("Owner Name");
			AssertEquals("Owner Name", importer.CurrentOwnerName);
			AssertEquals(ZString.Empty, header.CurrentManifestLine.EL_GoodsOwner);
			importer.NewLineAdded = true;
			mapper.SetOwnerName("Owner Name");
			AssertEquals("Owner Name", header.CurrentManifestLine.EL_GoodsOwner);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<ExportCustomsManifestHeader>();
			importer = new EuroPacificManifestImporter(Factory);
			mapper = new ShipperDetailsLine(importer, header, null);
		}

		ManifestImporter importer;
		ShipperDetailsLine mapper;
		ExportCustomsManifestHeader header;
	}
}
