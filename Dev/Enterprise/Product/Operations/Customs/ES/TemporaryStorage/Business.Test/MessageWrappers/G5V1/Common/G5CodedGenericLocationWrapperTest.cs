using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class G5CodedGenericLocationWrapperTest : WrapperHelperTest<G5CodedGenericLocationWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if location is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "location"), () => GetWrapper(null));
		}

		public void TestUNLOCOCode()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = "U";
				goodsLocation.Unlocode = "ESMAD";
				AssertEquals("Expected filled UNLOCOCode", "ESMAD", wrapper.UNLOCOCode);

				goodsLocation.CGL_Qualifier = "Y";
				goodsLocation.Unlocode = "ESMAP";
				AssertEquals("Expected empty UNLOCOCode when qualifier is not U (even if the fields are not empty)", ZString.Empty, wrapper.UNLOCOCode);
			});
		}

		public void TestCustomsOffice()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = "V";
				goodsLocation.CGL_CustomsOffice = "ES009999";
				AssertEquals("Expected filled CustomsOffice", "ES009999", wrapper.CustomsOffice);

				goodsLocation.CGL_Qualifier = "Y";
				goodsLocation.CGL_CustomsOffice = "ES009998";
				AssertEquals("Expected empty CustomsOffice when qualifier is not V (even if the fields are not empty)", ZString.Empty, wrapper.CustomsOffice);
			});
		}

		public void TestGPS()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = "W";
				goodsLocation.Address.E2_Latitude = 1234.1234m;
				wrapper = GetWrapper(goodsLocation);
				var gps = wrapper.GPS;
				AssertNotNull("Expected filled GPS", gps);
				AssertSame("Cached GPS", wrapper.GPS, gps);

				goodsLocation.CGL_Qualifier = "Y";
				goodsLocation.Address.E2_Latitude = 1234.1235m;
				wrapper = GetWrapper(goodsLocation);
				AssertNull("Expected empty GPS when qualifier is not W (even if the fields are not empty)", wrapper.GPS);
			});
		}

		public void TestEconomicOperator()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = "X";
				goodsLocation.Address.E2_GovRegNum = "ES12345678A";
				AssertEquals("Expected filled EconomicOperator", "ES12345678A", wrapper.EconomicOperator);

				goodsLocation.CGL_Qualifier = "Y";
				goodsLocation.Address.E2_GovRegNum = "ES12345678B";
				AssertEquals("Expected empty EconomicOperator when qualifier is not X (even if the fields are not empty)", ZString.Empty, wrapper.EconomicOperator);
			});
		}

		public void TestAuthorisationNumber()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = "Y";
				goodsLocation.Address.AuthorisationNumber = "ES08123456";
				AssertEquals("Expected filled AuthorisationNumber when Qualifier = Y", "ES08123456", wrapper.AuthorisationNumber);

				goodsLocation.CGL_Qualifier = "X";
				goodsLocation.Address.AuthorisationNumber = "ES08123457";
				AssertEquals("Expected empty AuthorisationNumber when qualifier is not Y (even if the fields are not empty)", ZString.Empty, wrapper.AuthorisationNumber);
			});
		}

		public void TestAdditionalId()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = "Y";
				goodsLocation.AdditionalIdentifier = "Additional id";
				AssertEquals("Expected filled AdditionalId when qualifier is Y", "Additional id", wrapper.AdditionalId);

				goodsLocation.CGL_Qualifier = "U";
				goodsLocation.AdditionalIdentifier = "Additional id2";
				AssertEquals("Expected empty AdditionalId when qualifier is not X or Y (even if the fields are not empty)", ZString.Empty, wrapper.AdditionalId);

				goodsLocation.CGL_Qualifier = "X";
				goodsLocation.AdditionalIdentifier = "Additional id3";
				AssertEquals("Expected filled AdditionalId when qualifier is X", "Additional id3", wrapper.AdditionalId);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			goodsLocation = Factory.New<CusGoodsLocation>();

			wrapper = GetWrapper(goodsLocation);
		}
		CusGoodsLocation goodsLocation;
		G5CodedGenericLocationWrapper wrapper;

		G5CodedGenericLocationWrapper GetWrapper(CusGoodsLocation location) => new G5CodedGenericLocationWrapper(location);

		protected override G5CodedGenericLocationWrapper GetProvider() => wrapper;
	}
}
