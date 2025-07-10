using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	public class RouteEntryValidationTest : Customs.Business.Testing.CusCodeDataValidationTest
	{
		public void TestCheckCY_Data()
		{
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.EuropeanUnion;
			header.AMA_TransportMode = "SEA";
			var port = header.Itinerary.AddNew();
			port.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(port.CY_DataInfo, "You have not entered a Country Code.");

			port.CY_Data = "XXXXX";
			port.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(port.CY_DataInfo, ListValidation.InvalidCodeMessageError);

			port.CY_Data = "AU";
			AssertNoMessageErrors(port.CY_DataInfo);
		}

		public void TestValidateFirstRoutingCountry()
		{
			const string errorMessage = "The first Country of Routing does not match the Port of Loading Country.";

			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_TransportMode = "ROA";
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F40;
			header.AMA_RL_NKPortOfLoading = "AUSYD";

			var port = header.Itinerary.AddNew();
			port.CY_Data = "DE";
			port.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(port, errorMessage);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
			port.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(port, errorMessage);

			port.CY_Data = "AU";
			port.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(port, errorMessage);

			header.AMA_TransportMode = "RAI";
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F41;
			port.CY_Data = "DE";
			port.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(port, errorMessage);
		}

		public void TestValidateLastRoutingCountry()
		{
			const string errorMessage = "The last Country of Routing does not match the Discharge Port Country.";

			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_TransportMode = "ROA";
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F40;
			header.AMA_RL_NKPortOfDischarge = "AUSYD";
			header.AMA_CustomsOffice = "DE00000";
			header.Itinerary.AddNew();

			var port2 = header.Itinerary.AddNew();
			port2.CY_Data = "DE";
			port2.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(port2, errorMessage);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
			port2.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(port2, errorMessage);

			port2.CY_Data = "AU";
			port2.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(port2, errorMessage);

			header.AMA_TransportMode = "RAI";
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F41;
			port2.CY_Data = "DE";
			port2.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(port2, errorMessage);
		}

		public void TestValidateFirstEuCountry()
		{
			const string errorMessage = "The first EU country of the routing does not match the Customs Office of First Entry.";

			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_TransportMode = "ROA";
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F40;
			header.AMA_CustomsOffice = "DE00000";

			var port1 = header.Itinerary.AddNew();
			port1.CY_Data = "CH";
			var port2 = header.Itinerary.AddNew();
			port2.CY_Data = "BE";
			port2.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(port2, errorMessage);

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
			port2.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(port2, errorMessage);

			port2.CY_Data = "DE";
			port2.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(port2, errorMessage);

			header.AMA_TransportMode = "RAI";
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F41;
			port2.CY_Data = "DE";
			port2.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(port2, errorMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
		}

		AsycudaManifestHeader header;
	}
}
