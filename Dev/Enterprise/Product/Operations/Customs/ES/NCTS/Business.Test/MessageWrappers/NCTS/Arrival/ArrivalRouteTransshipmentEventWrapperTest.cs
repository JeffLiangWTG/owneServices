using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalRouteTransshipmentEventWrapperTest : Customs.Business.Testing.DataProviderTestCase<ArrivalRouteTransshipmentEventWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Throw exception if transshipment is null", () => new ArrivalRouteTransshipmentEventWrapper(null));
		}

		public void TestEventPlace()
		{
			transshipment.BN_EventPlace = "AH";
			AssertEquals("Expected filled EventPlace", "AH", wrapper.EventPlace);
		}

		public void TestEventPlaceLanguage()
		{
			AssertEquals("Expected empty EventPlaceLanguage", ZString.Empty, wrapper.EventPlaceLanguage);
		}

		public void TestEventCountry()
		{
			transshipment.BN_EventCountryCode = "AH";
			AssertEquals("Expected filled EventCountry", "AH", wrapper.EventCountry);
		}

		public void TestIncidentInEvent()
		{
			AssertEquals("Always false IncidentInEvent", false, wrapper.IncidentInEvent);
		}

		public void TestIncidentFormData()
		{
			AssertNull("Expected null IncidentFormData", wrapper.IncidentFormData);
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
			transshipment.BN_TransportCountryCode = "AH";
			AssertEquals("Expected filled NewTransportNationality", "AH", wrapper.NewTransportNationality);
		}

		public void TestTransferFormData()
		{
			AssertNotNull("Not null IncidentFormData", wrapper.TransferFormData);
		}

		public void TestNewContainerIDs()
		{
			transshipment.Containers.AddNew().BC_ContainerNum = "1";
			transshipment.Containers.AddNew().BC_ContainerNum = "2";
			transshipment.Containers.AddNew().BC_ContainerNum = "3";

			var newContainerIDs = wrapper.NewContainerIDs;

			AssertEquals("Expected filled NewContainerIDs", new List<ZString> { "1", "2", "3" }.ToString(), newContainerIDs.ToList().ToString());
			AssertSame("Cached NewContainerIDs", wrapper.NewContainerIDs, newContainerIDs);
		}

		protected override void SetUp()
		{
			base.SetUp();

			transshipment = Factory.New<EnRouteTransshipment>();
			wrapper = new ArrivalRouteTransshipmentEventWrapper(transshipment);
		}
		EnRouteTransshipment transshipment;
		ArrivalRouteTransshipmentEventWrapper wrapper;

		protected override ArrivalRouteTransshipmentEventWrapper GetProvider() => wrapper;
	}
}
