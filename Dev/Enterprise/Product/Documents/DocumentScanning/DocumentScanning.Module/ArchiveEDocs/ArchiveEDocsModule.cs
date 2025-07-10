using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentScanning.Module
{
	public class ArchiveEDocsModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ArchiveEDocs; }
		}

		protected override ZPopupController GetNewController()
		{
			return new ArchiveEDocsController();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.DocManager; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ArchiveEDocs; }
		}

		public override void Show()
		{
			if (EnvProxy.IsHostedWithCargowise || ClientHookLoader.Instance?.Client == Clients.EDI)
			{
				Globals.Message.ShowWarning(Res.GetString("6D23C97A-808B-41C1-8F9D-F8DFE79B4803", "This functionality is not available for CargoWise Cloud clients."), Res.GetString("C2520864-B7CE-45FD-AFA7-500A79F36129", "Not Available"));

				return;
			}

			var dbHelper = new DocManagerDBHelper();
			var dbNamesMissing = dbHelper.GetDBNamesMissing();

			if (dbNamesMissing.Length == 0)
			{
				base.Show();
			}
			else
			{
				var listOfDbNames = dbNamesMissing.Aggregate(ZString.Empty, (current, dBName) => (ZString)(current + ("\t- " + dBName + System.Environment.NewLine)));
				var message = Res.GetString("89ca497e-b589-49f0-a269-5f759738e848", "The 'Create eDocs CD' form is not currently available because some Document Databases are missing. \r\nPlease inform your System Administrator about this problem - the following document databases are missing: \r\n\r\n{0}\r\nMost likely the base database", listOfDbNames) + " " + dbHelper.GetDatabaseName(0) + " " + Res.GetString("87ba9edf-ea33-4455-a73a-82457ba69e4f", "has been renamed or copied from somewhere else and has existing document data in it.\r\nTo fix the error, you need to rename or copy all other existing databases with the prefix") + " " + dbHelper.GetDatabaseName(0) + "_SDXXX";

				Globals.Message.ShowError(message, Res.GetString("6a0610cc-4ddb-479d-8b14-e66b5838f03a", "Document Databases Missing"));
			}
		}
	}
}
