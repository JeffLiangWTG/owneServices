using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.Module
{
	public class EntryDetailsFor5SGController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.KR.EntryDetailsFor5SG;
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.KR.EntryDetailsFor5SG;
		public override Type TypeOfTopLevelBusinessObject => typeof(KREntryHeaderDetailsView);
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
