using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.CN.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.CN.Module
{
	public class CNOrgBuyerSupplierLinkChinaCustomsDetailsController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.CN.OrgBuyerSupplierLinkChinaCustomsDetails;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => typeof(OrgSupplierBuyerLink);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CNOrgConsigneeModifyChinaCustomsDefaults;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CNOrgConsigneeModifyChinaCustomsDefaults;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CNOrgConsigneeModifyChinaCustomsDefaults;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CNOrgConsigneeModifyChinaCustomsDefaults;

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("This function is not supported.");

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|OrgBuyerSupplierLinkChinaCustomsDetails", "China Customs Defaults");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new CNOrgAdditionalCustomsDefaultsPlugIn(businessEntity);
	}
}
