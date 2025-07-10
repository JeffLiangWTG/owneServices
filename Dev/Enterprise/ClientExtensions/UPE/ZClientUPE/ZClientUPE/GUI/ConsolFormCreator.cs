using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.Customs.AU.ExportManifest.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.GUI.DataImport
{
	public class ConsolFormCreator
	{
		public ConsolFormCreator(ManifestDataImporter dataImporter)
		{
			this.DataImporter = dataImporter;
		}

		public void ShowForms()
		{
			UPEManifestImporter importer = DataImporter.Importer as UPEManifestImporter;
			if (importer != null && importer.Consols != null)
			{
				foreach (ForwardingConsol consol in importer.Consols)
				{
					JobConsolController consolController = (JobConsolController)ZControllerFactory.Create(ControllerIDs.JobConsol);
					consolController.SetNewBusinessObjectToReturn(consol);
					consolController.ShowNewForm();
				}
			}
		}

		#region Implementation

		internal protected ManifestDataImporter DataImporter;

		#endregion
	}
}
