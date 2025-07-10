using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor.Testing
{
	class IncidentBatchProcessReporterTest : TestCaseWithFactory
	{
		public void TestCounts()
		{
			AssertEquals("ProcessedIncidentCount", 0, Reporter.ProcessedIncidentCount);
			AssertEquals("UnprocessedIncidentCount", 0, Reporter.UnprocessedIncidentCount);
			AssertEquals("ErrorCount", 0, Reporter.ErrorCount);
			SupportIncident incident1 = CreateIncident("");
			SupportIncident incident2 = CreateIncident("");
			SupportIncident incident3 = CreateIncident("");
			SupportIncident incident4 = CreateIncident("");
			SupportIncident incident5 = CreateIncident("");
			Reporter.AddProcessedIncident(incident1, "");
			Reporter.AddProcessedIncident(incident2, "");
			Reporter.AddUnprocessedIncident(incident3, "");
			Reporter.AddUnprocessedIncident(incident4, "");
			Reporter.AddUnprocessedIncident(incident5, "");
			Reporter.AddError("");
			AssertEquals("ProcessedIncidentCount", 2, Reporter.ProcessedIncidentCount);
			AssertEquals("UnprocessedIncidentCount", 3, Reporter.UnprocessedIncidentCount);
			AssertEquals("ErrorCount", 1, Reporter.ErrorCount);
		}

		[TestDate(2021, 1, 1)]
		public void TestSendReport_NoGroup()
		{
			EDIDataRegistry.Instance.IncidentsBatchProcessNotificationGroup = Guid.Empty;
			string result = string.Empty;
			AssertNoExceptionThrown(delegate
			{
				result = Reporter.SendReport();
			});
			AssertEquals("Email (Subject: 'Incidents Batch Process Report for 01-Jan-21', For Group: WiseTech Global Client Extensions -> Release Builds & Upgrades -> Incidents Batch Process Notification Group) must have at least one recipient, CC or BCC", result);
			AssertEquals(null, ErrorReporter.LastExceptionReported);
			ErrorReporter.Clear();
		}

		[TestDate(2006, 1, 1)]
		public void TestSendReport()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "bob@builders.com";
			staff.GS_Code = "ZAC";
			Factory.Save();
			EDIDataRegistry.Instance.IncidentsBatchProcessNotificationGroup = group.PK.ToGuid();
			var error = Reporter.SendReport();
			AssertEquals(string.Empty, error);
			string body = Reporter.GetReportBody();
			string[] sectionList = body.Split(new string[] { "<h3>" }, StringSplitOptions.None);
			Assert("GetReportBody()", sectionList[0].Contains("</head>"));
			Assert("GetReportBody()", sectionList[1].StartsWith("Table of Contents"));
			Assert("GetReportBody()", sectionList[1].Contains("<a href=\"#toc1\">Unprocessed incidents</a>"));
			Assert("GetReportBody()", sectionList[2].StartsWith("Current Builds"));
			Assert("GetReportBody()", sectionList[3].StartsWith("Resolving Unprocessed Incidents"));
			Assert("GetReportBody()", sectionList[4].StartsWith("Unprocessed incidents: 0"));
			Assert("GetReportBody()", sectionList[5].StartsWith("Processed incidents: 0"));
			Assert("GetReportBody()", sectionList[6].Contains("Errors: 0"));
			AssertEquals("EmailsCreated.Count", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef firstEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("EmailsCreated[0].Recipients[0]", "bob@builders.com", firstEmail.Recipients[0]);
			AssertEquals("EmailsCreated[0].Subject", "Incidents Batch Process Report for 01-Jan-06", firstEmail.Subject);
			AssertEquals("EmailsCreated[0].Body", body, firstEmail.Body);
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_ExeVersionDate = ZDateTime.Now.AddDays(-1);
			build.HL_ReleaseStatus = "PRD";
			build.HL_Release = 1234;
			build.HL_Patch = 270;
			var clientCompany1 = CreateClientCompany("JAX111", "JAE", build);
			var clientCompany2 = CreateClientCompany("AAASYD", "AAE", build);
			SupportIncident incident1 = CreateIncident("CS00000001", "", clientCompany1);
			SupportIncident incident2 = CreateIncident("CS00000002", "Description Two", clientCompany1);
			SupportIncident incident3 = CreateIncident("CS00000003", "", clientCompany1);
			SupportIncident incident4 = CreateIncident("CS00000004", "", clientCompany1);
			SupportIncident incident5 = CreateIncident("CS00000005", "Description Five", clientCompany1);
			SupportIncident incident6 = CreateIncident("CS00000006", "", clientCompany1);
			SupportIncident incident7 = CreateIncident("CS00000007", "", clientCompany2);
			incident1.IM_CloseTimeUtc = new ZDateTime(2003, 5, 11);
			incident3.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident4.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			Reporter.AddProcessedIncident(incident2, "Upgrade sent");
			Reporter.AddProcessedIncident(incident1, "Upgrade sent");
			Reporter.AddProcessedIncident(incident6, "Upgrade not sent as client is already on the same version");
			Reporter.AddUnprocessedIncident(incident5, "No related work item");
			Reporter.AddUnprocessedIncident(incident4, "No related work item");
			Reporter.AddUnprocessedIncident(incident7, "No related work item");
			Reporter.AddUnprocessedIncident(incident3, "Work item's status not set to Fixed");
			Reporter.AddError("Release build was missing");
			Reporter.AddError("Incident was invalid");
			error = Reporter.SendReport();
			AssertEquals(string.Empty, error);
			body = Reporter.GetReportBody();
			sectionList = body.Split(new string[] { "<h3>" }, StringSplitOptions.None);
			Assert("GetReportBody()", sectionList[0].Contains("</head>"));
			Assert("GetReportBody()", sectionList[1].StartsWith("Table of Contents"));
			Assert("GetReportBody()", sectionList[2].StartsWith("Current Builds"));
			Assert("GetReportBody()", sectionList[3].StartsWith("Resolving Unprocessed Incidents"));
			Assert("GetReportBody()", sectionList[4].StartsWith("Unprocessed incidents: 4"));
			Assert("GetReportBody()", sectionList[5].StartsWith("Processed incidents: 3"));
			// Table of contents
			string section = sectionList[1];
			int pos1 = section.IndexOf("<a href=\"#toc0\">Resolving Unprocessed Incidents</a>");
			Assert("GetReportBody()", pos1 > 0);
			int pos2 = section.IndexOf("<a href=\"#toc1\">Unprocessed incidents</a>");
			Assert("GetReportBody()", pos2 > pos1);
			pos1 = pos2;
			pos2 = section.IndexOf("<a href=\"#toc2\">Critical: No related work item");
			Assert("GetReportBody()", pos2 > pos1);
			pos1 = pos2;
			pos2 = section.IndexOf("<a href=\"#toc3\">Non-critical: No related work item");
			Assert("GetReportBody()", pos2 > pos1);
			pos1 = pos2;
			pos2 = section.IndexOf("<a href=\"#toc4\">Non-critical: Work item's status not set to Fixed");
			Assert("GetReportBody()", pos2 > pos1);
			pos1 = pos2;
			pos2 = section.IndexOf("<a href=\"#toc5\">Processed incidents");
			Assert("GetReportBody()", pos2 > pos1);
			pos1 = pos2;
			pos2 = section.IndexOf("<a href=\"#toc6\">Upgrade not sent as client is already on the same version");
			Assert("GetReportBody()", pos2 > pos1);
			pos1 = pos2;
			pos2 = section.IndexOf("<a href=\"#toc7\">Upgrade sent");
			Assert("GetReportBody()", pos2 > pos1);
			Assert("GetReportBody()", sectionList[2].Contains("<a name=\"toc0\" />"));
			Assert("GetReportBody()", sectionList[3].Contains("<a name=\"toc1\" />"));
			Assert("GetReportBody()", sectionList[4].Contains("<a name=\"toc2\" />"));
			Assert("GetReportBody()", sectionList[4].Contains("<a name=\"toc3\" />"));
			Assert("GetReportBody()", sectionList[4].Contains("<a name=\"toc4\" />"));
			Assert("GetReportBody()", sectionList[4].Contains("<a name=\"toc5\" />"));
			Assert("GetReportBody()", sectionList[5].Contains("<a name=\"toc6\" />"));
			Assert("GetReportBody()", sectionList[5].Contains("<a name=\"toc7\" />"));
			// Processed
			section = sectionList[5];
			Assert("GetReportBody()", !section.Contains("How To Fix"));
			pos1 = section.IndexOf("<h4>Upgrade sent");
			Assert("GetReportBody()", pos1 > 0);
			pos2 = section.IndexOf(incident1.IM_IncidentNumber);
			Assert("GetReportBody()", pos2 > pos1);
			pos1 = pos2;
			pos2 = section.IndexOf(incident2.IM_IncidentNumber);
			Assert("GetReportBody()", pos2 > pos1);
			pos1 = section.IndexOf("<h4>Upgrade not sent as client is already on the same version");
			Assert("GetReportBody()", pos1 > 0);
			pos2 = section.IndexOf(incident6.IM_IncidentNumber);
			Assert("GetReportBody()", pos2 > pos1);
			// Unprocessed
			section = sectionList[4];
			pos1 = section.IndexOf("<h4>Critical: No related work item: 2");
			Assert("GetReportBody()", pos1 > 0);
			pos2 = section.IndexOf("<a href=\"#toc0\">How To Fix</a>");
			Assert("GetReportBody()", pos2 > pos1);
			pos1 = pos2;
			pos2 = section.IndexOf(incident7.IM_IncidentNumber);
			Assert("GetReportBody()", pos2 > pos1);
			pos1 = pos2;
			pos2 = section.IndexOf(incident5.IM_IncidentNumber);
			Assert("GetReportBody()", pos2 > pos1);
			pos1 = pos2;
			pos2 = section.IndexOf(incident5.IM_Description);
			Assert("GetReportBody()", pos2 > pos1);
			pos1 = pos2;
			pos2 = section.IndexOf("<h4>Non-critical: No related work item: 1");
			Assert("GetReportBody()", pos2 > pos1);
			pos1 = pos2;
			pos2 = section.IndexOf(incident4.IM_IncidentNumber);
			Assert("GetReportBody()", pos2 > pos1);
			pos1 = section.IndexOf("<h4>Non-critical: Work item's status not set to Fixed: 1");
			Assert("GetReportBody()", pos1 > 0);
			pos1 = pos2;
			pos2 = section.IndexOf(incident3.IM_IncidentNumber);
			Assert("GetReportBody()", pos2 > pos1);
			AssertEquals("EmailsCreated.Count", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef secondEmail = Env.OutgoingMailManager.EmailsCreated[1];
			AssertEquals("EmailsCreated[1].Recipients[0]", "bob@builders.com", secondEmail.Recipients[0]);
			AssertEquals("EmailsCreated[1].Subject", "Incidents Batch Process Report for 01-Jan-06", secondEmail.Subject);
			AssertEquals("EmailsCreated[1].Body", body, secondEmail.Body);
		}

		public void TestTestMethods()
		{
			string reason;
			SupportIncident incident1 = CreateIncident("");
			SupportIncident incident2 = CreateIncident("");
			AssertEquals("IsIncidentProcessed(\"1\")", false, Reporter.IsIncidentProcessed(incident1, out reason));
			AssertNull("IsIncidentProcessed(\"1\") reason", reason);
			Reporter.AddProcessedIncident(incident1, "y");
			AssertEquals("IsIncidentProcessed(\"1\")", true, Reporter.IsIncidentProcessed(incident1, out reason));
			AssertEquals("IsIncidentProcessed(\"1\") reason", "y", reason);
			AssertEquals("IsIncidentProcessed(\"2\")", false, Reporter.IsIncidentProcessed(incident2, out reason));
			AssertNull("IsIncidentProcessed(\"2\") reason", reason);
			Reporter.AddUnprocessedIncident(incident2, "Gah");
			AssertEquals("IsIncidentProcessed(\"2\")", false, Reporter.IsIncidentProcessed(incident2, out reason));
			AssertEquals("IsIncidentProcessed(\"2\") reason", "Gah", reason);
			AssertEquals("HasError(\"z\")", false, Reporter.HasError("z"));
			Reporter.AddError("z");
			AssertEquals("HasError(\"z\")", true, Reporter.HasError("z"));
		}

		SupportIncident CreateIncident(string number)
		{
			SupportIncident result = Factory.New<SupportIncident>();
			result.IM_IncidentNumber = number;
			result.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			return result;
		}

		SupportIncident CreateIncident(string number, string description, ClientCompany clientCompany)
		{
			SupportIncident result = CreateIncident(number);
			result.IM_Description = description;
			if (clientCompany != null)
			{
				result.IM_LCC = clientCompany.PK;
				result.IM_LD = clientCompany.LCC_LD;
			}

			return result;
		}

		ClientCompany CreateClientCompany(string orgCode, string enterpriseCode, ReleaseBuild build)
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = orgCode;
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_Email = "test@cargowise.com";
			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = enterpriseCode;
			enterprise.LE_OH = org.PK;
			LicenceDatabase database = enterprise.Databases.AddNew();
			database.LD_ServerCode = "SRV";
			database.LD_LicenceType = DatabaseTypes.Codes.Production;
			database.LD_HL_CurrentRunningVersion = build.PK;
			ClientCompany company = Factory.New<ClientCompany>();
			company.LCC_Code = "COM";
			company.LCC_OH = org.PK;
			company.LCC_LD = database.PK;
			Factory.Save();
			return company;
		}

		IncidentBatchProcessReporter Reporter
		{
			get
			{
				if (reporter == null)
				{
					reporter = new IncidentBatchProcessReporter();
				}

				return reporter;
			}
		}

		IncidentBatchProcessReporter reporter;
	}
}
