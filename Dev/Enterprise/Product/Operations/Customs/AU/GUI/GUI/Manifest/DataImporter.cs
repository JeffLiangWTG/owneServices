using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.ExportManifest.GUI
{
	public class ManifestDataImporter
	{
		public bool DoImport(BusinessObjectFactory factory, ref ExportCustomsManifestHeader header)
		{
			var dialog = new ZOpenFileDialog();
			dialog.CheckFileExists = true;
			DialogResult result = dialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				using (StreamReader dataToImport = new StreamReader(dialog.OpenFile(), Encoding.ASCII))
				{
					if (header == null)
					{
						header = factory.New<ExportCustomsManifestHeader>();
					}
					fImporter = ManifestImporter.GetImporter(factory, dialog.UnmappedFileName);

					NotificationBuffer buffer = new NotificationBuffer(null);
					bool importSuccessful = fImporter.ImportDataToHeader(header, dataToImport, buffer);
					if (importSuccessful)
					{
						return true;
					}
					else
					{
						Globals.Message.ShowError("Error importing manifest.  Errors:\r\n" + buffer.AsString);
					}
				}
			}
			return false;
		}

		public ManifestImporter Importer
		{
			get { return fImporter; }
		}
		ManifestImporter fImporter;
	}
}
