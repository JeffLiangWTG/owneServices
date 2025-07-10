using System;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(RepresentativeProvider))]
	sealed class RepresentativeProviderTest : PartyProviderAbstractTest<RepresentativeProvider>
	{
		public void TestStatus()
		{
			AssertEquals(2, Provider.Status);
		}

		public void TestConstructorNull()
		{
			AssertNull(RepresentativeProvider.New(Factory.New<JobDocAddress>()));
		}

		protected override bool ExpectAddressToBeNullWithIdentificationNumber => true;

		protected override bool ExpectAddressToBeNullWithoutIdentificationNumber => true;

		protected override RepresentativeProvider CreateProvider(JobDocAddress address) => RepresentativeProvider.New(address);

		protected override string ExpectedName => "Oscorp Industries1";

		protected override string AddressType => "REP";

		protected override bool ExpectContactToBeNullWithoutName => false;

		protected override bool ExpectContactToBeNullWithoutPhone => false;

		protected override Type ExpectedContactPersonProviderType => typeof(RepresentativeContactPersonProvider);

		protected override void SetupAddress()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var principal = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PRC", nctsHeader.Principal, "1");
			var guaranteeHeader = (CusGuaranteeHeader)NCTSTestHelper.SetupGuarantee(principal);
			guaranteeHeader.MainAccessCode = "AAAA";
			guaranteeHeader.MainAccessPersonName = "Name";
			guaranteeHeader.CPH_SubType = "0";
			address = nctsHeader.MovementHeader.Representative;
			var representativeOrg = NCTSTestHelper.CreateJobDocAddressForTest(Factory, AddressType, address, "1", phoneNumber: "AddressPhoneNr", contactName: "Name", contactPhone: "ContactPhoneNr", contactEmail: "EMailAddress", contactAllocation: "CUS");
			var guarantee = (NctsGuarantee)NCTSTestHelper.CreateGuaranteeForTest(nctsHeader, "0", "1234", "REF", "AAAA", "LO");
		}
	}
}
