using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.Business.Testing
{
	class ICSAsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAMA_VesselName()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_VesselName = "X";
			AssertHasWarning(header.AMA_VesselNameInfo, "The Vessel Name entered does not exist in the reference file.");

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Vessel";
			header.AMA_VesselName = vessel.RV_Code;
			AssertNoWarning(header.AMA_VesselNameInfo, "The Vessel Name entered does not exist in the reference file.");

			var consol = Factory.New<ForwardingConsol>();
			header.AMA_ParentId = consol.PK;
			header.AMA_VesselName = ZString.Empty;
			AssertNoMessageErrors(header.AMA_VesselNameInfo);

			header.AMA_ParentId = ZGuid.NewZGuid();
			header.Validation.ValidateAMA_VesselName();
			AssertNoMessageErrors(header.AMA_VesselNameInfo);

			header.AMA_ManifestType = EUManifestTypes.Codes.ICS;
			header.Validation.ValidateAMA_VesselName();
			AssertHasMessageErrorContaining(header.AMA_VesselNameInfo, "You have not entered");

			header.AMA_VesselName = vessel.RV_Code;
			header.Validation.ValidateAMA_VesselName();
			AssertHasMessageErrorContaining(header.AMA_VesselNameInfo, "Lloyds Number must be set for this vessel.");

			vessel.RV_LloydsNumber = "123";
			header.Validation.ValidateAMA_VesselName();
			AssertNoMessageErrors(header.AMA_VesselNameInfo);
		}

		public void TestCheckAMA_RN_NKConveyanceNationality()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var consol = Factory.New<ForwardingConsol>();
			header.AMA_ParentId = consol.PK;
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;

			AssertNoMessageErrors(header.AMA_RN_NKConveyanceNationalityInfo);

			header.AMA_ParentId = ZGuid.NewZGuid();
			header.Validation.ValidateAMA_RN_NKConveyanceNationality();
			AssertNoMessageErrors(header.AMA_RN_NKConveyanceNationalityInfo);

			header.AMA_ManifestType = EUManifestTypes.Codes.ICS;
			header.Validation.ValidateAMA_RN_NKConveyanceNationality();
			AssertHasMessageErrorContaining(header.AMA_RN_NKConveyanceNationalityInfo, "You have not entered");

			header.AMA_RN_NKConveyanceNationality = "IT";
			AssertNoMessageErrors(header.AMA_RN_NKConveyanceNationalityInfo);
		}
	}
}
