using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	abstract class SendAndAmendHeader26ProviderBaseTest<T> : ICS2BaseMessageProviderTest<T> where T : SendAndAmendHeader26Provider
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("ManifestHeader missing", () => new SendAndAmendHeader26Provider(null));
				AssertNoExceptionThrown(() => new SendAndAmendHeader26Provider(manifestHeader));
			});
		}

		public virtual void TestReferralRequestReference()
		{
			AssertEquals(string.Empty, Provider.ReferralRequestReference);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F26;

			AssertEquals(EUICS2SpecificCircumstanceList.Codes.F26, Provider.SpecificCircumstanceIndicator);
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

		public void TestTransportMode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty TransportMode", string.Empty, Provider.TransportMode);

				manifestHeader.AMA_TransportMode = "SEA";
				AssertEquals("SEA", "1", Provider.TransportMode);

				manifestHeader.AMA_TransportMode = "AIR";
				AssertEquals("AIR", "4", Provider.TransportMode);
			});
		}

		public void TestConsignments()
		{
			manifestHeader.Bills.AddNew();
			AssertType<SendAndAmend26ConsignmentHouseLevelProvider>("Consignments provider is not empty and in the correct type.", Provider.Consignments.Single());
		}

		public void TestHrcmScreeningResults()
		{
			AssertEquals(0, Provider.HrcmScreeningResults.Count);

			manifestHeader.BillScreenings.AddNew();
			var newProvider = GetProvider();
			AssertEquals(1, newProvider.HrcmScreeningResults.Count);
		}

		public void TestDeclarant()
		{
			var newHeaderProvider = GetProvider();
			AssertEquals("Declarant Identification Number", "DE654321", newHeaderProvider.Declarant.IdentificationNumber);
		}
	}
}
