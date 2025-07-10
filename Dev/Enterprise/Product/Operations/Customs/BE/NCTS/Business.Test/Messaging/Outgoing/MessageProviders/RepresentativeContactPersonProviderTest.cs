using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(RepresentativeContactPersonProvider))]
	sealed class RepresentativeContactPersonProviderTest : Customs.Business.Testing.DataProviderTestCase<RepresentativeContactPersonProvider>
	{
		public void TestNewOrNull_AddressNull()
		{
			AssertNull(RepresentativeContactPersonProvider.NewOrNull(null));
		}

		public void TestNewOrNull_AllPropertiesEmpty()
		{
			AssertNull(RepresentativeContactPersonProvider.NewOrNull(Factory.New<JobDocAddress>()));
		}

		public void TestNewOrNull_AllPropertiesEmpty_NoCusAllocated()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var representativeMovementHeader = nctsHeader.MovementHeader.Representative;
			var representativeOrg = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "REP", representativeMovementHeader, "1", phoneNumber: "AddressPhoneNr", contactName: "Name", contactPhone: "ContactPhoneNr", contactEmail: "EMailAddress");
			AssertNull(RepresentativeContactPersonProvider.NewOrNull(representativeMovementHeader));
		}

		public void TestName()
		{
			AssertEquals("Name", provider.Name);
		}

		public void TestPhoneNumber()
		{
			AssertEquals("ContactPhoneNr", provider.PhoneNumber);
		}

		public void TestPhoneNumber_FromAddress()
		{
			representativeOrg.Contacts[0].OC_Phone = "";
			AssertEquals("AddressPhoneNr", provider.PhoneNumber);
		}

		public void TestEMailAddress()
		{
			AssertEquals("EMailAddress", provider.EMailAddress);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			representativeMovementHeader = nctsHeader.MovementHeader.Representative;
			representativeOrg = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "REP", representativeMovementHeader, "1", phoneNumber: "AddressPhoneNr", contactName: "Name", contactPhone: "ContactPhoneNr", contactEmail: "EMailAddress", contactAllocation: "CUS");
			provider = RepresentativeContactPersonProvider.NewOrNull(representativeMovementHeader);
		}

		RepresentativeContactPersonProvider provider;
		OrgHeader representativeOrg;
		JobDocAddress representativeMovementHeader;

		protected override RepresentativeContactPersonProvider GetProvider() => provider;
	}
}
