using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	abstract class SendAndAmendHeader14ProviderBaseTest<T> : ICS2BaseMessageProviderTest<T> where T : SendAndAmendHeader14Provider
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("ManifestHeader missing", () => new SendAndAmendHeader14Provider(null));
				AssertNoExceptionThrown(() => new SendAndAmendHeader14Provider(manifestHeader));
			});
		}

		public virtual void TestReferralRequestReference()
		{
			AssertEquals(string.Empty, Provider.ReferralRequestReference);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;

			AssertEquals(EUICS2SpecificCircumstanceList.Codes.F14, Provider.SpecificCircumstanceIndicator);
		}

		public void TestAddressedMemberStateCountry()
		{
			manifestHeader.AddressedMemberState = "DE";
			AssertEquals("AddressedMemberStateCountry", "DE", Provider.AddressedMemberStateCountry);
		}

		public void TestRepresentative()
		{
			AssertNull(Provider.Representative);

			var orgaddress = Factory.NewWithValidTestData<OrgAddress>();
			manifestHeader.AMA_OA_ShippingAgent = orgaddress.PK;
			var newProvider = GetProvider();

			AssertNotNull(newProvider.Representative);
		}

		public void TestTransportMode()
		{
			AssertNullOrEmpty(Provider.TransportMode);

			CombineAssertions(() =>
			{
				manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("1", Provider.TransportMode);

				manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("3", Provider.TransportMode);

				manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("4", Provider.TransportMode);

				manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
				AssertEquals("8", Provider.TransportMode);
			});
		}

		public void TestConsignmentMasterLevel()
		{
			manifestHeader.AdditionalInfos.AddNew();
			manifestHeader.Bills.AddNew();
			var newProvider = GetProvider();

			CombineAssertions(() =>
			{
				AssertEquals(1, newProvider.ConsignmentMasterLevel.AdditionalInformationCollection.Count);
				AssertEquals(1, newProvider.ConsignmentMasterLevel.ConsignmentHouseLevelCollection.Count);
			});
		}
	}
}
