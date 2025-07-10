using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(AddressProvider))]
	sealed class AddressProviderTest : TestCaseWithFactory
	{
		public void TestPhone_RemoveNonNumericCharactersExceededMaximumLimit()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "SUP01";
			var mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "1500 HAPPY RD";
			mainAddress.OA_Phone = "+1 (273) 5495200";

			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = "Address 1";
			address1.OA_Phone = "+886 123456789";

			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "Address 2";
			address2.OA_Phone = "+886 12345678901";

			var address3 = org.Addresses.AddNew();
			address3.OA_Address1 = "Address 3";
			address3.OA_Phone = "+86 903 248 7136";

			Factory.Save();

			var bill = Factory.New<AsycudaBill>();
			bill.ABL_OA_Shipper = mainAddress.PK;
			var provider = new AddressProvider(bill, "Shipper", nameof(IHCH01Bills));
			CombineAssertions(() =>
			{
				AssertEquals("Original number: +1 (273) 5495200", "+12735495200", provider.Phone);

				bill.ABL_OA_Shipper = address1.PK;
				AssertEquals("Original number: +886 123456789", "+886 123456789", provider.Phone);

				bill.ABL_OA_Shipper = address2.PK;
				AssertEquals("Original number: +886 12345678901", "88612345678901", provider.Phone);

				bill.ABL_OA_Shipper = address3.PK;
				AssertEquals("Original number: +86 903 248 7136", "+869032487136", provider.Phone);
			});
		}
	}
}
