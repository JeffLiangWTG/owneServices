using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalNCTS5EndorsementWrapperTest : WrapperHelperTest<ArrivalNCTS5EndorsementWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if enRouteIncident is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "enRouteIncident"), () => GetWrapper(null));
			});
		}

		public void TestDate()
		{
			enRouteIncident.BN_EndorsementDate = ZDateTime.BrettsBirthday;
			AssertEquals("Expected filled BN_EndorsementDate", ZDateTime.BrettsBirthday, wrapper.Date);
		}

		public void TestAuthority()
		{
			enRouteIncident.BN_EndorsementAuthority = "AUTORITY";
			AssertEquals("Expected filled BN_EndorsementAuthority", "AUTORITY", wrapper.Authority);
		}

		public void TestPlace()
		{
			enRouteIncident.BN_EndorsementPlace = "PLACE";
			AssertEquals("Expected filled BN_EndorsementPlace", "PLACE", wrapper.Place);
		}

		public void TestCountry()
		{
			enRouteIncident.BN_EndorsementCountryCode = "CC";
			AssertEquals("Expected filled BN_EndorsementCountryCode", "CC", wrapper.Country);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			enRouteIncident = nctsHeader.EnRouteIncidents.AddNew();

			wrapper = GetWrapper(enRouteIncident);
		}

		ArrivalNCTS5EndorsementWrapper wrapper;
		EnRouteIncident enRouteIncident;

		ArrivalNCTS5EndorsementWrapper GetWrapper(EnRouteIncident enRouteIncident) => new ArrivalNCTS5EndorsementWrapper(enRouteIncident);

		protected override ArrivalNCTS5EndorsementWrapper GetProvider() => wrapper;
	}
}
