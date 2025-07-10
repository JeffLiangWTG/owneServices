using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(CNOrgBuyerSupplierLinkChinaCustomsDetailsController))]
	class CNOrgBuyerSupplierLinkChinaCustomsDetailsControllerTest : ZControllerBasherTest
	{
		public void TestSecurityCheckpoints()
		{
			var org = Factory.New<OrgHeader>();
			var controller = new CNOrgBuyerSupplierLinkChinaCustomsDetailsController();
			AssertEquals(Env.Security.CNOrgConsigneeModifyChinaCustomsDefaults, controller.GetCheckPointForDelete(org));
			AssertEquals(Env.Security.CNOrgConsigneeModifyChinaCustomsDefaults, controller.GetCheckPointForEdit(org));
			AssertEquals(Env.Security.CNOrgConsigneeModifyChinaCustomsDefaults, controller.GetCheckPointForNew(org));
			AssertEquals(Env.Security.CNOrgConsigneeModifyChinaCustomsDefaults, controller.GetCheckPointForView(org));
		}

		protected override string CountryCode => Enterprise.Core.Constants.CountryCodes.China;

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CN.OrgBuyerSupplierLinkChinaCustomsDetails;

		protected override Type GetBusinessObjectType() => typeof(OrgHeader);

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizO = Factory.NewWithValidTestData<OrgSupplierBuyerLink>();
			Factory.Save();
			return bizO;
		}
	}
}
