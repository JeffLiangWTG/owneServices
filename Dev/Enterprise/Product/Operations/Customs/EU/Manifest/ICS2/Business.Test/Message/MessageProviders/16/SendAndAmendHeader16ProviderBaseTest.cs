using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

abstract class SendAndAmendHeader16ProviderBaseTest<T> : ICS2BaseMessageProviderTest<T> where T : SendAndAmendHeader16Provider
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("ManifestHeader missing", () => new SendAndAmendHeader16Provider(null));
			AssertNoExceptionThrown(() => new SendAndAmendHeader16Provider(manifestHeader));
		});
	}

	public virtual void TestReferralRequestReference()
	{
		AssertEquals(string.Empty, Provider.ReferralRequestReference);
	}

	public void TestSpecificCircumstanceIndicator()
	{
		manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F16;

		AssertEquals(EUICS2SpecificCircumstanceList.Codes.F16, Provider.SpecificCircumstanceIndicator);
	}

	public void TestAddressedMemberStateCountry()
	{
		manifestHeader.AddressedMemberState = ZString.Empty;
		AssertNullOrEmpty("AddressedMemberStateCountry is not provided.", Provider.AddressedMemberStateCountry);

		manifestHeader.AddressedMemberState = Core.Constants.CountryCodes.Austria;
		AssertEquals(Core.Constants.CountryCodes.Austria, Provider.AddressedMemberStateCountry);
	}

	public void TestRepresentative()
	{
		manifestHeader.AMA_OA_ShippingAgent = Factory.NewWithValidTestData<OrgAddress>().PK;
		AssertType<RepresentativeProvider>("Representative", Provider.Representative);
	}

	public void TestConsignmentHouseLevel()
	{
		manifestHeader.Bills.AddNew();

		AssertType<SendAndAmend16ConsignmentHouseLevelProvider>(Provider.ConsignmentHouseLevel);
	}

	public void TestDeclarant()
	{
		var newHeaderProvider = GetProvider();
		AssertType<DeclarantPartyProvider>(Provider.Declarant);
		AssertEquals("Declarant Identification Number", "DE654321", newHeaderProvider.Declarant.IdentificationNumber);
	}
}
