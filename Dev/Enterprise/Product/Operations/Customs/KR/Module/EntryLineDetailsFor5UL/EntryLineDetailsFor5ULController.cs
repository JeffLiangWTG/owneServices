using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.Module
{
	public class EntryLineDetailsFor5ULController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.KR.EntryLineDetailsFor5UL;
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.KR.EntryLineDetailsFor5UL;
		public override Type TypeOfTopLevelBusinessObject => typeof(KREntryLineDetailsView);
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}
	}
}
