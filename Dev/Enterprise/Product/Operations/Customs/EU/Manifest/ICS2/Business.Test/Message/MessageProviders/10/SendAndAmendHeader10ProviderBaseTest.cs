using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	abstract class SendAndAmendHeader10ProviderBaseTest<T> : ICS2BaseMessageProviderTest<T> where T : SendAndAmendHeader10Provider
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("ManifestHeader missing", () => new SendAndAmendHeader10Provider(null));
				AssertNoExceptionThrown(() => new SendAndAmendHeader10Provider(manifestHeader));
			});
		}

		public void TestSpecificCircumstanceIndicator()
		{
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F10;
			AssertEquals(EUICS2SpecificCircumstanceList.Codes.F10, Provider.SpecificCircumstanceIndicator);
		}

		public void TestReEntryIndicator()
		{
			manifestHeader.ReEntryIndicator = ZBool.False;
			AssertEquals(0, Provider.ReEntryIndicator);

			manifestHeader.ReEntryIndicator = ZBool.True;
			AssertEquals(1, Provider.ReEntryIndicator);
		}

		public void TestSplitConsignmentIndicator()
		{
			manifestHeader.SplitConsignmentIndicator = ZBool.False;
			AssertEquals(0, Provider.SplitConsignmentIndicator);

			manifestHeader.SplitConsignmentIndicator = ZBool.True;
			AssertEquals(1, Provider.SplitConsignmentIndicator);
		}

		public void TestPreviousMRN()
		{
			manifestHeader.PreviousMRN = "MRN20000234";
			AssertEquals("MRN20000234", Provider.PreviousMRN);
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
			manifestHeader.AMA_LloydsNumber = "LLYD123";

			AssertType<ActiveBorderTransportMeansProvider>(Provider.ActiveBorderTransportMeans);
			AssertEquals("LLYD123", Provider.ActiveBorderTransportMeans.IdentificationNumber);
		}

		public void TestConsignmentMasterLevel()
		{
			AssertType<SendAndAmend10ConsignmentMasterLevelProvider>(Provider.ConsignmentMasterLevel);
		}

		public void TestCustomsOfficeOfEntry()
		{
			manifestHeader.AMA_CustomsOffice = "IECUSTOFF1";
			AssertEquals("IECUSTOFF1", Provider.CustomsOfficeOfFirstEntry);
		}
	}
}
