using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentScanning.Module
{
	class DocumentDbManagerModule : ZPopupModule
	{
		protected override ZPopupController GetNewController()
		{
			return new DocumentDbManagerController();
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DocumentDbManager; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.DocManager; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public override void Show()
		{
			if (IsAllowedToShow())
			{
				base.Show();
			}
		}

		protected bool IsAllowedToShow()
		{
			bool result = true;

			if (!GlbStaff.CurrentUser.GS_IsController)
			{
				result = false;
				string message = Res.GetString("c68c9cd8-9851-41ca-98c9-a6ba9a46bf30", "Only administrators can use '{0}'.", ID.Description);
				Globals.Message.ShowError(message);
			}

			return result;
		}
	}
}
