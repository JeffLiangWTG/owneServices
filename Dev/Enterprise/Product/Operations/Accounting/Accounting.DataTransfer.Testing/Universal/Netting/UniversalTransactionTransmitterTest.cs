using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Universal.Netting.Testing
{
	public class UniversalTransactionTransmitterTest : TestCaseWithFactory
	{
		[TestDate(2015, 03, 20)]
		public void TestTransmitUniversalTransaction_NettingNotEnabled()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var arInvoice1 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 1000M, 100M, 1000M, 100M, participantOrg1, creator.GLHeader1.PK, "FIN");
			var apInvoice1 = creator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", creator.AUD, 1M, 1000M, 100M, 1000M, 100M, participantOrg1, creator.GLHeader2.PK, "FIN");

			AssertTransmitForAllStatuses(arInvoice1, false, "Should not be processed successfully as netting not enabled");
			AssertTransmitForAllStatuses(apInvoice1, false, "Should not be processed successfully as netting not enabled");
		}

		[TestDate(2015, 03, 20)]
		public void TestTransmitUniversalTransaction_NettingStartDateNotSet()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, participantOrg3.PK.ToGuid());

			var arInvoice1 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 1000M, 100M, 1000M, 100M, participantOrg1, creator.GLHeader1.PK, "FIN");
			var apInvoice1 = creator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", creator.AUD, 1M, 1000M, 100M, 1000M, 100M, participantOrg1, creator.GLHeader2.PK, "FIN");

			AssertTransmitForAllStatuses(arInvoice1, false, "Should not be processed successfully as netting start date not set");
			AssertTransmitForAllStatuses(apInvoice1, false, "Should not be processed successfully as netting start date not set");
		}

		[TestDate(2015, 03, 20)]
		public void TestTransmitUniversalTransaction_NettingStartDateIsSet()
		{
			using (Factory.AddDisposableService())
			{
				AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.NettingStartDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(1).ToDateTime());
				AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, participantOrg3.PK.ToGuid());

				var arInvoice1 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 1000M, 100M, 1000M, 100M, participantOrg1, creator.GLHeader1.PK, "FIN");
				var arInvoice2 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 1000M, 100M, 1000M, 100M, participantOrg1, creator.GLHeader1.PK, "FIN");
				var apInvoice1 = creator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", creator.AUD, 1M, 1000M, 100M, 1000M, 100M, participantOrg1, creator.GLHeader2.PK, "FIN");
				var apInvoice2 = creator.CreateInvoiceWithLine(typeof(APInvoice), "INV002", creator.AUD, 1M, 1000M, 100M, 1000M, 100M, participantOrg1, creator.GLHeader2.PK, "FIN");

				Factory.Save();

				arInvoice1.AH_DueDate = ZDateTime.UtcToday;
				apInvoice1.AH_DueDate = ZDateTime.UtcToday;

				AssertTransmitForAllStatuses(arInvoice1, false, "Should not be processed successfully as netting start date is after due date");
				AssertTransmitForAllStatuses(apInvoice1, false, "Should not be processed successfully as netting start date is after due date");

				arInvoice2.AH_DueDate = ZDateTime.UtcToday.AddDays(2);
				apInvoice2.AH_DueDate = ZDateTime.UtcToday.AddDays(2);

				AssertTransmitForAllStatuses(arInvoice2, true, "Should be processed successfully as netting start date is before due date");
				AssertTransmitForAllStatuses(apInvoice2, true, "Should be processed successfully as netting start date is before due date");
			}
		}

		[TestDate(2015, 03, 20)]
		public void TestTransmitUniversalTransaction_ReversedTransaction()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.NettingStartDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-1).ToDateTime());
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, participantOrg3.PK.ToGuid());
			var arInvoice1 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 1000M, 100M, 1000M, 100M, participantOrg1, creator.GLHeader1.PK, "FIN");
			Factory.Save();

			using (Factory.AddDisposableService())
			{
				AssertEquals("Should be processed successfully as netting start date is before due date", true, Transmit(new NotificationCollection(), arInvoice1, AccountingConstants.InvoiceAdditionalReference.Posted));
				Factory.Save();

				var reverser = new ARInvoiceReversing(arInvoice1 as ARInvoice);
				reverser.Reverse();
				Factory.Save();

				var reversedInvoice = arInvoice1.ReverseTransaction;
				var reversedInvoiceBizO = Factory.Load<ARCreditNote>(reversedInvoice.PK);
				AssertEquals("Credit note created as a result of reversal should be processed successfully", true, Transmit(new NotificationCollection(), reversedInvoiceBizO, AccountingConstants.InvoiceAdditionalReference.Reversed));
				Factory.Save();
			}
		}

		[TestDate(2015, 03, 20)]
		public void TestTransmitUniversalTransaction_EHubID()
		{
			using (Factory.AddDisposableService())
			{
				AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.NettingStartDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime());
				AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, participantOrg3.PK.ToGuid());

				var arInvoice1 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 100M, 10M, 100M, 10M, participantOrg1, creator.GLHeader1.PK, "FIN"); //should be processed successfully
				var arInvoice2 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV002", creator.AUD, 1M, 1000M, 100M, 1000M, 100M, participantOrg2, creator.GLHeader2.PK, "FIN"); //should not be processed as org does not have HID cus code setup.
				var apInvoice1 = creator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", creator.AUD, 1M, 1000M, 100M, 1000M, 100M, participantOrg1, creator.GLHeader2.PK, "FIN"); //should be processed successfully

				Factory.Save();

				AssertTransmitForAllStatuses(arInvoice1, true, "Should be processed successfully");
				AssertTransmitForAllStatuses(arInvoice2, false, "Should not be processed as org does not have HID setup");
				AssertTransmitForAllStatuses(apInvoice1, true, "Should be processed successfully");
			}
		}

		[TestDate(2015, 03, 20)]
		public void TestTransmitUniversalTransaction_SendingToOwnOrgProxy()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.NettingStartDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime());
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, participantOrg3.PK.ToGuid());

			var originalCompanyOrgProxyPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			try
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = participantOrg3.PK;
				GlbCompany.CurrentCompany.Factory.Save();

				var arInvoice1 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 100M, 10M, 100M, 10M, participantOrg1, creator.GLHeader1.PK, "FIN"); //should be processed successfully
				var arInvoice2 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV003", creator.AUD, 1M, 100M, 10M, 100M, 10M, participantOrg3, creator.GLHeader1.PK, "FIN"); //should not be sent is the debtor is the org proxy
				var apInvoice1 = creator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", creator.AUD, 1M, 100M, 10M, 100M, 10M, participantOrg1, creator.GLHeader1.PK, "FIN"); //should be processed successfully
				var apInvoice2 = creator.CreateInvoiceWithLine(typeof(APInvoice), "INV003", creator.AUD, 1M, 100M, 10M, 100M, 10M, participantOrg3, creator.GLHeader1.PK, "FIN"); //should not be sent is the creditor is the org proxy

				Factory.Save();

				using (Factory.AddDisposableService())
				{
					AssertTransmitForAllStatuses(arInvoice1, true, "Should be processed successfully");
					AssertTransmitForAllStatuses(arInvoice2, false, "Should not be processed as debtor is the org proxy");
					AssertTransmitForAllStatuses(apInvoice1, true, "Should be processed successfully");
					AssertTransmitForAllStatuses(apInvoice2, false, "Should not be processed as creditor is the org proxy");
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = originalCompanyOrgProxyPK;
			}
		}

		[TestDate(2015, 03, 20)]
		public void TestTransmitUniversalTransaction_SendingToBranchOrgProxy()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.NettingStartDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime());
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, participantOrg3.PK.ToGuid());

			var originalBranchOrgProxyPK = GlbCompany.CurrentCompany.Branches[0].GB_OH_OrgProxy;
			try
			{
				GlbCompany.CurrentCompany.Branches[0].GB_OH_OrgProxy = participantOrg3.PK;
				GlbCompany.CurrentCompany.Factory.Save();

				var arInvoice1 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 100M, 10M, 100M, 10M, participantOrg1, creator.GLHeader1.PK, "FIN"); //should be processed successfully
				var arInvoice2 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV003", creator.AUD, 1M, 100M, 10M, 100M, 10M, participantOrg3, creator.GLHeader1.PK, "FIN"); //should not be sent is the debtor is the branch org proxy
				var apInvoice1 = creator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", creator.AUD, 1M, 100M, 10M, 100M, 10M, participantOrg1, creator.GLHeader1.PK, "FIN"); //should be processed successfully
				var apInvoice2 = creator.CreateInvoiceWithLine(typeof(APInvoice), "INV003", creator.AUD, 1M, 100M, 10M, 100M, 10M, participantOrg3, creator.GLHeader1.PK, "FIN"); //should not be sent is the creditor is the branch org proxy

				Factory.Save();

				using (Factory.AddDisposableService())
				{
					AssertTransmitForAllStatuses(arInvoice1, true, "Should be processed successfully");
					AssertTransmitForAllStatuses(arInvoice2, false, "Should not be processed as debtor is the branch org proxy");
					AssertTransmitForAllStatuses(apInvoice1, true, "Should be processed successfully");
					AssertTransmitForAllStatuses(apInvoice2, false, "Should not be processed as creditor is the branch org proxy");
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = originalBranchOrgProxyPK;
			}
		}

		public void TestTransmitUniversalTransaction_WithCashInvoice()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.NettingStartDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime());
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, participantOrg3.PK.ToGuid());

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = participantOrg3.PK;

			var invoice = creator.CreateCashInvoice(typeof(ARInvoice), "INV001", creator.AUD, 1M, participantOrg1, 100M, 0M, creator.GLHeader1.PK, creator.AUDBankAccount);

			Factory.Save();

			var prefix = ZString.Format("{0}|{1}|", invoice.AH_Ledger, invoice.AH_TransactionType);

			var expectedPostedAndFullyMatchedReference = AccountingConstants.InvoiceAdditionalReference.PostedAndFullyMatched;
			Assert("Posted and Fully matched reference added", invoice.Logs.HasLogWith(StmALogSchema.SL_Reference, prefix + expectedPostedAndFullyMatchedReference));

			using (Factory.AddDisposableService())
			{
				using (new DisposableAction(() => UniversalTransactionTransmitter.IsThreadDelayForTestOnly = false))
				{
					UniversalTransactionTransmitter.IsThreadDelayForTestOnly = true;
					Transmit(new NotificationCollection(), invoice, expectedPostedAndFullyMatchedReference);
				}

				Factory.Save();

				var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, invoice.PK);
				var messages = Factory.Load<EDIMessage>(query);
				AssertEquals("2 messages generated", 2, messages.Length);

				var universalTransaction = messages.FirstOrDefault(x => x.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalTransaction);
				AssertNotNull("Universal Transaction transmitted", universalTransaction);

				var universalEvent = messages.FirstOrDefault(x => x.EM_MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent);
				AssertNotNull("Universal Event transmitted", universalEvent);

				Assert($"The event needs to be sent after the transaction is sent. The EventMessageDateTime is: {universalEvent.EM_MessageDateTime.DebuggerDisplay}, the TransactionMessageDateTime is: {universalTransaction.EM_MessageDateTime.DebuggerDisplay}", universalEvent.EM_MessageDateTime > universalTransaction.EM_MessageDateTime);
			}
		}

		[TestDate(2015, 03, 20)]
		public void TestTransmitUniversalTransaction_Status()
		{
			using (Factory.AddDisposableService())
			{
				AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.NettingStartDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime());
				AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, participantOrg3.PK.ToGuid());

				var arInvoice1 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 100M, 10M, 100M, 10M, participantOrg1, creator.GLHeader1.PK, "FIN");

				Factory.Save();

				AssertTransmitForAllStatuses(arInvoice1, true, "Should be processed successfully");
				var notifications = new NotificationCollection();
				AssertExceptionThrown<NotImplementedException>("Status is invalid", () => Transmit(notifications, arInvoice1, "Invalid"));
				Factory.Save();
			}
		}

		[TestDate(2015, 03, 20)]
		public void TestTransmitUniversalTransaction_ControlAccountNotSetup()
		{
			AccountingMasterFilesRegistry.Instance.EnableNetting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.NettingStartDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime());
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, participantOrg3.PK.ToGuid());

			var arInvoice1 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 100M, 10M, 100M, 10M, participantOrg1, creator.GLHeader1.PK, "FIN");
			var apInvoice1 = creator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", creator.AUD, 1M, 100M, 10M, 100M, 10M, participantOrg1, creator.GLHeader2.PK, "FIN");

			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			Factory.Save();

			var notifications = new NotificationCollection();
			Transmit(notifications, arInvoice1, AccountingConstants.InvoiceAdditionalReference.Posted);
			Transmit(notifications, apInvoice1, AccountingConstants.InvoiceAdditionalReference.Posted);
			AssertContains("Please set up the control account(s) in the registry Accounting", string.Join(System.Environment.NewLine, notifications.Select(s => s.Message)));
			Factory.Save();
		}

		void AssertTransmitForAllStatuses(InvoicingBase invoice, bool expected, string message)
		{
			var notifications = new NotificationCollection();
			AssertEquals(message, expected, Transmit(notifications, invoice, AccountingConstants.InvoiceAdditionalReference.Posted));
			AssertEquals(message, expected, Transmit(notifications, invoice, AccountingConstants.InvoiceAdditionalReference.FullyMatched));
			AssertEquals(message, expected, Transmit(notifications, invoice, AccountingConstants.InvoiceAdditionalReference.UndoFullyMatched));
			Factory.Save();
		}

		bool Transmit(INotifications notifications, InvoicingBase invoice, string status)
		{
			return UniversalTransactionTransmitter.UniversalTransmitForNettingSystem(notifications, Factory, invoice, status);
		}

		TestObjectCreator creator;
		OrgHeader participantOrg1;
		OrgHeader participantOrg2;
		OrgHeader participantOrg3;

		protected override void SetUp()
		{
			base.SetUp();

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			creator = new TestObjectCreator(Factory);

			participantOrg1 = creator.CreateOrgHeader("TSTPCNT1", true, true);
			participantOrg1.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "P1");
			creator.AddEdiCommunication(participantOrg1, EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction
				, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "P2");

			participantOrg2 = creator.CreateOrgHeader("TSTPCNT2", true, true);
			creator.AddEdiCommunication(participantOrg2, EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction
				, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "NS12235");

			participantOrg3 = creator.CreateOrgHeader("TSTPCNT3", true, true);
			creator.AddEdiCommunication(participantOrg3, EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction
				, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "P1");
			creator.AddEdiCommunication(participantOrg3, EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction
				, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, "P2");

			participantOrg3.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "P2");

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, creator.CreateARSuspenseControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, creator.CreateAPSuspenseControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, creator.CreateJobRevenueJournalControlAccount().PK.ToGuid());

			Factory.Save();
		}
	}
}
