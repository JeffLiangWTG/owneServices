using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CustomsVoyageDestinationWrapper))]
	sealed class CustomsVoyageDestinationWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFactory()
		{
			AssertEquals("Factory", Factory, Wrapper.Factory);
		}

		public void TestImpendingArrivalStatus()
		{
			AssertEquals("Default Code", CMRBaseStatuses.Codes.NotSent, Wrapper.ActualArrivalStatus.Code);
		}

		public void TestMessages()
		{
			AssertEquals("Messages", Destination.Messages, Wrapper.Messages);
		}

		public void TestFlightNo()
		{
			Voyage.JV_VoyageFlight = "QF123";
			AssertEquals("FlightNo", "QF123", Wrapper.FlightNo);
		}

		public void TestEstimatedArrivalDate()
		{
			Destination.JB_E_ARV = new ZDateTime(2005, 6, 5, 17, 39, 0);
			AssertEquals("EstimatedArrivalDate", new ZDateTime(2005, 6, 5, 17, 39, 0), Wrapper.EstimatedArrivalDate);
		}

		public void TestActualArrivalDateTime()
		{
			Destination.JB_RL_NKPortOfDischarge = "AUPER";
			Destination.JB_A_ARV = new ZDateTime(2005, 6, 5, 17, 39, 0);
			AssertEquals("ActualArrivalDateTime", new ZDateTime(2005, 6, 5, 9, 39, 0), Wrapper.ActualArrivalDateTimeUTC);
			Destination.JB_RL_NKPortOfDischarge = ZString.Empty;
			AssertEquals("ActualArrivalDateTime", ZDateTime.Empty, Wrapper.ActualArrivalDateTimeUTC);
		}

		public void TestPortOfArrival()
		{
			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			AssertEquals("PortOfArrival", "AUSYD", Wrapper.PortOfArrival);
		}

		public void TestDateTimeOfDepartureUTC()
		{
			var origin = Voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "USLAX";
			origin.JA_A_DEP = new ZDateTime(2005, 6, 5, 16, 28, 0);
			AssertEquals("DateTimeOfDeparture", new ZDateTime(2005, 6, 5, 23, 28, 0), Wrapper.DateTimeOfDepartureUTC);
			origin.JA_RL_NKPortOfLoading = ZString.Empty;
			AssertEquals("DateTimeOfDeparture", ZDateTime.Empty, Wrapper.DateTimeOfDepartureUTC);
		}

		public void TestLastOverseasPortOfDeparture()
		{
			var origin = Voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "USLAX";
			origin.JA_A_DEP = new ZDateTime(2005, 6, 5, 16, 28, 0);
			AssertEquals("LastOverseasPortOfDeparture", "USLAX", Wrapper.LastOverseasPortOfDeparture);
		}

		public void TestEstimatedDateTimeOfArrival()
		{
			Destination.JB_E_ARV = new ZDateTime(2005, 6, 5, 16, 52, 0);
			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			AssertEquals("EstimatedDateTimeOfArrival", new ZDateTime(2005, 6, 5, 6, 52, 0), Wrapper.EstimatedDateTimeOfArrivalUTC);
			Destination.JB_RL_NKPortOfDischarge = ZString.Empty;
			AssertEquals("EstimatedDateTimeOfArrival", ZDateTime.Empty, Wrapper.EstimatedDateTimeOfArrivalUTC);
		}

		public void TestDischargeCTOEstablishmentID()
		{
			var header = Factory.New<OrgHeader>();
			Destination.JB_OA_ArrivalCTOAddress = header.Addresses[0].PK;
			Destination.ArrivalCTOAddress.LocalControlledPremisesID = "12345";
			AssertEquals("DischargeCTOEstablishmentID", "12345", Wrapper.DischargeCTOEstablishmentID);
		}

		public void TestStevedoreID()
		{
			AssertEquals("StevedoreID", ZString.Empty, Wrapper.StevedoreID);
		}

		public void TestDischargeIndicator()
		{
			AssertEquals("DischargeIndicator", false, Wrapper.DischargeIndicator);
			var header = Factory.New<OrgHeader>();
			Destination.JB_OA_ArrivalCTOAddress = header.Addresses[0].PK;
			Destination.ArrivalCTOAddress.LocalControlledPremisesID = "12345";
			AssertEquals("DischargeIndicator", true, Wrapper.DischargeIndicator);
		}

		public void TestStatusNeedsRecalculation()
		{
			AssertEquals("StatusNeedsRecalculation", false, ((IStatusNeedsRecalculationProvider)Wrapper).StatusNeedsRecalculation);
			Wrapper.Messages.AddNew().HasChanges = true;
			AssertEquals("StatusNeedsRecalculation", true, ((IStatusNeedsRecalculationProvider)Wrapper).StatusNeedsRecalculation);
		}

		public void TestNullWRapper()
		{
			var wrapper = new CustomsVoyageDestinationWrapper(VoyageWrapper, null);
			AssertNotNull(wrapper.Messages);
		}

		public void TestDetails()
		{
			Voyage.JV_VoyageFlight = "QF123";
			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			AssertEquals("Details", "Flight: QF123\r\nPort: AUSYD\r\n", ((ICMRMessageRespondee)Wrapper).Details);
		}

		public void TestShortDescription()
		{
			Voyage.JV_VoyageFlight = "QF123";
			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			AssertEquals("ShortDescription", "Flight: QF123 Port: AUSYD", ((ICMRMessageRespondee)Wrapper).ShortDescription);
		}

		public void TestLoad()
		{
			Wrapper.Factory.Save();
			AssertNotNull("Load", CustomsVoyageDestinationWrapper.Load(new BusinessObjectFactory(), Wrapper.Destination.PK));
		}

		protected override BusinessObject GetNewBusinessObject() => Wrapper;

		CustomsVoyageDestinationWrapper wrapper;
		CustomsVoyageDestinationWrapper Wrapper => wrapper ?? (wrapper = new CustomsVoyageDestinationWrapper(VoyageWrapper, Destination));

		CustomsJobVoyageWrapper voyageWrapper;
		CustomsJobVoyageWrapper VoyageWrapper => voyageWrapper ?? (voyageWrapper = new CustomsJobVoyageWrapper(Voyage));

		VoyageDestination destination;
		VoyageDestination Destination => destination ?? (destination = Voyage.Destinations.AddNew());

		JobVoyage voyage;
		JobVoyage Voyage => voyage ?? (voyage = Factory.New<JobVoyage>());
	}
}
