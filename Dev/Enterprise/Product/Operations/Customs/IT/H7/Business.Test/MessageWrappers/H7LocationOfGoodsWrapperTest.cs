using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(H7LocationOfGoodsWrapper))]
public sealed class H7LocationOfGoodsWrapperTest : DataProviderTestCase<H7LocationOfGoodsWrapper>
{
	public void TestType()
	{
		SetUpTests();
		goodsLocation.CGL_Type = "A";
		var wrapper = new H7LocationOfGoodsWrapper(goodsLocation);
		AssertEquals(nameof(wrapper.Type), "A", wrapper.Type);
	}

	public void TestQualifier()
	{
		SetUpTests();
		goodsLocation.CGL_Qualifier = "U";
		var wrapper = new H7LocationOfGoodsWrapper(goodsLocation);
		AssertEquals(nameof(wrapper.Qualifier), "U", wrapper.Qualifier);
	}

	public void TestUnlocode()
	{
		AssertNull(nameof(Provider.Unlocode), Provider.Unlocode);
	}

	public void TestAuthorizationNumber()
	{
		SetUpTests();
		var wrapper = new H7LocationOfGoodsWrapper(goodsLocation);

		CombineAssertions(() =>
		{
			AssertNull("Null when CGL_Qualifier is empty", wrapper.AuthorizationNumber);

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
			goodsLocation.CGL_Authorization = "00000A";
			AssertEquals(nameof(wrapper.AuthorizationNumber), "00000A", wrapper.AuthorizationNumber);
		});
	}

	public void TestLocationIdentifier()
	{
		SetUpTests();
		goodsLocation.CGL_AdditionalIdentifier = "AdditionalIdentifier";
		var wrapper = new H7LocationOfGoodsWrapper(goodsLocation);
		AssertEquals(nameof(wrapper.LocationIdentifier), "AdditionalIdentifier", wrapper.LocationIdentifier);
	}

	public void TestCustomsOfficeCode()
	{
		SetUpTests();
		var wrapper = new H7LocationOfGoodsWrapper(goodsLocation);

		CombineAssertions(() =>
		{
			AssertNull("Null when CGL_Qualifier is not V", wrapper.CustomsOfficeCode);

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
			goodsLocation.CGL_CustomsOffice = "office";
			AssertEquals(nameof(wrapper.CustomsOfficeCode), "office", wrapper.CustomsOfficeCode);
		});
	}

	public void TestLatitude()
	{
		AssertNull(nameof(Provider.Latitude), Provider.Latitude);
	}

	public void TestLongitude()
	{
		AssertNull(nameof(Provider.Longitude), Provider.Longitude);
	}

	public void TestRegistrationNumberOfParty()
	{
		SetUpTests();
		var wrapper = new H7LocationOfGoodsWrapper(goodsLocation);

		CombineAssertions(() =>
		{
			AssertNull("Null when CGL_Qualifier is not X", wrapper.RegistrationNumberOfParty);
			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
			goodsLocation.Address.E2_GovRegNum = "GovRegNum12345";
			AssertEquals(nameof(Provider.RegistrationNumberOfParty), "GovRegNum12345", wrapper.RegistrationNumberOfParty);
		});
	}

	public void TestAddress()
	{
		SetUpTests();
		var wrapper = new H7LocationOfGoodsWrapper(goodsLocation);

		CombineAssertions(nameof(Provider.Address), () =>
		{
			AssertNull("Null when CGL_Qualifier is not Z", wrapper.Address);

			goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
			var address = goodsLocation.Address;
			address.E2_CompanyName = "WiseTech";
			address.E2_Address1 = "25 Bourke Road";
			address.E2_Address2 = "Alexandria";
			address.E2_RN_NKCountryCode = "AU";
			address.Postcode = "2015";
			address.City = "Sydney";
			AssertNotNull(wrapper.Address);
			AssertEquals(nameof(wrapper.Address.Name), "WiseTech", wrapper.Address.Name);
			AssertEquals(nameof(wrapper.Address.StreetAndNumber), "25 Bourke Road Alexandria", wrapper.Address.StreetAndNumber);
			AssertEquals(nameof(wrapper.Address.Country), "AU", wrapper.Address.Country);
			AssertEquals(nameof(wrapper.Address.ZipCode), "2015", wrapper.Address.ZipCode);
			AssertEquals(nameof(wrapper.Address.City), "Sydney", wrapper.Address.City);
		});
	}

	public void TestMailingAddress()
	{
		AssertNull(nameof(Provider.MailingAddress), Provider.MailingAddress);
	}

	void SetUpTests()
	{
		header = Factory.New<AsycudaManifestHeader>();
		bill = header.Bills.AddNew();
		goodsLocation = bill.CusGoodsLocation;
	}

	protected override H7LocationOfGoodsWrapper GetProvider()
	{
		var pHeader = Factory.New<AsycudaManifestHeader>();
		var pBill = pHeader.Bills.AddNew();
		var pGoodsLocation = pBill.CusGoodsLocation;

		return new H7LocationOfGoodsWrapper(pGoodsLocation);
	}

	AsycudaManifestHeader header;
	AsycudaBill bill;
	CusGoodsLocation goodsLocation;
}
