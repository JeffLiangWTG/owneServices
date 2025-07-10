using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.HK.Business.Testing
{
	class OrgDetailsTest : TestCaseWithFactory
	{
		public void TestUnmatchedOrgDetailsAddress1()
		{
			unmatchOrgRecord.AddressLine1 = "1341 LONG DRIVE";
			AssertEquals("1341 LONG DRIVE", unmatchedOrgDetails.Address1);
		}

		public void TestUnmatchedOrgDetailsAddress2()
		{
			unmatchOrgRecord.AddressLine2 = "HEIDELBERG, VIC 3084";
			AssertEquals("HEIDELBERG, VIC 3084", unmatchedOrgDetails.Address2);
		}

		public void TestUnmatchedOrgDetailsCity()
		{
			unmatchOrgRecord.City = "TORONTO";
			AssertEquals("TORONTO", unmatchedOrgDetails.City);
		}

		public void TestUnmatchedOrgDetailsCountry()
		{
			unmatchOrgRecord.Country = "Australia";
			AssertEquals("Australia", unmatchedOrgDetails.Country);
		}

		public void TestUnmatchedOrgDetailsName()
		{
			unmatchOrgRecord.OrganisationName = "CONSIGNEE NAME";
			AssertEquals("CONSIGNEE NAME", unmatchedOrgDetails.Name);
		}

		public void TestUnmatchedOrgDetailsPostCode()
		{
			unmatchOrgRecord.PostCode = "MK16 XX";
			AssertEquals("MK16 XX", unmatchedOrgDetails.PostCode);
		}

		public void TestUnmatchedOrgDetailsState()
		{
			unmatchOrgRecord.StateOrProvince = "NSW";
			AssertEquals("NSW", unmatchedOrgDetails.State);
		}

		public void TestJobDocAddressDetailsAddress1()
		{
			jobDocAddress.Address1 = "11TH FLOOR, ALEX HOUSE, VICTORIA AV";
			AssertEquals("11TH FLOOR, ALEX HOUSE, VICTORIA AV", jobDocAddressDetails.Address1);
		}

		public void TestJobDocAddressDetailsAddress2()
		{
			jobDocAddress.Address2 = "BURNTREE";
			AssertEquals("BURNTREE", jobDocAddressDetails.Address2);
		}

		public void TestJobDocAddressDetailsCity()
		{
			jobDocAddress.City = "DUBLIN";
			AssertEquals("DUBLIN", jobDocAddressDetails.City);
		}

		public void TestJobDocAddressDetailsCountry()
		{
			jobDocAddress.E2_RN_NKCountryCode = "IE";
			AssertEquals("IE", jobDocAddressDetails.Country);
		}

		public void TestJobDocAddressDetailsIsEmpty()
		{
			jobDocAddress.E2_AddressOverride = true;
			AssertEquals(false, jobDocAddressDetails.IsEmpty);
		}

		public void TestJobDocAddressDetailsName()
		{
			jobDocAddress.E2_CompanyName = "WiseTech";
			AssertEquals("WiseTech", jobDocAddressDetails.Name);
		}

		public void TestJobDocAddressDetailsPostCode()
		{
			jobDocAddress.E2_Postcode = "W1A 1AB";
			AssertEquals("W1A 1AB", jobDocAddressDetails.PostCode);
		}

		public void TestJobDocAddressDetailsState()
		{
			jobDocAddress.State = "Victoria";
			AssertEquals("Victoria", jobDocAddressDetails.State);
		}

		protected override void SetUp()
		{
			base.SetUp();
			unmatchOrgRecord = new UnmatchOrgRecord();
			ForwardingConsol testConsolExports = Factory.New<ForwardingConsol>();
			ForwardingShipment exportShipment = testConsolExports.Shipments.AddNew();
			jobDocAddress = exportShipment.ConsigneeDocumentaryAddress;
			unmatchedOrgDetails = new UnmatchedOrgDetails(unmatchOrgRecord);
			jobDocAddressDetails = new JobDocAddressDetails(jobDocAddress);
		}

		UnmatchOrgRecord unmatchOrgRecord;
		JobDocAddress jobDocAddress;
		IOrgDetails unmatchedOrgDetails;
		IOrgDetails jobDocAddressDetails;
	}
}
