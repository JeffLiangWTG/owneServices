using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalRouteIncidentEventWrapperTest : Customs.Business.Testing.DataProviderTestCase<ArrivalRouteIncidentEventWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Throw exception if incident is null", () => new ArrivalRouteIncidentEventWrapper(null));
		}

		public void TestEventPlace()
		{
			incident.BN_EventPlace = "AH";
			AssertEquals("Expected filled EventPlace", "AH", wrapper.EventPlace);
		}

		public void TestEventPlaceLanguage()
		{
			AssertEquals("Expected empty EventPlaceLanguage", ZString.Empty, wrapper.EventPlaceLanguage);
		}

		public void TestEventCountry()
		{
			incident.BN_EventCountryCode = "AH";
			AssertEquals("Expected filled EventCountry", "AH", wrapper.EventCountry);
		}

		public void TestIncidentInEvent()
		{
			Assert("Always true IncidentInEvent", wrapper.IncidentInEvent);
		}

		public void TestIncidentFormData()
		{
			AssertNotNull("Not null IncidentFormData", wrapper.IncidentFormData);
		}

		public void TestNewSealsInEventNum()
		{
			AssertEquals("Expected empty NewSealsInEventNum", ZString.Empty, wrapper.NewSealsInEventNum);
		}

		public void TestNewSealsInformation()
		{
			AssertEquals("Expected empty NewSealsInformation", false, wrapper.NewSealsInformation.Any());
		}

		public void TestNewTransportNationality()
		{
			AssertEquals("Expected empty NewTransportNationality", ZString.Empty, wrapper.NewTransportNationality);
		}

		public void TestTransferFormData()
		{
			AssertNull("Expected null TransferFormData", wrapper.TransferFormData);
		}

		public void TestNewContainerIDs()
		{
			AssertEquals("Expected empty NewContainerIDs", false, wrapper.NewContainerIDs.Any());
		}

		protected override void SetUp()
		{
			base.SetUp();

			incident = Factory.New<EnRouteIncident>();
			wrapper = new ArrivalRouteIncidentEventWrapper(incident);
		}
		EnRouteIncident incident;
		ArrivalRouteIncidentEventWrapper wrapper;

		protected override ArrivalRouteIncidentEventWrapper GetProvider() => wrapper;
	}
}
