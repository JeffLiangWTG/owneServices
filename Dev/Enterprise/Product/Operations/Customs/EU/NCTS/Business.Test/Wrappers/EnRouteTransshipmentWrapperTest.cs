using System;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class EnRouteTransshipmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<EnRouteTransshipmentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new EnRouteTransshipmentWrapper(null));
		}

		public void TestEndorsementDate()
		{
			transshipment.BN_EndorsementDate = new ZDateTime(2020, 4, 14);
			AssertEquals("20200414", wrapper.EndorsementDate);
		}

		public void TestEndorsementAuthority()
		{
			transshipment.BN_EndorsementAuthority = "POLICE";
			AssertEquals("POLICE", wrapper.EndorsementAuthority);
		}

		public void TestEndorsementAuthorityLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.EndorsementAuthorityLanguage);
		}

		public void TestEndorsementPlace()
		{
			transshipment.BN_EndorsementPlace = "LYON";
			AssertEquals("LYON", wrapper.EndorsementPlace);
		}

		public void TestEndorsementPlaceLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.EndorsementPlaceLanguage);
		}

		public void TestEndorsementCountry()
		{
			transshipment.BN_EndorsementCountryCode = Core.Constants.CountryCodes.Poland;
			AssertEquals(Core.Constants.CountryCodes.Poland, wrapper.EndorsementCountry);
		}

		public void TestNewTransportID()
		{
			transshipment.BN_TransportID = "28 TY 89";
			AssertEquals("28 TY 89", wrapper.NewTransportID);
		}

		public void TestNewTransportIDLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.NewTransportIDLanguage);
		}

		public void TestNewTransportCountry()
		{
			transshipment.BN_TransportCountryCode = Core.Constants.CountryCodes.Denmark;
			AssertEquals(Core.Constants.CountryCodes.Denmark, wrapper.NewTransportCountry);
		}

		public void TestContainerNumbers()
		{
			var container1 = transshipment.Containers.AddNew();
			container1.BC_ContainerNum = "CONT123456";
			var container2 = transshipment.Containers.AddNew();
			container2.BC_ContainerNum = "CONT123456";
			var container3 = transshipment.Containers.AddNew();
			container3.BC_ContainerNum = "CONT987654";
			var containerNumbers = wrapper.ContainerNumbers;
			AssertContainsExactElementsInAnyOrder("Distinct Container Numbers", new[] { "CONT987654", "CONT123456" }, containerNumbers);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			transshipment = header.EnRouteTransshipments.AddNew();
			wrapper = new EnRouteTransshipmentWrapper(transshipment);
		}
		EnRouteTransshipment transshipment;
		EnRouteTransshipmentWrapper wrapper;

		protected override EnRouteTransshipmentWrapper GetProvider() => wrapper;
	}
}
