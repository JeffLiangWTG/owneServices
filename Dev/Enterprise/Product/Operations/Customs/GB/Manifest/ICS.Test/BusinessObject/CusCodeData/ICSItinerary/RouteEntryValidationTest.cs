using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.ICS.Business.Testing
{
	public class RouteEntryValidationTest : Customs.Business.Testing.CusCodeDataValidationTest
	{
		public void TestCheckCY_Data()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			header.AMA_TransportMode = "SEA";
			var port = header.Itinerary.AddNew();
			port.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(port.CY_DataInfo, "You have not entered a Country/Region Code.");

			port.CY_Data = "XXXXX";
			port.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(port.CY_DataInfo, ListValidation.InvalidCodeMessageError);

			port.CY_Data = "AU";
			AssertNoMessageErrors(port.CY_DataInfo);
		}
	}
}
