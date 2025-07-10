using System.IO;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	internal class ImportTaskForTest : ImportTask
	{
		public ImportTaskForTest(StringRegistryItem registryPath)
			: base(registryPath, new NotificationBuffer(), NotificationDataRegistry.Instance.LocalCartageNotificationGroup)
		{
		}

		public NotificationBuffer NotificationBuffer
		{
			get { return (NotificationBuffer)Notify; }
		}

		protected override ZString FileExtension
		{
			get { return "*.xml"; }
		}

		protected override void ProcessFile(FileInfo dataFile)
		{
			using (StreamReader sr = dataFile.OpenText())
			{ }
		}
	}
}
