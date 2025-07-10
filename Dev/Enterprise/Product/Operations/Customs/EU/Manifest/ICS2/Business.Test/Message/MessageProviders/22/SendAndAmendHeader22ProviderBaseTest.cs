using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	abstract class SendAndAmendHeader22ProviderBaseTest<T> : ICS2BaseMessageProviderTest<T> where T : SendAndAmendHeader22Provider
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("ManifestHeader missing", () => new SendAndAmendHeader22Provider(null));
				AssertNoExceptionThrown(() => new SendAndAmendHeader22Provider(manifestHeader));
			});
		}

		public virtual void TestReferralRequestReference()
		{
			AssertEquals(string.Empty, Provider.ReferralRequestReference);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F22;

			AssertEquals(EUICS2SpecificCircumstanceList.Codes.F22, Provider.SpecificCircumstanceIndicator);
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
			AssertType<SendAndAmend22ConsignmentHouseLevelProvider>("Consignments provider is not empty and in the correct type.", Provider.Consignments.Single());
		}

		public void TestDeclarant()
		{
			var newHeaderProvider = GetProvider();
			AssertEquals("Declarant Identification Number", "DE654321", newHeaderProvider.Declarant.IdentificationNumber);
		}

		public void TestSupportingDocumentsMasterLevel()
		{
			manifestHeader.SupportingDocuments.AddNew();
			AssertEquals("SupportingDocuments", 1, Provider.SupportingDocumentsMasterLevel.Count);
		}
	}
}
