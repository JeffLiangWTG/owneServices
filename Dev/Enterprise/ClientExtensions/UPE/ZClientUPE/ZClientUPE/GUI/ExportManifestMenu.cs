using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.ExportManifest.GUI;

namespace Enterprise.Client.UPE.GUI.DataImport
{
	public class UPEExportManifestMenu : ExportManifestMenu
	{
		public UPEExportManifestMenu()
		{
		}

		protected override void ImportDataMenuHandler(object sender, EventArgs e)
		{
			ExportCustomsManifestHeader tempHeader = Header;
			ManifestDataImporter dataImporter = new ManifestDataImporter();
			if (dataImporter.DoImport(tempHeader.Factory, ref Header))
			{
				ConsolFormCreator formCreator = new ConsolFormCreator(dataImporter);
				formCreator.ShowForms();
			}
		}
	}
}
