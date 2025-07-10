using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	abstract class SendAndAmendHeader15ProviderBaseTest<T> : ICS2BaseMessageProviderTest<T> where T : SendAndAmendHeader15Provider
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				_ = AssertExceptionThrown<ArgumentException>("ManifestHeader missing", () => new SendAndAmendHeader15Provider(null));
				AssertNoExceptionThrown(() => new SendAndAmendHeader15Provider(manifestHeader));
			});
		}

		public virtual void TestReferralRequestReference()
		{
			AssertEquals(string.Empty, Provider.ReferralRequestReference);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F15;

			AssertEquals(EUICS2SpecificCircumstanceList.Codes.F15, Provider.SpecificCircumstanceIndicator);
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

		public void TestAdditionalInformationMasterLevel()
		{
			_ = manifestHeader.AdditionalInfos.AddNew();
			AssertEquals("AdditionalInformation", 1, Provider.AdditionalInformationMasterLevel.Count);
		}

		public void TestConsignmentHouseLevel()
		{
			_ = manifestHeader.Bills.AddNew();
			AssertType<SendAndAmend15ConsignmentHouseLevelProvider>("Consignments provider is not empty and in the correct type.", Provider.ConsignmentHouseLevel);
		}

		public void TestDeclarant()
		{
			var newHeaderProvider = GetProvider();
			AssertEquals("Declarant Identification Number", "DE654321", newHeaderProvider.Declarant.IdentificationNumber);
		}

		public void TestActiveBorderTransportMeans()
		{
			AssertType<ActiveBorderTransportMeansProvider>(Provider.ActiveBorderTransportMeans);
		}

		public void TestConsignmentMasterLevel()
		{
			_ = manifestHeader.AdditionalInfos.AddNew();
			_ = manifestHeader.Bills.AddNew();
			var newProvider = GetProvider();

			CombineAssertions(() =>
			{
				AssertEquals(1, newProvider.ConsignmentMasterLevel.AdditionalInformationCollection.Count);
				AssertEquals(1, newProvider.ConsignmentMasterLevel.ConsignmentHouseLevelCollection.Count);
			});
		}
	}
}
