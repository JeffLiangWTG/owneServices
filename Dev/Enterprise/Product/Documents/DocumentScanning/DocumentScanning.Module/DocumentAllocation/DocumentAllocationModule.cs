using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentScanning.Module
{
	public class DocumentAllocationModule : ZPopupModule
	{
		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AllocateDocuments; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.DocManager; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DocumentAllocation; }
		}

		protected override ZPopupController GetNewController()
		{
			return new DocumentAllocationController();
		}

		public override void Show()
		{
			var dbHelper = new DocManagerDBHelper();
			var dbNamesMissing = dbHelper.GetDBNamesMissing();
			if (dbNamesMissing.Length == 0)
			{
				base.Show();
#if DEBUG
				IsShownForTestingOnly = true;
#endif
			}
			else
			{
				var listofDBNames = ZString.Empty;
				foreach (var dBName in dbNamesMissing)
				{
					listofDBNames += "\t- " + dBName + System.Environment.NewLine;
				}

				var message = Res.GetString("7a75033f-1dc2-4854-9b64-429c0c71af6e",
						"Allocate eDocs has encountered an error due to missing Document Databases.\r\nPlease inform your System Administrator that the following document databases are missing:\r\n\r\n{0}",
						listofDBNames) + "\r\n" +
					Res.GetString("144a5536-510b-430f-a4d9-47c0bf2770b1",
						"Most likely the main database {0}",
						dbHelper.GetDatabaseName(0)) + " " +
					Res.GetString("53f92f64-e414-4d7d-a7d4-134dc6ce3626",
						"has been renamed or copied from elsewhere and has existing document data in it.\r\nTo fix the error, rename or copy all other existing databases with the prefix {0}.\r\n\r\nNote: This error will continue to happen on jobs mapped to missing eDocs databases.\r\nHowever, allocate will still work on new jobs, jobs mapped to existing eDocs and jobs with no previously allocated documents.\r\n\r\nTo proceed and open the module anyway, click 'OK'.",
						dbHelper.GetDatabaseName(0) + "_SDXXX") + "\r\n";

				var result = Globals.Message.Show(message, Res.GetString("c90a28ec-26d5-48be-be64-1f772ad35fca", "Document Databases Missing"), MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
				if (result == DialogResult.OK)
				{
					base.Show();
#if DEBUG
					IsShownForTestingOnly = true;
#endif
				}
				else
				{
#if DEBUG
					IsShownForTestingOnly = false;
#endif
				}
			}
		}

#if DEBUG
		internal bool IsShownForTestingOnly;
#endif
	}
}
