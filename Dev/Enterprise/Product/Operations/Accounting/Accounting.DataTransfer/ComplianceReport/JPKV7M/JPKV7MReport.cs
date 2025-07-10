using System.Diagnostics;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.JPKV7M;
using Enterprise.Integration;
using static System.FormattableString;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M
{
	public class JPKV7MReport : IJPKV7MReport
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Not for front end")]
		public string ExportXmlToEDocs(AccComplianceReport complianceReport, ILogger logger)
		{
			Argument.NotNull(complianceReport, nameof(complianceReport));
			Argument.NotNull(logger, nameof(logger));

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var fileWriter = new JPKV7MFileWriter(complianceReport, logger);
			var tempDirectory = complianceReport.Factory.SubscribeForDispose(new TempDirectory());
			var filePath = fileWriter.WriteOutputFile(tempDirectory);
			complianceReport.AttachFileToEdoc(filePath, "JPK V7M file");

			var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
			var logInfo = Invariant($"Export finished for {complianceReport.ACR_Description} and took {elapsedSeconds:N1} seconds");
			logger.Log(LogType.Debug, logInfo);
			stopwatch.Stop();

			return AccComplianceReport.Status.ReportOutputGenerated;
		}
	}
}
