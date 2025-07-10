using System;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalFormDataTransshipmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<ArrivalFormDataTransshipmentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Throw exception if transshipment is null", () => new ArrivalFormDataTransshipmentWrapper(null));
		}

		public void TestFormDate()
		{
			transshipment.BN_EndorsementDate = ZDateTime.BrettsBirthday;
			AssertEquals("Expected filled FormDate", ZDateTime.BrettsBirthday, wrapper.FormDate);
		}

		public void TestFormAuthority()
		{
			transshipment.BN_EndorsementAuthority = "AH";
			AssertEquals("Expected filled FormAuthority", "AH", wrapper.FormAuthority);
		}

		public void TestFormAuthorityLanguage()
		{
			AssertEquals("Expected empty FormAuthorityLanguage", ZString.Empty, wrapper.FormAuthorityLanguage);
		}

		public void TestFormLocation()
		{
			transshipment.BN_EndorsementPlace = "AH";
			AssertEquals("Expected filled FormLocation", "AH", wrapper.FormLocation);
		}

		public void TestFormLocationLanguage()
		{
			AssertEquals("Expected empty FormLocationLanguage", ZString.Empty, wrapper.FormLocationLanguage);
		}

		public void TestFormCountry()
		{
			transshipment.BN_EndorsementCountryCode = "AH";
			AssertEquals("Expected filled FormCountry", "AH", wrapper.FormCountry);
		}

		public void TestFormText()
		{
			transshipment.BN_TransportID = "AH";
			AssertEquals("Expected filled FormText", "AH", wrapper.FormText);
		}

		public void TestFormTextLanguage()
		{
			AssertEquals("Expected empty FormTextLanguage", ZString.Empty, wrapper.FormTextLanguage);
		}

		protected override void SetUp()
		{
			base.SetUp();

			transshipment = Factory.New<EnRouteTransshipment>();
			wrapper = new ArrivalFormDataTransshipmentWrapper(transshipment);
		}
		EnRouteTransshipment transshipment;
		ArrivalFormDataTransshipmentWrapper wrapper;

		protected override ArrivalFormDataTransshipmentWrapper GetProvider() => wrapper;
	}
}
