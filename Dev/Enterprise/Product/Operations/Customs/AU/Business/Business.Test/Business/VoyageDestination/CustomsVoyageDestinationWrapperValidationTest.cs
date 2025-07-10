using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CustomsVoyageDestinationWrapperValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll()
		{
			Wrapper.Validation.ValidateAll();

			AssertHasNotifications(Wrapper.EstimatedDateTimeOfArrivalUTCInfo);
			AssertHasNotifications(Wrapper.PortOfArrivalInfo);
			AssertHasNotifications(Wrapper.DischargeCTOEstablishmentIDInfo);
		}

		public void TestActualArrivalDateTimee()
		{
			Wrapper.Validation.ValidateActualArrivalDateTimeUTC();
			AssertNoNotifications(Wrapper.ActualArrivalDateTimeUTCInfo);
		}

		public void TestEstimatedDateTimeOfArrival()
		{
			Wrapper.Validation.ValidateEstimatedDateTimeOfArrivalUTC();
			AssertHasMessageErrors(Wrapper.EstimatedDateTimeOfArrivalUTCInfo);

			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			Destination.JB_E_ARV = ZDateTime.Now;
			Wrapper.Validation.ValidateEstimatedDateTimeOfArrivalUTC();
			AssertNoNotifications(Wrapper.EstimatedDateTimeOfArrivalUTCInfo);
		}

		public void TestPortOfArrival()
		{
			Wrapper.Validation.ValidatePortOfArrival();
			AssertHasMessageErrors(Wrapper.PortOfArrivalInfo);

			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			Wrapper.Validation.ValidatePortOfArrival();
			AssertNoNotifications(Wrapper.PortOfArrivalInfo);

			Destination.JB_RL_NKPortOfDischarge = "crap";
			Wrapper.Validation.ValidatePortOfArrival();
			AssertHasMessageErrors(Wrapper.PortOfArrivalInfo);
		}

		public void ValidationDischargeCTOEstablishmentID()
		{
			Wrapper.Validation.ValidateDischargeCTOEstablishmentID();
			AssertHasMessageErrors(Wrapper.DischargeCTOEstablishmentIDInfo);

			var header = Factory.New<OrgHeader>();
			Destination.JB_OA_ArrivalCTOAddress = header.Addresses[0].PK;
			Destination.ArrivalCTOAddress.LocalControlledPremisesID = "12345";
			Wrapper.Validation.ValidateDischargeCTOEstablishmentID();
			AssertNoNotifications(Wrapper.DischargeCTOEstablishmentIDInfo);
		}

		CustomsVoyageDestinationWrapper wrapper;
		CustomsVoyageDestinationWrapper Wrapper
		{
			get
			{
				if (wrapper == null)
				{
					var voyageWrapper = new CustomsJobVoyageWrapper(Voyage);
					wrapper = new CustomsVoyageDestinationWrapper(voyageWrapper, Destination);
				}
				return wrapper;
			}
		}

		VoyageDestination destination;
		VoyageDestination Destination => destination ?? (destination = Voyage.Destinations.AddNew());

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
	}
}
