using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CustomsJobVoyageWrapperValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAutoValidationType()
		{
			AssertEquals(typeof(CustomsJobVoyageWrapperValidation), Wrapper.Validation.AutoValidationType);
		}

		public void TestValidateAll()
		{
			Wrapper.Validation.ValidateAll();
			AssertHasNotifications(Wrapper.LastOverseasPortOfDepartureInfo);
			AssertHasNotifications(Wrapper.PortOfFirstArrivalInfo);
			AssertHasNotifications(Wrapper.DateTimeOfDepartureUTCInfo);
			AssertHasNotifications(Wrapper.FlightNoInfo);
		}

		public void TestValidateLastOverseasPortOfDeparture()
		{
			Wrapper.Validation.ValidateLastOverseasPortOfDeparture();
			AssertHasMessageErrors("by default", Wrapper.LastOverseasPortOfDepartureInfo);

			SetupTwoOriginsAndTwoDestinations();
			Wrapper.Validation.ValidateLastOverseasPortOfDeparture();
			AssertNoNotifications("when set", Wrapper.LastOverseasPortOfDepartureInfo);
		}

		public void TestValidatePortOfFirstArrival()
		{
			Wrapper.Validation.ValidatePortOfFirstArrival();
			AssertHasMessageErrors("by default", Wrapper.PortOfFirstArrivalInfo);

			SetupTwoOriginsAndTwoDestinations();
			Wrapper.Validation.ValidatePortOfFirstArrival();
			AssertNoNotifications("when set", Wrapper.PortOfFirstArrivalInfo);
		}

		public void TestValidateDateTimeOfDepartureUTC()
		{
			Wrapper.Validation.ValidateDateTimeOfDepartureUTC();
			AssertHasMessageErrors("by default", Wrapper.DateTimeOfDepartureUTCInfo);

			SetupTwoOriginsAndTwoDestinations();
			Wrapper.Validation.ValidateDateTimeOfDepartureUTC();
			AssertNoNotifications("when set", Wrapper.DateTimeOfDepartureUTCInfo);
		}

		public void TestValidateFlightNo()
		{
			Wrapper.Validation.ValidateFlightNo();
			AssertHasMessageErrors("by default", Wrapper.FlightNoInfo);

			Wrapper.Voyage.JV_VoyageFlight = "QF123";
			Wrapper.Validation.ValidateFlightNo();
			AssertNoNotifications("when set", Wrapper.FlightNoInfo);

			Voyage.JV_VoyageFlight = "cheese";
			Voyage.Validation.ValidateJV_VoyageFlight();
			Wrapper.Validation.ValidateFlightNo();
			AssertHasMessageErrors("when invalid", Wrapper.FlightNoInfo);
		}

		#region Implementation

		void SetupTwoOriginsAndTwoDestinations()
		{
			VoyageDestination destination2 = Voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "AUMEL";
			destination2.JB_E_ARV = ZDateTime.Now.AddHours(3);
			VoyageDestination destination1 = Voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUSYD";
			destination1.JB_E_ARV = ZDateTime.Now;

			VoyageOrigin origin2 = Voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "USLAX";
			origin2.JA_A_DEP = new ZDateTime(2005, 6, 5, 16, 28, 0);
			VoyageOrigin origin1 = Voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "USNYC";
			origin1.JA_A_DEP = new ZDateTime(2005, 6, 5, 12, 28, 0);
		}

		CustomsJobVoyageWrapper wrapper;
		CustomsJobVoyageWrapper Wrapper
		{
			get
			{
				if (wrapper == null)
				{
					wrapper = new CustomsJobVoyageWrapper(Voyage);
				}
				return wrapper;
			}
		}

		JobVoyage voyage;
		JobVoyage Voyage
		{
			get
			{
				if (voyage == null)
				{
					voyage = Factory.New<JobVoyage>();
					voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
				}
				return voyage;
			}
		}

		#endregion
	}
}
