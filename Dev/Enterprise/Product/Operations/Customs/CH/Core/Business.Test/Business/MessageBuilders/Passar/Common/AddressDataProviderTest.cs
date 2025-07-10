using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

internal class AddressDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("JobDocAddress==null", AddressDataProvider.New(null));
		});
	}

	public void TestProvider()
	{
		var orgAddress = DocAddress.Address;
		orgAddress.OA_City = "Lugano";
		orgAddress.OA_RN_NKCountryCode = "CH";
		orgAddress.OA_PostCode = "6900";
		orgAddress.OA_Address1 = "Address 1";
		orgAddress.OA_Address2 = "Address 2";
		var dataProvider = AddressDataProvider.New(DocAddress);

		CombineAssertions(() =>
		{
			AssertEquals("City", "Lugano", dataProvider.City);
			AssertEquals("Country", "CH", dataProvider.Country);
			AssertEquals("Postcode", "6900", dataProvider.Postcode);
			AssertEquals("Street and Number", "Address 1 Address 2", dataProvider.StreetAndNumber);
		});
	}

	public void TestCareOf()
	{
		CombineAssertions(() =>
		{
			DocAddress.E2_AddressOverride = true;
			var dataProvider = AddressDataProvider.New(DocAddress);
			AssertEquals("No C/o", null, dataProvider.CareOf);
			DocAddress.E2_AdditionalAddressInformation = "Care of";
			AssertEquals("C/o", "Care of", dataProvider.CareOf);
		});
	}

	public void TestStreetAndNumber()
	{
		var orgAddress = DocAddress.Address;
		var dataProvider = AddressDataProvider.New(DocAddress);
		CombineAssertions(() =>
		{
			AssertEquals("Street and Number is empty", "", dataProvider.StreetAndNumber);
			orgAddress.OA_Address1 = "Address 1";
			AssertEquals("Address 1", "Address 1", dataProvider.StreetAndNumber);
			orgAddress.OA_Address2 = "Address 2";
			AssertEquals("Address 1 + Address 2", "Address 1 Address 2", dataProvider.StreetAndNumber);
			orgAddress.OA_Address1 = "";
			AssertEquals("Address 2", "Address 2", dataProvider.StreetAndNumber);
		});
	}

	JobDocAddress CreateDocAddress()
	{
		var orgAddress = Factory.New<OrgAddress>();
		var address = Factory.New<JobDocAddress>();
		address.E2_OA_Address = orgAddress.PK;
		return address;
	}

	JobDocAddress DocAddress => docAddress ?? (docAddress = CreateDocAddress());
	JobDocAddress docAddress;
}
