using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(DocSupportIncident))]
	class DocSupportIncidentTest : DocumentWrapperTestCase
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

			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "MILK o'tea";
			Incident.IM_OC_Contact = contact.PK;

			AssertEquals("ContactName", "Milk O'Tea", Wrapper.ContactName);
		}

		public void TestContactPhoneForDisplay()
		{
			AssertEquals(false, Wrapper.ContactPhoneForDisplay.IsEmpty);

			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Phone = "555 55 55";
			Incident.IM_OC_Contact = contact.PK;
			Factory.Save();
			AssertEquals("ContactPhone", "Dir: 555 55 55", Wrapper.ContactPhoneForDisplay);
		}

		public void TestContactSalutation()
		{
			AssertEquals("Pre-condition", "Dear Client", Wrapper.ContactSalutation);

			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "sergey gordok";
			Incident.IM_OC_Contact = contact.PK;

			AssertEquals("ContactSalutation", "Sergey Gordok", Wrapper.ContactSalutation);

			contact.OC_Salutation = "Sir Sergey";
			AssertEquals("ContactSalutation", "Sir Sergey", Wrapper.ContactSalutation);
		}

		public void TestClientCode()
		{
			Assert("Pre-condition", Wrapper.ClientCode.IsEmpty);

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "FRMG";
			Incident.IM_OH_Client = client.PK;
			Factory.Save();

			AssertEquals("ClientCode", "FRMG", Wrapper.ClientCode);
		}

		public void TestClientName()
		{
			Assert("Pre-condition", Wrapper.ClientName.IsEmpty);

			var client1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			client1.OH_FullName = "Formag LTD";
			Incident.IM_OH_Client = client1.PK;
			AssertEquals("ClientName", "Formag LTD", Wrapper.ClientName);

			var client2Licence = Billing.Business.Test.BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "PRD", true);
			var client2 = client2Licence.Company.Header;
			client2.OH_FullName = "DDD Company Australia";
			client2Licence.ClientCompany.LCC_OH = ZGuid.Empty;
			client2Licence.ClientCompany.LCC_Name = "DDD Company Inc.";
			Incident.IM_OH_Client = client2.PK;
			Incident.IM_LD = client2Licence.Database.PK;
			Incident.IM_LCC = client2Licence.ClientCompany.PK;
			AssertEquals("ClientName", "DDD Company Inc.", Wrapper.ClientName);
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

		public void TestCurrentUserName()
		{
			AssertEquals("CurrentUserName", Env.CurrentUser.FullName, Wrapper.CurrentUserName);

			GlbStaff.CurrentUser.GS_FullName = "Sergey Gordok";
			AssertEquals("CurrentUserName", "Sergey Gordok", Wrapper.CurrentUserName);
		}

		public void TestCurrentUserTitle()
		{
			AssertEquals("CurrentUserTitle", Env.CurrentUser.Title, Wrapper.CurrentUserTitle);

			GlbStaff.CurrentUser.GS_Title = "Serg";
			AssertEquals("CurrentUserTitle", "Serg", Wrapper.CurrentUserTitle);
		}

		public void TestCurrentUserEmailAddress()
		{
			AssertEquals("CurrentUserEmailAddress", Env.CurrentUser.EmailAddress, Wrapper.CurrentUserEmailAddress);

			GlbStaff.CurrentUser.GS_EmailAddress = "bkv85@mail.ru";
			AssertEquals("CurrentUserEmailAddress", "bkv85@mail.ru", Wrapper.CurrentUserEmailAddress);
		}

		public void TestIncidentBranchPhone()
		{
			Assert("IncidentBranchPhone", Wrapper.IncidentBranchPhone.IsEmpty);

			OrgAddress incidentBranchAddress = Factory.NewWithValidTestData<OrgAddress>();
			incidentBranchAddress.OA_Phone = "777777";
			Incident.IM_OA_BranchAddress = incidentBranchAddress.PK;
			AssertEquals("IncidentBranchPhone", "777777", Wrapper.IncidentBranchPhone);
		}

		public void TestIncidentNumber()
		{
			AssertEquals("IncidentNumber", Incident.IM_IncidentNumber, Wrapper.IncidentNumber);

			Incident.IM_IncidentNumber = "777777";
			AssertEquals("IncidentNumber", "777777", Wrapper.IncidentNumber);
		}

		public void TestClientReferenceNumber()
		{
			AssertEquals("Incident Client Reference Number", Incident.IM_ClientIncidentReference, Wrapper.ClientReferenceNumber);

			Incident.IM_ClientIncidentReference = "SR00024802";
			AssertEquals("Incident Client Reference Number", "SR00024802", Wrapper.ClientReferenceNumber);
		}

		public void TestIncidentSummary()
		{
			Assert("Pre-condition", Wrapper.Summary.IsEmpty);

			Incident.IM_Description = "blah blah blah";
			AssertEquals("Incident Summary", "blah blah blah", Wrapper.Summary);
		}

		public void TestIncidentDetailedDescription()
		{
			Assert("Pre-condition", Wrapper.DetailedDescription.IsEmpty);

			Incident.DetailNoteText = "blah blah blah";
			AssertEquals("Incident Detailed Description", "blah blah blah", Wrapper.DetailedDescription);
		}

		public void TestCriticality()
		{
			Assert("Pre-condition", Wrapper.Criticality.IsEmpty);
			Incident.IM_Priority = "CR3";
			AssertEquals("Incident criticality", "CR3", Wrapper.Criticality);
		}

		public void TestDetailedDispositionDescription()
		{
			AssertEquals("Added Awaiting Assignment", Wrapper.DetailedDispositionDescription);
			Incident.IM_Status = SupportIncidentLookups.Status.Working;
			Incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress;
			AssertEquals("Work in Progress / Investigating", Wrapper.DetailedDispositionDescription);
		}

		public void TestResolutionNoteText()
		{
			AssertEquals("", Wrapper.ResolutionNoteText);
			Incident.ResolutionNoteText = "Resolution Note...";
			AssertEquals("\r\n\r\nComments: Resolution Note...", Wrapper.ResolutionNoteText);
		}

		public void TestChargeableWorkBillingNotice()
		{
			AssertEquals("", Wrapper.ChargeableWorkBillingNotice);
			Incident.IM_ChargableWork = true;
			AssertEquals("\r\n\r\nBecause of the service provided this incident is chargeable under your maintenance contract and will be billed at the end of the current billing period.", Wrapper.ChargeableWorkBillingNotice);
		}

		public void TestSupportIncidentCloseType()
		{
			AssertEquals("", Wrapper.SupportIncidentCloseType);

			Incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			Incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, "");
			AssertEquals("Closed with reason: No Response from Client", Wrapper.SupportIncidentCloseType);
		}

		public void TestCurrentAssignedStaffFullNameAndEmailAddress()
		{
			AssertEquals(SupportIncident.SupportDisplayName, Wrapper.CurrentAssignedStaffFullName);
			AssertEquals(SupportIncident.SupportEmailAddress, Wrapper.CurrentAssignedStaffEmailAddress);

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Samuel Wang";
			staff.GS_EmailAddress = "samuel.wang@cargowise.com";
			Incident.IM_GS_NKAssignedToCurrent = staff.GS_Code;

			AssertEquals("Samuel Wang", Wrapper.CurrentAssignedStaffFullName);
			AssertEquals("samuel.wang@cargowise.com", Wrapper.CurrentAssignedStaffEmailAddress);
		}

		public void TestClosureDateText()
		{
			AssertEquals("", Wrapper.ClosureDateText);
			Incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "test message");
			AssertEquals(Incident.ClosureDateText, Wrapper.ClosureDateText);
		}

		public void TestClosingStaffCodeText()
		{
			AssertEquals("", Wrapper.ClosureDateText);
			Incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "test message");
			AssertEquals(Incident.ClosingStaffCodeText, Wrapper.ClosingStaffCodeText);
		}

		public void TestResolutionCommentText()
		{
			AssertEquals("", Wrapper.ResolutionCommentText);
			Incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "test message");
			AssertEquals("test message", Wrapper.ResolutionCommentText);
		}

		public void TestClientReference()
		{
			AssertEquals("", Wrapper.ClientReference);
			Incident.Request.INC_ClientReference = "test message";
			AssertEquals("test message", Wrapper.ClientReference);
		}

		public void TestResolvedToClosedDay()
		{
			AssertEquals("7", Wrapper.ResolvedToClosedDay);
			Incident.IM_Priority = "CR4";
			Incident.IM_Product = "ENT";
			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = Incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = Incident.IM_Product;
			var expireDays = 30;
			productResolutionAndClosureBehaviour.DaysResolvedToClosed = expireDays;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				AssertEquals("30", Wrapper.ResolvedToClosedDay);
			}
		}

		#region Implementation

		DocSupportIncident Wrapper
		{
			get { return (DocSupportIncident)base.Wrappers[0]; }
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			return new DocumentWrapper[] { DocSupportIncident.New(incident, Factory) };
		}

		SupportIncident Incident
		{
			get { return Wrapper.WrappedObject; }
		}

		#endregion
	}
}
