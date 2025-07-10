using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.EConversation.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.Mail.ServiceTasks.Test
{
	[TestedType(typeof(CurrentVersionReportProcessorServiceTask))]
	class CurrentVersionReportProcessorServiceTaskTest : EdiEmailProcessorServiceProviderTestCase<CurrentVersionReportProcessorServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
		}

		[TestDate(2005, 11, 1)]
		public void TestRunTask()
		{
			const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?><VersionReport><EnterpriseCode>EDI</EnterpriseCode><PhysicalServerID>SYD</PhysicalServerID></VersionReport>";
			var email1 = new EmailBuilderForTesting().WithAttachment("currentVersion.xml", xml).GetEmail();
			var email2 = new EmailBuilderForTesting().Subject("Licence Usage Data").WithAttachment("licenceUsage.xml", @"
<?xml version=""1.0"" encoding=""utf-16""?>
<LicenceConsumptionLogSchema xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService"">
  <LicenceConsumptionLogs>
    <LicenceModuleCode>FOR</LicenceModuleCode>
    <StaffName>Zubin Appoo</StaffName>
    <StaffEmail>zubs@zubs.com</StaffEmail>
    <UsageTime>2008-03-05T09:32:00+11:00</UsageTime>
    <CompanyLicenceCode>ABCXYZ123</CompanyLicenceCode>
  </LicenceConsumptionLogs>
</LicenceConsumptionLogSchema>".Trim()).GetEmail();
			var email3 = new EmailBuilderForTesting().Subject("Enterprise Report").WithAttachment("EnterpriseReport.zip", CreatePasswordZip("EnterpriseReport.xml", @"<?xml version=""1.0"" encoding=""utf-16""?>
<LicenceConsumptionLogSchema xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.edi.com.au/EnterpriseService"">
  <IsRequest>true</IsRequest>
  <RequestedBy>RequestedBy1</RequestedBy>
  <DateFrom>2008-03-01</DateFrom>
  <DateTo>2008-03-31</DateTo>
  <EnterpriseCode>ABC</EnterpriseCode>
  <ServerCode>123</ServerCode>
  <DbServerName>DbServerName1</DbServerName>
  <DbName>DbName1</DbName>
<LicenceConsumptionLogs>
    <LicenceModuleCode>COR</LicenceModuleCode>
    <StaffName>Zubin Appoo</StaffName>
    <StaffEmail>zubs@zubs.com</StaffEmail>
    <UsageTime>2008-03-06T09:32:00+11:00</UsageTime>
    <CompanyLicenceCode>ABCXYZ123</CompanyLicenceCode>
  </LicenceConsumptionLogs>
</LicenceConsumptionLogSchema>", CurrentVersionReportProcessorServiceTask.LicenceUsagePassword)).GetEmail();

			var emailReaderFactory = new EmailReaderFactoryForTest(r => r.AddEmailBundle(email1, email2, email3));
			CreateServiceTask(emailReaderFactory).RunTask();

			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Information, "Email:1 Attachments:1 From: Subject:", null),
				new LogForTest(LogType.Information, "Report for EDI--SYD for SQL Server [] database [] has been received", null),
				new LogForTest(LogType.Information, "Email:2 Attachments:1 From: Subject:Licence Usage Data", null),
				new LogForTest(LogType.Information, "Licence:ABC/-/123 Usage(System):0(0)", null),
				new LogForTest(LogType.Information, "Email:3 Attachments:1 From: Subject:Enterprise Report", null),
				new LogForTest(LogType.Information, "Licence Usage received from [ABC/-/123] for 01-Mar-08 to 31-Mar-08", null),
				new LogForTest(LogType.Information, "Licence:ABC/-/123 Usage(System):0(0)", null),
				new LogForTest(LogType.Information, "Processed 3 emails", null),
				new LogForTest(LogType.Information, "3 emails read, processed and deleted", null));
		}

		[TestDate(2005, 11, 1)]
		public void TestEmailWithVisualInsteadOfAttachment()
		{
			var emailReaderFactory = new EmailReaderFactoryForTest(reader =>
			{
				const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?><VersionReport><EnterpriseCode>EDI</EnterpriseCode><PhysicalServerID>SYD</PhysicalServerID></VersionReport>";
				Email email1 = new EmailBuilderForTesting().From("enterprise@test.com")
					.WithVisual("currentVersion.xml", xml).GetEmail();
				reader.AddEmailBundle(email1);
			});
			var serviceTask = new CurrentVersionReportProcessorServiceTask(emailReaderFactory) { ServiceLogger = TestLogger };
			serviceTask.RunTask();
			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Information, "Email:1 Attachments:1 From:<enterprise@test.com> Subject:", null),
				new LogForTest(LogType.Information, "Report for EDI--SYD for SQL Server [] database [] has been received", null),
				new LogForTest(LogType.Information, "Processed 1 emails", null),
				new LogForTest(LogType.Information, "1 emails read, processed and deleted", null));
			string expectedFile = Path.Combine(ReportProcessorHelper.GetReportDirectoryPath("CurrentVersionReport"), "EDI-SYD.xml");
			Assert("reports saved to disk", File.Exists(expectedFile));
			Assert("file contains version report", File.ReadAllText(expectedFile).Contains("<VersionReport>"));
			ReportProcessorHelper.ClearReportsFromTesting("CurrentVersionReport");
		}

		public void TestReportMovedToAttachmentByVirusScannerCanBeProcessed()
		{
			var emailReaderFactory = new EmailReaderFactoryForTest(reader =>
			{
				const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?><VersionReport><EnterpriseCode>EDI</EnterpriseCode><PhysicalServerID>SYD</PhysicalServerID></VersionReport>";
				Email originalEmail = new EmailBuilderForTesting().Subject("Enterprise Report").WithAttachment("EnterpriseReport.zip", CreatePasswordZip("EnterpriseReport.xml", xml, CurrentVersionReportProcessorServiceTask.LicenceUsagePassword)).GetEmail();
				Email alteredEmail = new EmailBuilderForTesting().From("Name", "address@test.com").Subject("Enterprise Report").WithAttachment("Enterprise Report", originalEmail).GetEmail();
				reader.AddEmailBundle(alteredEmail);
			});

			var serviceTask = new CurrentVersionReportProcessorServiceTask(emailReaderFactory) { ServiceLogger = TestLogger };
			serviceTask.RunTask();
			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Information, "Email:1 Attachments:1 From:\"Name\" <address@test.com> Subject:Enterprise Report", null),
				new LogForTest(LogType.Information, "Report for EDI--SYD for SQL Server [] database [] has been received", null),
				new LogForTest(LogType.Information, "Processed 1 emails", null),
				new LogForTest(LogType.Information, "1 emails read, processed and deleted", null));
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestReportSubjectIncorrect()
		{
			var emailReaderFactory = new EmailReaderFactoryForTest(reader =>
			{
				Email email = new EmailBuilderForTesting().From("Name", "address@test.com").Subject("Mangled beyond all recognition").WithAttachment("EnterpriseReport.zip", CreatePasswordZip("EnterpriseReport.xml", "blah", CurrentVersionReportProcessorServiceTask.LicenceUsagePassword)).GetEmail();
				reader.AddEmailBundle(email);
			});
			var serviceTask = new CurrentVersionReportProcessorServiceTask(emailReaderFactory) { ServiceLogger = TestLogger };
			serviceTask.RunTask();
			AssertLogs(
				new LogForTest(LogType.Information, $"Attempting to connect to Server:{EmailReaderForTest.TestServer} | Mailbox:{EmailReaderForTest.TestMailbox}", null),
				new LogForTest(LogType.Information, "Email:1 Attachments:1 From:\"Name\" <address@test.com> Subject:Mangled beyond all recognition", null),
				new LogForTest(LogType.Information, "Processed 1 emails", null),
				new LogForTest(LogType.Information, "1 emails read, processed and deleted", null));
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestCreateCurrentVersionReportProcessor()
		{
			var serviceTask = new CurrentVersionReportProcessorServiceTask();
			var processor = serviceTask.CreateCurrentVersionReportProcessor();
			AssertEquals("ReceivedViaEhub", false, processor.ReceivedViaEhub);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			EDIDataRegistry.Instance.CurrentVersionReportMailBox.Server = "";
			Env.Registry.MailServer = "mehServer";
			EDIDataRegistry.Instance.CurrentVersionReportMailBox.UserName = "TestVersionReports@edi.net.au";
			EDIDataRegistry.Instance.CurrentVersionReportMailBox.Password = "pwd";
		}

		protected override CurrentVersionReportProcessorServiceTask CreateServiceTaskCore(IEmailReaderFactory emailReaderFactory)
		{
			return new CurrentVersionReportProcessorServiceTask(emailReaderFactory);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
		protected override void TearDownCore()
		{
			ReportProcessorHelper.ClearReportsFromTesting("CurrentVersionReport");
			ReportProcessorHelper.ClearReportsFromTesting("LicenceUsage");
			base.TearDownCore();
		}

		byte[] CreatePasswordZip(string fileName, string fileContents, string password)
		{
			byte[] data = Encoding.UTF8.GetBytes(fileContents);
			using (MemoryStream contentStream = new MemoryStream(data))
			using (MemoryStream zipStream = new MemoryStream())
			{
				ZipCreator creator = new ZipCreator(password);
				creator.ZipStream(new ZipStream[] { new ZipStream(fileName, contentStream) }, zipStream);
				return zipStream.ToArray();
			}
		}
	}
}
