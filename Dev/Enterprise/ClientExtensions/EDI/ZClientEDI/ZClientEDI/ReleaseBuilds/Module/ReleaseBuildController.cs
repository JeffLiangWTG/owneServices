using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.ReleaseBuilds.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.ReleaseBuilds.Module
{
	public class ReleaseBuildController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ClientControllerRegistration.ReleaseBuild; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ClientModuleRegistration.ReleaseBuild; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ReleaseBuild); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ReleaseBuildForm((ReleaseBuild)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var result = (ReleaseBuild)base.GetNewBusinessEntityInLocalFactory();

			using (result.SuspendSettingHasChanges())
			using (result.GetValidationSuspender())
			{
				result.HL_Product = ZString.Empty;
			}

			return result;
		}

		#region Implementation

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
