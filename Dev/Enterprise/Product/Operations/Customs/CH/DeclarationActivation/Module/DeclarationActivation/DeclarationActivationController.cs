using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.DeclarationActivation.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CH.DeclarationActivation.Module;

public sealed class DeclarationActivationController : ZController
{
	protected override IZForm GetForm(IBusiness businessEntity)
	{
		Globals.Message.ShowError($"Form implemented by WI00894375 REQ03");
		return null;
	}

	public override ControllerID ID => ControllerIDs.Customs.CH.DeclarationActivation;

	public override ModuleIdentifier ModuleID => ModuleIDs.Customs.CH.DeclarationActivation;

	public override Type TypeOfTopLevelBusinessObject => typeof(DeclarationActivationHeader);

	public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

	protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

	protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

	protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

	protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;
}
