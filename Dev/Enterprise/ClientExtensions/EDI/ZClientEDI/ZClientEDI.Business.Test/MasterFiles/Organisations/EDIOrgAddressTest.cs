using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	[TestedType(typeof(EDIOrgAddress))]
	internal class EDIOrgAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReadOnly()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var address = Factory.NewWithValidTestData<EDIOrgAddress>();
			address.OA_Code = "AAA";
			address.OA_OH = org.PK;
			Factory.Save();

			AssertEquals(false, address.ReadOnly);
			AssertEquals(false, address.AddressCapability.ReadOnly);

			var database = Factory.NewWithValidTestData<LicenceDatabase>();

			var clientBranch = Factory.New<ClientBranch>();
			clientBranch.LCB_Code = "AAA";
			clientBranch.LCB_OA = address.PK;
			clientBranch.LCB_LD = database.PK;
			Factory.Save();

			var loadedAddress = new BusinessObjectFactory().Load<EDIOrgAddress>(address.PK);
			AssertEquals(true, loadedAddress.ReadOnly);
			AssertEquals(false, loadedAddress.AddressCapability.ReadOnly);
			AssertEquals(false, loadedAddress.IsValidationEnabled(loadedAddress.OA_CodeInfo));
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			EDIOrgHeader testHeader = Factory.New<EDIOrgHeader>();
			return testHeader.MainAddress;
		}

		public override BusinessObject GetNewBusinessObjectSafeSaving()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.NewWithValidTestData<EDIOrgHeader>();
			header.OH_Code = "TC1";
			var address = header.Addresses.AddNew();
			address.OA_Address1 = "Setup Address";
			factory.Save();

			return address;
		}
	}
}
