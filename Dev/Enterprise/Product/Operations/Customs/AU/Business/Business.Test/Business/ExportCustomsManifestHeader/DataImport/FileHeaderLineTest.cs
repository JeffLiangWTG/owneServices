using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class FileHeaderLineTest : TestCaseWithFactory
	{
		public void TestSetManifestType()
		{
			mapper.SetManifestType("EMM");
			AssertEquals(ManifestTypeList.Codes.ExportMainManifest, header.ED_ManifestType);
			header.ED_ManifestType = string.Empty;
			mapper.SetManifestType("ESM");
			AssertEquals(ManifestTypeList.Codes.SlotExportSubManifest, header.ED_ManifestType);
			header.ED_ManifestType = string.Empty;
			mapper.SetManifestType("XXX");
			AssertEquals(string.Empty, header.ED_ManifestType);
		}

		public void TestSetVessel()
		{
			mapper.SetVessel("SHIPNAME", "2468013");
			var vesselName1 = header.ED_VesselName;
			header.ED_VesselName = ZString.Empty;
			mapper.SetVessel("SHIPNAME", "1234567");
			var vesselName2 = header.ED_VesselName;
			AssertEquals(vesselName1, vesselName2);
			AssertEquals("SHIPNAME", header.Vessel.RV_Code);
			AssertEquals("1234567", header.Vessel.RV_LloydsNumber);
			//Factory.Save();	TODO: This line can be re-instated once the unique key constraint on RV_Code is removed from the reference file & duplicate vessels are allowed. (test will then have 2 vessels with same name but different Lloyds numbers)
		}

		public void TestSetPortOfDeparture()
		{
			mapper.SetPortOfDeparture("AUNTL");
			AssertEquals("AUNTL", header.ED_RL_NKPortOfDeparture);
		}

		public void TestSetPortOfDestination()
		{
			mapper.SetPortOfDestination("HKHKG");
			AssertEquals("HKHKG", header.ED_RL_NKPortOfDestination);
			AssertEquals("HK", header.ED_RN_NKCountryOfDestination);
		}

		public void TestSetETD()
		{
			mapper.SetETD("20040828");
			AssertEquals(new ZDateTime(2004, 8, 28), header.ED_DepartureDate);
			header.ED_DepartureDate = ZDateTime.Empty;

			mapper.SetETD("20040828154200");
			AssertEquals(new ZDateTime(2004, 8, 28, 15, 42, 0), header.ED_DepartureDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<ExportCustomsManifestHeader>();
			importer = new EuroPacificManifestImporter(Factory);
			mapper = new FileHeaderLine(importer, header, null);
		}

		ManifestImporter importer;
		FileHeaderLine mapper;
		ExportCustomsManifestHeader header;
	}
}
