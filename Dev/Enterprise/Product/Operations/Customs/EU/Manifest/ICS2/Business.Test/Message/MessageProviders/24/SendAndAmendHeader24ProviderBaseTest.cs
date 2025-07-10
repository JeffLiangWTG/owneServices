using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	abstract class SendAndAmendHeader24ProviderBaseTest<T> : ICS2BaseMessageProviderTest<T> where T : SendAndAmendHeader24Provider
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("ManifestHeader missing", () => new SendAndAmendHeader24Provider(null));
				AssertNoExceptionThrown(() => new SendAndAmendHeader24Provider(manifestHeader));
			});
		}

		public virtual void TestReferralRequestReference()
		{
			AssertEquals(string.Empty, Provider.ReferralRequestReference);
		}

		public void TestAddressedMemberStateCountry()
		{
			manifestHeader.AddressedMemberState = ZString.Empty;
			AssertNullOrEmpty(Provider.AddressedMemberStateCountry);

			manifestHeader.AddressedMemberState = "IE";
			AssertEquals("IE", Provider.AddressedMemberStateCountry);
		}

		public void TestRepresentative()
		{
			AssertNull(Provider.Representative);

			var orgaddress = Factory.NewWithValidTestData<OrgAddress>();
			manifestHeader.AMA_OA_ShippingAgent = orgaddress.PK;

			var newProvider = GetProvider();
			AssertNotNull(newProvider.Representative);
		}

		public void TestConsignments()
		{
			AssertEquals(0, Provider.Consignments.Count);

			manifestHeader.Bills.AddNew();
			var newProvider = GetProvider();
			AssertEquals(1, newProvider.Consignments.Count);
		}

		public void TestDeclarant()
		{
			var newHeaderProvider = GetProvider();
			AssertEquals("Declarant Identification Number", "DE654321", newHeaderProvider.Declarant.IdentificationNumber);
		}

		public void TestHrcmScreeningResults()
		{
			AssertEquals(0, Provider.HrcmScreeningResults.Count);

			manifestHeader.BillScreenings.AddNew();
			var newProvider = GetProvider();
			AssertEquals(1, newProvider.HrcmScreeningResults.Count);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F24;

			AssertEquals(EUICS2SpecificCircumstanceList.Codes.F24, Provider.SpecificCircumstanceIndicator);
		}
	}
}
