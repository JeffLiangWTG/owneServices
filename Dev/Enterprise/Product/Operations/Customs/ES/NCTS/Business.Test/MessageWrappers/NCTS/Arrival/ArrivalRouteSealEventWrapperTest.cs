using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalRouteSealEventWrapperTest : Customs.Business.Testing.DataProviderTestCase<ArrivalRouteSealEventWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Throw exception if seal is null", () => new ArrivalRouteSealEventWrapper(null));
		}

		public void TestEventPlace()
		{
			seal.BN_EventPlace = "AH";
			AssertEquals("Expected filled EventPlace", "AH", wrapper.EventPlace);
		}

		public void TestEventPlaceLanguage()
		{
			AssertEquals("Expected empty EventPlaceLanguage", ZString.Empty, wrapper.EventPlaceLanguage);
		}

		public void TestEventCountry()
		{
			seal.BN_EventCountryCode = "AH";
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
			seal.BN_NoOfSeals = 255;
			AssertEquals("Expected filled NewSealsInEventNum", "255", wrapper.NewSealsInEventNum);
		}

		public void TestNewSealsInformation()
		{
			seal.SealContainers.AddNew();
			seal.SealContainers.AddNew();
			AssertEquals("Expected 2 NewSealsInformation", 2, wrapper.NewSealsInformation.Count);
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

			seal = Factory.New<EnRouteSeal>();
			wrapper = new ArrivalRouteSealEventWrapper(seal);
		}
		EnRouteSeal seal;
		ArrivalRouteSealEventWrapper wrapper;

		protected override ArrivalRouteSealEventWrapper GetProvider() => wrapper;
	}
}
