using System;
using Enterprise.Client.UPE.GUI.DataImport;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.ExportManifest.GUI;
using Enterprise.Customs.AU.Module;

namespace Enterprise.Client.UPE
{
	public class UPEExportCustomsManifestModule : ExportCustomsManifestModule
	{
		#region Event Handlers

		protected override void HandleImportDataClick(object sender, EventArgs e)
		{
			ExportCustomsManifestHeader header = null;
			ManifestDataImporter dataImporter = new ManifestDataImporter();
			if (dataImporter.DoImport(Factory, ref header))
			{
				ExportCustomsManifestController controller = (ExportCustomsManifestController)this.GetNewController(header);
				controller.SetNewBusinessObjectToReturn(header);
				controller.ShowNewForm();

				ConsolFormCreator formCreator = new ConsolFormCreator(dataImporter);
				formCreator.ShowForms();
			}
		}

#endregion

			}
}
