using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	abstract class SendAndAmendHeader43ProviderBaseTest<T> : ICS2BaseMessageProviderTest<T> where T : SendAndAmendHeader43Provider
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("ManifestHeader missing", () => new SendAndAmendHeader43Provider(null));
				AssertNoExceptionThrown(() => new SendAndAmendHeader43Provider(manifestHeader));
			});
		}

		public void TestSpecificCircumstanceIndicator()
		{
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;

			AssertEquals(EUICS2SpecificCircumstanceList.Codes.F43, Provider.SpecificCircumstanceIndicator);
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

		public void TestConsignments()
		{
			manifestHeader.Bills.AddNew();
			AssertType<SendAndAmend43ConsignmentHouseLevelProvider>("Consignments provider is not empty and in the correct type.", Provider.Consignments.Single());
		}

		public void TestDeclarant()
		{
			var newHeaderProvider = GetProvider();
			AssertEquals("Declarant Identification Number", "DE654321", newHeaderProvider.Declarant.IdentificationNumber);
		}
	}
}
