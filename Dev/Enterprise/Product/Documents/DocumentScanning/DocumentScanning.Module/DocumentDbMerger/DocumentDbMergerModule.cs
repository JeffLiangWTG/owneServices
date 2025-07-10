using CargoWise.Data;
using Enterprise.DocumentScanning.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentScanning.Module
{
	class DocumentDbMergerModule : ZPopupModule
	{
		protected override ZPopupController GetNewController()
		{
			return new DocumentDbMergerController();
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DocumentDbMerger; }
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
			if (CheckIsAllowedToShow())
			{
				base.Show();
			}
		}

		protected bool CheckIsAllowedToShow()
		{
			bool result = true;

			if (!GlbStaff.CurrentUser.GS_IsController)
			{
				result = false;
				string message = Res.GetString("8a6108ff-49a2-449d-9de4-2263039d1b52", "Only administrators can use '{0}'.", ID.Description);
				Globals.Message.ShowError(message);
			}
			else if (SqlServerEdition == DbConnection.SqlServerEdition.Express)
			{
				result = false;
				string message = Res.GetString("4324ce3e-9aa2-4931-996e-8f5daebc3f0e", "You cannot use '{0}' with Desktop/Express SQL Server Editions.", ID.Description);
				Globals.Message.ShowError(message);
			}
			else if (!ChartFXLicence.CreateChartFXLicenceRegistryItemIfRequired())
			{
				result = false;
				string message = Res.GetString("f1b8e3ec-19f1-4cd6-96ce-604e135c6619", "The ChartFX License Key registration failed.", ID.Description);
				Globals.Message.ShowError(message);
			}

			return result;
		}

		protected virtual DbConnection.SqlServerEdition SqlServerEdition
		{
			get { return Db.Connection.ServerEdition; }
		}
	}
}
