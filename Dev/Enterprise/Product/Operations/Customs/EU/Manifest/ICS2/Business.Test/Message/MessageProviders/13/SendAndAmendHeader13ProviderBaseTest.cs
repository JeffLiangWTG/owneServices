using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	abstract class SendAndAmendHeader13ProviderBaseTest<T> : ICS2BaseMessageProviderTest<T> where T : SendAndAmendHeader13Provider
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("ManifestHeader missing", () => new SendAndAmendHeader13Provider(null));
				AssertNoExceptionThrown(() => new SendAndAmendHeader13Provider(manifestHeader));
			});
		}

		public virtual void TestReferralRequestReference()
		{
			AssertEquals(string.Empty, Provider.ReferralRequestReference);
		}

		public void TestSpecificCircumstanceIndicator()
		{
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F13;
			AssertEquals(EUICS2SpecificCircumstanceList.Codes.F13, Provider.SpecificCircumstanceIndicator);
		}

		public void TestReentryIndicator()
		{
			manifestHeader.ReEntryIndicator = ZBool.False;
			AssertEquals(0, Provider.ReentryIndicator);

			manifestHeader.ReEntryIndicator = ZBool.True;
			AssertEquals(1, Provider.ReentryIndicator);
		}

		public void TestSplitConsignmentIndicator()
		{
			manifestHeader.SplitConsignmentIndicator = ZBool.False;
			AssertEquals("0", Provider.SplitConsignment.SplitConsignmentIndicator);

			manifestHeader.SplitConsignmentIndicator = ZBool.True;
			AssertEquals("1", Provider.SplitConsignment.SplitConsignmentIndicator);
		}

		public void TestPreviousMRN()
		{
			manifestHeader.PreviousMRN = "MRN20000234";
			AssertEquals("MRN20000234", Provider.SplitConsignment.PreviousMRN);
		}

		public void TestSplitConsignment()
		{
			AssertType<SplitConsignmentProvider>(Provider.SplitConsignment);
			AssertNotNull(Provider.SplitConsignment);
		}

		public void TestRepresentative()
		{
			AssertNull(Provider.Representative);

			var orgaddress = Factory.NewWithValidTestData<OrgAddress>();
			manifestHeader.AMA_OA_ShippingAgent = orgaddress.PK;
			var newProvider = GetProvider();

			AssertNotNull(newProvider.Representative);
		}

		public void TestActiveBorderTransportMeans()
		{
			AssertType<ActiveBorderTransportMeansProvider>(Provider.ActiveBorderTransportMeans);
		}

		public void TestConsignmentMasterLevel()
		{
			AssertType<SendAndAmend13ConsignmentMasterLevelProvider>(Provider.ConsignmentMasterLevel);
		}

		public void TestEntryCustomsOfficeReferenceNumber()
		{
			manifestHeader.AMA_CustomsOffice = "CO1";
			AssertEquals("CO1", Provider.EntryCustomsOfficeReferenceNumber);

			manifestHeader.AMA_CustomsOffice = "CO2";
			AssertEquals("CO2", Provider.EntryCustomsOfficeReferenceNumber);
		}
	}
}
