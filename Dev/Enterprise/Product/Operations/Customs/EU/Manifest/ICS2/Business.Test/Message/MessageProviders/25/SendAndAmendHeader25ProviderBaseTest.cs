using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	abstract class SendAndAmendHeader25ProviderBaseTest<T> : ICS2BaseMessageProviderTest<T> where T : SendAndAmendHeader25Provider
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("ManifestHeader missing", () => new SendAndAmendHeader25Provider(null));
				AssertNoExceptionThrown(() => new SendAndAmendHeader25Provider(manifestHeader));
			});
		}

		public void TestSpecificCircumstanceIndicator()
		{
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F25;

			AssertEquals(EUICS2SpecificCircumstanceList.Codes.F25, Provider.SpecificCircumstanceIndicator);
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

		public void TestPreviousDocumentIdentification()
		{
			manifestHeader.PreviousMRN = "24DE1230A0554788U5";
			AssertEquals("24DE1230A0554788U5", Provider.PreviousDocumentIdentification);
		}

		public void TestConsignment()
		{
			AssertNull("Consignment prerequisite", Provider.Consignment);
			manifestHeader.Bills.AddNew();
			AssertNotNull("Consignment", Provider.Consignment);
		}

		public void TestDeclarant()
		{
			var newHeaderProvider = GetProvider();
			AssertEquals("Declarant Identification Number", "DE654321", newHeaderProvider.Declarant.IdentificationNumber);
		}
	}
}
