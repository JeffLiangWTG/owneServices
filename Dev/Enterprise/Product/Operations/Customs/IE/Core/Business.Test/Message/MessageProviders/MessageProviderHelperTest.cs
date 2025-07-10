using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class MessageProviderHelperTest : TestCaseWithFactory
	{
		public void TestGetPartyRegNo()
		{
			AssertEquals("null orgAddress", ZString.Empty, MessageProviderHelper.GetPartyRegNo(null));

			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			AssertEquals("No Customs Code", ZString.Empty, MessageProviderHelper.GetPartyRegNo(orgAddress));

			orgHeader.CustomsCodes.AddNew(OrgCusCode.IrelandCodeTypes.CGT, "C001");
			AssertEquals("CGT", "CGTC001", MessageProviderHelper.GetPartyRegNo(orgAddress));

			orgHeader.CustomsCodes.AddNew(OrgCusCode.IrelandCodeTypes.ITX, "I001");
			AssertEquals("ITX", "ITXI001", MessageProviderHelper.GetPartyRegNo(orgAddress));

			orgHeader.CustomsCodes.AddNew(OrgCusCode.IrelandCodeTypes.PYE, "P001");
			AssertEquals("PYE", "PYEP001", MessageProviderHelper.GetPartyRegNo(orgAddress));
		}

		public void TestGetVatIdentificationNumber() => CombineAssertions(() =>
		{
			AssertEquals("Empty key for OrgHeader", string.Empty, MessageProviderHelper.GetVatIdentificationNumber(Factory, ZGuid.Empty));
			var orgHeader = Factory.New<OrgHeader>();
			var headerPK = orgHeader.PK;
			AssertEquals("OrgHeader has no Eori", string.Empty, MessageProviderHelper.GetVatIdentificationNumber(Factory, headerPK));
			_ = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertEquals("OrgHeader has Eori", "123", MessageProviderHelper.GetVatIdentificationNumber(Factory, headerPK));
		});
	}
}
