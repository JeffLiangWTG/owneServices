using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AppDomainWrappers.Net
{
	[Serializable]
	public class ProcessConfig : ICloneable
	{
		public List<string> AssemblyDependancies { get; set; }
		public string AssemblyFile { get; set; }
		public string NamespacePath { get; set; }
		public string ClassName { get; set; }
		public string MethodName { get; set; }
		public string[] MethodParameters { get; set; }
		public string BinFolder { get; private set; }
		public bool DoNotUseTempDirectory { get; set; }
		public string TempWorkingDirectoryPath { get; }
		public string TempConfigDirectoryPath { get; set; }
		public string ClientLockFileName { get; set; }
		public string HostLockFileName { get; set; }
		public AppDomainRunMode RunMode { get; set; }
		public List<string> UsingImports { get; set; }

		public enum AppDomainRunMode
		{
			Method,
			RawCode
		}

		public ProcessConfig()
		{
			TempWorkingDirectoryPath = TemporaryWorkspace.GenerateTempDirectoryPathWithoutCreation();
			BinFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		public ProcessConfig(string tempWorkingDirectoryPath, string binFolder)
		{
			TempWorkingDirectoryPath = tempWorkingDirectoryPath;
			BinFolder = binFolder;
		}

		public object Clone()
		{
			return new ProcessConfig(TempWorkingDirectoryPath, BinFolder)
			{
				AssemblyDependancies = AssemblyDependancies?.ToList(),
				AssemblyFile = AssemblyFile,
				NamespacePath = NamespacePath,
				ClassName = ClassName,
				MethodName = MethodName,
				MethodParameters = MethodParameters?.ToArray(),
				DoNotUseTempDirectory = DoNotUseTempDirectory,
				ClientLockFileName = ClientLockFileName,
				HostLockFileName = HostLockFileName,
				RunMode = RunMode,
				UsingImports = UsingImports,
			};
		}
	}
}
