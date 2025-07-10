using System.Diagnostics.CodeAnalysis;
using CargoWise.Application;
using CargoWise.BuildTools;
using CargoWise.IO;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.Business.NativeSchemaGenerator
{
	public class NativeXMLSchemaGenerator
	{
		public static readonly string EmbedResourceFolder = "GeneratedSchemas";
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Package Name")]
		public static readonly string ProjectPath = Path.Combine(
			BuildConstants.LocalEnterprisePath,
			"Enterprise",
			"Product",
			"Core",
			"NativeDataTransfer",
			"DataTransfer.Native",
			"Enterprise.DataTransfer.Native.Business"
		);

		public int GenerateSchemas()
		{
			var nativeFilesWritten = 0;
			using (var tempDir = new TempDirectory())
			{
				nativeFilesWritten = GenerateXsdFiles(tempDir);
				EmbedFilesAsResources(tempDir);
			}
			return nativeFilesWritten;
		}

#if DEBUG
		public
#endif
		int GenerateXsdFiles(string folder)
		{
			var nativeInterface = ObjectFactory.Get<IImportService>("NativeXmlImportService");
			var nativeFilesWritten = nativeInterface.GenerateAndSaveAllXSDs(folder);
			return nativeFilesWritten + 2; // Extra 2 for the Native Root, Request Element.
		}

		void EmbedFilesAsResources(string sourceFolder)
		{
			var destinationFolder = Path.Combine(ProjectPath, EmbedResourceFolder);

			if (!Directory.Exists(destinationFolder))
			{
				throw new DirectoryNotFoundException($"The directory {destinationFolder} does not exist.");
			}
			foreach (var file in Directory.GetFiles(destinationFolder))
			{
				File.Delete(file);
			}
			foreach (var filePath in Directory.GetFiles(sourceFolder))
			{
				var destinationFilePath = Path.Combine(destinationFolder, Path.GetFileName(filePath));
				File.Copy(filePath, destinationFilePath, overwrite: true);
			}
		}
	}
}
