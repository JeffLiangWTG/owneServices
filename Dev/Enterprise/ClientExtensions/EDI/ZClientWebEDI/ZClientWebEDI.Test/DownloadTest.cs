using System;
using System.IO;
using System.Text;
using System.Web;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Integration.CustomerService;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class DownloadTest : ZPageTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetProccessedURL()
		{
			EDIDataRegistry.Instance.MyAccountPhysicalServerPath.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestCaseWithFactory.BaseSourcePath + @"Enterprise\ClientExtensions\EDI\ZClientWebEDI\ZClientWebEDI.Test\TestFiles");
			var mainURL = "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20100204a.pdf";
			var downloadPage = Page as DownloadForTest;
			var converter = ObjectFactory.Get<IStaffContactConverter>();
			var queryString = converter.CurrentStaffAndRegistrationToSecuredQueryString();
			queryString["file"] = mainURL;
			queryString["language"] = "AZ-AZ";
			var expectedURL = "http://www.cargowise.com/Documents/UpdateNotes/AZ-AZ/ediEnterpriseupdatenote20100204aAZ-AZ.pdf";
			AssertEquals(expectedURL, downloadPage.GetFileURL_Exposed(queryString));
			queryString["language"] = "EN";
			expectedURL = "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20100204a.pdf";
			AssertEquals(expectedURL, downloadPage.GetFileURL_Exposed(queryString));
			queryString["language"] = "EN-GB";
			expectedURL = "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20100204a.pdf";
			AssertEquals(expectedURL, downloadPage.GetFileURL_Exposed(queryString));
			queryString["language"] = string.Empty;
			expectedURL = "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20100204a.pdf";
			AssertEquals(expectedURL, downloadPage.GetFileURL_Exposed(queryString));
			queryString["language"] = null;
			expectedURL = "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20100204a.pdf";
			AssertEquals(expectedURL, downloadPage.GetFileURL_Exposed(queryString));
			queryString["file"] = "https://myaccount.cargowise.com/en-us/Home/CargoWiseOneWiseLearning.aspx#search=1cor318&video=20571080,305bf273ce1100f8dc73f0ecb380f836";
			expectedURL = "https://myaccount.cargowise.com/en-us/Home/CargoWiseOneWiseLearning.aspx#search=1cor318&video=20571080,305bf273ce1100f8dc73f0ecb380f836";
			AssertEquals(expectedURL, downloadPage.GetFileURL_Exposed(queryString));
			queryString["file"] = "https://www.hackerhere.haha.com/pid=134972834703";
			expectedURL = "https://myaccount.cargowise.com/Home.aspx";
			AssertEquals(expectedURL, downloadPage.GetFileURL_Exposed(queryString));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessRequest_LoggedIn()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			var testOrg = (EDIOrgHeader)database.WebAccessOrg;
			var testContact = (EDIOrgContact)testOrg.Contacts.AddNew();
			testContact.OC_ContactName = "User 1";
			testContact.OC_Email = "u1@cw1.com";
			testContact.OC_WebAccessEnabled = true;
			testContact.SetHashedPassword("123");
			Factory.Save();
			EDIDataRegistry.Instance.MyAccountPhysicalServerPath.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestCaseWithFactory.BaseSourcePath + @"Enterprise\ClientExtensions\EDI\ZClientWebEDI\ZClientWebEDI.Test\TestFiles");
			var mainURL = "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20100204a.pdf";
			var downloadPage = Page as DownloadForTest;
			downloadPage.SiteUser.Login(testOrg.OH_Code, testContact.OC_Email, "123");
			AssertNotNull(downloadPage.SiteUser);
			Assert(downloadPage.SiteUser.IsLoggedIn);
			HttpContext.Current.Request.QueryString.Add("language", "AZ-AZ");
			HttpContext.Current.Request.QueryString.Add("file", mainURL);
			downloadPage.ProcessRequest(HttpContext.Current);
			AssertEquals("http://www.cargowise.com/Documents/UpdateNotes/AZ-AZ/ediEnterpriseupdatenote20100204aAZ-AZ.pdf", HttpContext.Current.Response.RedirectLocation);
		}

		public void TestDownloadReport()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			var testOrg = (EDIOrgHeader)database.WebAccessOrg;
			var testContact = (EDIOrgContact)testOrg.Contacts.AddNew();
			testContact.OC_ContactName = "User 1";
			testContact.OC_Email = "u1@cw1.com";
			testContact.OC_WebAccessEnabled = true;
			testContact.SetHashedPassword("123");

			var report = Factory.New<EdiReportingQueue>();
			report.ERQ_OC = testContact.PK;
			report.ERQ_OH = testOrg.PK;
			report.ERQ_Period = 202401;
			report.ERQ_Status = "PCD";
			report.ERQ_ReportName = "report1";
			report.ERQ_ReportFileFullName = $@"\\127.0.0.1\{ZGuid.NewZGuid()}\file.zip";
			report.ERQ_ReportType = "RPT";
			Factory.Save();

			var downloadPage = Page as DownloadForTest;
			downloadPage.SiteUser.Login(testOrg.OH_Code, testContact.OC_Email, "123");
			AssertNotNull(downloadPage.SiteUser);
			Assert(downloadPage.SiteUser.IsLoggedIn);

			var sb = new StringBuilder();
			var stringWriter = new StringWriter(sb);
			var context = new HttpContext(new HttpRequest("download.aspx", "https://www.cw1.com/download.aspx", $"report={report.PK}"), new HttpResponse(stringWriter));
			downloadPage.ProcessRequest(context);

			AssertEquals("ZClientWebCargoWiseEDI.DownloadReport", ErrorReporter.LastKeyReported);
			AssertEquals("The network name cannot be found.\r\n", ErrorReporter.LastMessageReported);
			AssertEquals("The required file is currently unavailable. Please try again later.", sb.ToString());
			ErrorReporter.Clear();

			using (var fn = TempFile.New())
			{
				File.WriteAllText(fn.Filename, "file~");
				report.ERQ_ReportFileFullName = fn.Filename;
				Factory.Save();

				HttpContext.Current.Request.QueryString.Add("report", report.PK.ToString());
				downloadPage.ProcessRequest(HttpContext.Current);
				AssertEquals("", ErrorReporter.LastKeyReported);
				AssertEquals("", ErrorReporter.LastMessageReported);
			}
		}

		protected override ZPage GetNewZPage() => new DownloadForTest()
		{ IsCreateNewAppInstanceIfNullForTest = true };
		class DownloadForTest : Download
		{
			public string GetFileURL_Exposed(SecureQueryString queryString) => GetFileURL(queryString, null);
			protected override ZGlobal GetNewTestGlobal()
			{
				GlobalForTest result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}
		}

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}
	}
}
