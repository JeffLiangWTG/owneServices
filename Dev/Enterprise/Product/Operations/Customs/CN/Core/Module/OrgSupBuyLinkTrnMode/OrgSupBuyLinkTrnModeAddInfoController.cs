using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.CN.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.CN.Module
{
	public class OrgSupBuyLinkTrnModeAddInfoController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.CN.OrgSupBuyLinkTrnModeAdditionalCustomsDetails;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => typeof(MasterFiles.Business.OrgSupBuyLinkTrnMode);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.OrgConsigneeModifyAdditionalCustomsDefaults;

		public override ResourceStringData PluginTabPageCaption => Res.GetData("B280F57B-B1FF-4F42-8E38-4BE299D8FC2D", "China Customs Defaults");

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("This function is not supported.");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new OrgSupBuyLinkTrnModeAddInfoPlugIn(businessEntity);
	}
}
