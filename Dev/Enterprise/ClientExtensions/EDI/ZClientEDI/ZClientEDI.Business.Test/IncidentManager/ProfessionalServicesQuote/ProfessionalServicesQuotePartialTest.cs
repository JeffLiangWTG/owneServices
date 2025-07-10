using System;
using System.Data;
using System.Text;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Testing;
using Enterprise.Environment;
using Enterprise.Interop.OutlookIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(ProfessionalServicesQuote))]
	public class ProfessionalServicesQuotePartialTest : IncidentMainBaseTestCase
	{
		public void TestClientCodeAndName()
		{
			AssertNull("Precondition: Client should be null.", Incident.Client);
			AssertEquals("Precondition: ClientCode should be empty.", "", Incident.ClientCode);
			AssertEquals("Precondition: ClientName should be empty.", "", Incident.ClientName);

			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_FullName = "New Client";
			client.OH_Code = "ABCXYZ";
			Incident.IM_OH_Client = client.PK;

			AssertEquals("ClientCode", "ABCXYZ", Incident.ClientCode);
			AssertEquals("ClientName", "New Client", Incident.ClientName);
		}

		public void TestBusinessObjectDefaults()
		{
			AssertEquals("IM_IncidentNumber.ReadOnly", true, Incident.IM_IncidentNumberInfo.ReadOnly);
			AssertEquals("IM_CloseTimeUtc.ReadOnly", true, Incident.IM_CloseTimeUtcInfo.ReadOnly);
			AssertEquals("IM_CallbackByInfo.ReadOnly", true, Incident.IM_CallbackByInfo.ReadOnly);
			AssertEquals("IM_Status", ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.Quote, Incident.IM_Status);
			AssertEquals("CanAssignToPerson should be true", true, Incident.CanAssignToPerson);
			AssertEquals("CanCloseIncident should be true", true, Incident.CanCloseIncident);
			AssertEquals("CanCancelIncident should be true", true, Incident.CanCancelIncident);
			AssertEquals("IM_IsClosed", false, Incident.IsClosed);
			AssertEquals("IM_CloseTimeUtc", ZDateTime.Empty, Incident.IM_CloseTimeUtc);
			AssertEquals("IM_ResolutionCode", GetIM_ResolutionCodeDatabaseDefault(Incident), Incident.IM_ResolutionCode);
			AssertEquals("IM_Details", ORtfTextUtil.EmptyRtfByteArray, Incident.IM_Details);
			AssertEquals("IM_Priority", IncidentConstants.Priority.Medium, Incident.IM_Priority);
			AssertEquals("IM_ChargableWork", true, Incident.IM_ChargableWork);
			AssertEquals("IM_ChargableWorkYesSelection", true, Incident.IM_ChargableWorkYesSelection);
			AssertEquals("IM_ChargableWorkNoSelection", false, Incident.IM_ChargableWorkNoSelection);
			AssertEquals("IM_OA_BranchAddress_ZAddress.DefaultAddressType", AddressType.OFC, Incident.IM_OA_BranchAddress_ZAddress.DefaultAddressType);
		}

		public void TestOriginalIM_Status()
		{
			AssertEquals("Precondition: IM_Status should be PSQuote", ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.Quote, Incident.IM_Status);
			AssertEquals("Precondition: OriginalIM_Status should be empty", "", Incident.OriginalIM_Status);

			Incident.IM_Status = IncidentConstants.IncidentStatus.Deferred;
			AssertEquals("IM_Status", IncidentConstants.IncidentStatus.Deferred, Incident.IM_Status);
			AssertEquals("OriginalIM_Status", "", Incident.OriginalIM_Status);

			Incident.OnLoaded();
			AssertEquals("IM_Status", IncidentConstants.IncidentStatus.Deferred, Incident.IM_Status);
			AssertEquals("OriginalIM_Status", IncidentConstants.IncidentStatus.Deferred, Incident.OriginalIM_Status);
		}

		public void TestIM_IncidentTypeInfo()
		{
			Incident.FillWithValidTestData();
			AssertEquals("IM_IncidentTypeInfo.ReadOnly", false, Incident.IM_IncidentTypeInfo.ReadOnly);

			Factory.Save();
			AssertEquals("IM_IncidentTypeInfo.ReadOnly", true, Incident.IM_IncidentTypeInfo.ReadOnly);
		}

		public void TestIM_GS_CurrentlyAssignedTo()
		{
			Incident.IM_Status = IncidentConstants.IncidentStatus.Deferred;
			Incident.IM_GS_NKAssignedToCurrent = "XYZ";
			Incident.IM_Status = IncidentConstants.IncidentStatus.Unassigned;
			AssertEquals("Assigned person should be empty now", true, Incident.IM_GS_NKAssignedToCurrent.IsEmpty);

			Incident.IM_GS_NKAssignedToCurrent = "XYZ";
			AssertEquals("Status should now be 'Assigned'", IncidentConstants.IncidentStatus.Assigned, Incident.IM_Status);
		}

		public void TestIsReopening()
		{
			Incident.FillWithValidTestData();
			AssertEquals("Should not be re-opened", false, Incident.IsReopening);

			Incident.IM_Status = IncidentConstants.IncidentStatus.Closed;
			AssertEquals("Should not be re-opened after close", false, Incident.IsReopening);

			Incident.OnSaved(true);
			Incident.IM_Status = IncidentConstants.IncidentStatus.Unassigned;
			AssertEquals("Should NOW be re-opened", true, Incident.IsReopening);
		}

		public void TestPostChangeEvents()
		{
			AssertEquals("Precondition: There should be no logs.", null, GetLatestLog(Incident));

			Incident.IM_GG_Team = GetAGlbGroup().PK;
			Incident.IM_GS_NKAssignedToCurrent = ZString.Empty;
			Factory.Save();
			AssertLog(GetLatestLog(Incident), Events.StatusChange.Code, "'' changed to 'QUO'");

			Thread.Sleep(5);

			Incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			Incident.IM_Status = IncidentConstants.IncidentStatus.AwaitingDevelopmentWork;
			Factory.Save();
			StmALogDependentCollection logs = GetLogsSortedByDateDescending(Incident);
			AssertLogsContain(logs, Events.HoldAwaiting.Code, Env.CurrentUser.FullName);
			AssertLogsContain(logs, Events.StatusChange.Code, "'QUO' changed to 'ADW'");

			Thread.Sleep(5);

			Incident.IM_Status = IncidentConstants.IncidentStatus.FinishedDeployedAndPendingVerification;
			Factory.Save();
			logs = GetLogsSortedByDateDescending(Incident);
			AssertLogsContain(logs, Events.Delivered.Code, Env.CurrentUser.FullName);
			AssertLogsContain(logs, Events.StatusChange.Code, "'ADW' changed to 'FIX'");

			Thread.Sleep(5);

			Incident.IM_Status = IncidentConstants.IncidentStatus.Closed;
			Factory.Save();
			logs = GetLogsSortedByDateDescending(Incident);
			AssertLogsContain(logs, Events.IncidentClosed.Code, Env.CurrentUser.FullName);
			AssertLogsContain(logs, Events.StatusChange.Code, "'FIX' changed to 'CLO'");

			Thread.Sleep(5);

			Incident.IM_Status = IncidentConstants.IncidentStatus.FinishedDeployedAndPendingVerification;
			Factory.Save();
			logs = GetLogsSortedByDateDescending(Incident);
			AssertLogsContain(logs, Events.IncidentReopened.Code, Env.CurrentUser.FullName);
			AssertLogsContain(logs, Events.StatusChange.Code, "'CLO' changed to 'FIX'");

			Thread.Sleep(5);

			Incident.IM_Status = IncidentConstants.IncidentStatus.Cancelled;
			Factory.Save();
			logs = GetLogsSortedByDateDescending(Incident);
			AssertLogsContain(logs, Events.JobClose.Code, Env.CurrentUser.FullName);
			AssertLogsContain(logs, Events.StatusChange.Code, "'FIX' changed to 'CAN'");

			Thread.Sleep(5);

			Incident.IM_Status = IncidentConstants.IncidentStatus.InProgress;
			Factory.Save();
			logs = GetLogsSortedByDateDescending(Incident);
			AssertLogsContain(logs, Events.IncidentReopened.Code, Env.CurrentUser.FullName);
			AssertLogsContain(logs, Events.StatusChange.Code, "'CAN' changed to 'INP'");

			Thread.Sleep(5);

			Incident.IM_Status = "xxx";
			Factory.Save();
			logs = GetLogsSortedByDateDescending(Incident);
			AssertLogsContain(logs, Events.EditedARecord.Code, Env.CurrentUser.FullName);
			AssertLogsContain(logs, Events.StatusChange.Code, "'INP' changed to 'xxx'");

			Thread.Sleep(5);

			Incident.IM_Status = IncidentConstants.IncidentStatus.AwaitingSchedulingTeam;
			Factory.Save();
			logs = GetLogsSortedByDateDescending(Incident);
			AssertLogsContain(logs, Events.HoldAwaiting.Code, Env.CurrentUser.FullName);
			AssertLogsContain(logs, Events.StatusChange.Code, "'xxx' changed to 'WST'");

			Thread.Sleep(5);

			Incident.IM_GS_NKAssignedToCurrent = ZString.Empty;
			Factory.Save();
			AssertLog(GetLatestLog(Incident), Events.AssignedUserChanged.Code, "");

			Thread.Sleep(5);

			Incident.IM_Description = "xxx";
			Factory.Save();
			AssertLog(GetLatestLog(Incident), Events.EditedARecord.Code, "");

			Thread.Sleep(5);

			Incident.IM_Status = IncidentConstants.IncidentStatus.AwaitingTeamScheduler;
			Factory.Save();
			logs = GetLogsSortedByDateDescending(Incident);
			AssertLogsContain(logs, Events.HoldAwaiting.Code, "");
			AssertLogsContain(logs, Events.StatusChange.Code, "'WST' changed to 'ATS'");
		}

		public void TestContactResetOnClientChange()
		{
			Incident.IM_OA_BranchAddress = ZGuid.Empty;
			AssertEquals("Contact should be ReadOnly", true, Incident.IM_OC_ContactInfo.ReadOnly);

			ZGuid newGuid = ZGuid.NewZGuid();
			Incident.IM_OC_Contact = newGuid;
			AssertEquals("Contact should not be null", newGuid, Incident.IM_OC_Contact);

			Incident.IM_OA_BranchAddress = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			AssertEquals("Client has changed, Contact should no longer be selected", true, Incident.IM_OC_Contact.IsEmpty);
			AssertEquals("Contact should now be writable", false, Incident.IM_OC_ContactInfo.ReadOnly);
		}

		public void TestIM_OA_BranchAddressSetsIM_OH_Client()
		{
			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_FullName = "New Client";

			OrgAddress address = Factory.New<OrgAddress>();
			address.OA_OH = client.PK;

			Incident.IM_OA_BranchAddress = address.PK;
			AssertEquals("IM_OA_BranchAddress", address.PK, Incident.IM_OA_BranchAddress);
			AssertEquals("IM_OH_Client", client.PK, Incident.IM_OH_Client);
			AssertEquals("ClientName", "New Client", Incident.ClientName);
			AssertEquals("Client", client, Incident.Client);

			Incident.IM_OA_BranchAddress = ZGuid.Empty;
			AssertEquals("IM_OA_BranchAddress", ZGuid.Empty, Incident.IM_OA_BranchAddress);
			AssertEquals("IM_OH_Client", ZGuid.Empty, Incident.IM_OH_Client);
			AssertEquals("ClientName", "", Incident.ClientName);
			AssertEquals("Client", null, Incident.Client);
		}

		public void TestCheckIM_Description()
		{
			Assert("Precondition: IM_Description should be empty", Incident.IM_Description.IsEmpty);
			AssertEquals("Precondition: IM_Details should be empty", ORtfTextUtil.EmptyRtfByteArray, Incident.IM_Details);

			Incident.IM_Details = Encoding.UTF8.GetBytes(Env.CurrentUser.InitialsAndDateTime + "xxxxxxxxxxx\nyyy");
			AssertEquals("IM_Description should be the first line of IM_Details, without the initials/datetime", "xxxxxxxxxxx", Incident.IM_Description);

			Incident.IM_Description = "abcd";
			Incident.IM_Details = ORtfTextUtil.EmptyRtfByteArray;
			AssertEquals("IM_Description", "abcd", Incident.IM_Description);
		}

		public void TestAssignedUserChangedEvent()
		{
			Incident.IM_GG_Team = GetAGlbGroup().PK;
			//			Incident.IM_GS_NKAssignedToCurrent = Env.CurrentUser.PK;
			Incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			var loadedIncident = new BusinessObjectFactory().Load<DummyIncidentMain>(Incident.PK);

			StmALog[] logs = loadedIncident.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AssignedUserChanged.Code));
			AssertEquals("There should be one AssignedUserChanged event", 1, logs.Length);

			loadedIncident.IM_Description = "ABCDEFGHIJ";
			loadedIncident.Factory.Save();

			logs = loadedIncident.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AssignedUserChanged.Code));
			AssertEquals("There should be no new AssignedUserChanged event", 1, logs.Length);

			loadedIncident.IM_GS_NKAssignedToCurrent = ZString.Empty;
			loadedIncident.Factory.Save();

			logs = loadedIncident.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AssignedUserChanged.Code));
			AssertEquals("There should be a new AssignedUserChanged event", 2, logs.Length);
		}

		public void TestStatusChangesToUnassignedWhenUserIsRemoved()
		{
			Incident.IM_Status = IncidentConstants.IncidentStatus.InProgress;
			Incident.IM_GS_NKAssignedToCurrent = ZString.Empty;
			AssertEquals(Incident.IM_Status, IncidentConstants.IncidentStatus.InProgress);
			Incident.IM_Status = IncidentConstants.IncidentStatus.Assigned;
			Incident.IM_GS_NKAssignedToCurrent = ZString.Empty;
			AssertEquals(Incident.IM_Status, IncidentConstants.IncidentStatus.Unassigned);
		}

		#region Test Sending Emails

		[TestDate(2012, 4, 2, 11, 15, 0)]
		public void TestNotifyMailSent()
		{
			AssertEquals("Precondition: There should be no logs yet", null, GetLatestLog(Incident));

			Incident.FillWithValidTestData();
			Factory.Save();
			AssertEquals("IM_IsEmailSent", false, Incident.IM_IsEmailSent);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			Incident.NotifyMailSent();
			AssertEquals("IM_IsEmailSent", true, Incident.IM_IsEmailSent);

			Factory.Save();

			StmALog log = GetLatestLog(Incident);
			AssertEquals("IncidentEmailSent event should have been logged.", Events.IncidentEmailSent.Code, log.SL_SE_NKEvent);
			AssertEquals("Reference", "", log.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			Incident.NotifyMailSent("Reference");
			Factory.Save();

			log = GetLatestLog(Incident);
			AssertEquals("IncidentEmailSent event should have been logged.", Events.IncidentEmailSent.Code, log.SL_SE_NKEvent);
			AssertEquals("Reference", "Reference", log.SL_Reference);
		}

		[ExpectExceptionMessage(typeof(ApplicationException), "Changes have been made to this Incident. You must save before sending an Email.")]
		public void TestGetNewOutlookMailItemThrowsExceptionWhenHasChanges()
		{
			Incident.IM_Status = IncidentConstants.IncidentStatus.Assigned;
			Incident.IM_SubCategory = "splat";
			//			Incident.IM_GS_NKAssignedToCurrent = Env.CurrentUser.PK;
			Incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			Incident.GetNewOutlookMailItem();
		}

		public void TestGetNewOutlookMailItemDetailsNotEmpty()
		{
			TestGetNewOutlookMailItemDetails("Some Description", Encoding.ASCII.GetBytes("Details isn't empty"), "Details isn't empty");
		}

		public void TestGetNewOutlookMailItemDetailsEmpty()
		{
			TestGetNewOutlookMailItemDetails("Some Description", ZBlob.Empty, "Some Description");
		}

		void TestGetNewOutlookMailItemDetails(string iM_Description, ZBlob iM_Details, string expectedDetailsLine)
		{
			Incident.FillWithValidTestData();

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_FullName = "New Client";

			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Joe";
			contact.OC_Email = "joe@eoj";

			OrgAddress address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Phone = "12345";
			address.OA_Fax = "98765";
			address.OA_OH = client.PK;
			address.OA_Code = "Some Crap";

			GlbStaff newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_LoginName = "newguy";
			newStaff.GS_FullName = "New Guy";
			newStaff.GS_EmailAddress = "newguy@edi.com.au";

			var currentStaff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "currentguy@edi.com.au";

			Incident.IM_Status = IncidentConstants.IncidentStatus.Assigned;
			Incident.IM_GS_NKAssignedToCurrent = newStaff.GS_Code;
			Incident.IM_OA_BranchAddress = address.PK;
			Incident.IM_OC_Contact = contact.PK;
			Incident.IM_Description = iM_Description;
			Incident.IM_Details = iM_Details;

			Factory.Save();

			DummyOutlookMailItem mailItem = (DummyOutlookMailItem)Incident.GetNewOutlookMailItem();
			AssertNotNull("MailItem should not be null", mailItem);
			AssertEquals("MailItem.Recipient", "joe@eoj", mailItem.Recipients[0]);
			AssertEquals("MailItem.Subject", "Quotation #" + Incident.IM_IncidentNumber + " (" + iM_Description + ")", mailItem.Subject);
			AssertEquals("MailItem.ReplyRecipient", "currentguy@edi.com.au", mailItem.ReplyRecipient);

			contact.OC_Salutation = "Bob";
			Factory.Save();

			mailItem = (DummyOutlookMailItem)Incident.GetNewOutlookMailItem();
			AssertNotNull("MailItem should not be null", mailItem);
			AssertEquals("MailItem.Recipient", "joe@eoj", mailItem.Recipients[0]);
			AssertEquals("MailItem.Subject", "Quotation #" + Incident.IM_IncidentNumber + " (" + iM_Description + ")", mailItem.Subject);
			AssertEquals("MailItem.ReplyRecipient", "currentguy@edi.com.au", mailItem.ReplyRecipient);
		}

		public void TestReplaceMailTags()
		{
			//			Incident.IM_GS_NKAssignedToCurrent = Env.CurrentUser.PK;
			Incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			Incident.IM_OH_Client = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Incident.IM_OC_Contact = Factory.LoadTop1<OrgContact>(new ZQuery()).PK;

			string text = "<LoginFullName> <LoginEmail>";
			AssertEquals("ReplaceEmailTags()", GlbStaff.CurrentUser.GS_FullName.CapitaliseFirstLettersOfWords() + " " + GlbStaff.CurrentUser.GS_EmailAddress, Incident.ReplaceEmailTags(text, false));
			AssertEquals("ReplaceEmailTags()", IncidentConstants.SupportDisplayName + " " + SupportIncidentLookups.SupportEmailAddress, Incident.ReplaceEmailTags(text, true));

			GlbStaff.CurrentUser.GS_FullName = "steve o'callaghan";
			OrgContact contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "PAUL O'HAGAN";
			Incident.IM_OC_Contact = contact.PK;
			AssertEquals("ReplaceEmailTags()", "Steve O'Callaghan|Paul O'Hagan", Incident.ReplaceEmailTags("<LoginFullName>|<ContactName>", false));
		}

		#endregion

		#region Test User Initiated Actions

		public void TestMarkIncidentAsFixed()
		{
			AssertEquals("Precondition: IsBeingMarkedAsFixed should be false", false, Incident.IsBeingMarkedAsFixed);

			Incident.IM_Details = Encoding.UTF8.GetBytes("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
			//			Incident.IM_GS_NKAssignedToCurrent = Env.CurrentUser.PK;
			Incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			Incident.MarkIncidentAsFixed("rrr");
			AssertEquals("IsBeingMarkedAsFixed", true, Incident.IsBeingMarkedAsFixed);
			AssertEquals("IM_ResolutionCode", "rrr", Incident.IM_ResolutionCode);
		}

		public void TestCloseIncident()
		{
			AssertEquals("Precondition: IsClosing should be false", false, Incident.IsClosing);

			Incident.IM_Details = Encoding.UTF8.GetBytes("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
			Incident.MarkIncidentAsFixed("rrr");
			Incident.CloseIncident();

			AssertEquals("IsClosing", true, Incident.IsClosing);
			CheckDateTime(ZDateTime.Now, Incident.IM_CloseTime);
			CheckDateTime(ZDateTime.UtcNow, Incident.IM_CloseTimeUtc);
		}

		public void TestReopenIncident()
		{
			Incident.FillWithValidTestData();

			AssertEquals("Precondition: IsClosing should be false", false, Incident.IsClosing);
			AssertEquals("Precondition: IsBeingMarkedAsFixed should be false", false, Incident.IsBeingMarkedAsFixed);
			AssertEquals("Precondition: IsReopening should be false", false, Incident.IsReopening);
			AssertEquals("Precondition: IM_Status should be Quote", ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.Quote, Incident.IM_Status);

			Incident.MarkIncidentAsFixed("rrr");
			AssertEquals("IsClosing", false, Incident.IsClosing);
			AssertEquals("IsBeingMarkedAsFixed", true, Incident.IsBeingMarkedAsFixed);
			AssertEquals("IsReopening", false, Incident.IsReopening);
			AssertEquals("IM_Status", IncidentConstants.IncidentStatus.FinishedDeployedAndPendingVerification, Incident.IM_Status);

			Factory.Save();
			AssertEquals("IsClosing", false, Incident.IsClosing);
			AssertEquals("IsBeingMarkedAsFixed", false, Incident.IsBeingMarkedAsFixed);
			AssertEquals("IsReopening", false, Incident.IsReopening);
			AssertEquals("IM_Status", IncidentConstants.IncidentStatus.FinishedDeployedAndPendingVerification, Incident.IM_Status);

			Incident.CloseIncident();
			AssertEquals("IsClosing", true, Incident.IsClosing);
			AssertEquals("IsBeingMarkedAsFixed", false, Incident.IsBeingMarkedAsFixed);
			AssertEquals("IsReopening", false, Incident.IsReopening);
			AssertEquals("IM_Status", IncidentConstants.IncidentStatus.Closed, Incident.IM_Status);

			Factory.Save();
			AssertEquals("IsClosing", false, Incident.IsClosing);
			AssertEquals("IsBeingMarkedAsFixed", false, Incident.IsBeingMarkedAsFixed);
			AssertEquals("IsReopening", false, Incident.IsReopening);
			AssertEquals("IM_Status", IncidentConstants.IncidentStatus.Closed, Incident.IM_Status);

			Incident.ReopenIncident();
			AssertEquals("IsClosing", false, Incident.IsClosing);
			AssertEquals("IsBeingMarkedAsFixed", false, Incident.IsBeingMarkedAsFixed);
			AssertEquals("IsReopening", true, Incident.IsReopening);
			AssertEquals("IM_Status", ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.WorkInProgress, Incident.IM_Status);

			Factory.Save();
			AssertEquals("IsClosing", false, Incident.IsClosing);
			AssertEquals("IsBeingMarkedAsFixed", false, Incident.IsBeingMarkedAsFixed);
			AssertEquals("IsReopening", false, Incident.IsReopening);
			AssertEquals("IM_Status", ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.WorkInProgress, Incident.IM_Status);

			Incident.CancelIncident();
			Factory.Save();
			AssertEquals("IsClosing", false, Incident.IsClosing);
			AssertEquals("IsBeingMarkedAsFixed", false, Incident.IsBeingMarkedAsFixed);
			AssertEquals("IsReopening", false, Incident.IsReopening);
			AssertEquals("IM_Status", IncidentConstants.IncidentStatus.Cancelled, Incident.IM_Status);

			Incident.ReopenIncident();
			AssertEquals("IsClosing", false, Incident.IsClosing);
			AssertEquals("IsBeingMarkedAsFixed", false, Incident.IsBeingMarkedAsFixed);
			AssertEquals("IsReopening", true, Incident.IsReopening);
			AssertEquals("IM_Status", ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.WorkInProgress, Incident.IM_Status);
		}

		#endregion

		#region Test New Bound Properties

		public void TestClientHeader()
		{
			OrgHeader client = Factory.New<OrgHeader>();

			Incident.IM_OH_Client = client.PK;
			AssertEquals("ClientHeader", client.PK, Incident.ClientHeader);
			AssertEquals("ClientHeader should be ReadOnly.", true, Incident.ClientHeaderInfo.ReadOnly);
		}

		public void TestDescriptionHeader()
		{
			Incident.IM_Description = "BLAH";
			AssertEquals("DescriptionHeader", "BLAH", Incident.DescriptionHeader);
			AssertEquals("DescriptionHeader should be ReadOnly.", true, Incident.DescriptionHeaderInfo.ReadOnly);
		}

		public void TestIM_Calc_ContactPhoneAndEmail()
		{
			var contact = Factory.New<OrgContact>();

			contact.OC_Phone = "9876 5432";
			contact.OC_Email = "contact@office.com";

			AssertNull("Precondition: Contact should be null.", Incident.Contact);
			AssertEquals("IM_Calc_ContactEmail", "", Incident.IM_Calc_ContactEmail);
			AssertEquals("IM_Calc_ContactPhone", "", Incident.IM_Calc_ContactPhone);

			Incident.IM_OC_Contact = contact.PK;

			AssertEquals("IM_Calc_ContactEmail", "contact@office.com", Incident.IM_Calc_ContactEmail);
			AssertEquals("IM_Calc_ContactPhone", "9876 5432", Incident.IM_Calc_ContactPhone);
		}

		public void TestIM_TeamDesc()
		{
			AssertEquals("Precondition: Team should be null", null, Incident.Team);
			AssertEquals("IM_TeamDesc", "", Incident.IM_TeamDesc);

			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Desc = "Some Group";

			Incident.IM_GG_Team = group.PK;
			AssertEquals("IM_TeamDesc", "Some Group", Incident.IM_TeamDesc);
		}

		public void TestIM_CurrentlyAssignedToInitials()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "&*(";

			Incident.IM_GS_NKAssignedToCurrent = staff.GS_Code;
			AssertEquals("IM_CurrentlyAssignedToInitials", "&*(", Incident.IM_CurrentlyAssignedToInitials);
		}

		public void TestIM_ClientContractStatus()
		{
			AssertEquals("IM_ClientContractStatus", "Not functional on old incidents.", Incident.IM_ClientContractStatus);
		}

		#endregion

		#region Test IM_Status

		public void TestIM_StatusAssigned()
		{
			DummyIncidentMain incident = GetIncidentMainWithDateAndResolutionCodeSet();

			incident.IM_Status = IncidentConstants.IncidentStatus.Assigned;
			AssertEquals("CanAssignToPerson", true, incident.CanAssignToPerson);
			AssertEquals("CanCloseIncident", true, incident.CanCloseIncident);
			AssertEquals("CanCancelIncident", true, incident.CanCancelIncident);
			AssertEquals("IsClosed", false, incident.IsClosed);
			AssertEquals("IM_CloseTimeUtc", ZDateTime.Empty, incident.IM_CloseTimeUtc);
			AssertEquals("IM_ResolutionCode", GetIM_ResolutionCodeDatabaseDefault(incident), incident.IM_ResolutionCode);
		}

		public void TestIM_StatusUnchanged()
		{
			DummyIncidentMain incident = GetIncidentMainWithDateAndResolutionCodeSet();

			// Everything should be the same as default as the status is the same
			incident.IM_Status = ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.Quote;
			AssertEquals("CanAssignToPerson", true, incident.CanAssignToPerson);
			AssertEquals("CanCloseIncident", true, incident.CanCloseIncident);
			AssertEquals("CanCancelIncident", true, incident.CanCancelIncident);
			AssertEquals("IsClosed", false, incident.IsClosed);
			AssertEquals("IM_CloseTimeUtc", new ZDateTime(2000, 12, 12, 12, 12, 12), incident.IM_CloseTimeUtc);
			AssertEquals("IM_ResolutionCode", "ABC", incident.IM_ResolutionCode);
		}

		public void TestIM_StatusClosed()
		{
			DummyIncidentMain incident = GetIncidentMainWithDateAndResolutionCodeSet();

			incident.IM_Status = IncidentConstants.IncidentStatus.Closed;
			AssertEquals("CanAssignToPerson", false, incident.CanAssignToPerson);
			AssertEquals("CanCloseIncident", false, incident.CanCloseIncident);
			AssertEquals("CanCancelIncident", false, incident.CanCancelIncident);
			AssertEquals("IsClosed", true, incident.IsClosed);
			CheckDateTime(ZDateTime.Now, incident.IM_CloseTime);
			CheckDateTime(ZDateTime.UtcNow, incident.IM_CloseTimeUtc);
			AssertEquals("IM_ResolutionCode", "ABC", incident.IM_ResolutionCode);
		}

		public void TestIM_StatusCancelled()
		{
			DummyIncidentMain incident = GetIncidentMainWithDateAndResolutionCodeSet();

			incident.IM_Status = IncidentConstants.IncidentStatus.Cancelled;
			AssertEquals("CanAssignToPerson", false, incident.CanAssignToPerson);
			AssertEquals("CanCloseIncident", false, incident.CanCloseIncident);
			AssertEquals("CanCancelIncident", false, incident.CanCancelIncident);
			AssertEquals("IsClosed", false, incident.IsClosed);
			AssertEquals("IM_CloseTimeUtc", new ZDateTime(2000, 12, 12, 12, 12, 12), incident.IM_CloseTimeUtc);
			AssertEquals("IM_ResolutionCode", "ABC", incident.IM_ResolutionCode);
		}

		public void TestIM_StatusInProgress()
		{
			Incident.IM_Status = IncidentConstants.IncidentStatus.InProgress;
			AssertEquals("CanAssignToPerson", true, Incident.CanAssignToPerson);
			AssertEquals("CanCloseIncident", true, Incident.CanCloseIncident);
			AssertEquals("CanCancelIncident", true, Incident.CanCancelIncident);
			AssertEquals("IsClosed", false, Incident.IsClosed);
			AssertEquals("IM_CloseTimeUtc", ZDateTime.Empty, Incident.IM_CloseTimeUtc);
			AssertEquals("IM_ResolutionCode", GetIM_ResolutionCodeDatabaseDefault(Incident), Incident.IM_ResolutionCode);
		}

		DummyIncidentMain GetIncidentMainWithDateAndResolutionCodeSet()
		{
			Incident.IM_CloseTimeUtc = new ZDateTime(2000, 12, 12, 12, 12, 12);
			Incident.IM_ResolutionCode = "ABC";
			return Incident;
		}

		ZString GetIM_ResolutionCodeDatabaseDefault(DummyIncidentMain incident)
		{
			return new ZString(incident.IM_ResolutionCodeInfo.DatabaseDefault);
		}

		#endregion

		public override void TestSaveAndDeleteBusinessObject()
		{
			Incident.FillWithValidTestData();
			base.TestSaveAndDeleteBusinessObject();
		}

		public void TestDocManagerInfo()
		{
			AssertEquals("DocManagerInfo.BusinessEntity", Incident, Incident.DocManagerInfo.BusinessEntity);
			AssertEquals("DocManagerInfo.DocManagerCode", "INC", Incident.DocManagerInfo.DocManagerCode);
		}

		public void TestNotes()
		{
			AssertEquals("Notes.GetType()", typeof(IncidentNotes), Incident.Notes.GetType());
		}

		public void TestIM_DetailsAsText()
		{
			Assert("Precondition - Note.IM_DetailsAsText should be empty", Incident.IM_DetailsAsText.IsEmpty);

			Incident.IM_Details = ORtfTextUtil.TextToRtfBytes("Lost Cities");
			AssertEquals("IM_DetailsAsText should return the text with the RTF header stripped off", "Lost Cities", Incident.IM_DetailsAsText);
		}

		public void TestEmailSender()
		{
			AssertEquals("EmailSender should be Env.OutgoingMailManager.", Env.OutgoingMailManager, Env.OutgoingMailManager);
		}

		public void TestLogsAreRemovedWhenSaveFails()
		{
			Incident.IM_GG_Team = GetAGlbGroup().PK;
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STF";
			Incident.IM_GS_NKAssignedToCurrent = staff.GS_Code;
			Incident.ThrowExceptionOnSaving = true;

			try
			{
				Factory.Save();
			}
			catch
			{
			}

			AssertEquals("Logs.GetAllLogs().Count", 0, Incident.Logs.GetAllLogs().Count);

			Incident.ThrowExceptionOnSaving = false;
			Factory.Save();
			AssertEquals("Logs.GetAllLogs().Count", 3, Incident.Logs.GetAllLogs().Count);
		}

		#region TestIJobInvoicingPlugin

		public void TestEditSecurity()
		{
			IJobInvoicingPlugIn testJob = Factory.New<ProfessionalServicesQuote>();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestIJobInvoicingPlugInDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<ProfessionalServicesQuote>();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		#endregion

		#region Implementation

		new DummyIncidentMain Incident
		{
			get { return incident ?? (incident = Factory.New<DummyIncidentMain>()); }
		}

		void CheckDateTime(ZDateTime expectedValue, ZDateTime actualValue)
		{
			Assert("Expected value to be " + expectedValue.ToString() + " (+/- 1 minute) but was " + actualValue.ToString(),
				   ((actualValue >= expectedValue.AddMinutes(-1)) && (actualValue <= expectedValue.AddMinutes(1))));
		}

		GlbGroup GetAGlbGroup()
		{
			return Factory.LoadTop1<GlbGroup>(new ZQuery());
		}

		StmALog GetLatestLog(DummyIncidentMain incident)
		{
			StmALogDependentCollection logs = GetLogsSortedByDateDescending(incident);
			return (logs.Count > 0) ? logs[0] : null;
		}

		StmALogDependentCollection GetLogsSortedByDateDescending(DummyIncidentMain incident)
		{
			StmALogDependentCollection result = incident.Logs.GetAllLogs();
			result.LoadWithMoreFiltering(new ZQuery(new ZQuery(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.NotEqual, Events.AddedARecordToTheSystem.Code)));
			result.Sort(StmALogSchema.SL_PostedTimeUtc.Name, System.ComponentModel.ListSortDirection.Descending);
			return result;
		}

		void AssertLog(StmALog log, string expectedEvent, string expectedReference)
		{
			AssertEquals("Event", expectedEvent, log.SL_SE_NKEvent);
			AssertEquals("SL_Reference", expectedReference, log.SL_Reference);
		}

		void AssertLogsContain(StmALogDependentCollection logs, string expectedEvent, string expectedReference)
		{
			bool foundLog = false;

			for (int i = 0; i < logs.Count; i++)
			{
				if (logs.Count > i && logs[i].SL_SE_NKEvent == expectedEvent && logs[i].SL_Reference == expectedReference)
				{
					foundLog = true;
					break;
				}
			}

			AssertEquals("Log not found.", true, foundLog);
		}

		DummyIncidentMain incident;

		#region class DummyIncidentMain

		class DummyIncidentMain : ProfessionalServicesQuote
		{
			public DummyIncidentMain(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override IOutlookMailItem NewOutlookMailItem()
			{
				return new DummyOutlookMailItem();
			}

			public new string ReplaceEmailTags(string body, bool useSupportAsSignOff)
			{
				return base.ReplaceEmailTags(body, useSupportAsSignOff);
			}

			public override void OnSaving()
			{
				base.OnSaving();

				if (ThrowExceptionOnSaving)
				{
					throw new Exception("OnSaving threw an exception!");
				}
			}

			public bool ThrowExceptionOnSaving;

			protected override string GetNewIncidentNumber()
			{
				return "TEST123";
			}

			#region Exposed ProfessionalServicesQuote Properties

			public new string IncidentTypeName
			{
				get { return base.IncidentTypeName; }
			}

			public new bool CanAssignToPerson
			{
				get { return base.CanAssignToPerson; }
			}

			public new bool CanCancelIncident
			{
				get { return base.CanCancelIncident; }
			}

			public new bool CanCloseIncident
			{
				get { return base.CanCloseIncident; }
			}

			public new bool IsClosed
			{
				get { return base.IsClosed; }
			}

			public new ZString OriginalIM_Status
			{
				get { return base.OriginalIM_Status; }
			}

			public new bool IsReopening
			{
				get { return base.IsReopening; }
			}

			public new bool IsBeingMarkedAsFixed
			{
				get { return base.IsBeingMarkedAsFixed; }
			}

			public new bool IsClosing
			{
				get { return base.IsClosing; }
			}

			#endregion
		}

		#endregion

		#endregion
	}
}
