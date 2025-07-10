using System;
using System.IO;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	class ImportTaskWithCallback : ImportTaskForTest
	{
		public ImportTaskWithCallback(StringRegistryItem registryPath, Action<FileInfo> action)
			: base(registryPath)
		{
			this.action = action;
		}

		protected override void ProcessFile(FileInfo dataFile)
		{
			action(dataFile);
		}

		readonly Action<FileInfo> action;
	}
}
