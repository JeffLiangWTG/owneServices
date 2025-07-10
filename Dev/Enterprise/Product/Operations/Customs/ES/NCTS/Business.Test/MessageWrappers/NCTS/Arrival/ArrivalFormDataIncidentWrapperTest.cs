using System;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalFormDataIncidentWrapperTest : Customs.Business.Testing.DataProviderTestCase<ArrivalFormDataIncidentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Throw exception if incident is null", () => new ArrivalFormDataIncidentWrapper(null));
		}

		public void TestFormDate()
		{
			incident.BN_EndorsementDate = ZDateTime.BrettsBirthday;
			AssertEquals("Expected filled FormDate", ZDateTime.BrettsBirthday, wrapper.FormDate);
		}

		public void TestFormAuthority()
		{
			incident.BN_EndorsementAuthority = "AH";
			AssertEquals("Expected filled FormAuthority", "AH", wrapper.FormAuthority);
		}

		public void TestFormAuthorityLanguage()
		{
			AssertEquals("Expected empty FormAuthorityLanguage", ZString.Empty, wrapper.FormAuthorityLanguage);
		}

		public void TestFormLocation()
		{
			incident.BN_EndorsementPlace = "AH";
			AssertEquals("Expected filled FormLocation", "AH", wrapper.FormLocation);
		}

		public void TestFormLocationLanguage()
		{
			AssertEquals("Expected empty FormLocationLanguage", ZString.Empty, wrapper.FormLocationLanguage);
		}

		public void TestFormCountry()
		{
			incident.BN_EndorsementCountryCode = "AH";
			AssertEquals("Expected filled FormCountry", "AH", wrapper.FormCountry);
		}

		public void TestFormText()
		{
			incident.BN_Information = "AH";
			AssertEquals("Expected filled FormText", "AH", wrapper.FormText);
		}

		public void TestFormTextLanguage()
		{
			AssertEquals("Expected empty FormTextLanguage", ZString.Empty, wrapper.FormTextLanguage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			incident = header.EnRouteIncidents.AddNew();
			wrapper = new ArrivalFormDataIncidentWrapper(incident);
		}
		EnRouteIncident incident;
		ArrivalFormDataIncidentWrapper wrapper;

		protected override ArrivalFormDataIncidentWrapper GetProvider() => wrapper;
	}
}
