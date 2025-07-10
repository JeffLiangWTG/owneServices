using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DigitalSignature;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.PrintProcessing;
using Enterprise.PrintProcessing.Billing;
using Enterprise.ServiceManager.Tasks.PrintJobProcessor;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.DocumentEngineCore.Registry.DocumentSigningRegistryConstants;

[assembly: HostedService(DocumentSigningServiceTask.ServiceTaskCode,
	DocumentSigningServiceTask.ServiceTaskDescription,
	"DOC",
	typeof(DocumentSigningServiceTask),
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "5minutes",
	MaximumPeriod = "1day",
	DefaultScheduleRunEvery = "10minutes"
	)]

[assembly: HostedServiceQueueProvider(DocumentSigningServiceTask.ServiceTaskCode, "Print Jobs for signing", typeof(DocumentSigningServiceTaskQueue))]
namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	public class DocumentSigningServiceTaskQueue : IHostedServiceQueueProvider
	{
		QueueResult IHostedServiceQueueProvider.QueueResult
		{
			get
			{
				var result = QueueResult.Error;
				Db.Connection.ExecuteReader($"SELECT COUNT(*), ISNULL(MAX(DATEDIFF(second, SP_SystemLastEditTimeUtc, GETUTCDATE())), 0) FROM dbo.StmPrintJob WHERE SP_SignBy = '{DocumentsSignBy.DOS}' AND SP_IsSigned = 0", reader =>
				{
					var count = reader.GetInt32(0);
					var age = TimeSpan.FromSeconds(reader.GetInt32(1));
					result = new QueueResult(count, age);
				});
				return result;
			}
		}
	}

	public class DocumentSigningServiceTask : ServiceProviderImpl
	{
		public const string ServiceTaskCode = "DOS";
		public const string ServiceTaskDescription = "Document Signing Service Task";
		const int MaxBatchSize = 10;
		bool hasJobsToProcess;

		public override void RunTask(CancellationToken token)
		{
			RunTaskCore(token);
		}

		void RunTaskCore(CancellationToken token)
		{
			List<ZGuid> totalJobs = new List<ZGuid>();
			int successJobs = 0;

			List<string> errorList = new List<string>();
			using (var mutexes = new DisposableList(0))
			{
				List<ZGuid> totalJobsPerBranch = new List<ZGuid>();
				int successJobsPerBranch;
				List<string> errorListPerBranch = new List<string>();

				foreach (var branch in GetBranches())
				{
					var branchPK = branch.IsValid ? branch.ToGuid() : Guid.Empty;
					using var context = branchPK != Guid.Empty
						? Env.SetTemporaryUserContext(Env.CurrentUserPK, branchPK, Env.CurrentDepartmentPK)
						: null;

					ServiceLogger.Log(Integration.LogType.Information, $"Current Branch: {Env.CurrentBranch?.Code ?? "None"}");

					int batchSize = DocumentsDataRegistry.Instance.SigningServiceProviderBatching.GetFallBackValueAtAllLevels(Guid.Empty, branchPK, Guid.Empty);

					batchSize = batchSize == 0 ? MaxBatchSize : batchSize;

					totalJobsPerBranch.Clear();
					errorListPerBranch.Clear();
					successJobsPerBranch = 0;

					var signingOption = GetSigningOption(branchPK);

					ServiceLogger.Log(Integration.LogType.Debug, $"Signing Option: {signingOption}");

					var isCredentialsValid = ValidateCredentials(branch);
					var isAccessTokenValid = ValidateAccessToken(signingOption);
					var isEndpointValid = ValidateEndpoint(branch);
					var isEnabled = !branch.IsEmpty && DocumentsDataRegistry.Instance.EnableDocumentSigningService.GetFallBackValueAtAllLevels(Guid.Empty, branch.ToGuid(), Guid.Empty);

					var isServiceValid = isCredentialsValid && isAccessTokenValid && isEndpointValid;

					if (!isServiceValid)
					{
						string message = "Incomplete configuration.";
						if (!isEndpointValid)
						{
							message += " API endpoint is required.";
						}

						if (!isCredentialsValid)
						{
							message += " Credentials invalid.";
						}

						if (!isAccessTokenValid)
						{
							message += " Access Token invalid.";
						}

						message += " Reverting to NON.";

						ServiceLogger.Log(Integration.LogType.Warning, message);
					}

					hasJobsToProcess = false;
					var printJobsQueue = new DbOnlyBusinessObjectQueue<StmPrintJob>(GetJobsToSign(branch));
					do
					{
						hasJobsToProcess = false;
						printJobsQueue.ProcessBatch((printJobs, e) =>
						{
							if (!isEnabled)
							{
								ServiceLogger.Log(Integration.LogType.Information, $"This service task is disabled in registry for the Company [{printJobs[0].Branch?.Company?.GC_Name ?? string.Empty}]. Documents --> Document Signing Service --> Enable Document Signing Service");
							}

							var lockedJobs = TryToLockJobs(printJobs, mutexes, Db.Connection);
							for (int i = 0; i < lockedJobs.Length; i++)
							{
								if (!totalJobs.Contains(lockedJobs[i].PK))
								{
									totalJobs.Add(lockedJobs[i].PK);
								}
								if (!totalJobsPerBranch.Contains(lockedJobs[i].PK))
								{
									totalJobsPerBranch.Add(lockedJobs[i].PK);
								}
							}

							bool needToSave = false;
							e.Cancel |= token.IsCancellationRequested;
							if (!e.Cancel)
							{
								if (lockedJobs.Any())
								{
									hasJobsToProcess = true;

									if (isServiceValid && isEnabled)
									{
										needToSave = ValidateAndConvertWithPlaceholder(lockedJobs);
									}
									else
									{
										foreach (var job in lockedJobs)
										{
											job.SP_SignBy = DocumentsSignBy.NON;
										}

										needToSave = true;
									}

									var goodJobs = lockedJobs.Where(j => j.SP_SignBy == DocumentsSignBy.DOS).ToArray();

									ServiceLogger.Log(Integration.LogType.Debug, $"Processing files: [{string.Join(", ", goodJobs.Select(j => j.PK))}]");
									var errors = ProcessJobGroup(goodJobs, signingOption);

									if (goodJobs.Length == 0 && needToSave)
									{
										lockedJobs[0].Factory.Save();
									}

									successJobs = successJobs + goodJobs.Length - errors.Length;
									successJobsPerBranch = successJobs + goodJobs.Length - errors.Length;

									foreach (var error in errors)
									{
										if (error.Source.Equals(JobResponseInProcessJobGroup) && !errorList.Contains(error.Message))
										{
											errorList.Add(error.Message);
										}
										else if (!errorList.Contains(error.ToString()))
										{
											errorList.Add(error.ToString());
										}

										if (!errorListPerBranch.Contains(error.Message))
										{
											errorListPerBranch.Add(error.Message);
										}
									}
								}
							}
						}, batchSize, token);
					} while (hasJobsToProcess);

					if (totalJobsPerBranch.Count > 0 && successJobsPerBranch == 0 && errorListPerBranch.Count > 0)
					{
						SendNotificationIfRequired(branch, Res.GetString("66D24BEB-DB09-4890-99DF-260E7F62591D", "Document Signing Processing Error"), GetErrorMessage(totalJobsPerBranch.Count, errorListPerBranch), null, null);
					}

					var notification = GetCertificateExpiryNotificationForCurrentBranchIfRequired();
					if (isEnabled && !notification.IsNullOrEmpty())
					{
						if (SendNotificationIfRequired(branch, Res.GetString("74bac8c6-23c2-415b-b443-25f4b22d95a1", "Digital Signing Certificate Expiry"), notification, (NoResString)"DSCcertificateRenewals@wisetechglobal.com", Res.GetString("ffdf7d55-d309-41de-8b62-562476c980fa", "DS Certificate Renewal Service")))
						{
							DocumentsDataRegistry.Instance.DocumentSigningServiceLastCertificateExpiryNotificationDate.SetValue(Guid.Empty, branchPK, Guid.Empty, ZDateTime.UtcToday.ToDateTime());
						}
					}
				}
			}

			ServiceLogger.Log(Integration.LogType.Information, $"Finished processing queue. Total jobs {totalJobs.Count}, successful jobs {successJobs}.");

			if (totalJobs.Count > 0 && successJobs == 0 && errorList.Count > 0)
			{
				var reasonableErrors = errorList.Where(e =>
					!e.Contains(DocumentSigningConstants.EMD_InternalErrorPrefix)
					&& !e.Contains(DigitalSignConstants.DGS_InternalErrorPrefix)
					&& !DocumentSigningConstants.ErrorCodesToIgnoreForReporting.Any(code => e.Contains(code)));

				if (reasonableErrors.Any())
				{
					ErrorReporter.ReportOnce("DocumentSigningFailed", GetErrorMessage(totalJobs.Count, reasonableErrors));
				}
			}
		}

		static string GetErrorMessage(int totalJobs, IEnumerable<string> errorList)
		{
			return $"No jobs have been processed out of {totalJobs}. Distinct errors:" + System.Environment.NewLine + string.Join(System.Environment.NewLine, errorList);
		}

		bool ValidateAccessToken(string signingOption)
		{
			if (signingOption == PdfSigningOptionCodes.DigitalSign)
			{
				var accessToken = PdfSignatureFactory.GetDocumentSigningServiceAccessToken();
				if (string.IsNullOrEmpty(accessToken))
				{
					return false;
				}
			}

			return true;
		}

		bool ValidateCredentials(ZGuid branch)
		{
			var config = PdfSignatureFactory.GetDocumentSigningServiceCredentialsConfiguration(branch);
			if (config == null)
			{
				return false;
			}

			if (config.ProviderCode.IsEmpty || config.AccessKey.IsEmpty || config.ClientID.IsEmpty || config.KeyID.IsEmpty)
			{
				ServiceLogger.Log(Integration.LogType.Debug, $"ProviderCode exists: {!config.ProviderCode.IsEmpty}, AccessKey exists: {!config.AccessKey.IsEmpty}, ClientID exists: {!config.ClientID.IsEmpty}, KeyID exists: {!config.KeyID.IsEmpty}");
				return false;
			}

			return true;
		}

		string GetSigningOption(Guid branch)
		{
			var signingOptionProvider = DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.GetFallBackValueAtAllLevels(Guid.Empty, branch, Guid.Empty);
			if (signingOptionProvider != null)
			{
				return signingOptionProvider.ProviderCode;
			}

			return string.Empty;
		}

		bool ValidateEndpoint(ZGuid branch)
		{
			var url = PdfSignatureFactory.GetCloudSigningServiceProviderAPIEndpoint(branch);

			return !string.IsNullOrEmpty(url);
		}

		bool SendNotificationIfRequired(ZGuid branch, string subject, string message, string fromAddress, string fromDisplayName)
		{
			var notificationGroup = DocumentsDataRegistry.Instance.DocumentSigningNotificationGroup.GetFallBackValueAtAllLevels(Guid.Empty, branch.IsEmpty ? Guid.Empty : branch.ToGuid(), Guid.Empty);

			if (notificationGroup != Guid.Empty)
			{
				var email = new EmailDef();
				email.Subject = subject;
				email.Body = message;
				if (!fromAddress.IsNullOrEmpty())
				{
					email.FromAddress = fromAddress;
				}
				if (!fromDisplayName.IsNullOrEmpty())
				{
					email.FromDisplayName = fromDisplayName;
				}

				try
				{
					Env.OutgoingMailManager.CreateAndSave(email, notificationGroup, GroupSourceLocator.GetFromRegistryItem(DocumentsDataRegistry.Instance.DocumentSigningNotificationGroup));
					return true;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ServiceLogger.Log(Integration.LogType.Warning, $"Unable to send notification email. Error: {ex.Message}");
				}
			}
			return false;
		}

		string GetCertificateExpiryNotificationForCurrentBranchIfRequired()
		{
			var branchPK = Env.CurrentBranch?.PK;
			if (!branchPK.HasValue)
			{
				return null;
			}

			var expiryDate = DocumentsDataRegistry.Instance.DocumentSigningServiceCertificateExpiryDate.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK == Guid.Empty ? Guid.Empty : Env.CurrentCompanyPK, branchPK.Value == Guid.Empty ? Guid.Empty : branchPK.Value, Guid.Empty);
			if (expiryDate == default || expiryDate < ZDateTime.UtcNow)
			{
				return null;
			}
			var lastNotifiedDate = DocumentsDataRegistry.Instance.DocumentSigningServiceLastCertificateExpiryNotificationDate.GetFallBackValueAtAllLevels(Guid.Empty, branchPK.Value == Guid.Empty ? Guid.Empty : branchPK.Value, Guid.Empty);
			var timeLeftUntilExpiry = (expiryDate - ZDateTime.UtcNow).TotalDays;

			var notifyConditions = new (double upperDate, double lowerDate, int days)[]
			{
				(60, 30, -60),
				(30, 14, -30),
				(14, 7, -14),
				(7, 1, -7),
				(1, 0, -1)
			};

			foreach (var (upperDate, lowerDate, days) in notifyConditions)
			{
				if (timeLeftUntilExpiry <= upperDate && timeLeftUntilExpiry > lowerDate && (expiryDate.AddDays(days) > lastNotifiedDate || lastNotifiedDate == default))
				{
					return $"Digital Signing Service Certificate is expiring in {timeLeftUntilExpiry.FormatWithNoMoreThanTwoDecimalPlaces()} day(s) for [{Env.CurrentBranch?.Name}]. Please check the certificate expiry date in the registry and update it if required.";
				}
			}

			return null;
		}

		string JobResponseInProcessJobGroup => "Job response in DocumentSigningServiceTask";

		Exception[] ProcessJobGroup(StmPrintJob[] jobs, string signingOption)
		{
			if (jobs == null || jobs.Length == 0)
			{
				return Array.Empty<Exception>();
			}

			var errors = new List<Exception>();
			var documentsToSignAsBytes = ReadDocumentsToSignAsBytes(jobs);
			var batchSigner = PdfSignatureFactory.NewPdfBatchSigner(signingOption, jobs[0].SP_GB);
			var signedJobs = new List<StmPrintJob>();
			var jobTransactionIds = new Dictionary<ZGuid, ZString>();

			if (documentsToSignAsBytes.Count > 0)
			{
				ServiceLogger.Log(Integration.LogType.Information, $"Processing a batch of {documentsToSignAsBytes.Count} files");

				ProcessBatchSign(batchSigner, jobs, signedJobs, errors, jobTransactionIds, documentsToSignAsBytes);
			}
			else
			{
				foreach (var job in jobs)
				{
					var error = $"Unable to read an attachment from a job [{job.SP_DocumentName}]";
					ProcessJobFailed(error, job, null, errors);
				}
			}

			try
			{
				jobs[0].Factory.Save();
				ReportJobs(signedJobs, jobs.Except(signedJobs), jobTransactionIds, signingOption);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ReportJobs(Enumerable.Empty<StmPrintJob>(), jobs, jobTransactionIds, signingOption);
				throw;
			}

			return errors.ToArray();
		}

		readonly protected List<ZGuid> allSignedJobs = new List<ZGuid>();
		protected void AddProcessedJob(StmPrintJob job)
		{
			allSignedJobs.Add(job.PK);
		}

		void ProcessBatchSign(IPdfBatchSigner batchSigner, StmPrintJob[] jobs, List<StmPrintJob> signedJobsInGroup, List<Exception> errors, Dictionary<ZGuid, ZString> jobTransactionIds, Dictionary<string, byte[]> documentsToSignAsBytes)
		{
			try
			{
				jobs = FailAndRemoveReprocessingJobs(jobs, errors, documentsToSignAsBytes);

				if (jobs.Length == 0 || documentsToSignAsBytes.Count == 0)
				{
					return;
				}

				var response = BatchSign(batchSigner, documentsToSignAsBytes);

				if (response.Length != jobs.Length)
				{
					throw new InvalidOperationException($"Wrong number of responses: {response.Length}, batch size: {jobs.Length}");
				}

				var documentsToSignAsBytesList = documentsToSignAsBytes.Values.ToList();

				for (int i = 0; i < jobs.Length; i++)
				{
					if (string.IsNullOrEmpty(response[i].ErrorMessage))
					{
						jobs[i].SP_CustomProperties = documentsToSignAsBytesList[i];
						jobs[i].SP_IsSigned = true;
						jobs[i].SP_RetryAttempts = 0;
						jobs[i].SP_Status = nameof(PrintJobStatus.QUE);
						jobs[i].SP_FailureReason = ZString.Empty;
						jobs[i].SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdf;
						jobs[i].SP_EmailAttachments = Path.GetFileName(jobs[i].StoredAttachmentFilename);
						ServiceLogger.Log(Integration.LogType.Information, $"Signing successful. File [{jobs[i].SP_DocumentName}], {jobs[i].PK}");

						signedJobsInGroup.Add(jobs[i]);
						AddProcessedJob(jobs[i]);
					}
					else
					{
						var logMessage = $"Signing failed. File [{jobs[i].SP_DocumentName}], {jobs[i].PK}, error: {response[i].ErrorMessage}, retry {jobs[i].SP_RetryAttempts + 1} of {PrintJobManager.MaxRetryAttempts}";
						ProcessJobFailed(logMessage, jobs[i], response[i], errors);
					}

					jobTransactionIds.Add(jobs[i].PK, response[i].TransactionId);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var failReason = new ZString(WebServiceError + ex.Message).SubstringSafe(0, AutoStmPrintJob.Schema.SP_FailureReasonMaxLength);

				for (int i = 0; i < jobs.Length; i++)
				{
					jobs[i].SP_RetryAttempts++;
					if (jobs[i].SP_RetryAttempts >= PrintJobManager.MaxRetryAttempts)
					{
						jobs[i].SP_FailureReason = failReason;
					}
					errors.Add(ex);
				}

				ServiceLogger.Log(Integration.LogType.Warning, "Error contacting remote web service. Error: " + ex.Message + System.Environment.NewLine + "Stack Trace: " + ex.StackTrace);
			}
			finally
			{
				for (int i = 0; i < jobs.Length; i++)
				{
					for (int retry = 0; retry < 3; retry++)
					{
						try
						{
							if (File.Exists(jobs[i].StoredAttachmentFilename))
							{
								File.Delete(jobs[i].StoredAttachmentFilename);
							}

							break;
						}
						catch (IOException ex) when (!ex.IsCriticalException())
						{
							Thread.Sleep(200);
						}
					}
				}
			}
		}

		StmPrintJob[] FailAndRemoveReprocessingJobs(StmPrintJob[] initialJobs, List<Exception> errors, Dictionary<string, byte[]> documentsToSignAsBytes)
		{
			var reprocessingJobs = initialJobs.Where(job => allSignedJobs.Contains(job.PK));

			if (reprocessingJobs.Any())
			{
				foreach (var job in reprocessingJobs)
				{
					var logMessage = $"Job has already been signed once in this session and might be stuck [{job.SP_DocumentName}]";
					ProcessJobFailed(logMessage, job, null, errors, true);
					documentsToSignAsBytes.Remove(job.PK.ToString());
				}
			}

			return initialJobs.Except(reprocessingJobs).ToArray();
		}

		void ProcessJobFailed(string logMessage, StmPrintJob job, SignResult response, List<Exception> errors, bool forceFailJob = false)
		{
			var errorMessage = response?.ErrorMessage ?? logMessage;
			var error = new Exception(errorMessage);

			ServiceLogger.Log(Integration.LogType.Warning, logMessage);
			if (forceFailJob)
			{
				job.SP_RetryAttempts = PrintJobManager.MaxRetryAttempts;
			}
			else
			{
				job.SP_RetryAttempts++;
			}
			var branchPK = job.SP_GB.IsValid ? job.SP_GB.ToGuid() : Guid.Empty;
			job.SP_RunDateTime = ZDateTime.UtcNow.AddMinutes(DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.GetFallBackValueAtAllLevels(Guid.Empty, branchPK, Guid.Empty));

			if (job.SP_RetryAttempts >= PrintJobManager.MaxRetryAttempts)
			{
				var failReason = new ZString(errorMessage).SubstringSafe(0, StmPrintJobSchema.SP_FailureReason.MaxLength);
				job.SP_FailureReason = failReason;
			}

			error.Source = JobResponseInProcessJobGroup;
			errors.Add(error);
		}

		protected virtual void ReportJobs(IEnumerable<StmPrintJob> signedJobs, IEnumerable<StmPrintJob> failedJobs, Dictionary<ZGuid, ZString> jobTransactionIds, string signingOption)
		{
			if (signedJobs.Any() || failedJobs.Any())
			{
				ServiceLogger.Log(Integration.LogType.Debug, $"Reporting successful jobs: [{string.Join(", ", signedJobs.Select(j => j.PK))}]. Reporting failed jobs: [{string.Join(", ", failedJobs.Select(j => j.PK))}]");
				var billingManager = new DocumentSigningBillingManager();
				foreach (var job in signedJobs)
				{
					billingManager.LogUsage(job, signingOption, DocumentSigningStatus.Success, GetTransactionId(jobTransactionIds, job), ServiceLogger);
				}

				foreach (var job in failedJobs)
				{
					billingManager.LogUsage(job, signingOption, DocumentSigningStatus.Fail, GetTransactionId(jobTransactionIds, job), ServiceLogger);
				}

				billingManager.Factory.Save();
			}
		}

		static ZString GetTransactionId(Dictionary<ZGuid, ZString> jobTransactionIds, StmPrintJob job)
		{
			ZString id;
			jobTransactionIds.TryGetValue(job.PK, out id);
			return id;
		}

		Dictionary<string, byte[]> ReadDocumentsToSignAsBytes(IEnumerable<StmPrintJob> jobGroup)
		{
			var documentsAsBytes = new Dictionary<string, byte[]>();
			foreach (var job in jobGroup)
			{
				for (int tries = 0; tries < 3; tries++)
				{
					try
					{
						documentsAsBytes.Add(job.PK.ToGuid().ToString(), File.ReadAllBytes(job.StoredAttachmentFilename));
						ServiceLogger.Log(Integration.LogType.Debug, $"Read file bytes: [{job.PK}]");
						break;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						if ((tries + 1) < 3)
						{
							Thread.Sleep(200);
						}
						else
						{
							ServiceLogger.Log(Integration.LogType.Warning, $"Unable to read the file: [{job.StoredAttachmentFilename}]");
						}
					}
				}
			}

			return documentsAsBytes;
		}

		bool ValidateAndConvertWithPlaceholder(StmPrintJob[] lockedJobs)
		{
			bool needSave = false;
			int index = 0;
			foreach (var job in lockedJobs)
			{
				if (job.SP_JobType != nameof(PrintJobType.EML) && job.SP_JobType != nameof(PrintJobType.DDS))
				{
					ServiceLogger.Log(Integration.LogType.Warning, $"Incorrect job type: [{job.SP_JobType}]");
					job.SP_SignBy = DocumentsSignBy.NON;
					needSave = true;

					continue;
				}

				if (
					job.SP_EmailAttachmentFormat != AttachmentTypeList.Codes.Pdf
					&& job.SP_EmailAttachmentFormat != AttachmentTypeList.Codes.Pdfa
					&& job.SP_JobType != nameof(PrintJobType.DDS)
					)
				{
					ServiceLogger.Log(Integration.LogType.Warning, $"Cannot sign attachment format: [{job.SP_EmailAttachmentFormat}], file: [{job.SP_DocumentName}]");
					job.SP_SignBy = DocumentsSignBy.NON;
					needSave = true;

					continue;
				}

				if (job.SP_JobType == nameof(PrintJobType.DDS))
				{
					job.SP_EmailAttachmentFormat = AttachmentTypeList.Codes.Pdf;
				}

				try
				{
					job.SaveAttachmentToFilesystem(++index);
					ServiceLogger.Log(Integration.LogType.Debug, $"Saved file bytes: [{job.PK}]");
				}
				catch (ExcelInterfaceException e)
				{
					var message = new StringBuilder();
					message.AppendLine($"Job Information: ");
					message.AppendLine($"PK: {job.PK}");
					message.AppendLine($"SP_JobType: {job.SP_JobType}");
					message.AppendLine($"SP_SignBy: {job.SP_SignBy}");
					message.AppendLine($"SP_EmailAttachmentFormat: {job.SP_EmailAttachmentFormat}");
					message.AppendLine($"SP_IsSigned: {job.SP_IsSigned}");
					message.AppendLine($"SP_RetryAttempts: {job.SP_RetryAttempts}");
					ErrorReporter.ReportOnce("DOS service task error in SaveAttachmentToFilesystem", message.ToString(), e);
					throw;
				}
			}

			return needSave;
		}

		const string WebServiceError = "Web service error";

		protected virtual SignResult[] BatchSign(IPdfBatchSigner batchSigner, Dictionary<string, byte[]> documentsAsBytes)
		{
			ServiceLogger.Log(Integration.LogType.Debug, $"Signing: [{string.Join(", ", documentsAsBytes.Keys)}]");
			return batchSigner.Sign(documentsAsBytes).ToArray();
		}

		StmPrintJob[] TryToLockJobs(StmPrintJob[] printJobs, DisposableList mutexes, DbConnection connection)
		{
			try
			{
				SqlApplicationLock mutex;

				if (printJobs.Any() && PrintJobTaskCore.TryGetLockOnDeliveryGroupGuid(connection, printJobs.First(), out mutex))
				{
					mutexes.Add(mutex);

					return printJobs.Where(job => JobStillExistsAndAvailable(job)).ToArray();
				}
			}
			//If our db connection is dropped then we lose all our locks. 
			//We should not process jobs unless we know we have locks on them.
			catch (SqlException sqlException)
			{
				ServiceLogger.Log(Integration.LogType.Debug, $"Unable to grab mutexes");
				ErrorReporter.ReportOnce("DB Connection error while grabbing print job mutexes", sqlException);
			}

			return Array.Empty<StmPrintJob>();
		}

		bool JobStillExistsAndAvailable(StmPrintJob job)
		{
			job.ReloadSafe();
			var reloadQuery = new ZDBOnlyQuery(job.GetType()) { ReLoadExistingRows = true };
			reloadQuery.AddToFilter(StmPrintJobSchema.PK, job.PK);

			return job.Factory.LoadTop1<StmPrintJob>(reloadQuery) != null;
		}

		ZGuid[] GetBranches()
		{
			var sql = $"SELECT DISTINCT SP_GB FROM {StmPrintJobSchema.Constants.SqlSchemaName}.{StmPrintJobSchema.Constants.TableName} {GetWhereClause()} ORDER BY SP_GB";
			var result = new List<ZGuid>();

			using (var cmd = Db.Connection.Command(sql))
			{
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader[0] != DBNull.Value ? (Guid)reader[0] : Guid.Empty);
					}
				}
			}

			return result.ToArray();
		}

		ZNonPersistentDataQuery GetJobsToSign(ZGuid branch)
		{
			var sql = $@"
SELECT {StmPrintJobSchema.Constants.PK} from {StmPrintJobSchema.Constants.SqlSchemaName}.{StmPrintJobSchema.Constants.TableName}
{GetWhereClause(branch)}
ORDER BY {StmPrintJobSchema.Constants.SP_GB}, {StmPrintJobSchema.Constants.SP_RetryAttempts}, {StmPrintJobSchema.Constants.SP_RunDateTime}";
			return new ZNonPersistentDataQuery(sql);
		}

		string GetWhereClause()
		{
			return GetWhereClause(ZGuid.Missing);
		}

		string GetWhereClause(ZGuid branch)
		{
			var branchFilter = branch == ZGuid.Missing
				? string.Empty
				: branch == ZGuid.Empty
					? $"AND {StmPrintJobSchema.Constants.SP_GB} is null"
					: $"AND {StmPrintJobSchema.Constants.SP_GB} = '{branch}'";

			return $@"WHERE 1=1
AND {StmPrintJobSchema.Constants.SP_IsSigned} = 0
AND {StmPrintJobSchema.Constants.SP_SignBy} = '{ServiceTaskCode}'
AND {StmPrintJobSchema.Constants.SP_Status} <> '{PrintJobStatus.FAL}'
AND {StmPrintJobSchema.Constants.SP_RetryAttempts} < {PrintJobManager.MaxRetryAttempts}
AND {PrintJobTaskCore.GetLockConditionOnDeliveryGroupGuid()}
AND {PrintJobTaskCore.GetLockConditionOnParentGuid()}
AND {StmPrintJobSchema.Constants.SP_RunDateTime} < getutcdate()
{branchFilter}";
		}
	}
}
