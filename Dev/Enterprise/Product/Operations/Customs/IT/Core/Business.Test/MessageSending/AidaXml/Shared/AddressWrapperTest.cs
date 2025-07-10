using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing.MessageSending.AidaXml.Shared;

sealed class AddressWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When docAddress is null", () => new AddressWrapper(docAddress: null));
	}

	public void TestStreetAndNumber()
	{
		var addressWrapper = GetNewAddressWrapper();
		AssertEquals(nameof(IAddress.StreetAndNumber), "", addressWrapper.StreetAndNumber);

		orgAddress.Address1 = "ADDRESS 1";
		orgAddress.Address2 = "ADDRESS 2";
		addressWrapper = GetNewAddressWrapper();
		AssertEquals(nameof(IAddress.StreetAndNumber), "ADDRESS 1 ADDRESS 2", addressWrapper.StreetAndNumber);
	}

	public void TestCity()
	{
		var addressWrapper = GetNewAddressWrapper();
		AssertEquals(nameof(IAddress.City), "", addressWrapper.City);

		orgAddress.City = "CITY";
		addressWrapper = GetNewAddressWrapper();
		AssertEquals(nameof(IAddress.City), "CITY", addressWrapper.City);
	}

	public void TestCountry()
	{
		orgAddress.OA_RN_NKCountryCode = "";
		var addressWrapper = GetNewAddressWrapper();
		AssertEquals(nameof(IAddress.Country), "", addressWrapper.Country);

		orgAddress.OA_RN_NKCountryCode = "DE";
		addressWrapper = GetNewAddressWrapper();
		AssertEquals(nameof(IAddress.Country), "DE", addressWrapper.Country);
	}

	public void TestName()
	{
		var addressWrapper = GetNewAddressWrapper();
		AssertEquals(nameof(IAddress.Name), "", addressWrapper.Name);

		orgAddress.CompanyName = "COMPANY NAME";
		addressWrapper = GetNewAddressWrapper();
		AssertEquals(nameof(IAddress.Name), "COMPANY NAME", addressWrapper.Name);
	}

	public void TestZipCode()
	{
		var addressWrapper = GetNewAddressWrapper();
		AssertEquals(nameof(IAddress.ZipCode), "", addressWrapper.ZipCode);

		orgAddress.Postcode = "00009";
		addressWrapper = GetNewAddressWrapper();
		AssertEquals(nameof(IAddress.ZipCode), "00009", addressWrapper.ZipCode);
	}

	protected override void SetUp()
	{
		base.SetUp();

		orgAddress = Factory.New<OrgHeader>().MainAddress;
		jobDocAddress = Factory.New<JobDocAddress>();
	}

	OrgAddress orgAddress;
	JobDocAddress jobDocAddress;

	IAddress GetNewAddressWrapper()
	{
		jobDocAddress.E2_OA_Address = orgAddress.PK;
		return new AddressWrapper(jobDocAddress);
	}
}
