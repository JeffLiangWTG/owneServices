using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CustomsMessagingPlugInSupportOrgHeaderWrapperTest : TestCaseWithFactory
	{
		public void TestMaster()
		{
			AssertEquals("Master", orgHeader, wrapper.Master);
		}

		public void TestPlugInVisible()
		{
			Assert("invisible", !wrapper.PlugInVisible);
			Enterprise.Customs.CA.Registry.CACustomsDataRegistry.Instance.CSAFunctionActive.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var impAddInfo = OrgImpAddInfo.Get(orgHeader);
			impAddInfo.ZO_IsCSAApprovedImporter = true;
			Assert("visible", wrapper.PlugInVisible);
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.New<OrgHeader>();
			wrapper = new CustomsMessagingPlugInSupportOrgHeaderWrapper(orgHeader);
		}

		OrgHeader orgHeader;
		CustomsMessagingPlugInSupportOrgHeaderWrapper wrapper;
	}
}
