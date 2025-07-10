using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class FlatFileImportTaskTest : ImportTaskTest
	{
		protected override ImportTask GetImportTask()
		{
			return new FlatFileImportTask(SystemDataRegistry.Instance.ProductsDataImportDirectory, new FlatFileImporterForTesting(), Notify, NotificationDataRegistry.Instance.ProductImportNotificationGroup);
		}

		protected override ZString GetFileName()
		{
			return "Sample.csv";
		}

		protected override void SetRegistryItem(ZString directoryName)
		{
			SystemDataRegistry.Instance.ProductsDataImportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, directoryName);
		}

		#region class FlatFileImporterForTesting

		class FlatFileImporterForTesting : FlatFileImporter
		{
			protected override void ImportFlatFileCore(FileInfo dataFile, INotifications notifications)
			{
				using (StreamReader reader = dataFile.OpenText())
				{
				}
			}
		}

		#endregion
	}
}
