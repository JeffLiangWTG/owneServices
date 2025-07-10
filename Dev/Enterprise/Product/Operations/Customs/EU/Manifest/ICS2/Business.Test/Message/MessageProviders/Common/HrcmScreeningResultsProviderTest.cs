using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class HrcmScreeningResultsProviderTest : DataProviderTestCase<HrcmScreeningResultsProvider>
	{
		public void TestConstructor()
		{
			var billScreening = Factory.NewWithValidTestData<AsycudaBillScreening>();
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("AsycudaBillScreening missing", () => GenerateProvider(null));
				AssertNoExceptionThrown(() => GenerateProvider(billScreening));
			});
		}

		public void TestResult()
		{
			AssertEquals("Result", "1", Provider.Result);
		}

		public void TestScreeningMethods()
		{
			Assert("ScreeningMethod", Provider.ScreeningMethods.Any());
		}

		public void TestAuthorizedPersonName()
		{
			AssertEquals("AuthorizedPersonName", "TestName", Provider.AuthorizedPersonName);
		}

		public void TestAuthorizedPersonType()
		{
			AssertEquals("AuthorizedPersonType", "2", Provider.AuthorizedPersonType);
		}

		public void TestAuthorizedPersonIdentificationNumber()
		{
			AssertEquals("AuthorizedPersonIdentificationNumber", "PersonIdentifier", Provider.AuthorizedPersonIdentificationNumber);
		}

		public void TestBinaryFiles()
		{
			Assert("BinaryFiles", Provider.BinaryFiles.Any());
		}

		public void TestFacilityPlace()
		{
			AssertNotNull("FacilityPlace", Provider.FacilityPlace);
		}

		public void TestTransportDocumentHouseLevel()
		{
			AssertNull("TransportDocumentHouseLevel should be null as the related bill is master.", Provider.TransportDocumentHouseLevel);

			var houseBill = manifestHeader.Bills.AddNew();
			houseBill.ABL_BillNumber = "HB2023";
			houseBill.TransportDocumentType = "B230";

			var houseBillScreening = houseBill.BillScreenings.AddNew();
			var provider = new HrcmScreeningResultsProvider(houseBillScreening);

			AssertNotNull("TransportDocumentHouseLevel", provider.TransportDocumentHouseLevel);
			AssertEquals("Reference Number", "HB2023", provider.TransportDocumentHouseLevel.Identifier);
			AssertEquals("Transport Document Type", "B230", provider.TransportDocumentHouseLevel.Type);
		}

		public void TestReferralRequestReference()
		{
			var houseBill = manifestHeader.Bills.AddNew();
			var houseBillScreening = houseBill.BillScreenings.AddNew();
			houseBillScreening.ASR_TransportNumber = "DE8IN4QTO5Z4ESNMJ";

			var provider = new HrcmScreeningResultsProvider(houseBillScreening);
			AssertEquals("ReferralRequestReference", "DE8IN4QTO5Z4ESNMJ", provider.ReferralRequestReference);

			provider = new HrcmScreeningResultsProvider(houseBillScreening, null);
			AssertEquals("ReferralRequestReference", "DE8IN4QTO5Z4ESNMJ", provider.ReferralRequestReference);

			provider = new HrcmScreeningResultsProvider(houseBillScreening, string.Empty);
			AssertEquals("ReferralRequestReference", string.Empty, provider.ReferralRequestReference);

			provider = new HrcmScreeningResultsProvider(houseBillScreening, "CSE1CI1JFF12");
			AssertEquals("ReferralRequestReference", "CSE1CI1JFF12", provider.ReferralRequestReference);
		}

		public void TestAdditionalInformationCollection()
		{
			Assert("AdditionalInformationCollection", Provider.AdditionalInformationCollection.Any());
		}

		protected override void SetUp()
		{
			base.SetUp();

			manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			billScreening = manifestHeader.BillScreenings.AddNew();
			billScreening.ASR_Result = "1";
			billScreening.Bill.ScreeningMethods.AddNew();
			billScreening.Bill.EDocPivotCollection.AddNew();
			billScreening.AdditionalInfos.AddNew();

			billScreening.ASR_AuthorizedPersonName = "TestName";
			billScreening.ASR_AuthorizedPersonType = "2";
			billScreening.ASR_AuthorizedPersonIdentifier = "PersonIdentifier";
			billScreening.FacilityPlace.E2_AddressOverride = true;
			billScreening.FacilityPlace.Address1 = "TestAddress1";
			billScreening.ASR_TransportNumber = "DocumentRefNo";

			billScreening.Bill.ABL_BillNumber = "MB2023";
			billScreening.Bill.TransportDocumentType = "A123";
		}

		AsycudaManifestHeader manifestHeader;
		AsycudaBillScreening billScreening;

		HrcmScreeningResultsProvider GenerateProvider(AsycudaBillScreening billScreening) => new HrcmScreeningResultsProvider(billScreening);

		protected sealed override HrcmScreeningResultsProvider GetProvider()
		{
			return GenerateProvider(billScreening);
		}
	}
}
