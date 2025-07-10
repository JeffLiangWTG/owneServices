using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalNCTS5LocationWrapperTest : WrapperHelperTest<ArrivalNCTS5LocationWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if enRouteIncident is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "enRouteIncident"), () => GetWrapper(null));
			});
		}

		public void TestQualifier()
		{
			enRouteIncident.GoodsLocation.CGL_Qualifier = "U";
			wrapper = GetWrapper(enRouteIncident);
			AssertEquals("Expected filled QualifierOfIdentification", "U", wrapper.Qualifier);
		}

		public void TestUNLocode()
		{
			CombineAssertions(() =>
			{
				enRouteIncident.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				enRouteIncident.GoodsLocation.Unlocode = "ESBCN";
				wrapper = GetWrapper(enRouteIncident);
				AssertEquals("Expected filled Unlocode", "ESBCN", wrapper.UNLocode);
				enRouteIncident.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				wrapper = GetWrapper(enRouteIncident);
				AssertEquals("Expected filled Unlocode qhere Qualifier not U", ZString.Empty, wrapper.UNLocode);
			});
		}

		public void TestCountry()
		{
			enRouteIncident.BN_EventCountryCode = "ES";
			wrapper = GetWrapper(enRouteIncident);
			AssertEquals("Expected filled Unlocode", "ES", wrapper.Country);
		}

		public void TestGNSS()
		{
			enRouteIncident.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
			wrapper = GetWrapper(enRouteIncident);
			var gnss = wrapper.GNSS;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled GNSS", gnss);
				AssertSame("Cached GNSS", wrapper.GNSS, gnss);

				enRouteIncident.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				wrapper = GetWrapper(enRouteIncident);
				AssertNull("Expected filled GNSS where qualifier not W", wrapper.GNSS);
			});
		}

		public void TestAddress()
		{
			enRouteIncident.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			enRouteIncident.GoodsLocation.Address.E2_Address1AndE2_Address2 = "AAAAA2";
			enRouteIncident.GoodsLocation.Address.City = "Barcelona";
			enRouteIncident.GoodsLocation.Address.Postcode = "08140";
			wrapper = GetWrapper(enRouteIncident);
			var address = wrapper.Address;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Address", address);
				AssertSame("Cached Address", wrapper.Address, address);
				AssertEquals("Expected filled StreetAndNumber", "AAAAA2", wrapper.Address.StreetAndNumber);
				AssertEquals("Expected filled City", "Barcelona", wrapper.Address.City);
				AssertEquals("Expected filled PostCode", "08140", wrapper.Address.PostCode);
				enRouteIncident.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				wrapper = GetWrapper(enRouteIncident);
				AssertNull("Expected filled Address where qualifier not Z", wrapper.Address);
			});
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
		ArrivalNCTS5LocationWrapper wrapper;
		EnRouteIncident enRouteIncident;

		ArrivalNCTS5LocationWrapper GetWrapper(EnRouteIncident enRouteIncident) => new ArrivalNCTS5LocationWrapper(enRouteIncident);

		protected override ArrivalNCTS5LocationWrapper GetProvider() => wrapper;
	}
}
