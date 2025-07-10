using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ResourceStrings.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ResourceStrings.Module
{
	public class LocalLanguagesController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.LocalLanguages;

		public override ModuleIdentifier ModuleID => ModuleIDs.LocalLanguages;

		public override Type TypeOfTopLevelBusinessObject => typeof(RefLocalLanguage);

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.LocalLanguagesView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.LocalLanguagesNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.LocalLanguagesModify;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.LocalLanguagesDelete;

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new LocalLanguagesForm((RefLocalLanguage)businessEntity);
		}
	}
}
