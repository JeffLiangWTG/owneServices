using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAMA_Nature()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			header.AMA_Nature = ZString.Empty;
			AssertHasMessageError(header.AMA_NatureInfo, "You have not entered a Manifest Nature.");

			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			AssertNoNotifications(header.AMA_NatureInfo);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			AssertNoNotifications(header.AMA_NatureInfo);

			header.AMA_Nature = ShipmentTypeList.Codes.Transit24;
			AssertNoNotifications(header.AMA_NatureInfo);

			header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
			AssertNoNotifications(header.AMA_NatureInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			header.AMA_Nature = ShipmentTypeList.Codes.Transit24;
			AssertHasMessageError(header.AMA_NatureInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckAMA_TransshipmentType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			header.AMA_TransshipmentType = "REX";
			AssertNoNotifications(header.AMA_TransshipmentTypeInfo);

			header.AMA_TransshipmentType = "TSS";
			AssertHasMessageError(header.AMA_TransshipmentTypeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckAMA_CustomsOffice()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_CustomsOffice = ZString.Empty;

			AssertNoNotifications(header.AMA_CustomsOfficeInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_CustomsOffice = ZString.Empty;

			AssertNoNotifications(header.AMA_CustomsOfficeInfo);
		}
	}
}
