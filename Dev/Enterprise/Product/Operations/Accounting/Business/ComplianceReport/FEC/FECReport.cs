using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.ComplianceReport.FEC
{
	public class FECReport
	{
		public string ExportData(AccComplianceReport complianceReport, ILogger serviceLogger)
		{
			Argument.NotNull(complianceReport, nameof(complianceReport));
			Argument.NotNull(serviceLogger, nameof(serviceLogger));

			var stopwatch = Stopwatch.StartNew();

			var accountMappingHelper = new GLAccountToLocalAccountMapping(complianceReport, Core.SharedConstants.Languages.French);
			// FEC: This method is called twice.
			// 1. run: A list of accounts with missing mapping is initialised based on values in table dbo.AccGLAggregate.
			// 2. run: Other unmapped local accounts may have been found when processing each single transaction if those accounts were not in table dbo.AccGLAggregate.
			if (!accountMappingHelper.ValidateMappingAndUpdateNotesAndStatus())
			{
				return complianceReport.ACR_Status;
			}

			// the report periodicity is PRS so it is ensured that ACR_DateFrom is at the beginning of a period and ACR_DateTo is at the end of a period, which means we can safely split the generation on period boundaries
			var combineChunksStep = complianceReport.NextProcessingStepFromDate > complianceReport.ACR_DateTo;
			if (!combineChunksStep)
			{
				var fecFileWriter = complianceReport.SubscribeForDispose(new FECFileWriter(complianceReport, complianceReport.NextProcessingStepFromDate, accountMappingHelper));
				fecFileWriter.WriteFECTransactions();

				if (!accountMappingHelper.ValidateMappingAndUpdateNotesAndStatus(serviceLogger))
				{
					return complianceReport.ACR_Status;
				}

				var zipFilename = fecFileWriter.ZipContent();

				complianceReport.AttachFileToEdoc(zipFilename, FileDescription);
				complianceReport.NextProcessingStepFromDate = fecFileWriter.ChunkEndDatePlusOne;

				var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
				serviceLogger.Log(LogType.Debug, Invariant($"Generation finished for {complianceReport.ACR_Description} chunk {fecFileWriter.ChunkId} and took {elapsedSeconds:N1} seconds"));
			}
			else
			{
				var combinedFilename = CombineChunksIntoSingleFile(complianceReport, serviceLogger, accountMappingHelper);
				if (string.IsNullOrEmpty(combinedFilename) || !accountMappingHelper.ValidateMappingAndUpdateNotesAndStatus(serviceLogger))
				{
					return complianceReport.ACR_Status;
				}

				complianceReport.AttachFileToEdoc(combinedFilename, FileDescription);
				complianceReport.NextProcessingStepFromDate = ZDate.Empty;

				var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
				serviceLogger.Log(LogType.Debug, Invariant($"Export finished for {complianceReport.ACR_Description} and took {elapsedSeconds:N1} seconds to combine all chunks into a single file"));
			}

			complianceReport.Factory.Save();  // save eDocs

			return combineChunksStep ? AccComplianceReport.Status.ReportGenerated : AccComplianceReport.Status.ReportDataQueued;
		}

		internal string CombineChunksIntoSingleFile(AccComplianceReport complianceReport, ILogger serviceLogger, GLAccountToLocalAccountMapping accountMappingHelper)
		{
			complianceReport.DeleteNotes(NoteDescriptionCombiningFailed);

			// get all chunks from eDocs and ignore files attached manually by the user
			var chunkFilenames = complianceReport.DocManagerInfo().AllEDocs.OfType<IeDocBase>().Where(v => v.FileName.StartsWith("PERIOD") && v.FileName.EndsWith(".zip") && v.IsSystemGenerated && !v.IsDeleted).Select(v => v.FileName).OrderBy(v => v).ToArray();

			var tempDir = complianceReport.SubscribeForDispose(new TempDirectory());
			var fecFileWriter = complianceReport.SubscribeForDispose(new FECFileWriter(complianceReport, complianceReport.NextProcessingStepFromDate, accountMappingHelper));

			fecFileWriter.WriteFECHeader();
			fecFileWriter.WriteFECOpeningBalances();

			foreach (var filename in chunkFilenames)
			{
				try
				{
					using (var streamReader = OpenFileForReading(complianceReport, filename, tempDir))
					{
						fecFileWriter.WriteStream(streamReader);
					}
				}
				catch (FileNotFoundException ex)
				{
					var noteMessage = ex.Data.Contains(NoteMessageKey)
						? ex.Data[NoteMessageKey] as string
						: (NoResString)"The final ZIP file cannot be created because a required file was not found.";

					CreateNote(complianceReport, serviceLogger, ex.Message, ex.FileName, noteMessage + (NoResString)"\r\nFile: ");
					return null;
				}
				catch (FileLoadException ex)
				{
					CreateNote(complianceReport, serviceLogger, ex.Message, ex.FileName,
						(NoResString)"The final ZIP file cannot be created because duplicate PERIODxxxxxx.zip files are attached under eDocs.\r\nThis might lead to duplicated data in the final FEC file.\r\nPlease re-queue this FEC report.\r\nFile: ");
					return null;
				}

				DeleteEDoc(complianceReport, filename);
			}

			return fecFileWriter.ZipContent();
		}

		static void CreateNote(AccComplianceReport complianceReport, ILogger serviceLogger, string exceptionMessage, string fileName, string noteText)
		{
			serviceLogger.Log(LogType.Error, Invariant($"Combining all periods into one ZIP file is not possible for FEC report. {exceptionMessage} File: {fileName}"));
			// inform the user via the Notes tab

			var notesText = new ZStringBuilder();
			notesText.Append($"{noteText}{fileName}");
			complianceReport.AddNote(NoteDescriptionCombiningFailed, notesText.ToString());

			complianceReport.ACR_Status = AccComplianceReport.Status.ReportError;
			complianceReport.ACR_StatusMessage = (NoResString)"Combining all periods into one ZIP file is not possible. See Notes tab for details.";
			complianceReport.Logs.AddNew(Events.StatusUpdated, AccComplianceReport.StatusKey + complianceReport.ACR_Status);
			complianceReport.Factory.Save();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		StreamReader OpenFileForReading(AccComplianceReport complianceReport, ZString filename, string tempFolder)
		{
			var samePeriodFilenamePrefix = filename.SubstringSafe(0, 12);
			var systemGeneratedSamePeriodFiles = complianceReport.DocManagerInfo().AllEDocs.OfType<IeDocBase>()
				.Where(x => x.IsSystemGenerated && !x.IsDeleted
					&& x.FileName.StartsWith(samePeriodFilenamePrefix, StringComparison.OrdinalIgnoreCase)
					&& x.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
				.Select(x => x.FileName.ToString())
				.ToArray();

			// System generated files should be removed when re-queueing but if this fails ...
			if (systemGeneratedSamePeriodFiles.Length > 1)
			{
				throw new FileLoadException("Found duplicate PERIODxxxxxx.zip files", systemGeneratedSamePeriodFiles.Last());
			}

			var eDoc = complianceReport.DocManagerInfo().AllEDocs.OfType<IeDocBase>().FirstOrDefault(v => v.FileName == filename && v.IsSystemGenerated && !v.IsDeleted);
			if (eDoc == null)
			{
				var ex = new FileNotFoundException($"Could not find system-generated period file in eDocs.", filename);
				ex.Data[NoteMessageKey] =
					"The final ZIP file cannot be created because one of the system-generated PERIODxxxxxx.zip files could not be found in eDocs.\r\n" +
					"This might lead to incomplete data in the final FEC file.\r\n" +
					"Please re-queue this FEC report.";
				throw ex;
			}

			var zipFilenameInTempFolder = Path.Combine(tempFolder, filename);
			File.WriteAllBytes(zipFilenameInTempFolder, eDoc.ImageData);
			ZArchitecture.Core.ZipCompression.Unzip(zipFilenameInTempFolder, tempFolder);
			// There is only 1 CSV file in the ZIP file, but it could have a different name if a previous ZIP file with the same name has not been deleted permanently,
			// e.g. PERIOD202102[2].zip, zipped file PERIOD202102.csv
			var left = filename.Left(12);
			var fileEntries = Directory.GetFiles(tempFolder, left + "*.csv");
			var csvFilenameInTempFolder = fileEntries.Length > 0 ? fileEntries[0] : null;
			if (csvFilenameInTempFolder == null)
			{
				var ex = new FileNotFoundException("No CSV file in ZIP file.", filename);
				ex.Data[NoteMessageKey] =
					"The final ZIP file cannot be created because one of the PERIOD*.ZIP files doesn't contain a CSV file.\r\n" +
					"Please re-queue this FEC report.";
				throw ex;
			}
			return new StreamReader(new FileStream(csvFilenameInTempFolder, FileMode.Open));
		}

		void DeleteEDoc(AccComplianceReport complianceReport, string filename)
		{
			var eDoc = complianceReport.DocManagerInfo().AllEDocs.OfType<StorageDocsBase>().FirstOrDefault(v => v.SC_FileNameWithExtension == filename);
			if (eDoc != null)
			{
				eDoc.Delete();
			}
		}

		static string NoteDescriptionCombiningFailed
		{
			get => Res.GetString("FEC0CBE7-1AB9-4830-8723-21FDE0EB62E5", "Combining all periods into 1 ZIP file not possible");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "File description")]
		const string FileDescription = "FEC Tax Audit";
		const string NoteMessageKey = "NoteMessage";
	}
}
