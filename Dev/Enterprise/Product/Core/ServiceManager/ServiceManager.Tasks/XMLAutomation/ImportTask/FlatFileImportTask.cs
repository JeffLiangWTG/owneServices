using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class FlatFileImportTask : ImportTask
	{
		public FlatFileImportTask(StringRegistryItem registryPath, FlatFileImporter importer, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup)
		{
			this.Importer = importer;
		}

		protected override void ProcessFile(FileInfo dataFile)
		{
			Importer.ImportFlatFile(dataFile, Notify);
		}

		protected override ZString FileExtension
		{
			get { return "*.csv"; }
		}

		readonly FlatFileImporter Importer;
	}
}
