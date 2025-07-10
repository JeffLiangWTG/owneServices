using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	abstract class SendAndAmendHeader44ProviderBaseTest<T> : ICS2BaseMessageProviderTest<T> where T : SendAndAmendHeader44Provider
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("ManifestHeader missing", () => new SendAndAmendHeader44Provider(null));
				AssertNoExceptionThrown(() => new SendAndAmendHeader44Provider(manifestHeader));
			});
		}

		public void TestSpecificCircumstanceIndicator()
		{
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;

			AssertEquals(EUICS2SpecificCircumstanceList.Codes.F44, Provider.SpecificCircumstanceIndicator);
		}

		public void TestAddressedMemberStateCountry()
		{
			manifestHeader.AddressedMemberState = ZString.Empty;
			AssertNullOrEmpty("AddressedMemberStateCountry is not provided.", Provider.AddressedMemberStateCountry);

			manifestHeader.AddressedMemberState = "DE";
			AssertEquals("AddressedMemberStateCountry", "DE", Provider.AddressedMemberStateCountry);
		}

		public void TestRepresentative()
		{
			manifestHeader.AMA_OA_ShippingAgent = Factory.NewWithValidTestData<OrgAddress>().PK;
			AssertNotNull("Representative", Provider.Representative);
		}

		public void TestConsignments()
		{
			manifestHeader.Bills.AddNew();
			AssertType<SendAndAmend44ConsignmentHouseLevelProvider>("Consignments provider is not empty and in the correct type.", Provider.Consignments.Single());
		}

		public void TestDeclarant()
		{
			var newHeaderProvider = GetProvider();
			AssertEquals("Declarant Identification Number", "DE654321", newHeaderProvider.Declarant.IdentificationNumber);
		}
	}
}
