using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ExportCustomsManifestHeaderManifestHeaderWrapperTest : TestCaseWithFactory
	{
		public void TestVesselID()
		{
			header.ED_VesselName = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Code;
			AssertEquals("VesselID", header.Vessel.RV_LloydsNumber, wrapper.VesselID);
		}

		public void TestCCAN()
		{
			header.ED_CCAN = "12345";
			AssertEquals("VesselID", header.ED_CCAN, wrapper.CCAN);
		}

		public void TestDepotPremiseID()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "Cuckoo Sqkr";
			consignor.PrimaryRegistrationNumber.Number = "123456789";

			var cusCode = consignor.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
			cusCode.OK_CustomsRegNo = "987654321";
			Factory.Save();
			header.PackDepotAddress.OrganisationPK = consignor.PK;
			AssertEquals(2, header.PackDepotAddress.Organisation.CustomsCodes.Count);
			AssertEquals("DepotPremiseID should not be empty", "987654321", header.PackDepotAddress.Organisation.CustomsCodes[1].OK_CustomsRegNo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<ExportCustomsManifestHeader>();
			wrapper = new ExportCustomsManifestHeaderManifestHeaderWrapper(header);
		}

		ExportCustomsManifestHeader header;
		ExportCustomsManifestHeaderManifestHeaderWrapper wrapper;
	}
}
