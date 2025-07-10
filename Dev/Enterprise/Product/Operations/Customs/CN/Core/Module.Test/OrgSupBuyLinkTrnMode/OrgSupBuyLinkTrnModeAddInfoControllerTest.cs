using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(OrgSupBuyLinkTrnModeAddInfoController))]
	class OrgSupBuyLinkTrnModeAddInfoControllerTest : ZControllerBasherTest
	{
		public void TestSecurityCxheckpoints()
		{
			var org = Factory.New<OrgHeader>();
			var controller = new OrgSupBuyLinkTrnModeAddInfoController();
			AssertEquals(controller.GetCheckPointForDelete(org), Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults);
			AssertEquals(controller.GetCheckPointForEdit(org), Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults);
			AssertEquals(controller.GetCheckPointForNew(org), Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults);
			AssertEquals(controller.GetCheckPointForView(org), Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CN.OrgSupBuyLinkTrnModeAdditionalCustomsDetails;

		protected override string CountryCode => Enterprise.Core.Constants.CountryCodes.China;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizO = Factory.NewWithValidTestData<OrgSupplierBuyerLink>().OrgSupBuyLinkTrnModes[0];
			bizO.PF_ContainerMode = Core.Constants.ContainerModes.FCL;
			Factory.Save();
			return bizO;
		}
	}
}
