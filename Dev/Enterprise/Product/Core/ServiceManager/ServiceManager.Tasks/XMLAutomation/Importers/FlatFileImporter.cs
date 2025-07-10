using System.IO;
using CargoWise.ComponentModel;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public abstract class FlatFileImporter
	{
		public void ImportFlatFile(FileInfo dataFile, INotifications notifications)
		{
			ImportFlatFileCore(dataFile, notifications);
		}

		protected abstract void ImportFlatFileCore(FileInfo dataFile, INotifications notifications);
	}
}
