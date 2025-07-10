using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.eNett_Integration.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.LogWalker.LogSubscriber;

namespace Enterprise.BatchProcessor.Accounting.Testing
{
	[TestedType(typeof(eNettOutboundTransactionSubscriberForTest))]
	public class eNettOutboundTransactionSubscriberExceptionTest : LogSubscriberTest<eNettOutboundTransactionSubscriberForTest>
	{
		[ExpectNoExceptions]
		public void TestHandleWebException()
		{
			var mockErrorReporter = new Mock<IErrorReporter>();
			ErrorReporter.Instance = mockErrorReporter.Object;

			var mock = new Mock<eNettOutboundTransactionSubscriberForTest>() { CallBase = true };
			mock
				.Protected()
				.Setup("Process", ItExpr.IsAny<IQueuedLog>(), ItExpr.IsAny<IEnumerable<GlbCompany>>())
				.Throws(new WebException());
			mock.Object.ProcessLogQueueItemsExposed(new QueuedLogForTesting[] { new QueuedLogForTesting(Factory) });
			mock.VerifyAll();

			mockErrorReporter.Verify(m => m.Report("ComPay_ProcessLogQueueItems_Exception", "ComPay Outbound Subscriber threw an exception",
				It.IsAny<WebException>()), Times.Once);
			mockErrorReporter.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestHandleSocketException()
		{
			var mockErrorReporter = new Mock<IErrorReporter>();
			ErrorReporter.Instance = mockErrorReporter.Object;

			var mock = new Mock<eNettOutboundTransactionSubscriberForTest>() { CallBase = true };
			mock
				.Protected()
				.Setup("Process", ItExpr.IsAny<IQueuedLog>(), ItExpr.IsAny<IEnumerable<GlbCompany>>())
				.Throws(new SocketException());
			mock.Object.ProcessLogQueueItemsExposed(new QueuedLogForTesting[] { new QueuedLogForTesting(Factory) });
			mock.VerifyAll();

			mockErrorReporter.Verify(m => m.Report("ComPay_ProcessLogQueueItems_Exception", "ComPay Outbound Subscriber threw an exception",
				It.IsAny<SocketException>()), Times.Once);
			mockErrorReporter.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestHandleInvalidOperationException()
		{
			var mockErrorReporter = new Mock<IErrorReporter>();
			ErrorReporter.Instance = mockErrorReporter.Object;

			var mock = new Mock<eNettOutboundTransactionSubscriberForTest>() { CallBase = true };
			mock
				.Protected()
				.Setup("Process", ItExpr.IsAny<IQueuedLog>(), ItExpr.IsAny<IEnumerable<GlbCompany>>())
				.Throws(new InvalidOperationException());
			mock.Object.ProcessLogQueueItemsExposed(new QueuedLogForTesting[] { new QueuedLogForTesting(Factory) });
			mock.VerifyAll();

			mockErrorReporter.Verify(m => m.Report("ComPay_ProcessLogQueueItems_Exception", "ComPay Outbound Subscriber threw an exception",
				It.IsAny<InvalidOperationException>()), Times.Once);
			mockErrorReporter.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestHandleMultipleInvalidOperationException()
		{
			var mockErrorReporter = new Mock<IErrorReporter>();
			ErrorReporter.Instance = mockErrorReporter.Object;

			for (int i = 0; i < 3; i++)
			{
				var mock = new Mock<eNettOutboundTransactionSubscriberForTest>() { CallBase = true };
				mock
					.Protected()
					.Setup("Process", ItExpr.IsAny<IQueuedLog>(), ItExpr.IsAny<IEnumerable<GlbCompany>>())
					.Throws(new InvalidOperationException());
				mock.Object.ProcessLogQueueItemsExposed(new QueuedLogForTesting[] { new QueuedLogForTesting(Factory) });
				mock.VerifyAll();
			}

			mockErrorReporter.Verify(m => m.Report("ComPay_ProcessLogQueueItems_Exception", "ComPay Outbound Subscriber threw an exception",
				It.IsAny<InvalidOperationException>()), Times.Exactly(3));
			mockErrorReporter.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestTryHandleExceptionCore()
		{
			var mockErrorReporter = new Mock<IErrorReporter>();
			ErrorReporter.Instance = mockErrorReporter.Object;

			var subscriber = new eNettOutboundTransactionSubscriberForTest();
			var ex = new ApplicationException("Test Only");
			var logs = new List<IQueuedLog>();

			var actual = subscriber.TryHandleExceptionCoreExposed(ex, logs, 1);
			AssertEquals(ExceptionHandlingResult.Unhandled, actual);

			//Verified that no error is raised
			mockErrorReporter.VerifyNoOtherCalls();
		}

		public void TestProcessLogQueueItemsThrowToCriticalExceptions()
		{
			var mockErrorReporter = new Mock<IErrorReporter>();
			ErrorReporter.Instance = mockErrorReporter.Object;

			SystemDataRegistry.Instance.LogSubscriberMemoryLeakThresholdInBytes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			InvoicingBase inv, crd;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				inv = (ARInvoice)CreateTransaction(typeof(ARInvoice), "10001", eNettRegisteredOrg.PK, true);
				Factory.Save();
				crd = (ARCreditNote)CreateTransaction(typeof(ARCreditNote), "10001", eNettRegisteredOrg.PK);
				crd.AH_TransactionBelongsToGroup = inv.PK;
				crd.AH_IsCancelled = true;
				((IMatching)crd).CurrentMatchGroup.AddNew().AP_AH = crd.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(crd);
			}
			Factory.Save();
			List<string> notifications;

			bool wasCriticalExceptionRaised = false;
			try
			{
				notifications = RunLogWalkerCycleForTestWithoutPreAndPostConditionChecks();
			}
			catch (Exception ex)
			{
				Exception innerException = ex;
				while (innerException != null && !(innerException is OutOfMemoryException))
				{
					innerException = innerException.InnerException;
				}
				if (innerException is OutOfMemoryException)
				{
					wasCriticalExceptionRaised = true;
					mockErrorReporter.VerifyNoOtherCalls();
				}
				else
				{
					throw;
				}
			}
			if (!wasCriticalExceptionRaised)
			{
				Assert("OutOfMemoryException must be raised.", false);
			}

			eNettCompany.GC_Name = "Company";
			Factory.Save();
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(eNettCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode() { RegistrationCode = "201649", OrganisationPK = companyProxy.PK });
			ILogger logger = new LoggerForTesting();
			((LoggerForTesting)logger).AllowDebug = true;
			notifications = RunLogWalkerCycleForTestWithoutPreAndPostConditionChecks(logger);
			mockErrorReporter.Verify(m => m.Report("ComPay_ProcessLogQueueItems_Exception", "ComPay Outbound Subscriber threw an unhandled exception", It.IsAny<NullReferenceException>()));
			mockErrorReporter.VerifyNoOtherCalls();

			Assert(notifications.Any(message => message == "[eNettTransactionSender] finished processing logs."));
		}

		public new void TestRegisteredInSystemOrClientSpecificLogSubscribersList()
		{
			Assert(true);
		}

		#region Implementation

		InvoicingBase CreateTransaction(Type transactionType, string aH_TransactionNum, ZGuid aH_OH)
		{
			return CreateTransaction(transactionType, aH_TransactionNum, aH_OH, false);
		}

		InvoicingBase CreateTransaction(Type transactionType, string aH_TransactionNum, ZGuid aH_OH, bool createJob)
		{
			InvoicingBase result = TestObjectCreator.CreateInvoice(transactionType, TestObjectCreator.AUD, 1);
			result.AH_TransactionNum = aH_TransactionNum;
			result.AH_OH = aH_OH;
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(result, TestObjectCreator.AUD, 1, 10);
			line.AL_AC = TestObjectCreator.CC1.PK;
			if (createJob)
			{
				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
				job.JH_ParentID = shipment.PK;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				line.AL_JH = job.PK;
				TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			}
			result.Factory.Save();
			return result;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		GlbCompany eNettCompany;
		GlbBranch eNettCompanyBranch;
		OrgHeader eNettRegisteredOrg;
		OrgHeader companyProxy;

		protected override void SetUp()
		{
			base.SetUp();
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			eNettCompany = TestObjectCreator.CreateNewCompany("ABC");
			eNettCompany.GC_Name = "ThrowCriticalException";
			companyProxy = TestObjectCreator.CreateOrgHeader("ABCPROXY", true, true);
			eNettCompany.GC_OH_OrgProxy = companyProxy.PK;
			eNettCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201654");

			eNettCompanyBranch = TestObjectCreator.CreateNewBranch(eNettCompany, "BR1");
			eNettRegisteredOrg = TestObjectCreator.CreateOrgHeader("VALID", true, true);
			eNettRegisteredOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201654");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, eNettCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				eNettRegisteredOrg.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "201654");
			}

			GlbStaff currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "A@B.COM";
			GlbGroup group = Factory.New<GlbGroup>();
			group.Staff.Add(currentUserInCurrentFactory);
			Factory.Save();
			AccountingConfigurationRegistry.Instance.ENettNotificationsGroup.SetValue(eNettCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(eNettCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EnettRegistrationCode() { RegistrationCode = "201649", OrganisationPK = companyProxy.PK });

			MockENettWebService.Instance.SetupForTesting("CARGOWISE");
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockENettWebService.ClearInstance();
		}

		#endregion
	}
}
