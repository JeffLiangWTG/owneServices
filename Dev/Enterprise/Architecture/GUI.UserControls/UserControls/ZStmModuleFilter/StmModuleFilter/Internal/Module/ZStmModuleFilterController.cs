using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZStmModuleFilterController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;
		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not supported");
		}

		public override ControllerID ID => ControllerIDs.StmModuleFilter;

		public override ModuleIdentifier ModuleID => ModuleIDs.StmModuleFilter;

		public override Type TypeOfTopLevelBusinessObject => typeof(StmModuleFilter);
	}
}
