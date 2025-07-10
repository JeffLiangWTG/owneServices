using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(UnapprovedCreditNoteEmailServiceTask))]
	class UnapprovedCreditNoteEmailServiceTaskTest : ServiceTaskTestCase<UnapprovedCreditNoteEmailServiceTask>
	{
		public void TestEmailHasCorrectRecipent()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 1, 0, numTypeRCNAndRIRWithNonREQStatus: 0);
			var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, 0m, 0m, hasNotify: true);
			var company1NotifyGroup = CreateNotificationGroupAndEmails("company1user", 1);
			var company2NotifyGroup = CreateNotificationGroupAndEmails("company2user", 1);

			AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1NotifyGroup.Item1.ToGuid());

			var serviceTask = new UnapprovedCreditNoteEmailServiceTask();
			InitialiseAndRunTaskSchedule(serviceTask);

			AssertEquals("Pre-requisite", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated.First();
			AssertEmail(email, company1ExpectedEmailBody, company1NotifyGroup.Item2);
			AssertNotContains(company2NotifyGroup.Item2.First(), email.Recipients.RecipientsAsDelimitedString("; "));

			using (Env.Instance.TemporaryServiceTaskContext(UnapprovedCreditNoteEmailServiceTask.Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}
		}

		public void TestEmailCreationWhenNotificationGroupNotSet_NoAuthentication_RCN()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 1, 0, numTypeRCNAndRIRWithNonREQStatus: 0);
			var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, 0m, 0m, hasNotify: false);

			Assert(AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.GetFallBackValueAtAllLevels(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty).AuthorisationSettings.IsNullOrEmpty());
			Assert(AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.GetValueWithoutFallback(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty).Equals(Guid.Empty));
			var log = CreateAndRunServiceTaskWithLog();
			AssertEquals("Email should not be created because Notification Group is not set", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestEmailCreationWhenNotificationGroupNotSet_WithOppositeAuthentication_RIR()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 0, 1, numTypeRCNAndRIRWithNonREQStatus: 0);
			var company1AuthThreshold = 80m;
			var company1RCNAuthCollection = SetupAuthCollection(company1AuthThreshold);
			var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, 0m, 0m, hasNotify: false);

			Assert(AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.GetFallBackValueAtAllLevels(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty).AuthorisationSettings.IsNullOrEmpty());
			Assert(AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.GetValueWithoutFallback(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty).Equals(Guid.Empty));
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RCNAuthCollection))
			{
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("Email should not be created because Notification Group is not set", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestEmailCreationWhenNotificationGroupNotSet_WithOppositeAuthentication_RCN()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 1, 0, numTypeRCNAndRIRWithNonREQStatus: 0);
			var company1AuthThreshold = 80m;
			var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, company1AuthThreshold, 0m, hasNotify: false);
			var company1RCNAuthCollection = SetupAuthCollection(company1AuthThreshold);

			Assert(AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.GetValueWithoutFallback(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty).Equals(Guid.Empty));
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RCNAuthCollection))
			{
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("Email should not be created because Notification Group is not set", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestEmailCreationForOneCompany_WithNoAuthentication_RCN()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 1, 0, numTypeRCNAndRIRWithNonREQStatus: 0);
			var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, 0m, 0m, hasNotify: true);
			var company1NotifyGroup = CreateNotificationGroupAndEmails("company1user", 4);

			Assert(AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.GetFallBackValueAtAllLevels(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty).AuthorisationSettings.IsNullOrEmpty());
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1NotifyGroup.Item1.ToGuid()))
			{
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("Email be created despite Authorisation not being set because there are unactioned requests, but authorisation information should have empty strings", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company1NotifyGroup.Item2[0])), company1ExpectedEmailBody, company1NotifyGroup.Item2);
				AssertLog(log.ToString());
			}
		}

		public void TestEmailCreationForOneCompany_WithNoAuthentication_RIR()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 0, 1, numTypeRCNAndRIRWithNonREQStatus: 0);
			var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, 0m, 0m, hasNotify: true);
			var company1NotifyGroup = CreateNotificationGroupAndEmails("company1user", 4);

			Assert(AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.GetFallBackValueAtAllLevels(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty).AuthorisationSettings.IsNullOrEmpty());
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1NotifyGroup.Item1.ToGuid()))
			{
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("Email be created despite Authorisation not being set because there are unactioned requests, but authorisation information should have empty strings", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company1NotifyGroup.Item2[0])), company1ExpectedEmailBody, company1NotifyGroup.Item2);
				AssertLog(log.ToString());
			}
		}

		public void TestEmailCreationForOneCompany_WithAuthentication_RCN()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 1, 0, numTypeRCNAndRIRWithNonREQStatus: 0);
			var company1AuthThreshold = 80m;
			var company1RCNAuthCollection = SetupAuthCollection(company1AuthThreshold);
			var company1NotifyGroup = CreateNotificationGroupAndEmails("company1user", 4);

			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RCNAuthCollection))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1NotifyGroup.Item1.ToGuid()))
			{
				var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, company1AuthThreshold, 0m, hasNotify: true);
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("There should be one outstanding request (RCN)", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company1NotifyGroup.Item2[0])), company1ExpectedEmailBody, company1NotifyGroup.Item2);
			}
		}

		public void TestEmailCreationForOneCompany_WithAuthentication_RIR()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 0, 1, numTypeRCNAndRIRWithNonREQStatus: 0);
			var company1Auth2Threshold = 80m;
			var company1RIRAuthCollection = SetupAuthCollection(company1Auth2Threshold);
			var company1NotifyGroup = CreateNotificationGroupAndEmails("company1user", 4);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RIRAuthCollection))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1NotifyGroup.Item1.ToGuid()))
			{
				var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, 0m, company1Auth2Threshold, hasNotify: true);
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("There should be one outstanding request (RIR)", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company1NotifyGroup.Item2[0])), company1ExpectedEmailBody, company1NotifyGroup.Item2);
				AssertLog(log.ToString());
			}
		}

		public void TestEmailCreationForOneCompany_WithAuthentication_RCNAndRIR()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 2, 3, numTypeRCNAndRIRWithNonREQStatus: 10);
			var company1AuthThreshold = 80m;
			var company1Auth2Threshold = 100m;
			var company1RCNAuthCollection = SetupAuthCollection(company1AuthThreshold);
			var company1RIRAuthCollection = SetupAuthCollection(company1Auth2Threshold);
			var company1NotifyGroup = CreateNotificationGroupAndEmails("company1user", 4);

			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RCNAuthCollection))
			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RIRAuthCollection))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1NotifyGroup.Item1.ToGuid()))
			{
				var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, company1AuthThreshold, company1Auth2Threshold, hasNotify: true);
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("There should be multiple outstanding requests to one mailgroup", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company1NotifyGroup.Item2[0])), company1ExpectedEmailBody, company1NotifyGroup.Item2);
				AssertLog(log.ToString());
			}
		}

		public void TestEmailCreationForOneCompany_WithOneAuthentication_RCN()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 2, 3, numTypeRCNAndRIRWithNonREQStatus: 10);
			var company1AuthThreshold = 80m;
			var company1RCNAuthCollection = SetupAuthCollection(company1AuthThreshold);
			var company1NotifyGroup = CreateNotificationGroupAndEmails("company1user", 4);

			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RCNAuthCollection))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1NotifyGroup.Item1.ToGuid()))
			{
				var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, company1AuthThreshold, 0m, hasNotify: true);
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("There should be multiple outstanding requests to one mailgroup", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company1NotifyGroup.Item2[0])), company1ExpectedEmailBody, company1NotifyGroup.Item2);
				AssertLog(log.ToString());
			}
		}

		public void TestEmailCreationForOneCompany_WithOneAuthentication_RIR()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 2, 3, numTypeRCNAndRIRWithNonREQStatus: 10);
			var company1Auth2Threshold = 100m;
			var company1RIRAuthCollection = SetupAuthCollection(company1Auth2Threshold);
			var company1NotifyGroup = CreateNotificationGroupAndEmails("company1user", 4);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RIRAuthCollection))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1NotifyGroup.Item1.ToGuid()))
			{
				var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, 0m, company1Auth2Threshold, hasNotify: true);
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("There should be multiple outstanding requests to one mailgroup", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company1NotifyGroup.Item2[0])), company1ExpectedEmailBody, company1NotifyGroup.Item2);
				AssertLog(log.ToString());
			}
		}

		public void TestEmailCreationForTwoCompanies_WithNotificationNotSetForSecondCompany_WithNoAuthenticationForSecondCompany()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 2, 3, numTypeRCNAndRIRWithNonREQStatus: 10);
			var company1AuthThreshold = 80m;
			var company1Auth2Threshold = 100m;
			var company1RCNAuthCollection = SetupAuthCollection(company1AuthThreshold);
			var company1RIRAuthCollection = SetupAuthCollection(company1Auth2Threshold);
			var company1NotifyGroup = CreateNotificationGroupAndEmails("company1user", 4);

			var company2TestData = CreateOneCompanyAndCreditNoteRequestData("CT2", 6, 4, numTypeRCNAndRIRWithNonREQStatus: 15);

			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RCNAuthCollection))
			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RIRAuthCollection))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1NotifyGroup.Item1.ToGuid()))
			{
				var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, company1AuthThreshold, company1Auth2Threshold, hasNotify: true);
				var company2ExpectedEmailBody = CreateExpectedEmailBody(company2TestData.Item1.GC_Code, company2TestData.Item2, 0m, 0m, hasNotify: false);
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("There should be multiple outstanding requests to one mailgroup only", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company1NotifyGroup.Item2[0])), company1ExpectedEmailBody, company1NotifyGroup.Item2);
				AssertLog(log.ToString());
			}
		}

		public void TestEmailCreationForTwoCompanies_WithNotificationNotSetForSecondCompany()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 2, 3, numTypeRCNAndRIRWithNonREQStatus: 10);
			var company1AuthThreshold = 80m;
			var company1Auth2Threshold = 100m;
			var company1RCNAuthCollection = SetupAuthCollection(company1AuthThreshold);
			var company1RIRAuthCollection = SetupAuthCollection(company1Auth2Threshold);
			var company1NotifyGroup = CreateNotificationGroupAndEmails("company1user", 4);

			var company2TestData = CreateOneCompanyAndCreditNoteRequestData("CT2", 6, 4, numTypeRCNAndRIRWithNonREQStatus: 15);
			var company2AuthThreshold = 80m;
			var company2Auth2Threshold = 100m;
			var company2RCNAuthCollection = SetupAuthCollection(company2AuthThreshold);
			var company2RIRAuthCollection = SetupAuthCollection(company2Auth2Threshold);

			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RCNAuthCollection))
			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RIRAuthCollection))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(company2TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company2RCNAuthCollection))
			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(company2TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company2RIRAuthCollection))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1NotifyGroup.Item1.ToGuid()))
			{
				var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, company1AuthThreshold, company1Auth2Threshold, hasNotify: true);
				var company2ExpectedEmailBody = CreateExpectedEmailBody(company2TestData.Item1.GC_Code, company2TestData.Item2, company2AuthThreshold, company2Auth2Threshold, hasNotify: false);
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("There should be multiple outstanding requests to one mailgroup only", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company1NotifyGroup.Item2[0])), company1ExpectedEmailBody, company1NotifyGroup.Item2);
				AssertLog(log.ToString());
			}
		}

		public void TestEmailCreationForTwoCompanies_WithNoAuthenticationForSecondCompany()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 2, 3, numTypeRCNAndRIRWithNonREQStatus: 10);
			var company1AuthThreshold = 80m;
			var company1Auth2Threshold = 100m;
			var company1RCNAuthCollection = SetupAuthCollection(company1AuthThreshold);
			var company1RIRAuthCollection = SetupAuthCollection(company1Auth2Threshold);
			var company1NotifyGroup = CreateNotificationGroupAndEmails("company1user", 4);

			var company2TestData = CreateOneCompanyAndCreditNoteRequestData("CT2", 6, 4, numTypeRCNAndRIRWithNonREQStatus: 15);
			var company2NotifyGroup = CreateNotificationGroupAndEmails("company2user", 6);

			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RCNAuthCollection))
			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RIRAuthCollection))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1NotifyGroup.Item1.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company2TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company2NotifyGroup.Item1.ToGuid()))
			{
				var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, company1AuthThreshold, company1Auth2Threshold, hasNotify: true);
				var company2ExpectedEmailBody = CreateExpectedEmailBody(company2TestData.Item1.GC_Code, company2TestData.Item2, 0m, 0m, hasNotify: true);
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("There should be multiple outstanding requests to two mailgroups despite authentiation not being set", 2, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company1NotifyGroup.Item2[0])), company1ExpectedEmailBody, company1NotifyGroup.Item2);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company2NotifyGroup.Item2[0])), company2ExpectedEmailBody, company2NotifyGroup.Item2);
				AssertLog(log.ToString());
			}
		}

		public void TestEmailCreationForTwoCompanies_WithOneAuthenticationForSecondCompany_RCN()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 2, 3, numTypeRCNAndRIRWithNonREQStatus: 10);
			var company1AuthThreshold = 80m;
			var company1Auth2Threshold = 100m;
			var company1RCNAuthCollection = SetupAuthCollection(company1AuthThreshold);
			var company1RIRAuthCollection = SetupAuthCollection(company1Auth2Threshold);
			var company1NotifyGroup = CreateNotificationGroupAndEmails("company1user", 4);

			var company2TestData = CreateOneCompanyAndCreditNoteRequestData("CT2", 6, 4, numTypeRCNAndRIRWithNonREQStatus: 15);
			var company2AuthThreshold = 80m;
			var company2RCNAuthCollection = SetupAuthCollection(company2AuthThreshold);
			var company2NotifyGroup = CreateNotificationGroupAndEmails("company2user", 6);

			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RCNAuthCollection))
			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RIRAuthCollection))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(company2TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company2RCNAuthCollection))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1NotifyGroup.Item1.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company2TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company2NotifyGroup.Item1.ToGuid()))
			{
				var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, company1AuthThreshold, company1Auth2Threshold, hasNotify: true);
				var company2ExpectedEmailBody = CreateExpectedEmailBody(company2TestData.Item1.GC_Code, company2TestData.Item2, 0m, company2AuthThreshold, hasNotify: true);
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("There should be multiple outstanding requests to two mailgroups", 2, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company1NotifyGroup.Item2[0])), company1ExpectedEmailBody, company1NotifyGroup.Item2);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company2NotifyGroup.Item2[0])), company2ExpectedEmailBody, company2NotifyGroup.Item2);
				AssertLog(log.ToString());
			}
		}

		public void TestEmailCreationForTwoCompanies_WithOneAuthenticationForSecondCompany_RIR()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 2, 3, numTypeRCNAndRIRWithNonREQStatus: 10);
			var company1AuthThreshold = 80m;
			var company1Auth2Threshold = 100m;
			var company1RCNAuthCollection = SetupAuthCollection(company1AuthThreshold);
			var company1RIRAuthCollection = SetupAuthCollection(company1Auth2Threshold);
			var company1NotifyGroup = CreateNotificationGroupAndEmails("company1user", 4);

			var company2TestData = CreateOneCompanyAndCreditNoteRequestData("CT2", 6, 4, numTypeRCNAndRIRWithNonREQStatus: 15);
			var company2Auth2Threshold = 100m;
			var company2RIRAuthCollection = SetupAuthCollection(company2Auth2Threshold);
			var company2NotifyGroup = CreateNotificationGroupAndEmails("company2user", 6);

			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RCNAuthCollection))
			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RIRAuthCollection))
			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(company2TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company2RIRAuthCollection))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1NotifyGroup.Item1.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company2TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company2NotifyGroup.Item1.ToGuid()))
			{
				var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, company1AuthThreshold, company1Auth2Threshold, hasNotify: true);
				var company2ExpectedEmailBody = CreateExpectedEmailBody(company2TestData.Item1.GC_Code, company2TestData.Item2, 0m, company2Auth2Threshold, hasNotify: true);
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("There should be multiple outstanding requests to two mailgroups", 2, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company1NotifyGroup.Item2[0])), company1ExpectedEmailBody, company1NotifyGroup.Item2);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company2NotifyGroup.Item2[0])), company2ExpectedEmailBody, company2NotifyGroup.Item2);
				AssertLog(log.ToString());
			}
		}

		public void TestEmailCreationForTwoCompanies()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 2, 3, numTypeRCNAndRIRWithNonREQStatus: 10);
			var company1AuthThreshold = 80m;
			var company1Auth2Threshold = 100m;
			var company1RCNAuthCollection = SetupAuthCollection(company1AuthThreshold);
			var company1RIRAuthCollection = SetupAuthCollection(company1Auth2Threshold);
			var company1NotifyGroup = CreateNotificationGroupAndEmails("company1user", 4);

			var company2TestData = CreateOneCompanyAndCreditNoteRequestData("CT2", 6, 4, numTypeRCNAndRIRWithNonREQStatus: 15);
			var company2AuthThreshold = 80m;
			var company2Auth2Threshold = 100m;
			var company2RCNAuthCollection = SetupAuthCollection(company2AuthThreshold);
			var company2RIRAuthCollection = SetupAuthCollection(company2Auth2Threshold);
			var company2NotifyGroup = CreateNotificationGroupAndEmails("company2user", 6);

			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RCNAuthCollection))
			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RIRAuthCollection))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(company2TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company2RCNAuthCollection))
			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(company2TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company2RIRAuthCollection))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1NotifyGroup.Item1.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company2TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company2NotifyGroup.Item1.ToGuid()))
			{
				var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, company1TestData.Item2, company1AuthThreshold, company1Auth2Threshold, hasNotify: true);
				var company2ExpectedEmailBody = CreateExpectedEmailBody(company2TestData.Item1.GC_Code, company2TestData.Item2, company2AuthThreshold, company2Auth2Threshold, hasNotify: true);
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("There should be multiple outstanding requests to two mailgroups!", 2, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company1NotifyGroup.Item2[0])), company1ExpectedEmailBody, company1NotifyGroup.Item2);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company2NotifyGroup.Item2[0])), company2ExpectedEmailBody, company2NotifyGroup.Item2);
				AssertLog(log.ToString());
			}
		}

		public void TestExceptionAndErrorReporterHandling()
		{
			var serviceTask = new UnapprovedCreditNoteEmailServiceTaskWithException(new NotImplementedException("This is an not implemented exception created for testing"));
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			var log = logger.ToString();

			AssertContains("Information|Credit Note Approvals Email Notification service task started.", log);
			AssertContains("Error|Credit Note Approvals Email Notification service task ended abruptly.", log);
			AssertContains("Exception: System.NotImplementedException", log);
			AssertContains("Exception Message: This is an not implemented exception created for testing.", log);

			Assert("The UCN service task ErrorReporter should not include 'UCN Service Task Invalid Behavior Error'", !ErrorReporter.LastMessageReported.Contains("UCN Service Task Invalid Behavior Error"));
			ErrorReporter.Clear();

			serviceTask = new UnapprovedCreditNoteEmailServiceTaskWithException(new InvalidOperationException("This is an invalid opreation exception created for testing"));
			logger = InitialiseAndRunTaskSchedule(serviceTask);
			log = logger.ToString();

			AssertContains("Information|Credit Note Approvals Email Notification service task started.", log);
			AssertContains("Error|Credit Note Approvals Email Notification service task ended abruptly.", log);
			AssertContains("Exception: System.InvalidOperationException", log);
			AssertContains("Exception Message: This is an invalid opreation exception created for testing.", log);

			Assert("The UCN service task ErrorReporter should include 'UCN Service Task Invalid Behavior Error'", ErrorReporter.LastMessageReported.Contains("UCN Service Task Invalid Behavior Error"));
			ErrorReporter.Clear();

			serviceTask = new UnapprovedCreditNoteEmailServiceTaskWithException(new NullReferenceException("This is an null reference exception created for testing"));
			logger = InitialiseAndRunTaskSchedule(serviceTask);
			log = logger.ToString();

			AssertContains("Information|Credit Note Approvals Email Notification service task started.", log);
			AssertContains("Error|Credit Note Approvals Email Notification service task ended abruptly.", log);
			AssertContains("Exception: System.NullReferenceException", log);
			AssertContains("Exception Message: This is an null reference exception created for testing.", log);

			Assert("The UCN service task ErrorReporter should include 'UCN Service Task Invalid Behavior Error'", ErrorReporter.LastMessageReported.Contains("UCN Service Task Invalid Behavior Error"));
			ErrorReporter.Clear();
		}

		public void TestApprovalRequestsFilter()
		{
			var companyTestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 3, 2, numTypeRCNAndRIRWithNonREQStatus: 0);
			var companyNotifyGroup = CreateNotificationGroupAndEmails("company1user", 4);
			var preRequestsBos = Factory.Load<GenApprovalRequest>(new ZQuery());
			AssertEquals("Pre:the count of CompanyTestData should be 5", 5, companyTestData.Item2.Count);
			AssertEquals("Pre:the count of CompanyTestData should be 5", 5, preRequestsBos.Length);

			var dirtyDataRequest = Factory.New<ARCreditNoteApprovalRequest>();
			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			dirtyDataRequest.Initialize(new[] { creditNote }, ZGuid.NewZGuid(), JobHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			dirtyDataRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			Factory.Save();

			var approvalRequestsAfterAdded = Factory.Load<GenApprovalRequest>(new ZQuery());

			Env.OutgoingMailManager.EmailsCreated.Clear();

			Assert(AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.GetFallBackValueAtAllLevels(companyTestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty).AuthorisationSettings.IsNullOrEmpty());
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(companyTestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, companyNotifyGroup.Item1.ToGuid()))
			{
				var companyExpectedEmailBody = CreateExpectedEmailBody(companyTestData.Item1.GC_Code, companyTestData.Item2, 0m, 0m, hasNotify: true);
				var serviceTask = new UnapprovedCreditNoteEmailServiceTaskWithProcessorCache_ForTestOnly();
				var log = InitialiseAndRunTaskSchedule(serviceTask);

				var xP_PKQuery = serviceTask.processor.itemPKs_ForTestOnly;

				AssertEquals("The count of approvalRequest should be 6", 6, approvalRequestsAfterAdded.Length);
				Assert("The dirty data insert failed", approvalRequestsAfterAdded.Select(x => new ZGuid(x["XP_PK"])).Contains(dirtyDataRequest.PK));
				AssertEquals("The query should filter the dirty data and the count of query result should be 5", 5, xP_PKQuery.Count());
				companyTestData.Item2.Select(y => y.PK).ForEach(x => Assert("The query result should contain the PK", xP_PKQuery.Contains(x)));
				Assert("The query result should not contain the dirty data", !xP_PKQuery.Contains(dirtyDataRequest.PK));

				AssertEquals("Email be created despite Authorisation not being set because there are unactioned requests, but authorisation information should have empty strings", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(companyNotifyGroup.Item2[0])), companyExpectedEmailBody, companyNotifyGroup.Item2);
				AssertLog(log.ToString());
			}
		}

		public void TestEmailCreationForCreditNoteRequestWithJobDeleted()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var company1TestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 0, 0, 0);
			var request = CreateRCNRequestWithJobDeleted(company1TestData.Item1);
			var company1RCNAuthCollection = SetupAuthCollection(80m);
			var company1NotifyGroup = CreateNotificationGroupAndEmails("company1user", 1);

			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RCNAuthCollection))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(company1TestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1NotifyGroup.Item1.ToGuid()))
			{
				var company1ExpectedEmailBody = CreateExpectedEmailBody(company1TestData.Item1.GC_Code, new List<ARCreditNoteApprovalRequest>() { request }, 0m, 0m, true);
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("There should be multiple outstanding requests to one mailgroup", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmail(Env.OutgoingMailManager.EmailsCreated.First(x => x.Recipients.Contains(company1NotifyGroup.Item2[0])), company1ExpectedEmailBody, company1NotifyGroup.Item2);
				AssertLog(log.ToString());
			}
		}

		public void TestEmailApproversColumn_MultiRCN_SingleRIR() => AssertEmailApproversColumn(ApprovalCredentialOption.DoubleLogin, ApprovalCredentialOption.SingleLogin);
		public void TestEmailApproversColumn_MultiRCN_MultiRIR() => AssertEmailApproversColumn(ApprovalCredentialOption.SequentialLogin, ApprovalCredentialOption.DoubleLogin);
		public void TestEmailApproversColumn_SingleRCN_MultiRIR() => AssertEmailApproversColumn(ApprovalCredentialOption.SingleLogin, ApprovalCredentialOption.SequentialLogin);
		public void TestEmailApproversColumn_SingleRCN_SingleRIR() => AssertEmailApproversColumn(ApprovalCredentialOption.SingleLogin, ApprovalCredentialOption.SingleLogin);

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void AssertEmailApproversColumn(string rcnApprovalOption, string rirApprovalOption)
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var user1 = TestObjectCreator.CreateStaff("US1");
			user1.GS_FullName = "Test user 1";
			var user2 = TestObjectCreator.CreateStaff("US2");
			user2.GS_FullName = "Test user 2";

			var companyTestData = CreateOneCompanyAndCreditNoteRequestData("CT1", 1, 1, numTypeRCNAndRIRWithNonREQStatus: 0);
			var rcnRequest = companyTestData.Item2.First(x => x.XP_ApprovalType == Constants.GenApprovalRequestApprovalType.ARCreditNote);
			rcnRequest.XP_GS_NKApprovingUser1 = user1.GS_Code;
			rcnRequest.XP_GS_NKApprovingUser2 = user2.GS_Code;
			rcnRequest.PostingDetails.ApprovingOption = rcnApprovalOption;
			var rirRequest = companyTestData.Item2.First(x => x.XP_ApprovalType == Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal);
			rirRequest.XP_GS_NKApprovingUser1 = user2.GS_Code;
			rirRequest.XP_GS_NKApprovingUser2 = user1.GS_Code;
			rirRequest.PostingDetails.ApprovingOption = rirApprovalOption;

			Factory.Save();

			var companyAuthThreshold = 80m;
			var rcnHasApprovers = rcnApprovalOption != ApprovalCredentialOption.SingleLogin;
			var rirHasApprovers = rirApprovalOption != ApprovalCredentialOption.SingleLogin;

			var companyRCNAuthCollection = SetupAuthCollection(companyAuthThreshold);
			var companyRIRAuthCollection = SetupAuthCollection(companyAuthThreshold);

			var companyNotifyGroup = CreateNotificationGroupAndEmails("company1user", 4);

			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(companyTestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, companyRCNAuthCollection))
			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(companyTestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, companyRIRAuthCollection))
			using (AccountingConfigurationRegistry.Instance.CreditNoteApprovalsNotifyGroup.SetTemporaryValue(companyTestData.Item1.PK.ToGuid(), Guid.Empty, Guid.Empty, companyNotifyGroup.Item1.ToGuid()))
			{
				var companyExpectedEmailBody = CreateExpectedEmailBody(companyTestData.Item1.GC_Code, companyTestData.Item2, companyAuthThreshold, companyAuthThreshold, true, rcnHasApprovers, rirHasApprovers);
				var log = CreateAndRunServiceTaskWithLog();
				AssertEquals("There should be one outstanding request", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated.Single();
				AssertEmail(email, companyExpectedEmailBody, companyNotifyGroup.Item2);

				var emailBodySplitByTable = email.Body.Split(new[] { "<table>" }, StringSplitOptions.None);
				AssertEquals("Precondition: 2 tables and pre-table text", 3, emailBodySplitByTable.Length);

				if (!rcnHasApprovers)
				{
					AssertNotContains("Reviewed by:", emailBodySplitByTable[1]);
					AssertNotContains(GetRequestApprovers(rcnRequest), emailBodySplitByTable[1]);
				}
				if (!rirHasApprovers)
				{
					AssertNotContains("Reviewed by:", emailBodySplitByTable[2]);
					AssertNotContains(GetRequestApprovers(rirRequest), emailBodySplitByTable[2]);
				}
			}
		}

		class UnapprovedCreditNoteEmailServiceTaskWithProcessorCache_ForTestOnly : UnapprovedCreditNoteEmailServiceTask
		{
			public UnapprovedCreditNoteEmailNotificationProcessor processor;
			protected override void RunTaskCore()
			{
				processor = new UnapprovedCreditNoteEmailNotificationProcessor(ServiceLogger);
				processor.SendEmail();
			}
		}

		class UnapprovedCreditNoteEmailServiceTaskWithException : UnapprovedCreditNoteEmailServiceTask
		{
			readonly Exception exception;
			public UnapprovedCreditNoteEmailServiceTaskWithException(Exception ex)
			{
				exception = ex;
			}
			protected override void RunTaskCore()
			{
				throw exception;
			}
		}

		#region Assertion Helpers

		void AssertEmail(EmailDef email, List<ZString> expectedBodyParts, List<ZString> expectedRecipients)
		{
			AssertEquals(EmailContentTypes.HTML, email.ContentType);
			AssertEquals("Default@edi.com.au", email.FromAddress);
			AssertEquals("Credit Note Approvals Notification Email", email.FromDisplayName);
			AssertEquals("Unactioned Credit Note Approval Requests", email.Subject);
			expectedBodyParts.ForEach(x => AssertContains(x, email.Body));
			AssertEquals(expectedRecipients.Count, email.Recipients.Count);
			expectedRecipients.ForEach(x => AssertContains(x, email.Recipients.RecipientsAsDelimitedString("; ")));
		}

		void AssertLog(string log)
		{
			Assert(log.Contains("Information|Credit Note Approvals Email Notification service task started."));
			Assert(log.Contains("Information|Credit Note Approvals Email Notification service task completed."));
		}

		#endregion

		#region Setup

		TestServiceLogger CreateAndRunServiceTaskWithLog()
		{
			var serviceTask = new UnapprovedCreditNoteEmailServiceTask();
			return InitialiseAndRunTaskSchedule(serviceTask);
		}

		AuthorizationModeAndSettings SetupAuthCollection(ZDecimal authorisationThreshold)
		{
			var setting = new AuthorizationModeAndSettings();
			var resultCollection = setting.AuthorisationSettings;

			var upToThreshold = resultCollection.AddNew();
			upToThreshold.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToThreshold.Amount = authorisationThreshold;
			upToThreshold.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;

			var aboveThreshold = resultCollection.AddNew();
			aboveThreshold.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			aboveThreshold.Amount = authorisationThreshold;
			aboveThreshold.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

			return setting;
		}

		Tuple<ZGuid, List<ZString>> CreateNotificationGroupAndEmails(ZString emailNamePrefix, ZInt numberOfUsers)
		{
			var group = Factory.New<GlbGroup>();
			var emailList = new List<ZString>();
			group.GG_Code = TestObjectCreator.GetRandomString(3);
			for (var i = 1; i <= numberOfUsers; i++)
			{
				var newUser = Factory.New<GlbStaff>();
				newUser.GS_LoginName = string.Concat(emailNamePrefix, i);
				newUser.GS_EmailAddress = string.Concat(newUser.GS_LoginName, "@abc.com");
				group.Staff.Add(newUser);
				emailList.Add(newUser.GS_EmailAddress);
			}
			Factory.Save();
			return new Tuple<ZGuid, List<ZString>>(group.PK, emailList);
		}

		Tuple<GlbCompany, List<ARCreditNoteApprovalRequest>> CreateOneCompanyAndCreditNoteRequestData(string companyCode, int numTypeRCNWithREQStatus, int numTypeRIRWithREQStatus,
			int numTypeRCNAndRIRWithNonREQStatus)
		{
			var requests = new List<ARCreditNoteApprovalRequest>();
			var statusTypes = new List<ZString>()
			{
				Constants.GenApprovalRequestApprovalStatus.Requested, Constants.GenApprovalRequestApprovalStatus.Approved, Constants.GenApprovalRequestApprovalStatus.Cancelled,
				Constants.GenApprovalRequestApprovalStatus.Rejected, Constants.GenApprovalRequestApprovalStatus.Posted
			};

			var objectCreator = new TestObjectCreator(Factory);
			var company = objectCreator.CreateNewCompany(companyCode);
			company.GC_Name = companyCode + " Company";
			var branch = objectCreator.CreateBranch(companyCode, company);

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				if (numTypeRCNWithREQStatus > 0)
				{
					requests = CreateRequests(numTypeRCNWithREQStatus, requests, objectCreator, company, statusTypes: new List<ZString>() { Constants.GenApprovalRequestApprovalStatus.Requested }, approvalTypes:
						new List<ZString>() { Constants.GenApprovalRequestApprovalType.ARCreditNote });
				}

				if (numTypeRIRWithREQStatus > 0)
				{
					requests = CreateRequests(numTypeRIRWithREQStatus, requests, objectCreator, company, statusTypes: new List<ZString>() { Constants.GenApprovalRequestApprovalStatus.Requested }, approvalTypes:
						new List<ZString>() { Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal });
				}

				if (numTypeRCNAndRIRWithNonREQStatus > 0)
				{
					requests = CreateRequests(numTypeRCNAndRIRWithNonREQStatus, requests, objectCreator, company, statusTypes, approvalTypes: new List<ZString>() { Constants.GenApprovalRequestApprovalType.ARCreditNote,
						Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal });
				}
			}

			Factory.Save();
			return new Tuple<GlbCompany, List<ARCreditNoteApprovalRequest>>(company, requests);
		}

		ARCreditNoteApprovalRequest CreateRCNRequestWithJobDeleted(GlbCompany glbCompany)
		{
			var approvalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();
			approvalRequest.Initialize(new[] { creditNote }, ZGuid.NewZGuid(), JobHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			approvalRequest.XP_GB_RequestingBranch = glbCompany.Branches[0].PK;
			approvalRequest.XP_GB_JobBranch = glbCompany.Branches[0].PK;
			approvalRequest.XP_GE_JobDepartment = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			return approvalRequest;
		}

		List<ARCreditNoteApprovalRequest> CreateRequests(int numberOfRequests, List<ARCreditNoteApprovalRequest> requestsList, TestObjectCreator objectCreator, GlbCompany company,
			List<ZString> statusTypes, List<ZString> approvalTypes, int initial = 70, int step = 20)
		{
			var i = 0;
			while (i < numberOfRequests)
			{
				var approvalType = approvalTypes[(i % approvalTypes.Count) == 0 ? approvalTypes.Count - 1 : (i % approvalTypes.Count) - 1];
				var statusType = statusTypes[(i % statusTypes.Count) == 0 ? statusTypes.Count - 1 : (i % statusTypes.Count) - 1];
				var amount = new ZDecimal(initial + (step * i));
				if (approvalType == Constants.GenApprovalRequestApprovalType.ARCreditNote)
				{
					var request = objectCreator.CreateARCreditNoteApprovalRequest(amount, statusType);
					Factory.Save();
					requestsList.Add(request);
				}
				else if (approvalType == Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal)
				{
					var request = objectCreator.CreateInvoiceReversalApprovalRequest(amount, statusType);
					Factory.Save();
					requestsList.Add(request);
				}
				i++;
			}
			return requestsList;
		}

		List<ZString> CreateExpectedEmailBody(ZString companyCode, List<ARCreditNoteApprovalRequest> requests, decimal rCNAuthThreshold, decimal rIRAuthThreshold, bool hasNotify, bool hasCRNApproverColumn = false, bool hasRIRApproverColumn = false)
		{
			var emailBody = new List<ZString>();
			if (hasNotify && requests != null && requests.Count > 0)
			{
				var licenseCode = "EDI" + companyCode + "DAT";
				var rCNRequests = new List<ARCreditNoteApprovalRequest>();
				var rIRRequests = new List<ARCreditNoteApprovalRequest>();
				foreach (var request in requests)
				{
					if (request.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested && request.XP_ApprovalType == Constants.GenApprovalRequestApprovalType.ARCreditNote)
					{
						rCNRequests.Add(request);
					}
					else if (request.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Requested && request.XP_ApprovalType == Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal)
					{
						rIRRequests.Add(request);
					}
				}

				emailBody.Add("<p>The following Credit Notes require Authorisation.</p>");
				if (rCNRequests.Count > 0)
				{
					emailBody = CreateExpectedEmailBodyHeader(emailBody, hasCRNApproverColumn);
					if (rCNAuthThreshold != 0m)
					{
						rCNRequests.Where(x => LocalTotalAmount(x, true) <= rCNAuthThreshold).ForEach(x => CreateTableRowForRequest(emailBody, licenseCode, x, true, string.Empty, hasCRNApproverColumn));
						rCNRequests.Where(x => LocalTotalAmount(x, true) > rCNAuthThreshold).ForEach(x => CreateTableRowForRequest(emailBody, licenseCode, x, true, string.Empty, hasCRNApproverColumn));
					}
					else
					{
						rCNRequests.ForEach(x => CreateTableRowForRequest(emailBody, licenseCode, x, true, string.Empty, hasCRNApproverColumn));
					}
					emailBody.Add("<br/>");
				}
				if (rIRRequests.Count > 0)
				{
					emailBody = CreateExpectedEmailBodyHeader(emailBody, hasRIRApproverColumn);
					if (rIRAuthThreshold != 0m)
					{
						rIRRequests.Where(x => LocalTotalAmount(x, false) <= rIRAuthThreshold).ForEach(x => CreateTableRowForRequest(emailBody, licenseCode, x, false, string.Empty, hasRIRApproverColumn));
						rIRRequests.Where(x => LocalTotalAmount(x, false) > rIRAuthThreshold).ForEach(x => CreateTableRowForRequest(emailBody, licenseCode, x, false, string.Empty, hasRIRApproverColumn));
					}
					else
					{
						rIRRequests.ForEach(x => CreateTableRowForRequest(emailBody, licenseCode, x, false, string.Empty, hasRIRApproverColumn));
					}
					emailBody.Add("<br/>");
				}
				emailBody = CreateExpectedEmailBodyFooter(emailBody);
			}
			return emailBody;
		}

		decimal LocalTotalAmount(ARCreditNoteApprovalRequest request, bool isJob)
		{
			var result = 0m;
			var relatedLines = isJob ?
				Factory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_JH, request.XP_ParentID)) :
				Factory.Load<AccTransactionLines>(new ZQuery(AccTransactionLinesSchema.AL_AH, request.XP_ParentID));
			if (relatedLines != null)
			{
				foreach (var relatedLine in relatedLines)
				{
					if (relatedLine.AL_ReverseDate.IsEmpty && relatedLine.AL_LineType == "WIP")
					{
						result = result + relatedLine.AL_LineAmount;
					}
				}
			}
			return result;
		}

		List<ZString> CreateExpectedEmailBodyHeader(List<ZString> emailBody, bool hasApproverColumn)
		{
			emailBody.Add(FormattableString.Invariant($"<th align=\"left\">Approval Request:</th>"));
			emailBody.Add(FormattableString.Invariant($"<th align=\"left\">Branch:</th>"));
			emailBody.Add(FormattableString.Invariant($"<th align=\"left\">Department:</th>"));
			emailBody.Add(FormattableString.Invariant($"<th align=\"left\">Created by:</th>"));
			emailBody.Add(FormattableString.Invariant($"<th align=\"left\">Created On:</th>"));
			emailBody.Add(FormattableString.Invariant($"<th align=\"left\">Approvals Required:</th>"));
			emailBody.Add(FormattableString.Invariant($"<th align=\"left\">Next Approval Required:</th>"));
			if (hasApproverColumn)
			{
				emailBody.Add(FormattableString.Invariant($"<th align=\"left\">Reviewed by:</th>"));
			}
			emailBody.Add(FormattableString.Invariant($"<th align=\"left\">Reason Description:</th>"));
			return emailBody;
		}

		List<ZString> CreateExpectedEmailBodyFooter(List<ZString> emailBody)
		{
			emailBody.Add($"<p>To approve or reject the credit notes, please navigate to <a href=\"{ShowModuleUrlHandler.Instance.Create(ModuleIDs.ARCreditNoteApproval)}\">Manage > Receivables > Credit Note Approval</a>.</p>");
			return emailBody;
		}

		List<ZString> CreateTableRowForRequest(List<ZString> emailBody, ZString companyLicenseCode, ARCreditNoteApprovalRequest request, bool isJob, string reason, bool hasApproverColumn)
		{
			var approvalNumber = isJob ? Factory.Load<JobHeader>(request.XP_ParentID)?.JH_JobNum ?? ZString.Empty : Factory.Load<AccTransactionHeader>(request.XP_ParentID)?.AH_TransactionNum ?? ZString.Empty;
			if (approvalNumber != ZString.Empty)
			{
				approvalNumber = FormattableString.Invariant($" - {approvalNumber}");
			}
			emailBody.Add(FormattableString.Invariant($"<td align=\"left\"><a href=\"edient:Command=ShowViewForm&LicenceCode={companyLicenseCode}&ControllerID={ControllerIDs.ARCreditNoteApproval.ToString()}&BusinessEntityPK={request.PK.ToString()}&VersionNumber={new EnterpriseInformationRetriever().VersionNumber}&Hash="));
			emailBody.Add(FormattableString.Invariant($"\">Credit Note Approval{approvalNumber}</a></td>"));
			emailBody.Add(FormattableString.Invariant($"<td align=\"left\">{Factory.Load<GlbBranch>(request.XP_GB_JobBranch)?.GB_Code ?? ZString.Empty}</td>"));
			emailBody.Add(FormattableString.Invariant($"<td align=\"left\">{Factory.Load<GlbDepartment>(request.XP_GE_JobDepartment)?.GE_Code ?? ZString.Empty}</td>"));
			emailBody.Add(FormattableString.Invariant($"<td align=\"left\">{request.CreatedUser_FullName}</td>"));
			emailBody.Add(FormattableString.Invariant($"<td align=\"left\">{request.XP_SystemCreateTimeUtc.ToShortDateString()}</td>"));
			emailBody.Add(FormattableString.Invariant($"<td align=\"left\">{request.ApprovingOptionForDisplay}</td>"));
			emailBody.Add(FormattableString.Invariant($"<td align=\"left\">{request.NextAuthorisationLevelRequired}</td>"));
			if (hasApproverColumn)
			{
				emailBody.Add(FormattableString.Invariant($"<td align=\"left\">{GetRequestApprovers(request)}</td>"));
			}
			emailBody.Add(FormattableString.Invariant($"<td align=\"left\">{reason}</td>"));
			return emailBody;
		}

		string GetRequestApprovers(ARCreditNoteApprovalRequest request)
		{
			return string.Join(", ", new[] {
				request.ApprovingUser1?.GS_Code,
				request.ApprovingUser2?.GS_Code,
				request.ApprovingUser3?.GS_Code,
				request.ApprovingUser4?.GS_Code,
				request.ApprovingUser5?.GS_Code,
				request.ApprovingUser6?.GS_Code
			}.Where(x => x.HasValue));
		}

		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		IDisposable instanceDetailsDisposable;

		protected override void SetUpCore()
		{
			base.SetUpCore();
			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
		}

		protected override void TearDownCore()
		{
			instanceDetailsDisposable?.Dispose();
			base.TearDownCore();
		}
	}
}
