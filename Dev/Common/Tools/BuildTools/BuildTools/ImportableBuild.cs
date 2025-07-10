using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using Mono.Cecil;
using WTG.DevTools.SourceControl;

namespace CargoWise.BuildTools
{
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	public static class ImportableBuild
	{
		public static void Prepare(string localSourcePath)
		{
			Argument.NotNullOrEmpty(localSourcePath, nameof(localSourcePath));
			Prepare(localSourcePath, Path.Combine(localSourcePath, "bin"));
		}

		public static void Prepare(string localSourcePath, string buildOutputPath)
		{
			Argument.NotNullOrEmpty(localSourcePath, nameof(localSourcePath));
			Argument.NotNullOrEmpty(buildOutputPath, nameof(buildOutputPath));
			BuildConstants.LocalEnterprisePath = localSourcePath;
			ExtractReleaseInfoXml(buildOutputPath);
		}

		public static void ExtractReleaseInfoXml(string buildOutputPath)
		{
			Argument.NotNullOrEmpty(buildOutputPath, nameof(buildOutputPath));
			var assemblyPath = Path.Combine(buildOutputPath, BuildConstants.CargoWiseOneExeForVersionInfo);
			using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath))
			{
				var resourceName = "Enterprise." + ReleaseInfoXmlFile.FileName;
				var resource = assemblyDefinition.MainModule.Resources
					.OfType<EmbeddedResource>()
					.SingleOrDefault(r => r.Name == resourceName)
					?? throw new MissingMethodException(
						string.Format(CultureInfo.InvariantCulture, "Missing resource {0} in assembly {1}", resourceName, assemblyPath));

				string targetPath = Path.Combine(buildOutputPath, BuildConstants.ReleaseInfoXmlFileName);
				File.WriteAllBytes(targetPath, resource.GetResourceData());
				File.SetAttributes(targetPath, FileAttributes.Normal);
			}
		}

		public static void CopyDocumentXmlToBin(string buildOutputPath)
		{
			Argument.NotNullOrEmpty(buildOutputPath, nameof(buildOutputPath));
			string documentXmlsDir = Path.Combine(buildOutputPath, "DocumentXmls");
			Directory.CreateDirectory(documentXmlsDir);
			foreach (var clientXmlFile in BuildConstants.GetClientDocumentXmlPaths())
			{
				string targetPath = Path.Combine(documentXmlsDir, Path.GetFileName(clientXmlFile));
				File.Copy(clientXmlFile, targetPath, true);
				new FileInfo(targetPath).Attributes = FileAttributes.Normal;
			}
		}
	}
}
