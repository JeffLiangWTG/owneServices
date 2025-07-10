using System;
using CargoWise.Types;
using Enterprise.CustomerService.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(DocIncidentRequest))]
	class DocIncidentRequestTest : DocumentWrapperTestCase
	{
		public void TestHtmlStyleSheet()
		{
			SystemDataRegistry.Instance.HtmlEmailStyleSheet.SetValue(Env.CurrentCompany.PK, Guid.Empty, Env.CurrentDepartment.PK, "TestStyle.css");
			AssertEquals("HtmlStyleSheet", "TestStyle.css", Wrapper.HtmlStyleSheet);
		}

		[TestDate(1985, 10, 14)]
		public void TestCurrentDate()
		{
			AssertEquals("CurrentDate", ZDateTime.Now.ToShortDateString(), Wrapper.CurrentDate);
		}

		public void TestContactName()
		{
			Assert("Pre-condition", Wrapper.ContactName.IsEmpty);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "MILK o'tea";
			IncidentReq.INC_OC_ReportedBy = contact.PK;

			AssertEquals("ContactName", "Milk O'Tea", Wrapper.ContactName);
		}

		public void TestContactEmail()
		{
			Assert("Pre-condition", Wrapper.ContactEmail.IsEmpty);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test@cw1.com";
			IncidentReq.INC_OC_ReportedBy = contact.PK;

			AssertEquals("test@cw1.com", Wrapper.ContactEmail);
		}

		public void TestContactSalutation()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "sergey gordok";
			IncidentReq.INC_OC_ReportedBy = contact.PK;

			AssertEquals("ContactSalutation", "Sergey Gordok", Wrapper.ContactSalutation);

			contact.OC_Salutation = "Sir Sergey";
			AssertEquals("ContactSalutation", "Sir Sergey", Wrapper.ContactSalutation);
		}

		public void TestOrgCode()
		{
			Assert("Pre-condition", Wrapper.OrgCode.IsEmpty);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.Header.OH_Code = "FRMG";
			IncidentReq.INC_OC_ReportedBy = contact.PK;
			Factory.Save();

			AssertEquals("OrgCode", "FRMG", Wrapper.OrgCode);
		}

		public void TestCompanyName()
		{
			Assert("Pre-condition", Wrapper.CompanyName.IsEmpty);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.Header.OH_FullName = "Formag LTD";
			IncidentReq.INC_OC_ReportedBy = contact.PK;
			AssertEquals("CompanyName", "Formag LTD", Wrapper.CompanyName);
		}

		public void TestWorkplaceLocation()
		{
			Assert("Pre-condition", Wrapper.WorkplaceLocation.IsEmpty);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.Header.MainAddress.OA_Code = "Sydney, Australia";
			IncidentReq.INC_OC_ReportedBy = contact.PK;
			AssertEquals("WorkplaceLocation", "Sydney, Australia", Wrapper.WorkplaceLocation);
		}

		public void TestCurrentCompanyName()
		{
			AssertEquals("CurrentCompanyName", GlbCompany.CurrentCompany.GC_Name, Wrapper.CurrentCompanyName);

			GlbCompany.CurrentCompany.GC_Name = "Zaporojez Limited";
			AssertEquals("CurrentCompanyName", "Zaporojez Limited", Wrapper.CurrentCompanyName);
		}

		public void TestCurrentCompanyOrgProxyName()
		{
			AssertEquals("CurrentCompanyOrgProxyName", GlbCompany.CurrentCompany.OrgProxy.OH_FullName, Wrapper.CurrentCompanyOrgProxyName);

			OrgHeader testOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			testOrgProxy.OH_Code = "YYYY";
			testOrgProxy.OH_FullName = "TEST ORG";
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = testOrgProxy.PK;
			Factory.Save();
			AssertEquals("CurrentCompanyOrgProxyName", "TEST ORG", Wrapper.CurrentCompanyOrgProxyName);
		}

		public void TestCurrentCompanyWebSite()
		{
			AssertEquals("CurrentCompanyWebSite", GlbCompany.CurrentCompany.GC_WebAddress, Wrapper.CurrentCompanyWebSite);
			GlbCompany.CurrentCompany.GC_WebAddress = "www.ZaporojezLimited.com.ua";
			AssertEquals("CurrentCompanyWebSite", "www.ZaporojezLimited.com.ua", Wrapper.CurrentCompanyWebSite);
		}

		public void TestIncidentNumber()
		{
			AssertEquals("IncidentNumber", IncidentReq.INC_IncidentNumber, Wrapper.IncidentNumber);

			IncidentReq.INC_IncidentNumber = "777777";
			AssertEquals("IncidentNumber", "777777", Wrapper.IncidentNumber);
		}

		public void TestClientReferenceNumber()
		{
			AssertEquals("Incident Client Reference Number", IncidentReq.INC_ClientReference, Wrapper.ClientReferenceNumber);

			IncidentReq.INC_ClientReference = "SR00024802";
			AssertEquals("Incident Client Reference Number", "SR00024802", Wrapper.ClientReferenceNumber);
		}

		public void TestIncidentSummary()
		{
			Assert("Pre-condition", Wrapper.Summary.IsEmpty);

			IncidentReq.INC_Summary = "blah blah blah";
			AssertEquals("Incident Summary", "blah blah blah", Wrapper.Summary);
		}

		public void TestIncidentDetailedDescription()
		{
			Assert("Pre-condition", Wrapper.DetailedDescription.IsEmpty);

			IncidentReq.INC_Details = "blah blah blah";
			AssertEquals("Incident Detailed Description", "blah blah blah", Wrapper.DetailedDescription);
		}

		public void TestCriticality()
		{
			Assert("Pre-condition", Wrapper.Criticality.IsEmpty);
			IncidentReq.INC_Criticality = "CR3";
			AssertEquals("Incident criticality", "CR3", Wrapper.Criticality);
		}

		public void TestIncidentLinkURL()
		{
			EDIDataRegistry.Instance.GlowPortalRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://www.cw1.com/");
			AssertEquals($"https://www.cw1.com/INC/Desktop#/formFlow/688ed765-2824-48ff-9f48-9aca459979bd/{IncidentReq.PK}", Wrapper.IncidentLinkURL);
		}

		#region Implementation

		DocIncidentRequest Wrapper
		{
			get { return (DocIncidentRequest)base.Wrappers[0]; }
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var incidentRequest = Factory.NewWithValidTestData<IncidentRequest>();
			return new DocumentWrapper[] { DocIncidentRequest.New(incidentRequest, Factory) };
		}

		IncidentRequest IncidentReq => Wrapper.WrappedObject as IncidentRequest;

		#endregion Implementation
	}
}
