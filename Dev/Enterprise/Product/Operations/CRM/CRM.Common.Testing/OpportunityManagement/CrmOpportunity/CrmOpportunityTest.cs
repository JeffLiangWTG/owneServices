using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.CRM.Common.Testing
{
	[TestedType(typeof(CrmOpportunity))]
	public class CrmOpportunityTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var opportunity = Factory.New<CrmOpportunity>();
			opportunity.COP_OpportunityID = "OpportunityID";
			AssertEquals("Opportunity (OpportunityID)", opportunity.HumanReadableName);
			opportunity.COP_OpportunityID = "";
			AssertEquals("Opportunity", opportunity.HumanReadableName);
		}

		public void TestHumanReadableShortcutName()
		{
			var opportunity = Factory.New<CrmOpportunity>();

			opportunity.COP_OpportunityID = "blah";
			AssertEquals("blah", opportunity.HumanReadableShortcutName);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CODE";
			opportunity.COP_OH_Organization = org.PK;
			AssertEquals("blah - CODE", opportunity.HumanReadableShortcutName);

			opportunity.COP_OpportunityName = "description";
			AssertEquals("blah - CODE - description", opportunity.HumanReadableShortcutName);

			opportunity.COP_OH_Organization = ZGuid.Empty;
			AssertEquals("blah - description", opportunity.HumanReadableShortcutName);
		}

		#region Delete

		public void TestDelete()
		{
			CrmOpportunityProcessTasks task = (CrmOpportunityProcessTasks)Opportunity.WorkflowItems.AddNew();
			Opportunity.Delete();
			AssertEquals("No tasks on Opportunity", 0, Opportunity.WorkflowItems.Count);
			AssertEquals("Opp Deleted", true, Opportunity.IsDeleted);
			AssertEquals("Dependent Task Deleted", true, task.IsDeleted);
		}

		#endregion

		[TestUtcOffset(5, 30, 0)]
		public void TestCalendarReminder()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Zubin Appoo";
			staff.GS_EmailAddress = "zubin.appoo@cargowise.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "My Test Org";
			org.MainAddress.OA_Address1 = "Some Street";
			org.OH_RL_NKClosestPort = "INBOM";
			var opportunity = Factory.NewWithValidTestData<CrmOpportunity>();
			opportunity.COP_OpportunityID = "9876";
			opportunity.COP_OH_Organization = org.PK;
			opportunity.COP_NextFollowUp = new ZDateTimeOffset(2024, 7, 9, 9, 0, 0);
			opportunity.COP_GS_NKSalesPerson = staff.GS_Code;
			Factory.Save();

			var log = opportunity.Logs.AddNew();

			var reminder = opportunity.GetNewReminder(ZDateTime.Empty, opportunity.COP_NextFollowUp.ToUtcZDateTime());
			AssertEquals("Next follow up for Opportunity " + opportunity.COP_OpportunityID + " - My Test Org", reminder.Subject);
			AssertEquals("Next follow up for Opportunity " + opportunity.COP_OpportunityID + " - My Test Org", reminder.Body);
			var expectedUrl = GlowRegistry.Instance.GlowPortalsUri.Value.TrimEnd('/') + $"/goto/OpportunityEdit?entityPK={opportunity.PK}'";
			AssertStartsWith("HTML body should contain valid hyperlink", $"<HTML><HEAD><TITLE></TITLE></HEAD><BODY>Next follow up for Opportunity <a href='{expectedUrl}", reminder.HtmlBody);
			AssertEndsWith("HTML body should contain hyperlink text", $"'>{opportunity.COP_OpportunityID} - My Test Org</a></BODY></HTML>", reminder.HtmlBody);
			AssertEquals("zubin.appoo@cargowise.com", reminder.Recipients[0].Email);
			AssertEquals("Zubin Appoo", reminder.Recipients[0].Name);
			AssertEquals("SOME STREET MH INDIA", reminder.Location);
			AssertEquals(new ZDateTime(2024, 7, 9, 3, 30, 0), reminder.UTCDateFrom);

			//test cancellation
			opportunity.COP_NextFollowUp = ZDateTimeOffset.Empty;
			Factory.Save();

			reminder = opportunity.GetNewReminder(new ZDateTime(2024, 7, 9, 3, 30, 0), opportunity.COP_NextFollowUp.ToZDateTime());
			AssertEquals("Next follow up has been canceled for Opportunity " + opportunity.COP_OpportunityID + " - My Test Org", reminder.Subject);
			AssertEquals("Next follow up has been canceled for Opportunity " + opportunity.COP_OpportunityID + " - My Test Org", reminder.Body);
			AssertStartsWith("HTML body should contain valid hyperlink", $"<HTML><HEAD><TITLE></TITLE></HEAD><BODY>Next follow up has been canceled for Opportunity <a href='{expectedUrl}", reminder.HtmlBody);
			AssertEndsWith("HTML body should contain hyperlink text", $"'>{opportunity.COP_OpportunityID} - My Test Org</a></BODY></HTML>", reminder.HtmlBody);
			AssertEquals(CargoWise.Services.Calendar.ReminderType.Cancellation, reminder.ReminderType);
		}

		public void TestGlowLink()
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/"))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "My Test Org";
				var opportunity = Factory.NewWithValidTestData<CrmOpportunity>();
				opportunity.COP_OH_Organization = org.PK;

				var expectedGlowLink = "https://localhost/goto/OpportunityEdit?entityPK=" + opportunity.PK.ToString();
				AssertEquals("GLOW link", expectedGlowLink, opportunity.GlowLink);

				var linkText = $"{opportunity.COP_OpportunityID} - {opportunity.Organization?.OH_FullName}";
				AssertEquals("GLOW link", $"<a href='{expectedGlowLink}'>{linkText}</a>", opportunity.GlowLinkHtml);
			}
		}

		public void TestIsRestrictedForCurrentUser_OpportunityIsRestricted()
		{
			var controllerUser = Factory.New<GlbStaff>();
			controllerUser.GS_Code = "UR1";
			controllerUser.GS_LoginName = "User1";
			controllerUser.GS_IsController = true;

			var restrictedUser = Factory.New<GlbStaff>();
			restrictedUser.GS_Code = "UR2";
			restrictedUser.GS_LoginName = "User2";

			var authorizedUser = Factory.New<GlbStaff>();
			authorizedUser.GS_Code = "UR3";
			authorizedUser.GS_LoginName = "User3";

			Factory.Save();

			var opportunity = Factory.NewWithValidTestData<CrmOpportunity>();
			opportunity.COP_IsRestricted = true;
			var restriction = opportunity.EntityStaffRestrictions.AddNew();
			restriction.ESR_GS_NKStaff = authorizedUser.GS_Code;
			restriction.ESR_ParentTableCode = "COP";
			restriction.ESR_ParentID = opportunity.PK;
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(controllerUser.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("Controller is not restricted.", false, opportunity.IsRestrictedForCurrentUser());
			}
			using (EnvProxy.Instance.SetTemporaryUserContext(authorizedUser.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("Authorized user is not restricted.", false, opportunity.IsRestrictedForCurrentUser());
			}
			using (EnvProxy.Instance.SetTemporaryUserContext(restrictedUser.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("User without access is restricted.", true, opportunity.IsRestrictedForCurrentUser());
			}
		}

		public void TestIsRestrictedForCurrentUser_OpportunityIsNotRestricted()
		{
			var controllerUser = Factory.New<GlbStaff>();
			controllerUser.GS_Code = "UR1";
			controllerUser.GS_LoginName = "User1";
			controllerUser.GS_IsController = true;

			var restrictedUser = Factory.New<GlbStaff>();
			restrictedUser.GS_Code = "UR2";
			restrictedUser.GS_LoginName = "User2";

			var authorizedUser = Factory.New<GlbStaff>();
			authorizedUser.GS_Code = "UR3";
			authorizedUser.GS_LoginName = "User3";

			Factory.Save();

			var opportunity = Factory.NewWithValidTestData<CrmOpportunity>();
			opportunity.COP_IsRestricted = false;

			var restriction = opportunity.EntityStaffRestrictions.AddNew();
			restriction.ESR_GS_NKStaff = authorizedUser.GS_Code;
			restriction.ESR_ParentTableCode = "COP";
			restriction.ESR_ParentID = opportunity.PK;
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(controllerUser.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("Controller is not restricted.", false, opportunity.IsRestrictedForCurrentUser());
			}
			using (EnvProxy.Instance.SetTemporaryUserContext(authorizedUser.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("Authorized user is not restricted.", false, opportunity.IsRestrictedForCurrentUser());
			}
			using (EnvProxy.Instance.SetTemporaryUserContext(restrictedUser.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertEquals("User without access is not restricted.", false, opportunity.IsRestrictedForCurrentUser());
			}
		}

		#region Implementation

		OrgHeader Organisation
		{
			get
			{
				if (organisation == null)
				{
					organisation = Factory.NewWithValidTestData<OrgHeader>();
				}
				return organisation;
			}
		}
		OrgHeader organisation;

		CrmOpportunity Opportunity
		{
			get
			{
				if (opportunity == null)
				{
					opportunity = Factory.New<CrmOpportunity>();
					opportunity.COP_OH_Organization = Organisation.PK;
				}
				return opportunity;
			}
		}
		CrmOpportunity opportunity;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CrmOpportunity>();
		}

		#endregion
	}
}
