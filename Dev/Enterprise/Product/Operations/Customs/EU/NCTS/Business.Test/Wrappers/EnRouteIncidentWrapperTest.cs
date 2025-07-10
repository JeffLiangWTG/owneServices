using System;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class EnRouteIncidentWrapperTest : Customs.Business.Testing.DataProviderTestCase<EnRouteIncidentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new EnRouteIncidentWrapper(null));
		}

		public void TestIncidentFlag()
		{
			header.BH_ExportFlag = EventFlagList.Codes.Yes;
			AssertEquals("IncidentFlag should be true in case of an export transit", true, wrapper.IncidentFlag);
		}

		public void TestEndorsementDate()
		{
			incident.BN_EndorsementDate = new ZDateTime(2020, 6, 19);
			AssertEquals("20200619", wrapper.EndorsementDate);
		}

		public void TestEndorsementAuthority()
		{
			incident.BN_EndorsementAuthority = "POLICE";
			AssertEquals("POLICE", wrapper.EndorsementAuthority);
		}

		public void TestEndorsementAuthorityLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.EndorsementAuthorityLanguage);
		}

		public void TestEndorsementPlace()
		{
			incident.BN_EndorsementPlace = "LYON";
			AssertEquals("LYON", wrapper.EndorsementPlace);
		}

		public void TestEndorsementPlaceLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.EndorsementPlaceLanguage);
		}

		public void TestEndorsementCountry()
		{
			incident.BN_EndorsementCountryCode = Core.Constants.CountryCodes.Poland;
			AssertEquals(Core.Constants.CountryCodes.Poland, wrapper.EndorsementCountry);
		}

		public void TestIncidentInformation()
		{
			incident.BN_Information = "BLABLA";
			AssertEquals("BLABLA", wrapper.IncidentInformation);
		}

		public void TestIncidentInformationLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.IncidentInformationLanguage);
		}
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			incident = header.EnRouteIncidents.AddNew();
			wrapper = new EnRouteIncidentWrapper(incident);
		}

		NctsHeader header;
		EnRouteIncident incident;
		EnRouteIncidentWrapper wrapper;

		protected override EnRouteIncidentWrapper GetProvider() => wrapper;
	}
}
