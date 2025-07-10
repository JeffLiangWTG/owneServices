using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;
using WTG.ErrorReporting;

[assembly: HostedService(Enterprise.Client.EDI.ServiceTask.IssueManagerProcessorServiceTask.Code,
	"Issue Manager Processor",
	"CSP",
	typeof(Enterprise.Client.EDI.ServiceTask.IssueManagerProcessorServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1Minutes",
	DefaultScheduleRunEvery = "1minute")]

namespace Enterprise.Client.EDI.ServiceTask
{
	class IssueManagerProcessorServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			string errorLogDirectory = GetErrorLogDirectory();
			DownloadAsync(errorLogDirectory).Wait();

			CleanUpTmpFiles(errorLogDirectory);

			ServiceLogger.Information("Process started");
			Process(errorLogDirectory, new BusinessObjectFactoryProvider(), deleteFilesAfterProcessed: true);

			ServiceLogger.Information("Process completed successfully");
		}

		public IErrorReportingClientProvider ErrorReportingClientProvider { get; set; } = new ErrorReportingClientProvider();

		string GetErrorLogDirectory()
		{
			string result = ReportProcessorHelper.GetReportDirectoryPath("IssueProcessor");
			if (!Directory.Exists(result))
			{
				Directory.CreateDirectory(result);
			}
			return result;
		}

		async Task DownloadAsync(string errorLogDirectory)
		{
			DownloadFromEmail(errorLogDirectory);
			await DownloadFromWebServiceAsync(errorLogDirectory).ConfigureAwait(false);
		}

		void DownloadFromEmail(string errorLogDirectory)
		{
			try
			{
				new IssueManagerEmailDownloader(ServiceLogger, errorLogDirectory).RunTask();
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				if (ServiceLogger != null)
				{
					ServiceLogger.Log(LogType.Error, "Error downloading emails", ex);
				}
			}
		}

		internal async Task DownloadFromWebService_ForTestAsync(string errorLogDirectory)
		{
			await DownloadFromWebServiceAsync(errorLogDirectory).ConfigureAwait(false);
		}

		internal void CleanUpTmpFiles_ForTest(string errorLogDirectory)
		{
			CleanUpTmpFiles(errorLogDirectory);
		}

		void CleanUpTmpFiles(string errorLogDirectory)
		{
			ServiceLogger.Log(LogType.Information, "Start cleanup tmp file");
			var tmpFiles = Directory.EnumerateFiles(errorLogDirectory, "*.tmp").Take(EDIDataRegistry.Instance.ErrorReportingServiceMaxResults.Value / 2);
			foreach (var tmpFile in tmpFiles)
			{
				try
				{
					File.Move(
						tmpFile,
						Path.Combine(Path.GetDirectoryName(tmpFile), Path.GetFileNameWithoutExtension(tmpFile) + ".xml"));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ServiceLogger.Log(LogType.Warning, "Error renaming tmp file", ex);
				}
			}
			ServiceLogger.Log(LogType.Information, "Cleanup tmp file completed");
		}

		async Task DownloadFromWebServiceAsync(string errorLogDirectory)
		{
			if (!IsWebServiceDownloadEnabled())
			{
				ServiceLogger.Log(LogType.Error, "Error Reporting Service integration is disabled for non-Production installations.");
				return;
			}

			var serviceURIs = EDIDataRegistry.Instance.ErrorReportingServiceURIs.Value;
			var maxNumberOfReports = EDIDataRegistry.Instance.ErrorReportingServiceMaxResults.Value;
			var accessToken = EDIDataRegistry.Instance.ErrorReportingServiceAccessToken.Value;

			foreach (var uri in serviceURIs)
			{
				await DownloadFromWebServiceAsync(errorLogDirectory, uri, accessToken, maxNumberOfReports).ConfigureAwait(false);
			}
		}

		async Task DownloadFromWebServiceAsync(string errorLogDirectory, string serviceUri, string accessToken, int maxNumberOfReports)
		{
			foreach (var type in new[] { ErrorReportType.EnterpriseXml, ErrorReportType.GlowXmlEnterpriseCompatible })
			{
				await DownloadFromWebServiceAsync(errorLogDirectory, serviceUri, accessToken, type, maxNumberOfReports).ConfigureAwait(false);
			}
		}

		async Task DownloadFromWebServiceAsync(string errorLogDirectory, string serviceUri, string accessToken, ErrorReportType type, int maxNumberOfReports)
		{
			try
			{
				IErrorReportingClient client;
				try
				{
					client = ErrorReportingClientProvider.CreateClient(new Uri(serviceUri));
				}
				catch (UriFormatException ex)
				{
					ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Error Reporting Service URI '{0}' is not a valid URI.", serviceUri), ex);
					throw;
				}

				using (client)
				{
					var reportsIdentifiers = await client.RetrieveCrashReportIdentifiersAsync(type, accessToken, maxNumberOfReports).ConfigureAwait(false);
					var reportCount = 0;

					foreach (var reportIdentifier in reportsIdentifiers)
					{
						var fileName = Path.Combine(errorLogDirectory, ZGuid.NewZGuid() + ".tmp");
						try
						{
							using (var stream = await client.RetrieveCrashReportAsync(type, reportIdentifier, accessToken).ConfigureAwait(false))
							using (var fs = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
							{
								stream.CopyTo(fs, 64 * 1024);
								fs.SetLength(fs.Position);
							}

							File.Move(fileName, Path.ChangeExtension(fileName, ".xml"));

							await client.DeleteCrashReportAsync(type, reportIdentifier, accessToken).ConfigureAwait(false);
							reportCount++;
						}
						catch (ErrorReportUnavailableException ex)
						{
							ServiceLogger.Log(LogType.Warning, string.Format("Failed to download report with ID {0} of type {1} from {2}: {3}", reportIdentifier, type, serviceUri, ex.Message));
							continue;
						}
					}

					ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Downloaded {0} error report{1} from {2} for type {3}", reportCount, reportCount == 1 ? string.Empty : "s", serviceUri, type));
				}
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
				if (ServiceLogger != null)
				{
					if (ex is TaskCanceledException)
					{
						ServiceLogger.Log(LogType.Warning, string.Format("Timeout when downloading from web service : {0}", serviceUri), ex);
					}
					else
					{
						ServiceLogger.Log(LogType.Error, string.Format("Error downloading from web service : {0}", serviceUri), ex);
					}
				}
			}
		}

		static bool IsWebServiceDownloadEnabled() => IsProductionSystem() || EDIDataRegistry.Instance.ErrorReportingServiceTesting.Value;

		static bool IsProductionSystem() => ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Production;

#if DEBUG
		internal
#endif
		void Process(string errorLogDirectory, BusinessObjectFactoryProvider factoryProvider, bool deleteFilesAfterProcessed)
		{
			var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(factoryProvider.Current);
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				factoryProvider.Current.RefreshEnabled = false;

				var maxReportSize = EDIDataRegistry.Instance.MaxErrorReportSizeInMb.Value * 1024L * 1024L;
				RenameBackNotLargeFiles();

				foreach (var report in GetAllReportsOrderedBySize())
				{
					var filePath = report.FilePath;
					var factoryNewlyCreatedAfterSave = false;

					try
					{
						if (FileIsLarge(report.FileSize))
						{
							File.Move(filePath, Path.ChangeExtension(filePath, ".big"));
							ServiceLogger.Log(LogType.Warning, $"Found large file [{filePath}]");
							continue;
						}

						ServiceLogger.Log(LogType.Debug, $"Processing [{Path.GetFileName(filePath)}]");

						new ExceptionXml(ErrorLogHelper.GetNormalizedContent(filePath))
							.Process(new ScalableHelpErrorLogCollection(factoryProvider.Current), false, true);

						factoryProvider.SaveCurrentAndCreateNew();
						factoryNewlyCreatedAfterSave = true;

						if (deleteFilesAfterProcessed)
						{
							DeleteFile(filePath);
						}
					}
					catch (UnauthorizedAccessException ex)
					{
						// The email downloader may be in the process of creating the file.
						// Leave it for next time.
						ServiceLogger.Log(LogType.Error, filePath, ex);
					}
					catch (ZSaveException saveException)
					{
						var newFilePath = Path.ChangeExtension(filePath, ".bad");
						File.Move(filePath, newFilePath);
						ServiceLogger.Log(LogType.Error, $"Could not save data retrieved from [{newFilePath}]", saveException);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ServiceLogger.Log(LogType.Warning, $"Unexpected error while processing error report [{filePath}]", ex);
					}

					if (!factoryNewlyCreatedAfterSave)
					{
						factoryProvider.CreateNewWithoutSave();
					}
				}

				void DeleteFile(string filePath)
				{
					try
					{
						File.Delete(filePath);
					}
					catch (IOException ex)
					{
						ServiceLogger.Log(LogType.Error, $"Error deleting file [{filePath}]", ex);
					}
				}

				void RenameBackNotLargeFiles()
				{
					foreach (var filePath in Directory.EnumerateFiles(errorLogDirectory, "*.big"))
					{
						if (!FileIsLarge(GetFileSize(filePath)))
						{
							File.Move(filePath, Path.ChangeExtension(filePath, ".xml"));
						}
					}
				}

				(string FilePath, long FileSize)[] GetAllReportsOrderedBySize()
				{
					return Directory.EnumerateFiles(errorLogDirectory, "*.xml")
						.Select(x => (FilePath: x, FileSize: GetFileSize(x)))
						.OrderBy(x => x.FileSize)
						.ToArray();
				}

				long GetFileSize(string filePath)
				{
					return new FileInfo(filePath).Length;
				}

				bool FileIsLarge(long length)
				{
					return length > maxReportSize;
				}
			}
		}

		public const string Code = "IMP";
	}
}
