using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.TNT.AirCargo
{
	public class XXXAirCargoFileImporter : AirCargoFileImporter
	{
		public XXXAirCargoFileImporter(Form modalForm)
			: base(modalForm)
		{
		}

		#region Implementation

		#region CheckEnvironmentValid

		protected override bool CheckEnvironmentValid(out string errorMessage)
		{
			bool result = true;
			errorMessage = "";

			if (TNTDataRegistry.Instance.XXXFileSourceDirectory.IsEmpty)
			{
				errorMessage = "XXX Source Directory has not been correctly setup in the Registry." + System.Environment.NewLine;
				result = false;
			}

			if (TNTDataRegistry.Instance.XXXFileProcessedDirectory.IsEmpty)
			{
				errorMessage += "XXX Processed Directory has not been correctly setup in the Registry." + System.Environment.NewLine;
				result = false;
			}

			return result;
		}

		#endregion

		protected override string SourceDirectory
		{
			get { return TNTDataRegistry.Instance.XXXFileSourceDirectory; }
		}

		protected override string ValidFilePattern
		{
			get { return @"\.XXX$"; }
		}

		protected override string FileExtensionCore
		{
			get { return "XXX"; }
		}

		protected override string FileType
		{
			get { return "XXX"; }
		}

		protected override ZForm GetNewForm(ImportManager manager)
		{
			return new XXXImportForm((XXXImportManager)manager);
		}

		protected override ImportManager GetNewImportManager(string fileFullPath)
		{
			return new XXXImportManager(new BusinessObjectFactory(), fileFullPath);
		}

		#endregion
	}
}
