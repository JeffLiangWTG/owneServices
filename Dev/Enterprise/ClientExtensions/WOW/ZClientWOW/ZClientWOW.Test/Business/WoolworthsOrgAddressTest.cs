using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow
{
	[TestedType(typeof(WoolworthsOrgAddress))]
	public class WoolworthsOrgAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDontPopulateUsageComment()
		{
			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_Code = "1234";
			address.OA_Address1 = "splaty";
			address.OA_Address2 = "splaty";
			address.AddressCapability.SetCapabilityEnabled("spl");
			address.OA_OH = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			Factory.Save();
			AssertEquals("Usage comment represents the warehouse code and must not change", "1234", address.OA_Code);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(typeof(OrgAddress));
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return Factory.New<OrgHeader>().MainAddress;
		}

		public override BusinessObject GetNewBusinessObjectSafeSaving()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "TC1";
			var address = header.Addresses.AddNew();
			address.OA_Address1 = "Setup Address";
			factory.Save();
			return address;
		}
		#endregion
	}
}
