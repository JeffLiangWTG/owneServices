using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CargoGroupItemsLineTest : TestCaseWithFactory
	{
		public void TestSetCAN()
		{
			importer.CurrentOwnerName = "Owner Name";
			importer.CurrentCountryOfDestination = "NZ";
			mapper.SetCAN("AAAAAAMP7");
			AssertEquals("AAAAAAMP7", header.Lines[0].EL_CAN);
			AssertEquals(CANType.CustomsAuthorityNumber.Code, header.Lines[0].EL_TypeOfCAN);
			AssertEquals("NZ", header.Lines[0].EL_RN_NKCountryOfDestination);
			AssertEquals("Owner Name", header.Lines[0].EL_GoodsOwner);
			AssertEquals(true, importer.NewLineAdded);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<ExportCustomsManifestHeader>();
			//Header.Lines.AddNew();
			importer = new EuroPacificManifestImporter(Factory);
			mapper = new CargoGroupItemsLine(importer, header, null);
		}

		ManifestImporter importer;
		CargoGroupItemsLine mapper;
		ExportCustomsManifestHeader header;
	}
}
