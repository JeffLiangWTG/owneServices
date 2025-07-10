using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class TransportMeanRoadValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJW_RL_NKDiscPort()
		{
			transportMean.JW_RL_NKDiscPort = "";
			transportMean.Validation.ValidateJW_RL_NKDiscPort();
			AssertHasMessageErrorContaining("When Discharge Port is empty", transportMean.JW_RL_NKDiscPortInfo, MandatoryValidation.YouHaveNotEntered);
			transportMean.JW_RL_NKDiscPort = "ILJOR";
			AssertNoMessageErrorContaining("When Discharge Port is not empty", transportMean.JW_RL_NKDiscPortInfo, MandatoryValidation.YouHaveNotEntered);
			asycudaManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			transportMean.JW_RL_NKDiscPort = "";
			transportMean.Validation.ValidateJW_RL_NKDiscPort();
			AssertNoMessageErrorContaining("When Transport Mode is not Road('ROA') and Discharge Port is empty", transportMean.JW_RL_NKDiscPortInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJW_Vessel()
		{
			transportMean.JW_Vessel = "";
			transportMean.Validation.ValidateJW_Vessel();
			AssertHasMessageErrorContaining("When Vehicle ID is empty", transportMean.JW_VesselInfo, MandatoryValidation.YouHaveNotEntered);
			transportMean.JW_Vessel = "123";
			AssertNoMessageErrorContaining("When Vehicle ID is not empty", transportMean.JW_VesselInfo, MandatoryValidation.YouHaveNotEntered);
			asycudaManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			transportMean.JW_Vessel = "";
			transportMean.Validation.ValidateJW_Vessel();
			AssertNoMessageErrorContaining("When Transport Mode is not Road('ROA') and Vehicle ID is empty", transportMean.JW_RL_NKDiscPortInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckVehicleCountry()
		{
			transportMean.VehicleCountry = "";
			ValidateInfo(transportMean.VehicleCountryInfo);
			AssertHasMessageErrorContaining("Vehicle Registration Country - When no value entered", transportMean.VehicleCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("Vehicle Registration Country - When no list entry", transportMean.VehicleCountryInfo, ListValidation.InvalidCodeMessageError.ToString());

			transportMean.VehicleCountry = "-";
			ValidateInfo(transportMean.VehicleCountryInfo);
			AssertNoMessageErrorContaining("Vehicle Registration Country - When a value entered", transportMean.VehicleCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("Vehicle Registration Country - When invalid list entry", transportMean.VehicleCountryInfo, ListValidation.InvalidCodeMessageError.ToString());

			transportMean.VehicleCountry = Core.Constants.CountryCodes.Israel;
			ValidateInfo(transportMean.VehicleCountryInfo);
			AssertNoMessageErrorContaining("Vehicle Registration Country - When a value entered", transportMean.VehicleCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("Vehicle Registration Country - When valid list entry", transportMean.VehicleCountryInfo, ListValidation.InvalidCodeMessageError.ToString());

			asycudaManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea; 
			transportMean.VehicleCountry = "";
			ValidateInfo(transportMean.VehicleCountryInfo);
			AssertNoMessageErrorContaining("Vehicle Registration Country - When Transport Mode is not Road('ROA') and  no value entered", transportMean.VehicleCountryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTruckKind()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType("C1307", "C1307 List", Core.Constants.CountryCodes.Israel);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, "C1307", "1", "Test code1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();

			transportMean.TruckKind = "";
			ValidateInfo(transportMean.TruckKindInfo);
			AssertHasMessageErrorContaining("Truck Type - When no value entered", transportMean.TruckKindInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("Truck Type - When no list entry", transportMean.TruckKindInfo, ListValidation.InvalidCodeMessageError.ToString());

			transportMean.TruckKind = "2";
			ValidateInfo(transportMean.TruckKindInfo);
			AssertNoMessageErrorContaining("Truck Type - When a value entered", transportMean.TruckKindInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("Truck Type - When invalid list entry", transportMean.TruckKindInfo, ListValidation.InvalidCodeMessageError.ToString());

			transportMean.TruckKind = "1";
			ValidateInfo(transportMean.TruckKindInfo);
			AssertNoMessageErrorContaining("Truck Type - When a value entered", transportMean.TruckKindInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("Truck Type - When valid list entry", transportMean.TruckKindInfo, ListValidation.InvalidCodeMessageError.ToString());

			asycudaManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			transportMean.TruckKind = "";
			ValidateInfo(transportMean.TruckKindInfo);
			AssertNoMessageErrorContaining("Truck Type - When Transport Mode is not Road('ROA') and  no value entered", transportMean.VehicleCountryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();

			asycudaManifestHeader = Factory.New<AsycudaManifestHeader>();
			asycudaManifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Road;
			asycudaManifestHeader.AMA_RN_NKCountry = "IL";
			transportMean = (TransportMean)asycudaManifestHeader.TransportMeans.AddNew();
		}

		void ValidateInfo(ZPropertyInfo info)
		{
			((IBusinessObjectInternals)info.BizObj).Validate(info);
		}

		AsycudaManifestHeader asycudaManifestHeader;
		TransportMean transportMean;
	}
}
