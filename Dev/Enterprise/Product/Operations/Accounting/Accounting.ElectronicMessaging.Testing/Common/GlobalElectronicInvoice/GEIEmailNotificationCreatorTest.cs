using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class GEIEmailNotificationCreatorTest : EInvoicingEmailNotificationCreatorTest
	{
		[SuspendCriticalValidation]
		public void TestEmailNotificationCreation_CompanyDoesNotHaveActiveBranch()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("AUMEL");
			company.Branches[0].GB_IsActive = false;
			company.CompanyName = "TestCompany";
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 0.25m);
			invoice.AH_GC = company.PK;
			Factory.Save();

			var logger = new DetailedLoggerForTest();
			var emailCreator = new GEIEmailNotificationCreator(null, invoice, new List<ZString>(), logger);
			emailCreator.SendEmail();
			AssertEquals("Logs count.", 1, logger.Logs.Count);
			var expectedMessage = $"Email Notification was not sent for Transaction 'AR INV {invoice.AH_TransactionNum}' of company 'TestCompany' due to missing company branch.";
			AssertEquals("Logs match.", LogType.Warning, logger.Logs[0].Item1);
			AssertContains("Logs match.", expectedMessage, logger.Logs[0].Item2);
		}

		[SuspendCriticalValidation]
		public void TestSendEmailNotification_ForManyTransactions()
		{
			CreateStaffInAllGroup();
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, Groups.AllPK))
			{
				var transactions = new[]
					{
						TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 0.25m),
						TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.25m),
						TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 2.25m)
					}
					.Select(t => IncorrectTransactionDetails.FromBizo(t, Enumerable.Empty<ZString>()))
					.ToArray();

				var logger = new TestServiceLogger();
				var emailCreator = new GEIEmailNotificationCreator(null, transactions, GlbCompany.CurrentCompany, logger);
				emailCreator.SendEmail();

				AssertContains("Debug|E-Reporting Email Notification task completed.", logger.ToString());
				AssertContains("Debug|Email Notification was sent successfully for Eagle Datamation International.", logger.ToString());
				AssertEquals("Email sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[SuspendCriticalValidation]
		public void TestSendEmailNotification_ForManyTransactionsWithSameHeader()
		{
			CreateStaffInAllGroup();
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, Groups.AllPK))
			{
				var transactions = new[]
					{
						TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 0.25m),
						TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.25m),
						TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 2.25m)
					}
					.Select(t => IncorrectTransactionDetails.FromBizo(t, Enumerable.Empty<ZString>()))
					.ToArray();
				var batch = TestObjectCreator.CreateEInvoicingBatch(111, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);

				var logger = new TestServiceLogger();
				var emailCreator = new GEIEmailNotificationCreator(null, IncorrectTransactionDetails.FromBizo(batch, new ZString[] { "batch error" }), transactions, GlbCompany.CurrentCompany, logger);
				emailCreator.SendEmail();
				AssertContains("Debug|E-Reporting Email Notification task completed.", logger.ToString());
				AssertContains("Debug|Email Notification was sent successfully for Eagle Datamation International.", logger.ToString());
				AssertEquals("Email sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[SuspendCriticalValidation]
		public void TestEmailNotificationCreation_ForManyTransactionsAndBatch()
		{
			CreateStaffInAllGroup();
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, Groups.AllPK))
			{
				var batch = TestObjectCreator.CreateEInvoicingBatch(1273, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				var relatedTransactions = new[]
				{
					TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 0.25m),
					TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1.25m),
					TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 2.25m)
				};

				var logger = new TestServiceLogger();
				var emailCreator = new GEIEmailNotificationCreator(null, batch, new List<ZString>(), relatedTransactions, logger);
				emailCreator.SendEmail();

				AssertContains("Debug|E-Reporting Email Notification task completed.", logger.ToString());
				AssertContains("Debug|Email Notification was sent successfully for Eagle Datamation International.", logger.ToString());
				AssertEquals("Email sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		void CreateStaffInAllGroup()
		{
			var staffPluto = Factory.New<GlbStaff>();
			staffPluto.GS_LoginName = "someone";
			staffPluto.GS_Code = "SOM";
			staffPluto.GS_EmailAddress = "someone@cargowiseone";
			staffPluto.Groups.Add(Factory.Load<GlbGroup>(Core.Constants.Groups.AllPK));
			Factory.Save();
		}
	}
}
