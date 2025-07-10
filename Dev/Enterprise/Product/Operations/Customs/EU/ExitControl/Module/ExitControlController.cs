using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.ExitControl.Module
{
	public class ExitControlController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.EU.ExitControl;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.ExitControl;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusExitHeader);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.EuExitControl;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.EuExitControl;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.EuExitControl;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.EuExitControl;

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			var parent = (sourceEntity as CusExitHeader)?.Parent;
			var result = parent != null
				? ExitControlControllerParentFormHelper.ShowLoadedFormSelectExitControlAndAddToRecent(this, parent, sourceEntity)
				: base.ShowLoadedForm(sourceEntity, action);

			LastShownForm = result;
			return result;
		}

		protected override IZForm GetForm(IBusiness businessEntity) => GetExitControlFormCore(businessEntity);

		protected virtual ExitControlForm GetExitControlFormCore(IBusiness businessEntity) => new ExitControlForm((CusExitHeader)businessEntity);
	}
}
