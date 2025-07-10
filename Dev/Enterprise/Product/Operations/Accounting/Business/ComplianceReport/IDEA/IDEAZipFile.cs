using System.Globalization;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.ComplianceReport.IDEA
{
	public class IDEAZipFile
	{
		public IDEAZipFile(AccComplianceReport report, ILogger serviceLogger)
		{
			ComplianceReport = report;
			ServiceLogger = serviceLogger;
		}

		public const int EstimatedCompressionRate = 90;
		public const double OneMB = 1024.0 * 1024.0;

		readonly AccComplianceReport ComplianceReport;
		readonly ILogger ServiceLogger;

		string RemoveInvalidCharsFromFilename(string filename)
		{
			string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
			foreach (char c in invalid)
			{
				filename = filename.Replace(c.ToString(), "");
			}
			return filename;
		}

		internal string GetZipFileNameForComplianceReport()
		{
			var docManager = ComplianceReport?.DocManagerInfo();
			if (docManager != null)
			{
				for (var i = 0; i < docManager.AllEDocs.Count; i++)
				{
					var filename = docManager.AllEDocs[i].FileName;
					if (filename.StartsWith("IDEA-Export-") && filename.EndsWith(".zip"))
					{
						var delimiterPosition = filename.IndexOf("-part");
						if (delimiterPosition >= 0)
						{
							return $"{filename.Substring(0, delimiterPosition)}.zip";
						}
					}
				}
			}
			var zipFilename = $"IDEA-Export-{ZDateTime.Now.ToString("yyyyMMdd-HHmm", CultureInfo.InvariantCulture)}.zip";
			return RemoveInvalidCharsFromFilename(zipFilename);
		}

		public void CreateZipFileForEDocs(string folderToZip, IDEAeDocs docManager)
		{
			var archiveNumber = docManager.GetMaxArchiveNumber() + 1;
			var basicFilename = GetZipFileNameForComplianceReport();
			var filenameWithoutExtension = Path.GetFileNameWithoutExtension(basicFilename);
			var extension = Path.GetExtension(basicFilename);
			var filename = $"{filenameWithoutExtension}-part{archiveNumber:000}{extension}";

			var zipFileTempDir = ComplianceReport.Factory.SubscribeForDispose(new TempDirectory());

			var zipFilenameWithPath = Path.Combine(zipFileTempDir, filename);
			ZArchitecture.Core.ZipCompression.Zip(folderToZip, zipFilenameWithPath);

			var fileInfo = new FileInfo(zipFilenameWithPath);
			ServiceLogger.Log(LogType.Debug, Invariant($"Export saved as {filename} ({fileInfo.Length / OneMB:F3} MB)"));

			ComplianceReport.AttachFileToEdoc(zipFilenameWithPath, IDEATaxAuditExport.FileDescription);
		}
	}
}
