using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Environment;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DocumentUsageReporterTest : TestCaseWithFactory
	{
		public void TestReportForContacts()
		{
			var helper = new UsageCollectorTestHelper(Factory);
			Assert("No DocumentGenerated in the beginning.", helper.AssertUsageSummmaryMessagesCount(UsageFeatures.Codes.DocumentGenerated, 0));

			var documentUsageReporter = new DocumentUsageReporter();
			documentUsageReporter.MenuTitle = "TestMenuTitle";
			documentUsageReporter.IsPreview = true;
			documentUsageReporter.ContactDetails = new List<(string DeliveryMethod, string AttachmentType)>();
			documentUsageReporter.ContactDetails.Add(("EML", "PDF"));
			documentUsageReporter.ContactDetails.Add(("PRN", ""));

			var properties1 = new List<(string name, object value)>();
			var properties2 = new List<(string name, object value)>();

			properties1.Add(("MenuTitle", "TestMenuTitle"));
			properties1.Add(("DeliveryMethod", "EML"));
			properties1.Add(("AttachmentType", "PDF"));
			properties1.Add(("IsPreview", true));
			properties1.Add(("IsTriggeredViaWorkflow", false));
			properties1.Add(("IsUserSignatureUsed", false));

			properties2.Add(("MenuTitle", "TestMenuTitle"));
			properties2.Add(("DeliveryMethod", "PRN"));
			properties2.Add(("AttachmentType", ""));
			properties2.Add(("IsPreview", true));
			properties2.Add(("IsTriggeredViaWorkflow", false));
			properties2.Add(("IsUserSignatureUsed", false));

			documentUsageReporter.ReportForContacts();

			Assert("Contains message.", helper.AssertUsageSummaryMessagesContains(UsageFeatures.Codes.DocumentGenerated, properties1));
			Assert("Contains message.", helper.AssertUsageSummaryMessagesContains(UsageFeatures.Codes.DocumentGenerated, properties2));
		}

		public void TestReportForContactsViaWorkflow()
		{
			using (DisposableEnvironment.ForBranch(Env.CurrentBranch.PK))
			using (Env.Instance.TemporaryServiceTaskContext("LWK", true))
			{
				var helper = new UsageCollectorTestHelper(Factory);
				Assert("No DocumentGenerated in the beginning.", helper.AssertUsageSummmaryMessagesCount(UsageFeatures.Codes.DocumentGenerated, 0));

				var documentUsageReporter = new DocumentUsageReporter();
				documentUsageReporter.MenuTitle = "TestMenuTitle";
				documentUsageReporter.IsPreview = true;
				documentUsageReporter.ContactDetails = new List<(string DeliveryMethod, string AttachmentType)>();
				documentUsageReporter.ContactDetails.Add(("EML", "PDF"));

				var properties = new List<(string name, object value)>();

				properties.Add(("MenuTitle", "TestMenuTitle"));
				properties.Add(("DeliveryMethod", "EML"));
				properties.Add(("AttachmentType", "PDF"));
				properties.Add(("IsPreview", true));
				properties.Add(("IsTriggeredViaWorkflow", true));

				documentUsageReporter.ReportForContacts();

				Assert("Contains message.", helper.AssertUsageSummaryMessagesContains(UsageFeatures.Codes.DocumentGenerated, properties));
			}
		}

		public void TestReportForContactsUserSignature()
		{
			var helper = new UsageCollectorTestHelper(Factory);
			Assert("No DocumentGenerated in the beginning.", helper.AssertUsageMessagesCount(UsageFeatures.Codes.DocumentGenerated, 0));

			var documentUsageReporter = new DocumentUsageReporter();
			documentUsageReporter.MenuTitle = "TestMenuTitle";
			documentUsageReporter.IsUserSignatureUsed = true;
			documentUsageReporter.ContactDetails = new List<(string DeliveryMethod, string AttachmentType)>();
			documentUsageReporter.ContactDetails.Add(("EML", "PDF"));
			documentUsageReporter.ContactDetails.Add(("PRN", ""));

			var properties1 = new List<(string name, object value)>();
			var properties2 = new List<(string name, object value)>();

			properties1.Add(("MenuTitle", "TestMenuTitle"));
			properties1.Add(("DeliveryMethod", "EML"));
			properties1.Add(("AttachmentType", "PDF"));
			properties1.Add(("IsTriggeredViaWorkflow", false));
			properties1.Add(("IsUserSignatureUsed", true));

			properties2.Add(("MenuTitle", "TestMenuTitle"));
			properties2.Add(("DeliveryMethod", "PRN"));
			properties2.Add(("AttachmentType", ""));
			properties2.Add(("IsTriggeredViaWorkflow", false));
			properties2.Add(("IsUserSignatureUsed", true));

			documentUsageReporter.ReportForContacts();

			Assert("Contains message.", helper.AssertUsageSummaryMessagesContains(UsageFeatures.Codes.DocumentGenerated, properties1));
			Assert("Contains message.", helper.AssertUsageSummaryMessagesContains(UsageFeatures.Codes.DocumentGenerated, properties2));
		}
	}
}
