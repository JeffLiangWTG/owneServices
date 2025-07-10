using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ExtensionTest : TestCaseWithFactory
	{
		public void TestGetCustomsClientID()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var cusCode1 = orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "123456", Core.Constants.CountryCodes.Australia);
			AssertEquals("Should get CID code when Premises Address is blank", "123456", orgHeader.GetCustomsClientID());

			var address = Factory.New<OrgAddress>();
			cusCode1.OK_OA_PremisesAddress = address.PK;
			AssertEquals("Should not get CID code when Premises Address not matching Main address", ZString.Empty, orgHeader.GetCustomsClientID());

			cusCode1.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertEquals("Should get CID code when Premises Address empty", "123456", orgHeader.GetCustomsClientID());

			var cusCode2 = orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "654321", Core.Constants.CountryCodes.Australia);
			cusCode2.OK_OA_PremisesAddress = address.PK;
			AssertEquals("Should get CID code with matched Premises Address", "123456", orgHeader.GetCustomsClientID());

			AssertEquals("Should get CID code with matched Premises Address", "654321", orgHeader.GetCustomsClientID(address));
		}

		public void TestHasCCIDWithMatchedAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			var cusCode1 = orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "123456", Core.Constants.CountryCodes.Australia);
			cusCode1.OK_OA_PremisesAddress = address.PK;
			AssertEquals("Have CCID with matched address", true, orgHeader.HasCCIDWithMatchedAddress(address.PK));

			AssertEquals("Don't have CCID with matched address", false, orgHeader.HasCCIDWithMatchedAddress(ZGuid.BrettsGuid));

			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "789", Core.Constants.CountryCodes.Australia);
			AssertEquals("Don't have CCID with matched address but there's an CCID with blank address", true, orgHeader.HasCCIDWithMatchedAddress(ZGuid.BrettsGuid));
		}
	}
}
