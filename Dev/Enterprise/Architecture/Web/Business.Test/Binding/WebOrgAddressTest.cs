using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	[TestedType(typeof(WebOrgAddress))]
	[HttpContextEnabledTest]
	sealed class WebOrgAddressTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetAddressPk()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddressPK = header.MainAddress.PK;
			var anotherAddressPK = header.Addresses.AddNew(OrgAddressType.Delivery, ZBool.True).PK;

			var anotherHeader = Factory.NewWithValidTestData<OrgHeader>();
			var anotherMainAddressPK = anotherHeader.MainAddress.PK;

			var testWOrgAddress = new WebOrgAddress(Factory);
			testWOrgAddress.AddressPK = mainAddressPK;
			AssertEquals(testWOrgAddress.AddressPK, mainAddressPK);
			AssertEquals(testWOrgAddress.OrganisationPK, header.PK);

			testWOrgAddress.AddressPK = anotherAddressPK;
			AssertEquals(testWOrgAddress.AddressPK, anotherAddressPK);
			AssertEquals(testWOrgAddress.OrganisationPK, header.PK);

			testWOrgAddress.OrganisationPK = anotherHeader.PK;
			AssertEquals(testWOrgAddress.AddressPK, anotherMainAddressPK);
			AssertEquals(testWOrgAddress.OrganisationPK, anotherHeader.PK);

			testWOrgAddress.AddressPK = anotherAddressPK;
			AssertEquals(testWOrgAddress.AddressPK, anotherAddressPK);
			AssertEquals(testWOrgAddress.OrganisationPK, header.PK);
		}

		public void TestSetOrgPk_WithAddressType()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var receivableAddressPK = header.Addresses.AddNew(OrgAddressType.Receivables, ZBool.True).PK;
			header.Addresses.AddNew(OrgAddressType.Delivery, ZBool.True);

			var anotherHeader = Factory.NewWithValidTestData<OrgHeader>();
			var anotherMainAddressPK = anotherHeader.MainAddress.PK;
			header.Addresses.AddNew(OrgAddressType.Delivery, ZBool.True);

			var testWOrgAddress = new WebOrgAddress(Factory, OrgAddressType.Receivables);
			testWOrgAddress.OrganisationPK = header.PK;
			AssertEquals(testWOrgAddress.AddressPK, receivableAddressPK);

			testWOrgAddress.OrganisationPK = anotherHeader.PK;
			AssertEquals(testWOrgAddress.AddressPK, anotherMainAddressPK);
		}

		public void TestFormattedAddressReturnsNoSelectedForEmptyAddress()
		{
			WebOrgAddress test = new WebOrgAddress(Factory.New<OrgAddress>());
			AssertEquals("No Address Selected", test.FormattedAddressSummary);
		}

		public void TestFormattedAddressSummaryNewLineDelimiter()
		{
			OrgAddress testOrgAddress = Factory.New<OrgAddress>();
			testOrgAddress.OA_Address1 = "Address1";
			testOrgAddress.OA_Address2 = "Address2";
			WebOrgAddress test = new WebOrgAddress(testOrgAddress);
			AssertEquals(@"ADDRESS1
ADDRESS2", test.FormattedAddressSummary);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WebOrgAddress(Factory.New<OrgAddress>());
		}
	}
}
