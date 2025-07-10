using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DataConverters.CustomsFiles
{
	public class CustomsFilesImportModule : ZPopupModule
	{
		public CustomsFilesImportModule()
		{
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get	{ return Env.Licence.InterfaceConnector; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ImportCustomsFilesData; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.ImportCustomsFilesData; }
		}

		protected override ZPopupController GetNewController()
		{
			return new CustomsFilesImportController();
		}

		public override void Show()
		{
			if (!MainForm.IsShown)
			{
				base.Show();
			}
		}
	}
}
