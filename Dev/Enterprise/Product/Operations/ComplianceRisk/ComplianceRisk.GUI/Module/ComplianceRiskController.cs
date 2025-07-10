using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.ComplianceRisk.GUI
{
	public class ComplianceRiskController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("This controller does not have GUI.");

		public override ResourceStringData PluginTabPageCaption => Res.GetData("0e1572de-f933-4887-af5e-00c98fcbf6f5", "Compliance Risk");

		public override ControllerID ID => ControllerIDs.ComplianceRiskPlugin;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("Not supported");

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new ComplianceRiskPlugIn(businessEntity);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;
	}
}
