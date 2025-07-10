using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationDVDLocationOfGoodsWrapperTest : WrapperHelperTest<DeclarationDVDLocationOfGoodsWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if entryInstruction is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryInstruction"), () => new DeclarationDVDLocationOfGoodsWrapper(null));
		}

		public void TestLocationCountry()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Type = "B";
				goodsLocation.Address.E2_RN_NKCountryCode = "GB";
				AssertEquals("Expected filled LocationCountry with ES when Type is B", "ES", wrapper.LocationCountry);

				goodsLocation.CGL_Type = "A";
				AssertEquals("Expected filled LocationCountry with declared code when Type is not B", "GB", wrapper.LocationCountry);
			});
		}

		public void TestLocationType()
		{
			goodsLocation.CGL_Type = "A";
			AssertEquals("Expected filled LocationType", "A", wrapper.LocationType);
		}

		public void TestLocationQualifier()
		{
			goodsLocation.CGL_Qualifier = "Y";
			AssertEquals("Expected filled LocationQualifier", "Y", wrapper.LocationQualifier);
		}

		public void TestLocationId()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = "Y";
				goodsLocation.Address.AuthorisationNumber = "location id";
				AssertEquals("Expected filled LocationId when qualifier is Y", "location id", wrapper.LocationId);

				goodsLocation.CGL_Qualifier = "Z";
				AssertEquals("Expected empty LocationId when qualifier is not Y", ZString.Empty, wrapper.LocationId);
			});
		}

		public void TestLocationAdditionalId()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = "Y";
				goodsLocation.CGL_AdditionalIdentifier = "Additional id";
				AssertEquals("Expected filled LocationAdditionalId when qualifier is Y", "Additional id", wrapper.LocationAdditionalId);

				goodsLocation.CGL_Qualifier = "Z";
				AssertEquals("Expected empty LocationAdditionalId when qualifier is not Y", ZString.Empty, wrapper.LocationAdditionalId);
			});
		}

		public void TestLocationAddress()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = "Z";
				goodsLocation.Address.E2_Address1 = "Street Name";
				AssertEquals("Expected filled LocationAddress when qualifier is Z", "Street Name", wrapper.LocationAddress);

				goodsLocation.CGL_Qualifier = "Y";
				AssertEquals("Expected empty LocationAddress wwhen qualifier is not Z", ZString.Empty, wrapper.LocationAddress);
			});
		}

		public void TestLocationCity()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = "Z";
				goodsLocation.Address.E2_City = "City";
				AssertEquals("Expected filled LocationCity when qualifier is Z", "City", wrapper.LocationCity);

				goodsLocation.CGL_Qualifier = "Y";
				AssertEquals("Expected empty LocationCity wwhen qualifier is not Z", ZString.Empty, wrapper.LocationCity);
			});
		}

		public void TestLocationPostCode()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = "Z";
				goodsLocation.Address.E2_Postcode = "2801";
				AssertEquals("Expected filled LocationPostCode when qualifier is Z", "2801", wrapper.LocationPostCode);

				goodsLocation.CGL_Qualifier = "Y";
				AssertEquals("Expected empty LocationPostCode wwhen qualifier is not Z", ZString.Empty, wrapper.LocationPostCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			entryInstruction = Factory.New<CusEntryInstruction>();
			goodsLocation = entryInstruction.GoodsLocation;

			wrapper = new DeclarationDVDLocationOfGoodsWrapper(entryInstruction);
		}
		CusEntryInstruction entryInstruction;
		EU.Business.CusGoodsLocation goodsLocation;
		DeclarationDVDLocationOfGoodsWrapper wrapper;

		protected override DeclarationDVDLocationOfGoodsWrapper GetProvider() => wrapper;
	}
}
