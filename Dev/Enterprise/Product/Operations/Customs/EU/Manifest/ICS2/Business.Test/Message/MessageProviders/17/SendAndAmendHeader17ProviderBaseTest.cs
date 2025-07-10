using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

abstract class SendAndAmendHeader17ProviderBaseTest<T> : ICS2BaseMessageProviderTest<T> where T : SendAndAmendHeader17Provider
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("ManifestHeader missing", () => new SendAndAmendHeader17Provider(null));
			AssertNoExceptionThrown(() => new SendAndAmendHeader17Provider(manifestHeader));
		});
	}

	public virtual void TestReferralRequestReference()
	{
		AssertEquals(string.Empty, Provider.ReferralRequestReference);
	}

	public void TestSpecificCircumstanceIndicator()
	{
		manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F17;

		AssertEquals(EUICS2SpecificCircumstanceList.Codes.F17, Provider.SpecificCircumstanceIndicator);
	}

	public void TestAddressedMemberStateCountry()
	{
		manifestHeader.AddressedMemberState = ZString.Empty;
		AssertNullOrEmpty("AddressedMemberStateCountry is not provided.", Provider.AddressedMemberStateCountry);

		manifestHeader.AddressedMemberState = "IE";
		AssertEquals("IE", Provider.AddressedMemberStateCountry);
	}

	public void TestRepresentative()
	{
		manifestHeader.AMA_OA_ShippingAgent = Factory.NewWithValidTestData<OrgAddress>().PK;
		AssertNotNull("Representative", Provider.Representative);
	}

	public void TestConsignmentHouseLevel()
	{
		AssertNull("Consignment House Level", Provider.ConsignmentHouseLevel);
		manifestHeader.Bills.AddNew();
		AssertNotNull("Consignment House Level", Provider.ConsignmentHouseLevel);
		AssertType<SendAndAmend17ConsignmentHouseLevelProvider>(Provider.ConsignmentHouseLevel);
	}

	public void TestDeclarant()
	{
		var newHeaderProvider = GetProvider();
		AssertEquals("Declarant Identification Number", "DE654321", newHeaderProvider.Declarant.IdentificationNumber);
	}
}
