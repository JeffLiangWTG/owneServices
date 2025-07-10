using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Security.ServiceTasks.Testing
{
	[TestedType(typeof(SendPasswordInstructionsServiceTask))]
	class SendPasswordInstructionsServiceTaskTest : ServiceTaskTestCase<SendPasswordInstructionsServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestShouldOnlySendToWebAccessEnabledContacts()
		{
			WebDataRegistry.Instance.ContactCreatedStartFromDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 4, 30));
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://myaccount.test.com");

			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "aaa";
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_Email = "aaa@test.com";
			contact1.OC_SystemCreateTimeUtc = new ZDateTime(2023, 5, 1);

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "bbb";
			contact2.OC_IsActive = true;
			contact2.OC_WebAccessEnabled = false;
			contact2.OC_SystemCreateTimeUtc = new ZDateTime(2023, 5, 1);
			contact2.OC_Email = "bbb@test.com";

			Factory.Save();

			var process = new SendPasswordInstructionsServiceTask();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;

			logger.ClearLog();
			process.RunTask();

			var passwordLog1 = contact1.GetPasswordChangedOrSentLog();
			var passwordLog2 = contact2.GetPasswordChangedOrSentLog();
			AssertNotNull(passwordLog1);
			AssertNull(passwordLog2);
		}

		public void TestShouldOnlySendToActiveContacts()
		{
			WebDataRegistry.Instance.ContactCreatedStartFromDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 4, 30));
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://myaccount.test.com");

			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "aaa";
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_Email = "aaa@test.com";
			contact1.OC_SystemCreateTimeUtc = new ZDateTime(2023, 5, 1);

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "bbb";
			contact2.OC_IsActive = false;
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_SystemCreateTimeUtc = new ZDateTime(2023, 5, 1);
			contact2.OC_Email = "bbb@test.com";

			Factory.Save();

			var process = new SendPasswordInstructionsServiceTask();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;

			logger.ClearLog();
			process.RunTask();

			var passwordLog1 = contact1.GetPasswordChangedOrSentLog();
			var passwordLog2 = contact2.GetPasswordChangedOrSentLog();
			AssertNotNull(passwordLog1);
			AssertNull(passwordLog2);
		}

		public void TestShouldOnlySendToContactsCreatedSinceContactCreatedStartFromDate()
		{
			WebDataRegistry.Instance.ContactCreatedStartFromDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 4, 30));
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://myaccount.test.com");

			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "aaa";
			contact1.OC_IsActive = true;
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_Email = "aaa@test.com";
			contact1.OC_SystemCreateTimeUtc = new ZDateTime(2023, 5, 1);

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "bbb";
			contact2.OC_IsActive = true;
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_SystemCreateTimeUtc = new ZDateTime(2023, 4, 30);
			contact2.OC_Email = "bbb@test.com";

			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_ContactName = "ccc";
			contact3.OC_IsActive = true;
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_SystemCreateTimeUtc = new ZDateTime(2023, 4, 29);
			contact3.OC_Email = "ccc@test.com";

			Factory.Save();

			var process = new SendPasswordInstructionsServiceTask();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;

			logger.ClearLog();
			process.RunTask();

			var passwordLog1 = contact1.GetPasswordChangedOrSentLog();
			var passwordLog2 = contact2.GetPasswordChangedOrSentLog();
			var passwordLog3 = contact3.GetPasswordChangedOrSentLog();
			AssertNotNull(passwordLog1);
			AssertNotNull(passwordLog2);
			AssertNull(passwordLog3);
		}

		public void TestShouldOnlySendToContactsWithValidEmailAddresses()
		{
			WebDataRegistry.Instance.ContactCreatedStartFromDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 4, 30));
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://myaccount.test.com");

			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_ContactName = "ccc";
			contact3.OC_IsActive = true;
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_Email = "ccc@test.com";
			contact3.OC_SystemCreateTimeUtc = new ZDateTime(2023, 5, 1);

			var contact4 = Factory.NewWithValidTestData<OrgContact>();
			contact4.OC_ContactName = "ddd";
			contact4.OC_IsActive = true;
			contact4.OC_WebAccessEnabled = true;
			contact4.OC_Email = "";
			contact4.OC_SystemCreateTimeUtc = new ZDateTime(2023, 5, 1);

			Factory.Save();

			var process = new SendPasswordInstructionsServiceTask();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;

			logger.ClearLog();
			process.RunTask();

			var passwordLog3 = contact3.GetPasswordChangedOrSentLog();
			var passwordLog4 = contact4.GetPasswordChangedOrSentLog();
			AssertNotNull(passwordLog3);
			AssertNull(passwordLog4);
		}

		public void TestShouldOnlySendIfRegistryIsOverridden()
		{
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://myaccount.test.com");

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "aaa";
			contact.OC_IsActive = true;
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "aaa@test.com";
			contact.OC_SystemCreateTimeUtc = new ZDateTime(2023, 5, 1);

			Factory.Save();

			var process = new SendPasswordInstructionsServiceTask();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;

			logger.ClearLog();
			process.RunTask();

			AssertEquals(false, ((ZDateTime)WebDataRegistry.Instance.ContactCreatedStartFromDate.Value).IsValid);
			var passwordLog = contact.GetPasswordChangedOrSentLog();
			AssertNull(passwordLog);

			WebDataRegistry.Instance.ContactCreatedStartFromDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 4, 30));
			logger.ClearLog();
			process.RunTask();

			AssertEquals(true, ((ZDateTime)WebDataRegistry.Instance.ContactCreatedStartFromDate.Value).IsValid);
			var passwordLogNew = contact.GetPasswordChangedOrSentLog();
			AssertNotNull(passwordLogNew);
		}
	}
}
