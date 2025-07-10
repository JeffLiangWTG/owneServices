using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Business.Testing;
using Enterprise.Billing.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DigitalSignature;
using Enterprise.DocumentEngine.FlexCelInterface.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.PrintProcessing;
using Enterprise.PrintProcessing.Billing.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;
using static Enterprise.DocumentEngineCore.Registry.DocumentSigningRegistryConstants;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	[TestedType(typeof(DocumentSigningServiceTask))]
	sealed class DocumentSigningServiceTaskTest : ServiceTaskTestCase<DocumentSigningServiceTask>
	{
		public void TestNoBranch_HasOtherJobs()
		{
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(Guid.Empty, Branch3.PK.ToGuid(), Guid.Empty, new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "1", ClientID = "1", KeyID = "1" });
			DocumentsDataRegistry.Instance.CloudSigningServiceProviderAPIEndpoint.SetValue(Branch3.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "google.com");

			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job1 = GetTestPrintJob(nameof(PrintJobType.EML), ZGuid.Empty, deliveryGroup, branchOverride: ZGuid.Empty);
			var job2 = GetTestPrintJob(nameof(PrintJobType.EML), ZGuid.Empty, deliveryGroup);
			var blob1 = job1.SP_CustomProperties;
			var blob2 = job2.SP_CustomProperties;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 1 files", log, 1);
				AssertLogsContain("Warning|Incomplete configuration. API endpoint is required. Credentials invalid. Reverting to NON.", log, 1);
				AssertLogsContain($"Information|Signing successful. File [{job2.SP_DocumentName}], {job2.PK}", log, 1);
				AssertLogsContain($"Information|This service task is disabled in registry for the Company []. Documents --> Document Signing Service --> Enable Document Signing Service", log, 1);
				AssertLogsContain("Information|Finished processing queue. Total jobs 2, successful jobs 1.", log, 1);

				AssertNoEmails();
				AssertSignedOnce(task);
			});

			AssertJobRevertedToNON(job1.PK, blob1, new BusinessObjectFactory());
		}

		public void TestNoBranch()
		{
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(Guid.Empty, Branch3.PK.ToGuid(), Guid.Empty, new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "1", ClientID = "1", KeyID = "1" });
			DocumentsDataRegistry.Instance.CloudSigningServiceProviderAPIEndpoint.SetValue(Branch3.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "google.com");

			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job = GetTestPrintJob(nameof(PrintJobType.EML), ZGuid.Empty, deliveryGroup, branchOverride: ZGuid.Empty);
			var blob = job.SP_CustomProperties;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			CombineAssertions(() =>
			{
				AssertLogsContain($"Information|This service task is disabled in registry for the Company []. Documents --> Document Signing Service --> Enable Document Signing Service", log, 1);
				AssertLogsContain("Information|Finished processing queue. Total jobs 1, successful jobs 0.", log, 1);

				AssertNoEmails();
				AssertSignedOnce(task);
			});

			AssertJobRevertedToNON(job.PK, blob, new BusinessObjectFactory());
		}

		[TestDate(2025, 1, 1, 1, 1, 1)]
		public void TestPlaceholderErrorFailsJob()
		{
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				AssertPlaceholderErrorFailsJob();
			}
		}

		[TestDate(2025, 1, 1, 1, 1, 1)]
		public void TestPlaceholderErrorFailsJobWithCompany()
		{
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4))
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, 0))
			{
				AssertPlaceholderErrorFailsJob();
			}
		}

		public void AssertPlaceholderErrorFailsJob()
		{
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup);

			var responses = new List<SignResult[]>();
			responses.Add(new SignResult[]
			{
				new SignResult("PLACEHOLDER_ERROR and some other text", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("", ZGuid.NewZGuid().ToString())
			});

			task.SignResponses = responses;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);
			log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			job = newFactory.Load<StmPrintJob>(job.PK);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 1 files", log, 2);
				AssertLogsContain($"Warning|Signing failed. File [Test], {job.PK}, error: PLACEHOLDER_ERROR and some other text, retry 1 of 3", log, 1);
				AssertLogsContain($"Information|Signing successful. File [Test], {job.PK}", log, 1);
				AssertLogsContain("Information|Finished processing queue. Total jobs 1, successful jobs 1.", log, 1);
				AssertLogsContain("Information|Finished processing queue. Total jobs 0, successful jobs 0.", log, 1);

				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

				AssertSignedOnce(task);
			});

			ErrorReporter.Clear();
		}

		[TestDate(2025, 1, 1, 1, 1, 1)]
		public void TestEmudhraErrorsAreNotReportedToIssues_RSDS_BadResponse()
		{
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				AssertEmudhraErrorsAreNotReportedToIssues("RSDS-901", "BAD_RESPONSE");
			}
		}

		[TestDate(2025, 1, 1, 1, 1, 1)]
		public void TestEmudhraErrorsAreNotReportedToIssues_WebEx_Placeholder()
		{
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				AssertEmudhraErrorsAreNotReportedToIssues("WEB_EXCEPTION", "PLACEHOLDER_ERROR");
			}
		}

		[TestDate(2025, 1, 1, 1, 1, 1)]
		public void TestEmudhraErrorsAreNotReportedToIssues_RSDS_BadResponseWithCompany()
		{
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4))
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, 0))
			{
				AssertEmudhraErrorsAreNotReportedToIssues("RSDS-901", "BAD_RESPONSE");
			}
		}

		[TestDate(2025, 1, 1, 1, 1, 1)]
		public void TestEmudhraErrorsAreNotReportedToIssues_WebEx_PlaceholderWithCompany()
		{
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4))
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, 0))
			{
				AssertEmudhraErrorsAreNotReportedToIssues("WEB_EXCEPTION", "PLACEHOLDER_ERROR");
			}
		}

		void AssertEmudhraErrorsAreNotReportedToIssues(string ignoredCode1, string ignoredCode2)
		{
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup1 = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1);
			var longErrorInfo = "oopsie" + ZString.Replicate('a', StmPrintJobSchema.SP_FailureReason.MaxLength + 10);
			var interceptedLongErrorInfo = longErrorInfo.Substring(0, StmPrintJobSchema.SP_FailureReason.MaxLength);

			var responses = new List<SignResult[]>();
			responses.Add(new SignResult[]
			{
				new SignResult(ignoredCode1 + " and some other text", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult(ignoredCode2 + " and some other text", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult(longErrorInfo, ZGuid.NewZGuid().ToString())
			});

			task.SignResponses = responses;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);
			log = InitialiseAndRunTaskSchedule(task);
			log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			job = newFactory.Load<StmPrintJob>(job.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Job's SP_FailureReason should be valid", interceptedLongErrorInfo, job.SP_FailureReason);
				AssertLogsContain("Information|Processing a batch of 1 files", log, 3);
				AssertLogsContain($"Warning|Signing failed. File [Test], {job.PK}, error: {ignoredCode1} and some other text, retry 1 of 3", log, 1);
				AssertLogsContain($"Warning|Signing failed. File [Test], {job.PK}, error: {ignoredCode2} and some other text, retry 2 of 3", log, 1);
				AssertLogsContain($"Warning|Signing failed. File [Test], {job.PK}, error: {longErrorInfo}, retry 3 of 3", log, 1);
				AssertLogsContain("Information|Finished processing queue. Total jobs 1, successful jobs 0.", log, 1);
				AssertLogsContain("Information|Finished processing queue. Total jobs 0, successful jobs 0.", log, 2);

				var errorFull = $@"No jobs have been processed out of 1. Distinct errors:
{ignoredCode1} and some other text
{ignoredCode2} and some other text
{longErrorInfo}";
				var errorIssue = $@"No jobs have been processed out of 1. Distinct errors:
{longErrorInfo}";

				AssertEquals(errorIssue, ErrorReporter.LastMessageReported);

				AssertEmail(errorFull);
				AssertSignedOnce(task);
			});

			ErrorReporter.Clear();
		}

		[TestDate(2025, 1, 1, 1, 1, 1)]
		public void TestEmudhraErrorsAreNotReportedToIssues_AndNoOthers()
		{
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				AssertEmudhraErrorsAreNotReportedToIssues_AndNoOthers();
			}
		}

		[TestDate(2025, 1, 1, 1, 1, 1)]
		public void TestEmudhraErrorsAreNotReportedToIssues_AndNoOthers_WithCompany()
		{
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4))
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, 0))
			{
				AssertEmudhraErrorsAreNotReportedToIssues_AndNoOthers();
			}
		}

		public void AssertEmudhraErrorsAreNotReportedToIssues_AndNoOthers()
		{
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup1 = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1);

			var responses = new List<SignResult[]>();
			responses.Add(new SignResult[]
			{
				new SignResult("RSDS-901", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("BAD_RESPONSE", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("RSDS-901", ZGuid.NewZGuid().ToString())
			});

			task.SignResponses = responses;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);
			log = InitialiseAndRunTaskSchedule(task);
			log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			job = newFactory.Load<StmPrintJob>(job.PK);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 1 files", log, 3);
				AssertLogsContain($"Warning|Signing failed. File [Test], {job.PK}, error: RSDS-901, retry 1 of 3", log, 1);
				AssertLogsContain($"Warning|Signing failed. File [Test], {job.PK}, error: BAD_RESPONSE, retry 2 of 3", log, 1);
				AssertLogsContain($"Warning|Signing failed. File [Test], {job.PK}, error: RSDS-901, retry 3 of 3", log, 1);
				AssertLogsContain("Information|Finished processing queue. Total jobs 1, successful jobs 0.", log, 1);
				AssertLogsContain("Information|Finished processing queue. Total jobs 0, successful jobs 0.", log, 2);

				var errorFull = @"No jobs have been processed out of 1. Distinct errors:
RSDS-901
BAD_RESPONSE";
				AssertEquals("No real errors were reported, so should not send an issue.", string.Empty, ErrorReporter.LastMessageReported);

				AssertEmail(errorFull);
				AssertSignedOnce(task);
			});

			ErrorReporter.Clear();
		}

		[TestDate(2025, 1, 1, 1, 1, 1)]
		public void TestDigitalSignErrorsAreNotReportedToIssues()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var responseSet1 = new List<SignResult[]>
			{
				new SignResult[] { new SignResult("DGS-invalid_token - Invalid access token.", ZGuid.NewZGuid().ToString()) },
				new SignResult[] { new SignResult("BAD_RESPONSE", ZGuid.NewZGuid().ToString()) },
				new SignResult[] { new SignResult("DGS-021003 - Invalid TOTP Value.", ZGuid.NewZGuid().ToString()) }
			};

			var responseSet2 = new List<SignResult[]>
			{
				new SignResult[] { new SignResult("WEB_EXCEPTION", ZGuid.NewZGuid().ToString()) },
				new SignResult[] { new SignResult("PLACEHOLDER_ERROR", ZGuid.NewZGuid().ToString()) },
				new SignResult[] { new SignResult("GET_CERTIFICATE_SERVER_TIMEOUT", ZGuid.NewZGuid().ToString()) }
			};

			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				AssertDigitalSignErrorsAreNotReportedToIssues(company, responseSet1);
				AssertDigitalSignErrorsAreNotReportedToIssues(company, responseSet2);
			}

			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4))
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, 0))
			{
				AssertDigitalSignErrorsAreNotReportedToIssues(company, responseSet1);
				AssertDigitalSignErrorsAreNotReportedToIssues(company, responseSet2);
			}
		}

		void AssertDigitalSignErrorsAreNotReportedToIssues(GlbCompany company, List<SignResult[]> responses)
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			DocumentsDataRegistry.Instance.DocumentSigningServicePartnerCredentialsAccessToken.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DocumentSigningServiceCredentialsConfiguration() { AccessKey = "secretKey", ClientID = "clientID", KeyID = "accessToken" });
			DocumentsDataRegistry.Instance.EnableDocumentSigningService.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_RN_NKCountryCode = CountryCodes.Portugal;

			DocumentsDataRegistry.Instance.CloudSigningServiceProviderAPIEndpoint.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "google.com");
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.DigitalSign, AccessKey = "authId", ClientID = "authName", KeyID = "authSecretKey" });

			var task = new DocumentSigningServiceTaskForTest();
			var job = GetTestPrintJob(nameof(PrintJobType.EML), ZGuid.Empty, deliveryGroup, branchOverride: branch.PK);

			task.SignResponses = responses;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);
			log = InitialiseAndRunTaskSchedule(task);
			log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			job = newFactory.Load<StmPrintJob>(job.PK);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 1 files", log, 3);
				AssertLogsContain($"Warning|Signing failed. File [Test], {job.PK}, error: {responses[0][0].ErrorMessage}, retry 1 of 3", log, 1);
				AssertLogsContain($"Warning|Signing failed. File [Test], {job.PK}, error: {responses[1][0].ErrorMessage}, retry 2 of 3", log, 1);
				AssertLogsContain($"Warning|Signing failed. File [Test], {job.PK}, error: {responses[2][0].ErrorMessage}, retry 3 of 3", log, 1);
				AssertLogsContain("Information|Finished processing queue. Total jobs 1, successful jobs 0.", log, 1);
				AssertLogsContain("Information|Finished processing queue. Total jobs 0, successful jobs 0.", log, 2);

				AssertEquals("No reasonable errors reported to Issues.", string.Empty, ErrorReporter.LastMessageReported);
			});

			ErrorReporter.Clear();
			ClearTempFiles();
		}

		enum FindInLogsType
		{
			Exact,
			StartsWith,
			Contains
		}

		void AssertLogsContain(string message, TestServiceLogger logs, int count = 1, FindInLogsType findType = FindInLogsType.Exact)
		{
			int found = 0;
			for (int i = 0; i < logs.Count; i++)
			{
				switch(findType)
				{
					case FindInLogsType.Exact:
						if (logs[i] == message)
						{
							found++;
						}
						break;
					case FindInLogsType.StartsWith:
						if (logs[i].StartsWith(message))
						{
							found++;
						}
						break;
					case FindInLogsType.Contains:
						if (logs[i].Contains(message))
						{
							found++;
						}
						break;
					default: throw new InvalidOperationException("Unknown type");
				}
			}

			AssertEquals($"Message [{message}] expected [{count}] times but found [{found}] times", count, found);
		}

		[TestDate(2025, 1, 1, 1, 1, 1)]
		public void TestEmailNotificationsSentPerCompany()
		{
			DocumentsDataRegistry.Instance.CloudSigningServiceProviderAPIEndpoint.SetValue(Branch3.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "google.com");
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(Guid.Empty, Branch3.PK.ToGuid(), Guid.Empty, new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "1", ClientID = "1", KeyID = "1" });

			DocumentsDataRegistry.Instance.CloudSigningServiceProviderAPIEndpoint.SetValue(Branch4.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "google.com");
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(Guid.Empty, Branch4.PK.ToGuid(), Guid.Empty, new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "1", ClientID = "1", KeyID = "1" });

			DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup1 = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job1 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1);
			var job2 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1);
			var job3 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1, branchOverride: Branch3.PK);
			var job4 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1, branchOverride: Branch3.PK);
			var job5 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1, branchOverride: Branch4.PK);
			var job6 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1, branchOverride: Branch4.PK);

			var responses = new List<SignResult[]>();
			responses.Add(new SignResult[]
			{
				new SignResult("avada kedavra 1", ZGuid.NewZGuid().ToString()),
				new SignResult("avada kedavra 2", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("avada kedavra 3", ZGuid.NewZGuid().ToString()),
				new SignResult("avada kedavra 4", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("avada kedavra 5", ZGuid.NewZGuid().ToString()),
				new SignResult("avada kedavra 6", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("wingardium leviosa 1", ZGuid.NewZGuid().ToString()),
				new SignResult("wingardium leviosa 2", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("wingardium leviosa 3", ZGuid.NewZGuid().ToString()),
				new SignResult("wingardium leviosa 4", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("wingardium leviosa 5", ZGuid.NewZGuid().ToString()),
				new SignResult("wingardium leviosa 6", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("bombarda maxima 1", ZGuid.NewZGuid().ToString()),
				new SignResult("bombarda maxima 2", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("bombarda maxima 3", ZGuid.NewZGuid().ToString()),
				new SignResult("bombarda maxima 4", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("bombarda maxima 5", ZGuid.NewZGuid().ToString()),
				new SignResult("bombarda maxima 6", ZGuid.NewZGuid().ToString())
			});

			task.SignResponses = responses;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var jobAvada1 = newFactory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_FailureReason, "avada kedavra 5"));
			var jobAvada2 = newFactory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_FailureReason, "avada kedavra 6"));
			var jobWingardium1 = newFactory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_FailureReason, "wingardium leviosa 5"));
			var jobWingardium2 = newFactory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_FailureReason, "wingardium leviosa 6"));
			var jobBombarda1 = newFactory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_FailureReason, "bombarda maxima 5"));
			var jobBombarda2 = newFactory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_FailureReason, "bombarda maxima 6"));

			CombineAssertions(() =>
			{
				job1 = newFactory.Load<StmPrintJob>(job1.PK);
				job2 = newFactory.Load<StmPrintJob>(job2.PK);
				job3 = newFactory.Load<StmPrintJob>(job3.PK);
				job4 = newFactory.Load<StmPrintJob>(job4.PK);
				job5 = newFactory.Load<StmPrintJob>(job5.PK);
				job6 = newFactory.Load<StmPrintJob>(job6.PK);

				var sb = new StringBuilder();
				sb.AppendLine("Couldn't find one of the jobs. All failure reasons and retry count: ");
				sb.AppendLine($"{job1.SP_FailureReason}, {job1.SP_RetryAttempts}");
				sb.AppendLine($"{job2.SP_FailureReason}, {job2.SP_RetryAttempts}");
				sb.AppendLine($"{job3.SP_FailureReason}, {job3.SP_RetryAttempts}");
				sb.AppendLine($"{job4.SP_FailureReason}, {job4.SP_RetryAttempts}");
				sb.AppendLine($"{job5.SP_FailureReason}, {job5.SP_RetryAttempts}");
				sb.AppendLine($"{job6.SP_FailureReason}, {job6.SP_RetryAttempts}");

				sb.AppendLine("All logs:");
				for (int i = 0; i < log.Count; i++)
				{
					sb.AppendLine(log[i]);
				}

				Assert(sb.ToString(), jobAvada1 != null);
				Assert(sb.ToString(), jobAvada2 != null);
				Assert(sb.ToString(), jobWingardium1 != null);
				Assert(sb.ToString(), jobWingardium2 != null);
				Assert(sb.ToString(), jobBombarda1 != null);
				Assert(sb.ToString(), jobBombarda2 != null);
			}
			);

			AssertFailedJob(jobAvada1.PK, newFactory, 3, "avada kedavra 5");
			AssertFailedJob(jobAvada2.PK, newFactory, 3, "avada kedavra 6");
			AssertFailedJob(jobWingardium1.PK, newFactory, 3, "wingardium leviosa 5");
			AssertFailedJob(jobWingardium2.PK, newFactory, 3, "wingardium leviosa 6");
			AssertFailedJob(jobBombarda1.PK, newFactory, 3, "bombarda maxima 5");
			AssertFailedJob(jobBombarda2.PK, newFactory, 3, "bombarda maxima 6");

			EmailDef emailAvada = null;
			EmailDef emailWingardium = null;
			EmailDef emailBombarda = null;

			foreach (var email in Env.OutgoingMailManager.EmailsCreated)
			{
				AssertNotNull(email);

				if (email.Body.Contains("avada"))
				{
					emailAvada = email;
				}
				else if (email.Body.Contains("wingardium"))
				{
					emailWingardium = email;
				}
				else if (email.Body.Contains("bombarda"))
				{
					emailBombarda = email;
				}
			}

			string expectedEmail1 = NotificationEmail1;
			string expectedEmail2 = NotificationEmail2;

			if (emailAvada != null)
			{
				AssertEquals("Document Signing Processing Error", emailAvada.Subject);
				AssertEquals(@"No jobs have been processed out of 2. Distinct errors:
avada kedavra 1
avada kedavra 2
avada kedavra 3
avada kedavra 4
avada kedavra 5
avada kedavra 6", emailAvada.Body);

				if (emailAvada.Recipients[0].Email == expectedEmail1)
				{
					AssertEquals(expectedEmail1, emailAvada.Recipients[0].Email);
					expectedEmail1 = "";
				}
				else
				{
					AssertEquals(expectedEmail2, emailAvada.Recipients[0].Email);
					expectedEmail2 = "";
				}
			}

			if (emailWingardium != null)
			{
				AssertEquals("Document Signing Processing Error", emailWingardium.Subject);
				AssertEquals(@"No jobs have been processed out of 2. Distinct errors:
wingardium leviosa 1
wingardium leviosa 2
wingardium leviosa 3
wingardium leviosa 4
wingardium leviosa 5
wingardium leviosa 6", emailWingardium.Body);

				if (emailWingardium.Recipients[0].Email == expectedEmail1)
				{
					AssertEquals(expectedEmail1, emailWingardium.Recipients[0].Email);
					expectedEmail1 = "";
				}
				else
				{
					AssertEquals(expectedEmail2, emailWingardium.Recipients[0].Email);
					expectedEmail2 = "";
				}
			}

			if (emailBombarda != null)
			{
				AssertEquals("Document Signing Processing Error", emailBombarda.Subject);
				AssertEquals(@"No jobs have been processed out of 2. Distinct errors:
bombarda maxima 1
bombarda maxima 2
bombarda maxima 3
bombarda maxima 4
bombarda maxima 5
bombarda maxima 6", emailBombarda.Body);

				if (emailBombarda.Recipients[0].Email == expectedEmail1)
				{
					AssertEquals(expectedEmail1, emailBombarda.Recipients[0].Email);
					expectedEmail1 = "";
				}
				else
				{
					AssertEquals(expectedEmail2, emailBombarda.Recipients[0].Email);
					expectedEmail2 = "";
				}
			}

			AssertSignedOnce(task);

			ErrorReporter.Clear();
		}

		public void TestCertificateExpiryNotification_ForBranch()
		{
			AssertCertificateExpiryNotifications(ZGuid.Empty, Branch2.PK);
		}

		public void TestCertificateExpiryNotification_ForCompany()
		{
			AssertCertificateExpiryNotifications(Branch2.Company.PK, Branch2.PK);
		}

		public void AssertCertificateExpiryNotifications(ZGuid company, ZGuid branch)
		{
			var bodies = new List<string>();
			void TestCase(int expiryDays, int notifiedDays, bool shouldSendNotification)
			{
				var branchID = branch.IsEmpty ? Guid.Empty : branch.ToGuid();
				var companyID = company.IsEmpty ? Guid.Empty : company.ToGuid();
				var expiryDate = DateTime.UtcNow.AddDays(expiryDays);
				DocumentsDataRegistry.Instance.DocumentSigningServiceCertificateExpiryDate.SetTemporaryValue(companyID, company.IsEmpty ? branchID : Guid.Empty, Guid.Empty, expiryDate);
				DocumentsDataRegistry.Instance.DocumentSigningServiceLastCertificateExpiryNotificationDate.SetTemporaryValue(Guid.Empty, branchID, Guid.Empty, DateTime.UtcNow.AddDays(notifiedDays));
				var task = new DocumentSigningServiceTaskForTest();
				GetTestPrintJob(Enum.GetName(typeof(PrintJobType), PrintJobType.EML), ZGuid.Empty, TestAssistant.CreateNewDeliveryGroup(Factory), branchOverride: Branch2.PK);

				Factory.Save();
				InitialiseAndRunTaskSchedule(task);

				if (shouldSendNotification)
				{
					bodies.Add($"Digital Signing Service Certificate is expiring in {(expiryDate - DateTime.UtcNow).TotalDays.FormatWithNoMoreThanTwoDecimalPlaces()} day(s) for [{Branch2.GB_BranchName}]. Please check the certificate expiry date in the registry and update it if required.");
					AssertEquals("Last notified date updated correctly.", ZDateTime.UtcToday.ToDateTime(), DocumentsDataRegistry.Instance.DocumentSigningServiceLastCertificateExpiryNotificationDate.GetFallBackValueAtAllLevels(Guid.Empty, branchID, Guid.Empty).Date);
				}
			}

			TestCase(1, 0, false);
			TestCase(1, 1, false);
			TestCase(-1, -10, false);
			TestCase(90, -10, false);

			AssertNoEmails();

			TestCase(1, -10, true);
			TestCase(7, -7, true);
			TestCase(14, -7, true);
			TestCase(30, -60, true);
			TestCase(60, -30, true);

			AssertEmail("Digital Signing Certificate Expiry", bodies, "DSCcertificateRenewals@wisetechglobal.com", "DS Certificate Renewal Service", 5);
		}

		public void TestRunTimeIsInFuture()
		{
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup);
			job.SP_RunDateTime = job.SP_RunDateTime.AddMinutes(8);
			var initialBytes = job.SP_CustomProperties;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			job = newFactory.Load<StmPrintJob>(job.PK);
			AssertEquals(false, job.SP_IsSigned);
			AssertEquals(job.SP_CustomProperties, initialBytes);
			AssertEquals("DOS", job.SP_SignBy);
			AssertEquals(0, (int)job.SP_RetryAttempts);
			AssertEquals("", job.SP_FailureReason);
			AssertEquals("QUE", job.SP_Status);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Finished processing queue. Total jobs 0, successful jobs 0.", log);
			});

			job.SP_RunDateTime = job.SP_RunDateTime.AddMinutes(-11);
			job.Factory.Save();

			log = InitialiseAndRunTaskSchedule(task);

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			job = newFactory.Load<StmPrintJob>(job.PK);
			AssertEquals(true, job.SP_IsSigned);
			AssertNotEquals(job.SP_CustomProperties, initialBytes);
			AssertSignature(job);
			AssertEquals("DOS", job.SP_SignBy);
			AssertEquals(0, (int)job.SP_RetryAttempts);
			AssertEquals("", job.SP_FailureReason);
			AssertEquals("QUE", job.SP_Status);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Finished processing queue. Total jobs 0, successful jobs 0.", log, 1);
				AssertLogsContain("Information|Processing a batch of 1 files", log, 1);
				AssertLogsContain($"Information|Signing successful. File [{job.SP_DocumentName}], {job.PK.ToString()}", log, 1);
				AssertLogsContain("Information|Finished processing queue. Total jobs 1, successful jobs 1.", log, 1);
			});

			AssertNoEmails();
			AssertSignedOnce(task);
		}

		public void TestEndpointCredentialsWarnings()
		{
			AssertEndpointCredentialsWarnings(endpointValid: false, credentialsValid: false, countrySigningOption: false);
			AssertEndpointCredentialsWarnings(endpointValid: true, credentialsValid: false, countrySigningOption: false);
			AssertEndpointCredentialsWarnings(endpointValid: false, credentialsValid: true, countrySigningOption: true);
		}

		void AssertEndpointCredentialsWarnings(bool endpointValid, bool credentialsValid, bool countrySigningOption)
		{
			var endPoint = endpointValid ? "google.com" : "";
			DocumentsDataRegistry.Instance.CloudSigningServiceProviderAPIEndpoint.SetValue(Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, endPoint);

			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();

			StmPrintJob job;
			if (credentialsValid)
			{
				job = GetTestPrintJob(nameof(PrintJobType.EML), ZGuid.Empty, deliveryGroup);
			}
			else
			{
				job = GetTestPrintJob(nameof(PrintJobType.EML), ZGuid.Empty, deliveryGroup, branchOverride: Branch2.PK);
			}
			var initialBytes = job.SP_CustomProperties;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			var endpointWarning = endpointValid ? "" : " API endpoint is required.";
			var credentialsWarning = credentialsValid ? "" : " Credentials invalid.";
			var expectedWarning = "Warning|Incomplete configuration." + endpointWarning + credentialsWarning + " Reverting to NON.";

			AssertLogsContain(expectedWarning, log);

			AssertJobRevertedToNON(job.PK, initialBytes, new BusinessObjectFactory());
			AssertSignedOnce(task);
		}

		public void TestAccessTokenWarning()
		{
			var ptCompany = Factory.NewWithValidTestData<GlbCompany>();
			ptCompany.GC_RN_NKCountryCode = CountryCodes.Portugal;
			ptCompany.GC_IsGSTRegistered = false;

			var ptBranch = Factory.NewWithValidTestData<GlbBranch>();
			ptBranch.GB_GC = ptCompany.PK;
			ptBranch.GB_RN_NKCountryCode = CountryCodes.Portugal;

			Factory.Save();

			DocumentsDataRegistry.Instance.EnableDocumentSigningService.SetValue(ptCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			DocumentsDataRegistry.Instance.CloudSigningServiceProviderAPIEndpoint.SetValue(ptBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "google.com");
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(Guid.Empty, ptBranch.PK.ToGuid(), Guid.Empty, new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.DigitalSign, AccessKey = "1", ClientID = "1", KeyID = "1" });

			var accessTokenConfig = new DocumentSigningServiceCredentialsConfiguration() { AccessKey = "secretKey", ClientID = "clientID" };
			accessTokenConfig.SuspendValidationForKeyID_ForTestOnly = true;

			DocumentsDataRegistry.Instance.DocumentSigningServicePartnerCredentialsAccessToken.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accessTokenConfig);

			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();

			var job = GetTestPrintJob(nameof(PrintJobType.EML), ZGuid.Empty, deliveryGroup, branchOverride: ptBranch.PK);
			var initialBytes = job.SP_CustomProperties;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			AssertLogsContain("Warning|Incomplete configuration. Access Token invalid. Reverting to NON.", log);
			AssertJobRevertedToNON(job.PK, initialBytes, new BusinessObjectFactory());
			AssertSignedOnce(task);
		}

		public void TestRegistryDisabled()
		{
			DocumentsDataRegistry.Instance.EnableDocumentSigningService.SetValue(Branch3.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(Guid.Empty, Branch3.PK.ToGuid(), Guid.Empty, new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "1", ClientID = "1", KeyID = "1" });
			DocumentsDataRegistry.Instance.CloudSigningServiceProviderAPIEndpoint.SetValue(Branch3.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "google.com");

			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job1 = GetTestPrintJob(nameof(PrintJobType.EML), ZGuid.Empty, deliveryGroup);
			var job2 = GetTestPrintJob(nameof(PrintJobType.EML), ZGuid.Empty, deliveryGroup, branchOverride: Branch3.PK);
			var blob = job2.SP_CustomProperties;

			Factory.Save();

			var log = InitialiseTaskSchedule(task, out _, out var scheduleGovernor);
			scheduleGovernor.SetBranchPk(Env.CurrentBranchPK);
			RunTaskSchedule(task);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 1 files", log, 1);
				AssertLogsContain($"Information|Signing successful. File [{job1.SP_DocumentName}], {job1.PK.ToString()}", log, 1);
				AssertLogsContain($"Information|This service task is disabled in registry for the Company [{Branch3.Company.GC_Name}]. Documents --> Document Signing Service --> Enable Document Signing Service", log, 1);
				AssertLogsContain("Information|Finished processing queue. Total jobs 2, successful jobs 1.", log, 1);

				AssertNoEmails();
			});

			AssertJobRevertedToNON(job2.PK, blob, new BusinessObjectFactory());
		}

		StmPrintJob GetTestPrintJob(string jobType, ZGuid testParentGuid, StmDeliveryGroup deliveryGroup, bool sign = true, string attachmentType = "PDF", ZGuid? branchOverride = null)
		{
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = jobType;
			printJob.SP_EmailAttachmentFormat = attachmentType;
			printJob.SP_Destination = "example2@example.com";
			printJob.SP_DocumentName = "Test";
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "DocumentJobTaskTest";
			printJob.SP_GB = branchOverride ?? Branch.PK;
			printJob.SP_RunDateTime = ZDateTime.UtcNow.AddMinutes(-1);

			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				printJob.SP_CustomProperties = resourceRetriever.GetBytes("Small.xls");
			}

			if (!testParentGuid.IsEmpty)
			{
				printJob.SP_ParentTableName = "JobShipment";
				printJob.SP_ParentGuid = testParentGuid;
				printJob.SP_RelatedBusinessContext = "SHP"; // a valid business context
			}

			printJob.SP_RunDateTime = ZDateTime.UtcNow;
			printJob.SP_SB_DeliveryGroup = deliveryGroup.PK;

			if (sign)
			{
				printJob.SP_SignBy = "DOS";
			}

			return printJob;
		}

		public void TestSaveCopyToEdocs_PRN()
		{
			AssertWrongJobType(PrintJobType.PRN, createParent: true);
		}

		public void TestSaveCopyToEdocs_PRN_TIF()
		{
			AssertWrongJobType(PrintJobType.PRN, "TIF", createParent: true);
		}

		public void TestSaveCopyToEdocs_EML()
		{
			var jobType = PrintJobType.EML;
			AssertGoodJob(jobType, Guid.NewGuid());
		}

		public void TestSaveCopyToEdocs_DDS()
		{
			var jobType = PrintJobType.DDS;
			AssertGoodJob(jobType, Guid.NewGuid());
		}

		public void TestSaveCopyToEdocs_FTP()
		{
			AssertWrongJobType(PrintJobType.FTP, createParent: true);
		}

		public void TestSaveCopyToEdocs_FAA()
		{
			AssertWrongJobType(PrintJobType.FAA, createParent: true);
		}

		public void TestSign_EML()
		{
			var jobType = PrintJobType.EML;
			AssertGoodJob(jobType);
		}

		public void TestSign_EML_DeleteInDb()
		{
			var jobType = PrintJobType.EML;
			AssertGoodJob(jobType, ZGuid.Empty, deleteBeforeReporting: true);
		}

		public void TestSign_EML_TransactionId()
		{
			var jobType = PrintJobType.EML;
			AssertGoodJob(jobType);
		}

		public void TestSign_DDS()
		{
			var jobType = PrintJobType.DDS;
			AssertGoodJob(jobType);
		}

		public void TestSign_FTP()
		{
			AssertWrongJobType(PrintJobType.FTP);
		}

		public void TestSign_DDS_TIF()
		{
			var jobType = PrintJobType.DDS;
			AssertGoodJob(jobType, ZGuid.Empty, "TIF");
		}

		void AssertGoodJob(PrintJobType jobType)
		{
			AssertGoodJob(jobType, ZGuid.Empty);
		}

		public void TestSign_PRN()
		{
			AssertWrongJobType(PrintJobType.PRN);
		}

		public void TestSign_PRS()
		{
			AssertWrongJobType(PrintJobType.PRS);
		}

		public void TestSign_FAX()
		{
			AssertWrongJobType(PrintJobType.FAX);
		}

		public void TestSign_FAA()
		{
			AssertWrongJobType(PrintJobType.FAA);
		}

		public void TestSign_EML_XLS()
		{
			AssertWrongJobType(PrintJobType.EML, "XLS");
		}

		public void TestSign_DDS_XLS()
		{
			AssertGoodJob(PrintJobType.DDS, Guid.Empty, "XLS");
		}

		public void TestMultipleDocuments_WithIgnored()
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			var task = new DocumentSigningServiceTaskForTest();
			var goodJob = GetTestPrintJob(Enum.GetName(typeof(PrintJobType), PrintJobType.EML), Guid.Empty, deliveryGroup, attachmentType: "PDF");
			var printJob = GetTestPrintJob(Enum.GetName(typeof(PrintJobType), PrintJobType.PRN), Guid.Empty, deliveryGroup, attachmentType: "PDF");
			var faxJob = GetTestPrintJob(Enum.GetName(typeof(PrintJobType), PrintJobType.FAX), Guid.Empty, deliveryGroup, attachmentType: "PDF");
			var goodJobBadAttachment = GetTestPrintJob(Enum.GetName(typeof(PrintJobType), PrintJobType.EML), Guid.Empty, deliveryGroup, attachmentType: "TIF");

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			goodJob = AssertGoodJobCore(Guid.Empty, goodJob.PK, goodJob.SP_CustomProperties, newFactory, ZString.Empty);
			printJob = AssertJobRevertedToNON(printJob.PK, printJob.SP_CustomProperties, newFactory);
			faxJob = AssertJobRevertedToNON(faxJob.PK, faxJob.SP_CustomProperties, newFactory);
			goodJobBadAttachment = AssertJobRevertedToNON(goodJobBadAttachment.PK, goodJobBadAttachment.SP_CustomProperties, newFactory);

			CombineAssertions(() =>
			{
				AssertLogsContain("Warning|Incorrect job type: [PRN]", log);
				AssertLogsContain("Warning|Incorrect job type: [FAX]", log);
				AssertLogsContain($"Warning|Cannot sign attachment format: [TIF], file: [{goodJobBadAttachment.SP_DocumentName}]", log);

				AssertLogsContain("Information|Processing a batch of 1 files", log);
				AssertLogsContain($"Information|Signing successful. File [{goodJob.SP_DocumentName}], {goodJob.PK}", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 4, successful jobs 1.", log);

				AssertNoEmails();
			});
		}

		public void TestShouldNotProcessWrongAttachmentFormat()
		{
			SystemDataRegistry.Instance.EDocImportFileFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AttachmentTypeList.Codes.Pdf);
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			var task = new DocumentSigningServiceTaskForTest();
			var goodJobBadAttachment = GetTestPrintJob(Enum.GetName(typeof(PrintJobType), PrintJobType.EML), Guid.Empty, deliveryGroup, attachmentType: "XLS");

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			goodJobBadAttachment = AssertJobRevertedToNON(goodJobBadAttachment.PK, goodJobBadAttachment.SP_CustomProperties, newFactory);

			CombineAssertions(() =>
			{
				AssertLogsContain($"Warning|Cannot sign attachment format: [XLS], file: [{goodJobBadAttachment.SP_DocumentName}]", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 1, successful jobs 0.", log);

				AssertNoEmails();
			});
		}

		void AssertGoodJob(PrintJobType jobType, ZGuid testParentGuid, string attachmentType = "PDF", bool deleteBeforeReporting = false, bool deliveryGroupIsProcessed = true)
		{
			var jobTypeName = Enum.GetName(typeof(PrintJobType), jobType);
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			deliveryGroup.SB_IsProcessed = deliveryGroupIsProcessed;

			var task = new DocumentSigningServiceTaskForTest();
			task.DeleteBeforeReporting = deleteBeforeReporting;
			var transactionId = ZGuid.NewZGuid().ToString();

			var responses = new List<SignResult[]>
			{
				new SignResult[]
				{
					new SignResult("", transactionId),
				}
			};

			task.SignResponses = responses;

			var job = GetTestPrintJob(jobTypeName, testParentGuid, deliveryGroup, attachmentType: attachmentType);
			var initialBytes = job.SP_CustomProperties;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			job = AssertGoodJobCore(testParentGuid, job.PK, initialBytes, newFactory, transactionId);

			if (deleteBeforeReporting)
			{
				AssertNull(job);
			}
			else
			{
				CombineAssertions(() =>
				{
					AssertLogsContain("Information|Current Branch: " + job.Branch.GB_Code, log);
					AssertLogsContain("Debug|Signing Option: EMD", log);
					AssertLogsContain("Information|Processing a batch of 1 files", log);
					AssertLogsContain("Debug|Processing files: [" + job.PK + "]", log);
					AssertLogsContain("Debug|Read file bytes: [" + job.PK + "]", log);
					AssertLogsContain("Debug|Saved file bytes: [" + job.PK + "]", log);
					AssertLogsContain($"Information|Signing successful. File [{job.SP_DocumentName}], {job.PK}", log);
					AssertLogsContain("Information|Finished processing queue. Total jobs 1, successful jobs 1.", log);
					AssertNoEmails();
					AssertContainsExactElementsInAnyOrder(task.ProcessedJobs_Exposed, new ZGuid[] { job.PK });
					AssertSignedOnce(task);

					AssertContainsExactElementsInExactOrder(new[] { job.Branch.PK }, task.UsedBranches.Select(b => b.PK));
				});
			}
		}

		StmPrintJob AssertGoodJobCore(ZGuid testParentGuid, ZGuid jobPK, ZBlob initialBytes, BusinessObjectFactory newFactory, ZString transactionId)
		{
			var job = newFactory.Load<StmPrintJob>(jobPK);

			if (job == null)
			{
				return null;
			}

			AssertEquals(true, job.SP_IsSigned);
			AssertNotEquals(job.SP_CustomProperties, initialBytes);
			AssertSignature(job);
			AssertEquals("DOS", job.SP_SignBy);
			AssertEquals(0, (int)job.SP_RetryAttempts);
			AssertEquals("", job.SP_FailureReason);
			AssertEquals("QUE", job.SP_Status);

			var billingManager = new BillingManager();
			var stmUsages = billingManager.GetTransactions(Factory, 10);
			var parentStrAdditional = testParentGuid.IsEmpty ? string.Empty :
				@"
  ""ParentGuid"": """ + testParentGuid + @""",
  ""ParentTableName"": ""JobShipment"",";

			transactionId = transactionId.IsEmpty ? new ZString("testid") : transactionId;

			var additionalRefs = @"{
  ""FeatureCode"": ""DOS"",
  ""Module"": ""DocumentSigning"",
  ""FeatureDescription"": ""Document Signing"",
  ""CountryDocumentCode"": ""IN1"",
  ""JobType"": """ + job.SP_JobType + @""",
  ""DocumentName"": ""Test""," + parentStrAdditional + @"
  ""ProcessorName"": ""EMD"",
  ""RunDateTime"": """ + job.SP_RunDateTime.ToLongTimeString() + @""",
  ""SystemCreateUser"": ""E"",
  ""SignStatus"": ""Success"",
  ""TransactionId"": """ + transactionId + @"""
}";

			var parentStrExpected = testParentGuid.IsEmpty ? string.Empty :
				$@"
  <Reference2>{testParentGuid}</Reference2>
  <Reference3>JobShipment</Reference3>";

			var billingTransactionXML = BillingManager.GetTransactionXml(stmUsages.First());
			int index = billingTransactionXML.IndexOf("<ServiceOccuredUTC>") + "<ServiceOccuredUTC>".Length;
			string dateTime = billingTransactionXML.Substring(index, billingTransactionXML.IndexOf("</ServiceOccuredUTC>") - index);

			var reference5 = transactionId.IsEmpty
				? "<Reference5>testid</Reference5>"
				: $"<Reference5>{transactionId}</Reference5>";

			var expectedXML = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.edi.com.au/EnterpriseService/#Billing_1.4"">
  <BillableCount>1</BillableCount>
  <Branch>GB1</Branch>
  <Category>DOS</Category>
  <ClientID>{Branch.Company.LicenceKeyIdentifier}</ClientID>
  <ClientNumber>J.DAN</ClientNumber>
  <ClientStaffCode>E</ClientStaffCode>
  <PriceItemCode>IN1</PriceItemCode>
  <Reference1>Test</Reference1>" + parentStrExpected + $@"
  <Reference4>Success</Reference4>
  {reference5}
  <ReportingSource>ENT</ReportingSource>
  <ServiceOccuredUTC>{dateTime}</ServiceOccuredUTC>
  <Version>0</Version>
  <AdditionalRefs>{additionalRefs}</AdditionalRefs>
</BillingTransaction>";
			CombineAssertions(() =>
			{
				AssertEquals("Should have created a billing transaction", 1, stmUsages.Count());
				AssertXMLEquals(expectedXML, billingTransactionXML);
				AssertNoEmails();
			});

			return job;
		}

		void AssertWrongJobType(PrintJobType jobType, string attachmentType = "PDF", bool createParent = false)
		{
			var jobTypeName = Enum.GetName(typeof(PrintJobType), jobType);

			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job = GetTestPrintJob(jobTypeName, createParent ? ZGuid.Empty : ZGuid.NewZGuid(), deliveryGroup);
			job.SP_EmailAttachmentFormat = attachmentType;

			var initialBytes = job.SP_CustomProperties;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			job = AssertJobRevertedToNON(job.PK, initialBytes, newFactory);

			var billingManager = new BillingManager();
			var stmUsages = billingManager.GetTransactions(Factory, 10);

			CombineAssertions(() =>
			{	
				if (jobType != PrintJobType.DDS && jobType != PrintJobType.EML)
				{
					AssertLogsContain($"Warning|Incorrect job type: [{jobType}]", log);
				}
				else
				{
					AssertLogsContain($"Warning|Cannot sign attachment format: [{attachmentType}], file: [{job.SP_DocumentName}]", log);
				}

				AssertLogsContain("Information|Finished processing queue. Total jobs 1, successful jobs 0.", log);

				AssertEquals("Should have not created a billing transaction", 0, stmUsages.Count());

				AssertNoEmails();
				AssertSignedOnce(task);
			});
		}

		StmPrintJob AssertJobRevertedToNON(ZGuid jobPK, ZBlob initialBytes, BusinessObjectFactory newFactory)
		{
			var job = newFactory.Load<StmPrintJob>(jobPK);

			AssertEquals(false, job.SP_IsSigned);
			AssertEquals(initialBytes, job.SP_CustomProperties);
			AssertEquals(DocumentsSignBy.NON, job.SP_SignBy);
			return job;
		}

		void AssertSignature(StmPrintJob printJob)
		{
			printJob.SaveAttachmentToFilesystem(0);
			AssertEquals("File extention type of SavedFilename", ".PDF", Path.GetExtension(printJob.StoredAttachmentFilename).ToUpper());

			var signedConvertedPDF = File.ReadAllBytes(printJob.StoredAttachmentFilename);

			try
			{
				var reason = $@"{Core.Constants.CompanyBrandingName} \(WTG\) has provided the feature to generate and sign this PDF document using the data verified by the application users and available in {Core.Constants.ProductName} at the time of signing. WTG or any of its employees shall not be held responsible for any discrepancies observed in the data contained in this document.";

				var actualSignature = ImageToPDFConverterTest.GetSignatureFromPDF(signedConvertedPDF);

				AssertEquals(reason, actualSignature.First(e => e.key == "Reason").value);
			}
			finally
			{
				for (int retry = 0; retry < 3; retry++)
				{
					try
					{
						if (File.Exists(printJob.StoredAttachmentFilename))
						{
							File.Delete(printJob.StoredAttachmentFilename);
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

		public void TestNoEternalLoops()
		{
			DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			task.ThrowOnGetSignedHash = true;
			var job = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup);
			var initialBytes = job.SP_CustomProperties;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);
			for (int i = 0; i < 5; i++)
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				job = newFactory.Load<StmPrintJob>(job.PK);

				AssertEquals(false, job.SP_IsSigned);
				AssertEquals(job.SP_CustomProperties, initialBytes);
				AssertEquals(job.SP_SignBy, "DOS");
				AssertEquals(job.SP_RetryAttempts < 3 ? i + 1 : 3, job.SP_RetryAttempts);
				if (job.SP_RetryAttempts < 3)
				{
					AssertEquals("QUE", job.SP_Status);
				}

				task.RunTask();
			}

			AssertEquals("FAL", job.SP_Status);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 1 files", log, 3);
				AssertLogsContain("Warning|Error contacting remote web service. Error: kaboom\r\nStack Trace: ", log, 3, FindInLogsType.StartsWith);
				AssertLogsContain("at Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing.DocumentSigningServiceTaskTest.DocumentSigningServiceTaskForTest.BatchSign", log, 3, FindInLogsType.Contains);
				AssertLogsContain("Information|Finished processing queue. Total jobs 1, successful jobs 0.", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 0, successful jobs 0.", log, 5);
			});

			var error_report = @"No jobs have been processed out of 1. Distinct errors:
System.InvalidOperationException: kaboom";
			Assert("Error message is incorrect", ErrorReporter.LastMessageReported.StartsWith(error_report));

			var error_email = @"No jobs have been processed out of 1. Distinct errors:
kaboom"
			;
			AssertEmail(error_email);
			AssertSignedOnce(task);

			ErrorReporter.Clear();
		}

		public void TestNoEternalLoops_BadFile()
		{
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);
			deliveryGroup.SB_IsProcessed = false; // this ensures no file is saved

			var task = new DocumentSigningServiceTaskForTest();

			var job = GetTestPrintJob(Enum.GetName(typeof(PrintJobType), PrintJobType.EML), ZGuid.Empty, deliveryGroup, attachmentType: "PDF");
			var initialBytes = job.SP_CustomProperties;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			CombineAssertions(() =>
			{	
				AssertLogsContain("Warning|Unable to read an attachment from a job [Test]", log);
				AssertLogsContain($"Information|Finished processing queue. Total jobs 1, successful jobs 0.", log);
			});

			AssertSignedOnce(task);

			ErrorReporter.Clear();
		}

		public void TestDoesNotProcessSameJob_Single()
		{
			DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup);
			var initialBytes = job.SP_CustomProperties;

			task.AddProcessedJob_Exposed(job);

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			job = newFactory.Load<StmPrintJob>(job.PK);

			AssertEquals(false, job.SP_IsSigned);
			AssertEquals(job.SP_CustomProperties, initialBytes);
			AssertEquals(job.SP_SignBy, "DOS");
			AssertEquals(3, (int)job.SP_RetryAttempts);
			AssertEquals("FAL", job.SP_Status);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 1 files", log);
				AssertLogsContain("Warning|Job has already been signed once in this session and might be stuck [Test]", log);
				AssertLogsContain($"Information|Finished processing queue. Total jobs 1, successful jobs 0.", log);
			});

			var error_report = @"No jobs have been processed out of 1. Distinct errors:
Job has already been signed once in this session and might be stuck [Test]";
			Assert("Error message is incorrect", ErrorReporter.LastMessageReported.StartsWith(error_report));

			var error_email = @"No jobs have been processed out of 1. Distinct errors:
Job has already been signed once in this session and might be stuck [Test]"
			;
			AssertEmail(error_email);
			AssertSignedOnce(task);

			ErrorReporter.Clear();
			ClearTempFiles();
		}

		void AssertSignedOnce(DocumentSigningServiceTaskForTest task)
		{
			foreach (var item in task.SignedDocs)
			{
				AssertEquals("Each document should be singed only once", 1, item.Value);
			}
		}

		public void TestDoesNotProcessSameJob_Batch()
		{
			DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job1 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup);
			var job2 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup);
			var initialBytes1 = job1.SP_CustomProperties;
			var initialBytes2 = job2.SP_CustomProperties;

			task.AddProcessedJob_Exposed(job1);

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			job1 = newFactory.Load<StmPrintJob>(job1.PK);
			job2 = newFactory.Load<StmPrintJob>(job2.PK);

			AssertEquals(false, job1.SP_IsSigned);
			AssertEquals(job1.SP_CustomProperties, initialBytes1);
			AssertEquals(job1.SP_SignBy, "DOS");
			AssertEquals(3, (int)job1.SP_RetryAttempts);
			AssertEquals("FAL", job1.SP_Status);

			AssertEquals(true, job2.SP_IsSigned);
			AssertNotEquals(job2.SP_CustomProperties, initialBytes2);
			AssertEquals(job2.SP_SignBy, "DOS");
			AssertEquals(0, (int)job2.SP_RetryAttempts);
			AssertEquals("QUE", job2.SP_Status);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 2 files", log);
				AssertLogsContain("Warning|Job has already been signed once in this session and might be stuck [Test]", log);
				AssertLogsContain($"Information|Signing successful. File [{job2.SP_DocumentName}], {job2.PK}", log);
				AssertLogsContain($"Information|Finished processing queue. Total jobs 2, successful jobs 1.", log);
			});

			AssertNoEmails();
			AssertSignedOnce(task);

			ErrorReporter.Clear();
			ClearTempFiles();
		}

		void AssertEmail(string error, int count = 1)
		{
			AssertEquals(count, Env.OutgoingMailManager.EmailsCreated.Count);

			for (int i = 0; i < count; i++)
			{
				var email = Env.OutgoingMailManager.EmailsCreated[i];
				AssertEquals("Document Signing Processing Error", email.Subject);
				AssertEquals(error, email.Body);
				AssertEquals(NotificationEmail1, email.Recipients[0]);
			}
		}

		void AssertEmail(string subject, List<string> expectedBodies, string fromAddress, string fromDisplayName, int count = 1)
		{
			AssertEquals(count, Env.OutgoingMailManager.EmailsCreated.Count);

			for (int i = 0; i < count; i++)
			{
				var email = Env.OutgoingMailManager.EmailsCreated[i];
				AssertEquals(subject, email.Subject);
				AssertEquals(expectedBodies[i], email.Body);
				AssertEquals(NotificationEmail1, email.Recipients[0].Email);
				AssertEquals(fromAddress, email.FromAddress);
				AssertEquals(fromDisplayName, email.FromDisplayName);
			}
		}

		void AssertNoEmails()
		{
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestRetryStatusCleared()
		{
			DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup);
			job.SP_Status = "WRK";
			job.SP_FailureReason = "failure";
			job.SP_RetryAttempts = 2;

			var initialBytes = job.SP_CustomProperties;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			job = newFactory.Load<StmPrintJob>(job.PK);

			AssertEquals(true, job.SP_IsSigned);
			AssertNotEquals(job.SP_CustomProperties, initialBytes);
			AssertSignature(job);
			AssertEquals("DOS", job.SP_SignBy);
			AssertEquals(0, (int)job.SP_RetryAttempts);
			AssertEquals("", job.SP_FailureReason);
			AssertEquals("QUE", job.SP_Status);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 1 files", log);
				AssertLogsContain($"Information|Signing successful. File [{job.SP_DocumentName}], {job.PK}", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 1, successful jobs 1.", log);
				AssertNoEmails();
				AssertSignedOnce(task);
			});
		}

		public void TestBatching()
		{
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup1 = TestAssistant.CreateNewDeliveryGroup(Factory);
			var deliveryGroup2 = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job1 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1);
			var job2 = GetTestPrintJob(jobTypeName, ZGuid.NewZGuid(), deliveryGroup1);
			var job3 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup2);

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertJob(job1.PK, newFactory);
			AssertJob(job2.PK, newFactory);
			AssertJob(job3.PK, newFactory);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 3 files", log);
				AssertLogsContain($"Information|Signing successful. File [{job1.SP_DocumentName}], {job1.PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{job2.SP_DocumentName}], {job2.PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{job3.SP_DocumentName}], {job3.PK}", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 3, successful jobs 3.", log);
				AssertLogsContain($"Debug|Processing files: [{job1.PK}, {job2.PK}, {job3.PK}]", log);
				AssertLogsContain($"Debug|Read file bytes: [{job1.PK}]", log);
				AssertLogsContain($"Debug|Saved file bytes: [{job1.PK}]", log);
				AssertLogsContain($"Debug|Read file bytes: [{job2.PK}]", log);
				AssertLogsContain($"Debug|Saved file bytes: [{job2.PK}]", log);
				AssertLogsContain($"Debug|Read file bytes: [{job3.PK}]", log);
				AssertLogsContain($"Debug|Saved file bytes: [{job3.PK}]", log);
				AssertNoEmails();
				AssertSignedOnce(task);
			});
		}

		public void TestBatching_Grouping()
		{
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup1 = TestAssistant.CreateNewDeliveryGroup(Factory);
			var deliveryGroup2 = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job1 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1);
			var job2 = GetTestPrintJob(jobTypeName, ZGuid.NewZGuid(), deliveryGroup1);
			var job3 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup2);
			var job4 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1);
			var job5 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1);

			var otherBranch = job4.Branch.Company.Branches.AddNew();
			otherBranch.GB_Code = "GB2";
			otherBranch.GB_RN_NKCountryCode = CountryCodes.India;
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "1", ClientID = "1", KeyID = "1" });
			job4.SP_GB = otherBranch.PK;
			job5.SP_GB = otherBranch.PK;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertJob(job1.PK, newFactory);
			AssertJob(job2.PK, newFactory);
			AssertJob(job3.PK, newFactory);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Current Branch: " + job1.Branch.GB_Code, log);
				AssertLogsContain("Information|Processing a batch of 3 files", log);
				AssertLogsContain($"Information|Signing successful. File [{job1.SP_DocumentName}], {job1.PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{job2.SP_DocumentName}], {job2.PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{job3.SP_DocumentName}], {job3.PK}", log);
				AssertLogsContain("Information|Current Branch: " + job4.Branch.GB_Code, log);
				AssertLogsContain("Information|Processing a batch of 2 files", log);
				AssertLogsContain($"Information|Signing successful. File [{job4.SP_DocumentName}], {job4.PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{job5.SP_DocumentName}], {job5.PK}", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 5, successful jobs 5.", log);

				AssertLogsContain($"Debug|Processing files: [{job1.PK}, {job2.PK}, {job3.PK}]", log);
				AssertLogsContain($"Debug|Read file bytes: [{job1.PK}]", log);
				AssertLogsContain($"Debug|Saved file bytes: [{job1.PK}]", log);
				AssertLogsContain($"Debug|Read file bytes: [{job2.PK}]", log);
				AssertLogsContain($"Debug|Saved file bytes: [{job2.PK}]", log);
				AssertLogsContain($"Debug|Read file bytes: [{job3.PK}]", log);
				AssertLogsContain($"Debug|Saved file bytes: [{job3.PK}]", log);

				AssertLogsContain($"Debug|Processing files: [{job4.PK}, {job5.PK}]", log);
				AssertNoEmails();
				AssertSignedOnce(task);

				AssertContainsExactElementsInAnyOrder(new[] { job1.Branch.PK, job4.Branch.PK }, task.UsedBranches.Select(b => b.PK));
			});
		}

		public void TestBatching_Grouping_Before_Batching()
		{
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup1 = TestAssistant.CreateNewDeliveryGroup(Factory);
			var deliveryGroup2 = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var jobs = new List<StmPrintJob>();

			var otherBranch = Branch.Company.Branches.AddNew();
			otherBranch.GB_Code = "GB2";
			otherBranch.GB_RN_NKCountryCode = CountryCodes.India;
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "1", ClientID = "1", KeyID = "1" });

			for (int i = 0; i < 15; i++)
			{
				var job = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1);
				jobs.Add(job);
				if (i % 2 == 0)
				{
					job.SP_GB = otherBranch.PK;
				}
			}

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			for (int i = 0; i < jobs.Count; i++)
			{
				AssertJob(jobs[i].PK, newFactory);
			}

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 8 files", log);
				AssertLogsContain($"Information|Signing successful. File [{jobs[0].SP_DocumentName}], {jobs[0].PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{jobs[2].SP_DocumentName}], {jobs[2].PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{jobs[4].SP_DocumentName}], {jobs[4].PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{jobs[6].SP_DocumentName}], {jobs[6].PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{jobs[8].SP_DocumentName}], {jobs[8].PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{jobs[10].SP_DocumentName}], {jobs[10].PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{jobs[12].SP_DocumentName}], {jobs[12].PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{jobs[14].SP_DocumentName}], {jobs[14].PK}", log);

				AssertLogsContain("Information|Processing a batch of 7 files", log);
				AssertLogsContain($"Information|Signing successful. File [{jobs[1].SP_DocumentName}], {jobs[1].PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{jobs[3].SP_DocumentName}], {jobs[3].PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{jobs[5].SP_DocumentName}], {jobs[5].PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{jobs[7].SP_DocumentName}], {jobs[7].PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{jobs[9].SP_DocumentName}], {jobs[9].PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{jobs[11].SP_DocumentName}], {jobs[11].PK}", log);
				AssertLogsContain($"Information|Signing successful. File [{jobs[13].SP_DocumentName}], {jobs[13].PK}", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 15, successful jobs 15.", log);
				AssertNoEmails();
				AssertSignedOnce(task);
			});
		}

		[TestDate(2022, 2, 23, 1, 46, 0)]
		public void TestBatchResponses_OneBad_NoInterval()
		{
			DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup1 = TestAssistant.CreateNewDeliveryGroup(Factory);
			var deliveryGroup2 = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job1 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1);
			var job2 = GetTestPrintJob(jobTypeName, ZGuid.NewZGuid(), deliveryGroup1);
			var job3 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup2);

			var responses = new List<SignResult[]>();
			responses.Add(new SignResult[]
			{
				new SignResult("", ZGuid.NewZGuid().ToString()),
				new SignResult("avada kedavra", ZGuid.NewZGuid().ToString()),
				new SignResult("", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("avada kedavra", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("wingardium leviosa", ZGuid.NewZGuid().ToString()),
			});

			task.SignResponses = responses;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertJob(job1.PK, newFactory);
			AssertFailedJob(job2.PK, newFactory, 3, "wingardium leviosa");
			AssertJob(job3.PK, newFactory);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 3 files", log);
				AssertLogsContain($"Information|Signing successful. File [{job1.SP_DocumentName}], {job1.PK}", log);
				AssertLogsContain($"Warning|Signing failed. File [{job2.SP_DocumentName}], {job2.PK}, error: avada kedavra, retry 1 of 3", log);
				AssertLogsContain($"Information|Signing successful. File [{job3.SP_DocumentName}], {job3.PK}", log);
				AssertLogsContain("Information|Processing a batch of 1 files", log, 2);
				AssertLogsContain($"Warning|Signing failed. File [{job2.SP_DocumentName}], {job2.PK}, error: avada kedavra, retry 2 of 3", log);
				AssertLogsContain($"Warning|Signing failed. File [{job2.SP_DocumentName}], {job2.PK}, error: wingardium leviosa, retry 3 of 3", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 3, successful jobs 2.", log);
				AssertNoEmails();
			});

			log = InitialiseAndRunTaskSchedule(task);

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertFailedJob(job2.PK, newFactory, 3, "wingardium leviosa");
			AssertSignedOnce(task);

			ErrorReporter.Clear();
		}

		[TestDate(2022, 2, 23, 1, 46, 0)]
		public void TestBatchResponses_OneBad_NoInterval_Billing()
		{
			DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup1 = TestAssistant.CreateNewDeliveryGroup(Factory);
			var deliveryGroup2 = TestAssistant.CreateNewDeliveryGroup(Factory);

			Guid parent1 = Guid.NewGuid();
			Guid parent2 = Guid.NewGuid();
			Guid parent3 = Guid.NewGuid();

			var task = new DocumentSigningServiceTaskForTest();
			var job1 = GetTestPrintJob(jobTypeName, parent1, deliveryGroup1);
			var job2 = GetTestPrintJob(jobTypeName, parent2, deliveryGroup1);
			var job3 = GetTestPrintJob(jobTypeName, parent3, deliveryGroup2);

			var responses = new List<SignResult[]>();
			responses.Add(new SignResult[]
			{
				new SignResult("", ZGuid.NewZGuid().ToString()),
				new SignResult("avada kedavra", ZGuid.NewZGuid().ToString()),
				new SignResult("", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("avada kedavra", ZGuid.NewZGuid().ToString()),
			});

			responses.Add(new SignResult[]
			{
				new SignResult("wingardium leviosa", ZGuid.NewZGuid().ToString()),
			});

			task.SignResponses = responses;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertJob(job1.PK, newFactory);
			AssertFailedJob(job2.PK, newFactory, 3, "wingardium leviosa");
			AssertJob(job3.PK, newFactory);

			var billingManager = new BillingManager();
			var stmUsages = billingManager.GetTransactions(Factory, 10);

			var expectedXML1 =
$@"  <Reference2>{parent1}</Reference2>
  <Reference3>JobShipment</Reference3>
  <Reference4>Success</Reference4>";

			var expectedXML2 =
$@"  <Reference2>{parent2}</Reference2>
  <Reference3>JobShipment</Reference3>
  <Reference4>Fail</Reference4>";

			var expectedXML3 =
$@"  <Reference2>{parent3}</Reference2>
  <Reference3>JobShipment</Reference3>
  <Reference4>Success</Reference4>";

			var usages = stmUsages.ToArray();

			var billingTransactionXML = BillingManager.GetTransactionXml(usages[0]);
			AssertContains(expectedXML1, billingTransactionXML);

			billingTransactionXML = BillingManager.GetTransactionXml(usages[1]);
			AssertContains(expectedXML3, billingTransactionXML);

			billingTransactionXML = BillingManager.GetTransactionXml(usages[2]);
			AssertContains(expectedXML2, billingTransactionXML);

			AssertNoEmails();
			AssertSignedOnce(task);

			ErrorReporter.Clear();
		}

		public void TestBatchResponses_OneBad_IntervalTen()
		{
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup1 = TestAssistant.CreateNewDeliveryGroup(Factory);
			var deliveryGroup2 = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job1 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1);
			var job2 = GetTestPrintJob(jobTypeName, ZGuid.NewZGuid(), deliveryGroup1);
			var job3 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup2);

			var responses = new List<SignResult[]>();
			responses.Add(new SignResult[]
			{
				new SignResult("", ZGuid.NewZGuid().ToString()),
				new SignResult("avada kedavra", ZGuid.NewZGuid().ToString()),
				new SignResult("", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("avada kedavra", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("avada kedavra", ZGuid.NewZGuid().ToString())
			});

			task.SignResponses = responses;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertJob(job1.PK, newFactory);
			AssertJob(job3.PK, newFactory);
			job2 = NewFactory().Load<StmPrintJob>(job2.PK);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 3 files", log);
				AssertLogsContain($"Information|Signing successful. File [{job1.SP_DocumentName}], {job1.PK}", log);
				AssertLogsContain($"Warning|Signing failed. File [{job2.SP_DocumentName}], {job2.PK}, error: avada kedavra, retry 1 of 3", log);
				AssertLogsContain($"Information|Signing successful. File [{job3.SP_DocumentName}], {job3.PK}", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 3, successful jobs 2.", log);
				AssertGreaterThanOrEqualTo(job2.SP_RunDateTime, ZDateTime.UtcNow.AddMinutes(9));
			});

			log = InitialiseAndRunTaskSchedule(task);
			job2 = NewFactory().Load<StmPrintJob>(job2.PK);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 3 files", log);
				AssertLogsContain($"Information|Signing successful. File [{job1.SP_DocumentName}], {job1.PK}", log);
				AssertLogsContain($"Warning|Signing failed. File [{job2.SP_DocumentName}], {job2.PK}, error: avada kedavra, retry 1 of 3", log);
				AssertLogsContain($"Information|Signing successful. File [{job3.SP_DocumentName}], {job3.PK}", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 3, successful jobs 2.", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 0, successful jobs 0.", log);
				AssertGreaterThanOrEqualTo(job2.SP_RunDateTime, ZDateTime.UtcNow.AddMinutes(9));
			});

			job2.SP_RunDateTime = ZDateTime.UtcNow.AddMinutes(-11);
			job2.Factory.Save();
			log = InitialiseAndRunTaskSchedule(task);

			job2 = NewFactory().Load<StmPrintJob>(job2.PK);
			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 3 files", log);
				AssertLogsContain($"Information|Signing successful. File [{job1.SP_DocumentName}], {job1.PK}", log);
				AssertLogsContain($"Warning|Signing failed. File [{job2.SP_DocumentName}], {job2.PK}, error: avada kedavra, retry 1 of 3", log);
				AssertLogsContain($"Information|Signing successful. File [{job3.SP_DocumentName}], {job3.PK}", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 3, successful jobs 2.", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 0, successful jobs 0.", log);
				AssertLogsContain("Information|Processing a batch of 1 files", log);
				AssertLogsContain($"Warning|Signing failed. File [{job2.SP_DocumentName}], {job2.PK}, error: avada kedavra, retry 2 of 3", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 1, successful jobs 0.", log);
				AssertGreaterThanOrEqualTo(job2.SP_RunDateTime, ZDateTime.UtcNow.AddMinutes(9));
			});

			log = InitialiseAndRunTaskSchedule(task);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 3 files", log);
				AssertLogsContain($"Information|Signing successful. File [{job1.SP_DocumentName}], {job1.PK}", log);
				AssertLogsContain($"Warning|Signing failed. File [{job2.SP_DocumentName}], {job2.PK}, error: avada kedavra, retry 1 of 3", log);
				AssertLogsContain($"Information|Signing successful. File [{job3.SP_DocumentName}], {job3.PK}", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 3, successful jobs 2.", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 0, successful jobs 0.", log, 2);
				AssertLogsContain("Information|Processing a batch of 1 files", log);
				AssertLogsContain($"Warning|Signing failed. File [{job2.SP_DocumentName}], {job2.PK}, error: avada kedavra, retry 2 of 3", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 1, successful jobs 0.", log);
			});

			AssertEmail(@"No jobs have been processed out of 1. Distinct errors:
avada kedavra");

			job2.SP_RunDateTime = ZDateTime.UtcNow.AddMinutes(-11);
			job2.Factory.Save();

			log = InitialiseAndRunTaskSchedule(task);

			job2 = NewFactory().Load<StmPrintJob>(job2.PK);
			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 3 files", log);
				AssertLogsContain($"Information|Signing successful. File [{job1.SP_DocumentName}], {job1.PK}", log);
				AssertLogsContain($"Warning|Signing failed. File [{job2.SP_DocumentName}], {job2.PK}, error: avada kedavra, retry 1 of 3", log);
				AssertLogsContain($"Information|Signing successful. File [{job3.SP_DocumentName}], {job3.PK}", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 3, successful jobs 2.", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 0, successful jobs 0.", log, 2);
				AssertLogsContain("Information|Processing a batch of 1 files", log, 2);
				AssertLogsContain($"Warning|Signing failed. File [{job2.SP_DocumentName}], {job2.PK}, error: avada kedavra, retry 2 of 3", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 1, successful jobs 0.", log, 2);
				AssertLogsContain($"Warning|Signing failed. File [{job2.SP_DocumentName}], {job2.PK}, error: avada kedavra, retry 3 of 3", log);
			});

			newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertFailedJob(job2.PK, newFactory, 3, "avada kedavra");

			AssertEmail(@"No jobs have been processed out of 1. Distinct errors:
avada kedavra", 2);
			AssertSignedOnce(task);

			ErrorReporter.Clear();
		}

		[TestDate(2025, 1, 1, 1, 1, 1)]
		public void TestBatchResponses_OneBad_ThenGood()
		{
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				AssertBatchResponses_OneBad_ThenGood();
			}
		}

		[TestDate(2025, 1, 1, 1, 1, 1)]
		public void TestBatchResponses_OneBad_ThenGood_WithCompany()
		{
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4))
			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, 0))
			{
				AssertBatchResponses_OneBad_ThenGood();
			}
		}

		public void AssertBatchResponses_OneBad_ThenGood()
		{
			TestDateAttribute.Date = ZDateTime.UtcNow.ToDateTime();

			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup1 = TestAssistant.CreateNewDeliveryGroup(Factory);
			var deliveryGroup2 = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job1 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1);
			var job2 = GetTestPrintJob(jobTypeName, ZGuid.NewZGuid(), deliveryGroup1);
			var job3 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup2);

			var responses = new List<SignResult[]>();
			responses.Add(new SignResult[]
			{
				new SignResult("", ZGuid.NewZGuid().ToString()),
				new SignResult("avada kedavra", ZGuid.NewZGuid().ToString()),
				new SignResult("", ZGuid.NewZGuid().ToString())
			});

			responses.Add(new SignResult[]
			{
				new SignResult("", ZGuid.NewZGuid().ToString())
			});

			task.SignResponses = responses;

			Factory.Save();

			var log = InitialiseAndRunTaskSchedule(task);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertJob(job1.PK, newFactory);
			AssertJob(job2.PK, newFactory);
			AssertJob(job3.PK, newFactory);

			CombineAssertions(() =>
			{
				AssertLogsContain("Information|Processing a batch of 3 files", log);
				AssertLogsContain($"Information|Signing successful. File [{job1.SP_DocumentName}], {job1.PK}", log);
				AssertLogsContain($"Warning|Signing failed. File [{job2.SP_DocumentName}], {job2.PK}, error: avada kedavra, retry 1 of 3", log);
				AssertLogsContain($"Information|Signing successful. File [{job3.SP_DocumentName}], {job3.PK}", log);
				AssertLogsContain("Information|Processing a batch of 1 files", log);
				AssertLogsContain($"Information|Signing successful. File [{job2.SP_DocumentName}], {job2.PK}", log);
				AssertLogsContain("Information|Finished processing queue. Total jobs 3, successful jobs 3.", log);
				AssertSignedOnce(task);
			});
		}

		[TestDate(2025, 1, 1, 1, 1, 1)]
		public void TestBatchResponses_WithProviderBatchingDefault()
		{
			TestDateAttribute.Date = ZDateTime.UtcNow.ToDateTime();

			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var task = new DocumentSigningServiceTaskForTest();

				var jobs = CreateJobs();

				var responses = new List<SignResult[]>();
				responses.Add(new SignResult[]
				{
				new SignResult("", ZGuid.NewZGuid().ToString()),
				new SignResult("", ZGuid.NewZGuid().ToString()),
				new SignResult("", ZGuid.NewZGuid().ToString()),
				new SignResult("avada kedavra", ZGuid.NewZGuid().ToString()),
				new SignResult("", ZGuid.NewZGuid().ToString())
				});

				responses.Add(new SignResult[]
				{
				new SignResult("", ZGuid.NewZGuid().ToString())
				});

				task.SignResponses = responses;

				Factory.Save();

				var log = InitialiseAndRunTaskSchedule(task);

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				foreach (var job in jobs)
				{
					AssertJob(job.PK, newFactory);
				}

				CombineAssertions(() =>
				{
					AssertLogsContain("Information|Processing a batch of 5 files", log	);
					AssertLogsContain($"Information|Signing successful. File [{jobs[0].SP_DocumentName}], {jobs[0].PK}", log);
					AssertLogsContain($"Information|Signing successful. File [{jobs[1].SP_DocumentName}], {jobs[1].PK}", log);
					AssertLogsContain($"Information|Signing successful. File [{jobs[2].SP_DocumentName}], {jobs[2].PK}", log);
					AssertLogsContain($"Warning|Signing failed. File [{jobs[3].SP_DocumentName}], {jobs[3].PK}, error: avada kedavra, retry 1 of 3", log);
					AssertLogsContain($"Information|Signing successful. File [{jobs[4].SP_DocumentName}], {jobs[4].PK}", log);
					AssertLogsContain("Information|Processing a batch of 1 files", log);
					AssertLogsContain($"Information|Signing successful. File [{jobs[3].SP_DocumentName}], {jobs[3].PK}", log);
					AssertLogsContain("Information|Finished processing queue. Total jobs 5, successful jobs 5.", log);
					AssertSignedOnce(task);
				});
			}
		}

		[TestDate(2025, 1, 1, 1, 1, 1)]
		public void TestBatchResponses_WithProviderBatchingSetTo3()
		{
			TestDateAttribute.Date = ZDateTime.UtcNow.ToDateTime();

			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DocumentsDataRegistry.Instance.SigningServiceProviderBatching.SetTemporaryValue(Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, 3))
			{
				var task = new DocumentSigningServiceTaskForTest();

				var jobs = CreateJobs();

				var responses = new List<SignResult[]>();
				responses.Add(new SignResult[]
				{
				new SignResult("", ZGuid.NewZGuid().ToString()),
				new SignResult("avada kedavra", ZGuid.NewZGuid().ToString()),
				new SignResult("", ZGuid.NewZGuid().ToString())
				});

				responses.Add(new SignResult[]
				{
				new SignResult("", ZGuid.NewZGuid().ToString()),
				new SignResult("", ZGuid.NewZGuid().ToString())
				});

				responses.Add(new SignResult[]
				{
				new SignResult("", ZGuid.NewZGuid().ToString())
				});

				task.SignResponses = responses;

				Factory.Save();

				var log = InitialiseAndRunTaskSchedule(task);

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				foreach (var job in jobs)
				{
					AssertJob(job.PK, newFactory);
				}

				CombineAssertions(() =>
				{
					AssertLogsContain("Information|Processing a batch of 3 files", log);
					AssertLogsContain($"Information|Signing successful. File [{jobs[0].SP_DocumentName}], {jobs[0].PK}", log);
					AssertLogsContain($"Warning|Signing failed. File [{jobs[1].SP_DocumentName}], {jobs[1].PK}, error: avada kedavra, retry 1 of 3", log);
					AssertLogsContain($"Information|Signing successful. File [{jobs[2].SP_DocumentName}], {jobs[2].PK}", log);
					AssertLogsContain("Information|Processing a batch of 2 files", log);
					AssertLogsContain($"Information|Signing successful. File [{jobs[3].SP_DocumentName}], {jobs[3].PK}", log);
					AssertLogsContain($"Information|Signing successful. File [{jobs[4].SP_DocumentName}], {jobs[4].PK}", log);
					AssertLogsContain("Information|Processing a batch of 1 files", log);
					AssertLogsContain($"Information|Signing successful. File [{jobs[1].SP_DocumentName}], {jobs[1].PK}", log);
					AssertLogsContain("Information|Finished processing queue. Total jobs 5, successful jobs 5.", log);
					AssertSignedOnce(task);
				});
			}
		}

		public void TestBatchResponses_WithRetriesIntervalSetTo10()
		{
			TestDateAttribute.Date = ZDateTime.UtcNow.ToDateTime();

			using (DocumentsDataRegistry.Instance.SigningServiceRetriesInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				var task = new DocumentSigningServiceTaskForTest();

				var jobs = CreateJobs();

				var responses = new List<SignResult[]>();
				responses.Add(new SignResult[]
				{
				new SignResult("", ZGuid.NewZGuid().ToString()),
				new SignResult("generic error for test", ZGuid.NewZGuid().ToString()),
				new SignResult("generic error for test", ZGuid.NewZGuid().ToString()),
				new SignResult("", ZGuid.NewZGuid().ToString()),
				new SignResult("", ZGuid.NewZGuid().ToString())
				});

				task.SignResponses = responses;

				Factory.Save();

				var log = InitialiseAndRunTaskSchedule(task);
				log = InitialiseAndRunTaskSchedule(task);

				CombineAssertions(() =>
				{
					AssertLogsContain("Information|Processing a batch of 5 files", log);
					AssertLogsContain($"Information|Signing successful. File [{jobs[0].SP_DocumentName}], {jobs[0].PK}", log);
					AssertLogsContain($"Warning|Signing failed. File [{jobs[1].SP_DocumentName}], {jobs[1].PK}, error: generic error for test, retry 1 of 3", log);
					AssertLogsContain($"Warning|Signing failed. File [{jobs[2].SP_DocumentName}], {jobs[2].PK}, error: generic error for test, retry 1 of 3", log);
					AssertLogsContain($"Information|Signing successful. File [{jobs[3].SP_DocumentName}], {jobs[3].PK}", log);
					AssertLogsContain($"Information|Signing successful. File [{jobs[4].SP_DocumentName}], {jobs[4].PK}", log);
					AssertLogsContain("Information|Finished processing queue. Total jobs 5, successful jobs 3.", log);
					AssertLogsContain("Information|Finished processing queue. Total jobs 0, successful jobs 0.", log);
					AssertSignedOnce(task);
				});
			}
		}

		List<StmPrintJob> CreateJobs()
		{
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory);

			var jobs = new List<StmPrintJob>();
			for (int i = 0; i < 5; i++)
			{
				jobs.Add(GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup));
			}

			return jobs;
		}

		public void TestNoExceptionThrownAndJobRevertedToNONWhenSigningOptionIsEmpty()
		{
			var jobTypeName = Enum.GetName(typeof(PrintJobType), PrintJobType.EML);
			var deliveryGroup1 = TestAssistant.CreateNewDeliveryGroup(Factory);

			var task = new DocumentSigningServiceTaskForTest();
			var job1 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1);
			var job2 = GetTestPrintJob(jobTypeName, ZGuid.Empty, deliveryGroup1);

			var otherBranch = job1.Branch.Company.Branches.AddNew();
			otherBranch.GB_Code = "GB2";
			job1.SP_GB = otherBranch.PK;
			job2.SP_GB = otherBranch.PK;

			Factory.Save();
			TestServiceLogger log = null;

			AssertNoExceptionThrown(() => log = InitialiseAndRunTaskSchedule(task));
			AssertLogsContain("Warning|Incomplete configuration. Credentials invalid. Reverting to NON.", log);
			AssertLogsContain("Information|Finished processing queue. Total jobs 2, successful jobs 0.", log);

			var factory = new BusinessObjectFactory();
			AssertJobRevertedToNON(job1.PK, job1.SP_CustomProperties, factory);
			AssertJobRevertedToNON(job2.PK, job2.SP_CustomProperties, factory);
			AssertSignedOnce(task);
		}

		void AssertJob(ZGuid pk, BusinessObjectFactory newFactory)
		{
			var job = newFactory.Load<StmPrintJob>(pk);

			AssertEquals(true, job.SP_IsSigned);
			AssertSignature(job);
			AssertEquals("DOS", job.SP_SignBy);
			AssertEquals(0, (int)job.SP_RetryAttempts);
			AssertEquals("", job.SP_FailureReason);
			AssertEquals("QUE", job.SP_Status);
		}

		void AssertFailedJob(ZGuid pk, BusinessObjectFactory newFactory, int retry, string failureReason)
		{
			var job = newFactory.Load<StmPrintJob>(pk);

			AssertEquals(false, job.SP_IsSigned);
			AssertEquals("DOS", job.SP_SignBy);
			AssertEquals(retry, (int)job.SP_RetryAttempts);

			if (retry < 3)
			{
				AssertEquals("QUE", job.SP_Status);
				AssertEquals("", job.SP_FailureReason);
			}
			else
			{
				AssertEquals("FAL", job.SP_Status);
				AssertEquals(failureReason, job.SP_FailureReason);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "DOS", hostedServiceAttribute.Code);
				AssertEquals("Description", "Document Signing Service Task", hostedServiceAttribute.Description);
				AssertEquals("Category", "DOC", hostedServiceAttribute.Category);
				AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", false, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "5minutes", hostedServiceAttribute.MinimumPeriod);
			});
		}

		void ClearTempFiles()
		{
			foreach (var file in Directory.GetFiles(EnvProxy.Instance.TempPath))
			{
				File.Delete(file);
			}
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			DocumentSigningBillingManagerTest.InitializeBillingFieldMapping(Factory);
			SystemDataRegistry.Instance.EDocImportFileFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TIF");

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			Branch = company1.Branches.AddNew();
			Branch.GB_Code = "GB1";
			Branch.GB_RN_NKCountryCode = CountryCodes.India;

			Branch2 = company1.Branches.AddNew();
			Branch2.GB_Code = "GB3";

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Name = "company 2";
			Branch3 = company2.Branches.AddNew();
			Branch3.GB_Code = "GB4";
			Branch3.GB_RN_NKCountryCode = CountryCodes.India;

			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			Branch4 = company3.Branches.AddNew();
			Branch4.GB_Code = "GB5";
			Branch4.GB_RN_NKCountryCode = CountryCodes.India;

			DocumentsDataRegistry.Instance.EnableDocumentSigningService.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			DocumentsDataRegistry.Instance.EnableDocumentSigningService.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			DocumentsDataRegistry.Instance.EnableDocumentSigningService.SetValue(company3.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			DocumentsDataRegistry.Instance.CloudSigningServiceProviderAPIEndpoint.SetValue(Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "google.com");
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(Guid.Empty, Branch.PK.ToGuid(), Guid.Empty, new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "1", ClientID = "1", KeyID = "1" });

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var staff1 = group1.Staff.AddNew();
			staff1.FillWithValidTestData();
			staff1.GS_EmailAddress = NotificationEmail1;

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var staff2 = group2.Staff.AddNew();
			staff2.FillWithValidTestData();
			staff2.GS_EmailAddress = NotificationEmail2;

			DocumentsDataRegistry.Instance.DocumentSigningNotificationGroup.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, group1.PK.ToGuid());
			DocumentsDataRegistry.Instance.DocumentSigningNotificationGroup.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, group2.PK.ToGuid());

			UsageCollectorTest.ClearFeaturesForTest();
		}

		const string NotificationEmail1 = "notif1@gmail.com";
		const string NotificationEmail2 = "notif2@gmail.com";
		GlbBranch Branch;
		GlbBranch Branch2;
		GlbBranch Branch3;
		GlbBranch Branch4;

		class DocumentSigningServiceTaskForTest : DocumentSigningServiceTask
		{
			public bool ThrowOnGetSignedHash { get; set; }
			public int LastBatchSize;
			public List<SignResult[]> SignResponses;
			int batchCount;
			public List<IBranch> UsedBranches = new List<IBranch>();

			public bool DeleteBeforeReporting { get; set; }

			public void AddProcessedJob_Exposed(StmPrintJob job)
			{
				AddProcessedJob(job);
			}

			public ZGuid[] ProcessedJobs_Exposed
			{
				get
				{
					return allSignedJobs.ToArray();
				}
			}

			public readonly Dictionary<ZGuid, int> SignedDocs = new Dictionary<ZGuid, int>();

			protected override SignResult[] BatchSign(IPdfBatchSigner batchSigner, Dictionary<string, byte[]> documentsAsBytes)
			{
				UsedBranches.Add(Env.CurrentBranch);
				LastBatchSize = documentsAsBytes.Count;

				if (ThrowOnGetSignedHash)
				{
					throw new InvalidOperationException("kaboom");
				}

				var result = new List<SignResult>();
				if (SignResponses == null)
				{
					foreach (var document in documentsAsBytes)
					{
						result.Add(new SignResult(ZString.Empty, "testid"));

						AddSignedDoc(new ZGuid(document.Key));
					}
				}
				else
				{
					var responses = SignResponses[batchCount++];
					for (var i = 0; i < responses.Length; i++)
					{
						var response = responses[i];
						result.Add(new SignResult(response.ErrorMessage, response.TransactionId));

						if (string.IsNullOrEmpty(response.ErrorMessage))
						{
							AddSignedDoc(new ZGuid(documentsAsBytes.ToArray()[i].Key));
						}
					}
				}

				return BatchSignResultStored = result.ToArray();
			}

			void AddSignedDoc(ZGuid pk)
			{
				if (!SignedDocs.ContainsKey(pk))
				{
					SignedDocs.Add(pk, 0);
				}

				SignedDocs[pk]++;
			}

			protected override void ReportJobs(IEnumerable<StmPrintJob> signedJobs, IEnumerable<StmPrintJob> failedJobs, Dictionary<ZGuid, ZString> jobTransactionIds, string signingOption)
			{
				if (DeleteBeforeReporting)
				{
					var job = signedJobs.FirstOrDefault();
					if (job != null)
					{
						job.SP_SystemLastEditTimeUtc = ZDateTime.UtcNow;
					}

					job = failedJobs.FirstOrDefault();
					if (job != null)
					{
						job.SP_SystemLastEditTimeUtc = ZDateTime.UtcNow;
					}

					var pkStr = string.Join(",", signedJobs.Select(job => job.PK.ToString()).Concat(failedJobs.Select(job => job.PK.ToString())));
					using (var cmd = Db.Connection.Command($"delete from StmPrintJobCopyRecipient where SPR_SP in ('{pkStr}'); delete from StmPrintJob where SP_PK in ('{pkStr}')"))
					{
						cmd.ExecuteNonQuery();
					}
				}
				base.ReportJobs(signedJobs, failedJobs, jobTransactionIds, signingOption);
			}

			public SignResult[] BatchSignResultStored;
		}

		[TestDate(2022, 1, 1)]
		public void TestQueue()
		{
			// Arrange
			Db.Connection.ExecuteNonQuery("delete from StmPrintJob");
			var job1 = Factory.NewWithValidTestData<StmPrintJob>();
			job1.SP_SignBy = DocumentsSignBy.DOS;
			job1.SP_IsSigned = false;

			var job2 = Factory.NewWithValidTestData<StmPrintJob>();
			job2.SP_SignBy = DocumentsSignBy.NON;

			var job3 = Factory.NewWithValidTestData<StmPrintJob>();
			job3.SP_IsSigned = true;
			Factory.Save();
			IHostedServiceQueueProvider provider = new DocumentSigningServiceTaskQueue();

			// Act
			var result = provider.QueueResult;

			// Assert
			AssertEquals(1, result.QueueSize);

			var expectedSecondsAgo = (DateTime.UtcNow - TestDateAttribute.Date).TotalSeconds;
			AssertCloseEnough(((int)expectedSecondsAgo), ((int)result.MaximumItemAge.TotalSeconds), 60);
		}

		public void TestQueueWhenEmpty()
		{
			// Arrange
			Db.Connection.ExecuteNonQuery("delete from StmPrintJob");
			IHostedServiceQueueProvider provider = new DocumentSigningServiceTaskQueue();

			// Act
			var result = provider.QueueResult;

			// Assert
			AssertEquals(0, result.QueueSize);
			AssertEquals(TimeSpan.Zero, result.MaximumItemAge);
		}
	}
}
