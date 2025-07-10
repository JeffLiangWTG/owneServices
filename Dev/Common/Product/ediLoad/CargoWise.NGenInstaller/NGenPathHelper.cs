using System;
using System.IO;

namespace CargoWise.NGenInstallerProgram
{
	static class NGenPathHelper
	{
		public static string GetNGenExecutablePath(this CommandLineParameters parameters, bool use32Bit)
		{
			if (!string.IsNullOrEmpty(parameters.NGenPathForTest))
			{
				return parameters.NGenPathForTest;
			}

			return GetNGenExecutablePath(use32Bit);
		}

		public static string GetNGenExecutablePath(bool use32Bit)
		{
			return Path.Combine(Directory.GetParent(Environment.SystemDirectory).FullName, $@"Microsoft.NET\Framework{(use32Bit ? "" : "64")}\v4.0.30319\NGen.exe");
		}
	}
}
