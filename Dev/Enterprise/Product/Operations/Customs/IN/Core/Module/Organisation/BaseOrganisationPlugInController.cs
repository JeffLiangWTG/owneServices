using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.IN.Module;

public abstract class BaseOrganisationPlugInController : ZController
{
	protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("No form");

	public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

	public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("No form");

	public override ResourceStringData PluginTabPageCaption => Res.GetData("ED2ABEB1-230C-46AE-B2F9-D9304BDF1E9F", "Customs Defaults");

	public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

	protected override SecurityCheckpoint CheckPointForView => Env.Security.OrganisationView;

	protected override SecurityCheckpoint CheckPointForNew => Env.Security.OrganisationNew;

	protected override SecurityCheckpoint CheckPointForEdit => Env.Security.OrganisationModify;

	protected override SecurityCheckpoint CheckPointForDelete => Env.Security.OrganisationDelete;
}
