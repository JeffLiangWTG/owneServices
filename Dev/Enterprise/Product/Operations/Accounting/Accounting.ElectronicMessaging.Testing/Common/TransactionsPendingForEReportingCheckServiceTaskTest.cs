using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	[TestedType(typeof(TransactionsPendingForEReportingCheckServiceTask))]
	public class TransactionsPendingForEReportingCheckServiceTaskTest : ServiceTaskTestCase<TransactionsPendingForEReportingCheckServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		[TestDate(2024, 3, 15, 23, 30, 30)]
		public void TestRunServiceTask_OnlySetCompanyLevelRegistry()
		{
			var invoice1 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 11, 11, 11, 11), new ZDateTime(2024, 3, 1, 11, 11, 11));
			var invoice2 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 13, 11, 11, 11), new ZDateTime(2024, 3, 3, 11, 11, 11));
			var invoice3 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 15, 11, 11, 11), new ZDateTime(2024, 3, 5, 11, 11, 11));
			var invoice4 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXTIJ_UTCMinus8, new ZDateTime(2024, 3, 11, 11, 11, 11), new ZDateTime(2024, 3, 1, 11, 11, 11));
			var invoice5 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXTIJ_UTCMinus8, new ZDateTime(2024, 3, 13, 11, 11, 11), new ZDateTime(2024, 3, 3, 11, 11, 11));
			var invoice6 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXTIJ_UTCMinus8, new ZDateTime(2024, 3, 15, 11, 11, 11), new ZDateTime(2024, 3, 5, 11, 11, 11));

			Factory.Save();

			var notificationGroup = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = TestingNotificationGroup1.PK.ToGuid(),
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate,
				Days = 2
			};

			AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.SetValue(TestingMexicanCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroup);

			var serviceTask = new TransactionsPendingForEReportingCheckServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertLogsEqual(logger, new[] {
				"Information|Transactions Pending for E-Reporting Check Service started.",
				"Information|There are 2 transactions before 03-14-2024 for branch: TMX - MEX.",
				"Information|There are 2 transactions before 03-14-2024 for branch: TMX - TIJ.",
				"Information|There are totally 4 transactions for company: TMX.",
				"Information|Email has been sent to TMX.",
				"Information|Transactions Pending for E-Reporting Check Service ended."
			});

			var actualEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			using (DisposableEnvironment.ForCompany("TMX"))
			{
				AssertEmailsEqual(actualEmail, "TMX", TestingNotificationGroup1.PK.ToGuid(), new[] { invoice1, invoice2, invoice4, invoice5 });
			}

			//Testing by using PostDate
			notificationGroup.DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.PostDate;
			notificationGroup.Days = 10;
			AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.SetValue(TestingMexicanCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroup);
			var serviceTask2 = new TransactionsPendingForEReportingCheckServiceTask();
			var logger2 = InitialiseAndRunTaskSchedule(serviceTask2);

			AssertLogsEqual(logger2, new[] {
				"Information|Transactions Pending for E-Reporting Check Service started.",
				"Information|There are 3 transactions before 03-06-2024 for branch: TMX - MEX.",
				"Information|There are 3 transactions before 03-06-2024 for branch: TMX - TIJ.",
				"Information|There are totally 6 transactions for company: TMX.",
				"Information|Email has been sent to TMX.",
				"Information|Transactions Pending for E-Reporting Check Service ended."
			});

			var actualEmail2 = Env.OutgoingMailManager.EmailsCreated.Last();

			using (DisposableEnvironment.ForCompany("TMX"))
			{
				AssertEmailsEqual(actualEmail2, "TMX", TestingNotificationGroup1.PK.ToGuid(), new[] { invoice1, invoice2, invoice3, invoice4, invoice5, invoice6 });
			}
		}

		[TestDate(2024, 3, 15, 23, 30, 30)]
		public void TestRunServiceTask_OnlySetCompanyLevelRegistry_TwoCompanies()
		{
			var invoice1 = CreateARInvoiceWithEInvoicingPivot(TestingKoreanCompany, TestingBranchKRSEL_UTCPlus9, new ZDateTime(2024, 3, 11, 11, 11, 11), new ZDateTime(2024, 3, 1, 11, 11, 11));
			var invoice2 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 11, 11, 11, 11), new ZDateTime(2024, 3, 1, 11, 11, 11));
			var invoice3 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 11, 11, 11, 11), new ZDateTime(2024, 3, 1, 11, 11, 11), EInvoicingPivotState.Queued); //Test AIP status by the way

			Factory.Save();

			var notificationGroup1 = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = TestingNotificationGroup1.PK.ToGuid(),
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate,
				Days = 3
			};

			var notificationGroup2 = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = TestingNotificationGroup2.PK.ToGuid(),
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate,
				Days = 2
			};

			AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.SetValue(TestingKoreanCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroup1);
			AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.SetValue(TestingMexicanCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroup2);

			var serviceTask = new TransactionsPendingForEReportingCheckServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertLogsEqual(logger, new[] {
				"Information|Transactions Pending for E-Reporting Check Service started.",
				"Information|There are 1 transactions before 03-13-2024 for branch: TKR - SEL.",
				"Information|There are totally 1 transactions for company: TKR.",
				"Information|Email has been sent to TKR.",
				"Information|There are 1 transactions before 03-14-2024 for branch: TMX - MEX.",
				"Information|There are 0 transactions before 03-14-2024 for branch: TMX - TIJ.",
				"Information|There are totally 1 transactions for company: TMX.",
				"Information|Email has been sent to TMX.",
				"Information|Transactions Pending for E-Reporting Check Service ended."
			});

			var actualEmails = Env.OutgoingMailManager.EmailsCreated;

			using (DisposableEnvironment.ForCompany("TKR"))
			{
				AssertEmailsEqual(actualEmails[0], "TKR", TestingNotificationGroup1.PK.ToGuid(), new[] { invoice1 });
			}

			using (DisposableEnvironment.ForCompany("TMX"))
			{
				AssertEmailsEqual(actualEmails[1], "TMX", TestingNotificationGroup2.PK.ToGuid(), new[] { invoice2 });
			}
		}

		[TestDate(2024, 3, 15, 6, 30, 30)]
		public void TestRunServiceTask_OnlySetCompanyLevelRegistry_DifferentTimeZone()
		{
			TestDateAttribute.UseUNLOCO = true;

			var invoice1 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 11, 20, 20, 20), new ZDateTime(2024, 3, 1, 11, 11, 11));
			var invoice2 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 13, 20, 20, 20), new ZDateTime(2024, 3, 5, 11, 11, 11));
			var invoice3 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXTIJ_UTCMinus8, new ZDateTime(2024, 3, 11, 20, 20, 20), new ZDateTime(2024, 3, 1, 11, 11, 11));
			var invoice4 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXTIJ_UTCMinus8, new ZDateTime(2024, 3, 13, 20, 20, 20), new ZDateTime(2024, 3, 5, 11, 11, 11));

			Factory.Save();

			var notificationGroup = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = TestingNotificationGroup1.PK.ToGuid(),
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate,
				Days = 2
			};

			AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.SetValue(TestingMexicanCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroup);

			var serviceTask = new TransactionsPendingForEReportingCheckServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertLogsEqual(logger, new[] {
				"Information|Transactions Pending for E-Reporting Check Service started.",
				"Information|There are 2 transactions before 03-14-2024 for branch: TMX - MEX.",
				"Information|There are 1 transactions before 03-13-2024 for branch: TMX - TIJ.",
				"Information|There are totally 3 transactions for company: TMX.",
				"Information|Email has been sent to TMX.",
				"Information|Transactions Pending for E-Reporting Check Service ended."
			});

			var actualEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			using (DisposableEnvironment.ForCompany("TMX"))
			{
				AssertEmailsEqual(actualEmail, "TMX", TestingNotificationGroup1.PK.ToGuid(), new[] { invoice1, invoice2, invoice3 });
			}
		}

		[TestDate(2024, 3, 15, 23, 30, 30)]
		public void TestRunServiceTask_OnlySetBranchLevelRegistry_SetOneBranch()
		{
			var invoice1 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 11, 11, 11, 11), new ZDateTime(2024, 3, 1, 11, 11, 11));
			var invoice2 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 15, 11, 11, 11), new ZDateTime(2024, 3, 5, 11, 11, 11));
			var invoice3 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXTIJ_UTCMinus8, new ZDateTime(2024, 3, 11, 11, 11, 11), new ZDateTime(2024, 3, 1, 11, 11, 11));
			var invoice4 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXTIJ_UTCMinus8, new ZDateTime(2024, 3, 15, 11, 11, 11), new ZDateTime(2024, 3, 5, 11, 11, 11));

			Factory.Save();

			var notificationGroup = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = TestingNotificationGroup1.PK.ToGuid(),
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate,
				Days = 2
			};

			AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.SetValue(Guid.Empty, TestingBranchMXMEX_UTCMinus6.PK.ToGuid(), Guid.Empty, notificationGroup);

			var serviceTask = new TransactionsPendingForEReportingCheckServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertLogsEqual(logger, new[] {
				"Information|Transactions Pending for E-Reporting Check Service started.",
				"Information|There are 1 transactions before 03-14-2024 for branch: TMX - MEX.",
				"Information|Email has been sent to MEX.",
				"Information|Transactions Pending for E-Reporting Check Service ended."
			});

			var actualEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			using (DisposableEnvironment.ForCompany("TMX"))
			{
				AssertEmailsEqual(actualEmail, "MEX", TestingNotificationGroup1.PK.ToGuid(), new[] { invoice1 });
			}
		}

		[TestDate(2024, 3, 15, 23, 30, 30)]
		public void TestRunServiceTask_OnlySetBranchLevelRegistry_NoEmail()
		{
			var invoice1 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 11, 11, 11, 11), new ZDateTime(2024, 3, 1, 11, 11, 11));

			Factory.Save();

			var notificationGroup = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = TestingNotificationGroup1.PK.ToGuid(),
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate,
				Days = 10
			};

			AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.SetValue(Guid.Empty, TestingBranchMXMEX_UTCMinus6.PK.ToGuid(), Guid.Empty, notificationGroup);

			var serviceTask = new TransactionsPendingForEReportingCheckServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertLogsEqual(logger, new[] {
				"Information|Transactions Pending for E-Reporting Check Service started.",
				"Information|There are 0 transactions before 03-06-2024 for branch: TMX - MEX.",
				"Information|No email will be sent.",
				"Information|Transactions Pending for E-Reporting Check Service ended."
			});

			AssertEquals("There should be no email generated", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2024, 3, 15, 23, 30, 30)]
		public void TestRunServiceTask_OnlySetBranchLevelRegistry_SetAllBranches()
		{
			var invoice1 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 11, 11, 11, 11), new ZDateTime(2024, 3, 1, 11, 11, 11));
			var invoice2 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 15, 11, 11, 11), new ZDateTime(2024, 3, 5, 11, 11, 11));
			var invoice3 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXTIJ_UTCMinus8, new ZDateTime(2024, 3, 11, 11, 11, 11), new ZDateTime(2024, 3, 1, 11, 11, 11));
			var invoice4 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXTIJ_UTCMinus8, new ZDateTime(2024, 3, 15, 11, 11, 11), new ZDateTime(2024, 3, 5, 11, 11, 11));

			Factory.Save();

			var notificationGroup1 = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = TestingNotificationGroup1.PK.ToGuid(),
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate,
				Days = 2
			};

			var notificationGroup2 = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = TestingNotificationGroup2.PK.ToGuid(),
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate,
				Days = 4
			};

			AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.SetValue(Guid.Empty, TestingBranchMXMEX_UTCMinus6.PK.ToGuid(), Guid.Empty, notificationGroup1);
			AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.SetValue(Guid.Empty, TestingBranchMXTIJ_UTCMinus8.PK.ToGuid(), Guid.Empty, notificationGroup2);

			var serviceTask = new TransactionsPendingForEReportingCheckServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertLogsEqual(logger, new[] {
				"Information|Transactions Pending for E-Reporting Check Service started.",
				"Information|There are 1 transactions before 03-14-2024 for branch: TMX - MEX.",
				"Information|Email has been sent to MEX.",
				"Information|There are 1 transactions before 03-12-2024 for branch: TMX - TIJ.",
				"Information|Email has been sent to TIJ.",
				"Information|Transactions Pending for E-Reporting Check Service ended."
			});

			var actualEmails = Env.OutgoingMailManager.EmailsCreated;
			using (DisposableEnvironment.ForCompany("TMX"))
			{
				AssertEmailsEqual(actualEmails[0], "MEX", TestingNotificationGroup1.PK.ToGuid(), new[] { invoice1 });
				AssertEmailsEqual(actualEmails[1], "TIJ", TestingNotificationGroup2.PK.ToGuid(), new[] { invoice3 });
			}
		}

		[TestDate(2024, 3, 15, 23, 30, 30)]
		public void TestRunServiceTask_SetCompanyAndBranchLevelRegistry_SetOneBranch()
		{
			var invoice1 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 11, 11, 11, 11), new ZDateTime(2024, 3, 1, 11, 11, 11));
			var invoice2 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 15, 11, 11, 11), new ZDateTime(2024, 3, 5, 11, 11, 11));
			var invoice3 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXTIJ_UTCMinus8, new ZDateTime(2024, 3, 11, 11, 11, 11), new ZDateTime(2024, 3, 1, 11, 11, 11));
			var invoice4 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXTIJ_UTCMinus8, new ZDateTime(2024, 3, 15, 11, 11, 11), new ZDateTime(2024, 3, 5, 11, 11, 11));

			Factory.Save();

			var notificationGroup1 = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = TestingNotificationGroup1.PK.ToGuid(),
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate,
				Days = 4
			};

			var notificationGroup2 = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = TestingNotificationGroup2.PK.ToGuid(),
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate,
				Days = 2
			};

			AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.SetValue(Guid.Empty, TestingBranchMXMEX_UTCMinus6.PK.ToGuid(), Guid.Empty, notificationGroup1);
			AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.SetValue(TestingMexicanCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroup2);

			var serviceTask = new TransactionsPendingForEReportingCheckServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertLogsEqual(logger, new[] {
				"Information|Transactions Pending for E-Reporting Check Service started.",
				"Information|There are 1 transactions before 03-12-2024 for branch: TMX - MEX.",
				"Information|Email has been sent to MEX.",
				"Information|There are 1 transactions before 03-14-2024 for branch: TMX - TIJ.",
				"Information|Email has been sent to TIJ.",
				"Information|Transactions Pending for E-Reporting Check Service ended."
			});

			var actualEmails = Env.OutgoingMailManager.EmailsCreated;
			using (DisposableEnvironment.ForCompany("TMX"))
			{
				AssertEmailsEqual(actualEmails[0], "MEX", TestingNotificationGroup1.PK.ToGuid(), new[] { invoice1 });
				AssertEmailsEqual(actualEmails[1], "TIJ", TestingNotificationGroup2.PK.ToGuid(), new[] { invoice3 });
			}
		}

		[TestDate(2024, 3, 15, 23, 30, 30)]
		public void TestRunServiceTask_SetCompanyAndBranchLevelRegistry_SetAllBranches()
		{
			var invoice1 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 11, 11, 11, 11), new ZDateTime(2024, 3, 1, 11, 11, 11));
			var invoice2 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXMEX_UTCMinus6, new ZDateTime(2024, 3, 15, 11, 11, 11), new ZDateTime(2024, 3, 5, 11, 11, 11));
			var invoice3 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXTIJ_UTCMinus8, new ZDateTime(2024, 3, 11, 11, 11, 11), new ZDateTime(2024, 3, 1, 11, 11, 11));
			var invoice4 = CreateARInvoiceWithEInvoicingPivot(TestingMexicanCompany, TestingBranchMXTIJ_UTCMinus8, new ZDateTime(2024, 3, 15, 11, 11, 11), new ZDateTime(2024, 3, 5, 11, 11, 11));

			Factory.Save();

			var notificationGroup1 = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = TestingNotificationGroup1.PK.ToGuid(),
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate,
				Days = 2
			};

			var notificationGroup2 = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = TestingNotificationGroup2.PK.ToGuid(),
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate,
				Days = 4
			};

			var notificationGroup3 = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = TestingNotificationGroup2.PK.ToGuid(),
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate,
				Days = 10
			};

			AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.SetValue(Guid.Empty, TestingBranchMXMEX_UTCMinus6.PK.ToGuid(), Guid.Empty, notificationGroup1);
			AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.SetValue(Guid.Empty, TestingBranchMXTIJ_UTCMinus8.PK.ToGuid(), Guid.Empty, notificationGroup2);
			AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.SetValue(TestingMexicanCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroup3);

			var serviceTask = new TransactionsPendingForEReportingCheckServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertLogsEqual(logger, new[] {
				"Information|Transactions Pending for E-Reporting Check Service started.",
				"Information|There are 1 transactions before 03-14-2024 for branch: TMX - MEX.",
				"Information|Email has been sent to MEX.",
				"Information|There are 1 transactions before 03-12-2024 for branch: TMX - TIJ.",
				"Information|Email has been sent to TIJ.",
				"Information|Transactions Pending for E-Reporting Check Service ended."
			});

			var actualEmails = Env.OutgoingMailManager.EmailsCreated;
			using (DisposableEnvironment.ForCompany("TMX"))
			{
				AssertEmailsEqual(actualEmails[0], "MEX", TestingNotificationGroup1.PK.ToGuid(), new[] { invoice1 });
				AssertEmailsEqual(actualEmails[1], "TIJ", TestingNotificationGroup2.PK.ToGuid(), new[] { invoice3 });
			}
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			TestingMexicanCompany = TestObjectCreator.CreateNewCompany("TMX", CountryCodes.Mexico, TestObjectCreator.ABIGAS);
			TestingKoreanCompany = TestObjectCreator.CreateNewCompany("TKR", CountryCodes.KoreaSouth, TestObjectCreator.ABIGAS);

			TestingBranchMXMEX_UTCMinus6 = TestObjectCreator.CreateBranch("MEX", TestingMexicanCompany);
			TestingBranchMXTIJ_UTCMinus8 = TestObjectCreator.CreateBranch("TIJ", TestingMexicanCompany);
			TestingBranchKRSEL_UTCPlus9 = TestObjectCreator.CreateBranch("SEL", TestingKoreanCompany);

			TestingBranchMXMEX_UTCMinus6.GB_RL_NKHomePort = "MXMEX";
			TestingBranchMXTIJ_UTCMinus8.GB_RL_NKHomePort = "MXTIJ";
			TestingBranchKRSEL_UTCPlus9.GB_RL_NKHomePort = "KRSEL";

			TestingNotificationGroup1 = TestObjectCreator.CreateStaffGroup("GP1");
			TestingNotificationGroup2 = TestObjectCreator.CreateStaffGroup("GP2");

			var staff1 = TestObjectCreator.CreateStaff("AAA");
			staff1.GS_EmailAddress = "testmail1@wisetechglobal.com";
			TestingNotificationGroup1.Staff.Add(staff1);

			var staff2 = TestObjectCreator.CreateStaff("BBB");
			staff2.GS_EmailAddress = "testmail2@wisetechglobal.com";
			TestingNotificationGroup1.Staff.Add(staff2);

			var staff3 = TestObjectCreator.CreateStaff("CCC");
			staff3.GS_EmailAddress = "testmail3@wisetechglobal.com";
			TestingNotificationGroup2.Staff.Add(staff3);

			var staff4 = TestObjectCreator.CreateStaff("DDD");
			staff4.GS_EmailAddress = "testmail4@wisetechglobal.com";
			TestingNotificationGroup2.Staff.Add(staff4);

			Factory.Save();
		}

		InvoicingBase CreateARInvoiceWithEInvoicingPivot(GlbCompany company, GlbBranch branch, ZDateTime invoiceDate, ZDateTime postDate, string pivotStatus = EInvoicingPivotState.Pending)
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			invoice.AH_GC = company.PK.ToGuid();
			invoice.AH_GB = branch.PK.ToGuid();
			invoice.AH_InvoiceDate = invoiceDate;
			invoice.AH_PostDate = postDate;

			TestObjectCreator.CreateEInvoicingTransactionPivot(invoice, status: pivotStatus);

			return invoice;
		}

		void AssertEmailsEqual(EmailDef actualEmail, string expectedCode, Guid expectedGroupPK, IEnumerable<TransactionHeader> expectedTransactions)
		{
			var expectedTransactionTuples = expectedTransactions.Select(x => (x.PK, x.AH_TransactionType, x.AH_TransactionNum)).ToList();

			new TransactionsPendingForEReportingCheckEmail(expectedCode, expectedGroupPK, expectedTransactionTuples).Send();
			var expectedEmail = Env.OutgoingMailManager.EmailsCreated.Last();

			AssertEquals("Subject", expectedEmail.Subject, actualEmail.Subject);
			AssertEquals("Body", expectedEmail.Body, actualEmail.Body);
			AssertContainsExactElementsInAnyOrder("Recipients", expectedEmail.Recipients, actualEmail.Recipients.Cast<RecipientDef>().Select(x => x.Email));
		}

		void AssertLogsEqual(TestServiceLogger logger, string[] messages)
		{
			CombineAssertions(() =>
			{
				AssertEquals("The expected logs and the actual logs should have the same number of entries.", logger.Count, messages.Length);
				for (var i = 0; i < messages.Length; i++)
				{
					AssertEquals(messages[i], logger[i]);
				}
			});
		}

		GlbCompany TestingMexicanCompany;
		GlbCompany TestingKoreanCompany;
		GlbBranch TestingBranchMXMEX_UTCMinus6;
		GlbBranch TestingBranchMXTIJ_UTCMinus8;
		GlbBranch TestingBranchKRSEL_UTCPlus9;
		GlbGroup TestingNotificationGroup1;
		GlbGroup TestingNotificationGroup2;

		//List<string> LoggerInfo;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
