using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using MailManager;

namespace Enterprise.BatchProcessor.MailManager.Testing
{
	sealed class DiagnosticsReportGeneratorTest : TestCaseWithFactory
	{
		public void TestMessageFilter()
		{
			var requestEmail = Factory.New<MailItem>();
			requestEmail.MI_From = "enterpriseproduction@acsedi.edi.net.au";
			requestEmail.MI_Subject = "Messaging Diagnostics Request (AU)";

			int preCount = Factory.GetDatabaseCount(typeof(MailItem), new ZQuery(MailDBItemsSchema.MI_Subject, "Messaging Diagnostics Report"));

			var helper = new MessageFilterTestHelper<DiagnosticsReportGenerator, MailItem>();
			Assert(helper.Process(requestEmail));
			AssertEquals(preCount + 1, Factory.GetDatabaseCount(typeof(MailItem), new ZQuery(MailDBItemsSchema.MI_Subject, "Messaging Diagnostics Report")));

			requestEmail.MI_From = "test@edi.com.au";
			Assert(helper.Process(requestEmail));
			AssertEquals(preCount + 2, Factory.GetDatabaseCount(typeof(MailItem), new ZQuery(MailDBItemsSchema.MI_Subject, "Messaging Diagnostics Report")));

			requestEmail.MI_From = "test@cargowise.com";
			Assert(!helper.Process(requestEmail));
			AssertEquals(preCount + 2, Factory.GetDatabaseCount(typeof(MailItem), new ZQuery(MailDBItemsSchema.MI_Subject, "Messaging Diagnostics Report")));
		}

		public void TestProcessAnyDiagnosticsRequests()
		{
			Generator.ProcessMailItem(InsertDiagnosticsRequest());
			AssertEquals("Email Sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestGetReport()
		{
			AssertMultilineASCIIEquals("Report",
				@"===CargoWise One Info===
SMTPServer = 'XCH', SMTPPort = '25', SMTPUsername = '', MailServer = '', MailServerPort = '110', MailboxEmailAddress = 'Default@edi.com.au', MailboxUserName = ''

===Certificate Info===
Trust Point: Name = 'Gatekeeper TYPE 3 CA', EmailAddress = '', SerialNumber = '34 61 c1 08 a2 68 e4 8b 6d d7 d7 73 4e 8c d4 b6', ValidFromDate = '10/04/2003 10:00:00 AM', ValidToDate = '25/05/2022 9:59:59 AM'
Customs Encryption: Name = 'CCF E-mail Gateway', EmailAddress = 'cargo@ccf.homeaffairs.gov.au', SerialNumber = '57 be 41 80 75 f0 1d 1f 34 f2 c4 e1 a6 68 94 ed', ValidFromDate = '11/12/2019 11:00:00 AM', ValidToDate = '25/01/2022 10:59:59 AM'


===Company Info===
Company Name = 'Demo Company', GC_Code = 'DEM', GC_BusinessRegNo = '9999', GC_CustomsRegistrationNo = '', CustomsMailbox = '', SeaDepotMailbox = '', CMRTestMode = 'False'
===Branch Info===
BranchCode = 'DEM', CustomsMailbox = '', SeaDepotMailbox = '', CMRTestMode = 'False'



===Company Info===
Company Name = 'Eagle Datamation International Pte Ltd', GC_Code = 'SIN', GC_BusinessRegNo = '', GC_CustomsRegistrationNo = '', CustomsMailbox = '', SeaDepotMailbox = '', CMRTestMode = 'False'
===Branch Info===
BranchCode = 'SIN', CustomsMailbox = '', SeaDepotMailbox = '', CMRTestMode = 'False'



===Company Info===
Company Name = 'Eagle Datamation International', GC_Code = 'EDI', GC_BusinessRegNo = '41 065 894 724', GC_CustomsRegistrationNo = 'AAA374M', CustomsMailbox = '06050004400726', SeaDepotMailbox = '06050004400726', CMRTestMode = 'False'
===Branch Info===
BranchCode = 'BNE', CustomsMailbox = '06050004400726', SeaDepotMailbox = '06050004400726', CMRTestMode = 'False'
BranchCode = 'SYD', CustomsMailbox = '06050004400726', SeaDepotMailbox = '06050004400726', CMRTestMode = 'False'
BranchCode = 'TES', CustomsMailbox = '06050004400726', SeaDepotMailbox = '06050004400726', CMRTestMode = 'False'



===Last 5 Outgoing CMR Messages===

===Last 5 Incoming CMR Messages===

===Last 5 Outgoing CMR Interchanges===

===Last 5 Incoming CMR Interchanges===


===Last 5 Queued Outgoing Emails===

===Last 5 Sent Outgoing Emails===

===Last 5 Queued Incoming Emails===

===Last 5 Processed Incoming Emails===", Generator.GetReport());
		}

		public void TestGetReportDoesntBlowUpBadlyIfThereIsAnException()
		{
			TestHelperDiagnosticsReportGeneratorThatThrowsExceptionWhistGettingEnterpriseInfo generator = new TestHelperDiagnosticsReportGeneratorThatThrowsExceptionWhistGettingEnterpriseInfo();
			AssertEquals("Report", "===CargoWise One Info===\r\nAn exception of type Exception occured whilst generating the report.  Exception message = 'Fake Exception Message'", generator.GetReport());
		}

		public void TestGetMailItemReport()
		{
			MailItem testMailItem = Factory.New<MailItem>();
			testMailItem.MI_Direction = DirectionList.Codes.Transmit;
			testMailItem.MI_Status = "SNT";
			testMailItem.MI_SendDateTime = ZDateTime.Now.Date.AddHours(ZDateTime.Now.Hour).AddHours(1);
			testMailItem.MI_ReceivedDateTime = ZDateTime.Now.Date.AddHours(ZDateTime.Now.Hour).AddHours(1);
			testMailItem.MI_Subject = "ZZZ";
			testMailItem.MI_From = "FROM";
			Factory.Save();

			string expected = String.Format("MI_Direction = 'TRX', MI_Status = 'SNT', MI_ReceivedDateTime = '{0}', MI_Subject = 'ZZZ', MI_From = 'FROM'", testMailItem.MI_ReceivedDateTime);
			AssertContainsInfo("Last 5 Sent Outgoing Emails", expected);
		}

		public void TestGetInterchangeReport()
		{
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			interchange.EI_Status = "ZZZ";
			interchange.EI_From = "FROM";
			interchange.EI_To = "TO";
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_BodyText = "BODY";
			interchange.EI_FooterText = "FOOTER";
			Factory.Save();

			string expected = String.Format("EI_ReceiveTransmit = 'RCV', EI_ApplicationCode = 'CMR', EI_Status = 'ZZZ', EI_HeaderText = 'HEADER'");
			AssertContainsInfo("Last 5 Incoming CMR Interchanges", expected);
		}

		public void TestGetMessageReport()
		{
			EDIMessage message = Factory.New<Enterprise.Messaging.Testing.TestEdiMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			message.EM_MessageType = "YYY";
			message.EM_MessageSubType = "ZZZ";
			message.EM_Status = "AAA";
			message.EM_MessageText = "MESSAGETEXT" + EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();

			string expected = String.Format("EM_ReceiveTransmit = 'TRX', EM_ApplicationCode = 'CMR', EM_MessageType = 'YYY', EM_MessageSubType = 'ZZZ', EM_Status = 'AAA', EM_MessageText = '{0}', AddedTime = '{1}'",
				message.EM_MessageText,
				message.EM_SystemCreateTimeUtc);
			AssertContainsInfo("Last 5 Outgoing CMR Messages", expected);
		}

		public void TestGetEnterpriseReport()
		{
			Env.Registry.SMTPServer = "123";
			Env.Registry.SMTPPort = 234;
			Env.Registry.SMTPUsername = "345";
			Env.Registry.MailServer = "456";
			Env.Registry.MailServerPort = 567;
			Env.Registry.MailboxEmailAddress = "678@x.com";
			Env.Registry.MailboxUserName = "789";

			AssertContainsInfo("CargoWise One Info", "SMTPServer = '123', SMTPPort = '234', SMTPUsername = '345', MailServer = '456', MailServerPort = '567', MailboxEmailAddress = '678@x.com', MailboxUserName = '789'");
		}

		public void TestGetCertificatesReport()
		{
			AssertContainsInfo("Certificate Info", "Trust Point: Name = 'Gatekeeper TYPE 3 CA', EmailAddress = '', SerialNumber = '34 61 c1 08 a2 68 e4 8b 6d d7 d7 73 4e 8c d4 b6', ValidFromDate = '10/04/2003 10:00:00 AM', ValidToDate = '25/05/2022 9:59:59 AM'\r\nCustoms Encryption: Name = 'CCF E-mail Gateway', EmailAddress = 'cargo@ccf.homeaffairs.gov.au', SerialNumber = '57 be 41 80 75 f0 1d 1f 34 f2 c4 e1 a6 68 94 ed', ValidFromDate = '11/12/2019 11:00:00 AM', ValidToDate = '25/01/2022 10:59:59 AM'");
		}

		public void TestGetCompanyReport()
		{
			Guid companyGuid = GlbCompany.CurrentCompany.PK.ToGuid();
			Env.Registry.SetAUCustomsSenderIDForCpmpany(companyGuid, "111");
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForCompany(companyGuid, "222");
			Env.Registry.SetCMRTestModeForCompany(companyGuid, false);

			AssertContainsInfo("Company Info", "Company Name = 'Eagle Datamation International', GC_Code = 'EDI', GC_BusinessRegNo = '41 065 894 724', GC_CustomsRegistrationNo = 'AAA374M', CustomsMailbox = '111', SeaDepotMailbox = '222', CMRTestMode = 'False'");
		}

		public void TestGetBranchReport()
		{
			Guid branchGuid = GlbBranch.CurrentBranch.PK.ToGuid();
			Env.Registry.SetAUCustomsSenderIDForBranch(branchGuid, "111");
			Env.Registry.SetAUCustomsSeaCargoDepotMailboxForBranch(branchGuid, "222");
			Env.Registry.SetCMRTestModeForCompany(GlbCompany.CurrentCompany.PK.ToGuid(), false);

			AssertContainsInfo("Branch Info", "BranchCode = 'BNE', CustomsMailbox = '111', SeaDepotMailbox = '222', CMRTestMode = 'False'");
		}

		#region Implementation

		void AssertContainsInfo(string section, string expected)
		{
			bool containsInfo = false;
			foreach (string info in GetReportSection(section))
			{
				if (info.Contains(expected))
				{
					containsInfo = true;
					break;
				}
			}
			Assert(section, containsInfo);
		}

		string[] GetReportSection(string section)
		{
			List<string> results = new List<string>();

			string report = Generator.GetReport();
			section = "===" + section + "===";
			int start = 0;
			while (true)
			{
				start = report.IndexOf(section, start);
				if (start < 0)
				{
					break;
				}

				start = start + section.Length;

				int nextSec = report.IndexOf("\r\n===", start);
				int length = nextSec >= 0 ? nextSec - start : report.Length - start;
				results.Add(report.Substring(start, length).Trim());

				start += length;
			}

			return results.ToArray();
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.New<Enterprise.Integration.Customs.AU.ICertificateManagerHelper>(Factory).CreateCustomsCertificates2021();
			Factory.Save();
		}

		#endregion

		#region TestHelpers

		class TestHelperDiagnosticsReportGeneratorThatThrowsExceptionWhistGettingEnterpriseInfo : DiagnosticsReportGenerator
		{
			protected override string GetEnterpriseReport()
			{
				throw new Exception("Fake Exception Message");
			}
		}

		MailItem InsertDiagnosticsRequest()
		{
			var factory = new BusinessObjectFactory();
			var requestEmail = factory.New<MailItem>();
			requestEmail.MI_From = "enterpriseproduction@acsedi.edi.net.au";
			requestEmail.MI_Subject = "Messaging Diagnostics Request (AU)";
			requestEmail.MI_Status = "QUE";
			requestEmail.MI_Direction = DirectionList.Codes.Receive;
			requestEmail.MI_SendDateTime = ZDateTime.UtcNow;
			requestEmail.MI_ReceivedDateTime = ZDateTime.UtcNow;
			factory.Save();

			return requestEmail;
		}

		DiagnosticsReportGenerator fGenerator;
		DiagnosticsReportGenerator Generator
		{
			get
			{
				if (fGenerator == null)
				{
					fGenerator = new DiagnosticsReportGenerator();
				}
				return fGenerator;
			}
		}

		#endregion
	}
}
