using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class AccountingBillingServiceTest : TestCaseWithFactory
	{
		public void TestCreateJobHeader_NonexistentOperationsJob()
		{
			var jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Precondition", 0, jobCount);

			var result = service.CreateJobHeader(Guid.NewGuid(), JobShipmentSchema.Constants.Prefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), Guid.NewGuid());

			AssertNotNullOrEmpty(result.ErrorMessage);
			Assert(result.InputArgumentsHadError);

			jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("No job is created when an invalid business object PK is passed in", 0, jobCount);
		}

		public void TestCreateJobHeader_InvalidOperationsJob()
		{
			var jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Precondition", 0, jobCount);

			var operationsJob = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var result = service.CreateJobHeader(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), Guid.NewGuid());

			AssertNotNullOrEmpty(result.ErrorMessage);
			Assert(result.InputArgumentsHadError);

			jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("No job is created when an invalid business object PK is passed in", 0, jobCount);
		}

		public void TestCreateJobHeader_InvalidTablePrefix()
		{
			var jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Precondition", 0, jobCount);

			var localClient = Factory.NewWithValidTestData<OrgAddress>();
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var invalidTablePrefix = "IVPX";
			var result =
				service.CreateJobHeader(
					operationsJob.PK.ToGuid(),
					invalidTablePrefix,
					GlbStaff.CurrentUser.PK.ToGuid(),
					GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid(),
					localClient.PK.ToGuid());

			AssertEquals(
				"No revenue can be posted when invalid business object is introduced",
				invalidTablePrefixAcceptedMessage,
				result.ErrorMessage);

			Assert(result.InputArgumentsHadError);

			jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("No job is created when an invalid business object is introduced", 0, jobCount);
		}

		public void TestCreateJobHeader_ShouldSetLocalCharges()
		{
			var jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Precondition", 0, jobCount);

			var localClientAddress = Factory.NewWithValidTestData<OrgAddress>();
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var result = service.CreateJobHeader(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), localClientAddress.PK.ToGuid());

			AssertNullOrEmpty(result.ErrorMessage);
			jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Job is created", 1, jobCount);

			var jobHeader = Factory.Load<JobHeader>(new ZQuery())[0];
			AssertEquals(localClientAddress.PK, jobHeader.JH_OA_LocalChargesAddr);
		}

		public void TestCreateJobHeader_ShouldIgnoreLocalChargesForExistingHeader()
		{
			var jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Precondition", 0, jobCount);

			var originalLocalClientAddress = Factory.NewWithValidTestData<OrgAddress>();
			var otherLocalClientAddress = Factory.NewWithValidTestData<OrgAddress>();
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			service.CreateJobHeader(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), originalLocalClientAddress.PK.ToGuid());

			service.CreateJobHeader(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), otherLocalClientAddress.PK.ToGuid());

			jobCount = Factory.GetDatabaseCount(typeof(JobHeader));
			AssertEquals("Only one job is created", 1, jobCount);

			var jobHeader = Factory.Load<JobHeader>(new ZQuery())[0];
			AssertEquals(originalLocalClientAddress.PK, jobHeader.JH_OA_LocalChargesAddr);
		}

		public void TestPostRevenue_NonexistentOperationsJob()
		{
			var invalidJobID = Guid.NewGuid();
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();

			var postingResults =
				service.PostRevenue(
					invalidJobID,
					operationsJob.TablePrefix,
					GlbStaff.CurrentUser.PK.ToGuid(),
					GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid());

			AssertEquals(
				"No revenue can be posted when an invalid business object PK is passed in",
				string.Format(nonExistentOperationJobAcceptedMessageTemplate, operationsJob.TablePrefix, invalidJobID),
				postingResults.ErrorMessage);

			Assert(postingResults.InputArgumentsHadError);
		}

		public void TestPostRevenue_InvalidOperationsJob()
		{
			var operationsJob = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var postingResults =
				service.PostRevenue(
					operationsJob.PK.ToGuid(),
					operationsJob.TablePrefix,
					GlbStaff.CurrentUser.PK.ToGuid(),
					GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid());

			AssertEquals(
				"No revenue can be posted when not a valid business object for operation is introduced",
				invalidOperaitonJobAcceptedMessage,
				postingResults.ErrorMessage);

			Assert(postingResults.InputArgumentsHadError);
		}

		public void TestPostRevenue_InvalidTablePrefix()
		{
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var invalidTablePrefix = "IVPX";
			var postingResults =
				service.PostRevenue(
					operationsJob.PK.ToGuid(),
					invalidTablePrefix,
					GlbStaff.CurrentUser.PK.ToGuid(),
					GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid());

			AssertEquals(
				"No revenue can be posted when invalid business object is introduced",
				invalidTablePrefixAcceptedMessage,
				postingResults.ErrorMessage);

			Assert(postingResults.InputArgumentsHadError);
		}

		public void TestPostRevenue_WithoutNotificationErrors()
		{
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var creator = new TestPosterCreator();
			ObjectFactory.Substitute<IRevenuePosterCreator>(creator);

			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Warning, "Some Warning"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Information, "Some Information"));

			var postingResults = service.PostRevenue(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

			AssertNullOrEmpty(postingResults.ErrorMessage);
		}

		public void TestPostRevenue_WithNotificationErrors()
		{
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var creator = new TestPosterCreator();
			ObjectFactory.Substitute<IRevenuePosterCreator>(creator);

			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Warning, "Some Warning"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, "First Error"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Information, "Some Information"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, "Second Error"));

			var postingResults = service.PostRevenue(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

			AssertEquals("First Error; Second Error", postingResults.ErrorMessage);
			Assert(!postingResults.InputArgumentsHadError);
		}

		public void TestPostCost_NonexistentOperationsJob()
		{
			var invalidJobID = Guid.NewGuid();
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();

			var postingResults =
				service.PostCost(
					invalidJobID,
					operationsJob.TablePrefix,
					GlbStaff.CurrentUser.PK.ToGuid(),
					GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid());

			AssertEquals(
				"No cost can be posted when an invalid business object PK is passed in",
				string.Format(nonExistentOperationJobAcceptedMessageTemplate, operationsJob.TablePrefix, invalidJobID),
				postingResults.ErrorMessage);

			Assert(postingResults.InputArgumentsHadError);
		}

		public void TestPostCost_InvalidOperationsJob()
		{
			var operationsJob = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var postingResults =
				service.PostCost(
					operationsJob.PK.ToGuid(),
					operationsJob.TablePrefix,
					GlbStaff.CurrentUser.PK.ToGuid(),
					GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid());

			AssertEquals(
				"No cost can be posted when not a valid business object for operation is introduced",
				invalidOperaitonJobAcceptedMessage,
				postingResults.ErrorMessage);

			Assert(postingResults.InputArgumentsHadError);
		}

		public void TestPostCost_InvalidTablePrefix()
		{
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var invalidTablePrefix = "IVPX";
			var postingResults =
				service.PostCost(
					operationsJob.PK.ToGuid(),
					invalidTablePrefix,
					GlbStaff.CurrentUser.PK.ToGuid(),
					GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid());

			AssertEquals(
				"No cost can be posted when invalid business object is introduced",
				invalidTablePrefixAcceptedMessage,
				postingResults.ErrorMessage);

			Assert(postingResults.InputArgumentsHadError);
		}

		public void TestPostCost_WithoutNotificationErrors()
		{
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var creator = new TestPosterCreator();
			ObjectFactory.Substitute<ICostPosterCreator>(creator);

			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Warning, "Some Warning"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Information, "Some Information"));

			var postingResults = service.PostCost(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

			AssertNullOrEmpty(postingResults.ErrorMessage);
		}

		public void TestPostCost_WithNotificationErrors()
		{
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var creator = new TestPosterCreator();
			ObjectFactory.Substitute<ICostPosterCreator>(creator);

			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Warning, "Some Warning"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, "First Error"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Information, "Some Information"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, "Second Error"));

			var postingResults = service.PostCost(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

			AssertEquals("First Error; Second Error", postingResults.ErrorMessage);

			Assert(!postingResults.InputArgumentsHadError);
		}

		public void TestPostOverseasAgentCharges_NonexistentOperationsJob()
		{
			var invalidJobID = Guid.NewGuid();
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();

			var postingResults =
				service.PostOverseasAgentCharges(
					invalidJobID,
					operationsJob.TablePrefix,
					GlbStaff.CurrentUser.PK.ToGuid(),
					GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid());

			AssertEquals(
				"No overseas agent changes can be posted when an invalid business object PK is passed in",
				string.Format(nonExistentOperationJobAcceptedMessageTemplate, operationsJob.TablePrefix, invalidJobID),
				postingResults.ErrorMessage);

			Assert(postingResults.InputArgumentsHadError);
		}

		public void TestPostOverseasAgentCharges_InvalidOperationsJob()
		{
			var operationsJob = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var postingResults =
				service.PostOverseasAgentCharges(
					operationsJob.PK.ToGuid(),
					operationsJob.TablePrefix,
					GlbStaff.CurrentUser.PK.ToGuid(),
					GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid());

			AssertEquals(
				"No overseas agent changes can be posted when not a valid business object for operation is introduced",
				invalidOperaitonJobAcceptedMessage,
				postingResults.ErrorMessage);

			Assert(postingResults.InputArgumentsHadError);
		}

		public void TestPostOverseasAgentCharges_InvalidTablePrefix()
		{
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var invalidTablePrefix = "IVPX";
			var postingResults =
				service.PostOverseasAgentCharges(
					operationsJob.PK.ToGuid(),
					invalidTablePrefix,
					GlbStaff.CurrentUser.PK.ToGuid(),
					GlbBranch.CurrentBranch.PK.ToGuid(),
					GlbDepartment.CurrentDepartment.PK.ToGuid());

			AssertEquals(
				"No overseas agent changes can be posted when invalid business object is introduced",
				invalidTablePrefixAcceptedMessage,
				postingResults.ErrorMessage);

			Assert(postingResults.InputArgumentsHadError);
		}

		public void TestPostOverseasAgentCharges_WithoutNotificationErrors()
		{
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var creator = new TestPosterCreator();
			ObjectFactory.Substitute<IPostOverseasAgentChargesProcessorCreator>(creator);

			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Warning, "Some Warning"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Information, "Some Information"));

			var postingResults = service.PostOverseasAgentCharges(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

			AssertNullOrEmpty(postingResults.ErrorMessage);
		}

		public void TestPostOverseasAgentCharges_WithNotificationErrors()
		{
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var creator = new TestPosterCreator();
			ObjectFactory.Substitute<IPostOverseasAgentChargesProcessorCreator>(creator);

			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Warning, "Some Warning"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, "First Error"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Information, "Some Information"));
			creator.Processor.ProcessNotifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, "Second Error"));

			var postingResults = service.PostOverseasAgentCharges(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

			AssertEquals("First Error; Second Error", postingResults.ErrorMessage);

			Assert(!postingResults.InputArgumentsHadError);
		}

		public void TestExceptionHandlingDuringPostTransactions_Revenue()
		{
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var revenuePosterCreator = new Mock<IRevenuePosterCreator>();
			ObjectFactory.Substitute(revenuePosterCreator.Object);
			var processorWithException = GetMockProcessorWithException(true);
			AssertResults(processorWithException.Object, 0, 0);

			var processorWithExceptionAndEmail = GetMockProcessorWithException(true, shouldCreateEmail: true);
			AssertResults(processorWithExceptionAndEmail.Object, 0, 1);

			var processorWithoutException = GetMockProcessorWithException(false);
			AssertResults(processorWithoutException.Object, 1, 0);

			void AssertResults(IProcessor processor, int expectedSaveCount, int expectedEmailCount)
			{
				revenuePosterCreator.Reset();
				revenuePosterCreator.Setup(c => c.CreateRevenuePoster(It.IsAny<IWorkflowProvider>())).Returns(processor).Callback((IWorkflowProvider provider) =>
				{
					var jobParent = provider as IJobHeaderParent;
					jobParent.Factory.ChildFactories.Add(Factory);
				});
				var prevSaveCount = Factory.SaveCount;
				BillingActionResult postingResults = null;
				AssertNoExceptionThrown(() => postingResults = service.PostRevenue(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				AssertEquals("First Error; Second Error", postingResults.ErrorMessage);
				Assert(!postingResults.InputArgumentsHadError);
				AssertEquals(prevSaveCount + expectedSaveCount, Factory.SaveCount);
				AssertEquals("EmailsCreated.Count", expectedEmailCount, Env.OutgoingMailManager.EmailsCreated.Count);
				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
		}

		public void TestExceptionHandlingDuringPostTransactions_Cost()
		{
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var costPosterCreator = new Mock<ICostPosterCreator>();
			ObjectFactory.Substitute(costPosterCreator.Object);
			var processorWithException = GetMockProcessorWithException(true);
			AssertResults(processorWithException.Object, 0, 0);

			var processorWithExceptionAndEmail = GetMockProcessorWithException(true, shouldCreateEmail: true);
			AssertResults(processorWithExceptionAndEmail.Object, 0, 1);

			var processorWithoutException = GetMockProcessorWithException(false);
			AssertResults(processorWithoutException.Object, 1, 0);

			void AssertResults(IProcessor processor, int expectedSaveCount, int expectedEmailCount)
			{
				costPosterCreator.Reset();
				costPosterCreator.Setup(c => c.CreateCostPoster(It.IsAny<IWorkflowProvider>())).Returns(processor).Callback((IWorkflowProvider provider) =>
				{
					var jobParent = provider as IJobHeaderParent;
					jobParent.Factory.ChildFactories.Add(Factory);
				});
				var prevSaveCount = Factory.SaveCount;
				BillingActionResult postingResults = null;
				AssertNoExceptionThrown(() => postingResults = service.PostCost(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				AssertEquals("First Error; Second Error", postingResults.ErrorMessage);
				Assert(!postingResults.InputArgumentsHadError);
				AssertEquals(prevSaveCount + expectedSaveCount, Factory.SaveCount);
				AssertEquals("EmailsCreated.Count", expectedEmailCount, Env.OutgoingMailManager.EmailsCreated.Count);
				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
		}

		public void TestExceptionHandlingDuringPostTransactions_OverseasAgentCharges()
		{
			var operationsJob = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var overseasAgentChargesPosterCreator = new Mock<IPostOverseasAgentChargesProcessorCreator>();
			ObjectFactory.Substitute(overseasAgentChargesPosterCreator.Object);
			var processorWithException = GetMockProcessorWithException(true);
			AssertResults(processorWithException.Object, 0, 0);

			var processorWithExceptionAndEmail = GetMockProcessorWithException(true, shouldCreateEmail: true);
			AssertResults(processorWithExceptionAndEmail.Object, 0, 1);

			var processorWithoutException = GetMockProcessorWithException(false);
			AssertResults(processorWithoutException.Object, 1, 0);

			void AssertResults(IProcessor processor, int expectedSaveCount, int expectedEmailCount)
			{
				overseasAgentChargesPosterCreator.Reset();
				overseasAgentChargesPosterCreator.Setup(c => c.CreateOverseasAgentChargesPoster(It.IsAny<IWorkflowProvider>())).Returns(processor).Callback((IWorkflowProvider provider) =>
				{
					var jobParent = provider as IJobHeaderParent;
					jobParent.Factory.ChildFactories.Add(Factory);
				});
				var prevSaveCount = Factory.SaveCount;
				BillingActionResult postingResults = null;
				AssertNoExceptionThrown(() => postingResults = service.PostOverseasAgentCharges(operationsJob.PK.ToGuid(), operationsJob.TablePrefix, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				AssertEquals("First Error; Second Error", postingResults.ErrorMessage);
				Assert(!postingResults.InputArgumentsHadError);
				AssertEquals(prevSaveCount + expectedSaveCount, Factory.SaveCount);
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

		public void TestSplitApportionAmount_ConsolCostNotFound()
		{
			var result = service.SplitApportionAmount(Guid.NewGuid(), GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			AssertEquals(result.ErrorMessage, "Consol Cost could not be found.");

			Assert(result.InputArgumentsHadError);
		}

		public void TestSplitApportionAmount()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			Factory.Save();
			var apps = new ApportionmentListing(Factory, consol);
			try
			{
				var consolCost = apps.CostsCollection.TryAddNew();
				var testObjectCreator = new TestObjectCreator(Factory);
				consolCost.E6_AC_ChargeCode = testObjectCreator.DSBChargeCode.PK;
				consolCost.E6_OSCostAmount = 5163.96;
				consolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
				Factory.Save();
				AssertEquals(consolCost.ApportionmentCharges.Count, 2);
				AssertEquals(2581.98m, consolCost.ApportionmentCharges[0].JR_OSCostAmt);
				AssertEquals(2581.98m, consolCost.ApportionmentCharges[1].JR_OSCostAmt);
				TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JobConsolCost
SET
	E6_OSCostAmount = 300,
	E6_ApportionmentMethod = '{0}',
	E6_SystemLastEditTimeUtc = GETUTCDATE(),
	E6_SystemLastEditUser = '~BP'
WHERE
	E6_PK = '{1}'", AllocationMethod.Shipment, consolCost.PK));
				service.SplitApportionAmount(consolCost.PK.ToGuid(), GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());

				var reloadedConsolCost = Factory.Load<JobConsolCost>(consolCost.PK);
				AssertEquals(reloadedConsolCost.ApportionmentCharges.Count, 2);
				AssertEquals(150m, reloadedConsolCost.ApportionmentCharges[0].JR_OSCostAmt);
				AssertEquals(150m, reloadedConsolCost.ApportionmentCharges[1].JR_OSCostAmt);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		const string nonExistentOperationJobAcceptedMessageTemplate = "Operations job with Table-prefix '{0}' and PK '{1}' could not be found.";
		const string invalidOperaitonJobAcceptedMessage = "Operation job is not valid.";
		const string invalidTablePrefixAcceptedMessage = "Table prefix is invalid.";

		protected override void SetUp()
		{
			base.SetUp();

			GlbStaff staffPluto = Factory.New<GlbStaff>();
			staffPluto.GS_LoginName = @"test";
			staffPluto.GS_Code = "TE";
			staffPluto.GS_EmailAddress = "test@cargowiseone";
			staffPluto.Groups.Add(Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK));
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.DebtorGlobalCreditLimitNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Groups.AllPK);

			service = new AccountingBillingService();
		}

		AccountingBillingService service;

		class TestPosterCreator :
			IRevenuePosterCreator,
			ICostPosterCreator,
			IPostOverseasAgentChargesProcessorCreator
		{
			public TestProcessor Processor => processor ?? (processor = new TestProcessor());
			TestProcessor processor;

			public IWorkflowProvider LastUsedProvider { get; private set; }

			public IProcessor CreateRevenuePoster(IWorkflowProvider provider)
			{
				return GetPoster(provider);
			}

			public IProcessor CreateCostPoster(IWorkflowProvider provider)
			{
				return GetPoster(provider);
			}

			public IProcessor CreateOverseasAgentChargesPoster(IWorkflowProvider provider)
			{
				return GetPoster(provider);
			}

			IProcessor GetPoster(IWorkflowProvider provider)
			{
				LastUsedProvider = provider;
				return Processor;
			}
		}

		class TestProcessor : IProcessor
		{
			public List<INotification> ProcessNotifications => processNotifications ?? (processNotifications = new List<INotification>());
			List<INotification> processNotifications;

			public void Process(INotifications notifications, CancellationToken token = new CancellationToken())
			{
				notifications.AddRange(ProcessNotifications);
			}
		}
	}
}
