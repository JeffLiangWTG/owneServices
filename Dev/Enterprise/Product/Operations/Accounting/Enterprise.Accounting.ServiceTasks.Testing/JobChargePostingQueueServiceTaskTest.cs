using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(JobChargePostingQueueServiceTask))]
	public class JobChargePostingQueueServiceTaskTest : ServiceTaskTestCase<JobChargePostingQueueServiceTask>
	{
		public void TestBeginAndEndQueueLogs()
		{
			var serviceTask = new JobChargePostingQueueServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			var expectedMessage = @"Information|Job Charge Posting Queue processing is starting.
Debug|No Job Charge Posting Queue records were found.
Information|Job Charge Posting Queue processing has finished.";
			AssertEquals(expectedMessage, log.Trim());
		}

		public void TestExceptionHandlingDuringPostRevenue()
		{
			ZGuid pluginPK = default;
			ZGuid[] chargePKs = default;
			ZBlob[] chargeValuesHashes = default;
			BusinessObjectFactory serviceTaskFactory = default;

			var queueItem = SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostRevenue);
			Factory.Save();
			var processorWithException = GetMockProcessorWithException(true);
			var dependecyFactory = new Mock<IAccountingDependencyFactory>();
			ObjectFactory.Substitute(dependecyFactory.Object);
			var serviceTask = new JobChargePostingQueueServiceTask();

			SetupDependecy(processorWithException.Object);
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
			AssertResults(0, 0);

			queueItem = SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostRevenue);
			Factory.Save();
			var processorWithExceptionAndEmail = GetMockProcessorWithException(true, shouldCreateEmail: true);
			SetupDependecy(processorWithExceptionAndEmail.Object);
			log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
			AssertResults(0, 1);

			queueItem = SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostRevenue);
			Factory.Save();
			var processorWithoutException = GetMockProcessorWithException(false);
			SetupDependecy(processorWithoutException.Object);
			log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
			AssertResults(1, 0);

			using (Env.Instance.TemporaryServiceTaskContext(JobChargePostingQueueServiceTask.Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			void SetupDependecy(IProcessor processor)
			{
				pluginPK = default;
				chargePKs = default;
				chargeValuesHashes = default;
				serviceTaskFactory = default;
				dependecyFactory.Reset();
				dependecyFactory.Setup(x => x.GetJobRevenueQueuePoster(It.IsAny<IJobInvoicingPlugIn>(), It.IsAny<IEnumerable<Charge>>(), It.IsAny<IEnumerable<IJobChargePostingQueue>>())).
					Returns(processor).
					Callback<IJobInvoicingPlugIn, IEnumerable<Charge>, IEnumerable<IJobChargePostingQueue>>((plugin, charges, queues) =>
					{
						pluginPK = plugin.PK;
						serviceTaskFactory = plugin.Factory;
						chargePKs = charges.Select(x => x.PK).ToArray();
						chargeValuesHashes = queues.Select(x => x.ChargeValuesHash).ToArray();
					});
			}

			void AssertResults(int expectedSaveCount, int expectedEmailCount)
			{
				dependecyFactory.Verify(x => x.GetJobRevenueQueuePoster(It.IsAny<IJobInvoicingPlugIn>(), It.IsAny<IEnumerable<Charge>>(), It.IsAny<IEnumerable<IJobChargePostingQueue>>()), Times.Once);
				AssertEquals("pluginPK", Shipment.PK, pluginPK);
				AssertEquals("chargePKs.Count", 1, chargePKs.Length);
				AssertEquals("chargePK", Charge1.PK, chargePKs.First());
				AssertEquals("chargeValuesHashes.Count", 1, chargeValuesHashes.Length);
				AssertEquals("ChargeValuesHash", queueItem.JPQ_ChargeValuesHash, chargeValuesHashes.First());
				AssertContains("First Error\r\nSecond Error", log);
				AssertEquals(expectedSaveCount, serviceTaskFactory.SaveCount);
				AssertEquals("EmailsCreated.Count", expectedEmailCount, Env.OutgoingMailManager.EmailsCreated.Count);
				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
		}

		public void TestExceptionHandlingDuringPostCost()
		{
			ZGuid pluginPK = default;
			ZGuid[] chargePKs = default;
			ZBlob[] chargeValuesHashes = default;
			BusinessObjectFactory serviceTaskFactory = default;

			var queueItem = SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostCost);
			Factory.Save();
			var processorWithException = GetMockProcessorWithException(true);
			var dependecyFactory = new Mock<IAccountingDependencyFactory>();
			ObjectFactory.Substitute(dependecyFactory.Object);
			var serviceTask = new JobChargePostingQueueServiceTask();

			SetupDependecy(processorWithException.Object);
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
			AssertResults(0, 0);

			queueItem = SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostCost);
			Factory.Save();
			var processorWithExceptionAndEmail = GetMockProcessorWithException(true, shouldCreateEmail: true);
			SetupDependecy(processorWithExceptionAndEmail.Object);
			log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
			AssertResults(0, 1);

			queueItem = SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostCost);
			Factory.Save();
			var processorWithoutException = GetMockProcessorWithException(false);
			SetupDependecy(processorWithoutException.Object);
			log = InitialiseAndRunTaskSchedule(serviceTask).ToString();
			AssertResults(1, 0);

			void SetupDependecy(IProcessor processor)
			{
				pluginPK = default;
				chargePKs = default;
				chargeValuesHashes = default;
				serviceTaskFactory = default;
				dependecyFactory.Reset();
				dependecyFactory.Setup(x => x.GetJobCostQueuePoster(It.IsAny<IJobInvoicingPlugIn>(), It.IsAny<IEnumerable<Charge>>(), It.IsAny<IEnumerable<IJobChargePostingQueue>>())).
					Returns(processor).
					Callback<IJobInvoicingPlugIn, IEnumerable<Charge>, IEnumerable<IJobChargePostingQueue>>((plugin, charges, queues) =>
					{
						pluginPK = plugin.PK;
						serviceTaskFactory = plugin.Factory;
						chargePKs = charges.Select(x => x.PK).ToArray();
						chargeValuesHashes = queues.Select(x => x.ChargeValuesHash).ToArray();
					});
			}

			void AssertResults(int expectedSaveCount, int expectedEmailCount)
			{
				dependecyFactory.Verify(x => x.GetJobCostQueuePoster(It.IsAny<IJobInvoicingPlugIn>(), It.IsAny<IEnumerable<Charge>>(), It.IsAny<IEnumerable<IJobChargePostingQueue>>()), Times.Once);
				AssertEquals("pluginPK", Shipment.PK, pluginPK);
				AssertEquals("chargePKs.Count", 1, chargePKs.Length);
				AssertEquals("chargePK", Charge1.PK, chargePKs.First());
				AssertEquals("chargeValuesHashes.Count", 1, chargeValuesHashes.Length);
				AssertEquals("ChargeValuesHash", queueItem.JPQ_ChargeValuesHash, chargeValuesHashes.First());
				AssertContains("First Error\r\nSecond Error", log);
				AssertEquals(expectedSaveCount, serviceTaskFactory.SaveCount);
				AssertEquals("EmailsCreated.Count", expectedEmailCount, Env.OutgoingMailManager.EmailsCreated.Count);
				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
		}

		Mock<IProcessor> GetMockProcessorWithException(bool shouldThrowException, bool shouldCreateEmail = false)
		{
			var processor = new Mock<IProcessor>();
			var result = processor.Setup(p => p.Process(It.IsAny<INotifications>(), It.IsAny<CancellationToken>())).Callback((INotifications notifications, CancellationToken c) =>
			{
				notifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Warning, "Some Warning"));
				notifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, "First Error"));
				notifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Information, "Some Information"));
				notifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, "Second Error"));
			});

			if (shouldThrowException)
			{
				AccountingEmailDef email = null;
				if (shouldCreateEmail)
				{
					email = new AccountingEmailDef_ForTest();
				}
				result.Throws(new LogSubscriberToAbortLogGroupProcessingSilentlyException("Cannot continue posting transactions", email));
			}
			return processor;
		}

		public void TestRunServiceTask_PostBothCostAndSellChargesSucceed()
		{
			Charge1.JR_APInvoiceNum = "AP001";
			Charge1.JR_APInvoiceDate = ZDateTime.Today;

			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostCost);
			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostRevenue);

			Factory.Save();

			var serviceTask = new JobChargePostingQueueServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			AssertContains("Cost charges from the job S0001 with GroupID 1 were posted successfully", log);
			AssertContains("Sell charges from the job S0001 with GroupID 1 were posted successfully", log);

			AssertChargePosted(Charge1, true, true);
			AssertChargePosted(Charge2, false, false);

			AssertInvoicePosted();

			AssertJobChargePostingQueueDeleted();
		}

		public void TestRunServiceTask_PostMultipleCostAndSellChargesSucceed()
		{
			Charge1.JR_APInvoiceNum = "AP001";
			Charge1.JR_APInvoiceDate = ZDateTime.Today;

			var org = TestObjectCreator.CreateOrgHeader("Org1", true, true);

			Charge2.JR_APInvoiceNum = "AP002";
			Charge2.JR_APInvoiceDate = ZDateTime.Today;

			Charge2.JR_OH_CostAccount = org.PK;
			Charge2.JR_OH_SellAccount = org.PK;

			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostCost);
			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostRevenue);
			SetupJobChargePostingQueue(Charge2, JobChargePostingQueueLookups.PostCost);
			SetupJobChargePostingQueue(Charge2, JobChargePostingQueueLookups.PostRevenue);

			Factory.Save();

			var serviceTask = new JobChargePostingQueueServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			AssertContains($"Cost charges from the job S0001 with GroupID 1 were posted successfully", log);
			AssertContains($"Sell charges from the job S0001 with GroupID 1 were posted successfully", log);

			AssertChargePosted(Charge1, true, true);
			AssertChargePosted(Charge2, true, true);

			AssertInvoicePosted(2, 2);

			AssertJobChargePostingQueueDeleted();
		}

		public void TestRunServiceTask_PostSellChargesFailWithComplianceSequenceRelatedException()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				var creator = new TestObjectCreator(Factory);
				var menuPK = creator.SetupComplianceMenuAndPivot("Govt Compliance Inv");
				var complianceSequence = creator.SetupComplianceSequence(menuPK, "TXI", "ABC", 1, 100, 1);
				complianceSequence.XD_PrintingAuthorizationNumber = "Test";
				Factory.Save();

				var invoice1 = creator.SetupComplianceInvoice("TXI", false);
				invoice1.AH_PostDate = ZDateTime.Now.AddDays(1);
				Factory.Save();

				Charge1.JR_OH_SellAccount = creator.TestOrganisation.PK;
				Charge1.JR_AT_SellGSTRate = creator.FREEVAT.PK;
				SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostRevenue);

				Factory.Save();

				var serviceTask = new JobChargePostingQueueServiceTask();
				var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

				AssertContains("Post date must be equal or higher than previous document.", log);

				AssertChargePosted(Charge1, false, false);

				AssertJobChargePostingQueueDeleted();
			}
		}

		public void TestRunServiceTaskWhenEnableMandatorySupplyType()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Charge1.JR_SellSupplyType = "DSB";
			Charge1.JR_CostSupplyType = "INT";
			Charge1.JR_APInvoiceNum = "AP001";
			Charge1.JR_APInvoiceDate = ZDateTime.Today;

			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostCost);
			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostRevenue);

			Factory.Save();

			var serviceTask = new JobChargePostingQueueServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			AssertContains($"Cost charges from the job S0001 with GroupID 1 were posted successfully", log);
			AssertContains($"Sell charges from the job S0001 with GroupID 1 were posted successfully", log);

			AssertChargePosted(Charge1, true, true);
			AssertChargePosted(Charge2, false, false);

			AssertInvoicePosted();

			AssertJobChargePostingQueueDeleted();
		}

		public void TestRunServiceTaskWhenEnableMandatorySupplyType_FailedWithEmptyValue()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Charge1.JR_SellSupplyType = "";
			Charge1.JR_CostSupplyType = "";
			Charge1.JR_APInvoiceNum = "AP001";
			Charge1.JR_APInvoiceDate = ZDateTime.Today;

			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostCost);
			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostRevenue);

			Factory.Save();

			var serviceTask = new JobChargePostingQueueServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			string expectedMessage = @"Error|Cost charges from the job S0001 with GroupID 1 can not be posted. Error message:
Cost Supply Type must be entered.

Error|Sell charges from the job S0001 with GroupID 1 can not be posted. Error message:
Sell Supply Type must be entered.";
			AssertContains(expectedMessage, log);

			AssertChargePosted(Charge1, false, false);
			AssertChargePosted(Charge2, false, false);

			AssertInvoicePosted(0, 0);

			AssertJobChargePostingQueueDeleted();
		}

		public void TestRunServiceTask_PostCostFailAndSellChargesSucceed()
		{
			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostCost);
			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostRevenue);

			Factory.Save();

			var serviceTask = new JobChargePostingQueueServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			AssertContains("Sell charges from the job S0001 with GroupID 1 were posted successfully", log);

			AssertChargePosted(Charge1, false, true);
			AssertChargePosted(Charge2, false, false);

			AssertInvoicePosted(0, 1);

			AssertJobChargePostingQueueDeleted();
		}

		public void TestRunServiceTask_PostCostSucceedAndSellChargesFail()
		{
			Charge1.JR_APInvoiceNum = "AP001";
			Charge1.JR_APInvoiceDate = ZDateTime.Today;

			Charge1.JR_OH_SellAccount = ZGuid.Empty;

			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostCost);
			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostRevenue);

			Factory.Save();

			var serviceTask = new JobChargePostingQueueServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			AssertContains("Cost charges from the job S0001 with GroupID 1 were posted successfully", log);

			AssertChargePosted(Charge1, true, false);
			AssertChargePosted(Charge2, false, false);

			AssertInvoicePosted(1, 0);

			AssertJobChargePostingQueueDeleted();
		}

		public void TestRunServiceTask_PostBothCostAndSellChargesFail()
		{
			Charge1.JR_OH_SellAccount = ZGuid.Empty;

			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostCost);
			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostRevenue);

			Factory.Save();

			var serviceTask = new JobChargePostingQueueServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			AssertChargePosted(Charge1, false, false);
			AssertChargePosted(Charge2, false, false);

			AssertInvoicePosted(0, 0);

			AssertJobChargePostingQueueDeleted();
		}

		public void TestRunServiceTask_PostBothCostAndSellChargesFail_WhenJobIsReadyForFinancialClosure()
		{
			Job.JH_Status = MasterFiles.Business.JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Assert("Precondition", Job.IsReadyForFinancialClosure);

			Charge1.JR_APInvoiceNum = "AP001";
			Charge1.JR_APInvoiceDate = ZDateTime.Today;

			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostCost);
			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostRevenue);

			Factory.Save();

			var serviceTask = new JobChargePostingQueueServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			AssertChargePosted(Charge1, false, false);
			AssertChargePosted(Charge2, false, false);

			AssertInvoicePosted(0, 0);

			AssertJobChargePostingQueueDeleted();
		}

		public void TestRunServiceTask_PostChangedChargesFail()
		{
			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostCost);
			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostRevenue);
			Factory.Save();

			Charge1.JR_OSCostAmt = 33M;
			Charge1.JR_OSSellAmt = 44M;
			Factory.Save();

			var serviceTask = new JobChargePostingQueueServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			AssertContains($@"Cost charges from the job S0001 with GroupID 1 can not be posted. Error message:
The charges were changed after creating the posting queue.", log);
			AssertContains($@"Sell charges from the job S0001 with GroupID 1 can not be posted. Error message:
The charges were changed after creating the posting queue.", log);

			AssertInvoicePosted(0, 0);

			AssertJobChargePostingQueueDeleted();
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestRunServiceTask_PostChargesFromDifferentCompany()
		{
			Charge anotherCharge;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), TestObjectCreator.NonCurrentDepartment.PK.ToGuid()))
			{
				var anotherJob = TestObjectCreator.CreateJob(Shipment, TestObjectCreator.LocalClient, 0m, TestObjectCreator.Agent, 0m);
				anotherCharge = TestObjectCreator.CreateCharge(anotherJob, TestObjectCreator.CC3, "Desc 3", TestObjectCreator.AUD, 30M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 300M, TestObjectCreator.ABIGAS);
				anotherCharge.JR_APInvoiceNum = "AP001";
				anotherCharge.JR_APInvoiceDate = ZDateTime.Today;

				SetupJobChargePostingQueue(anotherCharge, JobChargePostingQueueLookups.PostCost);
				SetupJobChargePostingQueue(anotherCharge, JobChargePostingQueueLookups.PostRevenue);

				TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-1));

				Factory.Save();
			}

			var serviceTask = new JobChargePostingQueueServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			AssertContains($"Cost charges from the job S0001 with GroupID 1 were posted successfully", log);
			AssertContains($"Sell charges from the job S0001 with GroupID 1 were posted successfully", log);

			AssertChargePosted(anotherCharge, true, true);

			AssertInvoicePosted();

			AssertJobChargePostingQueueDeleted();
		}

		public void TestRunServiceTask_PostCostWithEmptyInvoiceNumber()
		{
			var invoiceDate = ZDateTime.Today;

			Charge1.JR_APInvoiceNum = "TEST001";
			Charge1.JR_APInvoiceDate = invoiceDate;

			Charge2.JR_APInvoiceNum = null;
			Charge2.JR_APInvoiceDate = invoiceDate;

			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostCost);
			SetupJobChargePostingQueue(Charge2, JobChargePostingQueueLookups.PostCost);

			Factory.Save();

			var serviceTask = new JobChargePostingQueueServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			string expectedMessage = @"Information|Job Charge Posting Queue processing is starting.
Debug|Cost charges from the job S0001 with GroupID 1 were posted successfully
Information|Job Charge Posting Queue processing has finished.";
			AssertEquals(expectedMessage, log.Trim());  // Here should report 'charge2 cannot be posted'. However we don't have validation for it. (WI00228790)

			AssertChargePosted(Charge1, true, false);
			AssertChargePosted(Charge2, false, false);
			AssertInvoicePosted(1, 0);
			AssertJobChargePostingQueueDeleted();
		}

		public void TestRunServiceTask_PostCostWithEmptyCreditor()
		{
			var invoiceDate = ZDateTime.Today;

			Charge1.JR_APInvoiceNum = "TEST001";
			Charge1.JR_APInvoiceDate = invoiceDate;

			Charge2.JR_APInvoiceNum = "TEST001";
			Charge2.JR_OH_CostAccount = ZGuid.Empty;
			Charge2.JR_APInvoiceDate = invoiceDate;

			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostCost);
			SetupJobChargePostingQueue(Charge2, JobChargePostingQueueLookups.PostCost);

			Factory.Save();

			var serviceTask = new JobChargePostingQueueServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			string expectedMessage = @"Information|Job Charge Posting Queue processing is starting.
Debug|Cost charges from the job S0001 with GroupID 1 were posted successfully
Information|Job Charge Posting Queue processing has finished.";
			AssertEquals(expectedMessage, log.Trim());  // Here should report 'charge2 cannot be posted'. However we don't have validation for it. (WI00228790)

			AssertChargePosted(Charge1, true, false);
			AssertChargePosted(Charge2, false, false);
			AssertInvoicePosted(1, 0);
			AssertJobChargePostingQueueDeleted();
		}

		public void TestCanRunInAnyBranch()
		{
			var serviceTask = new JobChargePostingQueueServiceTask();

			Charge1.JR_APInvoiceNum = "TEST001";
			Charge1.JR_APInvoiceDate = ZDate.Today;

			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostCost);
			Factory.Save();

			using (EnvProxy.Instance.TemporaryServiceTaskContext(JobChargePostingQueueServiceTask.Code, canRunInAnyBranch: true))
			{
				serviceTask.ServiceLogger = new LoggerForTest();
				serviceTask.RunTask();
			}
			AssertEquals("No Errors", 0, ErrorReporter.LastMessageReported.Length);
		}

		public void TestRunServiceTask_PostRevenueWithEmptyDebtor()
		{
			var invoiceDate = ZDateTime.Today;

			Charge2.JR_OH_SellAccount = ZGuid.Empty;

			SetupJobChargePostingQueue(Charge1, JobChargePostingQueueLookups.PostRevenue);
			SetupJobChargePostingQueue(Charge2, JobChargePostingQueueLookups.PostRevenue);

			Factory.Save();

			var serviceTask = new JobChargePostingQueueServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask).ToString();

			string expectedMessage = @"Information|Job Charge Posting Queue processing is starting.
Debug|Sell charges from the job S0001 with GroupID 1 were posted successfully
Information|Job Charge Posting Queue processing has finished.";
			AssertEquals(expectedMessage, log.Trim());  // Here should report 'charge2 cannot be posted'. However we don't have validation for it. (WI00228790)

			AssertChargePosted(Charge1, false, true);
			AssertChargePosted(Charge2, false, false);
			AssertInvoicePosted(0, 1);
			AssertJobChargePostingQueueDeleted();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		static void AssertChargePosted(Charge charge, bool isCostPosted, bool isSellPosted)
		{
			var factory = new BusinessObjectFactory();
			var newCharge = factory.Load<Charge>(charge.PK);
			AssertEquals($"cost charge is {(isCostPosted ? "" : "not")} posted", isCostPosted, newCharge.JR_IsCostPosted);
			AssertEquals($"sell charge is {(isSellPosted ? "" : "not")} posted", isSellPosted, newCharge.JR_IsRevenuePosted);
		}

		static void AssertInvoicePosted(int apInvoiceCount = 1, int arInvoiceCount = 1)
		{
			var factory = new BusinessObjectFactory();
			var invoices = factory.Load<InvoicingBase>(new ZQuery());
			var apInvoice = invoices.Where(x => x.AH_Ledger == LedgerTypes.AccountsPayable).ToList();
			AssertEquals($"AP Invoice is {(apInvoiceCount > 0 ? "" : "not")} posted", apInvoiceCount, apInvoice.Count);

			var arInvoice = invoices.Where(x => x.AH_Ledger == LedgerTypes.AccountsReceivable).ToList();
			AssertEquals($"AR Invoice is {(arInvoiceCount > 0 ? "" : "not")} posted", arInvoiceCount, arInvoice.Count);
		}

		static void AssertJobChargePostingQueueDeleted(int groupID = 1)
		{
			var factory = new BusinessObjectFactory();
			var query = new ZQuery(JobChargePostingQueueSchema.JPQ_GroupID, groupID);
			AssertEquals($"job charge posting queue with GroupID {groupID} should be deleted", false, factory.ExistsInDatabase(JobChargePostingQueueSchema.Constants.TableName, query));
		}

		JobChargePostingQueue SetupJobChargePostingQueue(Charge charge, string postingInstruction)
		{
			return JobChargePostingQueue.CreateNew(Factory, postingInstruction, charge, Shipment.PK, "JS");
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			GlbStaff staffPluto = Factory.New<GlbStaff>();
			staffPluto.GS_LoginName = @"test";
			staffPluto.GS_Code = "TE";
			staffPluto.GS_EmailAddress = "test@cargowiseone";
			staffPluto.Groups.Add(Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK));
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.DebtorGlobalCreditLimitNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.AllPK);

			(Shipment, Job, Charge1, Charge2) = CreateTestObjects(Factory, testObjectCreator: TestObjectCreator);
		}

		static (ForwardingShipment shipment, Job job, Charge charge1, Charge charge2) CreateTestObjects(BusinessObjectFactory factory, TestObjectCreator testObjectCreator = null)
		{
			testObjectCreator = testObjectCreator ?? new TestObjectCreator(factory);

			var shipment = testObjectCreator.CreateShipment("S0001", true);
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0m, testObjectCreator.Agent, 0m);
			var charge1 = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 10M, testObjectCreator.AALSHI, testObjectCreator.AUD, 100M, testObjectCreator.ABIGAS);
			var charge2 = testObjectCreator.CreateCharge(job, testObjectCreator.CC2, "Desc 2", testObjectCreator.AUD, 20M, testObjectCreator.AALSHI, testObjectCreator.AUD, 200M, testObjectCreator.ABIGAS);

			testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-1));

			return (shipment, job, charge1, charge2);
		}

		ForwardingShipment Shipment;
		Job Job;
		Charge Charge1, Charge2;

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#region Concurrency Exception Tests

		[CargoWise.Data.Testing.UseSnapshotProtection]
		public class ConcurrencyTest : TestCase
		{
			public void TestRunServiceTask_HandlesInvalidCharge_ConstraintDisabled()
				=> AssertHandlesInvalidChargeByConstraintStatus("NOCHECK", "disabled");

			public void TestRunServiceTask_HandlesInvalidCharge_ConstraintRemoved()
				=> AssertHandlesInvalidChargeByConstraintStatus("DROP", "removed");

			void AssertHandlesInvalidChargeByConstraintStatus(string command, string expectedLogKeyWord)
			{
				const string constraintName = "JobChargePostingQueue_JPQ_JR_FK2_JobCharge_RRR_120N";
				var factory = new BusinessObjectFactory();

				var (shipment, _, charge, _) = JobChargePostingQueueServiceTaskTest.CreateTestObjects(factory);
				charge.JR_APInvoiceNum = "AP001";
				charge.JR_APInvoiceDate = ZDateTime.Today;

				var queue = JobChargePostingQueue.CreateNew(factory, JobChargePostingQueueLookups.PostCost, charge, shipment.PK, "JS");
				factory.Save();

				using (var newConnection = Db.NewExtraConnectionToMainDb())
				{
					newConnection.Command($"ALTER TABLE dbo.JobChargePostingQueue {command} CONSTRAINT {constraintName}").ExecuteNonQuery();
					newConnection.Command($"DELETE FROM dbo.JobCharge WHERE JR_PK = '{charge.PK}'").ExecuteNonQuery();
				}

				var newFactory = new BusinessObjectFactory();
				AssertNotNull("PreRequisite", newFactory.Load<JobChargePostingQueue>(queue.PK));
				AssertNull("PreRequisite", newFactory.Load<JobCharge>(charge.PK));

				RunServiceTaskAndGetLogs();
				AssertEquals("Should have error reported.", $"Constraint {constraintName} was {expectedLogKeyWord}.", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}

			public void TestRunServiceTask_HandlesConcurrencyErrorForCostAndRevenue_SuccessAfterOneRetry()
			{
				var factory = new BusinessObjectFactory();

				var (shipment, _, charge1, _) = JobChargePostingQueueServiceTaskTest.CreateTestObjects(factory);
				charge1.JR_APInvoiceNum = "AP001";
				charge1.JR_APInvoiceDate = ZDateTime.Today;

				JobChargePostingQueue.CreateNew(factory, JobChargePostingQueueLookups.PostCost, charge1, shipment.PK, "JS");
				JobChargePostingQueue.CreateNew(factory, JobChargePostingQueueLookups.PostRevenue, charge1, shipment.PK, "JS");

				factory.Save();

				ConfigureDependenciesForConcurrencyTest(1);

				var log = RunServiceTaskAndGetLogs();

				var expectedLog = @"
Information|Job Charge Posting Queue processing is starting.
Debug|Cost charges from the job S0001 with GroupID 1 can not be posted due to concurrency error. Attempt number 1 of 3.
Debug|Cost charges from the job S0001 with GroupID 1 were posted successfully
Debug|Sell charges from the job S0001 with GroupID 1 can not be posted due to concurrency error. Attempt number 1 of 3.
Debug|Sell charges from the job S0001 with GroupID 1 were posted successfully
Information|Job Charge Posting Queue processing has finished.
".Trim();
				AssertContains(expectedLog, log);

				JobChargePostingQueueServiceTaskTest.AssertChargePosted(charge1, true, true);
				JobChargePostingQueueServiceTaskTest.AssertInvoicePosted();
				JobChargePostingQueueServiceTaskTest.AssertJobChargePostingQueueDeleted();
			}

			public void TestRunServiceTask_HandlesConcurrencyErrorForCostAndRevenue_SuccessAfterTwoRetries()
			{
				var factory = new BusinessObjectFactory();

				var (shipment, _, charge1, _) = JobChargePostingQueueServiceTaskTest.CreateTestObjects(factory);
				charge1.JR_APInvoiceNum = "AP001";
				charge1.JR_APInvoiceDate = ZDateTime.Today;

				JobChargePostingQueue.CreateNew(factory, JobChargePostingQueueLookups.PostCost, charge1, shipment.PK, "JS");
				JobChargePostingQueue.CreateNew(factory, JobChargePostingQueueLookups.PostRevenue, charge1, shipment.PK, "JS");

				factory.Save();

				ConfigureDependenciesForConcurrencyTest(2);

				var log = RunServiceTaskAndGetLogs();

				var expectedLog = @"
Information|Job Charge Posting Queue processing is starting.
Debug|Cost charges from the job S0001 with GroupID 1 can not be posted due to concurrency error. Attempt number 1 of 3.
Debug|Cost charges from the job S0001 with GroupID 1 can not be posted due to concurrency error. Attempt number 2 of 3.
Debug|Cost charges from the job S0001 with GroupID 1 were posted successfully
Debug|Sell charges from the job S0001 with GroupID 1 can not be posted due to concurrency error. Attempt number 1 of 3.
Debug|Sell charges from the job S0001 with GroupID 1 can not be posted due to concurrency error. Attempt number 2 of 3.
Debug|Sell charges from the job S0001 with GroupID 1 were posted successfully
Information|Job Charge Posting Queue processing has finished.
".Trim();
				AssertContains(expectedLog, log);

				JobChargePostingQueueServiceTaskTest.AssertChargePosted(charge1, true, true);
				JobChargePostingQueueServiceTaskTest.AssertInvoicePosted();
				JobChargePostingQueueServiceTaskTest.AssertJobChargePostingQueueDeleted();
			}

			public void TestRunServiceTask_HandlesConcurrencyErrorForCostAndRevenue_GivesUpAfterThreeRetries()
			{
				var factory = new BusinessObjectFactory();

				var (shipment, _, charge1, _) = JobChargePostingQueueServiceTaskTest.CreateTestObjects(factory);
				charge1.JR_APInvoiceNum = "AP001";
				charge1.JR_APInvoiceDate = ZDateTime.Today;

				JobChargePostingQueue.CreateNew(factory, JobChargePostingQueueLookups.PostCost, charge1, shipment.PK, "JS");
				JobChargePostingQueue.CreateNew(factory, JobChargePostingQueueLookups.PostRevenue, charge1, shipment.PK, "JS");

				factory.Save();

				ConfigureDependenciesForConcurrencyTest(3);

				var log = RunServiceTaskAndGetLogs();

				var expectedLogForCost = @"
Debug|Cost charges from the job S0001 with GroupID 1 can not be posted due to concurrency error. Attempt number 1 of 3.
Debug|Cost charges from the job S0001 with GroupID 1 can not be posted due to concurrency error. Attempt number 2 of 3.
Error|Cost charges from the job S0001 with GroupID 1 can not be posted due to concurrency error. Attempt number 3 of 3.
".Trim();
				var expectedLogForSell = @"
Debug|Sell charges from the job S0001 with GroupID 1 can not be posted due to concurrency error. Attempt number 1 of 3.
Debug|Sell charges from the job S0001 with GroupID 1 can not be posted due to concurrency error. Attempt number 2 of 3.
Error|Sell charges from the job S0001 with GroupID 1 can not be posted due to concurrency error. Attempt number 3 of 3.
".Trim();
				AssertContains(expectedLogForCost, log);
				AssertContains(expectedLogForSell, log);
				AssertContains("**CONCURRENCY Error Saving Record **", log);

				AssertChargePosted(charge1, false, false);
				JobChargePostingQueueServiceTaskTest.AssertInvoicePosted(0, 0);
				AssertJobChargePostingQueueDeleted();
			}

			string RunServiceTaskAndGetLogs()
			{
				var serviceTask = new JobChargePostingQueueServiceTask();
				serviceTask.ServiceLogger = new TestServiceLogger();
				serviceTask.RunTask();
				return serviceTask.ServiceLogger.ToString();
			}

			void ConfigureDependenciesForConcurrencyTest(int concurrencyErrorCount)
			{
				var concurrencyCostPoster = new PosterForConcurrencyTest(concurrencyErrorCount,
					(plugin, jobCharges, queueEntries) => new JobCostQueuePoster(plugin, jobCharges, queueEntries),
					(counter) => $"{nameof(JobCharge.JR_APInvoiceDate)} = '{ZDateTime.Today.AddMinutes(30 + counter).SqlFormat}'"
				);
				var concurrencyRevenuePoster = new PosterForConcurrencyTest(concurrencyErrorCount,
					(plugin, jobCharges, queueEntries) => new JobRevenueQueuePoster(plugin, jobCharges, queueEntries),
					(counter) => $"{nameof(JobCharge.JR_MarginPercentage)} = {80 + counter}"
				);

				var dependencyFactoryMock = new Mock<IAccountingDependencyFactory>();
				dependencyFactoryMock
					.Setup(x => x.GetJobCostQueuePoster(It.IsAny<IJobInvoicingPlugIn>(), It.IsAny<IEnumerable<Charge>>(), It.IsAny<IEnumerable<IJobChargePostingQueue>>()))
					.Returns((IJobInvoicingPlugIn plugin, IEnumerable<Charge> jobCharges, IEnumerable<IJobChargePostingQueue> queueEntries) => concurrencyCostPoster.Capture(plugin, jobCharges, queueEntries));
				dependencyFactoryMock
					.Setup(x => x.GetJobRevenueQueuePoster(It.IsAny<IJobInvoicingPlugIn>(), It.IsAny<IEnumerable<Charge>>(), It.IsAny<IEnumerable<IJobChargePostingQueue>>()))
					.Returns((IJobInvoicingPlugIn plugin, IEnumerable<Charge> jobCharges, IEnumerable<IJobChargePostingQueue> queueEntries) => concurrencyRevenuePoster.Capture(plugin, jobCharges, queueEntries));
				dependencyFactoryMock.Setup(x => x.GetBranchLevelPostingHelper()).Returns(new BranchLevelPostingHelper());
				dependencyFactoryMock.Setup(x => x.GetSingleActionPerTransactionOnDifferentLevels()).Returns(new SingleActionPerTransactionOnDifferentLevels());
				ObjectFactory.Substitute(dependencyFactoryMock.Object);
			}

			delegate IProcessor CreateCostOrRevenuePoster(IJobInvoicingPlugIn plugin, IEnumerable<Charge> jobCharges, IEnumerable<IJobChargePostingQueue> queueEntries);

			class PosterForConcurrencyTest : IProcessor
			{
				public PosterForConcurrencyTest(int concurrencyErrorCount,
					CreateCostOrRevenuePoster createInner,
					Func<int, string> changeToMakeConcurrencyError)
				{
					CauseConcurrencyErrorsUntilCount = concurrencyErrorCount;
					CreateInner = createInner;
					ChangeToMakeConcurrencyError = changeToMakeConcurrencyError;
				}

				readonly int CauseConcurrencyErrorsUntilCount;
				readonly CreateCostOrRevenuePoster CreateInner;
				readonly Func<int, string> ChangeToMakeConcurrencyError;

				int Counter;
				IProcessor Inner;
				IEnumerable<IJobChargePostingQueue> QueueEntries;

				public PosterForConcurrencyTest Capture(IJobInvoicingPlugIn plugIn, IEnumerable<Charge> jobCharges, IEnumerable<IJobChargePostingQueue> queueEntries)
				{
					QueueEntries = queueEntries;
					Inner = CreateInner(plugIn, jobCharges, queueEntries);
					return this;
				}

				public void Process(INotifications notifications, CancellationToken token = default)
				{
					Inner.Process(notifications, token);

					if (Counter < CauseConcurrencyErrorsUntilCount)
					{
						using (var conn = CargoWise.Data.Db.NewExtraConnectionToMainDb())
						{
							var pks = string.Join(",", QueueEntries.Select(x => "'" + x.ChargePK.ToString() + "'"));
							var setField = ChangeToMakeConcurrencyError(Counter);
							conn.ExecuteNonQuery($"UPDATE {nameof(JobCharge)} SET {setField},JR_SystemLastEditUser = '~BP', JR_SystemLastEditTimeUtc = GETUTCDATE()  WHERE {JobChargeSchema.PK.Name} IN ({pks})");
						}
					}
					++Counter;
				}
			}
		}
		#endregion
	}
}
