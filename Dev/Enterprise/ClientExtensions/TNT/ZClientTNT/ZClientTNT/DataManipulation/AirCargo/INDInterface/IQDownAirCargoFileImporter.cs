using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.TNT.AirCargo
{
	public class IQDownAirCargoFileImporter : AirCargoFileImporter
	{
		public IQDownAirCargoFileImporter(Form modalForm)
			: base(modalForm)
		{
		}

		#region Implementation

		#region CheckEnvironmentValid

		protected override bool CheckEnvironmentValid(out string errorMessage)
		{
			bool result = true;
			errorMessage = "";

			if (TNTDataRegistry.Instance.IQDownFileSourceDirectory.IsEmpty)
			{
				errorMessage = "IQDown Source Directory has not been correctly setup in the Registry." + System.Environment.NewLine;
				result = false;
			}

			if (TNTDataRegistry.Instance.IQDownFileProcessedDirectory.IsEmpty)
			{
				errorMessage += "IQDown Processed Directory has not been correctly setup in the Registry." + System.Environment.NewLine;
				result = false;
			}

			return result;
		}

		#endregion

		protected override string SourceDirectory
		{
			get { return TNTDataRegistry.Instance.IQDownFileSourceDirectory; }
		}

		protected override string ValidFilePattern
		{
			get { return @"^\w{3}\.IND\.\d{8}\.\d{6}\." + FileExtension + "$"; }
		}

		protected override string FileExtensionCore
		{
			get { return QuantumFile.Extension; }
		}

		protected override string FileType
		{
			get { return "IQDown"; }
		}

		protected override ZForm GetNewForm(ImportManager manager)
		{
			return new INDImportForm((IQDownImportManager)manager);
		}

		protected override ImportManager GetNewImportManager(string fileFullPath)
		{
			return new IQDownImportManager(new BusinessObjectFactory(), fileFullPath);
		}

		#endregion
	}
}
