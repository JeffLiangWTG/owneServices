using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	[TestedType(typeof(BatchQueueInvoicesForEInvoicingServiceTask))]
	public class BatchQueueInvoicesForEInvoicingServiceTaskTest : ServiceTaskTestCase<BatchQueueInvoicesForEInvoicingServiceTask>
	{
		public void TestPeriodSettings()
		{
			var attribute = GetHostedServiceAttributes().FirstOrDefault();

			AssertEquals("20minutes", attribute.MinimumPeriod);
			AssertEquals("1hour", attribute.DefaultSchedule.RunEvery);
		}

		public void TestInvoiceAreQueued()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Romania))
			{
				var invoice1 = CreateTransactionWithNoPiviot(new DateTime(2024, 01, 10), "01");
				var invoice2 = CreateTransactionWithNoPiviot(new DateTime(2023, 01, 10), "02");
				var invoice3 = CreateTransactionWithNoPiviot(new DateTime(2024, 01, 10), "01", "APINV");
				var invoice4 = CreateTransactionWithNoPiviot(new DateTime(2023, 01, 10), "02", "APINV");
				var crd1 = CreateTransactionWithNoPiviot(new DateTime(2024, 01, 10), "01", "ARCRD");
				var crd2 = CreateTransactionWithNoPiviot(new DateTime(2023, 01, 10), "02", "ARCRD");

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AccountingElectronicMessagingRegistry.Instance.QueueOldTransactionsFromDateReceivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2024, 01, 01));

				CombineAssertions("PreRequisite", () =>
				{
					AssertEquals(true, new ElectronicInvoicingTransactionProxy(invoice1.Item1).IsEligibleToCreatePivot());
					AssertEquals(true, new ElectronicInvoicingTransactionProxy(invoice2.Item1).IsEligibleToCreatePivot());
					AssertEquals(true, new ElectronicInvoicingTransactionProxy(crd1.Item1).IsEligibleToCreatePivot());
					AssertEquals(true, new ElectronicInvoicingTransactionProxy(crd2.Item1).IsEligibleToCreatePivot());

					AssertEquals(false, new ElectronicInvoicingTransactionProxy(invoice3.Item1).IsEligibleToCreatePivot());
					AssertEquals(false, new ElectronicInvoicingTransactionProxy(invoice4.Item1).IsEligibleToCreatePivot());
				});

				var logger = new TestServiceLogger();
				var serviceTask = new BatchQueueInvoicesForEInvoicingServiceTask() { ServiceLogger = logger };
				serviceTask.RunTask();

				AssertContains($"Starting queue 2 transactions for {GlbCompany.CurrentCompany.CompanyName}.", logger.ToString());
				AssertContains("Successfully batch queued transactions for eligible companies.", logger.ToString());

				CombineAssertions("Only invoice after the date should be queued.", () =>
				{
					AssertNotNull(invoice1.Item2.Invoke());
					AssertNotNull(crd1.Item2.Invoke());

					AssertNull(invoice2.Item2.Invoke());
					AssertNull(crd2.Item2.Invoke());
					AssertNull(invoice3.Item2.Invoke());
					AssertNull(invoice4.Item2.Invoke());
				});

				serviceTask.RunTask();
				AssertEquals("Registry should be set back to default when process finished.", DateTime.MinValue, AccountingElectronicMessagingRegistry.Instance.QueueOldTransactionsFromDateReceivables.Value);
			}
		}

		public void TestBatching()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Romania))
			{
				var invoice1 = CreateTransactionWithNoPiviot(new DateTime(2024, 01, 10), "01");
				var invoice2 = CreateTransactionWithNoPiviot(new DateTime(2024, 01, 10), "02");
				var invoice3 = CreateTransactionWithNoPiviot(new DateTime(2024, 01, 10), "03");

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AccountingElectronicMessagingRegistry.Instance.QueueOldTransactionsFromDateReceivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2024, 01, 01));

				CombineAssertions("PreRequisite", () =>
				{
					AssertEquals(true, new ElectronicInvoicingTransactionProxy(invoice1.Item1).IsEligibleToCreatePivot());
					AssertEquals(true, new ElectronicInvoicingTransactionProxy(invoice2.Item1).IsEligibleToCreatePivot());
					AssertEquals(true, new ElectronicInvoicingTransactionProxy(invoice3.Item1).IsEligibleToCreatePivot());
				});

				var logger = new TestServiceLogger();
				var serviceTask = new BatchQueueInvoicesForEInvoicingServiceTask() { ServiceLogger = logger, BatchSize_ForTestOnly = 2 };
				serviceTask.RunTask();

				AssertContains($"Information|Starting queue 2 transactions for {GlbCompany.CurrentCompany.CompanyName}.", logger.ToString());
				AssertContains("Debug|Successfully batch queued transactions for eligible companies.", logger.ToString());

				var count = new BusinessObject[]
				{
					invoice1.Item2.Invoke(),
					invoice2.Item2.Invoke(),
					invoice3.Item2.Invoke()
				}.Count(x => x != null);

				AssertEquals("Only 2 invoice after the date should be queued.", 2, count);
				AssertNotEquals("Registry should not be set back to default when process finished.", DateTime.MinValue, AccountingElectronicMessagingRegistry.Instance.QueueOldTransactionsFromDateReceivables.Value);

				serviceTask.RunTask();

				AssertContains($"Information|Starting queue 1 transactions for {GlbCompany.CurrentCompany.CompanyName}.", logger.ToString());
				AssertContains("Debug|Successfully batch queued transactions for eligible companies.", logger.ToString());

				count = new BusinessObject[]
				{
					invoice1.Item2.Invoke(),
					invoice2.Item2.Invoke(),
					invoice3.Item2.Invoke()
				}.Count(x => x != null);

				AssertEquals("All invoices after the date should be queued after 2 batches.", 3, count);

				serviceTask.RunTask();
				AssertEquals("Registry should be set back to default when process finished.", DateTime.MinValue, AccountingElectronicMessagingRegistry.Instance.QueueOldTransactionsFromDateReceivables.Value);
			}
		}

		public void TestBatching_MultipleCompanies()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Romania))
			{
				var org = TestObjectCreator.CreateOrgHeader("2010.00.01", true, true, true, true, true, true);
				var anotherCompanyAndBranch = TestObjectCreator.CreateCompanyAndBranch(CountryCodes.Romania, org);
				var anotherCompany = anotherCompanyAndBranch.Company;
				anotherCompany.GC_Name = "Arasaka";
				Factory.Save();

				var invoice1 = CreateTransactionWithNoPiviot(new DateTime(2024, 01, 10), "01");
				var invoice2 = CreateTransactionWithNoPiviot(new DateTime(2024, 01, 10), "02", "ARINV", anotherCompany, org);
				var invoice3 = CreateTransactionWithNoPiviot(new DateTime(2024, 01, 10), "03", "ARINV", anotherCompany, org);

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AccountingElectronicMessagingRegistry.Instance.QueueOldTransactionsFromDateReceivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2024, 01, 01));
				AccountingElectronicMessagingRegistry.Instance.QueueOldTransactionsFromDateReceivables.SetValue(anotherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2024, 01, 01));

				CombineAssertions("PreRequisite", () =>
				{
					AssertEquals(true, new ElectronicInvoicingTransactionProxy(invoice1.Item1).IsEligibleToCreatePivot());
					AssertEquals(true, new ElectronicInvoicingTransactionProxy(invoice2.Item1).IsEligibleToCreatePivot());
					AssertEquals(true, new ElectronicInvoicingTransactionProxy(invoice3.Item1).IsEligibleToCreatePivot());
				});

				var logger = new TestServiceLogger();
				var serviceTask = new BatchQueueInvoicesForEInvoicingServiceTask() { ServiceLogger = logger, BatchSize_ForTestOnly = 2 };
				serviceTask.RunTask();

				var firstLog = logger.ToString();
				var currentCompanyWasInFirstRun = firstLog.Contains($"Starting queue 1 transactions for {GlbCompany.CurrentCompany.CompanyName}.");
				if (currentCompanyWasInFirstRun)
				{
					AssertContains($"Starting queue 1 transactions for Arasaka.", logger.ToString());
				}
				else
				{
					AssertContains($"Starting queue 2 transactions for Arasaka.", logger.ToString());
				}

				AssertContains("Successfully batch queued transactions for eligible companies.", logger.ToString());

				var count = new BusinessObject[]
				{
					invoice1.Item2.Invoke(),
					invoice2.Item2.Invoke(),
					invoice3.Item2.Invoke()
				}.Count(x => x != null);

				AssertEquals("Only 2 invoice after the date should be queued.", 2, count);

				serviceTask.RunTask();

				if (currentCompanyWasInFirstRun)
				{
					AssertContains($"Starting queue 1 transactions for Arasaka.", logger.ToString());
				}
				else
				{
					AssertContains($"Starting queue 1 transactions for {GlbCompany.CurrentCompany.CompanyName}.", logger.ToString());
				}

				count = new BusinessObject[]
				{
					invoice1.Item2.Invoke(),
					invoice2.Item2.Invoke(),
					invoice3.Item2.Invoke()
				}.Count(x => x != null);

				AssertEquals("All invoices after the date should be queued after 2 batches.", 3, count);
			}
		}

		public void TestErrorReportWhenIBatchQueueInvoicesForEInvoicingProviderHasInconsistentLogic()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Romania))
			{
				var exceptionReporter = ExceptionReporterTestListener.Instance;
				var arInvoice = CreateTransactionWithNoPiviot(new DateTime(2024, 01, 10), "01");
				var apInvoice = CreateTransactionWithNoPiviot(new DateTime(2024, 01, 10), "01", "APINV");

				var expectedResult = new TransactionHeader[] { arInvoice.Item1, apInvoice.Item1 };

				var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
				var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
				var mockDecider = new Mock<IEInvoicingEligibilityDecider>();

				mockIAccountingCountryFactory
					.As<IInstanceProvider<IBatchQueueInvoicesForEInvoicingProvider>>()
					.Setup(x => x.Get().GetTransactionsToBeQueued(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>(), It.IsAny<DateTime>(), It.IsAny<int>()))
					.Returns(expectedResult);

				mockDecider.Setup(x => x.IsTransactionEligible(It.IsAny<ARInvoice>())).Returns(true);
				mockIAccountingCountryFactory
					.As<IInstanceProvider<IEInvoicingEligibilityDecider>>()
					.Setup(x => x.Get())
					.Returns(mockDecider.Object);

				mockIGlobalAccountingCountryFactory
					.Setup(x => x.GetCountryFactory(It.IsAny<ZString>()))
					.Returns(mockIAccountingCountryFactory.Object);

				ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

				CombineAssertions("PreRequisite", () =>
				{
					AssertEquals(true, new ElectronicInvoicingTransactionProxy(arInvoice.Item1).IsEligibleToCreatePivot());
					AssertEquals(false, new ElectronicInvoicingTransactionProxy(apInvoice.Item1).IsEligibleToCreatePivot());
				});

				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AccountingElectronicMessagingRegistry.Instance.QueueOldTransactionsFromDateReceivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2024, 01, 01));

				var logger = new TestServiceLogger();
				var serviceTask = new BatchQueueInvoicesForEInvoicingServiceTask() { ServiceLogger = logger };
				serviceTask.RunTask();

				AssertContains($"Starting queue 1 transactions for {GlbCompany.CurrentCompany.CompanyName}.", logger.ToString());
				AssertContains("Debug|Successfully batch queued transactions for eligible companies.", logger.ToString());
				AssertContains("Some transactions to be queued are not eligible for e-Invoicing. Etc, the one with transaction number APINV01.", logger.ToString());
				AssertEquals(1, exceptionReporter.Count);
				AssertEquals("Should have error reported.", "Some transactions to be queued are not eligible for e-Invoicing. Etc, the one with transaction number APINV01. Company code EDI.", exceptionReporter.Last().Message);
				exceptionReporter.Clear();
			}
		}

		(TransactionHeader, Func<AccEInvoicingTransactionPivot>) CreateTransactionWithNoPiviot(DateTime invoiceDate, string num, string type = "ARINV")
		{
			return CreateTransactionWithNoPiviot(invoiceDate, num, type, GlbCompany.CurrentCompany, TestObjectCreator.AALSHI);
		}

		(TransactionHeader, Func<AccEInvoicingTransactionPivot>) CreateTransactionWithNoPiviot(DateTime invoiceDate, string num, string type, GlbCompany company, OrgHeader orgHeader)
		{
			TransactionHeader transactionHeader;
			var random = new Random();
			if (type == "ARCRD")
			{
				transactionHeader = TestObjectCreator.CreateARCreditNote($"ARCRD{num}", orgHeader, TestObjectCreator.EUR, 1m, "AR Credit Note");
			}
			else if (type == "APINV")
			{
				transactionHeader = TestObjectCreator.CreateAPInvoice<APInvoice>($"APINV{num}", TestObjectCreator.EUR, 1m, 1000m, 0m, 0m, 1000m, 0m, 0m, orgHeader);
			}
			else
			{
				transactionHeader = TestObjectCreator.CreateARInvoice<ARInvoice>($"ARINV{num}", TestObjectCreator.EUR, 100m, orgHeader);
			}
			transactionHeader.AH_PostDate = invoiceDate;
			transactionHeader.AH_GC = company.PK;
			transactionHeader.AH_GB = company.FirstActiveBranch.PK;

			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(AccEInvoicingTransactionPivot));
			query.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionHeader.PK);
			var pivotQuery = () => Factory.LoadTop1<AccEInvoicingTransactionPivot>(query);
			var pivot = pivotQuery.Invoke();
			pivot?.Delete();
			Factory.Save();

			AssertNull(pivotQuery.Invoke());

			return (transactionHeader, pivotQuery);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);

		TestObjectCreator testObjectCreator;
	}
}
