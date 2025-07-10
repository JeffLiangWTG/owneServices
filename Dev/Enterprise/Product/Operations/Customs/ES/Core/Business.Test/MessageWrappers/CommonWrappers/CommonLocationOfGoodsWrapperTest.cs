using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing;

public class CommonLocationOfGoodsWrapperTest : WrapperHelperTest<CommonLocationOfGoodsWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown("Constructor Throws Exception if entryInstruction is null", typeof(ArgumentNullException),
			ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryInstruction"), () => new CommonLocationOfGoodsWrapper(null));
	}

	public void TestLocationType()
	{
		goodsLocation.CGL_Type = "A";
		AssertEquals("Expected filled LocationType", "A", wrapper.LocationType);
	}

	public void TestLocationQualifier()
	{
		goodsLocation.CGL_Qualifier = "T";
		AssertEquals("Expected filled LocationQualifier", "T", wrapper.LocationQualifier);
	}

	public void TestLocationId()
	{
		CombineAssertions(() =>
		{
			goodsLocation.CGL_Qualifier = "Y";
			goodsLocation.Address.AuthorisationNumber = "ES08123456";
			AssertEquals("Expected filled LocationId when Qualifier = Y", "ES08123456", wrapper.LocationId);

			goodsLocation.CGL_Type = "J";
			goodsLocation.Address.AuthorisationNumber = "ES08123456789";
			AssertEquals("Expected filled LocationId when Qualifier = Y and autorisation number is longer than 10 characters but Type is not B", "ES08123456789", wrapper.LocationId);

			goodsLocation.CGL_Type = "B";
			AssertEquals("Expected filled LocationId when Qualifier = Y and autorisation number is longer than 10 characters but Type is B and start with ES", "123456789", wrapper.LocationId);

			goodsLocation.Address.AuthorisationNumber = "FR08123456789";
			AssertEquals("Expected filled LocationId when Qualifier = Y and autorisation number is longer than 10 characters but Type is B and not start with ES", "FR08123456789", wrapper.LocationId);

			goodsLocation.CGL_Qualifier = "Z";
			AssertEquals("Expected empty LocationId when qualifier is not Y (even if the fields are not empty)", ZString.Empty, wrapper.LocationId);
		});
	}

	public void TestLocationAdditionalId()
	{
		CombineAssertions(() =>
		{
			goodsLocation.CGL_Qualifier = "Y";
			goodsLocation.CGL_AdditionalIdentifier = "Additional id";
			AssertEquals("Expected filled LocationAdditionalId", "Additional id", wrapper.LocationAdditionalId);

			goodsLocation.CGL_Qualifier = "Z";
			AssertEquals("Expected empty LocationAdditionalId when qualifier is not X or Y (even if the fields are not empty)", ZString.Empty, wrapper.LocationAdditionalId);
		});
	}

	public void TestLocationUNloCode()
	{
		CombineAssertions(() =>
		{
			goodsLocation.CGL_Qualifier = "U";
			goodsLocation.Unlocode = "ESMAD";
			AssertEquals("Expected filled LocationUNloCode", "ESMAD", wrapper.LocationUNloCode);

			goodsLocation.CGL_Qualifier = "Z";
			AssertEquals("Expected empty LocationUNloCode when qualifier is not U (even if the fields are not empty)", ZString.Empty, wrapper.LocationUNloCode);
		});
	}

	public void TestLocationCustomOffice()
	{
		CombineAssertions(() =>
		{
			goodsLocation.CGL_Qualifier = "V";
			goodsLocation.CGL_CustomsOffice = "ES009999";
			AssertEquals("Expected filled LocationCustomOffice", "ES009999", wrapper.LocationCustomOffice);

			goodsLocation.CGL_Qualifier = "Z";
			AssertEquals("Expected empty LocationCustomOffice when qualifier is not V (even if the fields are not empty)", ZString.Empty, wrapper.LocationCustomOffice);
		});
	}

	public void TestLocationGNSS()
	{
		CombineAssertions(() =>
		{
			goodsLocation.CGL_Qualifier = "W";
			goodsLocation.Address.E2_Latitude = 1234.1234m;
			wrapper = GetWrapper(entryInstruction);
			var locationGNSS = wrapper.LocationGNSS;
			AssertNotNull("Expected filled LocationGNSS", locationGNSS);
			AssertSame("Cached LocationGNSS", wrapper.LocationGNSS, locationGNSS);

			goodsLocation.CGL_Qualifier = "Y";
			wrapper = GetWrapper(entryInstruction);
			AssertNull("Expected empty LocationGNSS when qualifier is not W (even if the fields are not empty)", wrapper.LocationGNSS);
		});
	}

	public void TestLocationEconomicOperatorId()
	{
		CombineAssertions(() =>
		{
			goodsLocation.CGL_Qualifier = "X";
			goodsLocation.Address.E2_GovRegNum = "ES12345678A";
			AssertEquals("Expected filled LocationEconomicOperatorId", "ES12345678A", wrapper.LocationEconomicOperatorId);

			goodsLocation.CGL_Qualifier = "Z";
			AssertEquals("Expected empty LocationEconomicOperatorId when qualifier is not X (even if the fields are not empty)", ZString.Empty, wrapper.LocationEconomicOperatorId);
		});
	}

	public void TestLocationAddress()
	{
		CombineAssertions(() =>
		{
			goodsLocation.CGL_Qualifier = "Z";
			goodsLocation.Address.E2_Address1 = "Street Name";
			wrapper = GetWrapper(entryInstruction);
			var locationAddress = wrapper.LocationAddress;
			AssertNotNull("Expected filled LocationAddress", locationAddress);
			AssertSame("Cached LocationAddress", wrapper.LocationAddress, locationAddress);

			goodsLocation.CGL_Qualifier = "Y";
			wrapper = GetWrapper(entryInstruction);
			AssertNull("Expected empty LocationAddress when qualifier is not Z (even if the fields are not empty)", wrapper.LocationAddress);
		});
	}

	public void TestLocationPostcodeAddress()
	{
		CombineAssertions(() =>
		{
			goodsLocation.CGL_Qualifier = "T";
			goodsLocation.CGL_AdditionalIdentifier = "House Number";
			wrapper = GetWrapper(entryInstruction);
			var locationPostcodeAddress = wrapper.LocationPostcodeAddress;
			AssertNotNull("Expected filled LocationPostcodeAddress", locationPostcodeAddress);
			AssertSame("Cached LocationPostcodeAddress", wrapper.LocationPostcodeAddress, locationPostcodeAddress);

			goodsLocation.CGL_Qualifier = "Y";
			wrapper = GetWrapper(entryInstruction);
			AssertNull("Expected empty LocationPostcodeAddress when qualifier is not T (even if the fields are not empty)", wrapper.LocationPostcodeAddress);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		entryInstruction = Factory.New<CusEntryInstruction>();
		goodsLocation = entryInstruction.GoodsLocation;

		wrapper = GetWrapper(entryInstruction);
	}
	CusEntryInstruction entryInstruction;
	EU.Business.CusGoodsLocation goodsLocation;
	CommonLocationOfGoodsWrapper wrapper;

	CommonLocationOfGoodsWrapper GetWrapper(CusEntryInstruction entryInstruction) => new CommonLocationOfGoodsWrapper(entryInstruction);

	protected override CommonLocationOfGoodsWrapper GetProvider() => wrapper;
}
