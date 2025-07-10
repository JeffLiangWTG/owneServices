using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business.EmailTemplate;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.Registry;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Test;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;
using WTG.DevTools.Definitions;
using static Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentLookups;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;
using ZArchitectureBusiness = Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncident))]
	public class SupportIncidentTest : IncidentMainBaseTestCase
	{
		public void TestIM_ClosureResolutionChanged()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			registryValue.Cast<IncidentClosureDisposition>().Where(x => x.Code == DispositionList.Constants.Closed.Completed).ForEach(x => x.IsResolution = true);
			registryValue.Cast<IncidentClosureDisposition>().Where(x => x.Code == DispositionList.Constants.Closed.UpgradeDelivered).ForEach(x => x.IsResolution = true);

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-6);
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = DispositionList.Constants.Closed.Resolved;
			incident.IM_ClosureResolution = DispositionList.Constants.Closed.Other;
			incident.IM_ResolveTimeUtc = ZDateTime.UtcNow;
			incident.IM_CloseTimeUtc = ZDateTime.UtcNow.AddDays(-5);
			Factory.Save();
			Assert(incident.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.IncidentClosedCode)).Length == 0);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			incident.IM_ClosureResolution = DispositionList.Constants.Closed.Completed;
			Factory.Save();
			Assert((incident.Lookups.StatusDispositionList[incident.IM_ClosureResolution] as IncidentClosureDisposition).IsResolution);
			Assert("ICL event should not be added because of ResolutionCode", incident.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.IncidentClosedCode)).Length == 0);

			AssertEquals(DispositionList.Constants.Closed.Resolved, incident.IM_ResolutionCode);
			incident.UpdateClosureResolutionForResolvedIncident(DispositionList.Constants.Closed.Cancelled);
			AssertEquals(incident.IM_ResolveTimeUtc, incident.IM_CloseTimeUtc);

			Factory.Save();
			Assert(!(incident.Lookups.StatusDispositionList[incident.IM_ClosureResolution] as IncidentClosureDisposition).IsResolution);
			Assert("ICL event should be added because of non-ResolutionCode", incident.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.IncidentClosedCode)).Length == 1);
		}

		public void TestUpdateClosureResolutionForResolvedIncident()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			registryValue.Cast<IncidentClosureDisposition>().Where(x => x.Code == DispositionList.Constants.Closed.Completed).ForEach(x => x.IsResolution = true);
			registryValue.Cast<IncidentClosureDisposition>().Where(x => x.Code == DispositionList.Constants.Closed.UpgradeDelivered).ForEach(x => x.IsResolution = true);

			EDIDataRegistry.Instance.IncidentClosureDispositions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-5);
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = DispositionList.Constants.Closed.Cancelled;
			incident.IM_ClosureResolution = DispositionList.Constants.Closed.Completed;
			incident.IM_ResolveTimeUtc = ZDateTime.UtcNow;
			incident.IM_CloseTimeUtc = ZDateTime.UtcNow.AddDays(-5);
			Factory.Save();

			Assert(!incident.IM_ClosureResolutionInfo.HasChanges);
			Assert(!incident.IsCurrentResolutionCodeClosedOrResolved);
			incident.UpdateClosureResolutionForResolvedIncident(DispositionList.Constants.Closed.Cancelled);
			Assert("Closure Resolution should not be updated if the current is not resolved or closed", !incident.IM_ClosureResolutionInfo.HasChanges);

			incident.IM_ResolutionCode = DispositionList.Constants.Closed.Resolved;
			Factory.Save();

			incident.UpdateClosureResolutionForResolvedIncident(DispositionList.Constants.Closed.Completed);
			Factory.Save();
			Assert("No closure message should not be sent if the new value is same with old value", !incident.EConversation.Conversation.Messages.Any(x => x.Body == $"Close as {incident.IM_ClosureResolutionDescription}"));

			incident.UpdateClosureResolutionForResolvedIncident(DispositionList.Constants.Closed.UpgradeDelivered);
			Factory.Save();
			AssertEquals(DispositionList.Constants.Closed.UpgradeDelivered, incident.IM_ClosureResolution);
			AssertNotEquals("The time snap should not be updated if the new closure's ISResolution is true", incident.IM_CloseTimeUtc, incident.IM_ResolveTimeUtc);
			Assert("Message should be sent", incident.EConversation.Conversation.Messages.Any(x => x.Body == "Closed As Upgrade Delivered"));

			incident.UpdateClosureResolutionForResolvedIncident(DispositionList.Constants.Closed.Cancelled);
			Factory.Save();
			AssertEquals(DispositionList.Constants.Closed.Cancelled, incident.IM_ClosureResolution);
			AssertEquals("The time snap should be updated if the new closure's ISResolution is false", incident.IM_ResolveTimeUtc, incident.IM_CloseTimeUtc);
			Assert("Message should be sent", incident.EConversation.Conversation.Messages.Any(x => x.Body == $"Closed As Cancelled"));
		}

		public void TestDelayUpgrade()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.DelayUpgrade();
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed, incident.IM_ResolutionCode);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.DetailNoteText = "Hello";
			incident.SetLogTextForTest("No one");
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			Factory.Save();

			var relatedEventsBizObjs = incident.BusinessObjectsWithRelatedEvents;
			AssertEquals("2 notes", 2, relatedEventsBizObjs.Count(x => x is StmNote));
			AssertEquals("1 incident request", 1, relatedEventsBizObjs.Count(x => x is IncidentRequest));
		}

		public void TestLoadIncidentDBHits()
		{
			var ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var ent1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact0 = Factory.NewWithValidTestData<OrgContact>();
			var company0 = Factory.NewWithValidTestData<ClientCompany>();
			Factory.Save();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.DatabaseServerCode = ld1.LD_ServerCode;
			ld1.LD_LE = ent1.PK;

			incident.IM_OH_Client = org1.PK;
			incident.IM_LD = ld1.PK;
			incident.IM_OC_Contact = contact0.PK;
			incident.ClientCompanyCode = company0.LCC_Code;
			incident.IM_LCC = company0.PK;
			incident.Lookups.EnterpriseList.Load();
			Factory.Save();
			var expectedHitCounts = new Dictionary<string, int>
			{
				{ IncidentMainSchema.Constants.TableName, 1 },
				{ LicenceDatabaseSchema.Constants.TableName, 0 },
				{ LicenceHeaderSchema.Constants.TableName, 0 },
				{ OrgHeaderSchema.Constants.TableName, 0 },
				{ ClientCompanySchema.Constants.TableName, 0 },
			};
			var cleanFactory = new BusinessObjectFactory();
			var incident1 = cleanFactory.Load<SupportIncident>(incident.PK);
			AssertDbHits(expectedHitCounts, cleanFactory);
		}

		#region FieldChangeLog

		public void TestFieldChangeLog_Organisation()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			string initialValue = incident.ClientCode;
			incident.IM_OH_Client = org1.PK;
			Factory.Save();

			AssertLog("ClientCode", initialValue, org1.OH_Code, incident.PK, false);

			incident.IM_OH_Client = org2.PK;
			Factory.Save();

			AssertLog("ClientCode", org1.OH_Code, org2.OH_Code, incident.PK, true);
		}

		public void TestFieldChangeLog_Database()
		{
			var ld1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var ld2 = Factory.NewWithValidTestData<LicenceDatabase>();

			Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			string initialValue = incident.DatabaseServerCode;
			incident.DatabaseServerCode = ld1.LD_ServerCode;
			incident.IM_LD = ld1.PK;
			Factory.Save();

			AssertLog("Database", initialValue, ld1.LD_ServerCode, incident.PK, false);

			incident.DatabaseServerCode = ld2.LD_ServerCode;
			incident.IM_LD = ld2.PK;
			Factory.Save();

			AssertLog("Database", ld1.LD_ServerCode, ld2.LD_ServerCode, incident.PK, true);
		}

		public void TestFieldChangeLog_DatabaseCompany()
		{
			var company1 = Factory.NewWithValidTestData<ClientCompany>();
			var company2 = Factory.NewWithValidTestData<ClientCompany>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			string initialValue = incident.ClientCompanyCode;
			incident.ClientCompanyCode = company1.LCC_Code;
			incident.IM_LCC = company1.PK;
			Factory.Save();

			AssertLog("ClientCompanyCode", initialValue, company1.LCC_Code, incident.PK, false);

			incident.ClientCompanyCode = company2.LCC_Code;
			incident.IM_LCC = company2.PK;
			Factory.Save();

			AssertLog("ClientCompanyCode", company1.LCC_Code, company2.LCC_Code, incident.PK, true);
		}

		public void TestFieldChangeLog_Contact()
		{
			var contact0 = Factory.NewWithValidTestData<OrgContact>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OC_Contact = contact0.PK;

			string initialValue = incident.Contact.OC_ContactName;
			incident.IM_OC_Contact = contact1.PK;
			Factory.Save();

			AssertLog("ContactName", initialValue, contact1.OC_ContactName.ToString(), incident.PK, false);

			incident.IM_OC_Contact = contact2.PK;
			Factory.Save();

			AssertLog("ContactName", contact1.OC_ContactName.ToString(), contact2.OC_ContactName.ToString(), incident.PK, true);
		}

		public void TestGetEnterpeisePK_DBOnly()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var company = Factory.NewWithValidTestData<LicenceCompany>();

			company.LC_OH = org.PK;
			Factory.Save();

			var ld = Factory.NewWithValidTestData<LicenceDatabase>();

			var ent1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			var ent2 = Factory.NewWithValidTestData<LicenceEnterprise>();

			ld.LD_LE = ent1.PK;
			company.LC_LE = ent1.PK;

			AssertEquals(ld.LD_LEInfo.OriginalValue, SupportIncident.GetEnterprisePK(ld, null, true));
			AssertEquals(company.LC_LEInfo.OriginalValue, SupportIncident.GetEnterprisePK(null, org, true));

			Factory.Save();

			ld.LD_LE = ent2.PK;
			company.LC_LE = ent2.PK;

			AssertEquals("Object's original PK should be updated after saving", ent1.PK, SupportIncident.GetEnterprisePK(ld, null, true));
			AssertEquals("Object's original PK should be updated after saving", ent1.PK, SupportIncident.GetEnterprisePK(null, org, true));

			Factory.Save();

			AssertEquals(ent2.PK, SupportIncident.GetEnterprisePK(ld, null, true));
			AssertEquals(ent2.PK, SupportIncident.GetEnterprisePK(null, org, true));
		}

		public void TestFieldChangeLog_EnterpriseID()
		{
			var ld = Factory.NewWithValidTestData<LicenceDatabase>();

			var ent1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			var ent2 = Factory.NewWithValidTestData<LicenceEnterprise>();

			ld.LD_LE = ent1.PK;

			var incident = Factory.NewWithValidTestData<SupportIncidentForTest>();
			string initialValue = incident.Database?.EnterpriseID ?? ZString.Empty;
			incident.IM_LD = ld.PK;
			Factory.Save();

			AssertEquals("The result should not be controlled by Collection", 0, incident.Lookups.EnterpriseList.Count);
			AssertLog("EnterpriseID", initialValue, ent1.LE_EnterpriseID, incident.PK, false);

			ld.LD_LE = ent2.PK;
			incident.IM_Description = "xxxxx";
			incident.ResetIsEnterprisePKInitialised(); // need to be done for EnterprisePK to load the new value
			Factory.Save();

			AssertLog("EnterpriseID", ent1.LE_EnterpriseID, ent2.LE_EnterpriseID, incident.PK, true);
		}

		public void TestFieldChangeLog_EnterpriseCode()
		{
			var ld = Factory.NewWithValidTestData<LicenceDatabase>();

			var ent1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			var ent2 = Factory.NewWithValidTestData<LicenceEnterprise>();

			ld.LD_LE = ent1.PK;

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			string initialValue = incident.Database?.EnterpriseCode ?? ZString.Empty;
			incident.IM_LD = ld.PK;
			incident.EnterprisePK = ent1.PK;
			Factory.Save();

			AssertEquals("The result should not be controlled by Collection", 0, incident.Lookups.EnterpriseList.Count);
			AssertLog("EnterpriseCode", initialValue, ent1.LE_EnterpriseCode, incident.PK, false);

			incident.EnterprisePK = ent2.PK;
			incident.IM_Description = "xxxxx";
			Factory.Save();

			AssertLog("EnterpriseCode", ent1.LE_EnterpriseCode, ent2.LE_EnterpriseCode, incident.PK, true);
		}

		public void TestFieldChangedLog_IM_ClosureResolution()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();

			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = DispositionList.Constants.Closed.Resolved;
			incident.IM_ClosureResolution = DispositionList.Constants.Closed.Other;
			Factory.Save();
			AssertEquals(DispositionList.Constants.Closed.Other, incident.IM_ClosureResolution);

			incident.IM_ClosureResolution = DispositionList.Constants.Closed.Completed;
			Factory.Save();
			AssertEquals(DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);

			var log = incident.FieldChangedEventLogger.MostRecentLogStatusChange(SupportIncident.ChangedFieldDescription.IM_ClosureResolution, Events.StatusChangeCode);
			Assert(log.ReferenceFreeText.Contains($"{DispositionList.Constants.Closed.Other} to {DispositionList.Constants.Closed.Completed}"));
		}

		void AssertLog(string column, string oldValue, string newValue, ZGuid pk, bool shouldExist)
		{
			string logReference = $"Changed {column} from {oldValue} to {newValue}.";
			var query = new ZQuery(StmALogSchema.SL_Parent, pk);
			query.AddToFilter(StmALogSchema.SL_Reference, logReference);
			var log = Factory.Load<StmALog>(query);

			if (shouldExist)
			{
				query = new ZQuery(StmALogSchema.SL_Parent, pk);
				query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.NotEqual, ZString.Empty);

				var allLogsWithReference = Factory.Load<StmALog>(query);
				var logs = string.Join("\r\n", allLogsWithReference.Select(l => l.SL_Reference));

				AssertEquals($"Log '{logReference}' not found.\r\nFound logs with reference:\r\n{logs}", 1, log.Length);
			}
			else
			{
				AssertEquals(0, log.Length);
			}
		}

		#endregion

		public void TestAddMessageFromCurrentUser_TwoFactories()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			var convo = incident.EConversation.ExistingConversation;

			Factory.Save();

			var incident1 = factory1.Load<SupportIncident>(incident.PK);
			var incident2 = factory2.Load<SupportIncident>(incident.PK);

			AssertEquals(0, incident1.EConversation.ExistingConversation.Participants.Count);
			AssertEquals(0, incident2.EConversation.ExistingConversation.Participants.Count);

			incident1.AddPublicSystemLogMessage("Hello there");
			factory1.Save();

			Thread.Sleep(100);

			incident2.AddPublicSystemLogMessage("Hello there I say");

			AssertNoExceptionThrown(delegate
			{ factory2.Save(); });

			var convoReloaded = new BusinessObjectFactory() { RefreshEnabled = false }.Load<JobConversation>(convo.PK);

			AssertEquals("Hello there I say", convoReloaded.Messages.First().JCM_Body);
			AssertEquals("Hello there", convoReloaded.Messages.Last().JCM_Body);
			AssertEquals("We shouldnt create another participant when one exists from the first run", convoReloaded.Messages.Last().Sender, convoReloaded.Messages.First().Sender);
		}

		#region Resolution Comment

		public void TestResolutionComment()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_FullName = "blah blah lola lola";
			org.OH_RL_NKClosestPort = "AUSYD";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Edward Forgacs";
			contact.OC_Email = "edward.forgacs@cargowise.com";

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "ENT";
			enterprise.LE_OH = org.PK;
			Factory.Save();

			ReleaseBuild someBuild = Factory.New<ReleaseBuild>();
			someBuild.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = someBuild.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Description = "Help me";
			incident.DetailNoteText = "Don't know what i'm doing";
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_ClientIncidentReference = "CL111111";

			Factory.Save();

			AssertEquals("The resolution comment should be blank initially.", ZString.Empty, incident.ResolutionComment);
			AssertNotEquals("The incident should not be closed.", SupportIncidentLookups.Status.Closed, incident.IM_Status);

			AssertEquals("The resolution comment should be blank because the incident is not closed.", ZString.Empty, incident.ResolutionComment);

			incident.ResolutionNote.Text = "The incident is not closed.";
			AssertEquals("The resolution comment should still be blank because the incident is not closed.", ZString.Empty, incident.ResolutionComment);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, "The incident is now closed.");
			AssertEquals("The resolution comment should equal the resolution note text because the incident is still in support.", incident.ResolutionNoteText, incident.ResolutionComment);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, "The defect is now closed.");

			AssertEquals("The stage should now be defect.", SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertContains("The resolution comment should contain the defect disposition.", incident.Lookups.StatusDispositionList[incident.IM_ResolutionCode].Description, incident.ResolutionComment);

			incident.IM_ResolutionCode = ZString.Empty;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "The feature request is now closed.");

			AssertEquals("The stage should now be feature request.", SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertContains("The resolution comment should contain the feature request disposition.", incident.Lookups.StatusDispositionList[incident.IM_ClosureResolution].Description, incident.ResolutionComment);
		}

		public void TestResolutionComment_Escalation()
		{
			var incident = Factory.New<SupportIncidentForTest>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "internal escalation comment");
			AssertEquals(string.Empty, incident.ResolutionNoteText);

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "internal escalation comment");
			AssertEquals(string.Empty, incident.ResolutionNoteText);

			incident.Escalate(SupportIncidentCategoriesList.Codes.ContentDevelopment, "internal escalation comment");
			AssertEquals(string.Empty, incident.ResolutionNoteText);

			incident.Escalate(SupportIncidentCategoriesList.Codes.ComplianceRequirement, "internal escalation comment");
			AssertEquals(string.Empty, incident.ResolutionNoteText);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "internal escalation comment");
			AssertEquals(string.Empty, incident.ResolutionNoteText);
		}

		public void TestResolutionComment_CloseAfterEscalate()
		{
			var incident = Factory.New<SupportIncidentForTest>();
			incident.IM_Category = "CNT";
			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "escalation comment");

			AssertEquals(string.Empty, incident.ResolutionNoteText);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "published incident closure comment");
			AssertContains("published incident closure comment", incident.ResolutionNoteText);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, string.Empty, isAwaitingClient: true);
			AssertEquals(string.Empty, incident.ResolutionNoteText);
		}

		public void TestResolutionComment_CloseIncident()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Category = "SUP";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "contact@org.com";

			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;

			Factory.Save();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.CsStageDataFix, "This comment should be included in email");

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("eConversation message", true, messageList.Any(msg => msg.Body == "This comment should be included in email"));

			incident.Escalate(SupportIncidentCategoriesList.Codes.ComplianceRequirement, "");
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "Another comment should be included in email");
			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("eConversation message", true, messageList.Any(msg => msg.Body == "Another comment should be included in email"));
		}

		public void TestIncidentEConversationUnsubscribeParticipants()
		{
			var incident = Factory.New<SupportIncidentForTest>();
			incident.IM_Category = "SUP";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "contact@org.com";

			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;

			Factory.Save();

			var contactFollower = incident.EConversation.ExistingConversation.RelatedParties.FirstOrDefault(x => x.EmailAddress == "contact@org.com");
			Assert(contactFollower.JCP_IsSubscribed);
			contactFollower.JCP_IsSubscribed = false;
			Factory.Save();

			Assert("During saving process have UnsubscribeParticipants, this variable should be true", incident.HasUnsubscribeParticipants);
			AssertEquals("After saving successfully should clear UnsubscribeParticipants", 0, incident.EConversation.Conversation.UnsubscribedParticipants.Count);
		}

		[TestDate(2023, 01, 01)]
		public void TestCloseIncident_Resolved()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			var resolvedDecription = "ZZZ Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)resolvedDecription, supportParent, ZBool.True));

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var incident1 = Factory.New<SupportIncident>();
				incident1.IM_Category = "SUP";

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var contact = org.Contacts.AddNew();
				contact.OC_Email = "contact@org.com";

				incident1.IM_OH_Client = org.PK;
				incident1.IM_OC_Contact = contact.PK;

				Factory.Save();

				var originalResolutionCode = incident1.IM_ResolutionCode;

				Assert("Precondition: incident1 should be unresolved", incident1.IM_ClosureResolution.IsEmpty);

				incident1.CloseIncident(resolvedCode, "Comment");
				Factory.Save();
				AssertEquals("Disposition should now be set to resolved", DispositionList.Constants.Closed.Resolved, incident1.IM_ResolutionCode);
				AssertEquals("Closure resolution should be set to the code used for closing", resolvedCode, incident1.IM_ClosureResolution);
				AssertEquals("Resolve should set resolved time", new ZDateTime(2023, 01, 01), incident1.IM_ResolveTimeUtc);

				var incident1ResolvedLog = incident1.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);
				var incident1StatusUpdatedLog = incident1.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_EventTime).First
				(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode);

				var expectedResolvedReferences = new Dictionary<string, string>();
				expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, resolvedCode);
				expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Incident resolved with method {resolvedCode} - {resolvedDecription}"));
				AssertReference(expectedResolvedReferences, incident1ResolvedLog.SL_Reference);

				var expectedStatusUpdatedReferences = new Dictionary<string, string>();
				expectedStatusUpdatedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, originalResolutionCode);
				expectedStatusUpdatedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, incident1.IM_ResolutionCode);
				expectedStatusUpdatedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Incident resolved by user {GlbStaff.CurrentUser.GS_Code}"));
				AssertReference(expectedStatusUpdatedReferences, incident1StatusUpdatedLog.SL_Reference);
			}
		}

		[TestDate(2023, 01, 01)]
		public void TestCloseIncident_ResolvedAndPreviouslyResolved()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode1 = "ZZZ";
			var resolvedCode2 = "YYY";
			var resolvedDecription2 = "YYY Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode1, (NoResString)"ZZZ Description", supportParent, ZBool.True));
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode2, (NoResString)resolvedDecription2, supportParent, ZBool.True));

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var incident1 = Factory.New<SupportIncident>();
				incident1.IM_Category = "SUP";

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var contact = org.Contacts.AddNew();
				contact.OC_Email = "contact@org.com";

				incident1.IM_OH_Client = org.PK;
				incident1.IM_OC_Contact = contact.PK;

				Factory.Save();

				incident1.CloseIncident(resolvedCode1, "Comment");
				Factory.Save();

				AssertEquals("Precondition: Disposition should now be set to resolved", DispositionList.Constants.Closed.Resolved, incident1.IM_ResolutionCode);
				AssertEquals("Precondition: Closure resolution should be set to the code used for closing", resolvedCode1, incident1.IM_ClosureResolution);
				AssertEquals("Precondition: Resolve should set resolved time", new ZDateTime(2023, 01, 01), incident1.IM_ResolveTimeUtc);

				TestDateAttribute.AddDays(1);
				incident1.CloseIncident(resolvedCode2, "Comment2");
				Factory.Save();

				AssertEquals("Disposition should now be still set to resolved", DispositionList.Constants.Closed.Resolved, incident1.IM_ResolutionCode);
				AssertEquals("Closure resolution should be set to the code used for closing", resolvedCode2, incident1.IM_ClosureResolution);
				AssertEquals("Resolve should set resolved time again", new ZDateTime(2023, 01, 02), incident1.IM_ResolveTimeUtc);

				var incident1ResolvedLog = incident1.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_EventTime).First
				(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);
				var incident1StatusUpdatedLog = incident1.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_EventTime).First
				(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode);

				var expectedResolvedReferences = new Dictionary<string, string>();
				expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, resolvedCode1);
				expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, resolvedCode2);
				expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Incident resolved with method {resolvedCode2} - {resolvedDecription2}"));
				AssertReference(expectedResolvedReferences, incident1ResolvedLog.SL_Reference);

				var expectedStatusUpdatedReferences = new Dictionary<string, string>();
				expectedStatusUpdatedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, DispositionList.Constants.Closed.Resolved);
				expectedStatusUpdatedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, DispositionList.Constants.Closed.Resolved);
				expectedStatusUpdatedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Incident resolved by user {GlbStaff.CurrentUser.GS_Code}"));
				AssertReference(expectedStatusUpdatedReferences, incident1StatusUpdatedLog.SL_Reference);
			}
		}

		[TestDate(2023, 01, 01)]
		public void TestCloseIncident_SkipResolved()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			var resolvedDecription = "ZZZ Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)resolvedDecription, supportParent, ZBool.True));

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var incident1 = Factory.New<SupportIncident>();
				incident1.IM_Category = "SUP";
				var incident2 = Factory.New<SupportIncident>();
				incident2.IM_Category = "SUP";

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var contact = org.Contacts.AddNew();
				contact.OC_Email = "contact@org.com";

				incident1.IM_OH_Client = org.PK;
				incident1.IM_OC_Contact = contact.PK;
				incident2.IM_OH_Client = org.PK;
				incident2.IM_OC_Contact = contact.PK;

				Factory.Save();

				var originalResolutionCode = incident1.IM_ResolutionCode;
				var skipResolvedCode = SupportIncidentLookups.DispositionList.Constants.Closed.SystemHardwareNetwork;
				var skipResolvedRow = registryValue.Cast<IncidentClosureDisposition>().FirstOrDefault(x => x.Code == skipResolvedCode && x.ParentID == supportParent.PK);
				var skipResolvedDescription = skipResolvedRow.Description;

				Assert("Precondition: incident1 should be unresolved", incident1.IM_ClosureResolution.IsEmpty);
				AssertEquals("Precondition: We should be closing incident2 with a method which skips the resolution step", false, skipResolvedRow.IsResolution);

				incident1.CloseIncident(resolvedCode, "Comment");
				incident2.CloseIncident(skipResolvedCode, "Comment2");
				Factory.Save();

				AssertEquals("Disposition should now be set to resolved", DispositionList.Constants.Closed.Resolved, incident1.IM_ResolutionCode);
				AssertEquals("Closure resolution should be set to the code used for closing", resolvedCode, incident1.IM_ClosureResolution);
				AssertEquals("Resolve should set resolved time", new ZDateTime(2023, 01, 01), incident1.IM_ResolveTimeUtc);
				AssertEquals("Disposition should be set to closed", DispositionList.Constants.Closed.ResolvedAndClosed, incident2.IM_ResolutionCode);
				AssertEquals("Closure resolution should be set to the code used for closing", skipResolvedCode, incident2.IM_ClosureResolution);
				AssertEquals("Should set resolved time", new ZDateTime(2023, 01, 01), incident2.IM_ResolveTimeUtc);

				var incident2ResolvedLog = incident2.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);
				var incident2StatusUpdatedLog = incident2.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_EventTime).First
				(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode);

				var expectedResolvedReferences = new Dictionary<string, string>();
				expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, skipResolvedCode);
				expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Incident resolved with method {skipResolvedCode} - {skipResolvedDescription}"));
				AssertReference(expectedResolvedReferences, incident2ResolvedLog.SL_Reference);

				var expectedStatusUpdatedReferences = new Dictionary<string, string>();
				expectedStatusUpdatedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, originalResolutionCode);
				expectedStatusUpdatedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, DispositionList.Constants.Closed.ResolvedAndClosed);
				expectedStatusUpdatedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Incident closed by user {GlbStaff.CurrentUser.GS_Code}"));
				AssertReference(expectedStatusUpdatedReferences, incident2StatusUpdatedLog.SL_Reference);
			}
		}

		[TestDate(2023, 01, 01)]
		public void TestCloseIncident_ClosedAndPreviouslyResolved()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			var skipResolvedCode = "YYY";
			var skipResolvedDecription = "YYY Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)"ZZZ Description", supportParent, ZBool.True));
			registryValue.AddSystemChildren(registryValue.Add(skipResolvedCode, (NoResString)skipResolvedDecription, supportParent, ZBool.False));

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var incident1 = Factory.New<SupportIncident>();
				incident1.IM_Category = "SUP";

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var contact = org.Contacts.AddNew();
				contact.OC_Email = "contact@org.com";

				incident1.IM_OH_Client = org.PK;
				incident1.IM_OC_Contact = contact.PK;

				Factory.Save();

				incident1.CloseIncident(resolvedCode, "Comment");
				Factory.Save();

				AssertEquals("Precondition: Disposition should now be set to resolved", DispositionList.Constants.Closed.Resolved, incident1.IM_ResolutionCode);
				AssertEquals("Precondition: Closure resolution should be set to the code used for closing", resolvedCode, incident1.IM_ClosureResolution);
				AssertEquals("Precondition: Resolve should set resolved time", new ZDateTime(2023, 01, 01), incident1.IM_ResolveTimeUtc);

				TestDateAttribute.AddDays(1);
				incident1.CloseIncident(skipResolvedCode, "Comment2");
				Factory.Save();

				AssertEquals("Disposition should now be set to closed", DispositionList.Constants.Closed.ResolvedAndClosed, incident1.IM_ResolutionCode);
				AssertEquals("Closure resolution should be set to the code used for closing", skipResolvedCode, incident1.IM_ClosureResolution);
				AssertEquals("Close should set resolved time again", new ZDateTime(2023, 01, 02), incident1.IM_ResolveTimeUtc);

				var incident1ResolvedLog = incident1.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_EventTime).First
				(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);
				var incident1StatusUpdatedLog = incident1.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_EventTime).First
				(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode);

				var expectedResolvedReferences = new Dictionary<string, string>();
				expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, resolvedCode);
				expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, skipResolvedCode);
				expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Incident resolved with method {skipResolvedCode} - {skipResolvedDecription}"));
				AssertReference(expectedResolvedReferences, incident1ResolvedLog.SL_Reference);

				var expectedStatusUpdatedReferences = new Dictionary<string, string>();
				expectedStatusUpdatedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, DispositionList.Constants.Closed.Resolved);
				expectedStatusUpdatedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, DispositionList.Constants.Closed.ResolvedAndClosed);
				expectedStatusUpdatedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Incident closed by user {GlbStaff.CurrentUser.GS_Code}"));
				AssertReference(expectedStatusUpdatedReferences, incident1StatusUpdatedLog.SL_Reference);
			}
		}

		[TestDate(2023, 01, 01)]
		public void TestCloseIncident_FeatureRequestAccepted()
		{
			var incident1 = Factory.New<SupportIncident>();
			incident1.IM_Category = "SUP";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "contact@org.com";

			incident1.IM_OH_Client = org.PK;
			incident1.IM_OC_Contact = contact.PK;

			Factory.Save();

			incident1.IM_ClosureResolution = DispositionList.Constants.Closed.Completed;

			Assert("Precondition: incident1's closure resolution should be set", !incident1.IM_ClosureResolution.IsEmpty);

			incident1.CloseIncident(DispositionList.Constants.FeatureAccepted, "Comment");

			AssertEquals("Disposition (eRequest Status) should now be set to AUT", DispositionList.Constants.FeatureAccepted, incident1.IM_ResolutionCode);
			AssertEquals("Closure resolution should be erased", true, incident1.IM_ClosureResolution.IsEmpty);
			AssertEquals("Resolve should not set resolved time", true, incident1.IM_ResolveTimeUtc.IsEmpty);

			AssertEquals("Should not resolve the incident", false, incident1.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode));
		}

		void AssertReference(Dictionary<string, string> expectedReferenceParameters, string reference)
		{
			var freeTextBits = new List<string>();
			var paramBits = new List<(string Key, string Value)>();
			EventLogReferenceBuilder.New().ParseReference(reference, (text) => freeTextBits.Add(text), (key, value) => paramBits.Add((key, value)));
			foreach (var referencePair in expectedReferenceParameters)
			{
				AssertEquals(FormattableString.Invariant($"Value should match for key {referencePair.Key}"), referencePair.Value, paramBits.FirstOrDefault(x => x.Key == referencePair.Key).Value);
			}
		}

		public void TestIM_ResolutionCodeDescription_Resolved()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();

			incident1.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident2.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident1.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident2.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident1.IM_Product = ProductTypes.Codes.Enterprise;
			incident2.IM_Product = ProductTypes.Codes.Enterprise;
			incident1.IM_Status = SupportIncidentLookups.Status.Closed;
			incident2.IM_Status = SupportIncidentLookups.Status.Closed;

			incident1.IM_ResolutionCode = DispositionList.Constants.Closed.Resolved;
			incident1.IM_ClosureResolution = SupportIncidentLookups.DispositionList.Constants.Closed.Other;
			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ThirdPartySystemProblem;
			Factory.Save();

			AssertEquals("Should show Resolved description", "Resolved", incident1.IM_ResolutionCodeDescription);
			AssertEquals("Should show description of OTH", incident1.Lookups.StatusDispositionList.GetDescriptionFromCode(incident1.IM_ClosureResolution), incident1.IM_ClosureResolutionDescription);
			AssertEquals("Should retrieve description from lookup of IM_ResolutionCode", incident2.Lookups.StatusDispositionList.GetDescriptionFromCode(incident2.IM_ResolutionCode), incident2.IM_ResolutionCodeDescription);
		}

		public void TestIM_ClosureResolutionDescription()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();

			incident1.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident2.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident1.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident2.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident1.IM_Product = ProductTypes.Codes.Enterprise;
			incident2.IM_Product = ProductTypes.Codes.Enterprise;
			incident1.IM_Status = SupportIncidentLookups.Status.Closed;
			incident2.IM_Status = SupportIncidentLookups.Status.Closed;

			incident1.IM_ResolutionCode = DispositionList.Constants.Closed.Resolved;
			incident1.IM_ClosureResolution = SupportIncidentLookups.DispositionList.Constants.Closed.Other;
			incident2.IM_ResolutionCode = DispositionList.Constants.Closed.ResolvedAndClosed;
			incident2.IM_ClosureResolution = SupportIncidentLookups.DispositionList.Constants.Closed.ThirdPartySystemProblem;
			Factory.Save();

			AssertEquals("Should show Resolved description", "Resolved", incident1.IM_ResolutionCodeDescription);
			AssertEquals("Should show description of OTH", incident1.Lookups.StatusDispositionList.GetDescriptionFromCode(incident1.IM_ClosureResolution), incident1.IM_ClosureResolutionDescription);
			AssertEquals("Should show Closed description", "Closed", incident2.IM_ResolutionCodeDescription);
			AssertEquals("Should show description of TSP", incident2.Lookups.StatusDispositionList.GetDescriptionFromCode(incident2.IM_ClosureResolution), incident2.IM_ClosureResolutionDescription);
		}

		public void TestShouldSendSystemMessageAboutReopenRule_WhenIncidentIsClosed()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = "AAA";
			incident.IM_Priority = "CR4";
			incident.ProductArea = "ARC";
			incident.IM_SourceModuleId = "SourceModule2";
			Factory.Save();

			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;
			var criticalityResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID.IsEmpty).FirstOrDefault();
			var productResolutionAndClosureBehaviour = (ResolutionAndClosureBehaviour)resolutionAndClosureBehaviourCollection.Where(x => ((ResolutionAndClosureBehaviour)x).ParentID == criticalityResolutionAndClosureBehaviour.ID).FirstOrDefault();
			criticalityResolutionAndClosureBehaviour.Code = incident.IM_Priority;
			productResolutionAndClosureBehaviour.Code = incident.IM_Product;
			productResolutionAndClosureBehaviour.ClosedReopenRule = ResolutionAndClosureBehaviour.Constants.Code.AlwaysAllow;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				var closedReopenRule = incident.GetClosedReopenRule();
				AssertEquals("ClosedReopenRule should be ALW", ResolutionAndClosureBehaviour.Constants.Code.AlwaysAllow, closedReopenRule);
				AssertEquals("IM_ResolutionCode should be ADD", DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);

				incident.CloseIncident(DispositionList.Constants.Closed.SelfResolved, string.Empty);
				Factory.Save();
				AssertEquals("IM_ResolutionCode should be CLS", DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				Assert("A system message should be sent regarding the reopen rule", incident.EConversation.Conversation.Messages.Any(x => x.Body.Equals("The re-open rule for this incident at time of closure: ALW - Always Allow") && x.JCM_IsInternal && x.JCM_IsSystem));
			}

			productResolutionAndClosureBehaviour.ClosedReopenRule = ResolutionAndClosureBehaviour.Constants.Code.NeverAllow;
			Factory.Save();
			Factory.ClearCachedValue<ResolutionAndClosureBehaviour>("SupportIncidentLookups.GetResolutionAndClosureBehaviour:" + "ENT" + ":" + criticalityResolutionAndClosureBehaviour.PK);
			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				var closedReopenRule = incident.GetClosedReopenRule();
				AssertEquals("ClosedReopenRule should be NEV", ResolutionAndClosureBehaviour.Constants.Code.NeverAllow, closedReopenRule);
				incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
				Factory.Save();
				incident.CloseIncident(DispositionList.Constants.Closed.ThirdPartySystemProblem, string.Empty);
				Factory.Save();
				AssertEquals("IM_ResolutionCode should be CLS", DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				Assert("A system message should be sent regarding the reopen rule", incident.EConversation.Conversation.Messages.Any(x => x.Body.Equals("The re-open rule for this incident at time of closure: NEV - Never Allow") && x.JCM_IsInternal && x.JCM_IsSystem));
			}
		}

		public void TestCanRevertToAwaitingResponse()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Assert("New incident should cannot RevertToAwaitingResponse", !incident.CanRevertToAwaitingResponse);
			Factory.Save();
			Assert("New incident should cannot RevertToAwaitingResponse", !incident.CanRevertToAwaitingResponse);

			incident.CloseIncident(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, string.Empty);
			Factory.Save();
			AssertEquals(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var incident2 = factory2.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
			Assert(!incident2.CanRevertToAwaitingResponse);

			incident2.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Assert(incident2.CanRevertToAwaitingResponse);
			factory2.Save();
			Assert(incident2.CanRevertToAwaitingResponse);

			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };
			var incident3 = factory3.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
			Assert(incident3.CanRevertToAwaitingResponse);
		}

		public void TestTryRevertToAwaitingResponse()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.CloseIncident(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, string.Empty, true);
			Factory.Save();
			AssertEquals(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
			var iwrLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.IncidentAwaitingResponseCode);
			iwrLogQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Equal, "Incident has been set to Awaiting Client Response.");
			var iwrLogCount = incident.Logs.Find(iwrLogQuery).Length;
			AssertEquals("iwrLogCount should be 1", 1, iwrLogCount);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			incident.CloseIncident(DispositionList.Constants.Closed.Other, string.Empty);
			Factory.Save();
			AssertNotEquals(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
			iwrLogCount = incident.Logs.Find(iwrLogQuery).Length;
			AssertEquals("iwrLogCount should be 1", 1, iwrLogCount);

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var incident2 = factory2.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
			Assert(!incident2.CanRevertToAwaitingResponse);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			incident.CloseIncident(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, string.Empty, true);
			Factory.Save();
			iwrLogCount = incident.Logs.Find(iwrLogQuery).Length;
			AssertEquals("iwrLogCount should be 2", 2, iwrLogCount);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			Assert(incident.CanRevertToAwaitingResponse);

			var result = incident.RevertIncidentToAwaitingResponse();
			Factory.Save();
			AssertEquals(SupportIncident.RevertResult.Success, result);
			AssertEquals(true, incident.EConversation.GetTimeOrderedMessages().Any(msg => msg.Body == "Client response is not genuine. Revert status to Awaiting Response."));
			var latestEConversationMessage = incident.EConversation.LastAddedMessageForTest;
			AssertEquals(true, latestEConversationMessage.Body == "Awaiting Client Response" && latestEConversationMessage.JCM_IsSystem);

			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };
			var incident3 = factory3.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
			AssertEquals(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident3.IM_ResolutionCode);
		}

		public void TestCanRevertToAwaitingResponseShouldBeFalseAndNoErrorReportWhenCurrentResolutionCodeClosedOrResolved()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			Factory.Save();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			Factory.Save();

			incident.Logs.AddNew(Events.StatusChange, "Disposition - CWR to AUC");
			Factory.Save();

			ErrorReporter.Clear();
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			Assert("CanRevertToAwaitingResponse should be false", !incident.CanRevertToAwaitingResponse);
			AssertNullOrEmpty("Should not have errorReport message", ErrorReporter.LastMessageReported);
		}

		public void TestCanCloseOnBehalfOfClient()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();

			Assert(!incident.CanCloseOnBehalfOfClient);

			incident.IM_ResolutionCode = DispositionList.Constants.Open.AddedAwaitingAssignment;
			Assert(!incident.CanCloseOnBehalfOfClient);

			incident.IM_ResolutionCode = DispositionList.Constants.Open.AssignedAwaitingAction;
			Assert(!incident.CanCloseOnBehalfOfClient);

			incident.IM_ResolutionCode = DispositionList.Constants.Working.WorkInProgress;
			Assert(!incident.CanCloseOnBehalfOfClient);

			incident.CloseIncident(DispositionList.Constants.Closed.Other, string.Empty);
			Factory.Save();

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			incident.IM_ResolutionCode = DispositionList.Constants.Open.AddedAwaitingAssignment;
			Assert(incident.CanCloseOnBehalfOfClient);

			incident.IM_ResolutionCode = DispositionList.Constants.Open.AssignedAwaitingAction;
			Assert(incident.CanCloseOnBehalfOfClient);

			incident.IM_ResolutionCode = DispositionList.Constants.Working.WorkInProgress;
			Assert(incident.CanCloseOnBehalfOfClient);

			incident.AddSystemMessageToCustomer("test add system message");
			Factory.Save();

			incident.IM_ResolutionCode = DispositionList.Constants.Open.AddedAwaitingAssignment;
			Assert(incident.CanCloseOnBehalfOfClient);

			incident.IM_ResolutionCode = DispositionList.Constants.Open.AssignedAwaitingAction;
			Assert(incident.CanCloseOnBehalfOfClient);

			incident.IM_ResolutionCode = DispositionList.Constants.Working.WorkInProgress;
			Assert(incident.CanCloseOnBehalfOfClient);

			incident.AddStaffMessageToCustomer("test add support message");
			Factory.Save();

			incident.IM_ResolutionCode = DispositionList.Constants.Open.AddedAwaitingAssignment;
			Assert(!incident.CanCloseOnBehalfOfClient);

			incident.IM_ResolutionCode = DispositionList.Constants.Open.AssignedAwaitingAction;
			Assert(!incident.CanCloseOnBehalfOfClient);

			incident.IM_ResolutionCode = DispositionList.Constants.Working.WorkInProgress;
			Assert(!incident.CanCloseOnBehalfOfClient);
		}

		public void TestAddStageChangedMessageWithoutReason()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			Factory.Save();

			incident.SetIncidentStageWithoutReason("");
			var emptyMessage = string.Format(IncidentConstants.StageChangedMessageTemplate, incident.Lookups.StageList.GetDescriptionFromCode(""));
			AssertNotEquals(string.Empty, incident.IM_Category);
			Assert("Empty stage should not be added", !incident.EConversation.Conversation.Messages.Any(x => x.Body.Equals(emptyMessage)));

			incident.SetIncidentStageWithoutReason(SupportIncidentCategoriesList.Codes.Support);
			var supMessage = string.Format(IncidentConstants.StageChangedMessageTemplate, incident.Lookups.StageList.GetDescriptionFromCode(SupportIncidentCategoriesList.Codes.Support));
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			Assert("Support stage should not be added", !incident.EConversation.Conversation.Messages.Any(x => x.Body.Equals(supMessage)));

			incident.SetIncidentStageWithoutReason(SupportIncidentCategoriesList.Codes.Defect);
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);

			incident.SetIncidentStageWithoutReason(SupportIncidentCategoriesList.Codes.Defect);
			var defMessage = string.Format(IncidentConstants.StageChangedMessageTemplate, incident.Lookups.StageList.GetDescriptionFromCode(SupportIncidentCategoriesList.Codes.Defect));
			AssertEquals("The same stage should not be added", 1, incident.EConversation.Conversation.Messages.Count(x => x.Body.Equals(defMessage)));

			incident.SetIncidentStageWithoutReason(SupportIncidentCategoriesList.Codes.FeatureRequest);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			var ftrMessage = string.Format(IncidentConstants.StageChangedMessageTemplate, incident.Lookups.StageList.GetDescriptionFromCode(SupportIncidentCategoriesList.Codes.FeatureRequest));
			Assert("FTR stage should not be added", incident.EConversation.Conversation.Messages.Any(x => x.Body.Equals(ftrMessage)));
		}

		#endregion

		#region Cancellation Validation

		void AssertTaskCancellationPermission(bool currentUserCanCancel, bool cancellableTask, Action<SupportIncident> doIncidentAction)
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("XRM", "XRM");
			areas.AddPair("ARC", "ARC");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("SAA", "Module XRM", "XRM", false);
			product.ModuleMappings.AddNew("SBB", "Module ARC", "ARC", false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "CS00098432";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Priority = "CR6";
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.ProductArea = "ARC";
			incident.IM_Module = "SBB";
			incident.IM_Language = "EN";
			var task = incident.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Type = "UDF";

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypes = categorisedTaskTypes.AddNew();
			taskTypes.Code = "INC";
			var taskType = taskTypes.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.CanCancelTask = cancellableTask;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
			Env.Security.WorkflowTasksCanCancelAllTasks.IsAllowed = currentUserCanCancel;

			Factory.Save();

			doIncidentAction(incident);

			task.Validation.ValidateP9_Status();

			AssertNoErrors("Validation on task status SHOULD be disabled for incident actions", task.P9_StatusInfo);
		}

		public void TestShouldAllowTaskCancellation_WhenEscalatingIncident_AndUserDoesNotHavePermissionToCancelNonCancellableTask()
		{
			AssertTaskCancellationPermission
			(
				currentUserCanCancel: false,
				cancellableTask: false,
				(incident) => incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "")
			);
		}

		public void TestShouldAllowTaskCancellation_WhenChangingProductArea_AndUserDoesNotHavePermissionToCancelNonCancellableTask()
		{
			AssertTaskCancellationPermission
			(
				currentUserCanCancel: false,
				cancellableTask: false,
				(incident) => { incident.IM_Module = "SAA"; incident.ProductArea = "XRM"; }
			);
		}

		public void TestShouldAllowTaskCancellation_WhenAwaitingResponse_AndUserDoesNotHavePermissionToCancelNonCancellableTask()
		{
			AssertTaskCancellationPermission
			(
				currentUserCanCancel: false,
				cancellableTask: false,
				(incident) => incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "I feel like twiddling my thumbs.")
			);
		}

		public void TestShouldAllowTaskCancellation_WhenEscalatingIncident_AndUserHasPermissionToCancelNonCancellableTask()
		{
			AssertTaskCancellationPermission
			(
				currentUserCanCancel: true,
				cancellableTask: false,
				(incident) => incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "")
			);
		}

		public void TestShouldAllowTaskCancellation_WhenChangingProductArea_AndUserHasPermissionToCancelNonCancellableTask()
		{
			AssertTaskCancellationPermission
			(
				currentUserCanCancel: true,
				cancellableTask: false,
				(incident) => { incident.IM_Module = "SAA"; incident.ProductArea = "XRM"; }
			);
		}

		public void TestShouldAllowTaskCancellation_WhenAwaitingResponse_AndUserHasPermissionToCancelNonCancellableTask()
		{
			AssertTaskCancellationPermission
			(
				currentUserCanCancel: true,
				cancellableTask: false,
				(incident) => incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "I feel like twiddling my thumbs.")
			);
		}

		public void TestShouldAllowTaskCancellation_WhenEscalatingIncident_AndTaskIsCancellable()
		{
			AssertTaskCancellationPermission
			(
				currentUserCanCancel: false,
				cancellableTask: true,
				(incident) => incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "")
			);
		}

		public void TestShouldAllowTaskCancellation_WhenChangingProductArea_AndTaskIsCancellable()
		{
			AssertTaskCancellationPermission
			(
				currentUserCanCancel: false,
				cancellableTask: true,
				(incident) => { incident.IM_Module = "SAA"; incident.ProductArea = "XRM"; }
			);
		}

		public void TestShouldAllowTaskCancellation_WhenAwaitingResponse_AndTaskIsCancellable()
		{
			AssertTaskCancellationPermission
			(
				currentUserCanCancel: false,
				cancellableTask: true,
				(incident) => incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "")
			);
		}

		#endregion

		#region Email

		#region Assigned Staff Changed Email Notification

		protected virtual void AssignWorkTaskToStaffForEmailTest(SupportIncident task, GlbStaff staff)
		{
			task.IM_GS_NKAssignedToCurrent = staff != null ? staff.GS_Code : ZString.Empty;
			task.IM_GS_NKCustServiceContact = staff != null ? staff.GS_Code : ZString.Empty;
		}

		public virtual void TestAssignedStaffChangedEmailNotification()
		{
			EDIDataRegistry.Instance.EnableIncidentAssignedStaffChangedEmailNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			//first time allocated
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "NewUser@NewDomain.com";
			staff.GS_FullName = "Bob Smith";

			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			incident.IM_Description = "Vacuum the Carpet";
			AssignWorkTaskToStaffForEmailTest(incident, staff);
			Factory.Save();
			AssertEquals("Email should have been sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			string expectedSubject = IncidentConstants.GetIncidentTypeDescription(incident.IM_IncidentType) + ": " + incident.IM_IncidentNumber + " has been assigned to you";
			AssertEmailProperties(Env.OutgoingMailManager.EmailsCreated[0], "NewUser@NewDomain.com", expectedSubject, IncidentConstants.GetIncidentTypeDescription(incident.IM_IncidentType), incident.IM_IncidentNumber, "Vacuum the Carpet", "has been assigned to you by " + Env.CurrentUser.FullName + ".");

			//staff allocation changed
			GlbStaff anotherStaff = Factory.NewWithValidTestData<GlbStaff>();
			anotherStaff.GS_EmailAddress = "AnotherUser@AnotherDomain.com";
			AssignWorkTaskToStaffForEmailTest(incident, anotherStaff);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Factory.Save();
			AssertEquals("Email should have been sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmailProperties(Env.OutgoingMailManager.EmailsCreated[0], "AnotherUser@AnotherDomain.com", expectedSubject, IncidentConstants.GetIncidentTypeDescription(incident.IM_IncidentType), incident.IM_IncidentNumber, "Vacuum the Carpet", "has been assigned to you by " + Env.CurrentUser.FullName + ".");

			//incident status changed
			incident.IM_Status = IncidentConstants.IncidentStatus.FinishedDeployedAndPendingVerification;
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Factory.Save();
			AssertEquals("No email should be sent.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			//unassign
			AssignWorkTaskToStaffForEmailTest(incident, null);
			Factory.Save();
			AssertEquals("No email should be sent.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			//assign to new staff member
			AssignWorkTaskToStaffForEmailTest(incident, anotherStaff);
			Factory.Save();
			AssertEquals("An email should be sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			EDIDataRegistry.Instance.EnableIncidentAssignedStaffChangedEmailNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssignWorkTaskToStaffForEmailTest(incident, staff);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Factory.Save();
			AssertEquals("No email should be sent.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertContains("Email sent to new assigned staff - Bob Smith.", GetActualLogTextForStaffChangedLogAssertion(incident));
		}

		protected virtual bool ShouldSendEmailToCustomerServiceContact
		{
			get { return true; }
		}

		public void TestIM_StatusHasChanges()
		{
			EDIDataRegistry.Instance.EnableIncidentAssignedStaffChangedEmailNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "SaSho";
			staff.GS_EmailAddress = "TestMail@TestDomain.com";
			staff.GS_FullName = "Sasha Ivanov";

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Description = "Test Description";
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			incident.IM_Status = SupportIncidentLookups.Status.Open;
			Factory.Save();

			AssertEquals("Email should have been sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			string expectedRecipient = "TestMail@TestDomain.com";
			AssertEquals(expectedRecipient, Env.OutgoingMailManager.EmailsCreated[0].Recipients[0]);
		}

		public void TestCustomerServiceStaffChangedEmailNotification()
		{
			SupportIncident incident = (SupportIncident)GetNewBusinessObject();

			EDIDataRegistry.Instance.EnableIncidentAssignedStaffChangedEmailNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "NewUser@NewDomain.com";
			staff.GS_FullName = "Bob Smith";

			AssertEquals("Precondition: No email should be sent yet.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			incident.IM_Description = "Vacuum the Carpet";
			incident.IM_GS_NKCustServiceContact = staff.GS_Code;

			Factory.Save();
			string expectedSubject = IncidentConstants.GetIncidentTypeDescription(incident.IM_IncidentType) + ": " + incident.IM_IncidentNumber + " has been assigned to you";

			if (ShouldSendEmailToCustomerServiceContact)
			{
				AssertEquals("Email should have been sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmailProperties(Env.OutgoingMailManager.EmailsCreated[0], "NewUser@NewDomain.com", expectedSubject, IncidentConstants.GetIncidentTypeDescription(incident.IM_IncidentType), incident.IM_IncidentNumber, "Vacuum the Carpet", "has been assigned to you by " + Env.CurrentUser.FullName + ".");

				GlbStaff anotherStaff = Factory.NewWithValidTestData<GlbStaff>();
				anotherStaff.GS_EmailAddress = "AnotherUser@AnotherDomain.com";

				incident.IM_GS_NKCustServiceContact = anotherStaff.GS_Code;
				Env.OutgoingMailManager.EmailsCreated.Clear();
				Factory.Save();
				AssertEquals("Email should have been sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEmailProperties(Env.OutgoingMailManager.EmailsCreated[0], "AnotherUser@AnotherDomain.com", expectedSubject, IncidentConstants.GetIncidentTypeDescription(incident.IM_IncidentType), incident.IM_IncidentNumber, "Vacuum the Carpet", "has been assigned to you by " + Env.CurrentUser.FullName + ".");

				incident.IM_Status = IncidentConstants.IncidentStatus.FinishedDeployedAndPendingVerification;
				Env.OutgoingMailManager.EmailsCreated.Clear();
				Factory.Save();
				AssertEquals("No email should be sent.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				incident.IM_GS_NKCustServiceContact = ZString.Empty;
				Factory.Save();
				AssertEquals("No email should be sent.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				incident.IM_GS_NKCustServiceContact = anotherStaff.GS_Code;
				Factory.Save();
				AssertEquals("An email should be sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				EDIDataRegistry.Instance.EnableIncidentAssignedStaffChangedEmailNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				incident.IM_GS_NKCustServiceContact = staff.GS_Code;
				Env.OutgoingMailManager.EmailsCreated.Clear();
				Factory.Save();
				AssertEquals("No email should be sent.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				AssertContains("Email sent to new assigned staff - Bob Smith.", GetActualLogTextForStaffChangedLogAssertion(incident));
			}
			else
			{
				AssertEquals("Email should not have been sent.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertNotContains("No text", "Email sent to new assigned staff - Bob Smith.", GetActualLogTextForStaffChangedLogAssertion(incident));
			}
		}

		public void TestAssignedStaffChangedEmailNotificationNotSentIfNotSavedSuccessfully()
		{
			SupportIncident incident = (SupportIncident)GetNewBusinessObject();

			EDIDataRegistry.Instance.EnableIncidentAssignedStaffChangedEmailNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving_BlowUp);

			try
			{
				Factory.Save();
			}
			catch
			{
				AssertEquals("No email should be sent.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}

			Factory.Saving -= new BusinessObjectFactory.SavingEventHandler(Factory_Saving_BlowUp);
		}

		void Factory_Saving_BlowUp(BusinessObjectFactory factory)
		{
			throw new ApplicationException("Test");
		}

		public void TestSendingEmailWithEmptyRecipientAddress()
		{
			SupportIncident incident = (SupportIncident)GetNewBusinessObject();

			EDIDataRegistry.Instance.EnableIncidentAssignedStaffChangedEmailNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "";
			incident.IM_GS_NKAssignedToCurrent = staff.GS_Code;

			Factory.Save();
			AssertEquals("Email should not have been sent if the recipient email address is empty.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public virtual void TestEmailIsNotSentIfAssignedStaffIsSameAsCurrentUser()
		{
			SupportIncident incident = (SupportIncident)GetNewBusinessObject();

			EDIDataRegistry.Instance.EnableIncidentAssignedStaffChangedEmailNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.GS_LoginName = "staff1";
			staff2.GS_LoginName = "staff2";

			staff1.GS_EmailAddress = "staff1@staff1.com";
			staff2.GS_EmailAddress = "staff2@staff2.com";

			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily("staff1"))
			{
				AssignWorkTaskToStaffForEmailTest(incident, staff1);
				Factory.Save();
				AssertEquals("No email should be sent.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				AssignWorkTaskToStaffForEmailTest(incident, staff2);
				Factory.Save();
				AssertEquals("An email should be sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		void AssertEmailProperties(EmailDef email, string expectedRecipient, string expectedSubject, params string[] expectedBodyParts)
		{
			AssertNotNull("Email should not be null.", email);
			AssertEquals("Email.FromDisplayName", "ediProd System", email.FromDisplayName);
			AssertEquals("Email.FromAddress", "PleaseDoNotReply@wisetechglobal.com", email.FromAddress);
			AssertEquals("Email.ReplyTo", "PleaseDoNotReply@wisetechglobal.com", email.ReplyTo);
			AssertEquals("Email.Recipients.Count", 1, email.Recipients.Count);
			AssertEquals("Email.Recipients[0]", expectedRecipient, email.Recipients[0].Email);
			AssertEquals("Email.Subject", expectedSubject, email.Subject);

			foreach (string bodyPart in expectedBodyParts)
			{
				AssertContains(bodyPart, email.Body);
			}
		}

		public virtual void TestEmailBodyForChangedAssignedStaff()
		{
			EDIDataRegistry.Instance.EnableIncidentAssignedStaffChangedEmailNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "NewUser@NewDomain.com";
			staff.GS_FullName = "Fabrice Santoro";

			SupportIncident workTask = (SupportIncident)GetNewBusinessObject();
			workTask.IM_Description = "Beating Federer";
			AssignWorkTaskToStaffForEmailTest(workTask, staff);
			Factory.Save();

			AssertEquals("Email should have been sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			string expectedSubject = IncidentConstants.GetIncidentTypeDescription(Incident.IM_IncidentType) + ": " + Incident.IM_IncidentNumber + " has been assigned to you";

			string rawBodyText = IncidentConstants.GetTextFromResource(SupportIncidentEmailExternalResources.AssignedStaffEmailNotificationLetterFilePath);
			string preparedBodyText = new AssignedStaffNotificationEmailContentBuilder(workTask).BuildBody();
			if (!preparedBodyText.Contains(IncidentConstants.MarkName))
			{
				var builder = new ZStringBuilder(preparedBodyText);
				preparedBodyText = builder.Append(ZString.Format(IncidentConstants.MarkContent, CustomerServiceEmailUniqueIdUtil.GenerateUniqueEmailID(workTask))).ToString();
			}
			AssertEquals(preparedBodyText, Env.OutgoingMailManager.EmailsCreated[0].Body);

			string[] tags = new string[]
				{
					"<<Header>>",
					"<<IncidentType>>",
					"<<HTMLLink>>",
					"<<IM_Priority>>",
					"<<ClientCode>>",
					"<<ClientName>>",
					"<<IM_Description>>",
					"<<Footer>>"
				};

			foreach (string tag in tags)
			{
				AssertContains(tag, rawBodyText);
			}
		}

		#endregion

		void AssertRuleTagStatus(IncidentEmailTagRuleStatus expected, string tagCode, BusinessObject dataSource)
		{
			AssertRuleTagStatus(string.Empty, expected, tagCode, dataSource);
		}

		void AssertRuleTagStatus(string message, IncidentEmailTagRuleStatus expected, string tagCode, BusinessObject dataSource)
		{
			AssertEquals(message,
				expected,
				SupportIncidentEmailTriggeringRules.GetTagRuleStatus(dataSource, tagCode));
		}

		SupportIncident GetIncidentForTestingSuppressingAll()
		{
			var blnGroup = Factory.New<TagDefinition>();
			blnGroup.TGD_Code = "BLN";

			var magnitude = blnGroup.Magnitudes.AddNew();
			magnitude.TGM_Code = SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification;
			magnitude.TGM_IsActive = true;
			Factory.Save();

			var incident = (new BusinessObjectFactory() { RefreshEnabled = false }).NewWithValidTestData<SupportIncident>();
			incident.Factory.Save();

			return Factory.Load<SupportIncident>(incident.PK);
		}

		readonly string expectedNotificationDisabledMessage = "We have disabled outbound email notifications for this eRequest.";
		readonly string expectedNotificationEnabledMessage = "We have re-enabled outbound email notifications for this eRequest.";

		public void TestMuteIncidentEmailNotification()
		{
			var mainIncident = GetIncidentForTestingSuppressingAll();
			Assert("The operation should execute successfully", mainIncident.MuteEmailNotification());
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.New, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);
			Assert(SupportIncidentEmailTriggeringRules.IsAllSuppressed(mainIncident));

			Assert("Should not post message before saving", !mainIncident.EConversation.AnyLocalMessageContains(expectedNotificationDisabledMessage));
			mainIncident.Factory.Save();
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.Existing, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);
			Assert("Should post message after saving", mainIncident.EConversation.ExistingConversation.Messages.Any(x => x.Body == expectedNotificationDisabledMessage));
		}

		public void TestUnmuteIncidentEmailNotification()
		{
			var mainIncident = GetIncidentForTestingSuppressingAll();
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.NotExists, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);

			mainIncident.UnmuteEmailNotification();
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.NotExists, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);
			Assert(!SupportIncidentEmailTriggeringRules.IsAllSuppressed(mainIncident));
			Assert("No Changes", !mainIncident.EConversation.AnyLocalMessageContains(expectedNotificationEnabledMessage));

			mainIncident.Factory.Save();
			Assert("No Changes", !mainIncident.EConversation.ExistingConversation.Messages.Any(x => x.Body == expectedNotificationEnabledMessage));

			mainIncident.MuteEmailNotification();
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.New, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);

			mainIncident.Factory.Save();
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.Existing, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);
			Assert("Should post mute message", mainIncident.EConversation.ExistingConversation.Messages.Any(x => x.Body == expectedNotificationDisabledMessage));

			mainIncident.UnmuteEmailNotification();
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.Deleted, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);
			Assert("Should not post message before saving", !mainIncident.EConversation.AnyLocalMessageContains(expectedNotificationEnabledMessage));

			mainIncident.Factory.Save();
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.NotExists, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);
			Assert("Should post message after saving", mainIncident.EConversation.ExistingConversation.Messages.Any(x => x.Body == expectedNotificationEnabledMessage));
		}

		public void TestMuteAndUnmuteInSingleSave()
		{
			var mainIncident = GetIncidentForTestingSuppressingAll();
			Assert(!mainIncident.HasChanges);
			Assert("Should not be muted", !SupportIncidentEmailTriggeringRules.IsAllSuppressed(mainIncident));
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.NotExists, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);

			Assert(mainIncident.MuteEmailNotification());
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.New, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);
			Assert("Should be muted", SupportIncidentEmailTriggeringRules.IsAllSuppressed(mainIncident));

			Assert(mainIncident.MuteEmailNotification());
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.New, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);
			Assert("Should be muted", SupportIncidentEmailTriggeringRules.IsAllSuppressed(mainIncident));

			mainIncident.UnmuteEmailNotification();
			AssertRuleTagStatus("Status should be NotExists because the new link was deleted before saving", IncidentEmailTagRuleStatus.NotExists, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);
			Assert("Should not be muted", !SupportIncidentEmailTriggeringRules.IsAllSuppressed(mainIncident));
			Assert(mainIncident.HasChanges);

			Factory.Save();
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.NotExists, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);
			Assert("No changes", !mainIncident.EConversation.ExistingConversation.Messages.Any(x => x.Body == expectedNotificationEnabledMessage));
			Assert("No changes", !mainIncident.EConversation.ExistingConversation.Messages.Any(x => x.Body == expectedNotificationDisabledMessage));

			Assert(mainIncident.MuteEmailNotification());
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.New, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);

			mainIncident.UnmuteEmailNotification();
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.NotExists, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);

			Assert(mainIncident.MuteEmailNotification());
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.New, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);

			mainIncident.Factory.Save();
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.Existing, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);
			Assert("Should be muted", SupportIncidentEmailTriggeringRules.IsAllSuppressed(mainIncident));
			Assert(!mainIncident.EConversation.ExistingConversation.Messages.Any(x => x.Body == expectedNotificationEnabledMessage));
			AssertEquals("Should post only one message", 1, mainIncident.EConversation.ExistingConversation.Messages.Count(x => x.Body == expectedNotificationDisabledMessage));

			mainIncident.UnmuteEmailNotification();
			Assert(mainIncident.MuteEmailNotification());
			mainIncident.UnmuteEmailNotification();
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.Deleted, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);

			mainIncident.Factory.Save();
			AssertRuleTagStatus(IncidentEmailTagRuleStatus.NotExists, SupportIncidentEmailTemplateConstants.Codes.AllEmailNotification, mainIncident);
			Assert("Should not be muted", !SupportIncidentEmailTriggeringRules.IsAllSuppressed(mainIncident));
			AssertEquals("Should post only one message", 1, mainIncident.EConversation.ExistingConversation.Messages.Count(x => x.Body == expectedNotificationEnabledMessage));
			AssertEquals("Should not post new message", 1, mainIncident.EConversation.ExistingConversation.Messages.Count(x => x.Body == expectedNotificationDisabledMessage));
		}

		public void TestControllerIDForHtmlUrl()
		{
			SupportIncident workTask = (SupportIncident)GetNewBusinessObject();
			ControllerID controllerID = (ControllerID)workTask.GetType().InvokeMember(
				"ControllerIDForHtmlUrl",
				BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				workTask,
				null);
			AssertEquals(ExpectedControllerIDForHtmlUrl, controllerID);
		}

		#endregion

		#region Logging and Saving

		public void TestSaving()
		{
			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			Assert(incident.IM_IncidentNumber.IsEmpty);

			Factory.Save();
			Assert(!incident.IM_IncidentNumber.IsEmpty);

			ZString incidentNumber = incident.IM_IncidentNumber;

			System.Threading.Thread.Sleep(1000);
			Factory.Save();
			AssertEquals(incidentNumber, incident.IM_IncidentNumber);
		}

		void AssignToStaff(SupportIncident workTask, GlbStaff staff)
		{
			workTask.IM_GS_NKAssignedToCurrent = staff.GS_Code;
		}

		[TestDate(2012, 6, 22, 9, 0, 0)]
		public virtual void TestLogging()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "YAK";

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "XZ";

			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			incident.IM_Category = "SUP";
			AssignToStaff(incident, staff1);
			Factory.Save();
			AssertNotNull(incident.Logs.MostRecentLogByEventTime(Events.AssignedUserChanged));
			AssertEquals("Assigned User -  to YAK", incident.Logs.MostRecentLogByEventTime(Events.AssignedUserChanged).SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			AssignToStaff(incident, staff2);
			Factory.Save();

			AssertNotNull(incident.Logs.MostRecentLogByEventTime(Events.AssignedUserChanged));
			AssertEquals("Assigned User - YAK to XZ", incident.Logs.MostRecentLogByEventTime(Events.AssignedUserChanged).SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			incident.IM_GS_NKCustServiceContact = staff2.GS_Code;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			incident.IM_GS_NKCustServiceContact = staff1.GS_Code;
			Factory.Save();
			AssertEquals("Cust Svc - XZ to YAK", incident.Logs.MostRecentLogByEventTime(Events.AssignedUserChanged).SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			incident.IM_Status = IncidentMainLookups.Status.Working;
			Factory.Save();
			StmALog statusEvent = incident.Logs.MostRecentLogByEventTime(Events.StatusChange);
			AssertNotNull(statusEvent);
			AssertEquals("Status", ExpectedStatusFieldDescription + " - OPN to WRK", statusEvent.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress;
			Factory.Save();
			statusEvent = incident.Logs.MostRecentLogByEventTime(Events.StatusChange);
			AssertNotNull(statusEvent);
			AssertEquals("Support Disp", ExpectedResolutionFieldDescription + " - ADD to WRK", statusEvent.SL_Reference);

			AssertNotNull(incident.Logs.MostRecentLogByEventTime(Events.EntryWorkInProgress));
			AssertNull(incident.Logs.MostRecentLogByEventTime(Events.CallBackClient));
			AssertNull(incident.Logs.MostRecentLogByEventTime(Events.IncidentClosed));

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			incident.IM_ResolutionCode = "CBK";
			Factory.Save();
			AssertNotNull(incident.Logs.MostRecentLogByEventTime(Events.CallBackClient));
			AssertNull(incident.Logs.MostRecentLogByEventTime(Events.IncidentClosed));

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			incident.IM_Status = "CLS";
			incident.IM_ResolutionCode = "TRN";
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			Factory.Save();
			statusEvent = incident.Logs.MostRecentLogByEventTime(Events.StatusChange);
			AssertNotNull(statusEvent);
			AssertEquals("Category", "Stage" + " - SUP to DEF", statusEvent.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			Factory.Save();
			statusEvent = incident.Logs.MostRecentLogByEventTime(Events.StatusChange);
			AssertNotNull(statusEvent);
			AssertEquals("Category", "Stage" + " - DEF to SUP", statusEvent.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			incident.IM_ResolutionCode = "DEP";
			Factory.Save();
			statusEvent = incident.Logs.MostRecentLogByEventTime(Events.IncidentDevelopmentEstimateProvided);
			AssertNotNull(statusEvent);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			incident.IM_ResolutionCode = "FQP";
			Factory.Save();
			statusEvent = incident.Logs.MostRecentLogByEventTime(Events.IncidentFormalQuoteProvided);
			AssertNotNull(statusEvent);
		}

		public void TestUnexpectedCategoryChangeErrorReport()
		{
			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			incident.RelatedWorkItems.Add(workItem);
			incident.IM_Category = "DEF";
			Factory.Save();
			incident.IM_Category = "SUP";
			AssertEquals("SupportIncident.Unexpected_Category_Change", incident.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
			ErrorReporter.Clear();
		}

		public void TestLogging_SaveFailure()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "TS1";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "TS2";

			var incident = (SupportIncident)GetNewBusinessObject();
			AssignToStaff(incident, staff1);
			incident.IM_Category = "SUP";
			incident.IM_Status = "OPN";
			incident.IM_ResolutionCode = "AUC";
			incident.IM_Product = "ENT";

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedIncident = anotherFactory.Load<SupportIncident>(incident.PK);
			loadedIncident.IM_Product = "SAP";
			anotherFactory.Save();

			AssignToStaff(incident, staff2);
			incident.IM_Category = "DEF";
			incident.IM_Status = "CLS";
			incident.IM_ResolutionCode = "SRS";

			void ThrowException(BusinessObjectFactory factory)
			{
				throw new Exception();
			}

			try
			{
				Factory.Saving += ThrowException;
				Factory.Save();
			}
			catch
			{
				Factory.Saving -= ThrowException;
			}

			var logs = incident.Logs.GetAllLogs().Cast<StmALog>();
			AssertEquals(0, logs.Count(log => log.SL_Reference == "Assigned User - TS1 to TS2"));
			AssertEquals(0, logs.Count(log => log.SL_Reference == "Stage" + " - SUP to DEF"));
			AssertEquals(0, logs.Count(log => log.SL_Reference == ExpectedStatusFieldDescription + " - OPN to CLS"));
			AssertEquals(0, logs.Count(log => log.SL_Reference == ExpectedResolutionFieldDescription + " - AUC to SRS"));
			AssertEquals(0, logs.Count(log => log.SL_SE_NKEvent == Events.IncidentClosedCode));

			incident.Reload();
			AssignToStaff(incident, staff2);
			incident.IM_Category = "DEF";
			incident.IM_Status = "CLS";
			incident.IM_ResolutionCode = "SRS";

			Factory.Save();
			logs = incident.Logs.GetAllLogs().Cast<StmALog>();
			AssertEquals(1, logs.Count(log => log.SL_Reference == "Assigned User - TS1 to TS2"));
			AssertEquals(1, logs.Count(log => log.SL_Reference == "Stage" + " - SUP to DEF"));
			AssertEquals(1, logs.Count(log => log.SL_Reference == ExpectedStatusFieldDescription + " - OPN to CLS"));
			AssertEquals(1, logs.Count(log => log.SL_Reference == ExpectedResolutionFieldDescription + " - AUC to SRS"));
		}

		protected virtual string ExpectedStatusFieldDescription
		{
			get { return "Status"; }
		}

		protected virtual string ExpectedResolutionFieldDescription
		{
			get { return "Disposition"; }
		}

		public virtual void TestLogsAreRemovedWhenSaveFails()
		{
			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "STF";
			incident.IM_GS_NKAssignedToCurrent = staff.GS_Code;
			incident.IM_Status = "OPN";
			Factory.Save();
			incident.IM_Status = IncidentMainLookups.Status.Working;
			int originalLogCount = incident.Logs.GetAllLogs().Count;
			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving_BlowUp);

			try
			{
				Factory.Save();
			}
			catch
			{
			}

			AssertEquals("Logs.GetAllLogs().Count", originalLogCount, incident.Logs.GetAllLogs().Count);

			Factory.Saving -= new BusinessObjectFactory.SavingEventHandler(Factory_Saving_BlowUp);
			Factory.Save();
			AssertEquals("Logs.GetAllLogs().Count", originalLogCount + 1, incident.Logs.GetAllLogs().Count);
		}

		#endregion

		#region Properties

		public void TestContactClearedWhenClientChanged()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();

			OrgHeader org2 = Factory.New<OrgHeader>();
			OrgContact contact2 = org2.Contacts.AddNew();

			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;

			incident.IM_OH_Client = org2.PK;
			AssertEquals(ZGuid.Empty, incident.IM_OC_Contact);

			incident.IM_OC_Contact = contact2.PK;
			incident.IM_OH_Client = org2.PK;
			AssertEquals(contact2.PK, incident.IM_OC_Contact);

			incident.IM_OH_Client = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, incident.IM_OC_Contact);
		}

		public void TestIncidentNumber()
		{
			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			AssertEquals(true, incident.IM_IncidentNumberInfo.ReadOnly);
		}

		public void TestContactPhone()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "blah blah lola lola";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "100 Fake St";
			org.MainAddress.OA_Phone = "33445566";
			OrgAddress branchAddress = org.Addresses.AddNew();
			branchAddress.OA_Phone = "919919";

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Zubin Appoo";
			contact1.OC_Phone = "99112233";
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "John K";
			contact2.OC_OA_OrgAddress = branchAddress.PK;

			SupportIncident incident = (SupportIncident)GetNewBusinessObject();

			incident.IM_OA_BranchAddress = org.MainAddress.PK;
			incident.IM_OC_Contact = contact1.PK;
			AssertEquals("99112233", incident.ContactPhone);

			contact1.OC_Phone = "";
			AssertEquals("33445566", incident.ContactPhone);

			incident.IM_OC_Contact = contact2.PK;
			AssertEquals("919919", incident.ContactPhone);
		}

		public void TestContactPhoneWithNullBranchAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();

			var incident = (SupportIncident)GetNewBusinessObject();
			incident.IM_OA_BranchAddress = ZGuid.NewZGuid();
			incident.IM_OC_Contact = contact.PK;

			AssertEquals(ZString.Empty, Incident.ContactPhone);
		}

		public void TestGetPhoneWithFallbackForDisplay()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Demo_Company";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Phone = "111111111";
			org.MainAddress.OA_Address1 = "100 Fake St";
			OrgAddress branchAddress = org.Addresses.AddNew();
			branchAddress.OA_Address1 = "123 Fake St";
			branchAddress.OA_Phone = "333333333";

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "John Smith";
			contact1.OC_Phone = "222222222";

			SupportIncident incident = (SupportIncident)GetNewBusinessObject();

			incident.IM_OA_BranchAddress = org.MainAddress.PK;
			incident.IM_OC_Contact = contact1.PK;
			AssertEquals("Dir: 222222222", incident.ContactPhoneForDisplay);

			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Jane Doe";
			contact2.OC_OA_OrgAddress = branchAddress.PK;

			incident.IM_OC_Contact = contact2.PK;
			AssertEquals("Off: 333333333", incident.ContactPhoneForDisplay);

			OrgContact contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "John K";

			incident.IM_OC_Contact = contact3.PK;
			AssertEquals("Off: 111111111", incident.ContactPhoneForDisplay);
		}

		public void TestContactEmail()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "blah blah lola lola";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Email = "company@company.com";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Samuel";
			contact.OC_Email = "Samuel.Wang@cargowise.com";

			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			AssertEquals("", incident.ContactEmail);

			incident.IM_OC_Contact = contact.PK;
			AssertEquals("Samuel.Wang@cargowise.com", incident.ContactEmail);

			contact.OC_Email = "";

			AssertEquals("company@company.com", incident.ContactEmail);
		}

		public void TestChargeablePropertiesReadOnlyBaseLogic()
		{
			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			AssertEquals(true, incident.IM_ActualHoursWorkedInfo.ReadOnly);
			AssertEquals(true, incident.IM_QuoteAmountInfo.ReadOnly);
			AssertEquals(true, incident.IM_RX_NKQuoteCurrencyInfo.ReadOnly);

			incident.IM_ChargableWork = true;
			AssertEquals(false, incident.IM_ActualHoursWorkedInfo.ReadOnly);
			AssertEquals(false, incident.IM_QuoteAmountInfo.ReadOnly);
			AssertEquals(false, incident.IM_RX_NKQuoteCurrencyInfo.ReadOnly);
		}

		public void TestDefaultAddress()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Honey bunch sugar pops scwumpy wumpy wumpy";

			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			org.Addresses.Load();
			incident.IM_OA_BranchAddress_ZAddress.OrgPK = org.PK;
			AssertEquals(org.MainAddress.PK, incident.IM_OA_BranchAddress);
		}

		public void TestIM_OA_BranchAddress_ContactWorkplace()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "org";

			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			var adr1 = org.Addresses.AddNew();
			adr1.OA_RN_NKCountryCode = "UA";

			var adr2 = org.Addresses.AddNew();
			adr2.OA_RN_NKCountryCode = "NZ";

			var contact = org.Contacts.AddNew();
			contact.WorkingAddressPK = adr2.PK;

			incident.IM_OH_Client = org.PK;

			AssertEquals(ZGuid.Empty, incident.IM_OA_BranchAddress);

			incident.IM_OC_Contact = contact.PK;

			AssertEquals(adr2.PK, incident.IM_OA_BranchAddress);

			contact.WorkingAddressPK = adr1.PK;
			incident.IM_OC_Contact = ZGuid.Empty;

			AssertEquals(org.MainAddress.PK, incident.IM_OA_BranchAddress);

			incident.IM_OC_Contact = contact.PK;
			AssertEquals(adr1.PK, incident.IM_OA_BranchAddress);
		}

		public void TestContactPrimaryWorkplace()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "org";

			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			incident.IM_OH_Client = org.PK;
			var adr1 = org.Addresses.AddNew();
			adr1.OA_RN_NKCountryCode = "UA";
			adr1.OA_CompanyNameOverride = "name 1";

			var adr2 = org.Addresses.AddNew();
			adr2.OA_RN_NKCountryCode = "NZ";
			adr2.OA_CompanyNameOverride = "name 2";

			var contact = org.Contacts.AddNew();

			AssertEquals(ZString.Empty, incident.ContactPrimaryWorkplace);

			incident.IM_OC_Contact = contact.PK;
			AssertEquals("org", incident.ContactPrimaryWorkplace);

			contact.WorkingAddressPK = adr2.PK;
			AssertEquals("name 2", incident.ContactPrimaryWorkplace);

			contact.WorkingAddressPK = adr1.PK;
			AssertEquals("name 1", incident.ContactPrimaryWorkplace);
		}

		public void TestFeatureRequestContactPrimaryWorkplace()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "org";

			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			incident.IM_OH_Client = org.PK;
			var adr1 = org.Addresses.AddNew();
			adr1.OA_RN_NKCountryCode = "UA";
			adr1.OA_CompanyNameOverride = "name 1";

			var adr2 = org.Addresses.AddNew();
			adr2.OA_RN_NKCountryCode = "NZ";
			adr2.OA_CompanyNameOverride = "name 2";

			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();

			AssertEquals(ZString.Empty, incident.ContactPrimaryWorkplace);

			incident.IM_OC_Contact = contact1.PK;
			AssertEquals("", incident.FeatureRequestContactPrimaryWorkplace);

			incident.FeatureRequestContactPK = contact2.PK;
			AssertEquals("org", incident.FeatureRequestContactPrimaryWorkplace);

			contact1.WorkingAddressPK = adr2.PK;
			AssertEquals("org", incident.FeatureRequestContactPrimaryWorkplace);

			contact2.WorkingAddressPK = adr2.PK;
			AssertEquals("name 2", incident.FeatureRequestContactPrimaryWorkplace);

			contact1.WorkingAddressPK = adr1.PK;
			AssertEquals("name 2", incident.FeatureRequestContactPrimaryWorkplace);

			contact2.WorkingAddressPK = adr1.PK;
			AssertEquals("name 1", incident.FeatureRequestContactPrimaryWorkplace);
		}

		public void TestModuleMaxLength()
		{
			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			AssertEquals(3, incident.IM_ModuleInfo.MaxLength);
		}

		public void TestModuleDescription()
		{
			SupportIncident workTask = (SupportIncident)GetNewBusinessObject();
			workTask.IM_Product = "ENT";
			workTask.IM_Module = "";
			AssertEquals("", workTask.ModuleDescription);

			workTask.IM_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.ArchiveManager;
			AssertEquals((ZString)workTask.Lookups.GetModuleList("ENT").GetDescriptionFromCode(ModuleTreeCustomerServiceMenuSectionList.Codes.ArchiveManager), workTask.ModuleDescription);

			workTask.IM_Module = "XXX";
			AssertEquals((ZString)workTask.Lookups.GetModuleList("ENT").GetDescriptionFromCode("XXX"), workTask.ModuleDescription);
		}

		public void TestModuleDescriptionWithNonUniqueModuleCode()
		{
			var collection = new SystemProductCollection();
			var product1 = collection.AddNew();
			product1.Code = "HOB";
			product1.Description = "HOB desc";
			var child1 = product1.ModuleMappings.AddNew();
			child1.ModuleCode = "TWO";
			child1.ModuleDescription = (NoResString)"Smeagol";

			var product2 = collection.AddNew();
			product2.Code = "LOT";
			product2.Description = "LOT desc";
			var child2 = product2.ModuleMappings.AddNew();
			child2.ModuleCode = "TWO";
			child2.ModuleDescription = (NoResString)"Gollum";

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			SupportIncident workTask = (SupportIncident)GetNewBusinessObject();
			workTask.IM_Product = "HOB";
			workTask.IM_Module = "TWO";
			AssertEquals((ZString)workTask.Lookups.GetModuleList("HOB").GetDescriptionFromCode("TWO"), workTask.ModuleDescription);

			workTask.IM_Product = "LOT";
			AssertEquals((ZString)workTask.Lookups.GetModuleList("LOT").GetDescriptionFromCode("TWO"), workTask.ModuleDescription);
		}

		public void TestIsCurrentModuleEnabled()
		{
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "AAA Enabled Module", ProductAreaList.Codes.ARC, false);
			product.ModuleMappings.AddNew("BBB", "BBB Disabled Module", ProductAreaList.Codes.ARC, false, false, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var incidentWithEnabledModule = Factory.New<SupportIncident>();
			incidentWithEnabledModule.IM_Product = ProductTypes.Codes.Enterprise;
			incidentWithEnabledModule.IM_Module = "AAA";
			incidentWithEnabledModule.IM_Priority = "CR8";

			var incidentWithDisabledModule = Factory.New<SupportIncident>();
			incidentWithDisabledModule.IM_Product = ProductTypes.Codes.Enterprise;
			incidentWithDisabledModule.IM_Module = "BBB";
			incidentWithDisabledModule.IM_Priority = "CR8";

			var incidentWithNoModule = Factory.New<SupportIncident>();
			incidentWithNoModule.IM_Product = ProductTypes.Codes.Enterprise;
			incidentWithNoModule.IM_Module = "CCC";
			incidentWithNoModule.IM_Priority = "CR7";

			Factory.Save();

			AssertEquals(incidentWithDisabledModule.ModuleDescription, "BBB Disabled Module");
			AssertEquals(incidentWithEnabledModule.IsCurrentModuleEnabled, true);
			AssertEquals(incidentWithDisabledModule.IsCurrentModuleEnabled, false);
			AssertEquals(incidentWithNoModule.IsCurrentModuleEnabled, false);
		}

		public void TestProductDescription()
		{
			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			incident.IM_Product = "";
			AssertEquals("", incident.ProductDescription);

			incident.IM_Product = "ENT";
			AssertEquals(ProductTypes.Descriptions.CargoWise, incident.ProductDescription);

			incident.IM_Product = "XXX";
			AssertEquals("", incident.ProductDescription);
		}

		public void TestGetRecalculatedProductArea()
		{
			#region Test Data

			CodeDescriptionPairList eDIAreaList = new CodeDescriptionPairList();
			eDIAreaList.AddPair("CFA", "Category Fruits Area A");
			eDIAreaList.AddPair("CFB", "Category Fruits Area B");
			eDIAreaList.AddPair("8FA", "CR8 Fruits Area A");
			eDIAreaList.AddPair("8FB", "CR8 Fruits Area B");
			eDIAreaList.AddPair("9FA", "CR9 Fruits Area A");
			eDIAreaList.AddPair("9FB", "CR9 Fruits Area B");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, eDIAreaList);

			SystemProductCollection collection = new SystemProductCollection();
			var prod = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", true);
			prod.ModuleMappings.AddNew("APP", "Apples", "CFA", false);
			prod.ModuleMappings.AddNew("TOM", "Tomatoes", "CFB", false);

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			prod = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", true);
			prod.ModuleMappings.AddNew("APP", "Apples", "8FA", false);
			prod.ModuleMappings.AddNew("BAN", "Bannanas", "8FB", false);

			var nonEDIproduct = collection.AddNew();
			nonEDIproduct.Code = "VEG";
			nonEDIproduct.Description = (NoResString)"Vegetables";
			var nonEDImodule = nonEDIproduct.ModuleMappings.AddNew();
			nonEDImodule.ModuleCode = "TOM";
			nonEDImodule.ModuleDescription = (NoResString)"Tomatoes";
			nonEDImodule.ProductArea = "CFA";

			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			prod = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", true);
			prod.ModuleMappings.AddNew("APP", "Apples", "9FA", false);
			prod.ModuleMappings.AddNew("PEA", "Pears", "9FB", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Factory.Save();

			LegacyModuleMappingCollection legacyMenuSectionMappings = new LegacyModuleMappingCollection(ModuleListType.MenuSection);
			legacyMenuSectionMappings.AddNew("LAP", "Legacy Apples", "", "APP");
			EDIDataRegistry.Instance.LegacyMenuSectionMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, legacyMenuSectionMappings);

			LegacyModuleMappingCollection legacycr8ModuleMappings = new LegacyModuleMappingCollection(ModuleListType.Cr8);
			legacycr8ModuleMappings.AddNew("LBA", "Legacy Bannanas", "", "BAN");
			EDIDataRegistry.Instance.LegacyCr8ModuleMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, legacycr8ModuleMappings);

			LegacyModuleMappingCollection legacycr9ModuleMappings = new LegacyModuleMappingCollection(ModuleListType.Cr9);
			legacycr9ModuleMappings.AddNew("LPE", "Legacy Pears", "", "PEA");
			EDIDataRegistry.Instance.LegacyCr9ModuleMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, legacycr9ModuleMappings);

			#endregion

			var workTask = Factory.New<SupportIncident>();
			workTask.IM_Product = "ENT";
			workTask.IM_Priority = "";
			workTask.IM_Module = "APP";
			AssertEquals("CFA", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "";
			workTask.IM_Module = "BAN";
			AssertEquals("8FB", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "";
			workTask.IM_Module = "PEA";
			AssertEquals("9FB", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "CR4";
			workTask.IM_Module = "APP";
			AssertEquals("CFA", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "CR8";
			workTask.IM_Module = "APP";
			AssertEquals("8FA", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "CR9";
			workTask.IM_Module = "APP";
			AssertEquals("9FA", workTask.GetRecalculatedProductArea());

			#region Test Product Area for Legacy

			workTask.IM_Priority = "CR3";
			workTask.IM_Module = "LAP";
			AssertEquals("CFA", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "CR8";
			workTask.IM_Module = "LBA";
			AssertEquals("8FB", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "CR9";
			workTask.IM_Module = "LPE";
			AssertEquals("9FB", workTask.GetRecalculatedProductArea());

			#endregion

			workTask.IM_Product = "VEG";
			workTask.IM_Module = "TOM";
			AssertEquals("", workTask.GetRecalculatedProductArea());

			workTask.IM_Priority = "CR8";
			AssertEquals("CFA", workTask.GetRecalculatedProductArea());
		}

		public virtual void TestDetailNoteText()
		{
			const string testValue = "NEMIROFF IS BETTER THAN STOLICHNAYA";
			bool refreshBindingCalled = false;
			Incident.DetailNoteTextInfo.ValueChanged += delegate
			{ refreshBindingCalled = true; };
			Incident.HasChanges = false;

			Incident.DetailNoteText = testValue;
			Assert("RefreshBinding should be called", refreshBindingCalled);
			AssertEquals(testValue, Incident.DetailNoteText);
			Assert(Incident.HasChanges);

			Incident.SynchroniseNotes();
			StmNote[] notes = Incident.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.IncidentDetail.Description);
			AssertEquals("Should be stored in StmNote object", testValue, notes[0].ST_NoteText);
		}

		public void TestSetLongDetailNoteText()
		{
			void assertExtraNotes(SupportIncident inc, int noteLen)
			{
				var maxLength = inc.DetailNoteText_MaxLength;
				var notes = inc.Notes.GetAllNotes().OfType<StmNote>().ToArray();

				if (noteLen <= maxLength)
				{
					AssertEquals(new string('0', noteLen), inc.DetailNoteText);
					AssertEquals(1, notes.Length);
					AssertEquals(new string('0', noteLen), notes.Single().ST_NoteText);
					AssertEquals("Incident Detail", notes.Single().ST_Description);
				}
				else
				{
					const string footerText = "\r\nContinued on the Notes (Extra Incident Detail).";
					var firstNoteText = $"{new string('0', maxLength - footerText.Length)}{footerText}";
					AssertEquals(firstNoteText, inc.DetailNoteText);
					AssertEquals(firstNoteText, notes.Single(x => x.ST_Description == "Incident Detail").ST_NoteText);

					var totalExtraNotes = (int)Math.Ceiling((decimal)(noteLen + footerText.Length) / maxLength) - 1;
					AssertEquals(totalExtraNotes, notes.Length - 1);
					AssertEquals(noteLen, notes.Sum(x => x.ST_NoteText.Length) - footerText.Length);

					for (var idx = 0; idx < totalExtraNotes; idx++)
					{
						AssertNotNull(notes.Single(x => x.ST_Description == $"Extra Incident Detail {(idx + 1)}/{totalExtraNotes}"));
					}
				}
			}

			var incident0 = Factory.NewWithValidTestData<SupportIncident>();
			var maxLen = incident0.DetailNoteText_MaxLength;
			incident0.SetLongDetailNoteText(new string('0', maxLen / 2));

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.SetLongDetailNoteText(new string('0', maxLen));

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.SetLongDetailNoteText(new string('0', maxLen - 1));

			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.SetLongDetailNoteText(new string('0', maxLen + 1));

			var incident4 = Factory.NewWithValidTestData<SupportIncident>();
			incident4.SetLongDetailNoteText(new string('0', maxLen * 2));

			var incident5 = Factory.NewWithValidTestData<SupportIncident>();
			incident5.SetLongDetailNoteText(new string('0', maxLen * 3 + 10));

			Factory.Save();

			assertExtraNotes(incident0, maxLen / 2);
			assertExtraNotes(incident1, maxLen);
			assertExtraNotes(incident2, maxLen - 1);
			assertExtraNotes(incident3, maxLen + 1);
			assertExtraNotes(incident4, maxLen * 2);
			assertExtraNotes(incident5, maxLen * 3 + 10);
		}

		public void TestDetailNoteTextInfo()
		{
			AssertEquals("DetailNoteText", Incident.DetailNoteTextInfo.Name);
			AssertEquals(EDIPredefinedNoteTypes.Instance.IncidentDetail.TextOnlyMaxLength, Incident.DetailNoteTextInfo.MaxLength);
		}

		public void TestSynchroniseNotes()
		{
			KeyValuePair<PredefinedNoteType, ZPropertyInfo>[] synchedNoteTextToTest = GetSynchronisedNoteTextToTest(Incident);
			foreach (KeyValuePair<PredefinedNoteType, ZPropertyInfo> keyValuePair in synchedNoteTextToTest)
			{
				PredefinedNoteType noteType = keyValuePair.Key;
				ZPropertyInfo propertyInfo = keyValuePair.Value;

				StmNote[] notes = Incident.Notes.FindByDescription(noteType.Description);
				AssertEquals("Should only be lazy created when needed", 0, notes.Length);

				propertyInfo.SetValueFromString("MEH MEH");
				notes = Incident.Notes.FindByDescription(noteType.Description);
				AssertEquals("Should be created now", 1, notes.Length);
				StmNote logNote = notes[0];

				propertyInfo.SetValueFromString("");
				Assert("Should be deleted if empty", logNote.IsDeleted);

				propertyInfo.SetValueFromString("NEW TEXT");
				notes = Incident.Notes.FindByDescription(noteType.Description);
				AssertEquals("Should create a new StmNote object", 1, notes.Length);
				Assert("Should create a new StmNote object", !notes[0].IsDeleted);
				AssertNotEquals("Should be different instance", notes[0], logNote);

				AssertEquals("MaxLength", logNote.ST_NoteTextInfo.MaxLength, propertyInfo.MaxLength);
			}
		}

		public void TestSynchroniseNotes_ShouldNotCauseHasChangesAfterSaveSucceeded()
		{
			SupportIncident workTask = (SupportIncident)GetNewBusinessObject();
			workTask.FillWithValidTestData();
			KeyValuePair<PredefinedNoteType, ZPropertyInfo>[] synchedNoteTextToTest = GetSynchronisedNoteTextToTest(workTask);
			synchedNoteTextToTest[0].Value.SetValueFromString("meh");
			Factory.Save();
			Assert(!workTask.Notes.FindByDescription(synchedNoteTextToTest[0].Key.Description)[0].HasChanges);
			Assert(!workTask.HasChanges);

			synchedNoteTextToTest[0].Value.SetValueFromString("Meh Meh");
			Factory.Save();
			Assert(!workTask.Notes.FindByDescription(synchedNoteTextToTest[0].Key.Description)[0].HasChanges);
			Assert(!workTask.HasChanges);
		}

		protected virtual KeyValuePair<PredefinedNoteType, ZPropertyInfo>[] GetSynchronisedNoteTextToTest(SupportIncident incident)
		{
			return new KeyValuePair<PredefinedNoteType, ZPropertyInfo>[]
				{
					new KeyValuePair<PredefinedNoteType, ZPropertyInfo>(EDIPredefinedNoteTypes.Instance.IncidentDetail, incident.DetailNoteTextInfo)
				};
		}

		#endregion

		#region Business Object Overrides

		public void TestHumanReadableNameAndShortcutName()
		{
			var workTask = (SupportIncident)GetNewBusinessObject();

			workTask.IM_IncidentNumber = "blah";
			AssertEquals("blah", workTask.HumanReadableShortcutName);
			AssertEquals("blah", workTask.HumanReadableName);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CODE";
			workTask.IM_OH_Client = org.PK;
			AssertEquals("blah - CODE", workTask.HumanReadableShortcutName);
			AssertEquals("blah - CODE", workTask.HumanReadableName);

			workTask.IM_OH_Client = ZGuid.Empty;
			workTask.IM_Description = "BROKEN PLANE";
			AssertEquals("blah - BROKEN PLANE", workTask.HumanReadableShortcutName);
			AssertEquals("blah - BROKEN PLANE", workTask.HumanReadableName);

			workTask.IM_OH_Client = org.PK;
			AssertEquals("blah - CODE - BROKEN PLANE", workTask.HumanReadableShortcutName);
			AssertEquals("blah - CODE - BROKEN PLANE", workTask.HumanReadableName);
		}

		#endregion

		#region LicenceForTest

		protected LicenceHeader LicenceForTest
		{
			get
			{
				if (fLicenceForTest == null)
				{
					OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

					org.OH_FullName = "blah blah lola lola";
					org.OH_RL_NKClosestPort = "AUSYD";
					OrgContact contact = org.Contacts.AddNew();
					contact.OC_ContactName = "Zubin Appoo";
					contact.OC_Email = "zubin.appoo@edi.com.au";

					LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
					enterprise.LE_EnterpriseCode = "ENT";
					enterprise.LE_OH = org.PK;

					LicenceCompany company = Factory.New<LicenceCompany>();
					company.LC_CompanyCode = "COM";
					company.LC_LE = enterprise.PK;
					company.LC_OH = org.PK;

					ReleaseBuild someBuild = Factory.New<ReleaseBuild>();
					someBuild.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);

					LicenceDatabase database = Factory.New<LicenceDatabase>();
					database.LD_ServerCode = "SRV";
					database.LD_LE = enterprise.PK;
					database.LD_HL_CurrentRunningVersion = someBuild.PK;
					database.LD_PublicEmailAddressForUpdate = "test@test.com";

					fLicenceForTest = Factory.New<LicenceHeader>();
					fLicenceForTest.LA_LC = company.PK;
					fLicenceForTest.LA_LD = database.PK;
				}
				return fLicenceForTest;
			}
		}
		LicenceHeader fLicenceForTest;

		#endregion

		#region Notes / Logs

		public void TestNoteTypes()
		{
			var incident = (SupportIncident)GetNewBusinessObject();
			var expectedNoteTypes = GetExpectedNoteTypes();
			AssertEquals(expectedNoteTypes.Length, incident.NoteTypes.Count);
			foreach (var expectedNoteType in expectedNoteTypes)
			{
				AssertCollectionContains(expectedNoteType, incident.NoteTypes);
			}
		}

		[TestDate(2006, 6, 1, 9, 0, 0)]
		public void TestLogNote()
		{
			SupportIncident incident = (SupportIncident)GetNewBusinessObject();

			AssertEquals("", incident.LogText);
			string existingNote = incident.LogText;
			incident.SetLogTextForTest("Haha");
			AssertEquals(true, incident.HasChanges);
			incident.RunPreSaveValidation();
			AssertEquals("Haha", incident.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.IncidentLog.Description)[0].ST_NoteText);

			AssertEquals(true, incident.LogTextInfo.ReadOnly);
			AssertEquals(EDIPredefinedNoteTypes.Instance.IncidentLog.TextOnlyMaxLength, incident.LogTextInfo.MaxLength);
		}

		#endregion

		#region Tasks

		public void TestGetCurrentTask()
		{
			AssertCurrentTaskWithSameSequence(includeNextTask: false);

			AssertCurrentTask(
				-1, includeNextTask: false,
				ProcessTaskStatusCodeList.Codes.Cancelled,
				ProcessTaskStatusCodeList.Codes.Cancelled,
				ProcessTaskStatusCodeList.Codes.Closed,
				ProcessTaskStatusCodeList.Codes.Closed,
				ProcessTaskStatusCodeList.Codes.Cancelled);

			AssertCurrentTask(
				1, includeNextTask: false,
				ProcessTaskStatusCodeList.Codes.Open,
				ProcessTaskStatusCodeList.Codes.Working,
				ProcessTaskStatusCodeList.Codes.Closed,
				ProcessTaskStatusCodeList.Codes.Closed,
				ProcessTaskStatusCodeList.Codes.Cancelled);

			AssertCurrentTask(
				3, includeNextTask: false,
				ProcessTaskStatusCodeList.Codes.Open,
				ProcessTaskStatusCodeList.Codes.Cancelled,
				ProcessTaskStatusCodeList.Codes.Closed,
				ProcessTaskStatusCodeList.Codes.Assigned,
				ProcessTaskStatusCodeList.Codes.Cancelled);

			AssertCurrentTask(
				1, includeNextTask: false,
				ProcessTaskStatusCodeList.Codes.Cancelled,
				ProcessTaskStatusCodeList.Codes.Suspended,
				ProcessTaskStatusCodeList.Codes.Closed,
				ProcessTaskStatusCodeList.Codes.Open,
				ProcessTaskStatusCodeList.Codes.Cancelled);

			AssertCurrentTask(
				1, includeNextTask: false,
				ProcessTaskStatusCodeList.Codes.Cancelled,
				ProcessTaskStatusCodeList.Codes.Suspended,
				ProcessTaskStatusCodeList.Codes.Closed,
				ProcessTaskStatusCodeList.Codes.Working,
				ProcessTaskStatusCodeList.Codes.Cancelled);
		}

		public void TestGetCurrentOrNextTask()
		{
			AssertCurrentTaskWithSameSequence(includeNextTask: true);

			AssertCurrentTask(
				-1, includeNextTask: true,
				ProcessTaskStatusCodeList.Codes.Cancelled,
				ProcessTaskStatusCodeList.Codes.Cancelled,
				ProcessTaskStatusCodeList.Codes.Closed,
				ProcessTaskStatusCodeList.Codes.Closed,
				ProcessTaskStatusCodeList.Codes.Cancelled);

			AssertCurrentTask(
				1, includeNextTask: true,
				ProcessTaskStatusCodeList.Codes.Cancelled,
				ProcessTaskStatusCodeList.Codes.Working,
				ProcessTaskStatusCodeList.Codes.Closed,
				ProcessTaskStatusCodeList.Codes.Closed,
				ProcessTaskStatusCodeList.Codes.Cancelled);

			AssertCurrentTask(
				0, includeNextTask: true,
				ProcessTaskStatusCodeList.Codes.Open,
				ProcessTaskStatusCodeList.Codes.Cancelled,
				ProcessTaskStatusCodeList.Codes.Closed,
				ProcessTaskStatusCodeList.Codes.Closed,
				ProcessTaskStatusCodeList.Codes.Cancelled);

			AssertCurrentTask(
				2, includeNextTask: true,
				ProcessTaskStatusCodeList.Codes.Open,
				ProcessTaskStatusCodeList.Codes.Cancelled,
				ProcessTaskStatusCodeList.Codes.Working,
				ProcessTaskStatusCodeList.Codes.Assigned,
				ProcessTaskStatusCodeList.Codes.Cancelled);

			AssertCurrentTask(
				1, includeNextTask: true,
				ProcessTaskStatusCodeList.Codes.Cancelled,
				ProcessTaskStatusCodeList.Codes.Suspended,
				ProcessTaskStatusCodeList.Codes.Closed,
				ProcessTaskStatusCodeList.Codes.Open,
				ProcessTaskStatusCodeList.Codes.Cancelled);
		}

		public void TestCurrentStartableTask()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			CurrentTaskTestHelper.AssertCurrentStartableTask(incident, () => incident.CurrentTask, createBMSystem: false);
		}

		public void TestCurrentOrNextStartableTask()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			CurrentTaskTestHelper.AssertCurrentOrNextStartableTask(incident, () => incident.CurrentOrNextTask, createBMSystem: false);
		}

		public void TestNotesValidationSuspended()
		{
			var incident = Factory.New<SupportIncident>();
			incident.SetLogTextForTest("x");
			incident.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.IncidentLog.Description)[0].Validation.ValidateAll();
			AssertEquals("Validation should be suspended", "", incident.Notes.GetAllNotes().GetErrors().ToUniqueMessageListString());
		}

		void AssertCurrentTask(int indexValid, bool includeNextTask, params string[] codes)
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();

			using (ProcessTaskCollection.CanCreateTaskCollection())
			{
				var helper = ObjectFactory.Get<IBMTestHelper>();
				var jobHeader = helper.GetJobHeaderForParent(incident, Factory, addDefaultProcessHeaderIfNone: false);
				var workflow = helper.CreateWorkflow(jobHeader, "Workflow 1");
				var taskCollection = incident.WorkflowItems;
				ProcessTask expectedCurrentTask = null;

				for (int i = 0; i < codes.Length; i++)
				{
					ProcessTask task = taskCollection.AddNew();
					task.P9_Status = codes[i];
					task.P9_FH_ProcessHeader = workflow.PK;
					if (indexValid == i)
					{
						expectedCurrentTask = task;
					}
				}

				// Add a milestone to ensure that the milestone is never selected
				taskCollection.Milestones.AddNew().P9_Status = (indexValid >= 0) ? codes[indexValid] : ProcessTaskStatusCodeList.Codes.Working;

				var currentTask = includeNextTask ? incident.CurrentOrNextTask : incident.CurrentTask;

				if (indexValid >= 0)
				{
					AssertEquals(expectedCurrentTask, currentTask);
				}
				else
				{
					AssertNull(currentTask);
				}
			}
		}

		void AssertCurrentTaskWithSameSequence(bool includeNextTask)
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();

			using (ProcessTaskCollection.CanCreateTaskCollection())
			{
				var helper = ObjectFactory.Get<IBMTestHelper>();
				var jobHeader = helper.GetJobHeaderForParent(incident, Factory, addDefaultProcessHeaderIfNone: false);
				var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow 1");
				var workflow2 = helper.CreateWorkflow(jobHeader, "Workflow 2");
				var workflow3 = helper.CreateWorkflow(jobHeader, "Workflow 3");

				var taskCollection = incident.WorkflowItems;
				var task1 = Factory.NewWithPrimaryKey<SupportIncidentProcessTask>(new Guid("979520de-ba34-4339-b9ae-03ed90c5c864"));
				var task2 = Factory.NewWithPrimaryKey<SupportIncidentProcessTask>(new Guid("079520de-ba34-4339-b9ae-03ed90c5c864"));
				var task3 = Factory.NewWithPrimaryKey<SupportIncidentProcessTask>(new Guid("479520de-ba34-4339-b9ae-03ed90c5c864"));
				task1.P9_TaskID = "T00004003";
				task2.P9_TaskID = "T00004001";
				task3.P9_TaskID = "T00004002";
				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				task3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				taskCollection.Add(task1);
				taskCollection.Add(task2);
				taskCollection.Add(task3);
				task1.P9_Sequence = 1;
				task2.P9_Sequence = 1;
				task3.P9_Sequence = 1;
				task1.P9_FH_ProcessHeader = workflow1.PK;
				task2.P9_FH_ProcessHeader = workflow2.PK;
				task3.P9_FH_ProcessHeader = workflow3.PK;

				var actualCurrentTask = includeNextTask ? incident.CurrentOrNextTask : incident.CurrentTask;
				AssertEquals(task2.PK, actualCurrentTask.PK);
			}
		}

		public void TestSaveFactory_ShouldNotReApplyWorkflowTemplates()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "INC");
			var templateWorkflow = bmTestHelper.CreateWorkflow(template);
			var templateTask1 = bmTestHelper.CreateTask(template, templateWorkflow, description: "Marshmellow");
			var templateTask2 = bmTestHelper.CreateTask(template, templateWorkflow, description: "Happier");

			templateTask1.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask1.TemplateConditions.TemplateCondition2Value = "\"<P9_Description>\"!=\"\"";
			templateTask2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask2.TemplateConditions.TemplateCondition2Value = "\"<P9_Description>\"!=\"\"";

			Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();

			Factory.Save();
			AssertHasCorrectTasks("Tasks should have been applied from the template on initial save");

			incident.IM_Description = "I want you to be happier";

			Factory.Save();
			AssertHasCorrectTasks("Saving again shouldn't create more tasks. If we were to apply Workflow in the standard way (in OnFactorySaving), then the places where incidents also manually apply templates would cause duplication.");

			void AssertHasCorrectTasks(string assertionMessage)
			{
				AssertSequencesEqual(assertionMessage, new[] { "Marshmellow", "Happier" }, incident.WorkflowItems.Tasks.Cast<ProcessTask>().Select(t => t.P9_Description.ToString()));
			}
		}

		#endregion Tasks

		public void TestLicenceOrganisation()
		{
			Incident.IM_OH_Client = ZGuid.Empty;
			AssertNull(((IClientOrgLicenceProvider)Incident).LicenceOrganisation);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Incident.IM_OH_Client = org.PK;

			AssertEquals(((IClientOrgLicenceProvider)Incident).LicenceOrganisation, org);
		}

		public void TestSupportIncidentRelatedCommunicationsCollection()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var orgSalesCall1 = Factory.NewWithValidTestData<OrgSalesCall>();
			var orgSalesCall2 = Factory.NewWithValidTestData<OrgSalesCall>();
			var orgSalesCall3 = Factory.NewWithValidTestData<OrgSalesCall>();
			Factory.Save();

			var relatedActivityPivot1 = incident.RelatedChildActivityPivotCollection.AddNew();
			relatedActivityPivot1.RAP_ChildActivityID = orgSalesCall1.PK;
			relatedActivityPivot1.RAP_ChildActivityTableCode = OrgSalesCallSchema.Constants.Prefix;
			relatedActivityPivot1.RAP_ParentActivityID = incident.PK;
			relatedActivityPivot1.RAP_ParentActivityTableCode = IncidentMainSchema.Constants.Prefix;

			var relatedActivityPivot2 = incident.RelatedChildActivityPivotCollection.AddNew();
			relatedActivityPivot2.RAP_ChildActivityID = orgSalesCall2.PK;
			relatedActivityPivot2.RAP_ChildActivityTableCode = OrgSalesCallSchema.Constants.Prefix;
			relatedActivityPivot2.RAP_ParentActivityID = incident.PK;
			relatedActivityPivot2.RAP_ParentActivityTableCode = IncidentMainSchema.Constants.Prefix;

			var relatedActivityPivotAdditional = incident2.RelatedChildActivityPivotCollection.AddNew();
			relatedActivityPivotAdditional.RAP_ChildActivityID = orgSalesCall3.PK;
			relatedActivityPivotAdditional.RAP_ChildActivityTableCode = OrgSalesCallSchema.Constants.Prefix;
			relatedActivityPivotAdditional.RAP_ParentActivityID = incident2.PK;
			relatedActivityPivotAdditional.RAP_ParentActivityTableCode = IncidentMainSchema.Constants.Prefix;

			AssertNull("Collection should not contain irrelevant object", incident.SupportIncidentRelatedCommunicationsCollection.FindByPK(orgSalesCall3.PK));
			AssertEquals("Collection should be updated after adding", 2, incident.SupportIncidentRelatedCommunicationsCollection.Count);
			AssertEquals(1, incident.SupportIncidentRelatedCommunicationsCollection.Count(i => i.PK == orgSalesCall1.PK));
			AssertEquals(1, incident.SupportIncidentRelatedCommunicationsCollection.Count(i => i.PK == orgSalesCall2.PK));

			incident.RelatedChildActivityPivotCollection.Delete(relatedActivityPivot1);
			AssertEquals("Collection should be updated after deleting", 1, incident.SupportIncidentRelatedCommunicationsCollection.Count);

			var relatedActivityPivot3 = incident.RelatedChildActivityPivotCollection.AddNew();
			relatedActivityPivot3.RAP_ChildActivityID = orgSalesCall1.PK;
			relatedActivityPivot3.RAP_ChildActivityTableCode = OrgSalesCallSchema.Constants.Prefix;
			relatedActivityPivot3.RAP_ParentActivityID = incident.PK;
			relatedActivityPivot3.RAP_ParentActivityTableCode = IncidentMainSchema.Constants.Prefix;
			AssertEquals("Collection should be updated after adding again", 2, incident.SupportIncidentRelatedCommunicationsCollection.Count);
		}

		public void TestReferenceNumber()
		{
			Incident.IM_IncidentNumber = "IM2349238502";

			AssertEquals("IM2349238502", ((IClientOrgLicenceProvider)Incident).ReferenceNumber);
		}

		public void TestIAllowAttachEmailsToEDocs()
		{
			Incident.IM_IncidentNumber = "34324211";
			AssertEquals("34324211", ((IAllowAttachEmailsToEDocs)Incident).ReferenceNumber);
		}

		public void TestUpgradeDeployDate()
		{
			AssertEquals("", Incident.UpgradeDeployDate);
			Incident.IM_BugFixDeployed = new ZDateTime(2006, 11, 11);
			AssertEquals(new ZDateTime(2006, 11, 11).ToShortDateString(), Incident.UpgradeDeployDate);
		}

		public void TestNeedUpgrade()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_ReleaseRing = ReleaseRings.Codes.ALP;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_Priority = "AAA";
			incident1.IM_LD = database.PK;
			incident1.IM_LCC = clientCompany.PK;
			var workItem1 = incident1.RelatedWorkItems.AddNew();
			workItem1.FillWithValidTestData();

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_Priority = "AAA";
			incident2.IM_LD = database.PK;
			incident2.IM_LCC = clientCompany.PK;
			var workItem2 = incident2.RelatedWorkItems.AddNew();
			workItem2.FillWithValidTestData();

			ReleaseBuildContent.Factory.Value = _ => new MockReleaseBuildContent(workItem2);

			AssertEquals(false, incident1.NeedUpgrade);
			AssertEquals(true, incident2.NeedUpgrade);

			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_Priority = "AAA";
			var workItem3 = incident3.RelatedWorkItems.AddNew();
			workItem3.FillWithValidTestData();
			ReleaseBuildContent.Factory.Value = _ => new MockReleaseBuildContent(workItem3);
			AssertEquals(false, incident3.NeedUpgrade);
			AssertNull(incident3.Database);
		}

		public void TestIncidentCloseStatusWithNoOpenTasksWithTwoSession()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var incident1 = factory1.NewWithValidTestData<SupportIncident>();
			incident1.IM_Status = SupportIncidentLookups.Status.Open;
			var task1 = incident1.WorkflowItems.AddNew();
			task1.P9_Sequence = 10;
			task1.P9_Description = "task1_test";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			factory1.Save();

			incident1.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");

			var incident2 = factory2.Load<SupportIncident>(incident1.PK);
			var task2 = incident2.WorkflowItems.AddNew();
			task2.P9_Sequence = 20;
			task2.P9_Description = "task2_test";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			factory2.Save();

			factory1.Save();

			incident1.Reload();
			AssertEquals(2, incident1.WorkflowItems.Count);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident1.IM_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, incident1.WorkflowItems[0].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, incident1.WorkflowItems[1].P9_Status);
			AssertEquals("task1_test", incident1.WorkflowItems[0].P9_Description);
			AssertEquals("task2_test", incident1.WorkflowItems[1].P9_Description);
		}

		class MockReleaseBuildContent : IReleaseBuildContent
		{
			public MockReleaseBuildContent(NewWorkItem wiIsCargoWiseOneChange)
			{
				this.wiIsCargoWiseOneChange = wiIsCargoWiseOneChange;
			}

			readonly NewWorkItem wiIsCargoWiseOneChange;

			public bool IsCargoWiseOneChange(NewWorkItem workItem)
			{
				return workItem.PKEquals(wiIsCargoWiseOneChange);
			}

			public bool IsPatchedTo(NewWorkItem workItem, ReleaseBuild releaseBuild)
			{
				throw new NotImplementedException();
			}

			public bool IsPatchedTo(NewWorkItem workItem, Version currentVersion)
			{
				throw new NotImplementedException();
			}
		}

		[TestDate(2024, 9, 1)]
		public void TestOnSavingUpdatesRequest()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = contact1.Header.Contacts.AddNew();
			contact2.OC_ContactName = "jim";
			incident.IM_OC_Contact = contact1.PK;

			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.ProductArea = "DOM";
			incident.IM_Priority = "CR6";
			incident.IM_Module = "XXX";
			incident.IM_Description = "Error";
			incident.DetailNoteText = "My software is broken";
			incident.IM_ServiceType = "AAA";
			Factory.Save();
			AssertEquals("PRE", false, incident.IsWebRequest);
			var request = incident.Request;

			AssertEquals("XXX", request.INC_SubType);
			AssertEquals("Error", request.INC_Summary);
			AssertEquals("My software is broken", request.INC_Details);
			AssertEquals("ENT", request.INC_Type);
			AssertEquals("CR6", request.INC_Criticality);
			AssertEquals(contact1.PK, request.INC_OC_ReportedBy);
			AssertEquals(true, request.INC_OC_ApprovedBy.IsEmpty);
			AssertEquals("AAA", request.INC_ServiceType);
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.PendingFeatureResult, request.INC_Status);
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.PendingFeatureResult, incident.IM_RequestStatus);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", isAwaitingClient: true);
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.ClosedAwaitingResponse, request.INC_Status);
			AssertEquals(false, request.INC_IsCustomerResolved);
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.ClosedAwaitingResponse, incident.IM_RequestStatus);
			var mostRecentIWRLog = incident.GetMostRecentLogByEventCode(Events.IncidentAwaitingResponseCode).SL_EventTimeUtc;
			AssertEquals("Precondition:", new ZDateTime(2024, 9, 1), mostRecentIWRLog);
			AssertEquals("INC_ExpiryCountdownStartTimeUtc should be updated to SL_EventTimeUtc", mostRecentIWRLog, request.INC_ExpiryCountdownStartTimeUtc);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.PendingFeatureResult, request.INC_Status);
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.PendingFeatureResult, incident.IM_RequestStatus);

			TestDateAttribute.Date = new DateTime(2024, 9, 2);
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			Factory.Save();
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.Closed, request.INC_Status);
			AssertEquals(true, request.INC_IsCustomerResolved);
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.Closed, incident.IM_RequestStatus);
			AssertEquals("Precondition:", new ZDateTime(2024, 9, 2), incident.IM_ResolveTimeUtc);
			AssertEquals("INC_ExpiryCountdownStartTimeUtc should be updated to IM_ResolvedTime", incident.IM_ResolveTimeUtc, request.INC_ExpiryCountdownStartTimeUtc);

			incident.IM_Priority = "CR4";
			Factory.Save();
			AssertEquals("CR4", request.INC_Criticality);

			incident.IM_Product = ProductTypes.Codes.EHub;
			incident.IM_Module = "YYY";
			incident.IM_Description = "New Desc";
			incident.DetailNoteText = "New details";
			incident.IM_OC_Contact = contact2.PK;
			Factory.Save();

			// fields that are not updated once set
			AssertEquals("Error", request.INC_Summary);
			AssertEquals("My software is broken", request.INC_Details);

			// except reported by which gets updated
			AssertEquals(contact2.PK, request.INC_OC_ReportedBy);
			AssertEquals(ProductTypes.Codes.EHub, request.INC_Type);
			AssertEquals("YYY", request.INC_SubType);
		}

		public void TestCountryCodes()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			incident.IM_OC_Contact = contact1.PK;

			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.ProductArea = "DOM";
			incident.IM_Priority = "CR6";
			incident.IM_Module = "XXX";
			incident.IM_Description = "Error";
			incident.DetailNoteText = "My software is broken";
			Factory.Save();

			contact1.Header.OH_RL_NKClosestPort = "";
			incident.IM_RN_NKCountry = "";
			incident.Request.INC_RN_NKCountry = "";
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();
			AssertEquals("", incident.Request.INC_RN_NKCountry);

			incident.Request.INC_RN_NKCountry = "GB";
			incident.IM_RN_NKCountry = "";
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();
			AssertEquals("GB", incident.IM_RN_NKCountry);
			AssertEquals("GB", incident.Request.INC_RN_NKCountry);

			var ent = Factory.NewWithValidTestData<LicenceEnterprise>();
			var db = ent.Databases.AddNew();
			db.LD_ServerCode = "111";
			var company = ent.Companies.AddNew();
			company.LC_CompanyCode = "CO1";
			company.LC_LE = ent.PK;
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			company.LC_OH = org1.PK;
			var clientCompany1 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany1.LCC_LD = db.PK;
			clientCompany1.LCC_RN_NKCountryCode = "US";
			incident.IM_LCC = clientCompany1.PK;
			incident.IM_RN_NKCountry = "";
			incident.Request.INC_RN_NKCountry = "";
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();
			AssertEquals("US", incident.IM_RN_NKCountry);
			AssertEquals("US", incident.Request.INC_RN_NKCountry);

			incident.IM_LCC = ZGuid.Empty;
			incident.IM_OH_Client = contact1.Header.PK;
			contact1.Header.OH_RL_NKClosestPort = "CA2KS";
			incident.IM_RN_NKCountry = "";
			incident.Request.INC_RN_NKCountry = "";
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();
			AssertEquals("CA", incident.IM_RN_NKCountry);
			AssertEquals("CA", incident.Request.INC_RN_NKCountry);

			incident.IM_OH_Client = ZGuid.Empty;
			incident.IM_OC_Contact = contact1.PK;
			contact1.Header.OH_RL_NKClosestPort = "ZA2WC";
			incident.IM_RN_NKCountry = "";
			incident.Request.INC_RN_NKCountry = "";
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();
			AssertEquals("ZA", incident.IM_RN_NKCountry);
			AssertEquals("ZA", incident.Request.INC_RN_NKCountry);
		}

		public void TestOnSavingUpdatesRequest_WebRequest_IsResolved()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = contact1.Header.Contacts.AddNew();
			contact2.OC_ContactName = "jim";
			incident.IM_OC_Contact = contact1.PK;

			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.ProductArea = "DOM";
			incident.IM_Priority = "CR6";
			incident.IM_Module = "XXX";
			incident.IM_Description = "Error";
			incident.DetailNoteText = "My software is broken";
			incident.Request.INC_SystemCreateUser = User.WebUserCode;
			Factory.Save();
			AssertEquals("PRE", true, incident.IsWebRequest);
			var request = incident.Request;

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.ClosedAwaitingResponse, request.INC_Status);
			AssertEquals(false, request.INC_IsCustomerResolved);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			Factory.Save();
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.Closed, request.INC_Status);
			AssertEquals("web request must be resolved by them", false, request.INC_IsCustomerResolved);
		}

		public void TestDeleteEmptyNotes()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var x = incident.BusinessRequirementsAsBlob;
			Factory.Save();

			var query = new ZQuery(StmNoteSchema.ST_ParentID, incident.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, "IncidentMain");

			var busReqNotes = new BusinessObjectFactory().Load<StmNote>(query);
			AssertEquals(0, busReqNotes.Length);
		}

		public class SupportIncidentForTest : SupportIncident
		{
			public bool OnSavingShouldThrowAtEnd;
			public bool CloseIncidentWasCalled;
			public bool HasNewSubscribers;
			public bool HasUnsubscribeParticipants;

			public SupportIncidentForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();
				var x = BusinessRequirementsAsBlob;
				x = FeatureRequestInternalNoteAsBlob;
				x = FeatureRequestPrerequisitesAsBlob;
				x = SoftwareChangeAsBlob;
				x = TechnicalSpecificationAsBlob;

				if (OnSavingShouldThrowAtEnd)
				{
					throw new ZSaveException(new ZDataException(new Exception(), ((INeedRow)this).Row, Db.Connection), Factory);
				}
				CloseIncidentWasCalled = false;
			}

			protected override void OnSaveSucceeded()
			{
				if (EConversation.Conversation.UnsubscribedParticipants.Count > 0)
				{
					HasUnsubscribeParticipants = true;
				}

				base.OnSaveSucceeded();
			}

			protected override bool CanShowCloseIncidentForm => true;

			public void ResetIsEnterprisePKInitialised()
			{
				isEnterprisePKInitialised = false;
			}

			internal override void InvokeOnCloseIncidentEvent()
			{
				CloseIncidentWasCalled = true;
			}
		}

		public void TestAccessBusinessRequirementsNoteDuringSaving()
		{
			var incident = Factory.NewWithValidTestData<SupportIncidentForTest>();
			Factory.Save();

			var query = new ZQuery(StmNoteSchema.ST_ParentID, incident.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, "IncidentMain");

			var busReqNotes = new BusinessObjectFactory().Load<StmNote>(query);
			AssertEquals(0, busReqNotes.Length);
		}

		#region Custom Fields

		[TestedType(typeof(SupportIncident))]
		class SupportIncidentCustomFieldsTest : TestICustomFieldProvider
		{
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_Product = "ENT";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Source = "";
			incident.ProductArea = "DOM";
			incident.IM_Language = Core.SharedConstants.Languages.ChineseSimplified;
			IWorkflowProviderCore provider = incident;
			var criteria = (ColumnValueRanker)provider.GetTemplateSelectionCriteria();
			AssertEquals("P0_OH_Client is IM_OH_Client", org.PK, criteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client)[0]);
			AssertEquals("P0_SubType1 is IM_Product", "ENT", criteria.GetValues(ProcessTaskTemplateSchema.P0_SubType1)[0].ToString());
			AssertEquals("P0_SubType2 is IM_Category", SupportIncidentCategoriesList.Codes.FeatureRequest, criteria.GetValues(ProcessTaskTemplateSchema.P0_SubType2)[0].ToString());
			AssertEquals("P0_SubType3 is IM_Source", "", criteria.GetValues(ProcessTaskTemplateSchema.P0_SubType3)[0].ToString());
			AssertEquals("P0_SubType4 is ProductArea", "DOM", criteria.GetValues(ProcessTaskTemplateSchema.P0_SubType4)[0].ToString());
			AssertEquals("P0_SubType5 is IM_Language", Core.SharedConstants.Languages.ChineseSimplified, criteria.GetValues(ProcessTaskTemplateSchema.P0_SubType5)[0].ToString());
		}

		#endregion

		public void TestPopulateEstimateAndQuoteCurrencies()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			ClientInvoiceDelivery delivery = org.LicCompany.InvoiceDeliveries.AddNew();
			delivery.L9_IsBilled = true;
			delivery.L9_RX_NKInvoiceCurrency = "IDR";

			Factory.Save();

			var incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			AssertEquals("Should not populate", string.Empty, incident.Estimate.CIE_RX_NKCurrency);
			AssertEquals("Should not populate", string.Empty, incident.Quote.CIQ_RX_NKCurrency);

			incident.FeatureRequestClientPK = org.PK;
			AssertEquals("Should populate", "IDR", incident.Estimate.CIE_RX_NKCurrency);
			AssertEquals("Should populate", "IDR", incident.Quote.CIQ_RX_NKCurrency);

			var diffOrg = Factory.NewWithValidTestData<OrgHeader>();
			incident.FeatureRequestClientPK = diffOrg.PK;
			AssertEquals("Should not change", "IDR", incident.Estimate.CIE_RX_NKCurrency);
			AssertEquals("Should not change", "IDR", incident.Quote.CIQ_RX_NKCurrency);
		}

		public void TestPopulateInvalidCurrencies()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			ClientInvoiceDelivery delivery = org.LicCompany.InvoiceDeliveries.AddNew();
			delivery.L9_IsBilled = true;
			delivery.L9_RX_NKInvoiceCurrency = "EUR";

			ClientInvoiceDelivery delivery2 = org.LicCompany.InvoiceDeliveries.AddNew();
			delivery2.L9_IsBilled = true;
			delivery2.L9_RX_NKInvoiceCurrency = "USD";

			Factory.Save();

			var incident = Factory.New<SupportIncident>();

			AssertNoExceptionThrown(() => incident.FeatureRequestClientPK = org.PK);
			AssertEquals("Should populate", "EUR", incident.Estimate.CIE_RX_NKCurrency);
			AssertEquals("Should populate", "EUR", incident.Quote.CIQ_RX_NKCurrency);
		}

		public void TestFeatureRequestClientPK_HasChanges()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			AssertEquals("Precondition: Saved Incident Has No Changes", false, incident.HasChanges);
			incident.FeatureRequestClientPK = org.PK;
			AssertEquals("Incident with different FeatureRequestClientPK Has Changes", true, incident.HasChanges);
			Factory.Save();
			AssertEquals("Saved Incident Has No Changes", false, incident.HasChanges);
		}

		public void TestFeatureRequestClientPK_PopulateFromIncidentClient()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			AssertEquals("Should not populate - do not create a unnecessary pivot", ZGuid.Empty, incident.FeatureRequestClientPK);

			var featureRequest = Factory.New<SupportIncident>();
			featureRequest.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			featureRequest.IM_OH_Client = org.PK;
			AssertEquals("Should populate because feature request", org.PK, featureRequest.FeatureRequestClientPK);

			var diffOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			featureRequest.IM_OH_Client = diffOrg1.PK;
			AssertEquals("Should not change - only populate the first time", org.PK, featureRequest.FeatureRequestClientPK);

			var diffOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			featureRequest.FeatureRequestClientPK = diffOrg2.PK;
			AssertEquals("Should not change customer service client", diffOrg1.PK, featureRequest.IM_OH_Client);
		}

		public void TestFeatureRequestClientAddressPK_HasChanges()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			AssertEquals("Precondition: Saved Incident Has No Changes", false, incident.HasChanges);
			incident.FeatureRequestClientAddressPK = org.PK;
			AssertEquals("Incident with different FeatureRequestClientAddressPK Has Changes", true, incident.HasChanges);
			Factory.Save();
			AssertEquals("Saved Incident Has No Changes", false, incident.HasChanges);
		}

		public void TestFeatureRequestClientAddressPK_PopulateFromIncidentAddress_SetCategoryLast()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = address.PK;
			AssertEquals("Should not populate - do not create a unnecessary pivot", ZGuid.Empty, incident.FeatureRequestClientAddressPK);

			var featureRequest = Factory.New<SupportIncident>();
			featureRequest.IM_OA_BranchAddress = address.PK;
			featureRequest.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			AssertEquals("Should populate because feature request", address.PK, featureRequest.FeatureRequestClientAddressPK);

			var diffAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			featureRequest.IM_OA_BranchAddress = diffAddress1.PK;
			AssertEquals("Should not change - only populate the first time", address.PK, featureRequest.FeatureRequestClientAddressPK);

			var diffAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			featureRequest.FeatureRequestClientAddressPK = diffAddress2.PK;
			AssertEquals("Should not change customer service client", diffAddress1.PK, featureRequest.IM_OA_BranchAddress);
		}

		public void TestFeatureRequestClientAddressPK_PopulateFromIncidentAddress()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = address.PK;
			AssertEquals("Should not populate - do not create a unnecessary pivot", ZGuid.Empty, incident.FeatureRequestClientAddressPK);

			var featureRequest = Factory.New<SupportIncident>();
			featureRequest.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			featureRequest.IM_OA_BranchAddress = address.PK;
			AssertEquals("Should populate because feature request", address.PK, featureRequest.FeatureRequestClientAddressPK);

			var diffAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			featureRequest.IM_OA_BranchAddress = diffAddress1.PK;
			AssertEquals("Should not change - only populate the first time", address.PK, featureRequest.FeatureRequestClientAddressPK);

			var diffAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			featureRequest.FeatureRequestClientAddressPK = diffAddress2.PK;
			AssertEquals("Should not change customer service client", diffAddress1.PK, featureRequest.IM_OA_BranchAddress);
		}

		public void TestFeatureRequestClientAddressPK_PopulateFromIncidentAddress_WhenPriorityChanges()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.WorkingAddressPK = address.PK;

			var featureRequest = Factory.New<SupportIncident>();
			featureRequest.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			featureRequest.IM_OC_Contact = contact.PK;
			AssertEquals("Address should not populate", ZGuid.Empty, featureRequest.FeatureRequestClientAddressPK);
			AssertEquals("Contact should not populate", ZGuid.Empty, featureRequest.FeatureRequestContactPK);

			featureRequest.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			AssertEquals("Should populate address because feature request", address.PK, featureRequest.FeatureRequestClientAddressPK);
			AssertEquals("Should populate contact because feature request", contact.PK, featureRequest.FeatureRequestContactPK);
		}

		public void TestFeatureRequestContactPK_HasChanges()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			AssertEquals("Precondition: Saved Incident Has No Changes", false, incident.HasChanges);
			incident.FeatureRequestContactPK = org.PK;
			AssertEquals("Incident with different FeatureRequestContactPK Has Changes", true, incident.HasChanges);
			Factory.Save();
			AssertEquals("Saved Incident Has No Changes", false, incident.HasChanges);
		}

		public void TestFeatureRequestContactPK_PopulateFromIncidentContact()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var incident = Factory.New<SupportIncident>();
			incident.IM_OC_Contact = contact.PK;
			AssertEquals("Should not populate - do not create a unnecessary pivot", ZGuid.Empty, incident.FeatureRequestContactPK);

			var featureRequest = Factory.New<SupportIncident>();
			featureRequest.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			featureRequest.IM_OC_Contact = contact.PK;
			AssertEquals("Should populate because feature request", contact.PK, featureRequest.FeatureRequestContactPK);

			var diffContact1 = Factory.NewWithValidTestData<OrgContact>();
			featureRequest.IM_OC_Contact = diffContact1.PK;
			AssertEquals("Should not change - only populate the first time", contact.PK, featureRequest.FeatureRequestContactPK);

			var diffContact2 = Factory.NewWithValidTestData<OrgContact>();
			featureRequest.FeatureRequestContactPK = diffContact2.PK;
			AssertEquals("Should not change customer service client", diffContact1.PK, featureRequest.IM_OC_Contact);
		}

		public void TestFeatureRequestContactPK_BlankOnClientChange()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact2 = org2.Contacts.AddNew();

			var featureRequest = Factory.New<SupportIncident>();
			featureRequest.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			featureRequest.FeatureRequestClientPK = org.PK;
			featureRequest.FeatureRequestContactPK = contact.PK;
			AssertEquals("Precondition", contact.PK, featureRequest.FeatureRequestContactPK);

			featureRequest.FeatureRequestClientPK = org2.PK;
			AssertEquals("Should be blank", ZGuid.Empty, featureRequest.FeatureRequestContactPK);

			featureRequest.FeatureRequestContactPK = contact2.PK;
			AssertEquals("Precondition", contact2.PK, featureRequest.FeatureRequestContactPK);

			featureRequest.FeatureRequestClientPK = ZGuid.Empty;
			AssertEquals("Should be blank", ZGuid.Empty, featureRequest.FeatureRequestContactPK);
		}

		public void TestDbFeatureRequestContactPK()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org1.Contacts.AddNew();
			contact.OC_ContactName = ZGuid.NewZGuid().ToString();

			var incident = Factory.New<SupportIncident>();
			incident.IM_OC_Contact = contact.PK;

			var featureRequest = Factory.New<SupportIncident>();
			featureRequest.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			featureRequest.IM_OC_Contact = contact.PK;

			Factory.Save();

			featureRequest = new BusinessObjectFactory().Load<SupportIncident>(featureRequest.PK);

			var org2 = featureRequest.Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = ZGuid.NewZGuid().ToString().Substring(0, OrgHeaderSchema.OH_Code.MaxLength);

			var diffContact = org2.Contacts.AddNew();
			diffContact.OC_ContactName = ZGuid.NewZGuid().ToString();

			featureRequest.IM_OC_Contact = diffContact.PK;
			featureRequest.FeatureRequestContactPK = diffContact.PK;
			AssertEquals("Should not change - only populate the first time", contact.PK, featureRequest.DbFeatureRequestContactPK);

			featureRequest.Factory.Save();

			AssertEquals("Should change after save", diffContact.PK, featureRequest.DbFeatureRequestContactPK);
		}

		public void TestFetchForLoad_SupportIncident()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var contact = client.Contacts.AddNew();
			contact.OC_ContactName = "IT Support";
			contact.OC_Email = "itsupport@test.com";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			Factory.Save();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertNotNull(newFactory.Load<SupportIncident>(incident.PK));
			Assert("Fetch hints should be used", 1 <= newFactory.ActiveTableFetchHints);
			AssertEquals("CustomValue hints were added (on OrgHeader)", 1, newFactory.ActiveFetchHintsForTable(GenCustomAddOnValueSchema.Constants.TableName));
		}

		public void TestShouldNotHaveChangesUponReload()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("DOM", "Domestic Logistics");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("XXX", "XXX Menu Section", "DOM", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.ProductArea = "DOM";
			incident.IM_Priority = "CR6";
			incident.IM_Module = "XXX";
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var incidentReloaded = anotherFactory.Load<SupportIncident>(incident.PK);
			AssertEquals(false, incidentReloaded.HasChanges);
		}

		public void TestClientSupportsCr8Cr9()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "ENT";
			enterprise.LE_OH = org.PK;

			var company = Factory.New<LicenceCompany>();
			company.LC_CompanyCode = "COM";
			company.LC_LE = enterprise.PK;
			company.LC_OH = org.PK;

			var releaseBuild = Factory.New<ReleaseBuild>();

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = releaseBuild.PK;

			var licence = Factory.New<LicenceHeader>();
			licence.LA_LC = company.PK;
			licence.LA_LD = database.PK;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = company.LC_CompanyCode;
			clientCompany.LCC_LD = database.PK;

			Factory.Save();

			EDIDataRegistry.Instance.Cr8Cr9ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "1.1.2.2");

			var incident = Factory.New<SupportIncident>();
			AssertEquals(true, incident.ClientSupportsCr8Cr9);

			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			Factory.Save();

			incident.IM_Product = ProductTypes.Codes.Enterprise;
			releaseBuild.VersionNumber = new VersionNumber("1.1.2.0");
			AssertEquals(false, incident.ClientSupportsCr8Cr9);

			releaseBuild.VersionNumber = new VersionNumber("1.1.2.2");
			AssertEquals(true, incident.ClientSupportsCr8Cr9);

			incident.IM_Product = "XXX";
			releaseBuild.VersionNumber = new VersionNumber("1.1.2.0");
			AssertEquals(true, incident.ClientSupportsCr8Cr9);
		}

		#region ISendEmailSource Members

		public void TestImplementsISendEmailSource()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			AssertEquals("SupportIncident should implement ISendEmailSource", true, incident is ISendEmailSource);
		}

		public void TestGetAddressBookSelection()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = client.Contacts.AddNew();
			contact.OC_ContactName = "contact";
			incident.IM_OH_Client = client.PK;
			GlbStaff addedBy = Factory.NewWithValidTestData<GlbStaff>();
			addedBy.GS_FullName = "addedByStaff";
			incident.IM_SystemCreateUser = addedBy.GS_Code;
			GlbStaff customerServiceContact = Factory.NewWithValidTestData<GlbStaff>();
			customerServiceContact.GS_FullName = "customerServiceContact";
			incident.IM_GS_NKCustServiceContact = customerServiceContact.GS_Code;
			GlbStaff currentlyAssignedTo = Factory.NewWithValidTestData<GlbStaff>();
			currentlyAssignedTo.GS_FullName = "currentlyAssignedTo";
			incident.IM_GS_NKAssignedToCurrent = currentlyAssignedTo.GS_Code;
			Factory.Save();

			AddressBookSelection result = ((ISendEmailSource)incident).GetAddressBookSelection();
			AssertEquals("Should return 4 contacts", 4, result.Recipients.Count);
			AssertEquals(contact.OC_ContactName, result.Recipients[0].Name);
			AssertEquals(addedBy.GS_FullName, result.Recipients[1].Name);
			AssertEquals(customerServiceContact.GS_FullName, result.Recipients[2].Name);
			AssertEquals(currentlyAssignedTo.GS_FullName, result.Recipients[3].Name);
		}

		public void TestEmailSubject()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_IncidentNumber = "22";
			string result = ((ISendEmailSource)incident).EmailSubject;
			AssertEquals("Incorrect email subject", "Customer Service Incident: " + ((IWorkItemRelatedItem)incident).Number, result);
		}

		#endregion

		#region Related Work Items

		public void TestRelatedWorkItems()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			AssertEquals(false, incident.RelatedWorkItems.ReadOnly);
		}

		#region IWorkItemRelatedItem Members

		public void TestIWorkItemRelatedItemImplementation()
		{
			ReleaseBuildContentForLegacyTest.Enable();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_ReleaseRing = ReleaseRings.Codes.ALP;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			var incident1 = Factory.New<SupportIncident>();
			incident1.IM_LD = database.PK;
			incident1.IM_LCC = clientCompany.PK;
			incident1.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			RunWorkItemRelatedEventTest(incident1);

			var incident2 = Factory.New<SupportIncident>();
			incident2.IM_LD = database.PK;
			incident2.IM_LCC = clientCompany.PK;
			incident2.IM_Source = SupportIncidentLookups.SourceListConstants.CreatedFromProject;
			incident2.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			Assert(incident2.IsProjectRelatedIncident);
			RunWorkItemRelatedEventTest(incident2);
		}

		void RunWorkItemRelatedEventTest(SupportIncident incident)
		{
			incident.IM_IncidentNumber = "Z123";
			incident.IM_Description = "Test work ok";
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;

			IWorkItemRelatedItem item = incident;
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = SupportIncidentLookups.Status.Working;

			AssertEquals("Defect", item.Type);
			AssertEquals("Z123", item.Number);
			AssertEquals("Test work ok", item.ItemDescription);
			AssertEquals("Working", item.StatusDescription);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, item.AssignedStaffCode);
			AssertEquals(ClientControllerRegistration.SupportIncident, item.ControllerID);
			AssertEquals(incident.IM_Priority, item.Criticality);
			AssertEquals(incident.IM_PriorityInfo, item.CriticalityInfo);

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Status = SupportIncidentLookups.Status.Working;
			AssertEquals("Feature Request", item.Type);

			NewWorkItem workItem1 = incident.RelatedWorkItems.AddNew();
			ProcessTask workItem1Task = workItem1.WorkflowItems.AddNew();
			NewWorkItem workItem2 = incident.RelatedWorkItems.AddNew();
			ProcessTask workItem2Task = workItem2.WorkflowItems.AddNew();
			workItem2Task.P9_Type = ReleaseRingsLookup.CheckInTaskTypes.First();

			SupportIncident relatedFeatureRequest = Factory.NewWithValidTestData<SupportIncident>();
			relatedFeatureRequest.SetupForNewCreatedFeatureRequest();
			incident.RelatedFeatureRequests.Add(relatedFeatureRequest);
			relatedFeatureRequest.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			relatedFeatureRequest.IM_Status = SupportIncidentLookups.Status.Open;

			item.OnRelatedWorkItemClosed(workItem1);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("Related feature request shouldn't be affected", SupportIncidentLookups.Status.Open, relatedFeatureRequest.IM_Status);

			workItem1Task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			item.OnRelatedWorkItemClosed(workItem1);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("Related feature request shouldn't be affected", SupportIncidentLookups.Status.Open, relatedFeatureRequest.IM_Status);

			workItem2Task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			item.OnRelatedWorkItemClosed(workItem2);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);
			AssertEquals("Related feature request shouldn't be affected", SupportIncidentLookups.Status.Open, relatedFeatureRequest.IM_Status);

			item.OnRelatedWorkItemReOpened(workItem1);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("Related feature request shouldn't be affected", SupportIncidentLookups.Status.Open, relatedFeatureRequest.IM_Status);

			workItem2Task.P9_Type = "UDF";
			workItem2Task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.IM_Status = SupportIncidentLookups.Status.Closed;

			NewWorkItem workItem3 = Factory.New<NewWorkItem>();
			ProcessTask workItem3Task = workItem3.WorkflowItems.AddNew();
			workItem3Task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			item.OnWorkItemAdded(workItem3);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Related feature request shouldn't be affected", SupportIncidentLookups.Status.Open, relatedFeatureRequest.IM_Status);

			workItem3Task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			item.OnWorkItemAdded(workItem3);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Related feature request shouldn't be affected", SupportIncidentLookups.Status.Open, relatedFeatureRequest.IM_Status);

			item.OnWorkItemRemoved(workItem3);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Related feature request shouldn't be affected", SupportIncidentLookups.Status.Open, relatedFeatureRequest.IM_Status);

			incident.RelatedWorkItems.RemoveAll();
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("Related feature request shouldn't be affected", SupportIncidentLookups.Status.Open, relatedFeatureRequest.IM_Status);
		}

		public void TestOnRelatedWorkItemClosed_ClosedAndCancelledWorkItems()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Status = SupportIncidentLookups.Status.Working;

			var workItem1 = incident.RelatedWorkItems.AddNew();
			var workItem2 = incident.RelatedWorkItems.AddNew();
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);

			var item = (IWorkItemRelatedItem)incident;

			workItem1.WKI_Status = "CLS";
			item.OnRelatedWorkItemClosed(workItem1);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);

			workItem2.WKI_Status = "CAN";
			item.OnRelatedWorkItemClosed(workItem2);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);
		}

		public void TestOnWorkItemRemovedShouldNotCloseIncidentIfOpenTasksExistOnIncident()
		{
			//Setup an incident with 2 work items as related items and an open/assigned task
			ReleaseBuildContentForLegacyTest.Enable();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_ReleaseRing = ReleaseRings.Codes.ALP;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			var incident = Factory.New<SupportIncident>();
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			var openIncidentTask = incident.WorkflowItems.AddNew();
			openIncidentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();

			var cancelledWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			var cancelledWorkItemTask = cancelledWorkItem.WorkflowItems.AddNew();
			cancelledWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			incident.RelatedItems.Add(cancelledWorkItem);

			var openWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			var openWorkItemTask = openWorkItem.WorkflowItems.AddNew();
			openWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			incident.RelatedItems.Add(openWorkItem);

			Factory.Save();

			AssertEquals("Precondition: Should have 1 open task", 1, incident.WorkflowItems.Count);
			AssertEquals("Precondition: Should have 1 open task", ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[0].P9_Status);
			AssertEquals("Precondition: Should have 2 related items", 2, incident.RelatedItems.Count);

			AssertEquals("Precondition: Should be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, cancelledWorkItem.WKI_Status);
			AssertEquals("Precondition: Should be open", ProcessTaskStatusCodeList.Codes.Open, openWorkItem.WKI_Status);

			incident.RelatedItems.Remove(openWorkItem);

			Assert("Incident should not add cancellation message", !incident.EConversation.AnyLocalMessageContains("All related work items are cancelled."));
			AssertEquals("Task should still be open", ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[0].P9_Status);
			AssertNotEquals("Incident should still be open", SupportIncidentLookups.Status.Closed, incident.IM_Status);
		}

		public void TestNoAutoUpgradeForWorkItemWithCancelledCheckin()
		{
			ReleaseBuildContentForLegacyTest.Enable();

			var incident = Factory.New<SupportIncident>();
			incident.IM_IncidentNumber = "Z123";
			incident.IM_Description = "Test work ok";
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			var item = (IWorkItemRelatedItem)incident;
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			incident.IM_Status = SupportIncidentLookups.Status.Working;

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.IM_Status = SupportIncidentLookups.Status.Working;

			var workItem1 = incident.RelatedWorkItems.AddNew();
			workItem1.WKI_Priority = ReleaseRings.Codes.STD;
			var task1 = workItem1.WorkflowItems.AddNew();
			task1.P9_Type = EDITaskTypes_ForTest.TaskCodingOfFunctionality;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var task2 = workItem1.WorkflowItems.AddNew();
			task2.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.STD).CheckinTask;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			item.OnRelatedWorkItemClosed(workItem1);

			Factory.Save();

			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);

			var message1 = incident.EConversation.GetTimeOrderedMessages().First(x => x.Body == "Work completed, no upgrade will be sent") as JobConversationMessage;
			AssertEquals("Should be a system message", true, message1.JCM_IsSystem);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			var incTask = incident.WorkflowItems.AddNew();
			incTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			incTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var task3 = workItem1.WorkflowItems.AddNew();
			task3.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GP1).CheckinTask;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			var message2 = incident.EConversation.LastAddedMessageForTest;

			item.OnRelatedWorkItemClosed(workItem1);
			Factory.Save();

			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);
		}

		public void TestType()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			IWorkItemRelatedItem relatedItem = incident;

			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			AssertEquals("If incident is in \"Support\" stage the type of related item should be Support Incident", EDIWorkTaskRelatedItemTypes.SupportIncident, relatedItem.Type);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			AssertEquals("If incident is in \"Defect\" stage the type should be \"Defect\"", EDIWorkTaskRelatedItemTypes.Defect, relatedItem.Type);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			AssertEquals("If incident is in \"Feature Request\" stage the type should be \"Feature Request\"", EDIWorkTaskRelatedItemTypes.FeatureRequest, relatedItem.Type);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.ComplianceRequirement;
			AssertEquals("If incident is in \"Compliance Requirement\" stage the type should be \"Compliance Requirement\"", EDIWorkTaskRelatedItemTypes.ComplianceRequirement, relatedItem.Type);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
			AssertEquals("If incident is in \"Service Request\" stage the type should be \"Service Request\"", EDIWorkTaskRelatedItemTypes.CustomerServiceRequest, relatedItem.Type);
		}

		#endregion

		public void TestRelatedWorkItemsShouldNotIncludeRelatedItems()
		{
			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			SupportIncident supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			project.RelatedItems.Add(supportIncident);
			NewWorkItem workItem = supportIncident.RelatedWorkItems.AddNew();
			workItem.FillWithValidTestData();
			AssertEquals(1, supportIncident.RelatedWorkItems.Count);
			AssertEquals(workItem, supportIncident.RelatedWorkItems[0]);
			Factory.Save();

			SupportIncident incidentInNewFactory = new BusinessObjectFactory().Load<SupportIncident>(supportIncident.PK);
			AssertEquals(1, incidentInNewFactory.RelatedWorkItems.Count);
			AssertEquals(workItem.PK, incidentInNewFactory.RelatedWorkItems[0].PK);
		}

		public void TestSupportedRelatedItemModules_WorkItem()
		{
			SupportIncident supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			var defectWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			var otherWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			supportIncident.DefectCausedByWorkItemPK = defectWorkItem.PK;
			Factory.Save();

			var info = supportIncident.SupportedRelatedItemModules.First(x => x.Type == ProcessManagement.Business.WorkTaskRelatedItemTypes.WorkItem);
			AssertEquals(false, defectWorkItem.MatchesFilter(info.AdditionalFilterForFindBox));
			AssertEquals(true, otherWorkItem.MatchesFilter(info.AdditionalFilterForFindBox));
		}

		public void TestCalculateWorkItemDependentStatusAndDisposition()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			var task = incident.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			incident.RelatedItems.Add(workItem);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);

			var loadedTask = new BusinessObjectFactory().Load<SupportIncidentProcessTask>(task.PK);
			loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			loadedTask.Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);

			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);

			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate;
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate, incident.IM_ResolutionCode);

			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.AwaitingFormalQuotation;
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.AwaitingFormalQuotation, incident.IM_ResolutionCode);

			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			incident.RelatedItems.Remove(workItem);
			Factory.Save();

			var checkInTask = workItem.WorkflowItems.AddNew();
			task.P9_Type = "CH0";
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.RelatedItems.Add(workItem);

			loadedTask = new BusinessObjectFactory().Load<SupportIncidentProcessTask>(task.PK);
			loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			loadedTask.Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Suspended, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Suspended.Deferred, incident.IM_ResolutionCode);

			Factory.Save();
			AssertEquals("Should be no change", SupportIncidentLookups.Status.Suspended, incident.IM_Status);
			AssertEquals("Should be no change", SupportIncidentLookups.DispositionList.Constants.Suspended.Deferred, incident.IM_ResolutionCode);
		}

		public void TestCalculateWorkItemDependentStatusAndDisposition_WaitingUpgrade_UpgradeDelivered()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			var licDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licDatabase.LD_ReleaseRing = "ALP";
			incident.IM_LD = licDatabase.PK;
			Factory.Save();

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			incident.RelatedItems.Add(workItem);

			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = "UDF";
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var checkInTask = workItem.WorkflowItems.AddNew();
			checkInTask.P9_Type = "CH0";
			checkInTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			checkInTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			Factory.Save();
			AssertEquals("Should be no change", SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("Should be no change", SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);

			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered;
			Factory.Save();
			AssertEquals("Should be no change", SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("Should be no change", SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, incident.IM_ResolutionCode);

			checkInTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			Factory.Save();
			AssertEquals("Should be changed back", SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);
		}

		public void TestCalculateWorkItemDependentStatusAndDisposition_IncorrectDispositionWithWorkItemLinked()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			var task = incident.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			incident.RelatedItems.Add(workItem);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);

			var loadedTask = new BusinessObjectFactory().Load<SupportIncidentProcessTask>(task.PK);
			loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			loadedTask.Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);

			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var newTask = incident.WorkflowItems.AddNew();
			newTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			Factory.Save();

			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.Assigned, incident.IM_ResolutionCode);

			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			task.Reload();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			newTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);

			incident.IM_Description = "Test";
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);
		}

		public void TestWorkflowItems_CountChanged_PreviousDispositionClosed_ShouldNotRecalculateDispositionIfCurrentTaskManuallyCreated()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			var task = incident.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			var loadedTask = new BusinessObjectFactory().Load<SupportIncidentProcessTask>(task.PK);
			loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			loadedTask.Factory.Save();
			AssertEquals("Precondition", IncidentMainLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Precondition", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals("Precondition", SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);

			var newTask = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			incident.WorkflowItems.Add(newTask);

			AssertEquals("Status should be reopened", IncidentMainLookups.Status.Working, incident.IM_Status);
			AssertEquals("Disposition should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals("Disposition should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);
			AssertEquals("Should not have sent any econversation messages", false, incident.EConversationHasChanges);

			Factory.Save();

			AssertEquals("Status should be reopened", IncidentMainLookups.Status.Working, incident.IM_Status);
			AssertEquals("Disposition should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals("Disposition should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);
			AssertEquals("Should not have sent any econversation messages", false, incident.EConversationHasChanges);

			var newTask2 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			incident.WorkflowItems.Add(newTask2);

			AssertEquals("Status should be unchanged", IncidentMainLookups.Status.Working, incident.IM_Status);
			AssertEquals("Disposition should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals("Disposition should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);
			AssertEquals("Should not have sent any econversation messages", false, incident.EConversationHasChanges);
		}

		public void TestWorkflowItems_CountChanged_PreviousDispositionClosed_ShouldNotRecalculateDispositionIfCurrentTaskCreatedByTemplate()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			var task = incident.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			var loadedTask = new BusinessObjectFactory().Load<SupportIncidentProcessTask>(task.PK);
			loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			loadedTask.Factory.Save();
			AssertEquals("Precondition", IncidentMainLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Precondition", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals("Precondition", SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);

			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = incident.WorkflowItems.WorkflowType;

			var templateTask = workflowTemplate.WorkflowItems.Tasks.AddNew();
			templateTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			templateTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			workflowTemplate.Factory.Save();

			incident.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates(new[] { workflowTemplate }));

			var workflowTask = incident.WorkflowItems.Tasks.Cast<ProcessTask>().FirstOrDefault(x => x.SourceTemplatePK == workflowTemplate.PK);

			AssertNotNull("Precondition: Should have created a new task from the template", workflowTask);
			AssertEquals("Status should be reopened", IncidentMainLookups.Status.Working, incident.IM_Status);
			AssertEquals("Disposition should not be updated", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);

			Factory.Save();

			AssertEquals("Status should be reopened", IncidentMainLookups.Status.Working, incident.IM_Status);
			AssertEquals("Disposition should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
		}

		public void TestWorkflowItems_CountChanged_PreviousDispositionResolved_ShouldNotRecalculateDispositionIfCurrentTaskManuallyCreated()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			var task = incident.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			var loadedTask = new BusinessObjectFactory().Load<SupportIncidentProcessTask>(task.PK);
			loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			loadedTask.Factory.Save();
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;
			AssertEquals("Precondition", IncidentMainLookups.Status.Closed, incident.IM_Status);

			var newTask = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			newTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			newTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			incident.WorkflowItems.Add(newTask);

			AssertEquals("Status should be reopened", IncidentMainLookups.Status.Working, incident.IM_Status);
			AssertEquals("Disposition should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, incident.IM_ResolutionCode);
			AssertEquals("Should not have sent any econversation messages", false, incident.EConversationHasChanges);

			Factory.Save();

			AssertEquals("Status should be reopened", IncidentMainLookups.Status.Working, incident.IM_Status);
			AssertEquals("Disposition should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, incident.IM_ResolutionCode);
			AssertEquals("Should not have sent any econversation messages", false, incident.EConversationHasChanges);
		}

		public void TestWorkflowItems_CountChanged_PreviousDispositionClosed_ShouldNotRecalculateDispositionIfNoCurrentTaskAndOpenTaskIsManuallyCreated()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			var task = incident.WorkflowItems.AddNew();

			Factory.Save();

			var loadedTask = new BusinessObjectFactory().Load<SupportIncidentProcessTask>(task.PK);
			loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			loadedTask.Factory.Save();
			AssertEquals("Precondition", IncidentMainLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Precondition", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals("Precondition", SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);

			incident.WorkflowItems.AddNew();

			AssertEquals("Status should be reopened", IncidentMainLookups.Status.Open, incident.IM_Status);
			AssertEquals("Disposition should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals("Disposition should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);
			AssertEquals("Should not have sent any econversation messages", false, incident.EConversationHasChanges);

			Factory.Save();

			AssertEquals("Status should be reopened", IncidentMainLookups.Status.Open, incident.IM_Status);
			AssertEquals("Disposition should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals("Disposition should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);
			AssertEquals("Should not have sent any econversation messages", false, incident.EConversationHasChanges);
		}

		public void TestWorkflowItems_TwoTasks_FirstCANAndSecondOPN_ShouldRecalculateDisposition()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;

			var cancelledTask = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			cancelledTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			cancelledTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			incident.WorkflowItems.Add(cancelledTask);
			cancelledTask.P9_ParentID = incident.PK;
			cancelledTask.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;

			AssertEquals("Precondition", IncidentMainLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Precondition", SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, incident.IM_ResolutionCode);

			var openTask = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			openTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			openTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			incident.WorkflowItems.Add(openTask);
			openTask.P9_ParentID = incident.PK;
			openTask.P9_ParentTableCode = IncidentMainSchema.Constants.Prefix;

			AssertEquals("IM_Status should be open", IncidentMainLookups.Status.Open, incident.IM_Status);
			AssertEquals("IM_ResolutionCode should be AUC", SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction, incident.IM_ResolutionCode);
		}

		public void TestIsClosedDisposition()
		{
			var closedDispositions = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = closedDispositions.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			closedDispositions.Add("YYY", (NoResString)"Why", supportParent, false);
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, closedDispositions))
			{
				AssertEquals(false, incident.IsClosedDisposition(string.Empty));
				AssertEquals(false, incident.IsClosedDisposition("ZZZ"));
				AssertEquals(true, incident.IsClosedDisposition("YYY"));
				AssertEquals(false, incident.IsClosedDisposition(SupportIncidentLookups.DispositionList.Constants.Closed.Completed));
				AssertEquals(true, incident.IsClosedDisposition(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved));
				AssertEquals(true, incident.IsClosedDisposition(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed));
				AssertEquals("Categories shouldn't be included as dispositions", false, incident.IsClosedDisposition(SupportIncidentCategoriesList.Codes.Support));
				AssertEquals("Special case, because DEF is used as both a non-closed disposition and a Stage category", false, incident.IsClosedDisposition(SupportIncidentLookups.DispositionList.Constants.Suspended.Deferred));
			}
		}

		public void TestRelatedWorkItemsAttachDetachEvent()
		{
			var newWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			newWorkItem.WKI_WorkItemNumber = "WI00000001";

			var supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			supportIncident.IM_IncidentNumber = "CS00000002";

			supportIncident.RelatedItems.Add(newWorkItem);
			Factory.Save();

			var attachedEvent = newWorkItem.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.Event.SE_Code == Events.Attached.Code);
			AssertEquals("Should have an attached event", true, attachedEvent != null);
			AssertEquals("WI00000001 attached to CS00000002", attachedEvent.SL_Reference);

			supportIncident.RelatedItems.Remove(newWorkItem);
			Factory.Save();

			var detachedEvent = newWorkItem.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.Event.SE_Code == Events.Detached.Code);
			AssertEquals("Should have a detached event", true, detachedEvent != null);
			AssertEquals("WI00000001 detached from CS00000002", detachedEvent.SL_Reference);
		}

		public void TestWorkItemNumber()
		{
			var newWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			newWorkItem.WKI_WorkItemNumber = "WI00000001";

			SupportIncident supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			supportIncident.IM_IncidentNumber = "CS00000001";

			supportIncident.RelatedItems.Add(newWorkItem);

			AssertEquals("WI00000001", supportIncident.WorkItemNumber);
		}

		#endregion

		#region Related Feature Requests

		public void TestParentFeatureRequest()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.SetupForNewCreatedFeatureRequest();

			SupportIncident incident2 = Factory.New<SupportIncident>();
			incident2.SetupForNewCreatedFeatureRequest();

			SupportIncident incident3 = Factory.New<SupportIncident>();
			incident3.SetupForNewCreatedFeatureRequest();

			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident2.IM_Category);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident3.IM_Category);

			incident.RelatedFeatureRequests.Add(incident2);
			incident.RelatedFeatureRequests.Add(incident3);
			AssertEquals(incident, incident2.ParentFeatureRequest);
			AssertEquals(incident.PK, incident2.ParentFeatureRequestPK);
			AssertEquals(incident, incident3.ParentFeatureRequest);
			AssertEquals(incident.PK, incident3.ParentFeatureRequestPK);
			AssertNull(incident.ParentFeatureRequest);
			AssertEquals(ZGuid.Empty, incident.ParentFeatureRequestPK);
		}

		public void TestRelatedFeatureRequests()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.SetupForNewCreatedFeatureRequest();

			SupportIncident incident2 = Factory.New<SupportIncident>();
			incident2.SetupForNewCreatedFeatureRequest();

			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident2.IM_Category);

			incident.RelatedFeatureRequests.Add(incident2);
			AssertEquals(1, incident.RelatedFeatureRequests.Count);
			AssertEquals(0, incident2.RelatedFeatureRequests.Count);
		}

		[ExpectNoExceptions]
		public void TestOnRelatedWorkItemEvents()
		{
			SupportIncident feature1 = Factory.NewWithValidTestData<SupportIncident>();
			feature1.SetupForNewCreatedFeatureRequest();
			SupportIncident feature2 = Factory.NewWithValidTestData<SupportIncident>();
			feature2.SetupForNewCreatedFeatureRequest();
			SupportIncident feature3 = Factory.NewWithValidTestData<SupportIncident>();
			feature3.SetupForNewCreatedFeatureRequest();

			feature1.RelatedFeatureRequests.Add(feature2);
			feature2.RelatedFeatureRequests.Add(feature3);
			feature3.RelatedFeatureRequests.Add(feature1);

			SupportIncident feature4 = Factory.NewWithValidTestData<SupportIncident>();
			feature4.SetupForNewCreatedFeatureRequest();
			SupportIncident feature5 = Factory.NewWithValidTestData<SupportIncident>();
			feature5.SetupForNewCreatedFeatureRequest();

			feature4.RelatedFeatureRequests.Add(feature5);
			feature5.RelatedFeatureRequests.Add(feature4);

			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();

			((IWorkItemRelatedItem)feature1).OnWorkItemAdded(workItem);
			((IWorkItemRelatedItem)feature1).OnWorkItemRemoved(workItem);
			((IWorkItemRelatedItem)feature1).OnRelatedWorkItemClosed(workItem);
			((IWorkItemRelatedItem)feature1).OnRelatedWorkItemReOpened(workItem);

			((IWorkItemRelatedItem)feature4).OnWorkItemAdded(workItem);
			((IWorkItemRelatedItem)feature4).OnWorkItemRemoved(workItem);
			((IWorkItemRelatedItem)feature4).OnRelatedWorkItemClosed(workItem);
			((IWorkItemRelatedItem)feature4).OnRelatedWorkItemReOpened(workItem);

			Assert("Already pass stack overflow exception check if arrive here", true);
		}

		#endregion

		#region Defect Caused By Work Item

		public void TestDefectCausedByWorkItem()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.SetupForNewCreatedDefect();
			NewWorkItem workitem = Factory.NewWithValidTestData<NewWorkItem>();
			incident.DefectCausedByWorkItemPK = workitem.PK;
			Factory.Save();

			BusinessObjectFactory newfactory = new BusinessObjectFactory();
			SupportIncident loadIncident = newfactory.Load<SupportIncident>(incident.PK);
			AssertEquals(workitem.PK, loadIncident.DefectCausedByWorkItemPK);

			NewWorkItem workitem2 = Factory.NewWithValidTestData<NewWorkItem>();
			incident.DefectCausedByWorkItemPK = workitem2.PK;
			Factory.Save();

			newfactory = new BusinessObjectFactory();
			loadIncident = newfactory.Load<SupportIncident>(incident.PK);
			AssertEquals(workitem2.PK, loadIncident.DefectCausedByWorkItemPK);
		}

		public void TestDefectCausedByWorkItem_Attach()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.SetupForNewCreatedDefect();
			NewWorkItem workitem = Factory.NewWithValidTestData<NewWorkItem>();
			incident.DefectCausedByWorkItemPK = workitem.PK;
			Factory.Save();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			SupportIncident loadIncident = factory.Load<SupportIncident>(incident.PK);
			AssertEquals(workitem.PK, loadIncident.DefectCausedByWorkItemPK);
		}

		public void TestDefectCausedByWorkItem_Detach()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.SetupForNewCreatedDefect();
			NewWorkItem workitem = Factory.NewWithValidTestData<NewWorkItem>();
			incident.DefectCausedByWorkItemPK = workitem.PK;
			Factory.Save();

			BusinessObjectFactory newfactory = new BusinessObjectFactory();
			SupportIncident loadIncident = newfactory.Load<SupportIncident>(incident.PK);
			AssertEquals(workitem.PK, loadIncident.DefectCausedByWorkItemPK);

			incident.DefectCausedByWorkItemPK = ZGuid.Empty;
			Factory.Save();

			newfactory = new BusinessObjectFactory();
			loadIncident = newfactory.Load<SupportIncident>(incident.PK);
			AssertEquals(ZGuid.Empty, loadIncident.DefectCausedByWorkItemPK);
			AssertNull("The link should be deleted", loadIncident.LoadDefectCausedByIncidentPivot());
		}

		public void TestDefectCausedByWorkItem_Delete()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.SetupForNewCreatedDefect();
			NewWorkItem workitem = Factory.NewWithValidTestData<NewWorkItem>();
			incident.DefectCausedByWorkItemPK = workitem.PK;
			Factory.Save();

			AssertNotNull(GenPivot.LoadRelation2Pivot(Factory, incident.PK, IncidentMainSchema.Constants.Prefix, EDIGenPivotTypes.DefectCausedByWorkItem));

			BusinessObjectFactory newfactory = new BusinessObjectFactory();
			SupportIncident loadIncident = newfactory.Load<SupportIncident>(incident.PK);
			AssertEquals(workitem.PK, loadIncident.DefectCausedByWorkItemPK);
			loadIncident.Delete();
			newfactory.Save();

			AssertNull("link deleted", GenPivot.LoadRelation2Pivot(new BusinessObjectFactory(), incident.PK, IncidentMainSchema.Constants.Prefix, EDIGenPivotTypes.DefectCausedByWorkItem));
		}

		public void TestDefectCausedByWorkItem_MultipleIncidents()
		{
			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.SetupForNewCreatedDefect();

			NewWorkItem workitem = Factory.NewWithValidTestData<NewWorkItem>();
			incident1.DefectCausedByWorkItemPK = workitem.PK;

			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.SetupForNewCreatedDefect();
			incident2.DefectCausedByWorkItemPK = workitem.PK;

			Factory.Save();

			BusinessObjectFactory newfactory = new BusinessObjectFactory();
			SupportIncident loadIncident1 = newfactory.Load<SupportIncident>(incident1.PK);
			loadIncident1.AddStaffMessageToCustomer("Test");

			SupportIncident loadIncident2 = newfactory.Load<SupportIncident>(incident2.PK);
			loadIncident2.AddStaffMessageToCustomer("EEEE");
			newfactory.Save();

			newfactory = new BusinessObjectFactory();
			loadIncident1 = newfactory.Load<SupportIncident>(incident1.PK);
			loadIncident2 = newfactory.Load<SupportIncident>(incident2.PK);
			AssertEquals(workitem.PK, loadIncident1.DefectCausedByWorkItemPK);
			AssertEquals(workitem.PK, loadIncident2.DefectCausedByWorkItemPK);
		}

		#endregion

		#region Last Ten Incidents For Same Enterprise

		public void TestLastTenIncidentsForSameClient()
		{
			OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client3 = Factory.NewWithValidTestData<OrgHeader>();

			PrepareLicencesForTest(client1, client2, client3);
			PrepareIncidentsDataForTest(client1, client2, client3);

			SupportIncident newIncident = Factory.NewWithValidTestData<SupportIncident>();
			AssertEquals("Pre-condition", 0, newIncident.LastTenIncidentsForSameEnterprise.Count);

			newIncident.IM_OH_Client = client1.PK;
			AssertEquals("Should have ten incidents", 10, newIncident.LastTenIncidentsForSameEnterprise.Count);
			IEnumerable<SupportIncident> incidents = newIncident.LastTenIncidentsForSameEnterprise.Cast<SupportIncident>();
			AssertEquals(0, incidents.Count(i => i.IM_IncidentNumber == "CS1$"));
			AssertEquals(1, incidents.Count(i => i.IM_IncidentNumber == "CS2$"));
			AssertEquals(1, incidents.Count(i => i.IM_IncidentNumber == "CS3$"));
			AssertEquals(1, incidents.Count(i => i.IM_IncidentNumber == "CS4$"));
			AssertEquals(1, incidents.Count(i => i.IM_IncidentNumber == "CS5$"));
			AssertEquals(0, incidents.Count(i => i.IM_IncidentNumber == "CS6$"));
			AssertEquals(1, incidents.Count(i => i.IM_IncidentNumber == "CS7$"));
			AssertEquals(1, incidents.Count(i => i.IM_IncidentNumber == "CS8$"));
			AssertEquals(1, incidents.Count(i => i.IM_IncidentNumber == "CS9$"));
			AssertEquals(1, incidents.Count(i => i.IM_IncidentNumber == "CS10$"));
			AssertEquals(1, incidents.Count(i => i.IM_IncidentNumber == "CS11$"));
			AssertEquals(1, incidents.Count(i => i.IM_IncidentNumber == "CS12$"));
		}

		public void TestLastTenIncidentsForSameClientAsText()
		{
			OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client3 = Factory.NewWithValidTestData<OrgHeader>();

			PrepareLicencesForTest(client1, client2, client3);
			PrepareIncidentsDataForTest(client1, client2, client3);

			SupportIncident newIncident = Factory.NewWithValidTestData<SupportIncident>();
			AssertEquals("Pre-condition", ZString.Empty, newIncident.LastTenIncidentsForSameEnterpriseAsText);
			newIncident.IM_OH_Client = client1.PK;
			ZString incidentsAsText = newIncident.LastTenIncidentsForSameEnterpriseAsText;

			AssertNotContains("CS1$", incidentsAsText);
			AssertNotContains("Incident 1$", incidentsAsText);
			AssertContains("CS2$", incidentsAsText);
			AssertContains("Incident 2$", incidentsAsText);
			AssertContains("CS3$", incidentsAsText);
			AssertContains("Incident 3$", incidentsAsText);
			AssertContains("CS4$", incidentsAsText);
			AssertContains("Incident 4$", incidentsAsText);
			AssertContains("CS5$", incidentsAsText);
			AssertContains("Incident 5$", incidentsAsText);
			AssertNotContains("CS6$", incidentsAsText);
			AssertNotContains("Incident 6$", incidentsAsText);
			AssertContains("CS7$", incidentsAsText);
			AssertContains("Incident 7$", incidentsAsText);
			AssertContains("CS8$", incidentsAsText);
			AssertContains("Incident 8$", incidentsAsText);
			AssertContains("CS9$", incidentsAsText);
			AssertContains("Incident 9$", incidentsAsText);
			AssertContains("CS10$", incidentsAsText);
			AssertContains("Incident 10$", incidentsAsText);
			AssertContains("CS11$", incidentsAsText);
			AssertContains("Incident 11$", incidentsAsText);
			AssertContains("CS12$", incidentsAsText);
			AssertContains("Incident 12$", incidentsAsText);
		}

		void PrepareLicencesForTest(OrgHeader client1, OrgHeader client2, OrgHeader client3)
		{
			LicenceEnterprise enterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			LicenceEnterprise enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise1.LE_EnterpriseCode = "AAA";
			enterprise2.LE_EnterpriseCode = "BBB";
			enterprise1.LE_OH = client1.PK;

			LicenceCompany company1 = Factory.NewWithValidTestData<LicenceCompany>();
			LicenceCompany company2 = Factory.NewWithValidTestData<LicenceCompany>();
			LicenceCompany company3 = Factory.NewWithValidTestData<LicenceCompany>();
			company1.LC_OH = client1.PK;
			company2.LC_OH = client2.PK;
			company3.LC_OH = client3.PK;
			company1.LC_LE = enterprise1.PK;
			company2.LC_LE = enterprise2.PK;
			company3.LC_LE = enterprise1.PK;
		}

		void PrepareIncidentsDataForTest(OrgHeader client1, OrgHeader client2, OrgHeader client3)
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "CS1$";
			incident.IM_Description = "Incident 1$";
			Factory.Save();

			for (int i = 2; i <= 12; i++)
			{
				incident = Factory.NewWithValidTestData<SupportIncident>();
				incident.IM_IncidentNumber = "CS" + i + "$";
				incident.IM_Description = "Incident " + i + "$";

				if (i == 6)
				{
					incident.IM_OH_Client = client2.PK;
				}
				else if (i > 7)
				{
					incident.IM_OH_Client = client1.PK;
				}
				else
				{
					incident.IM_OH_Client = client3.PK;
				}
			}
			Factory.Save();
		}

		#endregion

		#region Email

		[TestDate(2012, 6, 28, 9, 0, 0)]
		public void TestSendEstimateOrQuoteEmailIfApplicable()
		{
			GlbStaff.CurrentUser.GS_FullName = "Jenny Nguyen";

			var collection = new CodeDescriptionIncidentEmailTemplatePairCollection(typeof(DocSupportIncident));
			var templatePair = collection.AddNew();
			templatePair.Code = SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided;
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailSubject = "DevelopmentEstimateProvided Subject";
			templatePair.EmailTemplates.LegacyAndERequestV1EmailTemplate.EmailBody = "DevelopmentEstimateProvided (*ResolutionNoteText*)";
			EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_ReferenceType = "ALL";
			docType.RT_DocType = "SES";
			docType.RT_Desc = "Software Estimate";
			docType.RT_IsPublished = ZBool.True;

			SupportIncident incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLegacyClient();
			incident.ClientCompany.Database.LD_PublicEmailAddressForUpdate = string.Empty; // prevent the automated system emails
			incident.IM_OC_Contact = Contact.PK;
			incident.IM_Module = ModuleTreeCustomerServiceMenuSectionList.Codes.ArchiveManager;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Priority = "CR7";
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "Incident details";
			incident.IM_ClientIncidentReference = "SR0023492";
			incident.ChangeIncidentStageFromSupportToFeatureRequestWithoutLogging();

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			byte[] pdfBody = new byte[] { 1, 1, 1, 1, 1 };
			var eDoc = Incident.DocManagerInfo.AddFileOrDocument(pdfBody, "Estimate.pdf", docType.RT_DocType);
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided;
			incident.EstimateOrQuoteAdded("Please find estimate attached", eDoc);
			AssertEquals("no emails yet", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			Factory.Save();

			AssertEquals("email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("From Display Name", SupportIncident.SupportDisplayName, email.FromDisplayName);
			AssertEquals("From Email Address", SupportIncident.SupportEmailAddress, email.FromAddress);
			AssertEquals("subject", "DevelopmentEstimateProvided Subject", email.Subject);
			AssertContains("body", "DevelopmentEstimateProvided", email.Body);
			AssertContains("body", "Please find estimate attached", email.Body);
			AssertContains("signature", "Jenny Nguyen", email.Body);
			AssertContains("signature", SupportIncident.SupportEmailAddress, email.Body);
			var attachment = email.Attachments.Cast<AttachmentDef>().First(s => s.DisplayName == "Estimate.pdf");
			AssertArrayEqualsByElements("attachment body", pdfBody, attachment.Data);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			EDIDataRegistry.Instance.CustomerServiceIncidentClosedNotificationEmailTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CodeDescriptionIncidentEmailTemplatePairCollection());
			incident.EstimateOrQuoteAdded("Please find estimate attached", eDoc);
			Factory.Save();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
			AssertEquals(true, loadedIncident.EConversation.GetTimeOrderedMessages().Any(msg => msg.Body == "No development estimate provided notification email sent because no template has been setup"));
		}

		#region Email Action

		void SetUpIncidentForEmailing(SupportIncident incident)
		{
			incident.FillWithValidTestData();

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_FullName = "New Client";

			OrgContact contact = client.Contacts.AddNew();
			contact.OC_ContactName = "Joe";
			contact.OC_Email = "joe@eoj";

			OrgAddress address = client.MainAddress;
			address.OA_Phone = "12345";
			address.OA_Fax = "98765";
			address.OA_OH = client.PK;

			GlbStaff newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_LoginName = "newguy";
			newStaff.GS_FullName = "New Guy";
			newStaff.GS_EmailAddress = "newguy@edi.com.au";

			incident.IM_Status = "OPN";
			incident.IM_GS_NKAssignedToCurrent = newStaff.GS_Code;
			incident.IM_OH_Client = client.PK;
			incident.IM_OA_BranchAddress = address.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Description = "Description, this goes in subject";
			incident.DetailNoteText = "Some Details!";

			Factory.Save();
		}

		public void TestGetEmailWhenHasChanges()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.EmailPreconditionFailure += new EventHandler<SupportIncident.EmailPreconditionFailureArgs>(Incident_EmailPreconditionFailure);
			incident.HasChanges = true;
			incident.GetEmailObjectForNotification();
			AssertEquals("Changes have been made to this record. You must save before sending an Email.", LastEmailFailureMessage);
			LastEmailFailureMessage = "";
		}

		public void TestGetEmailWhenClientIsEmpty()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.EmailPreconditionFailure += new EventHandler<SupportIncident.EmailPreconditionFailureArgs>(Incident_EmailPreconditionFailure);
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();
			incident.GetEmailObjectForNotification();
			AssertEquals("Please specify a Client before sending an Email.", LastEmailFailureMessage);
			LastEmailFailureMessage = "";
		}

		public void TestGetEmailThrowsExceptionWhenContactIsEmpty()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.EmailPreconditionFailure += new EventHandler<SupportIncident.EmailPreconditionFailureArgs>(Incident_EmailPreconditionFailure);
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			incident.IM_OH_Client = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			Factory.Save();
			incident.GetEmailObjectForNotification();
			AssertEquals("Please specify a Contact before sending an Email.", LastEmailFailureMessage);
			LastEmailFailureMessage = "";
		}

		void Incident_EmailPreconditionFailure(object sender, SupportIncident.EmailPreconditionFailureArgs e)
		{
			LastEmailFailureMessage = e.FailureReason;
		}

		string LastEmailFailureMessage;

		public void TestGetIncidentEmailToContactBusinessObject_Notification()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			SetUpIncidentForEmailing(incident);

			GlbStaff.CurrentUser.GS_FullName = "Sergey Gordok";
			var emailContactObject = incident.GetEmailObjectForNotification();

			AssertEquals("EmailContactObject.BusinessObjectSendingEmail", incident, emailContactObject.BusinessObjectSendingEmail);
			AssertEquals("EmailContactObject.FromDisplayName", SupportIncident.SupportDisplayName, emailContactObject.FromDisplayName);
			AssertEquals("EmailContactObject.FromEmailAddress", SupportIncident.SupportEmailAddress, emailContactObject.FromEmailAddress);
			AssertEquals("EmailContactObject.To", "joe@eoj", emailContactObject.ToEmailAddress);
			AssertEquals("EmailContactObject.Cc", "", emailContactObject.Cc);
			AssertEquals("EmailContactObject.Subject", "Notification of Incident: " + incident.IM_IncidentNumber + " - Description, this goes in subject", emailContactObject.Subject);

			AssertContains("EmailContactObject.Body", "Your Incident Number is: " + incident.IM_IncidentNumber + ".", emailContactObject.Body);
			AssertContains("EmailContactObject.Body", "We are also confirming your contact details are:", emailContactObject.Body);
			AssertContains("EmailContactObject.Body", "Name: " + incident.Client.OH_FullName, emailContactObject.Body);
			AssertContains("EmailContactObject.Body", "Phone: " + incident.BranchAddress.OA_Phone, emailContactObject.Body);
			AssertContains("EmailContactObject.Body", "If your contact details are incorrect, or if you require further clarification regarding this issue please let us know by directly replying to this email.", emailContactObject.Body);
		}

		public void TestGetEmailObjectForCorrespondence()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			SetUpIncidentForEmailing(incident);

			GlbStaff.CurrentUser.GS_FullName = "Sergey Gordok";
			GlbStaff.CurrentUser.GS_EmailAddress = "sergey.gordok@cargowise.com";
			var email = incident.GetEmailObjectForCorrespondence();

			AssertEquals("BusinessObjectSendingEmail", incident, email.BusinessObjectSendingEmail);
			AssertEquals("FromDisplayName", SupportIncident.SupportDisplayName, email.FromDisplayName);
			AssertEquals("FromEmailAddress", SupportIncident.SupportEmailAddress, email.FromEmailAddress);
			AssertEquals("To", "joe@eoj", email.ToEmailAddress);
			AssertEquals("Cc", "", email.Cc);
			AssertEquals("Subject", "Update on Incident: " + incident.IM_IncidentNumber + " - Description, this goes in subject", email.Subject);
			Assert(!email.UseCurrentUsersNameAndTitle);
			Assert(!email.UseCurrentUsersEmailAddress);

			AssertContains("Body", "This email is regarding Customer Service Incident " + incident.IM_IncidentNumber + ".", email.Body);
			AssertContains("Body", "(Enter correspondence details here)", email.Body);
			AssertContains("Body", incident.DetailNoteText, email.Body);

			GlbStaff someStaff = Factory.NewWithValidTestData<GlbStaff>();
			someStaff.GS_Code = "ZAC";
			incident.IM_GS_NKAssignedToCurrent = someStaff.GS_Code;
			incident.IM_CloseTimeUtc = ZDateTime.Now;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction;

			email.SendEmail();

			AssertEquals("When sending email updates, the incident should remain with the currently assigned staff", someStaff.GS_Code, incident.IM_GS_NKAssignedToCurrent);
			Assert("Internal comment added", incident.EConversation.AnyLocalMessageContains("Email Update sent and attached to eDocs"));
		}

		public void TestGetEmailObjectForCorrespondence_SuspendsValidation()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.FillWithValidTestData();
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();

			contact.OC_Email = "";
			incident.IM_OH_Client = client.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;

			GlbStaff.CurrentUser.GS_FullName = "Ted Burhan";
			GlbStaff.CurrentUser.GS_EmailAddress = "ted.burhan@cargowise.com";

			Factory.Save();

			var email = incident.GetEmailObjectForCorrespondence();

			AssertEquals("ToEmailAddress", "", email.ToEmailAddress);
			AssertNoErrors(email);
			AssertEquals("IsValidationSuspended", false, email.IsValidationSuspended);

			email.ToEmailAddress = "";
			AssertHasError(email.ToEmailAddressInfo, "Please enter a value.");
		}

		#endregion

		protected ClientControllerID ExpectedControllerIDForHtmlUrl
		{
			get { return ClientControllerRegistration.SupportIncident; }
		}

		#endregion

		#region Actions

		#region General

		public void TestEscalateIncident_IncidentEvent()
		{
			#region Template setup

			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_OH_Client = templateOrg.PK;
			template1.P0_SubType2 = SupportIncidentCategoriesList.Codes.Defect;
			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "ESC Escalate Defect";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 5;
			task11.P9_Type = "INV";
			task11.P9_Description = "Escalate as defect";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task12 = template1.WorkflowItems.AddNew();
			task12.P9_FH_ProcessHeader = header1.PK;
			task12.P9_Sequence = 20;
			task12.P9_Type = "INV";
			task12.P9_Description = "Schedule work item";
			task12.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "INC";
			template2.P0_OH_Client = templateOrg.PK;
			template2.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;
			var header2 = template2.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "ESC Escalate Feature";

			var task21 = template2.WorkflowItems.AddNew();
			task21.P9_FH_ProcessHeader = header2.PK;
			task21.P9_Sequence = 5;
			task21.P9_Type = "INV";
			task21.P9_Description = "Escalate as feature request";
			task21.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task22 = template2.WorkflowItems.AddNew();
			task22.P9_FH_ProcessHeader = header2.PK;
			task22.P9_Sequence = 20;
			task22.P9_Type = "INV";
			task22.P9_Description = "Create work item";
			task22.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_ProcessType = "INC";
			template3.P0_OH_Client = templateOrg.PK;
			template3.P0_SubType2 = SupportIncidentCategoriesList.Codes.Support;
			var header3 = template3.ProcessHeaders.AddNew();
			header3.FH_CompletionStatement = "ESC Pass Back to Support";

			var task31 = template3.WorkflowItems.AddNew();
			task31.P9_FH_ProcessHeader = header3.PK;
			task31.P9_Sequence = 10;
			task31.P9_Type = "INV";
			task31.P9_Description = "Investigate";
			task31.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			#endregion

			#region Company setup

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DDDAAASYD";

			var enteprise = Factory.New<LicenceEnterprise>();
			enteprise.LE_OH = org.PK;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SYD";
			database.LD_LE = enteprise.PK;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_LD = database.PK;
			clientCompany.LCC_Code = "AAA";
			clientCompany.LCC_OH = ZGuid.Empty;

			#endregion

			Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;

			var existingTask1 = incident.WorkflowItems.AddNew();
			existingTask1.P9_Sequence = 10;
			var existingTask2 = incident.WorkflowItems.AddNew();
			existingTask2.P9_Sequence = 20;

			Factory.Save();

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			Factory.Save();

			AssertEquals("Database shouldn't change", database.PK, incident.IM_LD);
			AssertEquals("DB company shouldn't change", clientCompany.PK, incident.IM_LCC);

			AssertEquals(4, incident.WorkflowItems.Count);
			AssertEquals("Escalate as defect", incident.WorkflowItems[2].P9_Description);
			AssertEquals("Schedule work item", incident.WorkflowItems[3].P9_Description);
			AssertEquals("Escalate Defect", incident.WorkflowItems[2].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Escalate Defect", incident.WorkflowItems[3].ProcessHeader.FH_CompletionStatement);
			AssertEquals(25, incident.WorkflowItems[2].P9_Sequence);
			AssertEquals(40, incident.WorkflowItems[3].P9_Sequence);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[2].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[3].P9_Status);
			AssertEquals("", incident.WorkflowItems[2].P9_GS_NKAssignedStaffMember);

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			Factory.Save();

			AssertEquals(6, incident.WorkflowItems.Count);
			AssertEquals("Escalate as feature request", incident.WorkflowItems[4].P9_Description);
			AssertEquals("Create work item", incident.WorkflowItems[5].P9_Description);
			AssertEquals("Escalate Feature", incident.WorkflowItems[4].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Escalate Feature", incident.WorkflowItems[5].ProcessHeader.FH_CompletionStatement);
			AssertEquals(45, incident.WorkflowItems[4].P9_Sequence);
			AssertEquals(60, incident.WorkflowItems[5].P9_Sequence);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[4].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[5].P9_Status);
			AssertEquals("", incident.WorkflowItems[4].P9_GS_NKAssignedStaffMember);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			Factory.Save();

			AssertEquals(7, incident.WorkflowItems.Count);
			AssertEquals("Investigate", incident.WorkflowItems[6].P9_Description);
			AssertEquals("Pass Back to Support", incident.WorkflowItems[6].ProcessHeader.FH_CompletionStatement);
			AssertEquals(70, incident.WorkflowItems[6].P9_Sequence);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[6].P9_Status);
			AssertEquals("", incident.WorkflowItems[6].P9_GS_NKAssignedStaffMember);
		}

		public void TestEscalateClosedIncidentChangingStageShouldRefreshDisposition()
		{
			#region Template setup

			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_OH_Client = templateOrg.PK;
			template1.P0_SubType2 = SupportIncidentCategoriesList.Codes.Defect;
			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "ESC Escalate Defect";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 5;
			task11.P9_Type = "INV";
			task11.P9_Description = "Escalate as defect";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task12 = template1.WorkflowItems.AddNew();
			task12.P9_FH_ProcessHeader = header1.PK;
			task12.P9_Sequence = 20;
			task12.P9_Type = "INV";
			task12.P9_Description = "Schedule work item";
			task12.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "INC";
			template2.P0_OH_Client = templateOrg.PK;
			template2.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;
			var header2 = template2.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "ESC Escalate Feature";

			var task21 = template2.WorkflowItems.AddNew();
			task21.P9_FH_ProcessHeader = header2.PK;
			task21.P9_Sequence = 5;
			task21.P9_Type = "INV";
			task21.P9_Description = "Escalate as feature request";
			task21.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task22 = template2.WorkflowItems.AddNew();
			task22.P9_FH_ProcessHeader = header2.PK;
			task22.P9_Sequence = 20;
			task22.P9_Type = "INV";
			task22.P9_Description = "Create work item";
			task22.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_ProcessType = "INC";
			template3.P0_OH_Client = templateOrg.PK;
			template3.P0_SubType2 = SupportIncidentCategoriesList.Codes.Support;
			var header3 = template3.ProcessHeaders.AddNew();
			header3.FH_CompletionStatement = "ESC Pass Back to Support";

			var task31 = template3.WorkflowItems.AddNew();
			task31.P9_FH_ProcessHeader = header3.PK;
			task31.P9_Sequence = 10;
			task31.P9_Type = "INV";
			task31.P9_Description = "Back to Support";
			task31.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			#endregion

			#region Company setup

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "DDDAAASYD";

			var enteprise = Factory.New<LicenceEnterprise>();
			enteprise.LE_OH = org.PK;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SYD";
			database.LD_LE = enteprise.PK;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_LD = database.PK;
			clientCompany.LCC_Code = "AAA";
			clientCompany.LCC_OH = ZGuid.Empty;

			#endregion

			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = incident2.IM_OH_Client = org.PK;
			incident1.IM_LD = incident2.IM_LD = database.PK;
			incident1.IM_LCC = incident2.IM_LCC = clientCompany.PK;

			var existingTask1 = incident1.WorkflowItems.AddNew();
			existingTask1.P9_Sequence = 10;
			var existingTask2 = incident1.WorkflowItems.AddNew();
			existingTask2.P9_Sequence = 20;
			var existingTask3 = incident2.WorkflowItems.AddNew();
			existingTask3.P9_Sequence = 10;
			var existingTask4 = incident2.WorkflowItems.AddNew();
			existingTask4.P9_Sequence = 20;

			Factory.Save();

			incident1.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			incident2.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			Factory.Save();

			incident1.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			incident1.CloseIncident(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, string.Empty);
			incident2.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, string.Empty);
			Factory.Save();

			AssertEquals("Precondition", SupportIncidentCategoriesList.Codes.FeatureRequest, incident1.IM_Category);
			AssertEquals("Precondition", SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, incident1.IM_ResolutionCode);
			AssertEquals("Precondition", string.Empty, incident1.IM_ClosureResolution);
			AssertEquals("Precondition", SupportIncidentCategoriesList.Codes.Support, incident2.IM_Category);
			AssertEquals("Precondition", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident2.IM_ResolutionCode);
			AssertEquals("Precondition", SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, incident2.IM_ClosureResolution);

			incident1.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			incident2.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			incident1.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			incident2.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			Factory.Save();

			AssertEquals(5, incident1.WorkflowItems.Count);
			AssertEquals("Precondition: Should apply WF template", "Back to Support", incident1.WorkflowItems[4].P9_Description);
			AssertEquals("Precondition: Should update stage to support", SupportIncidentCategoriesList.Codes.Support, incident1.IM_Category);
			AssertEquals("ERequest Status should be recalculated", SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident1.IM_ResolutionCode);
			AssertEquals("Closure resolution should remain unset", string.Empty, incident1.IM_ClosureResolution);
			AssertEquals("Precondition: Stage should stay as support", SupportIncidentCategoriesList.Codes.Support, incident2.IM_Category);
			AssertEquals("ERequest Status should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident2.IM_ResolutionCode);
			AssertEquals("Closure Resolution should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, incident2.IM_ClosureResolution);
		}

		public void TestCloseIncidentAsAcceptedFeatureRequest()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			incident.CloseAsAcceptedFeatureRequest("It is a FR so closed.");
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, incident.IM_ResolutionCode);

			SupportIncident defect = Factory.New<SupportIncident>();
			defect.SetupForNewCreatedDefect();
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, defect.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, defect.IM_Status);
			defect.CloseAsAcceptedFeatureRequest("It is a FR so closed.");
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, defect.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, defect.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, defect.IM_ResolutionCode);

			SupportIncident feature = Factory.New<SupportIncident>();
			feature.SetupForNewCreatedFeatureRequest();
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, feature.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, feature.IM_Status);
			feature.CloseAsAcceptedFeatureRequest("It is a FR so closed.");
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, feature.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, feature.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, feature.IM_ResolutionCode);
		}

		#endregion

		#region Support

		#region Close

		[TestDate(2005, 1, 4, 13, 30, 0)]
		public void TestCloseSupportIncident()
		{
			GlbStaff staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "S2";
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = staff2.GS_Code;
			incident.IM_SystemCreateTimeUtc = new ZDateTime(2005, 1, 3, 9, 0, 0);

			string existingNote = incident.ResolutionNoteText;
			incident.CloseIncident("TRN", "I am closing because i hate it");
			AssertEquals(staff2.PK, incident.CustServiceContact.PK);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals("TRN", incident.IM_ClosureResolution);
			AssertContains(ZDateTime.Now.ToString() + " " + GlbStaff.CurrentUser.GS_Code + " - Closed As Training - customer referred to eLearning materials - I am closing because i hate it", incident.ResolutionNoteText);
			AssertEquals(new ZDateTime(2005, 1, 4, 13, 30, 0), incident.CloseInSupportDate);

			TestDateAttribute.Date = new DateTime(2005, 1, 5, 13, 0, 0);
			incident.CloseIncident("SYS", "");
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals("SYS", incident.IM_ClosureResolution);
			AssertContains(ZDateTime.Now.ToString() + " " + GlbStaff.CurrentUser.GS_Code + " - Closed", incident.ResolutionNoteText);
		}

		public void TestSupportIncidentCloseDate()
		{
			SupportIncident incident1 = Factory.New<SupportIncident>();
			incident1.IM_Category = "SUP";
			SupportIncident incident2 = Factory.New<SupportIncident>();
			incident2.IM_Category = "SUP";
			SupportIncident incident3 = Factory.New<SupportIncident>();
			incident3.IM_Category = "SUP";
			Factory.Save();

			Assert(incident1.IM_CloseTimeUtc.IsEmpty);
			Assert(incident2.IM_CloseTimeUtc.IsEmpty);
			Assert(incident3.IM_CloseTimeUtc.IsEmpty);

			incident1.Escalate(SupportIncidentCategoriesList.Codes.Defect, null);
			incident2.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, null);
			Factory.Save();
			Assert(incident1.IM_CloseTimeUtc.IsEmpty);
			Assert(incident2.IM_CloseTimeUtc.IsEmpty);

			incident1.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			incident2.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			incident3.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingReferredToLearningMaterials, "");
			Factory.Save();
			Assert(!incident1.IM_CloseTimeUtc.IsEmpty);
			Assert(!incident2.IM_CloseTimeUtc.IsEmpty);
			Assert(!incident3.IM_CloseTimeUtc.IsEmpty);
		}

		public void TestCloseAsAwaitingResponse()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Category = "SUP";
			Factory.Save();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Awaiting Client Response"));
			AssertEquals(false, messageList.Any(msg => msg.Body == "Closed As Awaiting Client Response"));
		}

		public void TestMessageShouldStartWithResolvedWhenClosureMethodIsResolution()
		{
			var incident1 = Factory.New<SupportIncident>();
			incident1.IM_Category = "SUP";
			var incident2 = Factory.New<SupportIncident>();
			incident2.IM_Category = "SUP";
			Factory.Save();

			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			var resolvedDecription = "ZZZ Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)resolvedDecription, supportParent, ZBool.True));

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				incident1.CloseIncident("ZZZ", "");
				incident2.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
				Factory.Save();
				var messageList1 = incident1.EConversation.GetTimeOrderedMessages();
				AssertEquals(true, messageList1.Any(msg => msg.Body == "Resolved As ZZZ Description"));
				var messageList2 = incident2.EConversation.GetTimeOrderedMessages();
				AssertEquals(true, messageList2.Any(msg => msg.Body == "Closed As Self Resolved"));
			}
		}

		public void TestMessageShouldStartWithClosedWhenCloseViaCloseOnBehalfOfClient()
		{
			var incident1 = Factory.New<SupportIncident>();
			incident1.CloseViaCloseOnBehalfOfClient = false;
			incident1.IM_Category = "SUP";
			var incident2 = Factory.New<SupportIncident>();
			incident2.IM_Category = "SUP";
			incident2.CloseViaCloseOnBehalfOfClient = true;
			Factory.Save();

			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			var resolvedDecription = "ZZZ Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)resolvedDecription, supportParent, ZBool.True));

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				incident1.CloseIncident("ZZZ", "");
				incident2.CloseIncident("ZZZ", "");
				Factory.Save();
				var messageList1 = incident1.EConversation.GetTimeOrderedMessages();
				AssertEquals(true, messageList1.Any(msg => msg.Body == "Resolved As ZZZ Description"));
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, incident1.IM_ResolutionCode);
				var messageList2 = incident2.EConversation.GetTimeOrderedMessages();
				AssertEquals(true, messageList2.Any(msg => msg.Body == "Closed As ZZZ Description"));
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident2.IM_ResolutionCode);
			}
		}

		public void TestSystemMessageShouldPostWhenCloseViaCloseOnBehalfOfClient()
		{
			var incident1 = Factory.New<SupportIncident>();
			incident1.CloseViaCloseOnBehalfOfClient = false;
			incident1.IM_Category = "SUP";
			var incident2 = Factory.New<SupportIncident>();
			incident2.IM_Category = "SUP";
			incident2.CloseViaCloseOnBehalfOfClient = true;
			Factory.Save();

			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			var resolvedDecription = "ZZZ Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)resolvedDecription, supportParent, ZBool.True));

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				incident1.CloseIncident("ZZZ", "");
				incident2.CloseIncident("ZZZ", "");
				Factory.Save();
				var messageList1 = incident1.EConversation.GetTimeOrderedMessages();
				AssertEquals(true, messageList1.Any(msg => msg.Body == "Resolved As ZZZ Description"));
				AssertEquals(false, messageList1.Any(msg => msg.Body == "This incident was closed on behalf of the client by WiseTech Global support"));
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, incident1.IM_ResolutionCode);
				var messageList2 = incident2.EConversation.GetTimeOrderedMessages();
				AssertEquals(true, messageList2.Any(msg => msg.Body == "Closed As ZZZ Description"));
				AssertEquals(true, messageList2.Any(msg => msg.Body == "This incident was closed on behalf of the client by WiseTech Global support"));
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident2.IM_ResolutionCode);
			}
		}

		#endregion

		#region Investigate

		public void TestInvestigateInSupport()
		{
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			SupportIncident incident = Factory.New<SupportIncident>();

			incident.CloseIncident("TRN", "I am closing because i hate it");
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertZDatesWithin5Minutes("Close In Support Date", ZDateTime.Now, incident.CloseInSupportDate);
			incident.IM_GS_NKCustServiceContact = staff2.GS_Code;

			incident.InvestigateInSupport();
			AssertEquals(staff2.PK, incident.CustServiceContact.PK);
			AssertEquals(ZDateTime.Empty, incident.CloseInSupportDate);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);

			incident.IM_GS_NKCustServiceContact = ZString.Empty;
			incident.InvestigateInSupport();
			AssertEquals(GlbStaff.CurrentUser.PK, incident.CustServiceContact.PK);
			AssertEquals(ZDateTime.Empty, incident.CloseInSupportDate);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);
		}

		#endregion

		#region Re-Open

		[TestDate(2012, 5, 22, 14, 44, 0)]
		public void TestReopen_Support()
		{
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			SupportIncident incident = Factory.New<SupportIncident>();

			incident.CloseIncident("TRN", "I am closing because i hate it");
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertZDatesWithin5Minutes("Close In Support Date", ZDateTime.Now, incident.CloseInSupportDate);
			incident.RunPreSaveValidation();
			AssertContains("I am closing because i hate it", incident.ResolutionNoteText);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);

			AssertEquals(ZDateTime.Empty, incident.CloseInSupportDate);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);
			incident.RunPreSaveValidation();
			AssertEquals(ZDateTime.Now.ToString() + " E - Closed As Training - customer referred to eLearning materials - I am closing because i hate it", incident.ResolutionNoteText);
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Incident Re-opened"));

			incident.IM_GS_NKCustServiceContact = ZString.Empty;
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);

			AssertEquals(GlbStaff.CurrentUser.PK, incident.CustServiceContact.PK);
			AssertEquals(ZDateTime.Empty, incident.CloseInSupportDate);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);

			var webuser = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.WebUserCode);
			using (Env.SetTemporaryUserContext(webuser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.CsStageDataFix, "Test close");
				incident.IM_GS_NKCustServiceContact = "";
				incident.IM_GS_NKAssignedToCurrent = "";
				incident.Reopen(IncidentEventFactory.Codes.ERequestReopen);
			}

			AssertEquals("", incident.IM_GS_NKCustServiceContact);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);
		}

		public void TestReopen_Support_ByClient()
		{
			CreateWorkflowTemplate();

			GlbStaff pm1 = Factory.NewWithValidTestData<GlbStaff>();
			pm1.GS_Code = "PM1";
			pm1.GS_EmailAddress = "pm1@test.com";
			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "Module A", ProductAreaList.Codes.ARC, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			ProductAreaAssignmentCollection assignmentCollection = new ProductAreaAssignmentCollection();
			ProductAreaAssignment assignment1 = assignmentCollection.AddNew();
			assignment1.ProductArea = ProductAreaList.Codes.ARC;
			assignment1.Staff = pm1.GS_Code;
			EDIDataRegistry.Instance.ProductAreaAssignments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, assignmentCollection);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = "CR8";
			incident.IM_Module = "AAA";
			var existingTask1 = incident.WorkflowItems.AddNew();
			existingTask1.P9_Sequence = 10;
			var existingTask2 = incident.WorkflowItems.AddNew();
			existingTask2.P9_Sequence = 20;

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			incident.Reopen(IncidentEventFactory.Codes.ERequestReopen);
			Factory.Save();

			AssertEquals(5, incident.WorkflowItems.Count);

			AssertEquals("Contact Client", incident.WorkflowItems[2].P9_Description);
			AssertEquals("Investigate R2", incident.WorkflowItems[3].P9_Description);
			AssertEquals("Notify Client", incident.WorkflowItems[4].P9_Description);

			AssertEquals("Investigate", incident.WorkflowItems[2].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Investigate", incident.WorkflowItems[3].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Investigate", incident.WorkflowItems[4].ProcessHeader.FH_CompletionStatement);

			AssertEquals(30, incident.WorkflowItems[2].P9_Sequence);
			AssertEquals(40, incident.WorkflowItems[3].P9_Sequence);
			AssertEquals(70, incident.WorkflowItems[4].P9_Sequence);

			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, incident.WorkflowItems[2].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[3].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[4].P9_Status);

			AssertEquals("PM1", incident.WorkflowItems[2].P9_GS_NKAssignedStaffMember);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingReferredToLearningMaterials, "");
			incident.Reopen(IncidentEventFactory.Codes.ERequestReopen);
			Factory.Save();

			AssertEquals(8, incident.WorkflowItems.Count);

			AssertEquals("Contact Client", incident.WorkflowItems[5].P9_Description);
			AssertEquals("Investigate R2", incident.WorkflowItems[6].P9_Description);
			AssertEquals("Notify Client", incident.WorkflowItems[7].P9_Description);

			AssertEquals("Investigate", incident.WorkflowItems[5].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Investigate", incident.WorkflowItems[6].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Investigate", incident.WorkflowItems[7].ProcessHeader.FH_CompletionStatement);

			AssertEquals(80, incident.WorkflowItems[5].P9_Sequence);
			AssertEquals(90, incident.WorkflowItems[6].P9_Sequence);
			AssertEquals(120, incident.WorkflowItems[7].P9_Sequence);
		}

		public void TestReopen_Support_ByProdUser()
		{
			CreateWorkflowTemplate();

			GlbStaff pm1 = Factory.NewWithValidTestData<GlbStaff>();
			pm1.GS_Code = "PM2";
			pm1.GS_EmailAddress = "pm2@test.com";
			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("BBB", "Module B", ProductAreaList.Codes.ARC, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			ProductAreaAssignmentCollection assignmentCollection = new ProductAreaAssignmentCollection();
			ProductAreaAssignment assignment1 = assignmentCollection.AddNew();
			assignment1.ProductArea = ProductAreaList.Codes.ARC;
			assignment1.Staff = pm1.GS_Code;
			EDIDataRegistry.Instance.ProductAreaAssignments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, assignmentCollection);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = "CR8";
			incident.IM_Module = "BBB";
			var existingTask1 = incident.WorkflowItems.AddNew();
			existingTask1.P9_Sequence = 10;
			var existingTask2 = incident.WorkflowItems.AddNew();
			existingTask2.P9_Sequence = 20;

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			AssertEquals(6, incident.WorkflowItems.Count);

			AssertEquals("Reopen investigate", incident.WorkflowItems[2].P9_Description);
			AssertEquals("Notify Client User", incident.WorkflowItems[3].P9_Description);
			AssertEquals("Confirm Escalation", incident.WorkflowItems[4].P9_Description);
			AssertEquals("Notify CargoWise User", incident.WorkflowItems[5].P9_Description);

			AssertEquals("Investigate", incident.WorkflowItems[2].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Investigate", incident.WorkflowItems[3].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Escalate", incident.WorkflowItems[4].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Escalate", incident.WorkflowItems[5].ProcessHeader.FH_CompletionStatement);

			AssertEquals(25, incident.WorkflowItems[2].P9_Sequence);
			AssertEquals(40, incident.WorkflowItems[3].P9_Sequence);
			AssertEquals(50, incident.WorkflowItems[4].P9_Sequence);
			AssertEquals(70, incident.WorkflowItems[5].P9_Sequence);

			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, incident.WorkflowItems[2].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[3].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[4].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[5].P9_Status);

			AssertEquals("PM2", incident.WorkflowItems[2].P9_GS_NKAssignedStaffMember);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingReferredToLearningMaterials, "");
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			AssertEquals(10, incident.WorkflowItems.Count);

			AssertEquals("Reopen investigate", incident.WorkflowItems[6].P9_Description);
			AssertEquals("Notify Client User", incident.WorkflowItems[7].P9_Description);
			AssertEquals("Confirm Escalation", incident.WorkflowItems[8].P9_Description);
			AssertEquals("Notify CargoWise User", incident.WorkflowItems[9].P9_Description);

			AssertEquals("Investigate", incident.WorkflowItems[6].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Investigate", incident.WorkflowItems[7].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Escalate", incident.WorkflowItems[8].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Escalate", incident.WorkflowItems[9].ProcessHeader.FH_CompletionStatement);

			AssertEquals(75, incident.WorkflowItems[6].P9_Sequence);
			AssertEquals(90, incident.WorkflowItems[7].P9_Sequence);
			AssertEquals(100, incident.WorkflowItems[8].P9_Sequence);
			AssertEquals(120, incident.WorkflowItems[9].P9_Sequence);
		}

		public void TestReopen_FeatureRequest_CR7()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");

			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.AwaitingFormalQuotation;
			Factory.Save();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.AwaitingFormalQuotation, "");
			Factory.Save();

			incident.Reopen(IncidentEventFactory.Codes.ERequestReopen);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.AwaitingFormalQuotation, incident.IM_ResolutionCode);
		}

		void CreateWorkflowTemplate()
		{
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INC";
			template.P0_OH_Client = templateOrg.PK;

			var header1 = template.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "RCW Investigate";
			var header2 = template.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "RCW Escalate";
			var header3 = template.ProcessHeaders.AddNew();
			header3.FH_CompletionStatement = "REQ Investigate";

			var task11 = template.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 5;
			task11.P9_Type = "INV";
			task11.P9_Description = "Reopen investigate";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task12 = template.WorkflowItems.AddNew();
			task12.P9_FH_ProcessHeader = header1.PK;
			task12.P9_Sequence = 20;
			task12.P9_Type = "INV";
			task12.P9_Description = "Notify Client User";
			task12.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task21 = template.WorkflowItems.AddNew();
			task21.P9_FH_ProcessHeader = header2.PK;
			task21.P9_Sequence = 30;
			task21.P9_Type = "INV";
			task21.P9_Description = "Confirm Escalation";
			task21.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task22 = template.WorkflowItems.AddNew();
			task22.P9_FH_ProcessHeader = header2.PK;
			task22.P9_Sequence = 50;
			task22.P9_Type = "INV";
			task22.P9_Description = "Notify CargoWise User";
			task22.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task31 = template.WorkflowItems.AddNew();
			task31.P9_FH_ProcessHeader = header3.PK;
			task31.P9_Sequence = 60;
			task31.P9_Type = "AAA";
			task31.P9_Description = "Contact Client";
			task31.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task32 = template.WorkflowItems.AddNew();
			task32.P9_FH_ProcessHeader = header3.PK;
			task32.P9_Sequence = 70;
			task32.P9_Type = "AAA";
			task32.P9_Description = "Investigate R2";
			task32.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task33 = template.WorkflowItems.AddNew();
			task33.P9_FH_ProcessHeader = header3.PK;
			task33.P9_Sequence = 100;
			task33.P9_Type = "AAA";
			task33.P9_Description = "Notify Client";
			task33.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			Factory.Save();
		}

		#endregion

		#region Assign

		public void TestAssignToSupport()
		{
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			SupportIncident incident = Factory.New<SupportIncident>();

			incident.CloseIncident("TRN", "I am closing because i hate it");
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertZDatesWithin5Minutes("Close In Support Date", ZDateTime.Now, incident.CloseInSupportDate);
			incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;

			incident.AssignToStaff(staff2, "Some comment");
			AssertEquals(staff2.PK, incident.CustServiceContact.PK);
			AssertEquals(ZDateTime.Empty, incident.CloseInSupportDate);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals("AUC", incident.IM_ResolutionCode);

			string additionalNoteText = (string)incident.GetType().InvokeMember(
				"AdditionalNote",
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty,
				null,
				incident,
				null);
			AssertEquals("Email Additional Note", "<BR /><BR />Comment:<BR />Some comment", additionalNoteText);
		}

		public void TestAssignmentComment()
		{
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();

			incident.AssignToStaff(GlbStaff.CurrentUser, "BLABLABLA");
			AssertEquals("BLABLABLA", incident.AssignmentComment);

			incident.AssignToStaff(staff2, "Some comment");
			AssertEquals("Some comment", incident.AssignmentComment);

			Factory.Save();
			AssertEquals(null, incident.AssignmentComment);
		}

		#endregion

		#endregion

		#region Defect

		#region Return to Support

		public void TestReturnDefectToSupport()
		{
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "Escalated as defect");
			incident.IM_GS_NKAssignedToCurrent = staff2.GS_Code;

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "No defect found");

			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);

			AssertEquals(staff2.GS_Code, incident.IM_GS_NKAssignedToCurrent);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, incident.IM_GS_NKCustServiceContact);

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Escalated as defect"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "No defect found"));
		}

		public void TestReturnDefectToSupport_NoCustServiceContact()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = User.ServiceUserCode;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "Escalated as defect");
			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "No defect found");

			AssertEquals(SupportIncidentCategoriesList.Codes.Support, Incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);
			AssertEquals(ZString.Empty, incident.IM_GS_NKCustServiceContact);
		}

		#endregion

		#region Pass to Feature Request

		public void TestPassToFeatureRequest()
		{
			SupportIncident incident = Factory.New<SupportIncident>();

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "Escalated as defect");
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "Feature request required");

			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Escalated as defect"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "Feature request required"));
		}

		#endregion

		#region Reopen Defect

		public void TestReopen_Defect()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Description = "Close Incident";
			task1.P9_Sequence = 10;
			task1.P9_GS_NKAssignedStaffMember = "SCW";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var task2 = incident.WorkflowItems.AddNew();
			task2.P9_Description = "Notifiy Client";
			task2.P9_Sequence = 20;
			task2.P9_GS_NKAssignedStaffMember = "TST";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "SCW";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "TST";

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "DDD";

			Factory.Save();

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);

			incident.Reopen(IncidentEventFactory.Codes.ERequestReopen);
			Factory.Save();
			AssertEquals("Should be reopened as defect", SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals("Should be reopened", SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("New task is added", 3, incident.WorkflowItems.Count);
			AssertEquals("Current assignee", "SCW", incident.IM_GS_NKAssignedToCurrent);

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Incident Re-opened"));

			var task3 = incident.WorkflowItems[2];
			AssertEquals("Last closed task is cloned", "Close Incident", task3.P9_Description);
			AssertEquals("Last closed task is cloned", "SCW", task3.P9_GS_NKAssignedStaffMember);
			AssertEquals("Last closed task is cloned", 21, task3.P9_Sequence);
			AssertEquals("Last closed task is cloned", ProcessTaskStatusCodeList.Codes.Assigned, task3.P9_Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task3.P9_Description = "Cancel Incident";
			task3.P9_GS_NKAssignedStaffMember = "DDD";
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);

			incident.Reopen(IncidentEventFactory.Codes.ERequestReopen);
			Factory.Save();
			AssertEquals("Should be reopened as defect", SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals("Should be reopened", SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals("New task is added", 4, incident.WorkflowItems.Count);
			AssertEquals("Current assignee", "DDD", incident.IM_GS_NKAssignedToCurrent);

			var task4 = incident.WorkflowItems[3];
			AssertEquals("Last closed task is cloned", "Cancel Incident", task4.P9_Description);
			AssertEquals("Last closed task is cloned", "DDD", task4.P9_GS_NKAssignedStaffMember);
			AssertEquals("Last closed task is cloned", 22, task4.P9_Sequence);
			AssertEquals("Last closed task is cloned", ProcessTaskStatusCodeList.Codes.Assigned, task4.P9_Status);
		}

		public void TestReopen_Defect_ProductAreaAssignedStaff()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");

			incident.WorkflowItems.RemoveAndDeleteAll();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);

			GlbStaff pm1 = Factory.NewWithValidTestData<GlbStaff>();
			pm1.GS_Code = "PM1";
			pm1.GS_EmailAddress = "pm1@test.com";
			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "Module A", ProductAreaList.Codes.ARC, false);
			product.ModuleMappings.AddNew("BBB", "Module B", "", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			ProductAreaAssignmentCollection assignmentCollection = new ProductAreaAssignmentCollection();
			ProductAreaAssignment assignment1 = assignmentCollection.AddNew();
			assignment1.ProductArea = ProductAreaList.Codes.ARC;
			assignment1.Staff = pm1.GS_Code;
			EDIDataRegistry.Instance.ProductAreaAssignments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, assignmentCollection);

			incident.IM_Priority = "CR8";
			incident.ProductArea = "ARC";
			incident.IM_Module = "AAA";
			incident.Reopen(IncidentEventFactory.Codes.ERequestReopen);
			Factory.Save();
			AssertEquals("Should be reopened as defect", SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals("Should be reopened", SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("Should be product area assigned staff", "PM1", incident.IM_GS_NKAssignedToCurrent);

			var task = incident.WorkflowItems.AddNew();
			task.P9_Description = "Close Incident";
			task.P9_Sequence = 10;
			task.P9_GS_NKAssignedStaffMember = "";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();
			incident.Reopen(IncidentEventFactory.Codes.ERequestReopen);
			Factory.Save();
			AssertEquals("Should be reopened as defect", SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals("Should be reopened", SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals("Should be product area assigned staff", "PM1", incident.IM_GS_NKAssignedToCurrent);
			AssertEquals("Should be product area assigned staff", "PM1", incident.WorkflowItems[1].P9_GS_NKAssignedStaffMember);

			incident.WorkflowItems.RemoveAndDeleteAll();
			incident.IM_GS_NKAssignedToCurrent = "";
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			Factory.Save();
			incident.ProductArea = "";
			incident.IM_Module = "BBB";
			incident.Reopen(IncidentEventFactory.Codes.ERequestReopen);
			Factory.Save();
			AssertEquals("Should be reopened as defect", SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals("Should be reopened", SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals("Should be left opened with no assigned staff", "", incident.IM_GS_NKAssignedToCurrent);
		}

		#endregion

		#endregion

		#region Feature

		#region Return to Support

		public void TestReturnFeatureRequestToSupport()
		{
			GlbStaff otherStaff = Factory.NewWithValidTestData<GlbStaff>();

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = GlbStaff.CurrentUser.GS_Code;

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "New feature request");

			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);

			AssertEquals("Open", incident.IM_StatusDescription);
			AssertEquals("Added Awaiting Assignment", incident.IM_ResolutionCodeDescription);

			incident.IM_GS_NKAssignedToCurrent = otherStaff.GS_Code;

			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "Not a valid feature request");

			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, incident.IM_GS_NKCustServiceContact);

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "New feature request"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "Not a valid feature request"));
		}

		public void TestReturnFeatureRequestToSupport_NoCustServiceContact()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = User.ServiceUserCode;
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "New feature request");
			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "Not a valid feature request");

			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);
			AssertEquals(ZString.Empty, incident.IM_GS_NKCustServiceContact);
		}

		#endregion

		#region Pass to Defect Management

		public void TestPassToDefectManagement()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "New feature request");

			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);

			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "It is a defect");

			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "New feature request"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "It is a defect"));
		}

		#endregion

		#region Close

		public void TestCloseFeatureRequest()
		{
			GlbGroup invoicingAndLicensingGroup = Factory.NewWithValidTestData<GlbGroup>();
			invoicingAndLicensingGroup.GG_Code = "INVLIC";

			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_OH_Client = templateOrg.PK;
			template1.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;

			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "FQP Quotation";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Type = "CAN";
			task11.P9_Description = "Invoice Raised - Cancellation Fee";
			task11.P9_GS_NKAssignedStaffMember = "";
			task11.P9_GG_AssignedGroup = invoicingAndLicensingGroup.PK;
			task11.P9_EstDuration = TaskDurationCalculator.GetDurationFromTimeSpan(new TimeSpan(1, 0, 0));
			task11.P9_EstimateVariationFactor = 3M;

			var bucketComponent = bmTestHelper.CreateBucket(system, "Entry to BMS", 1);
			var bufferComponent = bmTestHelper.CreateBuffer(system, "Buffer", 10, sequence: 2);
			bmTestHelper.LinkComponents(bucketComponent, bufferComponent);

			Factory.Save();

			GlbStaff otherStaff = Factory.New<GlbStaff>();

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");

			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, "Dont know!");

			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, incident.IM_ClosureResolution);
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Closed As Upgrade Delivered"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "Dont know!"));

			ZDateTime beforeQuoteProvided = ZDateTime.Now;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided, "Haha!");

			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Haha!"));
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
		}

		public void TestCancelFeatureRequest()
		{
			SupportIncident featureRequest = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest.SetupForProjectFeatureRequest();

			SupportIncidentProcessTask task1 = featureRequest.WorkflowItems.AddNew();
			task1.FillWithValidTestData();
			SupportIncidentProcessTask task2 = featureRequest.WorkflowItems.AddNew();
			task2.FillWithValidTestData();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			featureRequest.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, "Testing");

			Factory.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, featureRequest.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, featureRequest.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, featureRequest.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, featureRequest.IM_ClosureResolution);
		}

		#endregion

		#region Close Project Related Feature Request

		public void TestCloseFeatureRequest_ProjectRelated()
		{
			var incident = Factory.New<SupportIncident>();
			incident.SetupForProjectFeatureRequest();

			Assert(incident.IsProjectRelatedIncident);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "Dont know!");

			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Closed As Completed"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "Dont know!"));

			incident = Factory.New<SupportIncident>();
			incident.SetupForProjectFeatureRequest();

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			var task2 = incident.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var task3 = incident.WorkflowItems.AddNew();
			task3.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Assert(incident.IsProjectRelatedIncident);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertContains(SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, "Dont know either!");

			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, incident.IM_ClosureResolution);
			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Closed As Cancelled"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "Dont know either!"));
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task3.P9_Status);
		}

		#endregion

		#region Reopen Feature

		public void TestReopen_FeatureRequest()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Description = "Close Incident";
			task1.P9_Sequence = 10;
			task1.P9_GS_NKAssignedStaffMember = "SCW";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var task2 = incident.WorkflowItems.AddNew();
			task2.P9_Description = "Notifiy Client";
			task2.P9_Sequence = 20;
			task2.P9_GS_NKAssignedStaffMember = "TST";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "SCW";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "TST";

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "DDD";

			Factory.Save();

			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);

			incident.Reopen(IncidentEventFactory.Codes.ERequestReopen);
			Factory.Save();
			AssertEquals("Should be reopened as feature request", SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals("Should be reopened", SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("New task is added", 3, incident.WorkflowItems.Count);

			var task3 = incident.WorkflowItems[2];
			AssertEquals("Last closed task is cloned", "Close Incident", task3.P9_Description);
			AssertEquals("Last closed task is cloned", "SCW", task3.P9_GS_NKAssignedStaffMember);
			AssertEquals("Last closed task is cloned", 21, task3.P9_Sequence);
			AssertEquals("Last closed task is cloned", ProcessTaskStatusCodeList.Codes.Assigned, task3.P9_Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task3.P9_Description = "Cancel Incident";
			task3.P9_GS_NKAssignedStaffMember = "DDD";
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);

			incident.Reopen(IncidentEventFactory.Codes.ERequestReopen);
			AssertEquals("Should be reopened as feature request", SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals("Should be reopened", SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("New task is added", 4, incident.WorkflowItems.Count);
			AssertEquals("Current assignee", "DDD", incident.IM_GS_NKAssignedToCurrent);

			var task4 = incident.WorkflowItems[3];
			AssertEquals("Last closed task is cloned", "Cancel Incident", task4.P9_Description);
			AssertEquals("Last closed task is cloned", "DDD", task4.P9_GS_NKAssignedStaffMember);
			AssertEquals("Last closed task is cloned", 22, task4.P9_Sequence);
			AssertEquals("Last closed task is cloned", ProcessTaskStatusCodeList.Codes.Assigned, task4.P9_Status);
		}

		public void TestReopen_FeatureRequest_ProductAreaAssignedStaff()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");

			incident.WorkflowItems.RemoveAndDeleteAll();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			Factory.Save();
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);

			GlbStaff pm1 = Factory.NewWithValidTestData<GlbStaff>();
			pm1.GS_Code = "PM1";
			pm1.GS_EmailAddress = "pm1@test.com";
			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "Module A", ProductAreaList.Codes.ARC, false);
			product.ModuleMappings.AddNew("BBB", "Module B", "", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			ProductAreaAssignmentCollection assignmentCollection = new ProductAreaAssignmentCollection();
			ProductAreaAssignment assignment1 = assignmentCollection.AddNew();
			assignment1.ProductArea = ProductAreaList.Codes.ARC;
			assignment1.Staff = pm1.GS_Code;
			EDIDataRegistry.Instance.ProductAreaAssignments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, assignmentCollection);

			incident.IM_Priority = "CR8";
			incident.ProductArea = "ARC";
			incident.IM_Module = "AAA";
			incident.Reopen(IncidentEventFactory.Codes.ERequestReopen);
			Factory.Save();
			AssertEquals("Should be reopened as feature request", SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals("Should be reopened", SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("Should be product area assigned staff", "PM1", incident.IM_GS_NKAssignedToCurrent);

			var task = incident.WorkflowItems.AddNew();
			task.P9_Description = "Close Incident";
			task.P9_Sequence = 10;
			task.P9_GS_NKAssignedStaffMember = "";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();
			incident.Reopen(IncidentEventFactory.Codes.ERequestReopen);
			Factory.Save();
			AssertEquals("Should be reopened as feature request", SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals("Should be reopened", SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals("Should be product area assigned staff", "PM1", incident.IM_GS_NKAssignedToCurrent);
			AssertEquals("Should be product area assigned staff", "PM1", incident.WorkflowItems[1].P9_GS_NKAssignedStaffMember);

			incident.WorkflowItems.RemoveAndDeleteAll();
			incident.IM_GS_NKAssignedToCurrent = "";
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			Factory.Save();
			incident.ProductArea = "";
			incident.IM_Module = "BBB";
			incident.Reopen(IncidentEventFactory.Codes.ERequestReopen);
			Factory.Save();
			AssertEquals("Should be reopened as feature request", SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals("Should be reopened", SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals("Should be left opened with no assigned staff", "", incident.IM_GS_NKAssignedToCurrent);
		}

		#endregion

		#endregion

		#region Reopen & Close

		public void TestReopenThenClose_ShouldAddSTCEventForStatusAndDisposition()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			Factory.Save();

			ZQuery statusQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Equal, "Status -  to CLS");
			var statusCount = incident.Logs.Find(statusQuery).Length;
			AssertEquals("The count of STC Event For 'Status -  to CLS' should be 1", 1, statusCount);
			ZQuery dispositionQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Equal, "Disposition -  to COM");
			var dispositionCount = incident.Logs.Find(dispositionQuery).Length;
			AssertEquals("The count of STC Event For 'Disposition -  to COM' should be 1", 1, dispositionCount);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();

			statusCount = incident.Logs.Find(statusQuery).Length;
			dispositionQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Equal, "Disposition - COM to CWR");
			dispositionCount = incident.Logs.Find(dispositionQuery).Length;
			AssertEquals("The count of STC Event For 'Status -  to CLS' should be 1", 1, statusCount);
			AssertEquals("The count of STC Event For 'Disposition - COM to CWR' should be 1", 1, dispositionCount);
		}

		#endregion

		#endregion

		#region General

		#region Close

		public void TestCloseIncident()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, "");
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);

			incident = Factory.New<SupportIncident>();
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, "");
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
		}

		#endregion

		#region Wait for Upgrade

		public void TestWaitForUpgrade()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			LicenceEnterprise enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;
			LicenceCompany company = Factory.NewWithValidTestData<LicenceCompany>();
			company.LC_LE = enterprise.PK;
			company.LC_OH = org.PK;
			LicenceDatabase database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_LE = enterprise.PK;
			LicenceHeader header = Factory.NewWithValidTestData<LicenceHeader>();
			header.LA_LC = company.PK;
			header.LA_LD = database.PK;
			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = company.LC_CompanyCode;
			clientCompany.LCC_LD = database.PK;
			ClientCompany.LCC_OH = org.PK;

			Factory.Save();

			InternalIncidentLicenceSettings settings = new InternalIncidentLicenceSettings();
			LicenceEnterpriseKey key = new LicenceEnterpriseKey();
			key.LE_PK = enterprise.PK;
			settings.LicenceEnterpriseKeys.Add(key);
			settings.EdiProd_LicencePK = header.PK;
			settings.UAT_ALP_LicencePK = header.PK;
			settings.UAT_DPR_LicencePK = header.PK;
			settings.UAT_GPC_LicencePK = header.PK;
			settings.UAT_GPR_LicencePK = header.PK;
			settings.UAT_STD_LicencePK = header.PK;
			EDIDataRegistry.Instance.InternalIncidentLicenceSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Source = "";
			incident.IM_LCC = clientCompany.PK;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, SupportIncident.InternalIncidentComment);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);
			incident.WaitForUpgrade();
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			incident.IM_LCC = ZGuid.Empty;
			incident.WaitForUpgrade();
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);

			incident = Factory.New<SupportIncident>();
			incident.IM_Source = "";
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);
			incident.WaitForUpgrade();
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);

			incident = Factory.New<SupportIncident>();
			incident.IM_Source = "";
			incident.IM_LCC = clientCompany.PK;
			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncident.InternalFeatureRequestComment);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			incident.WaitForUpgrade();
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ResolutionCode);

			incident = Factory.New<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.ComplianceRequirement;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			incident.WaitForUpgrade();
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);

			incident = Factory.New<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			incident.WaitForUpgrade();
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);

			incident = Factory.New<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			incident.WaitForUpgrade();
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);
		}

		#endregion

		#region Set Upgrade Delivered

		[TestDate(2024, 10, 8)]
		public void TestSetUpgradeDelivered()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.SetUpgradeDelivered();
			Factory.Save();
			var expectedResolvedTime = ZDateTime.UtcNow;
			var expectedResolvedReferences = new Dictionary<string, string>();
			expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered);
			expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, "Incident resolved with method UPD - Upgrade Delivered");

			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(expectedResolvedTime, incident.IM_ResolveTimeUtc);

			var incidentResolvedLog = incident.Logs.GetAllLogs().Cast<StmALog>().Last(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);
			AssertReference(expectedResolvedReferences, incidentResolvedLog.SL_Reference);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			TestDateAttribute.AddDays(2);
			expectedResolvedTime = expectedResolvedTime.AddDays(2);
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);

			incident.SetUpgradeDelivered();
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(expectedResolvedTime, incident.IM_ResolveTimeUtc);

			incidentResolvedLog = incident.Logs.GetAllLogs().Cast<StmALog>().Last(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);
			AssertEquals(expectedResolvedTime, incidentResolvedLog.SL_EventTime);
			AssertReference(expectedResolvedReferences, incidentResolvedLog.SL_Reference);

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			TestDateAttribute.AddDays(2);
			expectedResolvedTime = expectedResolvedTime.AddDays(2);

			incident.SetUpgradeDelivered();
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(expectedResolvedTime, incident.IM_ResolveTimeUtc);

			incidentResolvedLog = incident.Logs.GetAllLogs().Cast<StmALog>().Last(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);
			AssertEquals(expectedResolvedTime, incidentResolvedLog.SL_EventTime);
			AssertReference(expectedResolvedReferences, incidentResolvedLog.SL_Reference);
		}

		[TestDate(2024, 10, 8)]
		public void TestSetUpgradeDelivered_WhenIsResolutionTrue()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var updDispositions = (IncidentClosureDisposition)registryValue.Cast<CodeDescriptionBoolTreeNode>().FirstOrDefault(d => d.Code == SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered && d.ParentID == supportParent.ID);

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.SetUpgradeDelivered();
			var expectedResolvedTime = ZDateTime.UtcNow;
			var expectedResolvedReferences = new Dictionary<string, string>();
			expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered);
			expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, "Incident resolved with method UPD - Upgrade Delivered");

			var incidentResolvedLog = incident.Logs.GetAllLogs().Cast<StmALog>().Last(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);

			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, incident.IM_ClosureResolution);
			AssertEquals(expectedResolvedTime, incident.IM_ResolveTimeUtc);
			AssertEquals(expectedResolvedTime, incidentResolvedLog.SL_EventTime);
			AssertReference(expectedResolvedReferences, incidentResolvedLog.SL_Reference);

			updDispositions.IsResolution = true;
			Factory.ClearCachedValue<CodeDescriptionPairList>("SupportIncidentLookups.GetClosureDispositionList:SUP:::X");
			TestDateAttribute.AddDays(2);
			expectedResolvedTime = expectedResolvedTime.AddDays(2);
			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				incident.SetUpgradeDelivered();

				incidentResolvedLog = incident.Logs.GetAllLogs().Cast<StmALog>().Last(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);

				AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
				AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, incident.IM_ResolutionCode);
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, incident.IM_ClosureResolution);
				AssertEquals(expectedResolvedTime, incident.IM_ResolveTimeUtc);
				AssertEquals(expectedResolvedTime, incidentResolvedLog.SL_EventTime);
				AssertReference(expectedResolvedReferences, incidentResolvedLog.SL_Reference);
			}
		}

		[TestDate(2024, 10, 8)]
		public void TestSetUpgradeDeliveredDoesNotCloseTasks()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.SetupForNewCreatedFeatureRequest();
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			ProcessTask task1 = incident.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			ProcessTask task2 = incident.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			incident.SetUpgradeDelivered();
			Factory.Save();

			var expectedResolvedReferences = new Dictionary<string, string>();
			expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered);
			expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, "Incident resolved with method UPD - Upgrade Delivered");
			var expectedResolvedTime = ZDateTime.UtcNow;

			var incidentResolvedLog = incident.Logs.GetAllLogs().Cast<StmALog>().Last(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);

			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
			AssertEquals(expectedResolvedTime, incident.IM_ResolveTimeUtc);
			AssertEquals(expectedResolvedTime, incidentResolvedLog.SL_EventTime);
			AssertReference(expectedResolvedReferences, incidentResolvedLog.SL_Reference);
		}

		#endregion

		#region Set Work Item Created

		public void TestSetWorkItemCreated()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);

			incident.SetWorkItemCreated();
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			incident.SetWorkItemCreated();
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);

			incident.Escalate(SupportIncidentCategoriesList.Codes.ComplianceRequirement, "");
			incident.SetWorkItemCreated();
			AssertEquals(SupportIncidentCategoriesList.Codes.ComplianceRequirement, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);

			incident.Escalate(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, "");
			incident.SetWorkItemCreated();
			AssertEquals(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);
		}

		#endregion

		#endregion

		#region Properties

		#region IM_RN_NKCountry

		public void TestSettingLicenceDefaultsCountry()
		{
			var database = Factory.NewWithValidTestData<LicenceDatabase>();

			var auClientCompany = Factory.New<ClientCompany>();
			auClientCompany.LCC_LD = database.PK;
			auClientCompany.LCC_Code = "AUC";
			auClientCompany.LCC_RN_NKCountryCode = "AU";

			var usClientCompany = Factory.New<ClientCompany>();
			usClientCompany.LCC_LD = database.PK;
			usClientCompany.LCC_Code = "USC";
			usClientCompany.LCC_RN_NKCountryCode = "US";

			var incident = Factory.New<SupportIncident>();
			AssertEquals("Precondition", "", incident.IM_RN_NKCountry);

			incident.IM_LCC = auClientCompany.PK;
			AssertEquals("AU", incident.IM_RN_NKCountry);

			incident.IM_LCC = usClientCompany.PK;
			AssertEquals("Do not default country if already populated", "AU", incident.IM_RN_NKCountry);

			incident.IM_RN_NKCountry = "";
			incident.IM_LCC = ZGuid.Empty;
			incident.IM_LCC = usClientCompany.PK;
			AssertEquals("US", incident.IM_RN_NKCountry);
		}

		#endregion

		#region IM_LD

		public void TestIM_LD()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = parentOrg.PK;

			var database1 = Factory.New<LicenceDatabase>();
			database1.LD_ServerCode = "HST";
			database1.LD_LE = enterprise.PK;

			var clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_LD = database1.PK;
			clientCompany1.LCC_Code = "AAA";
			clientCompany1.LCC_OH = org1.PK;

			var clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_LD = database1.PK;
			clientCompany2.LCC_Code = "BBB";
			clientCompany2.LCC_OH = org2.PK;

			Factory.Save();

			var incident = Factory.New<SupportIncident>();
			incident.IM_LD = database1.PK;
			AssertEquals("DB company does not default when client is not set", ZGuid.Empty, incident.IM_LCC);

			incident.IM_OH_Client = org1.PK;
			incident.IM_LD = ZGuid.Empty;
			incident.IM_LCC = ZGuid.Empty;

			incident.IM_LD = database1.PK;
			AssertEquals("DB company defaults when client and db are set", clientCompany1.PK, incident.IM_LCC);

			incident.IM_OH_Client = org2.PK;
			incident.IM_LD = ZGuid.Empty;
			incident.IM_LCC = ZGuid.Empty;

			incident.IM_LD = database1.PK;
			AssertEquals("DB company defaults when client and db are set", clientCompany2.PK, incident.IM_LCC);

			var database2 = Factory.New<LicenceDatabase>();
			database2.LD_ServerCode = "TLX";
			database2.LD_LE = enterprise.PK;
			database2.LD_Product = "SPH";
			incident.IM_LD = database2.PK;
			AssertEquals("DB company is cleared when db product is not ENT/CW1", ZGuid.Empty, incident.IM_LCC);

			var database3 = Factory.New<LicenceDatabase>();
			database3.LD_ServerCode = "SYD";
			database3.LD_LE = enterprise.PK;
			database3.LD_OH_WebAccessOrg = org3.PK;

			incident.IM_LD = database3.PK;
			AssertEquals("Should set master org as client when database changes", org3.PK, incident.IM_OH_Client);
		}

		public void TestDatabaseCodeCompanyCodeReadOnly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "HST";
			database.LD_LE = enterprise.PK;

			var build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.FirstReleaseCSBiDirectionMessage);
			database.LD_HL_CurrentRunningVersion = build.PK;

			var clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_LD = database.PK;
			clientCompany1.LCC_Code = "AAA";
			clientCompany1.LCC_OH = org.PK;

			var clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_LD = database.PK;
			clientCompany2.LCC_Code = "BBB";

			Factory.Save();

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_LD = database.PK;
			incident1.IM_LCC = clientCompany1.PK;
			AssertEquals(false, incident1.DatabaseServerCode_ReadOnly);
			AssertEquals(false, incident1.ClientCompanyCode_ReadOnly);
			incident1.IM_ClientIncidentReference = "SR00003483";
			Factory.Save();
			AssertEquals(true, incident1.DatabaseServerCode_ReadOnly);
			AssertEquals(true, incident1.ClientCompanyCode_ReadOnly);

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			AssertEquals(false, incident2.DatabaseServerCode_ReadOnly);
			AssertEquals(false, incident2.ClientCompanyCode_ReadOnly);
			incident2.IM_LD = database.PK;
			incident2.IM_LCC = clientCompany1.PK;
			AssertEquals(false, incident2.DatabaseServerCode_ReadOnly);
			AssertEquals(false, incident2.ClientCompanyCode_ReadOnly);
			Factory.Save();
			AssertEquals(true, incident2.DatabaseServerCode_ReadOnly);
			AssertEquals(true, incident2.ClientCompanyCode_ReadOnly);

			database.LD_HL_CurrentRunningVersion = ZGuid.Empty;
			Factory.Save();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_LD = database.PK;
			incident3.IM_LCC = clientCompany1.PK;
			Factory.Save();
			AssertEquals(false, incident3.DatabaseServerCode_ReadOnly);
			AssertEquals(false, incident3.ClientCompanyCode_ReadOnly);

			database.LD_HL_CurrentRunningVersion = build.PK;
			clientCompany1.LCC_DeactivateTimeUtc = ZDateTime.Today.AddMonths(-6);
			Factory.Save();
			var loadedIncident2 = new BusinessObjectFactory().Load<SupportIncident>(incident2.PK);
			AssertEquals(true, loadedIncident2.DatabaseServerCode_ReadOnly);
			AssertEquals(false, loadedIncident2.ClientCompanyCode_ReadOnly);
			loadedIncident2.IM_LCC = clientCompany2.PK;
			loadedIncident2.ClientCompanyCode = clientCompany2.LCC_Code;
			AssertEquals(false, loadedIncident2.ClientCompanyCode_ReadOnly);
			loadedIncident2.Factory.Save();
			AssertEquals(true, loadedIncident2.ClientCompanyCode_ReadOnly);
		}

		#endregion

		#region SourceModule

		public void TestIsSourceModuleOverriden()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			AssertEquals(false, incident.IsSourceModuleOverriden);

			incident.IM_SourceModuleId = "DummySourceModule";
			AssertEquals("Can not 'override' if incident hasn't been saved yet", false, incident.IsSourceModuleOverriden);
			Factory.Save();
			AssertEquals(false, Factory.Load<SupportIncident>(incident.PK).IsSourceModuleOverriden);

			incident.IM_SourceModuleId = "DummySourceModule2";
			AssertEquals("Should now be overriden", true, incident.IsSourceModuleOverriden);
			Factory.Save();
			AssertEquals(true, Factory.Load<SupportIncident>(incident.PK).IsSourceModuleOverriden);
		}

		public void TestSourceModuleWithPath()
		{
			var sourceModules = new SourceModuleCollection();
			sourceModules.AddNew("Dummy1", "Dummy 1", "Dummies >", ModuleListType.MenuSection, "DUM", true, true, "ENT");
			sourceModules.AddNew("COR", "ediCore", "[Licence]", ModuleListType.MenuSection, "", false, true, "ENT");
			EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;

			incident.IM_SourceModuleId = "";
			AssertEquals("<Not Available>", incident.SourceModuleWithPath);

			incident.IM_SourceModuleId = IncidentApproval.NotAvailableActiveModuleID;
			AssertEquals("<Not Available>", incident.SourceModuleWithPath);

			incident.IM_SourceModuleId = ModuleListBuilder.Codes.All;
			AssertEquals(ModuleListBuilder.Descriptions.All, incident.SourceModuleWithPath);

			incident.IM_SourceModuleId = "Dummy1";
			AssertEquals("Dummies > Dummy 1", incident.SourceModuleWithPath);

			incident.IM_SourceModuleId = "COR";
			AssertEquals("[Licence] ediCore", incident.SourceModuleWithPath);

			incident.IM_Product = "XXX";
			AssertEquals("COR", incident.SourceModuleWithPath);
		}

		public void TestIncidentTriageDescription()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IMT_Triage = Guid.Empty;
			Factory.Save();

			AssertEquals("Not Applied", incident.TriageDescription);

			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_SupportDescription = "Test Triage";
			triage.IMT_TriageNumber = "TRI0001";
			incident.IM_IMT_Triage = triage.PK;
			Factory.Save();

			AssertEquals("TRI0001 - Test Triage", incident.TriageDescription);
		}

		public void TestIncidentTriageSupportDescription()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IMT_Triage = Guid.Empty;
			Factory.Save();

			AssertEquals("", incident.TriageSupportDescription);

			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_SupportDescription = "Test Triage";
			incident.IM_IMT_Triage = triage.PK;
			Factory.Save();

			AssertEquals("Test Triage", incident.TriageSupportDescription);
		}

		public void TestIncidentTriageNumber()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IMT_Triage = Guid.Empty;
			Factory.Save();

			AssertEquals("", incident.TriageNumber);

			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_TriageNumber = "TRI0001";
			incident.IM_IMT_Triage = triage.PK;
			Factory.Save();

			AssertEquals("TRI0001", incident.TriageNumber);
		}

		public void TestLogMenuItem()
		{
			var sourceModules = new SourceModuleCollection();
			sourceModules.AddNew("Dummy1", "Dummy 1", "Dummies >", ModuleListType.MenuSection, "DUM", true, true, "ENT");
			sourceModules.AddNew("COR", "ediCore", "[Licence]", ModuleListType.MenuSection, "", false, true, "ENT");
			EDIDataRegistry.Instance.SourceModules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, sourceModules);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			Factory.Save();

			incident.IM_SourceModuleId = "Dummy1";
			Factory.Save();

			AssertEquals("Dummies > Dummy 1", incident.SourceModuleWithPath);
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Menu Item changed from [<Not Available>] to [Dummies > Dummy 1]"));
		}

		#endregion

		public void TestProductAreaClearedWhenChangedToDifferentModuleType()
		{
			var incident = Factory.New<SupportIncident>();

			CombineAssertions(() =>
			{
				AssertProductAreaIsCleared(false, Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround, string.Empty);
				AssertProductAreaIsCleared(false, Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround, Core.Constants.CustomerService.CriticalityCodes.CR5_Training);
				AssertProductAreaIsCleared(true, Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround, Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement);
				AssertProductAreaIsCleared(true, Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround, Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest);

				AssertProductAreaIsCleared(false, Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement, string.Empty);
				AssertProductAreaIsCleared(true, Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement, Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround);
				AssertProductAreaIsCleared(true, Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement, Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest);

				AssertProductAreaIsCleared(false, Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest, string.Empty);
				AssertProductAreaIsCleared(true, Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest, Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround);
				AssertProductAreaIsCleared(true, Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest, Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement);

				AssertProductAreaIsCleared(false, string.Empty, Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround);
				AssertProductAreaIsCleared(false, string.Empty, Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement);
				AssertProductAreaIsCleared(false, string.Empty, Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest);
			});
		}

		void AssertProductAreaIsCleared(bool expectedIsCleared, string beforeCriticality, string afterCriticalty)
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = beforeCriticality;
			incident.ProductArea = "XXX";
			incident.IM_Priority = afterCriticalty;

			var actualIsCleared = incident.ProductArea.IsEmpty;
			AssertEquals("ProductArea Cleared?\r\nCriticaly Before:" + beforeCriticality + " After:" + afterCriticalty, expectedIsCleared, actualIsCleared);
		}

		#region IM_Priority (Criticality)

		public void TestSetCriticalityWithoutLoggingReasonEConversation()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			Factory.Save();

			incident.SetCriticalityWithoutLoggingReason(Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown);
			Factory.Save();
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Criticality changed from CR1 to CR2"));

			incident.SetCriticalityWithoutLoggingReason(Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround);
			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("EConversation should not say criticality changed from CR2 when it hasn't been saved", false, messageList.Any(msg => msg.Body == "Criticality changed from CR2 to CR3"));
			Assert("PreviouslySavedCriticality should not be updated before saving", incident.Criticality != incident.PreviouslySavedCriticality);

			Factory.Save();
			Assert("PreviouslySavedCriticality should be updated after saving", incident.Criticality == incident.PreviouslySavedCriticality);
		}

		public void TestPriorityDisplayedOnClientSide()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "ENT";
			enterprise.LE_OH = org.PK;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			Factory.Save();

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			AssertEquals("Precondition", true, incident.ClientSupportsCr8Cr9);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			AssertEquals(Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, incident.PriorityDisplayedOnClientSide);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR5_Training;
			AssertEquals(Core.Constants.CustomerService.CriticalityCodes.CR5_Training, incident.PriorityDisplayedOnClientSide);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			AssertEquals(Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement, incident.PriorityDisplayedOnClientSide);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			AssertEquals(Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest, incident.PriorityDisplayedOnClientSide);

			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;

			Factory.Save();
			AssertEquals("Precondition", false, incident.ClientSupportsCr8Cr9);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			AssertEquals(Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown, incident.PriorityDisplayedOnClientSide);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR5_Training;
			AssertEquals(Core.Constants.CustomerService.CriticalityCodes.CR5_Training, incident.PriorityDisplayedOnClientSide);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			AssertEquals("Should be CR5 as client does not support CR8", Core.Constants.CustomerService.CriticalityCodes.CR5_Training, incident.PriorityDisplayedOnClientSide);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			AssertEquals("Should be CR5 as client does not support CR9", Core.Constants.CustomerService.CriticalityCodes.CR5_Training, incident.PriorityDisplayedOnClientSide);
		}

		public void TestChangeCriticality_IncidentEvent()
		{
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_OH_Client = templateOrg.PK;
			template1.P0_SubType2 = SupportIncidentCategoriesList.Codes.Support;
			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "CCR Support";

			var task1 = template1.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = header1.PK;
			task1.P9_Sequence = 10;
			task1.P9_Type = "INV";
			task1.P9_Description = "CR5 Only Task";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task1.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task1.TemplateConditions.TemplateCondition2Value = @"""<IM_Priority>"" == ""CR5"" && ""<PreviouslySavedCriticality>"" == ""CR4""";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = "CR4";
			var existingTask1 = incident.WorkflowItems.AddNew();
			existingTask1.P9_Sequence = 10;
			existingTask1.P9_GS_NKAssignedStaffMember = "AAA";
			existingTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var existingTask2 = incident.WorkflowItems.AddNew();
			existingTask2.P9_Sequence = 20;
			existingTask2.P9_GS_NKAssignedStaffMember = "BBB";
			existingTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			incident.IM_Priority = "CR4";
			incident.TriggerCriticalityChangeEvent();
			AssertEquals("No new tasks", 2, incident.WorkflowItems.Count);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, incident.WorkflowItems[0].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, incident.WorkflowItems[1].P9_Status);

			incident.IM_Priority = "CR5";
			incident.TriggerCriticalityChangeEvent();
			AssertEquals("New task has been added", 3, incident.WorkflowItems.Count);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, incident.WorkflowItems[0].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, incident.WorkflowItems[1].P9_Status);
			var newTask = incident.WorkflowItems[2];
			AssertEquals("CR5 Only Task", newTask.P9_Description);
			AssertEquals("Support", newTask.ProcessHeader.FH_CompletionStatement);
			AssertEquals(30, newTask.P9_Sequence);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, newTask.P9_Status);
			AssertEquals("", newTask.P9_GS_NKAssignedStaffMember);
		}

		public void TestChangeCriticality_ReCalculateDispostionIfChangeFromCr7()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var task = incident.WorkflowItems.AddNew();
			incident.IM_Priority = "CR7";
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate;

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Disposition should not be changed", SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate, incident.IM_ResolutionCode);

			incident.IM_Priority = "CR4";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals("Disposition should be re-calculated", SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);
		}

		public void TestChangeCriticality_TriggerEventIfChangeToCr7()
		{
			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_OH_Client = templateOrg.PK;
			template1.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;
			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "DER Dev Estimate";

			var task1 = template1.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = header1.PK;
			task1.P9_Sequence = 10;
			task1.P9_Type = "INV";
			task1.P9_Description = "Provide Estimate";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = "CR5";
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment;

			incident.IM_Priority = "CR7";
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate, incident.IM_ResolutionCode);
			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals("Provide Estimate", incident.WorkflowItems[0].P9_Description);
		}

		public void TestUnsavedIncidentSetDefaultStageOnCriticalityChange()
		{
			var incident = Factory.New<SupportIncident>();
			AssertEquals("", incident.IM_Priority);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown;
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR5_Training;
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			AssertEquals(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, incident.IM_Category);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);

			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR7_CustomisationRequest;
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);

			Factory.Save();
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
		}

		public void TestChangeCriticality_PreviouslySavedCriticality()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR1_SystemDown;
			Factory.Save();
			Assert("PreviouslySavedCriticality should not be blank", incident.PreviouslySavedCriticality != string.Empty);

			incident.SetCriticalityWithoutLoggingReason(Core.Constants.CustomerService.CriticalityCodes.CR2_ModuleDown);
			Factory.Save();
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Criticality changed from CR1 to CR2"));

			incident.SetCriticalityWithoutLoggingReason(Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround);
			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("EConversation should not say criticality changed from CR2 when it hasn't been saved", false, messageList.Any(msg => msg.Body == "Criticality changed from CR2 to CR3"));
			Assert("PreviouslySavedCriticality should not be updated before saving", incident.Criticality != incident.PreviouslySavedCriticality);

			Factory.Save();
			Assert("PreviouslySavedCriticality should be updated after saving", incident.Criticality == incident.PreviouslySavedCriticality);
		}

		#endregion

		public void TestWorkItemStatus()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			AssertEquals("", incident.WorkItemStatus);

			NewWorkItem workItem = incident.RelatedWorkItems.AddNew();
			AssertEquals("Open Pending Allocation", incident.WorkItemStatus);

			WorkItemProcessTask task = workItem.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals("Working - Suspended (Temporary Pause)", incident.WorkItemStatus);

			NewWorkItem workItem2 = incident.RelatedWorkItems.AddNew();
			WorkItemProcessTask task2 = workItem2.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals("Working - Working", incident.WorkItemStatus);
		}

		public void TestWorkItemAssignedTo()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "SCW";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "AAC";

			SupportIncident incident = Factory.New<SupportIncident>();
			AssertEquals("", incident.WorkItemAssignedTo);

			NewWorkItem workItem1 = incident.RelatedWorkItems.AddNew();
			WorkItemProcessTask task1 = workItem1.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_GS_NKAssignedStaffMember = "SCW";
			AssertEquals("SCW", incident.WorkItemAssignedTo);

			NewWorkItem workItem2 = incident.RelatedWorkItems.AddNew();
			AssertEquals("SCW", incident.WorkItemAssignedTo);

			WorkItemProcessTask task2 = workItem2.WorkflowItems.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task2.P9_GS_NKAssignedStaffMember = "AAC";
			AssertEquals("SCW, AAC", incident.WorkItemAssignedTo);
		}

		public void TestOverallAssignedTo()
		{
			GlbStaff supportStaff = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff devStaff = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff featureStaff = Factory.NewWithValidTestData<GlbStaff>();

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = supportStaff.GS_Code;

			AssertEquals("Assigned To:", incident.OverallAssignedToLabelText);
			AssertEquals(supportStaff.GS_Code, incident.OverallAssignedToCode);
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.AssignToStaff(devStaff, "");

			AssertEquals("Assigned To:", incident.OverallAssignedToLabelText);
			AssertEquals(devStaff.GS_Code, incident.OverallAssignedToCode);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			AssertEquals("Assigned To:", incident.OverallAssignedToLabelText);
			AssertEquals(supportStaff.GS_Code, incident.OverallAssignedToCode);

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			incident.AssignToStaff(featureStaff, "");

			AssertEquals("Assigned To:", incident.OverallAssignedToLabelText);
			AssertEquals(featureStaff.GS_Code, incident.OverallAssignedToCode);
		}

		public void TestOverallAssignedTo_ClosedIncident()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			staff1.GS_FullName = "Support Staff";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ST2";
			staff2.GS_FullName = "Dev Staff";

			SupportIncident incident = Factory.New<SupportIncident>();
			AssertEquals("Assigned To:", incident.OverallAssignedToLabelText);
			AssertEquals("", incident.OverallAssignedToCode);
			AssertEquals("", incident.OverallAssignedToDescription);

			incident.IM_GS_NKCustServiceContact = "ST2";
			AssertEquals("Assigned To:", incident.OverallAssignedToLabelText);
			AssertEquals("ST2", incident.OverallAssignedToCode);
			AssertEquals("Dev Staff", incident.OverallAssignedToDescription);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			AssertEquals("Incident Closed By:", incident.OverallAssignedToLabelText);
			AssertEquals("ST2", incident.OverallAssignedToCode);
			AssertEquals("Dev Staff", incident.OverallAssignedToDescription);

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Sequence = 10;
			task1.P9_GS_NKAssignedStaffMember = "ST1";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			AssertEquals("Last Task Closed By:", incident.OverallAssignedToLabelText);
			AssertEquals("ST1", incident.OverallAssignedToCode);
			AssertEquals("Support Staff", incident.OverallAssignedToDescription);

			var task2 = incident.WorkflowItems.AddNew();
			task2.P9_Sequence = 20;
			task2.P9_GS_NKAssignedStaffMember = "ST2";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			AssertEquals("Last Task Closed By:", incident.OverallAssignedToLabelText);
			AssertEquals("ST2", incident.OverallAssignedToCode);
			AssertEquals("Dev Staff", incident.OverallAssignedToDescription);

			var task3 = incident.WorkflowItems.AddNew();
			task3.P9_Sequence = 30;
			task3.P9_GS_NKAssignedStaffMember = "ST1";
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			AssertEquals("Last Task Closed By:", incident.OverallAssignedToLabelText);
			AssertEquals("ST2", incident.OverallAssignedToCode);
			AssertEquals("Dev Staff", incident.OverallAssignedToDescription);

			var task4 = incident.WorkflowItems.AddNew();
			task4.P9_Sequence = 15;
			task4.P9_GS_NKAssignedStaffMember = "ST1";
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
			AssertEquals("Last Task Closed By:", incident.OverallAssignedToLabelText);
			AssertEquals("ST1", incident.OverallAssignedToCode);
			AssertEquals("Support Staff", incident.OverallAssignedToDescription);
		}

		public void TestOverallAssignedTo_WithCapability()
		{
			GlbStaff supportStaff = Factory.NewWithValidTestData<GlbStaff>();
			supportStaff.GS_Code = "ST1";
			supportStaff.GS_FullName = "Support Staff";
			GlbStaff devStaff = Factory.NewWithValidTestData<GlbStaff>();
			devStaff.GS_Code = "ST2";
			devStaff.GS_FullName = "Dev Staff";

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = supportStaff.GS_Code;
			AssertEquals("Assigned To:", incident.OverallAssignedToLabelText);
			AssertEquals("ST1", incident.OverallAssignedToCode);
			AssertEquals("Support Staff", incident.OverallAssignedToDescription);

			incident.IM_GS_NKAssignedToCurrent = devStaff.GS_Code;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			AssertEquals("Assigned To:", incident.OverallAssignedToLabelText);
			AssertEquals("ST2", incident.OverallAssignedToCode);
			AssertEquals("Dev Staff", incident.OverallAssignedToDescription);

			incident.IM_GS_NKCustServiceContact = "";
			incident.IM_GS_NKAssignedToCurrent = "";

			var task = incident.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "DEV";
			capability.G4_Description = "Developer Level 1";
			task.P9_G4_RequiredCapability = capability.PK;
			AssertEquals("Capability:", incident.OverallAssignedToLabelText);
			AssertEquals("DEV", incident.OverallAssignedToCode);
			AssertEquals("Developer Level 1", incident.OverallAssignedToDescription);
		}

		public void TestOverallAssignedTo_WithNullCapability()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = "";
			incident.IM_GS_NKAssignedToCurrent = "";

			var task = incident.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_G4_RequiredCapability = ZGuid.NewZGuid();

			string code = string.Empty;
			AssertNoExceptionThrown(delegate
			{
				code = incident.OverallAssignedToCode;
			});

			AssertEquals("", code);
		}

		public void TestOverallAssignedTo_WithStaff()
		{
			GlbStaff supportStaff = Factory.NewWithValidTestData<GlbStaff>();
			supportStaff.GS_Code = "ST1";
			supportStaff.GS_FullName = "Support Staff";
			GlbStaff devStaff = Factory.NewWithValidTestData<GlbStaff>();
			devStaff.GS_Code = "ST2";
			devStaff.GS_FullName = "Dev Staff";
			GlbStaff featureStaff = Factory.NewWithValidTestData<GlbStaff>();
			featureStaff.GS_Code = "ST3";
			featureStaff.GS_FullName = "Feature Staff";

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = supportStaff.GS_Code;

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_GS_NKAssignedStaffMember = devStaff.GS_Code;
			AssertEquals("Task Assigned:", incident.OverallAssignedToLabelText);
			AssertEquals("ST2", incident.OverallAssignedToCode);
			AssertEquals("Dev Staff", incident.OverallAssignedToDescription);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var task2 = incident.WorkflowItems.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_GS_NKAssignedStaffMember = featureStaff.GS_Code;
			AssertEquals("Task Assigned:", incident.OverallAssignedToLabelText);
			AssertEquals("ST3", incident.OverallAssignedToCode);
			AssertEquals("Feature Staff", incident.OverallAssignedToDescription);
		}

		public void TestStage_ClosedStatus()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			incident.AssignAndInvestigateInSupport(GlbStaff.CurrentUser);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.NoSupportContract, "");
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Other, "");
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.NoSupportContract, "");
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Other, "");
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Other, "");
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.NoSupportContract, "");
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
		}

		public void TestStage_ClosedStatusByLogPostedTime()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.AssignToStaff(GlbStaff.CurrentUser, "");
			Factory.Save();

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.AssignToStaff(GlbStaff.CurrentUser, "");
			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			Factory.Save();

			incident.AssignToStaff(GlbStaff.CurrentUser, "");
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Other, "");
			Factory.Save();

			ZQuery query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Equal, "Status - OPN to CLS");
			query.OrderBy = StmALogSchema.Constants.SL_EventTime + OrderByClause.Descending;
			StmALog log = incident.Logs.Find(query)[0];
			Thread.Sleep(1000);
			Factory.Save();

			SupportIncident loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, loadedIncident.IM_Category);
		}

		public void TestStatusDescription()
		{
			var incident = (SupportIncident)GetNewBusinessObject();
			incident.IM_Status = "";
			AssertEquals("", incident.StatusDescription);

			incident.IM_Status = "OPN";
			AssertEquals("Overall Status: Open - eRequest Status: Added Awaiting Assignment", incident.StatusDescription);

			incident.IM_Status = "XXX";
			AssertEquals("", incident.StatusDescription);
		}

		public void TestIM_ClientContractStatus()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "ENT";
			enterprise.LE_OH = client.PK;
			Factory.Save();

			LicenceCompany company = Factory.New<LicenceCompany>();
			company.LC_CompanyCode = "COM";
			company.LC_LE = enterprise.PK;
			company.LC_OH = client.PK;

			ReleaseBuild someBuild = Factory.New<ReleaseBuild>();
			someBuild.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = someBuild.PK;

			LicenceHeader licence = Factory.New<LicenceHeader>();
			licence.LA_LC = company.PK;
			licence.LA_LD = database.PK;
			licence.LA_SiteLiveDate = new ZDateTime(2006, 5, 28);

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = company.LC_CompanyCode;
			clientCompany.LCC_LD = database.PK;

			Factory.Save();

			SupportIncident incident = Factory.New<SupportIncident>();
			AssertEquals("IM_ClientContractStatus", "", incident.IM_ClientContractStatus);

			incident.IM_OH_Client = client.PK;
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;

			client.MiscServ.OM_AROnCreditHold = true;
			AssertEquals("IM_ClientContractStatus", "Credit on hold - Go-Live: 28-May-06", incident.IM_ClientContractStatus);

			client.MiscServ.OM_AROnCreditHold = false;
			licence.LA_ContractExpiryDate = ZDateTime.Now.AddDays(-1);
			AssertEquals("IM_ClientContractStatus", "Contract has expired (" + licence.LA_ContractExpiryDate.Date.ToShortDateString() + ") - Go-Live: 28-May-06",
			incident.IM_ClientContractStatus);

			licence.LA_ContractExpiryDate = ZDateTime.Now;
			licence.LA_SupportMode = "";
			AssertEquals("IM_ClientContractStatus", "Current - Go-Live: 28-May-06", incident.IM_ClientContractStatus);

			licence.LA_SupportMode = "NOS";
			AssertEquals("IM_ClientContractStatus", "Current - No Support - Go-Live: 28-May-06", incident.IM_ClientContractStatus);

			licence.LA_SupportMode = "STD";
			AssertEquals("IM_ClientContractStatus", "Current - Standard Support - Go-Live: 28-May-06", incident.IM_ClientContractStatus);

			licence.LA_SupportMode = "24H";
			AssertEquals("IM_ClientContractStatus", "Current - 24-Hour Support - Go-Live: 28-May-06", incident.IM_ClientContractStatus);

			incident.IM_LCC = ZGuid.Empty;
			AssertEquals("IM_ClientContractStatus", "Current - 24-Hour Support - Go-Live: 28-May-06", incident.IM_ClientContractStatus);

			incident.IM_LD = ZGuid.Empty;
			AssertEquals("IM_ClientContractStatus", "Current - 24-Hour Support - Go-Live: 28-May-06", incident.IM_ClientContractStatus);

			incident.IM_OH_Client = ZGuid.Invalid;
			AssertEquals("IM_ClientContractStatus", "", incident.IM_ClientContractStatus);
		}

		[TestDate(2022, 7, 11)]
		public void TestIM_ClientContractStatusWhenDatabaseIsNotSpecified()
		{
			var licenceHeader1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SYD", false);
			var licenceHeader2 = BillingTestHelper.CreateAnotherDatabase(licenceHeader1, "NJG", false);
			licenceHeader1.LA_ContractExpiryDate = ZDateTime.Now.AddDays(-1);
			licenceHeader2.Database.LD_Product = "F20";
			Factory.Save();

			var org = licenceHeader1.Database.LicEnterprise.Organisation;
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_Product = "F20";
			AssertNotEquals("IM_ClientContractStatus should not show expiry info", "Contract has expired (10-Jul-22)", incident.IM_ClientContractStatus);
			AssertEquals("IM_ClientContractStatus", "Current - Standard Support", incident.IM_ClientContractStatus);
		}

		public void TestIM_ClientContractStatusShouldReturnEarliestGoLiveDate()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var licCompany = Factory.NewWithValidTestData<LicenceCompany>();
			var licHeader1 = Factory.NewWithValidTestData<LicenceHeader>();
			var licHeader2 = Factory.NewWithValidTestData<LicenceHeader>();
			var licHeader3 = Factory.NewWithValidTestData<LicenceHeader>();
			incident.IM_OH_Client = org.PK;
			licCompany.LC_OH = org.PK;
			licHeader1.LA_LC = licCompany.PK;
			licHeader1.LA_SiteLiveDate = ZDate.Empty;
			licHeader2.LA_LC = licCompany.PK;
			licHeader2.LA_SiteLiveDate = new ZDate(2020, 05, 01);
			licHeader3.LA_LC = licCompany.PK;
			licHeader3.LA_SiteLiveDate = new ZDate(2020, 07, 01);
			Factory.Save();

			AssertEquals("IM_ClientContractStatus should show earliest go live date", "Current - Standard Support - Go-Live: 01-May-20", incident.IM_ClientContractStatus);
		}

		public void TestIM_ClientContractStatusNoLicenceCompanyShouldReturnEarliestGoLiveDate()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var licDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var licHeader1 = Factory.NewWithValidTestData<LicenceHeader>();
			var licHeader2 = Factory.NewWithValidTestData<LicenceHeader>();
			var licHeader3 = Factory.NewWithValidTestData<LicenceHeader>();
			incident.IM_OH_Client = org.PK;
			incident.IM_LD = licDatabase.PK;
			licHeader1.LA_LD = licDatabase.PK;
			licHeader1.LA_SiteLiveDate = ZDate.Empty;
			licHeader2.LA_LD = licDatabase.PK;
			licHeader2.LA_SiteLiveDate = new ZDate(2020, 05, 01);
			licHeader3.LA_LD = licDatabase.PK;
			licHeader3.LA_SiteLiveDate = new ZDate(2020, 07, 01);
			Factory.Save();

			AssertEquals("IM_ClientContractStatus should show earliest go live date", "Current - Standard Support - Go-Live: 01-May-20", incident.IM_ClientContractStatus);
		}

		#region ReadOnly States on different Stages

		public void TestDefectReadOnly()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;

			AssertEquals(true, incident.IM_GS_NKCustDefectCausedByInfo.ReadOnly);
			AssertEquals(true, incident.DefectCausedByWorkItemPKInfo.ReadOnly);
			AssertEquals(true, incident.IM_GG_TeamInfo.ReadOnly);
			AssertEquals(true, incident.IM_ClientBugSeverityInfo.ReadOnly);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;

			AssertEquals(false, incident.IM_GS_NKCustDefectCausedByInfo.ReadOnly);
			AssertEquals(false, incident.DefectCausedByWorkItemPKInfo.ReadOnly);
			AssertEquals(false, incident.IM_GG_TeamInfo.ReadOnly);
			AssertEquals(false, incident.IM_ClientBugSeverityInfo.ReadOnly);

			incident.IM_Status = SupportIncidentLookups.Status.Closed;

			AssertEquals(true, incident.IM_GS_NKCustDefectCausedByInfo.ReadOnly);
			AssertEquals(true, incident.DefectCausedByWorkItemPKInfo.ReadOnly);
			AssertEquals(true, incident.IM_GG_TeamInfo.ReadOnly);
			AssertEquals(true, incident.IM_ClientBugSeverityInfo.ReadOnly);

			Factory.Save();

			AssertEquals(true, incident.IM_GS_NKCustDefectCausedByInfo.ReadOnly);
			AssertEquals(true, incident.DefectCausedByWorkItemPKInfo.ReadOnly);
			AssertEquals(true, incident.IM_GG_TeamInfo.ReadOnly);
			AssertEquals(true, incident.IM_ClientBugSeverityInfo.ReadOnly);

			EDISecurityCheckpoints.CustomerServiceIncidentEditDefectManagement.IsAllowed = false;
			try
			{
				AssertEquals(true, incident.IM_GS_NKCustDefectCausedByInfo.ReadOnly);
				AssertEquals(true, incident.DefectCausedByWorkItemPKInfo.ReadOnly);
				AssertEquals(true, incident.IM_GG_TeamInfo.ReadOnly);
				AssertEquals(true, incident.IM_ClientBugSeverityInfo.ReadOnly);
			}
			finally
			{
				EDISecurityCheckpoints.CustomerServiceIncidentEditDefectManagement.IsAllowed = true;
			}
		}

		#region Feature Request ReadOnly

		public void TestFeatureRequestReadOnly()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			AssertFeatureRequestFieldsForReadOnly(incident, true);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			AssertFeatureRequestFieldsForReadOnly(incident, false);

			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			AssertFeatureRequestFieldsForReadOnly(incident, true);

			Factory.Save();
			AssertFeatureRequestFieldsForReadOnly(incident, true);

			try
			{
				EDISecurityCheckpoints.CustomerServiceIncidentEditFeatureManagement.IsAllowed = false;
				AssertFeatureRequestFieldsForReadOnly(incident, true);
			}
			finally
			{
				EDISecurityCheckpoints.CustomerServiceIncidentEditFeatureManagement.IsAllowed = true;
			}
		}

		void AssertFeatureRequestFieldsForReadOnly(SupportIncident incident, bool expectedReadOnlyState)
		{
			const string assertMessage = "Incorrectly set readonly state for Feature Request field";
			AssertEquals(assertMessage, expectedReadOnlyState, incident.IM_FeatureRequestIndustryValueInfo.ReadOnly);
		}

		#endregion

		#endregion

		[TestDate(2006, 10, 22)]
		public void TestClientLocalTime()
		{
			ITimeZone localZone = GlbBranch.CurrentBranch.HomePort.TimeZoneSet.GetCalculationTimeZone();
			DateTime universalTime = localZone.ToUniversalTime(ZDateTime.Now.ToDateTime());

			RefUNLOCO port1 = SetUpPortAndTimeZone("XX", new ZShort(300));
			RefUNLOCO port2 = SetUpPortAndTimeZone("GG", new ZShort(120));

			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_RL_NKClosestPort = port1.RL_Code;

			OrgAddress branch1 = client.Addresses.AddNew();
			branch1.OA_RL_NKRelatedPortCode = port2.RL_Code;

			OrgAddress branch2 = client.Addresses.AddNew();

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = client.PK;
			incident.IM_OA_BranchAddress = branch1.PK;

			AssertEquals("Address with port - Date when UTC offset = 2", universalTime.AddHours(2).Date, incident.ClientLocalTime.Date);
			AssertEquals("Address with port - Hour when UTC offset = 2", universalTime.AddHours(2).Hour, incident.ClientLocalTime.Hour);
			AssertEquals("Address with port - Minute when UTC offset = 2", universalTime.AddHours(2).Minute, incident.ClientLocalTime.Minute);

			incident.IM_OA_BranchAddress = branch2.PK;
			AssertEquals("Address with no port falls back to Org port - Date when UTC offset = 5", universalTime.AddHours(5).Date, incident.ClientLocalTime.Date);
			AssertEquals("Address with no port falls back to Org port - Hour when UTC offset = 5", universalTime.AddHours(5).Hour, incident.ClientLocalTime.Hour);
			AssertEquals("Address with no port falls back to Org port - Minute when UTC offset = 5", universalTime.AddHours(5).Minute, incident.ClientLocalTime.Minute);
		}

		public void TestDateAddedInLoggedTimeZone()
		{
			RefUNLOCO port1 = SetUpPortAndTimeZone("XX", new ZShort(120));
			port1.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_RL_NKHomePort = port1.Code;
			branch1.GB_PostCode = "002068";

			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "DP1";

			GlbStaff createUser = Factory.NewWithValidTestData<GlbStaff>();
			createUser.GS_Code = "S.G";
			createUser.GS_LoginName = "Serg";
			createUser.GS_GB_HomeBranch = branch1.PK;

			Factory.Save();

			SupportIncident incident;
			GlbBranch branch2;
			GlbDepartment department2;
			GlbStaff currentUser;

			using (Env.SetTemporaryUserContext(createUser.GS_LoginName, branch1.PK.ToGuid(), department1.PK.ToGuid()))
			{
				incident = Factory.NewWithValidTestData<SupportIncident>();

				Factory.Save();

				RefUNLOCO port2 = SetUpPortAndTimeZone("YY", new ZShort(660));
				port2.RL_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

				branch2 = Factory.NewWithValidTestData<GlbBranch>();
				branch2.GB_GC = GlbCompany.CurrentCompany.PK;
				branch2.GB_RL_NKHomePort = port2.Code;
				branch2.GB_PostCode = "002069";

				department2 = Factory.NewWithValidTestData<GlbDepartment>();
				department2.GE_Code = "DP2";

				currentUser = Factory.NewWithValidTestData<GlbStaff>();
				currentUser.GS_Code = "T.X";
				currentUser.GS_LoginName = "Tina";
				currentUser.GS_GB_HomeBranch = branch2.PK;

				Factory.Save();
			}
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, branch2.PK.ToGuid(), department2.PK.ToGuid()))
			{
				SupportIncident loadedeIncident = Factory.Load<SupportIncident>(incident.PK);

				AssertEquals("Incorrect system create time in current time zone", loadedeIncident.IM_InstallDate.AddHours(11), loadedeIncident.SystemCreateTimeInLocalTimeZone);
			}
		}

		[TestDate(2008, 1, 1, 10, 10, 10)]
		public void TestSystemCreateTimeInUTCSetOnFirstSaving()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			AssertEquals(ZDateTime.Empty, incident.IM_InstallDate);
			AssertEquals(ZDateTime.Empty, incident.SystemCreateTimeInUTC);

			ZDateTime expectedUTCTime = new ZDateTime(2008, 1, 1, 10, 10, 10);
			Factory.Save();
			AssertEquals("Should only be set once", expectedUTCTime, incident.IM_InstallDate);
			AssertEquals("Should only be set once", expectedUTCTime, incident.SystemCreateTimeInUTC);

			TestUtcOffsetAttribute.Time = new TimeSpan(12, 12, 12);
			Factory.Save();
			AssertEquals("Should only be set once", expectedUTCTime, incident.IM_InstallDate);
			AssertEquals("Should only be set once", expectedUTCTime, incident.SystemCreateTimeInUTC);

			incident.IM_Description = "Changing something so HasChanges is true";
			Factory.Save();
			AssertEquals("Should only be set once", expectedUTCTime, incident.IM_InstallDate);
			AssertEquals("Should only be set once", expectedUTCTime, incident.SystemCreateTimeInUTC);
		}

		RefUNLOCO SetUpPortAndTimeZone(ZString code, ZShort uTCOffset)
		{
			RefTimeZone timeZone = Factory.New<RefTimeZone>();
			timeZone.R2_CivilianTimeZoneCode = code + "T";
			timeZone.R2_OffsetMinutesFromUTC = uTCOffset;

			RefTimeZoneSet timeZoneSet = Factory.New<RefTimeZoneSet>();
			timeZoneSet.R3_R2_StandardZone = timeZone.PK;
			timeZoneSet.R3_R2_DaylightSavingZone = Factory.NewWithValidTestData<DaylightSavingTimeZone>().PK;
			timeZoneSet.R3_TimeZoneSetName = code;

			RefUNLOCO port = Factory.New<RefUNLOCO>();
			port.RL_Code = code + "VVV";
			port.RL_R3 = timeZoneSet.PK;

			return port;
		}

		public void TestRingRelease()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ZUBIN";
			org.OH_FullName = "Zubs Organisation";

			var entnterprise = Factory.New<LicenceEnterprise>();
			entnterprise.LE_EnterpriseCode = "ENT";
			entnterprise.LE_OH = org.PK;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = entnterprise.PK;
			database.LD_ReleaseRing = ReleaseRings.Codes.STD;

			var company = Factory.New<ClientCompany>();
			company.LCC_Code = "COM";
			company.LCC_LD = database.PK;
			company.LCC_OH = org.PK;

			Factory.Save();

			var incident = Factory.New<SupportIncident>();
			AssertEquals("", incident.RingRelease);

			incident.IM_LD = database.PK;
			incident.IM_LCC = company.PK;
			AssertEquals(ReleaseRings.Lookup(ReleaseRings.Codes.STD).LongDescription, incident.RingRelease);
		}

		public void TestIsInternalFeatureRequest()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			LicenceEnterprise enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;
			LicenceCompany company = Factory.NewWithValidTestData<LicenceCompany>();
			company.LC_LE = enterprise.PK;
			company.LC_OH = org.PK;
			LicenceDatabase database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_LE = enterprise.PK;
			LicenceHeader header = Factory.NewWithValidTestData<LicenceHeader>();
			header.LA_LC = company.PK;
			header.LA_LD = database.PK;
			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = company.LC_CompanyCode;
			clientCompany.LCC_LD = database.PK;
			clientCompany.LCC_OH = org.PK;

			Factory.Save();

			InternalIncidentLicenceSettings settings = new InternalIncidentLicenceSettings();
			LicenceEnterpriseKey key = new LicenceEnterpriseKey();
			key.LE_PK = enterprise.PK;
			settings.LicenceEnterpriseKeys.Add(key);
			settings.EdiProd_LicencePK = header.PK;
			settings.UAT_ALP_LicencePK = header.PK;
			settings.UAT_DPR_LicencePK = header.PK;
			settings.UAT_GPC_LicencePK = header.PK;
			settings.UAT_GPR_LicencePK = header.PK;
			settings.UAT_STD_LicencePK = header.PK;
			EDIDataRegistry.Instance.InternalIncidentLicenceSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			SupportIncident featureRequest = Factory.New<SupportIncident>();
			Assert(!featureRequest.IsInternalFeatureRequest);

			featureRequest.SetupForInternalReportedIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, "", null);
			featureRequest.IM_OH_Client = Factory.New<OrgHeader>().PK;
			featureRequest.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;

			Assert(!featureRequest.IsInternalFeatureRequest);

			featureRequest.SetupForInternalReportedIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncident.InternalFeatureRequestComment, header);
			Assert("IsInternalFeatureRequest", featureRequest.IsInternalFeatureRequest);

			featureRequest.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			Assert("!IsInternalFeatureRequest", !featureRequest.IsInternalFeatureRequest);

			featureRequest.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "back to you");
			Assert("IsInternalFeatureRequest", featureRequest.IsInternalFeatureRequest);

			LicenceEnterprise enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			LicenceDatabase database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			database2.LD_LE = enterprise2.PK;
			ClientCompany clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_Code = "AAA";
			clientCompany2.LCC_LD = database2.PK;
			featureRequest.IM_LCC = clientCompany2.PK;
			Assert("!IsInternalFeatureRequest", !featureRequest.IsInternalFeatureRequest);
		}

		public void TestIsInternal()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			LicenceEnterprise enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;

			LicenceCompany company = Factory.NewWithValidTestData<LicenceCompany>();
			company.LC_LE = enterprise.PK;
			company.LC_OH = org.PK;

			LicenceDatabase database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			database1.LD_LE = enterprise.PK;
			database1.LD_ServerCode = "AAA";

			LicenceDatabase database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			database2.LD_LE = enterprise.PK;
			database2.LD_ServerCode = "BBB";

			LicenceHeader header = Factory.NewWithValidTestData<LicenceHeader>();
			header.LA_LC = company.PK;
			header.LA_LD = database1.PK;

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = company.LC_CompanyCode;
			clientCompany.LCC_LD = database1.PK;
			clientCompany.LCC_OH = org.PK;

			Factory.Save();

			InternalIncidentLicenceSettings settings = new InternalIncidentLicenceSettings();
			LicenceEnterpriseKey key = new LicenceEnterpriseKey();
			key.LE_PK = enterprise.PK;
			settings.LicenceEnterpriseKeys.Add(key);
			settings.EdiProd_LicencePK = header.PK;
			settings.UAT_ALP_LicencePK = header.PK;
			settings.UAT_DPR_LicencePK = header.PK;
			settings.UAT_GPC_LicencePK = header.PK;
			settings.UAT_GPR_LicencePK = header.PK;
			settings.UAT_STD_LicencePK = header.PK;
			EDIDataRegistry.Instance.InternalIncidentLicenceSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			SupportIncident incident = Factory.New<SupportIncident>();
			Assert(!incident.IsInternal);

			incident.SetupForInternalReportedIncident(SupportIncidentCategoriesList.Codes.Defect, "", null);
			incident.IM_OH_Client = Factory.New<OrgHeader>().PK;
			incident.IM_Source = SupportIncidentLookups.SourceListConstants.WTGInternalViaEdiProd;

			Assert(!incident.IsInternal);

			incident.SetupForInternalReportedIncident("", SupportIncident.InternalIncidentComment, header);
			Assert("IsInternalIncident", incident.IsInternal);
			AssertEquals(org.PK, incident.IM_OH_Client);
			AssertEquals(enterprise.PK, incident.EnterprisePK);
			AssertEquals(database1.PK, incident.IM_LD);
			AssertEquals(clientCompany.PK, incident.IM_LCC);
		}

		public void TestContactAccreditationStatusInfoRefreshBinding()
		{
			bool refreshBindingCalled = false;
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Module = "001";
			incident.ContactAccreditationStatusInfo.ValueChanged += delegate
			{ refreshBindingCalled = true; };
			incident.IM_Module = "002";
			Assert(refreshBindingCalled);
		}

		public void TestCurrentTaskEstimatedDateAsText()
		{
			AssertEquals("Pre-condition: empty date", ZString.Empty, Incident.CurrentTaskEstimatedDateAsText);

			ProcessTask task1 = Incident.WorkflowItems.AddNew();
			task1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2009, 11, 23, 11, 36, 0)));
			task1.P9_Sequence = 10;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			ProcessTask task2 = Incident.WorkflowItems.AddNew();
			task2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2009, 12, 10, 17, 0, 0)));
			task2.P9_Sequence = 20;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			AssertEquals("Should be the date of task1", "23-Nov-09 11:36", Incident.CurrentTaskEstimatedDateAsText);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Should be the date of task2", "10-Dec-09 17:00", Incident.CurrentTaskEstimatedDateAsText);
		}

		public void TestSettingBranchAddressSetsCorrectARSettlementGroupAddressToJob()
		{
			OrgHeader mainOrg = Factory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("mainOrg.MainAddress", mainOrg.MainAddress);
			OrgAddress arAddress = mainOrg.Addresses.AddNew(OrgAddressType.Receivables, true);
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AssertNotNull("org.MainAddress", org.MainAddress);

			org.ARSettlementGroupPK = mainOrg.PK;
			SupportIncident incident = (SupportIncident)this.GetNewBusinessObject();
			Job.Loader loader = new Job.Loader(incident);
			Job job = loader.Load();
			AssertNull("Precondition: Job", job);
			incident.IM_OH_Client = org.PK;
			incident.IM_OA_BranchAddress = org.MainAddress.PK;
			job = loader.Load();
			AssertNull("Setting IM_OH_Client does not create Job", job);

			loader.TryCreate();
			incident.IM_OH_Client = org.PK;
			incident.IM_OA_BranchAddress = org.MainAddress.PK;
			job = loader.Load();
			AssertNotNull("Job", job);
			AssertEquals("Should be AR adress of the Settlement Group", arAddress.PK, job.JH_OA_LocalChargesAddr);
		}

		public void TestClientRelationshipManager()
		{
			AssertNull("Pre-condition: no primary relationship manager", Incident.ClientPrimaryRelationshipManager);
			AssertNull("Pre-condition: no secondary relationship manager", Incident.ClientSecondaryRelationshipManager);
			AssertEquals("", Incident.RelationshipStaffCodeAndName);

			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			Incident.IM_OH_Client = org.PK;

			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "CO1";
			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "CO2";
			GlbCompany company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "CO3";

			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;
			GlbBranch branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch3.GB_GC = company3.PK;

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ZE1";
			staff1.GS_FullName = "RM One";
			OrgStaffAssignments assignment = org.StaffAssignments.AddNew();
			assignment.O8_GS_NKPersonResponsible = staff1.GS_Code;
			assignment.O8_Role = "RM1";
			assignment.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			assignment.O8_GC = company1.PK;

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ZE2";
			staff2.GS_FullName = "RM Two";
			OrgStaffAssignments assignment2 = org.StaffAssignments.AddNew();
			assignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			assignment2.O8_Role = "RM2";
			assignment2.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			assignment2.O8_GC = company1.PK;

			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "ZE3";
			staff3.GS_FullName = "RM Three";
			OrgStaffAssignments assignment3 = org.StaffAssignments.AddNew();
			assignment3.O8_GS_NKPersonResponsible = staff3.GS_Code;
			assignment3.O8_Role = "RM1";
			assignment3.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			assignment3.O8_GC = company2.PK;

			Factory.Save();

			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("CO1", "CO1");
			list.AddPair("CO2", "CO2");
			list.AddPair("CO3", "CO2");
			EDIDataRegistry.Instance.RelationshipManagerCompanyLookup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			GlbStaff.CurrentUser.GS_GB_HomeBranch = branch1.PK;
			AssertEquals("Key Account Manager:", Incident.RelationshipManagerOrCoordinatorLabel);
			AssertEquals("Primary Relationship Manager should be staff one", "ZE1", Incident.ClientPrimaryRelationshipManager.GS_Code);
			AssertEquals("Secondary Relationship Manager should be staff two", "ZE2", Incident.ClientSecondaryRelationshipManager.GS_Code);
			AssertEquals("ZE1 RM One / ZE2 RM Two", Incident.RelationshipStaffCodeAndName);

			GlbStaff.CurrentUser.GS_GB_HomeBranch = branch2.PK;
			AssertEquals("Key Account Manager:", Incident.RelationshipManagerOrCoordinatorLabel);
			AssertEquals("Primary Relationship Manager should be staff three", "ZE3", Incident.ClientPrimaryRelationshipManager.GS_Code);
			AssertEquals("ZE3 RM Three", Incident.RelationshipStaffCodeAndName);

			GlbStaff.CurrentUser.GS_GB_HomeBranch = branch3.PK;
			AssertEquals("Key Account Manager:", Incident.RelationshipManagerOrCoordinatorLabel);
			AssertEquals("Primary Relationship Manager should be staff three", "ZE3", Incident.ClientPrimaryRelationshipManager.GS_Code);
			AssertEquals("ZE3 RM Three", Incident.RelationshipStaffCodeAndName);
		}

		public void TestClientRelationshipManager_ProductSpecific()
		{
			AssertNull("Pre-condition: no primary relationship manager", Incident.ClientPrimaryRelationshipManager);
			AssertNull("Pre-condition: no secondary relationship manager", Incident.ClientSecondaryRelationshipManager);
			AssertEquals("", Incident.RelationshipStaffCodeAndName);

			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			Incident.IM_Product = ProductTypes.Codes.Enterprise;
			Incident.IM_OH_Client = org.PK;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "CO1";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ZE1";
			staff1.GS_FullName = "RM One";
			var assignment = org.StaffAssignments.AddNew();
			assignment.O8_GS_NKPersonResponsible = staff1.GS_Code;
			assignment.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			assignment.O8_Role = "RM1";
			assignment.O8_GC = ZGuid.Empty;
			assignment.O8_Product = ZString.Empty;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ZE2";
			staff2.GS_FullName = "RM Two";
			var assignment2 = org.StaffAssignments.AddNew();
			assignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			assignment2.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			assignment2.O8_Role = "RM2";
			assignment2.O8_GC = ZGuid.Empty;
			assignment2.O8_Product = ZString.Empty;

			Factory.Save();

			var list = new CodeDescriptionPairList();
			list.AddPair("CO1", "CO1");
			list.AddPair("CO2", "CO2");
			list.AddPair("CO3", "CO2");
			EDIDataRegistry.Instance.RelationshipManagerCompanyLookup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			GlbStaff.CurrentUser.GS_GB_HomeBranch = branch.PK;
			AssertEquals("Key Account Manager:", Incident.RelationshipManagerOrCoordinatorLabel);
			AssertEquals("Primary Relationship Manager should be staff one", "ZE1", Incident.ClientPrimaryRelationshipManager.GS_Code);
			AssertEquals("Secondary Relationship Manager should be staff two", "ZE2", Incident.ClientSecondaryRelationshipManager.GS_Code);
			AssertEquals("ZE1 RM One / ZE2 RM Two", Incident.RelationshipStaffCodeAndName);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "ZE3";
			staff3.GS_FullName = "RM Three";
			var assignment3 = org.StaffAssignments.AddNew();
			assignment3.O8_GS_NKPersonResponsible = staff3.GS_Code;
			assignment3.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			assignment3.O8_Role = "RM1";
			assignment3.O8_GC = ZGuid.Empty;
			assignment3.O8_Product = ProductTypes.Codes.Enterprise;
			Factory.Save();

			Incident.Client.StaffAssignments.Load();
			AssertEquals("Should prioritize product specific roles", "ZE3", Incident.ClientPrimaryRelationshipManager.GS_Code);
			AssertEquals("ZE3 RM Three / ZE2 RM Two", Incident.RelationshipStaffCodeAndName);

			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_Code = "ZE4";
			staff4.GS_FullName = "RM Four";
			var assignment4 = org.StaffAssignments.AddNew();
			assignment4.O8_GS_NKPersonResponsible = staff4.GS_Code;
			assignment4.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			assignment4.O8_Role = "RM1";
			assignment4.O8_GC = company.PK;
			assignment4.O8_Product = ZString.Empty;
			Factory.Save();

			Incident.Client.StaffAssignments.Load();
			AssertEquals("Should prioritize product specific roles", "ZE4", Incident.ClientPrimaryRelationshipManager.GS_Code);
			AssertEquals("ZE4 RM Four / ZE2 RM Two", Incident.RelationshipStaffCodeAndName);

			var staff5 = Factory.NewWithValidTestData<GlbStaff>();
			staff5.GS_Code = "ZE5";
			staff5.GS_FullName = "RM Five";
			var assignment5 = org.StaffAssignments.AddNew();
			assignment5.O8_GS_NKPersonResponsible = staff5.GS_Code;
			assignment5.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			assignment5.O8_Role = "RM1";
			assignment5.O8_GC = company.PK;
			assignment5.O8_Product = ProductTypes.Codes.Enterprise;
			Factory.Save();

			Incident.Client.StaffAssignments.Load();
			AssertEquals("Should prioritize product specific roles", "ZE5", Incident.ClientPrimaryRelationshipManager.GS_Code);
			AssertEquals("ZE5 RM Five / ZE2 RM Two", Incident.RelationshipStaffCodeAndName);
		}

		public void TestClientRelationshipCoordinator()
		{
			AssertNull("Pre-condition: no relationship coordinator", Incident.ClientRelationshipCoordinator);
			AssertEquals("", Incident.RelationshipStaffCodeAndName);

			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			Incident.IM_OH_Client = org.PK;

			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "CO1";
			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "CO2";

			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ZE1";
			staff1.GS_FullName = "RC One";
			OrgStaffAssignments assignment = org.StaffAssignments.AddNew();
			assignment.O8_GS_NKPersonResponsible = staff1.GS_Code;
			assignment.O8_Role = "RC";
			assignment.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			assignment.O8_GC = company1.PK;

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ZE2";
			staff2.GS_FullName = "RC Two";
			OrgStaffAssignments assignment2 = org.StaffAssignments.AddNew();
			assignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			assignment2.O8_Role = "RC";
			assignment2.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			assignment2.O8_GC = company2.PK;

			Factory.Save();

			GlbStaff.CurrentUser.GS_GB_HomeBranch = branch1.PK;
			AssertEquals("Key Account Co:", Incident.RelationshipManagerOrCoordinatorLabel);
			AssertEquals("Relationship Coordinator should be staff one", "ZE1", Incident.ClientRelationshipCoordinator.GS_Code);
			AssertEquals("ZE1 RC One", Incident.RelationshipStaffCodeAndName);

			GlbStaff.CurrentUser.GS_GB_HomeBranch = branch2.PK;
			AssertEquals("Key Account Co:", Incident.RelationshipManagerOrCoordinatorLabel);
			AssertEquals("Relationship Coordinator should be staff two", "ZE2", Incident.ClientRelationshipCoordinator.GS_Code);
			AssertEquals("ZE2 RC Two", Incident.RelationshipStaffCodeAndName);
		}

		public void TestClientRelationshipCoordinatorOrManager()
		{
			AssertNull("Pre-condition: no primary relationship manager", Incident.ClientPrimaryRelationshipManager);
			AssertNull("Pre-condition: no secondary relationship manager", Incident.ClientSecondaryRelationshipManager);
			AssertNull("Pre-condition: no relationship coordinator", Incident.ClientRelationshipCoordinator);
			AssertEquals("", Incident.RelationshipStaffCodeAndName);

			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			Incident.IM_OH_Client = org.PK;

			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "CO1";
			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "CO2";
			GlbCompany company3 = Factory.NewWithValidTestData<GlbCompany>();
			company3.GC_Code = "CO3";

			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;
			GlbBranch branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch3.GB_GC = company3.PK;

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ZE1";
			staff1.GS_FullName = "RM One";
			OrgStaffAssignments assignment = org.StaffAssignments.AddNew();
			assignment.O8_GS_NKPersonResponsible = staff1.GS_Code;
			assignment.O8_Role = "RM1";
			assignment.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			assignment.O8_GC = company1.PK;

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ZE2";
			staff2.GS_FullName = "RM Two";
			OrgStaffAssignments assignment2 = org.StaffAssignments.AddNew();
			assignment2.O8_GS_NKPersonResponsible = staff2.GS_Code;
			assignment2.O8_Role = "RM2";
			assignment2.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			assignment2.O8_GC = company1.PK;

			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "ZE3";
			staff3.GS_FullName = "RC One";
			OrgStaffAssignments assignment3 = org.StaffAssignments.AddNew();
			assignment3.O8_GS_NKPersonResponsible = staff3.GS_Code;
			assignment3.O8_Role = "RC";
			assignment3.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			assignment3.O8_GC = company1.PK;

			GlbStaff staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_Code = "ZE4";
			staff4.GS_FullName = "RC Two";
			OrgStaffAssignments assignment4 = org.StaffAssignments.AddNew();
			assignment4.O8_GS_NKPersonResponsible = staff4.GS_Code;
			assignment4.O8_Role = "RC";
			assignment4.O8_Department = OrgStaffAssignmentsLookups.AllServices;
			assignment4.O8_GC = company2.PK;

			Factory.Save();

			GlbStaff.CurrentUser.GS_GB_HomeBranch = branch1.PK;
			AssertEquals("Key Account Manager:", Incident.RelationshipManagerOrCoordinatorLabel);
			AssertEquals("Primary Relationship Manager should be staff one", "ZE1", Incident.ClientPrimaryRelationshipManager.GS_Code);
			AssertEquals("Secondary Relationship Manager should be staff two", "ZE2", Incident.ClientSecondaryRelationshipManager.GS_Code);
			AssertEquals("ZE1 RM One / ZE2 RM Two", Incident.RelationshipStaffCodeAndName);

			GlbStaff.CurrentUser.GS_GB_HomeBranch = branch2.PK;
			AssertEquals("Key Account Co:", Incident.RelationshipManagerOrCoordinatorLabel);
			AssertEquals("Relationship Coordinator should be staff two", "ZE4", Incident.ClientRelationshipCoordinator.GS_Code);
			AssertEquals("ZE4 RC Two", Incident.RelationshipStaffCodeAndName);

			GlbStaff.CurrentUser.GS_GB_HomeBranch = branch3.PK;
			AssertEquals("Key Account Manager:", Incident.RelationshipManagerOrCoordinatorLabel);
			AssertEquals("", Incident.RelationshipStaffCodeAndName);
		}

		public void TestIsClosedOrCancelled()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			Assert(!((IWorkTaskRelatedItem)incident).IsClosedOrCancelled);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "Close close");
			Assert(((IWorkTaskRelatedItem)incident).IsClosedOrCancelled);
		}

		public void TestFilteredRelatedItems()
		{
			var defect = Factory.NewWithValidTestData<SupportIncident>();
			defect.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			defect.IM_Status = SupportIncidentLookups.Status.Closed;
			var issue = Factory.NewWithValidTestData<EdiHelpErrorLog>();
			var workitem = Factory.NewWithValidTestData<NewWorkItem>();
			workitem.WKI_Status = ProcessTaskStatusCodeList.Codes.Working;
			var project = Factory.NewWithValidTestData<EDIProject>();
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.RelatedItems.Add(defect);
			incident.RelatedItems.Add(issue);
			incident.RelatedItems.Add(workitem);
			incident.RelatedItems.Add(project);
			incident.RelatedItems.Add(opportunity);

			AssertEquals(5, incident.FilteredRelatedItems.Count);
			AssertCollectionContains(defect, incident.FilteredRelatedItems);
			AssertCollectionContains(issue, incident.FilteredRelatedItems);
			AssertCollectionContains(workitem, incident.FilteredRelatedItems);
			AssertCollectionContains(project, incident.FilteredRelatedItems);
			AssertCollectionContains(opportunity, incident.FilteredRelatedItems);

			incident.ShowOnlyNonClosedItems = true;
			AssertEquals(3, incident.FilteredRelatedItems.Count);
			AssertCollectionNotContains(defect, incident.FilteredRelatedItems);
			AssertCollectionContains(issue, incident.FilteredRelatedItems);
			AssertCollectionContains(workitem, incident.FilteredRelatedItems);
			AssertCollectionNotContains(project, incident.FilteredRelatedItems);
			AssertCollectionContains(opportunity, incident.FilteredRelatedItems);
		}

		public void TestConsistentOrgAddressAndContactWithWorkflowTasks()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address1 = org1.Addresses.AddNew();
			OrgContact contact1 = org1.Contacts.AddNew();
			contact1.WorkingAddressPK = address1.PK;

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgAddress address2 = org2.Addresses.AddNew();
			OrgContact contact2 = org2.Contacts.AddNew();
			contact2.WorkingAddressPK = address2.PK;

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org1.PK;
			incident.IM_OC_Contact = contact1.PK;

			SupportIncidentProcessTask task = incident.WorkflowItems.AddNew();

			AssertEquals(address1.PK, task.P9_OA);
			AssertEquals(contact1.PK, task.P9_OC);
			AssertEquals(org1.PK, task.OrganisationPK);

			incident.IM_OH_Client = org2.PK;
			incident.IM_OA_BranchAddress = address2.PK;
			incident.IM_OC_Contact = contact2.PK;

			AssertEquals(address2.PK, task.P9_OA);
			AssertEquals(contact2.PK, task.P9_OC);
			AssertEquals(org2.PK, task.OrganisationPK);
		}

		[TestDate(2010, 7, 14, 15, 49, 24)]
		public void TestLastDispositionChangeDateTimeUtc()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			AssertEquals(ZDateTime.Empty, incident.LastDispositionChangeDateTimeUtc);
			Factory.Save();

			incident.AssignAndInvestigateInSupport(GlbStaff.CurrentUser);
			Factory.Save();
			AssertEquals(new ZDateTime(2010, 7, 14, 15, 49, 24), incident.LastDispositionChangeDateTimeUtc);

			TestDateAttribute.Date = new DateTime(2010, 7, 14, 15, 58, 10);
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "On hold");
			Factory.Save();
			AssertEquals(new ZDateTime(2010, 7, 14, 15, 58, 10), incident.LastDispositionChangeDateTimeUtc);
		}

		public void TestIncidentCloseTypeText()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			GlbStaff defectMgr = Factory.New<GlbStaff>();
			defectMgr.GS_FullName = "Defect Team";
			defectMgr.GS_EmailAddress = "defect@test.com";
			group.Staff.Add(defectMgr);
			defectMgr.CurrentGroupLink.GK_MembershipType = "MGR";
			EDIDataRegistry.Instance.IncidentDefectManagerGroupENT.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;

			AssertEquals("", incident.IncidentCloseTypeText);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			AssertEquals("Incident is NOT closed so no close type text", "", incident.IncidentCloseTypeText);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.NoResponseFromClient, "");
			AssertEquals("Closed with reason: No Response from Client", incident.IncidentCloseTypeText);

			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;
			AssertEquals("Resolved with reason: No Response from Client", incident.IncidentCloseTypeText);
		}

		public void TestClientSizeDescription()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("ABC", "Medium Size");
			list.AddPair("DDD", "Large Size");
			OrganisationsDataRegistry.Instance.ClientSizeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			SupportIncident incident = Factory.New<SupportIncident>();
			AssertEquals("", incident.ClientSizeDescription);

			OrgHeader client = Factory.New<OrgHeader>();
			incident.IM_OH_Client = client.PK;
			AssertEquals("", incident.ClientSizeDescription);

			client.MiscServ.OM_CMClientSize = "ABC";
			AssertEquals("Medium Size", incident.ClientSizeDescription);

			client.MiscServ.OM_CMClientSize = "DDD";
			AssertEquals("Large Size", incident.ClientSizeDescription);
		}

		public void TestChangeProductClearProductArea()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();
			SupportIncident incident = Factory.New<SupportIncident>();
			AssertEquals("", incident.ProductArea);

			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.ProductArea = ProductAreaList.Codes.CUS;
			AssertEquals(ProductAreaList.Codes.CUS, incident.ProductArea);

			incident.IM_Product = "AAA";
			AssertEquals("CUS", incident.ProductArea);
		}

		public void TestProductAreaDescription()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			AssertEquals("", incident.ProductAreaDescription);

			incident.ProductArea = ProductAreaList.Codes.DOM;
			AssertEquals(ProductAreaList.Descriptions.DOM, incident.ProductAreaDescription);
		}

		public void TestCurrentTaskStatus()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			AssertEquals("", incident.CurrentTaskStatus);

			SupportIncidentProcessTask task = incident.WorkflowItems.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, incident.CurrentTaskStatus);
		}

		public void TestLicenceDatabaseHostedLocation()
		{
			var list = new CodeDescriptionBoolCollection();
			list.Add("SYD", (NoResString)"SYD Data Center 2", true);
			EDIDataRegistry.Instance.DatabaseHostedLocations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			AssertEquals("", incident.LicenceDatabaseHostedLocation);

			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = org.LicCompany.LicDatabases.AddNew();
			database.FillWithValidTestData();
			database.LD_HostedLocation = "SYD";
			var licHeader = org.LicCompany.GetHeader(database);
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_OH = org.PK;
			clientCompany.LCC_LD = database.PK;
			clientCompany.LCC_Code = org.LicCompany.LC_CompanyCode;

			incident.IM_OH_Client = org.PK;
			incident.IM_LCC = licHeader.ClientCompany.PK;

			Factory.Save();

			AssertEquals("SYD Data Center 2", incident.LicenceDatabaseHostedLocation);
		}

		public void TestPersistentStageValue()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.AssignAndInvestigateInSupport(GlbStaff.CurrentUser);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.NoSupportContract, "");
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Other, "");
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Other, "");
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
			AssertEquals(SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);

			incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Support, "");
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
		}

		public void TestProductAreaAssignedStaff()
		{
			GlbStaff pm1 = Factory.NewWithValidTestData<GlbStaff>();
			pm1.GS_Code = "PM1";
			pm1.GS_EmailAddress = "pm1@test.com";

			GlbStaff pm2 = Factory.NewWithValidTestData<GlbStaff>();
			pm2.GS_Code = "PM2";
			pm2.GS_EmailAddress = "pm2@test.com";
			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "Module A", ProductAreaList.Codes.ARC, false);
			product.ModuleMappings.AddNew("BBB", "Module B", "", false);
			product.ModuleMappings.AddNew("CCC", "Module C", ProductAreaList.Codes.FIN, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			ProductAreaAssignmentCollection assignmentCollection = new ProductAreaAssignmentCollection();
			ProductAreaAssignment assignment1 = assignmentCollection.AddNew();
			assignment1.ProductArea = ProductAreaList.Codes.ARC;
			assignment1.Staff = pm1.GS_Code;
			ProductAreaAssignment assignment2 = assignmentCollection.AddNew();
			assignment2.ProductArea = ProductAreaAssignmentLookups.BlankProductAreaCode;
			assignment2.Staff = pm2.GS_Code;
			EDIDataRegistry.Instance.ProductAreaAssignments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, assignmentCollection);

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;

			incident.ProductArea = ProductAreaList.Codes.ARC;
			incident.IM_Module = "AAA";
			AssertEquals("PM1", incident.ProductAreaAssignedStaff.GS_Code);

			incident.ProductArea = "";
			incident.IM_Module = "BBB";
			AssertEquals("PM2", incident.ProductAreaAssignedStaff.GS_Code);

			incident.ProductArea = ProductAreaList.Codes.FIN;
			incident.IM_Module = "CCC";
			AssertNull(incident.ProductAreaAssignedStaff);
		}

		#endregion

		#region Default Values

		public void TestDefaultFeatureClient()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "~code";

			var mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "some address";
			mainAddress.OA_City = "some city";
			mainAddress.OA_State = "NSW";
			mainAddress.OA_RN_NKCountryCode = "AU";

			var address = org.Addresses.AddNew();
			address.OA_Address1 = "other address";
			address.OA_City = "other city";
			address.OA_RN_NKCountryCode = "GB";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "test1@email.com";
			contact1.OC_ContactName = "name 1";
			contact1.WorkingAddressPK = mainAddress.PK;

			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "test2@email.com";
			contact2.OC_ContactName = "name 2";
			contact2.WorkingAddressPK = address.PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "staff";
			staff.GS_WorkingLanguage = SharedConstants.Languages.ChineseSimplified;
			Factory.Save();

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = Org.MainAddress.PK;
			incident.IM_OC_Contact = Contact.PK;
			incident.IM_Description = "Help me";
			incident.DetailNoteText = "Don't know what i'm doing";
			incident.IM_LD = ClientCompany.Database.PK;
			incident.IM_LCC = ClientCompany.PK;
			incident.IM_ClientIncidentReference = "CL111111";
			incident.IM_Priority = "CR3";
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact2.PK;
			Factory.Save();

			AssertEquals(ZGuid.Empty, incident.FeatureRequestClientAddressPK_ZAddress.OrgPK);

			incident.IM_Category = "FTR";
			incident.IM_Priority = "CR6";
			AssertEquals(org.PK, incident.FeatureRequestClientAddressPK_ZAddress.OrgPK);
			AssertEquals(address.PK, incident.FeatureRequestClientAddressPK);
			AssertEquals(contact2.PK, incident.FeatureRequestContactPK);
		}

		public void TestDefaultValues()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Mei";
			staff.GS_WorkingLanguage = SharedConstants.Languages.ChineseSimplified;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Guid.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				SupportIncident incident1 = Factory.New<SupportIncident>();
				AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident1.IM_Category);
				AssertEquals(SupportIncidentLookups.Status.Open, incident1.IM_Status);
				AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident1.IM_ResolutionCode);
				AssertEquals(ZArchitectureBusiness.AddressType.OFC, incident1.IM_OA_BranchAddress_ZAddress.DefaultAddressType);
				AssertEquals(SharedConstants.Languages.English, incident1.IM_Language);
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var incident = Factory.New<SupportIncident>();
				AssertEquals(staff.GS_WorkingLanguage, incident.IM_Language);
			}
		}

		public void TestSetupForInternalReportedIncident()
		{
			#region Test Data

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TESTORG";
			LicenceEnterprise enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;
			enterprise.LE_EnterpriseCode = "AAA";
			LicenceCompany company = Factory.NewWithValidTestData<LicenceCompany>();
			company.LC_OH = org.PK;
			company.LC_LE = enterprise.PK;
			company.LC_CompanyCode = "BBB";
			LicenceDatabase database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_LE = enterprise.PK;
			database.LD_ServerCode = "XXX";
			LicenceHeader licence = Factory.NewWithValidTestData<LicenceHeader>();
			licence.LA_LC = company.PK;
			licence.LA_LD = database.PK;
			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = company.LC_CompanyCode;
			clientCompany.LCC_LD = database.PK;
			clientCompany.LCC_OH = org.PK;

			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_ContactName = "Dummy Contact";
			contact.OC_Email = "dummy@test.com";

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Samuel";
			staff.GS_EmailAddress = "samuel@test.com";
			staff.GS_Code = "SCW";

			Factory.Save();

			#endregion

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_LD = database.PK;

			incident.SetupForInternalReportedIncident("", SupportIncident.InternalIncidentComment, licence);
			incident.IM_IncidentNumber = "CS00004512";
			incident.IM_Description = "Test Incident";
			AssertEquals(SupportIncidentLookups.SourceListConstants.WTGInternalViaEdiProd, incident.IM_Source);
			AssertEquals("", incident.IM_Priority);
			AssertEquals("", incident.IM_GS_NKAssignedToCurrent);
			AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);
			AssertEquals(true, incident.EConversation.GetTimeOrderedMessages().Any(msg => msg.Body == "Internal Incident"));
			AssertEquals("TESTORG", incident.ClientCode);
			AssertEquals("AAABBBXXX", incident.ClientCompany.LicenceCode);
			AssertEquals(GlbStaff.CurrentUser.GS_FullName, incident.Contact.OC_ContactName);

			SupportIncident featureRequest = Factory.New<SupportIncident>();
			featureRequest.SetupForInternalReportedIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncident.InternalFeatureRequestComment, licence);
			featureRequest.IM_IncidentNumber = "CS00004515";
			featureRequest.IM_Description = "Test Internal FR";
			AssertEquals(SupportIncidentLookups.SourceListConstants.WTGInternalViaEdiProd, featureRequest.IM_Source);
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, featureRequest.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Open, featureRequest.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, featureRequest.IM_ResolutionCode);
			AssertEquals("TESTORG", incident.ClientCode);
			AssertEquals("AAABBBXXX", incident.ClientCompany.LicenceCode);
			AssertEquals(GlbStaff.CurrentUser.GS_FullName, incident.Contact.OC_ContactName);

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "AAA";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "BBB";

			SupportIncidentProcessTask task1 = featureRequest.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;

			AssertEquals("Current assigned to AAA", "AAA", featureRequest.IM_GS_NKAssignedToCurrent);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			SupportIncidentProcessTask task2 = featureRequest.WorkflowItems.AddNew();
			task2.P9_Sequence = 2;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;

			AssertEquals("Current assigned to BBB", "BBB", featureRequest.IM_GS_NKAssignedToCurrent);
		}

		#endregion

		#region Notes / Logs

		protected PredefinedNoteType[] GetExpectedNoteTypes()
		{
			return new PredefinedNoteType[]
			{
				EDIPredefinedNoteTypes.Instance.IncidentLog,
				EDIPredefinedNoteTypes.Instance.IncidentDetail,
				PredefinedNoteTypes.Instance.FaxEmailTransmissionLog,
				EDIPredefinedNoteTypes.Instance.IncidentResolutionDetail,
				EDIPredefinedNoteTypes.Instance.IncidentComment,
				EDIPredefinedNoteTypes.Instance.FeatureRequestPrerequisites,
				EDIPredefinedNoteTypes.Instance.FeatureRequestInternalNote,
				EDIPredefinedNoteTypes.Instance.BusinessRequirements,
				EDIPredefinedNoteTypes.Instance.TechnicalSpecification,
				EDIPredefinedNoteTypes.Instance.FeatureRequestSoftwareChangeNote,
				EDIPredefinedNoteTypes.Instance.IncidentClosingStaffCode,
				EDIPredefinedNoteTypes.Instance.IncidentClosureDate,
				EDIPredefinedNoteTypes.Instance.IncidentResolutionComment,
				EDIPredefinedNoteTypes.Instance.IncidentDispositionText
			};
		}

		public void TestIncidentDetailNote()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.DetailNoteText = "";
			incident.ResolutionNoteText = "";
			incident.SetLogTextForTest("");

			incident.DetailNoteText = "Hello Detail Note";
			AssertEquals(true, incident.HasChanges);

			incident.SynchroniseNotes();
			AssertEquals("Hello Detail Note", incident.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.IncidentDetail.Description)[0].ST_NoteText);
			AssertEquals(false, incident.DetailNoteTextInfo.ReadOnly);
		}

		public void TestIncidentResolutionNote()
		{
			SupportIncident incident = Factory.New<SupportIncident>();

			AssertEquals("", incident.ResolutionNoteText);
			string existingNote = incident.ResolutionNoteText;
			incident.ResolutionNoteText = "Something else";
			AssertEquals(true, incident.HasChanges);
			incident.RunPreSaveValidation();
			AssertEquals("Something else", incident.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.IncidentResolutionDetail.Description)[0].ST_NoteText);

			AssertEquals(true, incident.ResolutionNoteTextInfo.ReadOnly);
		}

		public void TestIncidentResolutionNote_ConcurrentAccess()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			SupportIncident incident = newFactory.NewWithValidTestData<SupportIncident>();
			newFactory.Save();

			BusinessObjectFactory newFactory1 = new BusinessObjectFactory();
			newFactory1.RefreshEnabled = false;
			SupportIncident loadedIncident = newFactory1.Load<SupportIncident>(incident.PK);
			string existingNote = loadedIncident.ResolutionNoteText;    //Force to create a new Resolution Note

			AssertEquals("", incident.ResolutionNoteText);
			incident.ResolutionNoteText = "Resolution note 1";
			AssertEquals(true, incident.HasChanges);
			newFactory.Save();
			StmNote[] resolutionNotes = incident.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.IncidentResolutionDetail.Description);
			AssertEquals(1, resolutionNotes.Length);
			AssertEquals("Resolution note 1", resolutionNotes[0].ST_NoteText);

			loadedIncident.ResolutionNoteText = "Resolution note 2";
			newFactory1.Save();

			BusinessObjectFactory newFactory2 = new BusinessObjectFactory();
			newFactory2.RefreshEnabled = false;
			SupportIncident loadedIncident2 = newFactory2.Load<SupportIncident>(incident.PK);
			resolutionNotes = loadedIncident2.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.IncidentResolutionDetail.Description);
			AssertEquals(1, resolutionNotes.Length);
			AssertEquals("Resolution note 2", resolutionNotes[0].ST_NoteText);
		}

		[TestDate(2006, 6, 1, 9, 0, 0)]
		public void TestIncidentLogNote()
		{
			SupportIncident incident = Factory.New<SupportIncident>();

			AssertEquals("", incident.LogText);
			string existingNote = incident.LogText;
			incident.SetLogTextForTest("Haha");
			AssertEquals(true, incident.HasChanges);
			incident.RunPreSaveValidation();
			AssertEquals("Haha", incident.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.IncidentLog.Description)[0].ST_NoteText);

			AssertEquals(true, incident.LogTextInfo.ReadOnly);
		}

		#endregion

		#region PopulateDatabaseCompanyFromClient

		public void TestPopulateDatabaseCompanyFromClient()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "DDD", "COM", "SRV", true);

			var licInactiveDb = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", true);
			licInactiveDb.Database.LD_IsActive = false;
			var licWithoutClientCompany = BillingTestHelper.CreateAnotherDatabase(licInactiveDb, "CCL", false);

			Factory.Save();

			var incident = Factory.New<SupportIncident>();
			var incident2 = Factory.New<SupportIncident>();
			incident.IM_OH_Client = lic.Company.LC_OH;
			incident2.IM_OH_Client = licWithoutClientCompany.Company.LC_OH;

			AssertEquals(lic.Company.LC_LE, incident.EnterprisePK);
			AssertEquals(lic.LA_LD, incident.IM_LD);
			AssertEquals(lic.ClientCompany.PK, incident.IM_LCC);

			AssertEquals(licWithoutClientCompany.Company.LC_LE, incident2.EnterprisePK);
			AssertEquals(licWithoutClientCompany.LA_LD, incident2.IM_LD);
			AssertEquals(ZGuid.Empty, incident2.IM_LCC);
		}

		public void TestUpdateLicenceFromClient_MostRecentBuildFirst()
		{
			var build1 = Factory.New<ReleaseBuild>();
			var build2 = Factory.New<ReleaseBuild>();
			var build3 = Factory.New<ReleaseBuild>();
			build1.VersionNumber = new VersionNumber(1, 4, 6390, 3);
			build2.VersionNumber = new VersionNumber(18, 9, 19, 510);
			build3.VersionNumber = new VersionNumber(18, 9, 20, 1);

			// Org 1 has two databases
			var lic1Org1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1");
			var lic2Org1 = BillingTestHelper.CreateAnotherDatabase(lic1Org1, "DB2");
			var org1 = lic1Org1.Company.Header;

			lic1Org1.Database.LD_HL_CurrentRunningVersion = build1.PK;
			lic2Org1.Database.LD_HL_CurrentRunningVersion = build2.PK;

			// Org 2 has the same two databases from org 1, plus another with more recent build
			var lic1Org2 = BillingTestHelper.CreateAnotherLicence(lic1Org1, "CO2");
			var lic2Org2 = Factory.New<LicenceHeader>();
			lic2Org2.LA_LD = lic2Org1.LA_LD;
			lic2Org2.LA_LC = lic1Org2.LA_LC;
			lic2Org2.LA_AgreedLiveDate = new ZDateTime(2010, 1, 1);
			var lic3Org2 = BillingTestHelper.CreateAnotherDatabase(lic1Org2, "DB3");
			lic3Org2.Database.LD_HL_CurrentRunningVersion = build3.PK;

			var org2 = lic1Org2.Company.Header;

			Factory.Save();

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_OH_Client = org1.PK;

			AssertEquals("DB with most recent build version", lic2Org1.Database.LD_ServerCode, incident.Database.LD_ServerCode);

			incident.IM_OH_Client = org2.PK;
			AssertEquals("DB with most recent build version", lic3Org2.Database.LD_ServerCode, incident.Database.LD_ServerCode);
		}

		#endregion

		#region Implementation

		IBMTestHelper bmTestHelper;
		IBMSystem system;

		protected override void SetUp()
		{
			base.SetUp();

			bmTestHelper = ObjectFactory.Get<IBMTestHelper>();

			bmTestHelper.EnableBMSInRegistry();
			system = bmTestHelper.CreateSystem(Factory, "INC");

			Incident.IM_Description = "IM_Description_TestOnly"; //save 'Incident' before testing to avoid edocs db creation timeout.
			Factory.Save();
			AssertNull(Incident.DocManagerInfo.MasterFactory.GetStorageMainForPK(Incident.PK));
		}

		protected new SupportIncident Incident
		{
			get { return (SupportIncident)base.Incident; }
		}

		#endregion

		#region Incident Details

		protected OrgContact Contact
		{
			get
			{
				if (fContact == null)
				{
					fContact = Org.Contacts.AddNew();
					fContact.OC_ContactName = "Zubin Appoo";
					fContact.OC_Email = "zubin.appoo@edi.com.au";
				}
				return fContact;
			}
		}
		OrgContact fContact;

		protected LicenceDatabase Database
		{
			get
			{
				if (fDatabase == null)
				{
					fDatabase = Factory.New<LicenceDatabase>();
					fDatabase.LD_ServerCode = "SRV";
					fDatabase.LD_LE = Enterprise.PK;
					fDatabase.LD_HL_CurrentRunningVersion = SomeBuild.PK;
					fDatabase.LD_PublicEmailAddressForUpdate = "test@test.com";
				}
				return fDatabase;
			}
		}
		LicenceDatabase fDatabase;

		protected LicenceHeader Header
		{
			get
			{
				if (fHeader == null)
				{
					fHeader = Factory.New<LicenceHeader>();
					fHeader.LA_LC = Company.PK;
					fHeader.LA_LD = Database.PK;
				}
				return fHeader;
			}
		}
		LicenceHeader fHeader;

		protected ClientCompany ClientCompany
		{
			get
			{
				if (clientCompany == null)
				{
					clientCompany = Factory.New<ClientCompany>();
					clientCompany.LCC_LD = Database.PK;
					clientCompany.LCC_Code = "COM";
					clientCompany.LCC_OH = Org.PK;
				}
				return clientCompany;
			}
		}
		ClientCompany clientCompany;

		protected OrgHeader Org
		{
			get
			{
				if (fOrg == null)
				{
					fOrg = Factory.NewWithValidTestData<OrgHeader>();
					fOrg.OH_FullName = "blah blah lola lola";
					fOrg.OH_RL_NKClosestPort = "AUSYD";
				}
				return fOrg;
			}
		}
		OrgHeader fOrg;

		protected LicenceEnterprise Enterprise
		{
			get
			{
				if (fEnterprise == null)
				{
					fEnterprise = Factory.New<LicenceEnterprise>();
					fEnterprise.LE_EnterpriseCode = "ENT";
					fEnterprise.LE_OH = Org.PK;
				}
				return fEnterprise;
			}
		}
		LicenceEnterprise fEnterprise;

		protected LicenceCompany Company
		{
			get
			{
				if (fCompany == null)
				{
					fCompany = Factory.New<LicenceCompany>();
					fCompany.LC_CompanyCode = "COM";
					fCompany.LC_LE = Enterprise.PK;
					fCompany.LC_OH = Org.PK;
				}
				return fCompany;
			}
		}
		LicenceCompany fCompany;

		protected ReleaseBuild SomeBuild
		{
			get
			{
				if (fSomeBuild == null)
				{
					fSomeBuild = Factory.New<ReleaseBuild>();
					fSomeBuild.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);
				}
				return fSomeBuild;
			}
		}
		ReleaseBuild fSomeBuild;

		#endregion

		[TestDate(2015, 1, 19, 3, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestOnFactorySaving_IM_SystemLastEditTimeUtc()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			AssertEquals(TestDateAttribute.Date, incident.IM_SystemLastEditTimeUtc);
		}

		[TestDate(2018, 11, 5, 3, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestOnFactorySaving_Request_INC_SystemLastEditTimeUtc()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			AssertEquals(TestDateAttribute.Date, incident.Request.INC_SystemLastEditTimeUtc);

			TestDateAttribute.AddHours(2);
			incident.EConversation.Conversation.Messages.Add(Factory.NewWithValidTestData<JobConversationMessage>());
			Factory.Save();
			AssertEquals(TestDateAttribute.Date, incident.Request.INC_SystemLastEditTimeUtc);
		}

		public void TestEDocsAddedAndSystemMessageShouldBeSent()
		{
			byte[] eDoc = new byte[] { 1, 1, 1, 1, 1 };

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			var file = ((IDocManagerSupport)incident).DocManagerInfo.AddFileOrDocument(eDoc, "TestPDFPublished.pdf", "MSC");
			file.IsPublished = true;
			Factory.Save();
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Attached to eDocs: TestPDFPublished.pdf"));
		}

		public void TestEConvoForAddedEDocsWhenStatusIsClosed()
		{
			byte[] eDoc = new byte[] { 1, 1, 1, 1, 1 };

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.NoDefectFound, "");
			Factory.Save();

			var file = ((IDocManagerSupport)incident).DocManagerInfo.AddFileOrDocument(eDoc, "TestPDFPublished.pdf", "MSC");
			file.IsPublished = true;
			Factory.Save();
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Attached to eDocs: TestPDFPublished.pdf"));
		}

		public void TestEConvoForAddedEDocsWhenReOpened()
		{
			byte[] eDoc = new byte[] { 1, 1, 1, 1, 1 };

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.NoDefectFound, " ");
			Factory.Save();
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			var file = ((IDocManagerSupport)incident).DocManagerInfo.AddFileOrDocument(eDoc, "TestPDFPublished.pdf", "MSC");
			file.IsPublished = true;
			Factory.Save();
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Attached to eDocs: TestPDFPublished.pdf"));
		}

		public void TestOnFactorySaving()
		{
			byte[] pDFbody = new byte[] { 1, 1, 1, 1, 1 };
			byte[] iMGbody = new byte[] { 0x42, 0x4D, 0x42, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3E, 0x00, 0x00, 0x00, 0x28, 0x00,
										 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00,
										 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
										 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0x80, 0x00,
										 0x00, 0x00 };

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = Org.MainAddress.PK;
			incident.IM_OC_Contact = Contact.PK;
			incident.IM_Description = "Help me";
			incident.DetailNoteText = "Don't know what i'm doing";
			incident.IM_LD = ClientCompany.Database.PK;
			incident.IM_LCC = ClientCompany.PK;
			incident.IM_ClientIncidentReference = "CL111111";
			Factory.Save();

			var newFilePublished = ((IDocManagerSupport)incident).DocManagerInfo.AddFileOrDocument(pDFbody, "TestPDFPublished.pdf", "MSC");
			var newDocumentPublished = ((IDocManagerSupport)incident).DocManagerInfo.AddFileOrDocument(iMGbody, "TestIMGPublished.bmp", "MSC");
			var newFileNotPublished = ((IDocManagerSupport)incident).DocManagerInfo.AddFileOrDocument(pDFbody, "TestPDFNotPublished.pdf", "MSC");
			var newDocumentNotPublished = ((IDocManagerSupport)incident).DocManagerInfo.AddFileOrDocument(iMGbody, "TestIMGNotPublished.bmp", "MSC");

			newFilePublished.IsPublished = true;
			((StorageFile)newFilePublished).SC_Desc = "Yet another dummy description for PDF file";

			newDocumentPublished.IsPublished = true;
			newDocumentPublished.Description = "Yet another dummy description for IMG file";
			var imageData = newDocumentPublished.GetImageDataReader().ConvertToByteArrayAndCloseStream();

			((StorageFile)newFileNotPublished).SC_IsPublished = false;
			newDocumentNotPublished.IsPublished = false;

			Factory.Save();

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Attached to eDocs: TestPDFPublished.pdf, TestIMGPublished.bmp"));

			var targetEmail = Env.OutgoingMailManager.EmailsCreated.Where(x => !x.Subject.Contains("New Messages in")).ToList();
			AssertEquals("Correct email", 2, targetEmail.Count);
			EmailDef emailSys = targetEmail[0];

			// Customer System Notification Email. Mail #1
			AssertEquals(EDIDataRegistry.Instance.IncidentFromEmailAddress.Value, emailSys.FromAddress);
			AssertEquals(Header.Database.LD_PublicEmailAddressForUpdate, emailSys.Recipients[0]);
			AssertEquals("Customer Service Incident Raised - Your Ref: " + incident.IM_ClientIncidentReference, emailSys.Subject);

			EmailDef emailWithAttachedDocs = targetEmail[1];

			// Customer System Email with attached Published Docs. Mail #2
			AssertEquals(EDIDataRegistry.Instance.IncidentFromEmailAddress.Value, emailWithAttachedDocs.FromAddress);
			AssertEquals(Header.Database.LD_PublicEmailAddressForUpdate, emailWithAttachedDocs.Recipients[0]);
			AssertEquals("Customer Service Incident Raised - Your Ref: " + incident.IM_ClientIncidentReference, emailWithAttachedDocs.Subject);

			AssertEquals(1, emailWithAttachedDocs.Attachments.Count);
			AssertEquals("Incident Details.xml", emailWithAttachedDocs.Attachments[0].DisplayName);

			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceResponse));
			Xsd.CustomerServiceResponse response;
			using (MemoryStream ms = new MemoryStream(emailWithAttachedDocs.Attachments[0].Data))
			{
				response = (Xsd.CustomerServiceResponse)serializer.Deserialize(ms);
			}

			AssertEquals(2, response.Attachments.Count);
			Xsd.CustomerServiceResponseAttachment attachmentFile = response.Attachments[0];
			AssertEquals("Filename", "TestPDFPublished.pdf", attachmentFile.FileName);
			AssertEquals("ImageData", pDFbody, attachmentFile.Data);
			AssertEquals("DocType", "MSC", attachmentFile.DocType);
			AssertEquals("Desc", "Yet another dummy description for PDF file", attachmentFile.Desc);

			Xsd.CustomerServiceResponseAttachment attachmentDocument = response.Attachments[1];
			AssertEquals("Filename", "TestIMGPublished.bmp", attachmentDocument.FileName);
			AssertEquals("ImageData", imageData, attachmentDocument.Data);
			AssertEquals("DocType", "MSC", attachmentDocument.DocType);
			AssertEquals("Desc", "Yet another dummy description for IMG file", attachmentDocument.Desc);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			((IDocManagerSupport)incident).DocManagerInfo.Save();
			Factory.Save();

			AssertEquals("No email should be sent yet again because nothing has changed", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			SupportIncident incident1 = Factory.New<SupportIncident>();
			incident1.IM_OH_Client = Org.PK;
			incident1.IM_OC_Contact = Contact.PK;
			incident1.IM_Description = "Help me";
			incident1.DetailNoteText = "Don't know what i'm doing";
			incident1.IM_LD = Database.PK;
			incident1.IM_LCC = Header.ClientCompany.PK;
			incident1.IM_ClientIncidentReference = "";
			Factory.Save();

			var incident1TargetEmail = Env.OutgoingMailManager.EmailsCreated.Where(x => !x.Subject.Contains("New Messages in")).ToList();
			AssertEquals("No email should be sent, because Incident1.IM_ClientIncidentReference = \"\"", 0, incident1TargetEmail.Count);

			Database.LD_PublicEmailAddressForUpdate = "";

			SupportIncident incident2 = Factory.New<SupportIncident>();
			incident2.IM_OH_Client = Org.PK;
			incident2.IM_OC_Contact = Contact.PK;
			incident2.IM_Description = "Help me";
			incident2.DetailNoteText = "Don't know what i'm doing";
			incident2.IM_LD = Database.PK;
			incident2.IM_LCC = Header.ClientCompany.PK;
			incident2.IM_ClientIncidentReference = "CL111111";
			Factory.Save();

			var incident2TargetEmail = Env.OutgoingMailManager.EmailsCreated.Where(x => !x.Subject.Contains("New Messages in")).ToList();
			AssertEquals("No email should be sent, because Database.LD_PublicEmailAddressForUpdate = \"\"", 0, incident2TargetEmail.Count);
		}

		public void TestEConversationForNewAddedEDoc()
		{
			byte[] fileContent = new byte[] { 1, 1, 1, 1, 1 };

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			var file1 = incident.DocManagerInfo.AddFileOrDocument(fileContent, "TestPDFPublished1.pdf", "MSC");
			file1.IsPublished = true;
			Factory.Save();
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Attached to eDocs: TestPDFPublished1.pdf"));

			GlbStaff webuser = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.WebUserCode);
			using (Env.SetTemporaryUserContext(webuser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var file2 = incident.DocManagerInfo.AddFileOrDocument(fileContent, "TestPDFPublished2.pdf", "MSC");
				file2.IsPublished = true;
				Factory.Save();
				messageList = incident.EConversation.GetTimeOrderedMessages();
				AssertEquals(false, messageList.Any(msg => msg.Body == "Attached to eDocs: TestPDFPublished2.pdf"));
			}
		}

		public void TestSetIncidentCloseDate()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			Assert(incident.IM_CloseTimeUtc.IsEmpty);
			Assert(incident.CloseInSupportDate.IsEmpty);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			Factory.Save();
			Assert(incident.IM_CloseTimeUtc.IsEmpty);
			Assert(!incident.CloseInSupportDate.IsEmpty);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.NoDefectFound, "");
			Factory.Save();
			Assert(!incident.IM_CloseTimeUtc.IsEmpty);
			Assert(!incident.CloseInSupportDate.IsEmpty);

			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();
			Assert(incident.IM_CloseTimeUtc.IsEmpty);
			Assert("Reopen as defect, support close date should not be cleared", !incident.CloseInSupportDate.IsEmpty);
		}

		public void TestIncidentComment()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			AssertEquals("", incident.IncidentComment);

			incident.IncidentComment = "some comment";
			AssertEquals("some comment", incident.IncidentComment);

			incident.IncidentComment = "ANOTHER COMMENT";
			Factory.Save();

			var loadFactory = new BusinessObjectFactory();
			SupportIncident loadedIncident = loadFactory.Load<SupportIncident>(incident.PK);
			AssertEquals("ANOTHER COMMENT", loadedIncident.IncidentComment);

			loadedIncident.IncidentComment = "";
			loadFactory.Save();

			incident.IncidentComment = "New Comment";
			loadedIncident.IncidentComment = "New Comment 2";
			Factory.Save();
			loadFactory.Save();

			loadFactory = new BusinessObjectFactory();
			loadedIncident = loadFactory.Load<SupportIncident>(incident.PK);
			AssertEquals("New Comment 2", loadedIncident.IncidentComment); // only latest stays

			ZQuery query = new ZDBOnlyQuery(typeof(StmNote));
			query.AddToFilter(StmNoteSchema.ST_ParentID, incident.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, incident.TableName);
			query.AddToFilter(StmNoteSchema.ST_Description, EDIPredefinedNoteTypes.Instance.IncidentComment.Description);
			StmNote[] notes = new BusinessObjectFactory().Load<StmNote>(query);
			AssertEquals(1, notes.Length);
		}

		public void TestCreateIncident()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			AssertEquals("", incident.IM_Product);
		}

		public void TestCreateIncident_WithWorkflow()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INC";

			var header = template.ProcessHeaders.AddNew();
			header.FH_CompletionStatement = "Investigate";

			var task1 = template.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = header.PK;
			task1.P9_Sequence = 5;
			task1.P9_Type = "INV";
			task1.P9_Description = "Investigate";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task1.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task1.TemplateConditions.TemplateCondition2Value = @"""<IM_Category>"" == ""SUP""";

			Factory.Save();

			SupportIncident incident = null;

			GlbStaff serviceTaskUser = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, User.ServiceUserCode);
			using (Env.SetTemporaryUserContext(serviceTaskUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				incident = Factory.NewWithValidTestData<SupportIncident>();
				Factory.Save();
			}

			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[0].P9_Status);
			AssertEquals("", incident.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals("", incident.IM_GS_NKCustServiceContact);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			task1.P9_G4_RequiredCapability = capability.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(serviceTaskUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				incident = Factory.NewWithValidTestData<SupportIncident>();
				Factory.Save();
			}

			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, incident.WorkflowItems[0].P9_Status);
			AssertEquals(task1.P9_G4_RequiredCapability, incident.WorkflowItems[0].P9_G4_RequiredCapability);
			AssertEquals("", incident.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
		}

		public void TestPopulateFromIssueOccurrence()
		{
			ReleaseBuildContentForLegacyTest.Enable();

			var areas = new CodeDescriptionPairList();
			areas.AddPair("ARC", "Architecture");
			areas.AddPair("INT", "International Logistics");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("OTH", "Other", "ARC", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var legacyMenuSectionMappings = new LegacyModuleMappingCollection(ModuleListType.MenuSection);
			legacyMenuSectionMappings.AddNew("ALL", "All Items", string.Empty, "OTH");
			EDIDataRegistry.Instance.LegacyMenuSectionMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, legacyMenuSectionMappings);

			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();

			var contact = org.Contacts.AddNew();
			var db = org.LicCompany.LicDatabases.AddNew();
			var licence = org.LicCompany.GetHeader(db);
			db.LD_OC_LicenseeAdminContact = contact.PK;
			db.LD_OH_WebAccessOrg = org.PK;
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = org.LicCompany.LC_CompanyCode;
			clientCompany.LCC_LD = db.PK;
			clientCompany.LCC_OH = org.PK;

			var build = Factory.New<ReleaseBuild>();

			var issue = Factory.New<EdiHelpErrorLog>();
			issue.HE_ExceptionMessage = "Muu Muu Muu Muu Muu Muu Muu Muu Muu Muu Muu Muu Muu Muu Muu Muu Boo";
			issue.HE_IssueNumber = "00001234";

			var occurrence = issue.Occurrences.AddNew();
			occurrence.HO_ExceptionID = "E00006789";
			occurrence.HO_ExceptionDateTime = new ZDateTime(2007, 1, 10, 12, 15, 9);
			occurrence.HO_LD = licence.LA_LD;
			occurrence.HO_HL = build.PK;
			occurrence.HO_XMLData = "<EDI_Exception_Report><LoginName>Jackensteinerheim.Beanstalkeruquer</LoginName></EDI_Exception_Report>";

			Factory.Save();

			var incident = Factory.New<SupportIncident>();
			incident.PopulateFromIssueOccurrence(occurrence, licence.ClientCompany);

			CombineAssertions(() =>
			{
				AssertEquals("DetailNoteText", "Muu Muu Muu Muu Muu Muu Muu Muu Muu Muu Muu Muu Muu Muu Muu Muu Boo", incident.DetailNoteText);
				AssertEquals("IM_Category", SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
				AssertEquals("IM_Status", SupportIncidentLookups.Status.Open, incident.IM_Status);
				AssertEquals("IM_Description", "Error E00006789 reported by Jackensteinerheim.Beanstalkeru... on 10-Jan-07 12:15", incident.IM_Description);
				AssertEquals("IM_GS_NKAssignedToCurrent", GlbStaff.CurrentUser.GS_Code, incident.IM_GS_NKAssignedToCurrent);
				AssertEquals("IM_HL_ClientReportedOnVersion", build.PK, incident.IM_HL_ClientReportedOnVersion);
				AssertEquals("IM_LA", licence.ClientCompany.PK, incident.IM_LCC);
				AssertEquals("IM_OC_Contact", contact.PK, incident.IM_OC_Contact);
				AssertEquals("IM_OH_Client", org.PK, incident.IM_OH_Client);
				AssertEquals("IM_Module", "OTH", incident.IM_Module);
				AssertEquals("IM_ProgramArea", "ARC", incident.IM_ProgramArea);
				AssertEquals("IM_Priority", Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround, incident.IM_Priority);
				AssertEquals("IM_Product", ProductTypes.Codes.Enterprise, incident.IM_Product);
				AssertEquals("IM_Source", SupportIncidentLookups.SourceListConstants.IssueManagerReported, incident.IM_Source);
				AssertEquals("IM_ResolutionCode", SupportIncidentLookups.DispositionList.Constants.Working.Assigned, incident.IM_ResolutionCode);
				AssertEquals("AutoMatchDone Log Reference", "Created from issue 00001234", incident.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AutoMatchDone.Code))[0].SL_Reference);
			});

			var workItem = incident.RelatedWorkItems.AddNew();
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = ReleaseRingsLookup.CheckInTaskTypes.First();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals("IM_ResolutionCode", SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);
			AssertEquals("IM_Status", SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("IM_Category", SupportIncidentCategoriesList.Codes.Defect, incident.IM_Category);
		}

		public override void TestPopulateWorkItem()
		{
			ProcessManagement.Business.ProcessManagementRegistry.Instance.DefectWorkItemTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { NewWorkItemLookups.WorkItemTypeConstants.DefectFix });
			var tree = new CodeDescriptionBoolTreeNodeCollection(true, 3, 5);
			tree.AddSystemChildren();
			CodeDescriptionBoolTreeTestHelper.Add(tree, "ENT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "ENT", "XRM");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "ENT", "XRM", "INT");
			CodeDescriptionBoolTreeTestHelper.Add(tree, "ENT", "XRM", "INT", NewWorkItemLookups.WorkItemTypeConstants.DefectFix);
			CodeDescriptionBoolTreeTestHelper.Add(tree, "ENT", "XRM", "INT", NewWorkItemLookups.WorkItemTypeConstants.DefectFix, NewWorkItem.DefaultFixPatchTo);
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			Incident.IM_Product = "ENT";
			Incident.IM_Module = "INT";
			Incident.IM_SourceModuleId = "ABC";
			Incident.IM_Description = "Something hello";
			Incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			Incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");

			NewWorkItem item = Factory.NewWithValidTestData<NewWorkItem>();
			Incident.PopulateWorkItem(item);
			AssertEquals("ENT", item.WKI_WorkItemType);
			AssertEquals("INT", item.WKI_ActivityType);
			AssertEquals(NewWorkItem.DefaultFixPatchTo, item.WKI_Priority);
			AssertEquals("Something hello", item.WKI_Summary);
			AssertEquals(NewWorkItemLookups.WorkItemTypeConstants.DefectFix, item.WKI_ActivitySubtype);

			Incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			Incident.PopulateWorkItem(item);
			AssertEquals(NewWorkItem.DefaultFixPatchTo, item.WKI_Priority);

			Incident.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			Incident.PopulateWorkItem(item);
			AssertEquals("ENT", item.WKI_WorkItemType);
			AssertEquals("INT", item.WKI_ActivityType);
			AssertEquals(NewWorkItem.DefaultFixPatchTo, item.WKI_Priority);
			AssertEquals("Something hello", item.WKI_Summary);
			AssertEquals("", item.WKI_ActivitySubtype);
		}

		public void TestCanCreateWorkItem()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			AssertEquals("Precondition: CanCreateWorkItem is true", true, incident.CanCreateWorkItem);
			NewWorkItem workItem = incident.RelatedWorkItems.AddNew();
			workItem.WKI_WorkItemType = "UDF";
			workItem.WKI_Summary = "test";
			AssertEquals("Should allow adding more work items", true, incident.CanCreateWorkItem);

			SupportIncidentProcessTask task = incident.WorkflowItems.AddNew();
			task.P9_Type = "INV";
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task.P9_Description = "Test";

			Factory.Save();

			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);
			AssertEquals("Should not affected by disposition", true, incident.CanCreateWorkItem);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			AssertEquals(true, incident.CanCreateWorkItem);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.ComplianceRequirement;
			AssertEquals(true, incident.CanCreateWorkItem);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
			AssertEquals(true, incident.CanCreateWorkItem);

			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			AssertEquals(false, incident.CanCreateWorkItem);

			SupportIncident closedIncident = Factory.New<SupportIncident>();
			closedIncident.IM_Status = SupportIncidentLookups.Status.Closed;
			foreach (ICodeDescription stageCodeDesc in new SupportIncidentCategoriesList())
			{
				closedIncident.IM_Category = stageCodeDesc.Code;
				AssertEquals("CanCreateWorkItem when closed and in stage: " + stageCodeDesc.Code, stageCodeDesc.Code != SupportIncidentCategoriesList.Codes.Support, closedIncident.CanCreateWorkItem);
			}
		}

		public void TestHasWorkItemsWithShelfCheckInTask()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			Assert("HasWorkItemsWithShelfCheckInTask", !incident.HasWorkItemsWithShelfCheckInTask);
			NewWorkItem workItem = incident.RelatedWorkItems.AddNew();
			Assert("HasWorkItemsWithShelfCheckInTask", !incident.HasWorkItemsWithShelfCheckInTask);
			var processTask = workItem.WorkflowItems.AddNew();
			processTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			Assert("HasWorkItemsWithShelfCheckInTask", incident.HasWorkItemsWithShelfCheckInTask);
		}

		#region TestIJobInvoicingPlugin

		public void TestEditSecurity()
		{
			IJobInvoicingPlugIn testJob = Factory.New<SupportIncident>();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestIJobInvoicingPlugInDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<SupportIncident>();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		public void TestClient()
		{
			var incident = Factory.New<SupportIncident>();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			incident.IM_OH_Client = org.PK;
			AssertEquals("InvoicingSupporter.Consignee", org, ((IJobInvoicingPlugIn)incident).InvoicingSupporter.Consignee);
			AssertEquals("InvoicingSupporter.Consignee", org, ((IJobInvoicingPlugIn)incident).InvoicingSupporter.Consignee);
		}

		#endregion

		public void TestModuleAndProductAreaChangeLogged()
		{
			CodeDescriptionPairList areas = new CodeDescriptionPairList();
			areas.AddPair("COR", "Core Product");
			areas.AddPair("DOM", "Domestic Logistics");
			areas.AddPair("INT", "International Logistics");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("SAA", "Super Module A", "COR", false);
			product.ModuleMappings.AddNew("SBB", "Super Module B", "DOM", false);
			product.ModuleMappings.AddNew("SDD", "Super Module D", "INT", false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = "CR8";
			incident.ProductArea = "COR";
			incident.IM_Module = "SAA";
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("The log should NOT be added : new incident", false, messageList.Any(msg => msg.Body == "Module changed from"));
			AssertEquals("The log should NOT be added : new incident", false, messageList.Any(msg => msg.Body == "Product Area changed from"));

			Factory.Save();
			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("The log should NOT be added : new incident", false, messageList.Any(msg => msg.Body == "Module changed from"));
			AssertEquals("The log should NOT be added : new incident", false, messageList.Any(msg => msg.Body == "Product Area changed from"));

			incident.ProductArea = "DOM";
			incident.IM_Module = "SBB";
			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("The log should NOT be added : changes not saved yet", false, messageList.Any(msg => msg.Body == "Module changed from"));
			AssertEquals("The log should NOT be added : changes not saved yet", false, messageList.Any(msg => msg.Body == "Product Area changed from"));

			Factory.Save();
			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("The log should be added : incident persistant and changes successfully saved", true, messageList.Any(msg => msg.Body == "Module changed from SAA (Super Module A) to SBB (Super Module B)"));
			AssertEquals("The log should be added : incident persistant and changes successfully saved", true, messageList.Any(msg => msg.Body == "Product Area changed from COR (Core Product) to DOM (Domestic Logistics)"));

			incident.SetLogTextForTest(ZString.Empty);
			incident.ProductArea = "INT";
			incident.IM_Module = "SAA";
			incident.IM_Module = "SBB";
			incident.IM_Module = "SDD";
			AssertEquals("INT", incident.ProductArea);
			AssertEquals("SDD", incident.IM_Module);

			Factory.Save();
			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("The log should omit intermittent changes", false, messageList.Any(msg => msg.Body == "Module changed from SBB (Super Module B) to SAA (Super Module A)"));
			AssertEquals("The log should contain last changes", true, messageList.Any(msg => msg.Body == "Module changed from SBB (Super Module B) to SDD (Super Module D)"));
			AssertEquals("The log should omit intermittent changes", false, messageList.Any(msg => msg.Body == "Product Area changed from DOM (Domestic Logistics) to COR (Core Product)"));
			AssertEquals("The log should contain last changes", true, messageList.Any(msg => msg.Body == "Product Area changed from DOM (Domestic Logistics) to INT (International Logistics)"));
		}

		public void TestLogModuleAndProductAreaChange()
		{
			#region setup licence

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Test Org";
			org.OH_RL_NKClosestPort = "AUSYD";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Samuel";
			contact.OC_Email = "sam@test.com.au";

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;
			Factory.Save();

			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			Factory.Save();

			#endregion

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("CCC", "CCC Description", ProductAreaList.Codes.ARC, false);
			product.ModuleMappings.AddNew("HHH", "HHH Description", ProductAreaList.Codes.ARC, false);
			product.ModuleMappings.AddNew("JJJ", "JJJ Description", ProductAreaList.Codes.CUS, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_ClientIncidentReference = "SR0023492";

			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = "CCC";
			incident.IM_Priority = "CR8";
			Factory.Save();

			incident.IM_Module = "HHH";
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			AssertEquals("HHH", incident.IM_Module);
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Module changed from CCC (CCC Description) to HHH (HHH Description)"));
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			incident.IM_Module = "JJJ";
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			AssertEquals("CUS", incident.ProductArea);
			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Product Area changed from ARC (Architecture) to CUS (Customs Compliance)"));
			AssertEquals(true, messageList.Any(msg => msg.Body == "Module changed from HHH (HHH Description) to JJJ (JJJ Description)"));
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#region Property Change Triggers Event

		//TODO uncomment when form is in accessible DLL
		// Ensure that forms do not appear during save transactions, such as SuspendedTaskForm
		//[ExpectNoExceptions, GuiTest]
		//public void TestProductAreaChangeTriggersEvent()
		//{
		//	SupportIncident existingIncident = Factory.NewWithValidTestData<SupportIncident>();
		//	var task = existingIncident.WorkflowItems.AddNew();
		//	// use current user so the next task can be closed during the trigger
		//	task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
		//	task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

		//	CodeDescriptionPairList areas = new CodeDescriptionPairList();
		//	areas.AddPair("ARC", "Architecture");
		//	areas.AddPair("INT", "International Logistics");
		//	areas.AddPair("XRM", "XRMs");
		//	EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

		//	var collection = new SystemProductCollection();
		//	var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
		//	product.ModuleMappings.AddNew("SAA", "Super Module A", "ARC", false);
		//	product.ModuleMappings.AddNew("SBB", "Super Module B", "INT", false);
		//	product.ModuleMappings.AddNew("SCC", "Super Module C", "XRM", false);
		//	EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

		//	#region Template 1

		//	var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
		//	template1.P0_ProcessType = "INC";
		//	template1.P0_SubType4 = "ARC";

		//	var header1 = template1.ProcessHeaders.AddNew();
		//	header1.FH_CompletionStatement = "Level 1 Support";

		//	var task11 = template1.WorkflowItems.AddNew();
		//	task11.P9_FH_ProcessHeader = header1.PK;
		//	task11.P9_Sequence = 10;
		//	task11.P9_Description = "ARC Task 1";
		//	task11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

		//	var task12 = template1.WorkflowItems.AddNew();
		//	task12.P9_FH_ProcessHeader = header1.PK;
		//	task12.P9_Sequence = 20;
		//	task12.P9_Description = "ARC Task 2";
		//	task12.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

		//	#endregion

		//	#region Template 2

		//	var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
		//	template2.P0_ProcessType = "INC";
		//	template2.P0_SubType4 = "INT";

		//	var header2 = template2.ProcessHeaders.AddNew();
		//	header2.FH_CompletionStatement = "Level 1 Support";

		//	var task21 = template2.WorkflowItems.AddNew();
		//	task21.P9_FH_ProcessHeader = header2.PK;
		//	task21.P9_Sequence = 10;
		//	task21.P9_Description = "INT Task 1";
		//	task21.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

		//	var task22 = template2.WorkflowItems.AddNew();
		//	task22.P9_FH_ProcessHeader = header2.PK;
		//	task22.P9_Sequence = 20;
		//	task22.P9_Description = "INT Task 2";
		//	task22.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

		//	#endregion

		//	#region Template 3

		//	var template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
		//	template3.P0_ProcessType = "INC";

		//	var header3 = template3.ProcessHeaders.AddNew();
		//	header3.FH_CompletionStatement = "Default Support";

		//	var task3 = template3.WorkflowItems.AddNew();
		//	task3.P9_FH_ProcessHeader = header3.PK;
		//	task3.P9_Sequence = 10;
		//	task3.P9_Description = "GEN Task 1";
		//	task3.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

		//	#endregion

		//	Factory.Save();

		//	var org = Factory.NewWithValidTestData<OrgHeader>();
		//	var contact = org.Contacts.AddNew();
		//	contact.OC_Email = "sam@test.inc";
		//	SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
		//	incident.IM_OH_Client = org.PK;
		//	incident.IM_OC_Contact = contact.PK;
		//	incident.IM_Product = ProductTypes.Codes.Enterprise;

		//	using (SupportIncidentForm form = new SupportIncidentForm(incident))
		//	{
		//		form.Show();
		//		TasksControl tasksControl = new TasksControl();
		//		form.Controls.Add(tasksControl);

		//		incident.IM_Module = "SAA";
		//		Factory.Save();

		//		// bind the control after the tasks have been loaded, not before
		//		tasksControl.SetDataBinding(incident, "");

		//		AssertEquals(2, incident.WorkflowItems.Count);
		//		AssertEquals("ARC Task 1", incident.WorkflowItems[0].P9_Description);
		//		AssertEquals("ARC Task 2", incident.WorkflowItems[1].P9_Description);
		//		AssertEquals(10, incident.WorkflowItems[0].P9_Sequence);
		//		AssertEquals(20, incident.WorkflowItems[1].P9_Sequence);
		//		AssertEquals("Level 1 Support", incident.WorkflowItems[0].ProcessHeader.FH_CompletionStatement);
		//		AssertEquals("Level 1 Support", incident.WorkflowItems[1].ProcessHeader.FH_CompletionStatement);

		//		incident.WorkflowItems[0].P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
		//		incident.WorkflowItems[0].P9_Status = ProcessTaskStatusCodeList.Codes.Working;
		//		Factory.Save();

		//		incident.ProductArea = "INT";
		//		incident.IM_Module = "SBB";
		//		incident.AddStaffMessageToCustomer("EConversation will trigger another Factory save during OnSaveSucceeded");
		//		Factory.Save();

		//		AssertEquals(4, incident.WorkflowItems.Count);
		//		AssertEquals("INT Task 1", incident.WorkflowItems[2].P9_Description);
		//		AssertEquals("INT Task 2", incident.WorkflowItems[3].P9_Description);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, incident.WorkflowItems[0].P9_Status);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, incident.WorkflowItems[1].P9_Status);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[2].P9_Status);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[3].P9_Status);

		//		incident.ProductArea = "ARC";
		//		incident.IM_Module = "SAA";
		//		Factory.Save();

		//		AssertEquals(6, incident.WorkflowItems.Count);
		//		AssertEquals("ARC Task 1", incident.WorkflowItems[4].P9_Description);
		//		AssertEquals("ARC Task 2", incident.WorkflowItems[5].P9_Description);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, incident.WorkflowItems[0].P9_Status);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, incident.WorkflowItems[1].P9_Status);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, incident.WorkflowItems[2].P9_Status);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, incident.WorkflowItems[3].P9_Status);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[4].P9_Status);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[5].P9_Status);

		//		incident.ProductArea = "XRM";
		//		incident.IM_Module = "SCC";
		//		Factory.Save();

		//		AssertEquals(7, incident.WorkflowItems.Count);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, incident.WorkflowItems[0].P9_Status);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, incident.WorkflowItems[1].P9_Status);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, incident.WorkflowItems[2].P9_Status);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, incident.WorkflowItems[3].P9_Status);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, incident.WorkflowItems[4].P9_Status);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, incident.WorkflowItems[5].P9_Status);
		//		AssertEquals(ProcessTaskStatusCodeList.Codes.Open, incident.WorkflowItems[6].P9_Status);

		//		incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");
		//		Factory.Save();
		//		incident.ProductArea = "ARC";
		//		incident.IM_Module = "SAA";
		//		Factory.Save();
		//		AssertEquals("Close incident should not trigger product area change event", 7, incident.WorkflowItems.Count);

		//		incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
		//		incident.ProductArea = "INT";
		//		incident.IM_Module = "SBB";
		//		incident.WorkflowItems.RemoveAndDeleteAll();
		//		new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(incident);
		//		Factory.Save();
		//		AssertEquals("Tasks are created manually, should not duplicate even change product area event is triggered", 2, incident.WorkflowItems.Count);
		//		AssertEquals("INT Task 1", incident.WorkflowItems[0].P9_Description);
		//		AssertEquals("INT Task 2", incident.WorkflowItems[1].P9_Description);
		//	}
		//}

		public void TestProductAreaChangeTriggersEvent_ReTriggerLastEvent()
		{
			SetupIncidentProductAndProductAreas();

			SupportIncident existingIncident = Factory.NewWithValidTestData<SupportIncident>();

			CodeDescriptionPairList areas = new CodeDescriptionPairList();
			areas.AddPair("ARC", "Architecture");
			areas.AddPair("INT", "International Logistics");
			areas.AddPair("XRM", "XRMs");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("SAA", "Super Module A", "ARC", false);
			product.ModuleMappings.AddNew("SBB", "Super Module B", "INT", false);
			product.ModuleMappings.AddNew("SCC", "Super Module C", "XRM", false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			#region Template 1

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_SubType4 = "ARC";

			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Level 1 Support";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Description = "ARC Task 1";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task12 = template1.WorkflowItems.AddNew();
			task12.P9_FH_ProcessHeader = header1.PK;
			task12.P9_Sequence = 20;
			task12.P9_Description = "ARC Task 2";
			task12.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			#endregion

			#region Event Template 1

			var templateOrg = Factory.NewWithValidTestData<OrgHeader>();
			EDIDataRegistry.Instance.IncidentEventWorkflowTemplateClientOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, templateOrg.PK.ToGuid());

			var eventTemplate1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			eventTemplate1.P0_ProcessType = "INC";
			eventTemplate1.P0_SubType4 = "ARC";
			eventTemplate1.P0_OH_Client = templateOrg.PK;

			var eventHeader1 = eventTemplate1.ProcessHeaders.AddNew();
			eventHeader1.FH_CompletionStatement = "ESC Escalate";

			var eventTask1 = eventTemplate1.WorkflowItems.AddNew();
			eventTask1.P9_FH_ProcessHeader = eventHeader1.PK;
			eventTask1.P9_Sequence = 10;
			eventTask1.P9_Description = "ESC ARC Task 1";
			eventTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			#endregion

			#region Event Template 2

			var eventTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			eventTemplate2.P0_ProcessType = "INC";
			eventTemplate2.P0_SubType4 = "INT";
			eventTemplate2.P0_OH_Client = templateOrg.PK;

			var eventHeader2 = eventTemplate2.ProcessHeaders.AddNew();
			eventHeader2.FH_CompletionStatement = "ESC Escalate";

			var eventTask2 = eventTemplate2.WorkflowItems.AddNew();
			eventTask2.P9_FH_ProcessHeader = eventHeader2.PK;
			eventTask2.P9_Sequence = 10;
			eventTask2.P9_Description = "ESC INT Task 1";
			eventTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			#endregion

			#region Event Template 3

			var eventTemplate3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			eventTemplate3.P0_ProcessType = "INC";
			eventTemplate3.P0_SubType4 = "XRM";
			eventTemplate3.P0_OH_Client = templateOrg.PK;

			var eventHeader3 = eventTemplate3.ProcessHeaders.AddNew();
			eventHeader3.FH_CompletionStatement = "ESC Escalate";

			var eventTask3 = eventTemplate3.WorkflowItems.AddNew();
			eventTask3.P9_FH_ProcessHeader = eventHeader3.PK;
			eventTask3.P9_Sequence = 10;
			eventTask3.P9_Description = "ESC XRM Task 1";
			eventTask3.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			#endregion

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.inc";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Module = "SAA";
			incident.IM_Priority = "CR4";
			Factory.Save();

			AssertEquals(2, incident.WorkflowItems.Count);
			AssertEquals("ARC Task 1", incident.WorkflowItems[0].P9_Description);
			AssertEquals("ARC Task 2", incident.WorkflowItems[1].P9_Description);
			AssertEquals(10, incident.WorkflowItems[0].P9_Sequence);
			AssertEquals(20, incident.WorkflowItems[1].P9_Sequence);
			AssertEquals("Level 1 Support", incident.WorkflowItems[0].ProcessHeader.FH_CompletionStatement);
			AssertEquals("Level 1 Support", incident.WorkflowItems[1].ProcessHeader.FH_CompletionStatement);

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			AssertEquals(3, incident.WorkflowItems.Count);
			AssertEquals("ESC ARC Task 1", incident.WorkflowItems[2].P9_Description);

			incident.ProductArea = "INT";
			incident.IM_Module = "SBB";
			AssertEquals(3, incident.WorkflowItems.Count);
			AssertEquals("ESC INT Task 1", incident.WorkflowItems[2].P9_Description);

			incident.ProductArea = "XRM";
			incident.IM_Module = "SCC";
			AssertEquals(3, incident.WorkflowItems.Count);
			AssertEquals("ESC XRM Task 1", incident.WorkflowItems[2].P9_Description);

			incident.ProductArea = "ARC";
			incident.IM_Module = "SAA";
			AssertEquals(3, incident.WorkflowItems.Count);
			AssertEquals("ESC ARC Task 1", incident.WorkflowItems[2].P9_Description);

			Factory.Save();
			incident.ProductArea = "INT";
			incident.IM_Module = "SBB";
			AssertEquals(4, incident.WorkflowItems.Count);
			AssertEquals("ESC INT Task 1", incident.WorkflowItems[3].P9_Description);
		}

		public void TestProductAreaChangeTriggersEvent_IncidentClose()
		{
			SetupIncidentProductAndProductAreas();

			#region Template 1

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_SubType4 = "ARC";

			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Level 1 Support";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Description = "ARC Task 1";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			#endregion

			#region Template 2

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "INC";
			template2.P0_SubType4 = "INT";

			var header2 = template2.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Level 1 Support";

			var task21 = template2.WorkflowItems.AddNew();
			task21.P9_FH_ProcessHeader = header2.PK;
			task21.P9_Sequence = 10;
			task21.P9_Description = "INT Task 1";
			task21.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			#endregion

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.inc";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.ProductArea = "ARC";
			incident.IM_Module = "SAA";
			incident.IM_Language = "EN";
			Factory.Save();

			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals("ARC Task 1", incident.WorkflowItems[0].P9_Description);
			AssertEquals(10, incident.WorkflowItems[0].P9_Sequence);

			incident.ProductArea = "XRM";
			incident.IM_Module = "SCC";
			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			Assert(incident.IM_CloseTimeUtc.IsEmpty);

			incident.ProductArea = "INT";
			incident.IM_Module = "SBB";
			AssertEquals("Task is added because incident has no closed date - change to close status is not yet saved", 2, incident.WorkflowItems.Count);
			AssertEquals("INT Task 1", incident.WorkflowItems[1].P9_Description);
			AssertEquals(20, incident.WorkflowItems[1].P9_Sequence);

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, "");

			Factory.Save();

			incident.ProductArea = "ARC";
			incident.IM_Module = "SAA";
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			Assert(!incident.IM_CloseTimeUtc.IsEmpty);
			AssertEquals("No task is added because incident is closed and closed date has value", 2, incident.WorkflowItems.Count);
		}

		public void TestProductChangeTriggersEvent()
		{
			SetupIncidentProductAndProductAreas();

			#region Template 1

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_SubType1 = ProductTypes.Codes.Enterprise;

			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Level 1 Support";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Description = "ENT Task 1";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			#endregion

			#region Template 2

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "INC";
			template2.P0_SubType1 = ProductTypes.Codes.GLOW;

			var header2 = template2.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Level 1 Support";

			var task21 = template2.WorkflowItems.AddNew();
			task21.P9_FH_ProcessHeader = header2.PK;
			task21.P9_Sequence = 10;
			task21.P9_Description = "GLW Task 1";
			task21.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			#endregion

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.inc";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.ProductArea = "ARC";
			incident.IM_Module = "SAA";
			incident.IM_Language = "EN";
			Factory.Save();

			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals("ENT Task 1", incident.WorkflowItems[0].P9_Description);
			AssertEquals(10, incident.WorkflowItems[0].P9_Sequence);

			incident.IM_Product = ProductTypes.Codes.GLOW;
			AssertEquals("Task from best matched template is added", 2, incident.WorkflowItems.Count);
			AssertEquals("GLW Task 1", incident.WorkflowItems[1].P9_Description);
			AssertEquals(20, incident.WorkflowItems[1].P9_Sequence);

			incident.ProductArea = "XRM";
			AssertEquals("No new task as best matched template has applied", 2, incident.WorkflowItems.Count);
		}

		public void TestModuleChangeTriggersEvent()
		{
			SetupIncidentProductAndProductAreas();

			#region Template 1

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_SubType1 = ProductTypes.Codes.Enterprise;

			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Level 1 Support";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Description = "SAA Task 1";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task11.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task11.TemplateConditions.TemplateCondition2Value = @"""<IM_Module>"" == ""SAA""";

			var task12 = template1.WorkflowItems.AddNew();
			task12.P9_FH_ProcessHeader = header1.PK;
			task12.P9_Sequence = 10;
			task12.P9_Description = "SBB Task 1";
			task12.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task12.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task12.TemplateConditions.TemplateCondition2Value = @"""<IM_Module>"" == ""SBB""";

			#endregion

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.inc";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.ProductArea = "ARC";
			incident.IM_Module = "SAA";
			incident.IM_Language = "EN";
			Factory.Save();

			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals("SAA Task 1", incident.WorkflowItems[0].P9_Description);
			AssertEquals(10, incident.WorkflowItems[0].P9_Sequence);

			incident.ProductArea = "INT";
			incident.IM_Module = "SBB";
			AssertEquals("Task from best matched template is added", 2, incident.WorkflowItems.Count);
			AssertEquals("SBB Task 1", incident.WorkflowItems[1].P9_Description);
			AssertEquals(20, incident.WorkflowItems[1].P9_Sequence);

			incident.IM_SourceModuleId = "123";
			AssertEquals("No new task as best matched template has applied", 2, incident.WorkflowItems.Count);
		}

		public void TestMenuSectionChangeTriggersEvent()
		{
			SetupIncidentProductAndProductAreas();

			#region Template 1

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_SubType1 = ProductTypes.Codes.Enterprise;

			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Level 1 Support";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Description = "MAA Task 1";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task11.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task11.TemplateConditions.TemplateCondition2Value = @"""<IM_SourceModuleId>"" == ""MAA""";

			var task12 = template1.WorkflowItems.AddNew();
			task12.P9_FH_ProcessHeader = header1.PK;
			task12.P9_Sequence = 10;
			task12.P9_Description = "MBB Task 1";
			task12.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task12.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task12.TemplateConditions.TemplateCondition2Value = @"""<IM_SourceModuleId>"" == ""MBB""";

			#endregion

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.inc";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.ProductArea = "ARC";
			incident.IM_Module = "SAA";
			incident.IM_SourceModuleId = "MAA";
			incident.IM_Language = "EN";
			Factory.Save();

			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals("MAA Task 1", incident.WorkflowItems[0].P9_Description);
			AssertEquals(10, incident.WorkflowItems[0].P9_Sequence);

			incident.IM_SourceModuleId = "MBB";
			AssertEquals("Task from best matched template is added", 2, incident.WorkflowItems.Count);
			AssertEquals("MBB Task 1", incident.WorkflowItems[1].P9_Description);
			AssertEquals(20, incident.WorkflowItems[1].P9_Sequence);

			incident.IM_Module = "SBB";
			AssertEquals("No new task as best matched template has applied", 2, incident.WorkflowItems.Count);
		}

		public void TestCountryChangeTriggersEvent()
		{
			SetupIncidentProductAndProductAreas();

			#region Template 1

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_SubType1 = ProductTypes.Codes.Enterprise;

			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Level 1 Support";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Description = "AU Task 1";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task11.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task11.TemplateConditions.TemplateCondition2Value = @"""<IM_RN_NKCountry>"" == ""AU""";

			var task12 = template1.WorkflowItems.AddNew();
			task12.P9_FH_ProcessHeader = header1.PK;
			task12.P9_Sequence = 10;
			task12.P9_Description = "Non AU Task 1";
			task12.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task12.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task12.TemplateConditions.TemplateCondition2Value = @"""<IM_RN_NKCountry>"" != ""AU""";

			#endregion

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.inc";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.ProductArea = "ARC";
			incident.IM_Module = "SAA";
			incident.IM_RN_NKCountry = "AU";
			Factory.Save();

			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals("AU Task 1", incident.WorkflowItems[0].P9_Description);
			AssertEquals(10, incident.WorkflowItems[0].P9_Sequence);

			incident.IM_RN_NKCountry = "NZ";
			AssertEquals("Task from best matched template is added", 2, incident.WorkflowItems.Count);
			AssertEquals("Non AU Task 1", incident.WorkflowItems[1].P9_Description);
			AssertEquals(20, incident.WorkflowItems[1].P9_Sequence);

			incident.IM_Module = "SBB";
			AssertEquals("No new task as best matched template has applied", 2, incident.WorkflowItems.Count);
		}

		public void TestLanguageChangeTriggersEvent()
		{
			SetupIncidentProductAndProductAreas();

			#region Template 1

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_SubType1 = ProductTypes.Codes.Enterprise;
			template1.P0_SubType5 = "ZH-CN";

			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Level 1 Support";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Description = "CHS Task 1";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			#endregion

			#region Template 2

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "INC";
			template2.P0_SubType1 = ProductTypes.Codes.Enterprise;
			template2.P0_SubType5 = string.Empty;

			var header2 = template2.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Level 1 Support";

			var task21 = template2.WorkflowItems.AddNew();
			task21.P9_FH_ProcessHeader = header2.PK;
			task21.P9_Sequence = 10;
			task21.P9_Description = "All languages Task 1";
			task21.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			#endregion

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.inc";
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_OH_Client = org.PK;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";
			incident.ProductArea = "ARC";
			incident.IM_Module = "SAA";
			incident.IM_Language = "ZH-CN";
			Factory.Save();

			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals("CHS Task 1", incident.WorkflowItems[0].P9_Description);
			AssertEquals(10, incident.WorkflowItems[0].P9_Sequence);

			incident.IM_Language = "EN";
			AssertEquals("Task from best matched template is added", 2, incident.WorkflowItems.Count);
			AssertEquals("All languages Task 1", incident.WorkflowItems[1].P9_Description);
			AssertEquals(20, incident.WorkflowItems[1].P9_Sequence);

			incident.ProductArea = "XRM";
			AssertEquals("No new task as best matched template has applied", 2, incident.WorkflowItems.Count);
		}

		void SetupIncidentProductAndProductAreas()
		{
			CodeDescriptionPairList areas = new CodeDescriptionPairList();
			areas.AddPair("ARC", "Architecture");
			areas.AddPair("INT", "International Logistics");
			areas.AddPair("XRM", "XRMs");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			var collection = new SystemProductCollection();
			var product1 = collection.AddNew(ProductTypes.Codes.Enterprise, "CW1", true);
			product1.ModuleMappings.AddNew("SAA", "Super Module A", "ARC", false);
			product1.ModuleMappings.AddNew("SBB", "Super Module B", "INT", false);
			product1.ModuleMappings.AddNew("SCC", "Super Module C", "XRM", false);
			var product2 = collection.AddNew(ProductTypes.Codes.GLOW, "Glow", true);
			product2.ModuleMappings.AddNew("SAA", "Super Module A", "ARC", false);
			product2.ModuleMappings.AddNew("SBB", "Super Module B", "INT", false);
			product2.ModuleMappings.AddNew("SCC", "Super Module C", "XRM", false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		#endregion

		[TestDate(2009, 2, 3, 4, 4, 4)]
		public void TestInitialStaffAssignmentDate()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();

			Factory.Save();
			AssertEquals(ZDateTime.Empty, incident.InitialStaffAssignmentDate);
			AssertEquals(ZDateTime.Empty, incident2.InitialStaffAssignmentDate);

			incident.AssignAndInvestigateInSupport(GlbStaff.CurrentUser);
			incident2.InvestigateInSupport();
			Factory.Save();
			AssertEquals(ZDateTime.Now, incident.InitialStaffAssignmentDate);
			AssertEquals(ZDateTime.Now, incident2.InitialStaffAssignmentDate);

			SupportIncident incident5 = Factory.NewWithValidTestData<SupportIncident>();
			SupportIncident incident6 = Factory.NewWithValidTestData<SupportIncident>();
			incident5.AssignAndInvestigateInSupport(GlbStaff.CurrentUser);
			incident6.InvestigateInSupport();
			Factory.Save();
			AssertEquals(ZDateTime.Now, incident5.InitialStaffAssignmentDate);
			AssertEquals(ZDateTime.Now, incident6.InitialStaffAssignmentDate);
		}

		[TestDate(2015, 1, 19, 3, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestChangingsNotesUpdatesAuditFields_IM_SystemLastEditTimeUtc()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.SetLogTextForTest("1");
			Factory.Save();
			AssertEquals(TestDateAttribute.Date, incident.IM_SystemLastEditTimeUtc);
		}

		public void TestChangingsNotesUpdatesAuditFields()
		{
			SupportIncident incident = (SupportIncident)GetNewBusinessObject();
			AssertEquals("", incident.DetailNoteText);
			incident.DetailNoteText = "1";
			Assert(incident.IM_SystemLastEditTimeUtc.IsEmpty);
			incident.RunPreSaveValidation();
			Assert(incident.IM_SystemLastEditTimeUtc.IsValid);
			incident.IM_SystemLastEditTimeUtc = ZDateTime.Empty;
			Factory.Save();
			Assert(incident.IM_SystemLastEditTimeUtc.IsValid);

			GlbStaff.CurrentUser.GS_Code = "U2";
			incident.DetailNoteText = "2";
			incident.RunPreSaveValidation();
			Factory.Save();
			AssertEquals(incident.IM_SystemLastEditUser, "U2");

			GlbStaff.CurrentUser.GS_Code = "U3";
			incident.DetailNoteText = "3";
			incident.RunPreSaveValidation();
			Factory.Save();
			AssertEquals(incident.IM_SystemLastEditUser, "U3");

			GlbStaff.CurrentUser.GS_Code = "U4";
			incident.ResolutionNoteText = "4";
			incident.RunPreSaveValidation();
			Factory.Save();
			AssertEquals(incident.IM_SystemLastEditUser, "U4");

			GlbStaff.CurrentUser.GS_Code = "NON";
			incident.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("no change", incident.IM_SystemLastEditUser, "U4");
		}

		[TestDate(2012, 5, 1, 15, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestEscalationLogShouldBeSaved()
		{
			#region Setup

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			org.OH_FullName = "blah blah lola lola";
			org.OH_RL_NKClosestPort = "AUSYD";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Samuel Wang";
			contact.OC_Email = "samuel.wang@cargowise.com";

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "ENT";
			enterprise.LE_OH = org.PK;
			Factory.Save();

			ReleaseBuild someBuild = Factory.New<ReleaseBuild>();
			someBuild.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_HL_CurrentRunningVersion = someBuild.PK;
			database.LD_PublicEmailAddressForUpdate = "test@test.com";

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = Org.MainAddress.PK;
			incident.IM_OC_Contact = Contact.PK;
			incident.IM_Description = "Help me";
			incident.DetailNoteText = "Don't know what i'm doing";
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_ClientIncidentReference = "SR00001034";
			Factory.Save();

			#endregion

			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "Incident is closed.");
			Factory.Save();

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Escalated as Defect" && msg.MessageType == MessageType.LocalPublished));
			AssertEquals(true, messageList.Any(msg => msg.Body == "Incident is closed." && msg.MessageType == MessageType.LocalInternal));

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			SupportIncident loadedIncident = newFactory.Load<SupportIncident>(incident.PK);
			messageList = loadedIncident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Escalated as Defect" && msg.MessageType == MessageType.LocalPublished));
			AssertEquals(true, messageList.Any(msg => msg.Body == "Incident is closed." && msg.MessageType == MessageType.LocalInternal));

			loadedIncident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "Defect is fixed");
			messageList = loadedIncident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Closed As Completed" && msg.MessageType == MessageType.LocalPublished));
			AssertEquals(true, messageList.Any(msg => msg.Body == "Defect is fixed" && msg.MessageType == MessageType.LocalPublished));
		}

		public void TestWorkflowItems_CountChanged()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_IncidentNumber = "CS00001130";
			incident.IM_Description = "I'm an incident";
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.IM_Status = SupportIncidentLookups.Status.Working;
			var task1 = incident.WorkflowItems.Tasks.AddNew();
			var task2 = incident.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			AssertEquals(false, incident.HasChanges);

			incident.WorkflowItems.Load();
			AssertEquals(false, incident.HasChanges);
		}

		public void TestGetCurrentCustomerSystemStatusCode_FeatureRequest()
		{
			var helper = new SupportIncidentTestHelper(Factory);

			var incident1 = helper.CreateIncidentWithLegacyClient();

			incident1.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.PendingFeatureResult, incident1.GetCurrentCustomerSystemStatusCode());

			incident1.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.Closed, incident1.GetCurrentCustomerSystemStatusCode());

			var incident2 = helper.CreateIncidentWithBidirectionalUpdateClient();

			incident2.Escalate(SupportIncidentCategoriesList.Codes.FeatureRequest, "");
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.PendingFeatureResult, incident2.GetCurrentCustomerSystemStatusCode());

			incident2.CloseIncident(SupportIncidentLookups.DispositionList.Constants.FeatureAccepted, "");
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.FeatureRequestAccepted, incident2.GetCurrentCustomerSystemStatusCode());

			incident2.CloseIncident(SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided, "");
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.DevelopmentEstimateProvided, incident2.GetCurrentCustomerSystemStatusCode());

			incident2.CloseIncident(SupportIncidentLookups.DispositionList.Constants.FormalQuotationProvided, "");
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.FormalQuotationProvided, incident2.GetCurrentCustomerSystemStatusCode());

			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate;
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.DevelopmentEstimateRequested, incident2.GetCurrentCustomerSystemStatusCode());

			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.AwaitingFormalQuotation;
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.FormalQuotationRequested, incident2.GetCurrentCustomerSystemStatusCode());

			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.FormalQuotationAccepted;
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.FormalQuotationAccepted, incident2.GetCurrentCustomerSystemStatusCode());

			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.FormalQuotationDeclined;
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.FormalQuotationDeclined, incident2.GetCurrentCustomerSystemStatusCode());

			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated;
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.DevelopmentTeam, incident2.GetCurrentCustomerSystemStatusCode());

			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.DevelopmentTeam, incident2.GetCurrentCustomerSystemStatusCode());

			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.Resolved, incident2.GetCurrentCustomerSystemStatusCode());

			incident2.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.Closed, incident1.GetCurrentCustomerSystemStatusCode());

			incident2.Reopen("");
			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress;
			AssertEquals(SupportIncidentLookups.LegacyStatusCodes.PendingFeatureResult, incident2.GetCurrentCustomerSystemStatusCode());
		}

		#region IARInvoiceSavingNotificationSubscriber

		public void TestGetRecipientsForNotification()
		{
			GlbGroup incidentGroupManager = GetGroupWithStaffWithEmailAndTitle("MYGROUP", "manager@test.com", "MGR");
			EDIDataRegistry.Instance.IncidentFeatureRequestGroupENT.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, incidentGroupManager.PK.ToGuid());
			Factory.Save();

			IARInvoiceSavingNotificationSubscriber subscriber = Incident;

			AssertEquals("One recipient: ", 1, subscriber.GetRecipientsForNotification().Length);
			AssertEquals("Should contain group manager email address: ", "manager@test.com", subscriber.GetRecipientsForNotification()[0]);
		}

		GlbGroup GetGroupWithStaffWithEmailAndTitle(ZString groupCode, ZString staffEmail, ZString staffTitle)
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = groupCode;
			group.GG_Desc = "Some Group iN the WOrld";
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Maaaah baah haaah Lola";
			staff.GS_EmailAddress = staffEmail;

			GlbGroupLink link = Factory.New<GlbGroupLink>();
			link.GK_GG = group.PK;
			link.GK_GS = staff.PK;
			link.GK_MembershipType = staffTitle;
			return group;
		}

		#endregion

		#region Workflow Related

		public void TestCalculateStatusAndDisposition()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();

			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);

			var task1 = incident.WorkflowItems.Tasks.AddNew();
			task1.P9_Sequence = 10;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);

			task1.P9_G4_RequiredCapability = capability.PK;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction, incident.IM_ResolutionCode);

			task1.P9_G4_RequiredCapability = ZGuid.Empty;
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction, incident.IM_ResolutionCode);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var task2 = incident.WorkflowItems.Tasks.AddNew();
			task2.P9_Sequence = 20;
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction, incident.IM_ResolutionCode);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction, incident.IM_ResolutionCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(SupportIncidentLookups.Status.Suspended, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Suspended.Deferred, incident.IM_ResolutionCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, incident.IM_ResolutionCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ResolutionCode);
		}

		public void TestCalculateStatusAndDisposition_TemplateCreatedTask()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = incident.WorkflowItems.WorkflowType;

			var templateTask1 = workflowTemplate.WorkflowItems.Tasks.AddNew();
			templateTask1.P9_Sequence = 10;
			templateTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			workflowTemplate.Factory.Save();

			incident.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates(new[] { workflowTemplate }));

			var workflowTask = incident.WorkflowItems.Tasks.Cast<ProcessTask>().FirstOrDefault(x => x.SourceTemplatePK == workflowTemplate.PK);

			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction, incident.IM_ResolutionCode);

			var task1 = incident.WorkflowItems.Tasks[0];
			task1.P9_G4_RequiredCapability = capability.PK;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction, incident.IM_ResolutionCode);

			task1.P9_G4_RequiredCapability = ZGuid.Empty;
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction, incident.IM_ResolutionCode);
		}

		public void TestCalculateStatusAndDisposition_TemplateCreatedTasks()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = incident.WorkflowItems.WorkflowType;

			var templateTask1 = workflowTemplate.WorkflowItems.Tasks.AddNew();
			templateTask1.P9_Sequence = 10;
			templateTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			var templateTask2 = workflowTemplate.WorkflowItems.Tasks.AddNew();
			templateTask2.P9_Sequence = 20;
			templateTask2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			templateTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			workflowTemplate.Factory.Save();

			incident.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates(new[] { workflowTemplate }));

			var workflowTask = incident.WorkflowItems.Tasks.Cast<ProcessTask>().FirstOrDefault(x => x.SourceTemplatePK == workflowTemplate.PK);

			AssertEquals("Precondition: 2 tasks should be created from the template", 2, incident.WorkflowItems.Count);
			var task1 = incident.WorkflowItems.Tasks[0];
			AssertEquals("Precondition: Should be template task 1", 10, task1.P9_Sequence);
			task1.P9_G4_RequiredCapability = ZGuid.Empty;
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var task2 = incident.WorkflowItems.Tasks[1];
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.Assigned, incident.IM_ResolutionCode);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction, incident.IM_ResolutionCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(SupportIncidentLookups.Status.Suspended, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Suspended.Deferred, incident.IM_ResolutionCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, incident.IM_ResolutionCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals("IM_ResolutionCode Should be COM", SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ResolutionCode);
		}

		public void TestApplyingTemplateWithCancelledTasksShouldCreateIncidentResolutionEvents()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();

			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = IncidentConstants.IncidentType.SupportIncident;
			workflowTemplate.P0_SubType1 = ProductTypes.Codes.Enterprise;

			var templateTask1 = workflowTemplate.WorkflowItems.Tasks.AddNew();
			templateTask1.P9_Sequence = 10;
			templateTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			workflowTemplate.Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			Factory.Save();

			AssertEquals("Precondition: 1 task should be created from the template", 1, incident.WorkflowItems.Count);
			var task1 = incident.WorkflowItems.Tasks[0];
			AssertEquals("Precondition: Task should be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, task1.P9_Status);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, incident.IM_ClosureResolution);

			var incidentResolvedLog = incident.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);

			var expectedResolvedReferences = new Dictionary<string, string>
			{
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled }
			};

			AssertReference(expectedResolvedReferences, incidentResolvedLog.SL_Reference);
		}

		public void TestApplyingTemplateOnSaveWithCancelledTasksShouldCreateIncidentResolutionEvents()
		{
			var workflowTemplateFactory = new BusinessObjectFactory();
			var staff = workflowTemplateFactory.NewWithValidTestData<GlbStaff>();
			var workflowTemplate = workflowTemplateFactory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = IncidentConstants.IncidentType.SupportIncident;

			var templateTask1 = workflowTemplate.WorkflowItems.Tasks.AddNew();
			templateTask1.P9_Sequence = 10;
			templateTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			workflowTemplate.P0_SubType1 = ProductTypes.Codes.Enterprise;
			workflowTemplate.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;
			workflowTemplateFactory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			Factory.Save();

			AssertEquals("Precondition: 1 task should be created from the template", 1, incident.WorkflowItems.Count);
			var task1 = incident.WorkflowItems.Tasks[0];
			AssertEquals("Precondition: Task should be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, task1.P9_Status);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled, incident.IM_ClosureResolution);

			var incidentResolvedLog = incident.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);

			var expectedResolvedReferences = new Dictionary<string, string>
			{
				{ CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled }
			};

			AssertReference(expectedResolvedReferences, incidentResolvedLog.SL_Reference);
		}

		public void TestApplyingTemplateWithFirstTaskCancelledShouldNotCloseIncident()
		{
			var workflowTemplateFactory = new BusinessObjectFactory();
			var staff = workflowTemplateFactory.NewWithValidTestData<GlbStaff>();
			var workflowTemplate = workflowTemplateFactory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = IncidentConstants.IncidentType.SupportIncident;

			var templateTask1 = workflowTemplate.WorkflowItems.Tasks.AddNew();
			templateTask1.P9_Sequence = 10;
			templateTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			var templateTask2 = workflowTemplate.WorkflowItems.Tasks.AddNew();
			templateTask2.P9_Sequence = 20;
			templateTask2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			templateTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			workflowTemplate.P0_SubType1 = ProductTypes.Codes.Enterprise;
			workflowTemplate.P0_SubType2 = SupportIncidentCategoriesList.Codes.FeatureRequest;
			workflowTemplateFactory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;

			Factory.Save();

			AssertEquals("Precondition: 2 tasks should be created from the template", 2, incident.WorkflowItems.Count);
			AssertEquals("Precondition: Task should be cancelled", ProcessTaskStatusCodeList.Codes.Cancelled, incident.WorkflowItems.Tasks[0].P9_Status);
			AssertEquals("Precondition: Task should not be cancelled", ProcessTaskStatusCodeList.Codes.Assigned, incident.WorkflowItems.Tasks[1].P9_Status);
			AssertEquals("Should not have closed incident", SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals("Should not have closed incident", SupportIncidentLookups.DispositionList.Constants.Working.Assigned, incident.IM_ResolutionCode);
			AssertEquals("Should not have closed incident", string.Empty, incident.IM_ClosureResolution);
		}

		public void TestCalculateStatusAndDisposition_FeatureRequest()
		{
			var project = Factory.NewWithValidTestData<EDIProject>();
			var featureRequest = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest.SetupForProjectFeatureRequest();
			featureRequest.RelatedProjectPK = project.PK;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "AAA";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "BBB";

			AssertEquals("Status should be Open", SupportIncidentLookups.Status.Open, featureRequest.IM_Status);
			AssertEquals("Disposition should be Awaiting Assignment", SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, featureRequest.IM_ResolutionCode);
			AssertEquals("No currently assigned to staff", ZString.Empty, featureRequest.IM_GS_NKAssignedToCurrent);

			var task1 = featureRequest.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;

			AssertEquals("Status should be Working", SupportIncidentLookups.Status.Open, featureRequest.IM_Status);
			AssertContains("Disposition should be Assigned", "Assigned - Awaiting Action", featureRequest.IM_ResolutionCodeDescription);
			AssertEquals("Current assigned to AAA", "AAA", featureRequest.IM_GS_NKAssignedToCurrent);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var task2 = featureRequest.WorkflowItems.AddNew();
			task2.P9_Sequence = 2;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;

			AssertEquals("Status should be Suspended", SupportIncidentLookups.Status.Suspended, featureRequest.IM_Status);
			AssertEquals("Current assigned to BBB", "BBB", featureRequest.IM_GS_NKAssignedToCurrent);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var task3 = featureRequest.WorkflowItems.AddNew();
			task3.P9_Sequence = 3;
			task3.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			featureRequest.IM_Status = SupportIncidentLookups.Status.Working;
			featureRequest.WaitForUpgrade();

			AssertEquals("Status should not change", SupportIncidentLookups.Status.Working, featureRequest.IM_Status);
			AssertContains("Disposition should not change", "Awaiting Auto Upgrade Deployment", featureRequest.IM_ResolutionCodeDescription);

			featureRequest.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, string.Empty);
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Status should be Closed", SupportIncidentLookups.Status.Closed, featureRequest.IM_Status);
			AssertContains("Resolution Code should be Closed", "Closed", featureRequest.IM_ResolutionCodeDescription);
			AssertContains("Disposition should be unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelivered, featureRequest.IM_ClosureResolution);

			featureRequest.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.FormalQuotationAccepted;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("Status should be re-calculated", SupportIncidentLookups.Status.Working, featureRequest.IM_Status);
			AssertEquals("Disposition should be not changed", SupportIncidentLookups.DispositionList.Constants.FormalQuotationAccepted, featureRequest.IM_ResolutionCode);
		}

		public void TestCalculateStatusAndDisposition_ShouldNotUpdateDispositionIfAwaitingClientResponse()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();

			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, incident.IM_ResolutionCode);
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);

			var task1 = incident.WorkflowItems.Tasks.AddNew();
			task1.P9_Sequence = 10;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals("Awaiting Client Response should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);

			task1.P9_G4_RequiredCapability = capability.PK;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals("Awaiting Client Response should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);

			task1.P9_G4_RequiredCapability = ZGuid.Empty;
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals("Awaiting Client Response should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var task2 = incident.WorkflowItems.Tasks.AddNew();
			task2.P9_Sequence = 20;
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("Awaiting Client Response should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(SupportIncidentLookups.Status.Open, incident.IM_Status);
			AssertEquals("Awaiting Client Response should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("Awaiting Client Response should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(SupportIncidentLookups.Status.Suspended, incident.IM_Status);
			AssertEquals("Awaiting Client Response should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Awaiting Client Response should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Awaiting Client Response should remain unchanged", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
		}

		public void TestRemoveTaskAssigneeClearCurrentIncidentAssignment()
		{
			SupportIncident featureRequest = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest.SetupForNewCreatedFeatureRequest();

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";

			SupportIncidentProcessTask task1 = featureRequest.WorkflowItems.AddNew();
			task1.P9_Sequence = 1;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_GS_NKAssignedStaffMember = "SCW";

			SupportIncidentProcessTask task2 = featureRequest.WorkflowItems.AddNew();
			GlbGroup group = Factory.New<GlbGroup>();
			task2.P9_Sequence = 2;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_GG_AssignedGroup = group.PK;
			task2.P9_GS_NKAssignedStaffMember = "";

			AssertEquals("Current assigned to SCW", "SCW", featureRequest.IM_GS_NKAssignedToCurrent);

			task1.P9_GS_NKAssignedStaffMember = "";
			AssertEquals("Current assigned to nobody", "", featureRequest.IM_GS_NKAssignedToCurrent);

			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			AssertEquals("Current assigned to SCW", "SCW", featureRequest.IM_GS_NKAssignedToCurrent);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Current assigned to nobody", "", featureRequest.IM_GS_NKAssignedToCurrent);

			task2.P9_GS_NKAssignedStaffMember = "SCW";
			AssertEquals("Current assigned to SCW", "SCW", featureRequest.IM_GS_NKAssignedToCurrent);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Current assigned to nobody", "", featureRequest.IM_GS_NKAssignedToCurrent);
		}

		public void TestCalculateWorkflowDependentPropertiesDoesNotAffectUnchangedIncidents()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "SCW";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SC2";
			Factory.Save();

			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.SetupForNewCreatedFeatureRequest();
			SupportIncidentProcessTask task = incident1.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = "SCW";
			task.P9_Status = "ASN";
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			AssertEquals("SCW", incident1.IM_GS_NKAssignedToCurrent);
			AssertEquals("OPN", incident1.IM_Status);

			string query = "UPDATE dbo.IncidentMain SET IM_GS_NKAssignedToCurrent = 'SC2', IM_Status = 'CLS', IM_SystemLastEditTimeUtc = GETUTCDATE(), IM_SystemLastEditUser = 'E' WHERE IM_PK = '" + incident1.PK.ToString() + "'";
			using (DbCommand command = Db.Connection.Command(query))
			{
				command.ExecuteNonQuery();
			}

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			SupportIncident loadedIncident1 = newFactory.Load<SupportIncident>(incident1.PK);
			AssertEquals("SC2", loadedIncident1.IM_GS_NKAssignedToCurrent);
			AssertEquals("CLS", loadedIncident1.IM_Status);
			SupportIncident loadedIncident2 = newFactory.Load<SupportIncident>(incident2.PK);
			loadedIncident2.IM_Description = "TEST";
			newFactory.Save();

			AssertEquals("SC2", loadedIncident1.IM_GS_NKAssignedToCurrent);
			AssertEquals("CLS", loadedIncident1.IM_Status);
		}

		public void TestCalculateWorkflowDependentPropertiesDoesNotAffectWorkItemRelatedDisposition()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "SCW";
			Factory.Save();

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.SetupForNewCreatedFeatureRequest();
			NewWorkItem workitem = Factory.NewWithValidTestData<NewWorkItem>();
			incident.RelatedWorkItems.Add(workitem);

			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);

			SupportIncidentProcessTask task = incident.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = "SCW";
			task.P9_Status = "WRk";
			Factory.Save();

			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);

			var checkInTask = workitem.WorkflowItems.AddNew();
			task.P9_Type = "CH0";
			task.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			workitem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.WaitForUpgrade();
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);
			Factory.Save();
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
		}

		public void TestGetProcessJobHeader_WhenBMSDisabled_ShouldNotCreateRecord()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;
			TestCaseHelper.ClearTable(ProcessHeaderSchema.Constants.TableName);

			var job = (IWorkflowProvider)Factory.New<SupportIncident>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory);
			AssertNull(jobHeader);
			Factory.Save();

			AssertEquals(0, (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.ProcessHeader"));
		}

		public void TestClose_CompleteTasks()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.SetupForNewCreatedDefect();
			ProcessTask task1 = incident.WorkflowItems.Tasks.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask task2 = incident.WorkflowItems.Tasks.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			ProcessTask task3 = incident.WorkflowItems.Tasks.AddNew();
			task3.P9_GS_NKAssignedStaffMember = "ZZ";
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			ProcessTask task4 = incident.WorkflowItems.Tasks.AddNew();
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ProcessTask task5 = incident.WorkflowItems.Tasks.AddNew();
			task5.P9_GS_NKAssignedStaffMember = "";
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			ProcessTask task6 = incident.WorkflowItems.Tasks.AddNew();
			task6.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			ProcessTask task7 = incident.WorkflowItems.Tasks.AddNew();
			task7.P9_GS_NKAssignedStaffMember = "";
			task7.P9_GG_AssignedGroup = group.PK;
			task7.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, "");
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task3.P9_Status);
			AssertEquals("ZZ", task3.P9_GS_NKAssignedStaffMember);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task4.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task5.P9_Status);
			AssertEquals("", task5.P9_GS_NKAssignedStaffMember);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task6.P9_Status);
			AssertEquals("", task7.P9_GS_NKAssignedStaffMember);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task7.P9_Status);
			AssertEquals("", task7.P9_GS_NKAssignedStaffMember);

			incident.Validation.ValidateAll();
			AssertNoErrors(task1.P9_StatusInfo);
			AssertNoErrors(task2.P9_StatusInfo);
			AssertNoErrors(task3.P9_StatusInfo);
			AssertNoErrors(task4.P9_StatusInfo);
			AssertNoErrors(task5.P9_StatusInfo);
			AssertNoErrors(task6.P9_StatusInfo);
			AssertNoErrors(task7.P9_StatusInfo);
		}

		public void TestCloseIncident_CancelsUnclosedTasks()
		{
			AssertCancelsUnclosedTasks(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled);
			AssertCancelsUnclosedTasks(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.DispositionList.Constants.Closed.NotFeatureRequest);

			AssertCancelsUnclosedTasks(SupportIncidentCategoriesList.Codes.ComplianceRequirement, SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled);
			AssertCancelsUnclosedTasks(SupportIncidentCategoriesList.Codes.ComplianceRequirement, SupportIncidentLookups.DispositionList.Constants.Closed.NotComplianceRequirement);

			AssertCancelsUnclosedTasks(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, SupportIncidentLookups.DispositionList.Constants.Closed.Cancelled);
			AssertCancelsUnclosedTasks(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, SupportIncidentLookups.DispositionList.Constants.Closed.NotCustomerServiceRequest);
		}

		void AssertCancelsUnclosedTasks(string stage, string method)
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Category = stage;
			ProcessTask task1 = incident.WorkflowItems.Tasks.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask task2 = incident.WorkflowItems.Tasks.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			ProcessTask task3 = incident.WorkflowItems.Tasks.AddNew();
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			ProcessTask task4 = incident.WorkflowItems.Tasks.AddNew();
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ProcessTask task5 = incident.WorkflowItems.Tasks.AddNew();
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			ProcessTask task6 = incident.WorkflowItems.Tasks.AddNew();
			task6.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			incident.CloseIncident(method, "");
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(method, incident.IM_ClosureResolution);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task3.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task4.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task5.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task6.P9_Status);
		}

		public void TestRemoveNonPersistentTasks()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			ProcessTask task1 = incident.WorkflowItems.AddNew();
			ProcessTask task2 = incident.WorkflowItems.AddNew();

			Factory.Save();
			AssertEquals(2, incident.WorkflowItems.Count);
			AssertCollectionContains(task1, incident.WorkflowItems);
			AssertCollectionContains(task2, incident.WorkflowItems);

			ProcessTask task3 = incident.WorkflowItems.AddNew();
			ProcessTask task4 = incident.WorkflowItems.AddNew();
			incident.SuspendPopulatingWorkflowTemplate();
			incident.SetUpgradeDelivered();

			Factory.Save();
			AssertEquals(2, incident.WorkflowItems.Count);
			AssertCollectionNotContains(task3, incident.WorkflowItems);
			AssertCollectionNotContains(task4, incident.WorkflowItems);

			incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.SuspendPopulatingWorkflowTemplate();
			incident.WorkflowItems.AddNew();
			Factory.Save();
			AssertEquals(0, incident.WorkflowItems.Count);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
		}

		public void TestCloseIncidentByWorkflowTask()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var task = incident.WorkflowItems.AddNew();
			task.P9_Sequence = 10;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);
			var originalClosureResolution = incident.IM_ClosureResolution;

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);

			var eConversationMessages = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("Closed As Completed", eConversationMessages.First().Body);

			var incidentResolvedLog = incident.Logs.GetAllLogs().Cast<StmALog>().First
				(x => x.SL_SE_NKEvent == AutoEvents.IncidentResolvedCode);
			var incidentStatusUpdatedLog = incident.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_EventTime).First
			(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode);

			var expectedResolvedReferences = new Dictionary<string, string>();
			expectedResolvedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, SupportIncidentLookups.DispositionList.Constants.Closed.Completed);
			AssertReference(expectedResolvedReferences, incidentResolvedLog.SL_Reference);

			var expectedStatusUpdatedReferences = new Dictionary<string, string>();
			expectedStatusUpdatedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed);
			expectedStatusUpdatedReferences.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Incident closed by user {GlbStaff.CurrentUser.GS_Code}"));
			AssertReference(expectedStatusUpdatedReferences, incidentStatusUpdatedLog.SL_Reference);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			incident.OnCloseIncident += (s, e) => { incident.AddPublicSystemLogMessage("Incident closed by task"); };

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);

			eConversationMessages = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("Incident closed by task", eConversationMessages.First().Body);
		}

		public void TestCloseIncidentByWorkflowTask_ShouldNotCloseIfAwaitingClientResponse()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.CustomerServiceRequest;
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "", true);

			var task = incident.WorkflowItems.AddNew();
			task.P9_Sequence = 10;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("Should not overwrite awaiting client response", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
			AssertEquals("Should not set a closure resolution", string.Empty, incident.IM_ClosureResolution);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals("Should not overwrite awaiting client response", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
			AssertEquals("Should not set a closure resolution", string.Empty, incident.IM_ClosureResolution);
		}

		#endregion

		#region Project Related Incidents

		public void TestAttachAndDetachProject()
		{
			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			SupportIncident featureRequest = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest.SetupForNewCreatedFeatureRequest();
			featureRequest.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;

			GlbStaff businessConsultant = Factory.NewWithValidTestData<GlbStaff>();
			businessConsultant.GS_Code = "BCS";

			featureRequest.RelatedProjectPK = project.PK;
			featureRequest.IM_GS_NKSpecifiedBy = businessConsultant.GS_Code;
			AssertEquals(SupportIncidentLookups.SourceListConstants.CreatedFromProject, featureRequest.IM_Source);
			AssertNotNull(featureRequest.BusinessConsultant);
			AssertEquals("BCS", featureRequest.BusinessConsultant.GS_Code);

			featureRequest.RelatedProjectPK = ZGuid.Empty;
			AssertEquals(SupportIncidentLookups.SourceListConstants.ERequestPortal, featureRequest.IM_Source);
			AssertNull("Detach project clear Business Consultant assignment", featureRequest.BusinessConsultant);

			featureRequest.RelatedProjectPK = ZGuid.Empty;
			featureRequest.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
			featureRequest.RelatedItems.Add(project);
			AssertEquals(SupportIncidentLookups.SourceListConstants.CreatedFromProject, featureRequest.IM_Source);

			featureRequest.RelatedItems.Load();
			featureRequest.RelatedItems.Remove(project);
			AssertEquals(SupportIncidentLookups.SourceListConstants.ERequestPortal, featureRequest.IM_Source);
		}

		public void TestAttachAndDetachWorkitem()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.SetupForNewCreatedFeatureRequest();
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			ProcessTask task1 = incident.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			ProcessTask task2 = incident.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();
			incident.SetUpgradeDelivered();
			Factory.Save();
			AssertEquals(SupportIncidentCategoriesList.Codes.FeatureRequest, incident.IM_Category);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
			AssertEquals("Precondition: Disposition initially", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			incident.RelatedItems.Add(workItem);
			AssertEquals("Attaching WorkItem, set Disposition to CFD", SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);
			incident.RelatedItems.Load();
			incident.RelatedItems.Remove(workItem);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
		}

		public void TestDetachWorkingWorkitem_UpdateIncidentStatus()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			incident.SetupForNewCreatedFeatureRequest();

			var workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem1.WKI_Status = "CLS";
			incident.RelatedItems.Add(workItem1);

			var workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem1.WKI_Status = "CLS";
			incident.RelatedItems.Add(workItem2);
			Factory.Save();

			workItem2.WKI_Status = "WRK";
			Factory.Save();
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.Status.Working, incident.IM_Status);
			AssertEquals("CLS", workItem1.WKI_Status);
			AssertEquals("WRK", workItem2.WKI_Status);

			incident.RelatedItems.Remove(workItem2);
			Factory.Save();
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals("CLS", workItem1.WKI_Status);
			AssertEquals("WRK", workItem2.WKI_Status);
		}

		#endregion

		#region eConversation

		public void TestConversationWasSavedIntoDatabaseWhenSupportIncidentFirstSaved()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var conversation = newFactory.Load<JobConversation>(incident.EConversation.Conversation.PK);

			AssertNotNull(conversation);
		}

		public void TestEConversationInitialisationNoEmptyMessageCreated()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			IeDoc doc = incident.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 1, 1, 1, 1 }, "Estimate.pdf", "AAA");
			doc.IsPublished = true;
			Factory.Save();

			var incReloaded = new BusinessObjectFactory() { RefreshEnabled = false }.Load<SupportIncident>(incident.PK);
			var convo = incReloaded.EConversation.JobConversationForTest;
			AssertEquals("Only one", 1, convo.Messages.Count);
			AssertContains("Only one", "Attached to eDocs: Estimate.pdf", convo.Messages[0].Body);
		}

		public void TestAddInternalMessage()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_Module = "COR";
			incident.IM_Priority = "CR4";
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "Incident details";

			Factory.Save();

			incident.AddInternalMessage("This one is internal message and not visible to client");

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			var message = (JobConversationMessage)messageList.Single(msg => msg.Body == "This one is internal message and not visible to client");
			AssertEquals(true, message.JCM_IsInternal);
		}

		public void TestAddInternalMessage_DisplayCorrespondingDescription()
		{
			var productAreas = new CodeDescriptionPairList();
			productAreas.AddPair("PA1", "Product Area 1");
			productAreas.AddPair("PA2", "Product Area 2");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var xxxMapping = product.ModuleMappings.AddNew("AAA", "AAA Default", "PA1", true);
			xxxMapping.SourceModuleMappings.AddNew("SourceModule1", "PA2");
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.CargoWiseOne, "test", true);
			var yyyMapping = product.ModuleMappings.AddNew("AAA", "AAA CR8", "PA1", true);
			yyyMapping.SourceModuleMappings.AddNew("SourceModule1", "PA2");
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = new SystemProductCollection();
			product = collection.AddNew(ProductTypes.Codes.CargoWiseOne, "test", true);
			yyyMapping = product.ModuleMappings.AddNew("AAA", "AAA CR9", "PA1", true);
			yyyMapping.SourceModuleMappings.AddNew("SourceModule1", "PA2");

			product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			yyyMapping = product.ModuleMappings.AddNew("BBB", "BBB CR9", "PA1", true);
			yyyMapping.SourceModuleMappings.AddNew("SourceModule2", "PA2");
			EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = "CR9";
			incident.IM_Module = "BBB";
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "Incident details";
			Factory.Save();

			incident.IM_Product = ProductTypes.Codes.CargoWiseOne;
			incident.IM_Module = "AAA";
			Factory.Save();

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("The log should contain last changes", true, messageList.Any(msg => msg.Body == "Module changed from BBB (BBB CR9) to AAA (AAA CR9)"));
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestConvertOldLogToEConversation()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "SCW";
			staff1.GS_FullName = "Samuel";

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "UTT";
			staff2.GS_FullName = "Unit Tester";
			staff2.GS_IsActive = false;

			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "AA";
			staff3.GS_FullName = "AAA";

			GlbStaff staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_Code = "BB ";
			staff4.GS_FullName = "BBB";

			GlbStaff staff5 = Factory.NewWithValidTestData<GlbStaff>();
			staff5.GS_Code = "G";
			staff5.GS_FullName = "CCC";

			// Full name of ~BP is different between testing and production databases
			GlbStaff system1 = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "~BP"));
			system1.GS_FullName = "CargoWise Team";

			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.SetLogTextForTest(
@"3/07/2011 2:31:42 PM SCW - Waiting for callback - how did you go?
----------------------------------------------------------------------------------------------------------------------
14-Jun-11 14:31 - I like creating logs with blank users
14-Jun-11 14:30 UTT - Blah blah Line One
Line Two
Line Three

Line Four
----------------------------------------------------------------------------------------------------------------------
14-Jun-11 14:29 AA - Email Sent to someone@test.com - Update on Incident: CS00145906 - Test Incident
----------------------------------------------------------------------------------------------------------------------
14-Jun-11 08:55 BB  - Criticality changed from CR3 to CR4
----------------------------------------------------------------------------------------------------------------------
16/08/2006 10:51:48 PM - Previously linked to work item: W00044444 - Create silent exceptions due to blank user
15/07/2006 22:51:48 ~BP - Been converted already ... somehow
----------------------------------------------------------------------------------------------------------------------
22/06/2005 4:45:12 PM G - Notification email sent to contact (Samuel).
----------------------------------------------------------------------------------------------------------------------
");
			Factory.Save();

			var incidentInAnotherFactory = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);

			var messageList = incidentInAnotherFactory.EConversation.GetTimeOrderedMessages().Cast<JobConversationMessage>().ToArray();
			AssertConversationMessage(messageList[0], "Samuel", "SCW", new ZDateTime(2011, 7, 3, 4, 31, 42, DateTimeKind.Utc), "Waiting for callback - how did you go?");
			AssertConversationMessage(messageList[1], "System", "~BP", new ZDateTime(2011, 6, 14, 4, 31, 0, DateTimeKind.Utc), "I like creating logs with blank users");
			AssertConversationMessage(messageList[2], "Unit Tester", "UTT", new ZDateTime(2011, 6, 14, 4, 30, 0, DateTimeKind.Utc), "Blah blah Line One\r\nLine Two\r\nLine Three\r\n\r\nLine Four");
			AssertConversationMessage(messageList[3], "AAA", "AA", new ZDateTime(2011, 6, 14, 4, 29, 0, DateTimeKind.Utc), "Email Sent to someone@test.com - Update on Incident: CS00145906 - Test Incident");
			AssertConversationMessage(messageList[4], "BBB", "BB", new ZDateTime(2011, 6, 13, 22, 55, 0, DateTimeKind.Utc), "Criticality changed from CR3 to CR4");
			AssertConversationMessage(messageList[5], "System", "~BP", new ZDateTime(2006, 8, 16, 12, 51, 48, DateTimeKind.Utc), "Previously linked to work item: W00044444 - Create silent exceptions due to blank user");
			AssertConversationMessage(messageList[6], "System", "~BP", new ZDateTime(2006, 7, 15, 12, 51, 48, DateTimeKind.Utc), "Been converted already ... somehow");
			AssertConversationMessage(messageList[7], "CCC", "G", new ZDateTime(2005, 6, 22, 6, 45, 12, DateTimeKind.Utc), "Notification email sent to contact (Samuel).");

			AssertNoErrors(messageList[7].JCM_PostedTimeUtcInfo);
		}

		void AssertConversationMessage(JobConversationMessage message, ZString expectedUserName, ZString expectedUserCode, ZDateTime expectedSentTimeInUTC, ZString expectedBody)
		{
			AssertEquals(expectedUserName, message.SenderDisplayName);
			AssertEquals(expectedUserCode, message.SenderCode);
			AssertEquals(expectedSentTimeInUTC.ToDateTime().ToLocalTime(), message.SendLocalDateTime);
			AssertEquals(expectedBody, message.Body);
		}

		public void TestAddEConversationMessageSetIncidentHasChanges()
		{
			SupportIncident incident = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();
			AssertEquals(false, incident.HasChanges);

			incident.AddStaffMessageToCustomer("Public User Message In Email");
			AssertEquals(true, incident.HasChanges);
			Factory.Save();

			AssertEquals(false, incident.HasChanges);

			incident.AddStaffMessageToCustomer("Public User Message In Email For Legacy Client System");
			AssertEquals(true, incident.HasChanges);
			Factory.Save();

			AssertEquals(false, incident.HasChanges);

			incident.AddSystemMessageToCustomer("Public System Log In Email For Legacy Client System");
			AssertEquals(true, incident.HasChanges);
			Factory.Save();

			AssertEquals(false, incident.HasChanges);

			incident.AddInternalMessage("Internal User Message");
			AssertEquals(true, incident.HasChanges);
			Factory.Save();

			AssertEquals(false, incident.HasChanges);

			incident.AddPublicSystemLogMessage("Public System Log");
			AssertEquals(true, incident.HasChanges);
			Factory.Save();

			AssertEquals(false, incident.HasChanges);

			incident.AddInternalSystemLogMessage("Internal System Log");
			AssertEquals(true, incident.HasChanges);
			Factory.Save();

			AssertEquals(false, incident.HasChanges);
		}

		protected string GetActualLogTextForStaffChangedLogAssertion(SupportIncident workTask)
		{
			ZStringBuilder builder = new ZStringBuilder();
			foreach (var message in workTask.EConversation.GetTimeOrderedMessages())
			{
				builder.AppendLine(message.Body);
			}
			return builder.ToString();
		}

		public void TestLogAssignedStaffChange()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			staff1.GS_FullName = "Staff One";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ST2";
			staff2.GS_FullName = "Staff Two";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_GS_NKCustServiceContact = "ST1";
			Factory.Save();

			incident.IM_GS_NKCustServiceContact = "ST2";
			Factory.Save();

			var lastMessage = incident.EConversation.GetTimeOrderedMessages()[0];
			AssertEquals("Should contain expected message", "Email sent to new assigned staff - Staff Two.", lastMessage.Body);
			AssertEquals("Should be internal message", MessageType.LocalInternal, lastMessage.MessageType);
			AssertEquals("Should be system message", MessageSubType.SystemLog, lastMessage.MessageSubType);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "ST3";
			staff3.GS_FullName = "Staff Three";
			incident.IM_GS_NKAssignedToCurrent = "ST3";
			Factory.Save();
			var messages = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("Should not contain any staff three assignment message", 0, messages.Count(msg => msg.Body == "Email sent to new assigned staff - Staff Three."));
		}

		#endregion

		public void TestLoadDoesNotAccessSecurity()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsController = false;
			var helper = new SupportIncidentTestHelper(Factory);
			var incident = helper.CreateIncidentWithLegacyClient();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			Factory.Save();

			Env.Instance.ResetSecurityForTest();

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				AssertEquals("PRE", false, Env.Instance.IsSecurityCreatedForTest);

				var factory = new BusinessObjectFactory() { RefreshEnabled = false };
				factory.Load<SupportIncident>(incident.PK);
				AssertEquals(false, Env.Instance.IsSecurityCreatedForTest);

				// Prove security would have been created if a checkpoint was used
				var allowed = EDISecurityCheckpoints.CustomerServiceIncidentEdit.IsAllowed;
				AssertEquals(true, Env.Instance.IsSecurityCreatedForTest);
			}
		}

		public void TestIM_IncidentNumber()
		{
			var incident = Factory.New<SupportIncident>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			incident.IM_OH_Client = org.PK;
			incident.IM_Product = "ENT";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Source = "";
			incident.ProductArea = "DOM";
			incident.IM_Language = Core.SharedConstants.Languages.ChineseSimplified;

			var incident2 = Factory.New<SupportIncident>();
			incident2.IM_OH_Client = org.PK;
			incident2.IM_Product = "ENT";
			incident2.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident2.IM_Source = "";
			incident2.ProductArea = "DOM";
			incident2.IM_Language = Core.SharedConstants.Languages.ChineseSimplified;
			incident2.IM_IncidentNumber = "12345";

			Factory.Save();
			AssertEquals("incident and request have same automatic number", incident.Request.INC_IncidentNumber, incident.IM_IncidentNumber);

			AssertEquals("incident and request have same manual number", "12345", incident2.Request.INC_IncidentNumber);
			AssertEquals("incident and request have same manual number", "12345", incident2.IM_IncidentNumber);

			var incident3 = Factory.New<SupportIncidentForTest>();
			incident3.IM_OH_Client = org.PK;
			incident3.IM_Product = "ENT";
			incident3.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident3.IM_Source = "";
			incident3.ProductArea = "DOM";
			incident3.IM_Language = Core.SharedConstants.Languages.ChineseSimplified;
			incident3.OnSavingShouldThrowAtEnd = true;

			AssertExceptionThrown<ZSaveException>(() => { Factory.Save(); });
			AssertEquals("", incident3.Request.INC_IncidentNumber);
			AssertEquals("", incident3.IM_IncidentNumber);
		}

		public void TestSendStaffMessagesToOtherSubscribedStaff()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@wisetechglobal.com";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "staff2@wisetechglobal.com";
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_EmailAddress = "staff3@wisetechglobal.com";
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_EmailAddress = "staff4@wisetechglobal.com";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff4.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var helper = new SupportIncidentTestHelper(Factory);
				var incident = helper.CreateIncidentWithLicencedContact("ENT");
				incident.CustomerNotifier = new NoActionIncidentCustomerNotifier(incident);
				incident.IM_Module = "COR";
				incident.IM_Priority = "CR4";
				incident.IM_Description = "Test Incident";
				incident.DetailNoteText = "Incident details";

				var participantStaff1 = incident.EConversation.Conversation.Participants.AddNewParticipant(staff1);
				var participantStaff2 = incident.EConversation.Conversation.Participants.AddNewParticipant(staff2);
				var participantStaff3 = incident.EConversation.Conversation.Participants.AddNewParticipant(staff3);
				participantStaff1.JCP_IsSubscribed = true;
				participantStaff2.JCP_IsSubscribed = false;
				participantStaff3.JCP_IsSubscribed = true;
				Factory.Save();
				Env.OutgoingMailManager.EmailsCreated.Clear();

				incident.AddStaffMessageToCustomer("Hello Customer");
				incident.AddInternalMessage("Some internal message");
				incident.AddStaffMessageToCustomer("All is resolved.");
				AssertEquals("PRE", false, Env.CurrentUser.IsSystemAccount);
				Factory.Save();
			}

			var emails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals(1, emails.Count);
			var emailToStaff = emails[0];
			var recipients = emailToStaff.Recipients.Cast<RecipientDef>().ToList();
			AssertEquals(2, recipients.Count);
			AssertNotNull(recipients.Single(x => x.Email == "staff1@wisetechglobal.com"));
			AssertNotNull(recipients.Single(x => x.Email == "staff3@wisetechglobal.com"));
			AssertContains("Hello Customer", emailToStaff.Body);
			AssertContains("Some internal message", emailToStaff.Body);
			AssertContains("All is resolved", emailToStaff.Body);
		}

		void IM_ProgramArea_Setup()
		{
			var pks = new List<ZGuid>();
			var helper = new SupportIncidentTestHelper(Factory);
			var magicNumber = 100;
			for (int i = 0; i < magicNumber; i++)
			{
				//var incident = helper.CreateIncidentWithLicencedContact("ENT");
				var incident = Factory.New<SupportIncident>();
				incident.IM_Module = "ALL";
				incident.IM_IncidentType = i % 2 == 0 ? "INC" : "WI";
				incident.IM_ProgramArea = i < magicNumber / 2 ? "GLW" : "ARC";
				incident.IM_Priority = "CR9";
				pks.Add(incident.PK);
				var jobHeader = ProcessJobHeader.GetForParent(incident, Factory);
				var processHeader = jobHeader.ProcessHeaders.AddNew();
				processHeader.FH_IsActive = i % 4 == 0;
			}
			Factory.Save();
		}

		public void TestIM_ProgramAreaIndex_ShouldNotUseIndex()
		{
			IM_ProgramArea_Setup();
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var sql = "SELECT * FROM dbo.ProcessHeader WHERE FH_IsActive = 1 AND ( FH_ParentId IN ( SELECT IM_PK FROM dbo.IncidentMain WHERE ( IM_ProgramArea = 'GLW' ) ) ) ";
				using (var reader = TestConnection.Command(sql).ExecuteReader())
				{ while (reader.Read()) { } }

				var plans = TestConnection.ExecutedCommandsAndQueryPlans.Single(t => t.Item1.Contains("IM_PK", StringComparison.CurrentCultureIgnoreCase));
				var planalyzer = new QueryPlanalyzer(plans.Item2.Single());

				CombineAssertions(() =>
				{
					AssertCollectionNotContains("No index seeks on NR_RX__IM_ProgramArea (WHERE IM_IncidentType = 'INC')", "NR_RX__IM_ProgramArea", planalyzer.IndexSeeks.Select(x => x.IndexName));
				});
			}
		}

		public void TestEnsureOpenTask()
		{
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var incident = Factory.New<SupportIncident>();
			SupportIncidentTestHelper.AddWorkflowTask(incident, "description", "UDF", "CAN", staff2.GS_Code, "", ZDateTimeOffset.Invalid);
			incident.CloseIncident("TRN", "self resolved");
			AssertEquals("PRE", SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals("PRE", true, incident.WorkflowItems.AllTasksClosedOrCancelled);

			incident.EnsureOpenTask();
			AssertNotEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(false, incident.WorkflowItems.AllTasksClosedOrCancelled);
		}

		public void TestEnsureOpenTask_IncidentNotClosed()
		{
			CreateWorkflowTemplate();
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incidentTask = SupportIncidentTestHelper.AddWorkflowTask(incident1, "create work item", "UDF", ProcessTaskStatusCodeList.Codes.Working, staff.GS_Code, "", ZDateTimeOffset.Invalid);
			incident1.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			var workItem1 = incident1.RelatedWorkItems.AddNew();
			var task1 = workItem1.WorkflowItems.AddNew();
			task1.P9_Type = EDITaskTypes_ForTest.TaskCodingOfFunctionality;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			incidentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals("PRE", SupportIncidentLookups.Status.Working, incident1.IM_Status);
			AssertEquals("PRE", true, incident1.WorkflowItems.AllTasksClosedOrCancelled);

			incident1.EnsureOpenTask();
			AssertNotEquals(SupportIncidentLookups.Status.Closed, incident1.IM_Status);
			AssertEquals(false, incident1.WorkflowItems.AllTasksClosedOrCancelled);
			var tasks = incident1.WorkflowItems.Tasks.Cast<ProcessTask>().ToList();
			AssertEquals(4, tasks.Count);
			AssertEquals("Notify Client", tasks[3].P9_Description);
		}

		public void TestEnsureOpenTask_AwaitingResponse_NewMessage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			var task = incident.WorkflowItems.AddNew();
			task.P9_Sequence = 1;
			task.P9_Type = "INV";
			task.P9_Description = "investigation";
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			incident.EnsureOpenTask(isLegacyReopenRequest: false, isByNewMessage: true, isByNewEmail: false);
			AssertNotEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertNotEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
			AssertEquals(false, incident.WorkflowItems.AllTasksClosedOrCancelled);
		}

		public void TestEmptyCapabilityFound()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incidentTask = SupportIncidentTestHelper.AddWorkflowTask(incident1, "create work item", "UDF",
				ProcessTaskStatusCodeList.Codes.Working, "", "", ZDateTimeOffset.Invalid);
			incidentTask.P9_G4_RequiredCapability = ZGuid.BrettsGuid;
			AssertEquals("Should be an empty string for OverallAssignedToDescription if GlbCapability not found.", "", incident1.OverallAssignedToDescription);
		}

		public void TestEnterprise()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "E01";
			enterprise.LE_OH = org.PK;

			var company = Factory.NewWithValidTestData<LicenceCompany>();
			company.LC_LE = enterprise.PK;
			company.LC_OH = org.PK;

			var database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			database1.LD_LE = enterprise.PK;
			database1.LD_ServerCode = "AAA";

			var header = Factory.NewWithValidTestData<LicenceHeader>();
			header.LA_LC = company.PK;
			header.LA_LD = database1.PK;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = company.LC_CompanyCode;
			clientCompany.LCC_LD = database1.PK;
			clientCompany.LCC_OH = org.PK;

			Factory.Save();

			var incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			AssertEquals(incident.EnterprisePK, enterprise.PK);
			AssertEquals(incident.EnterpriseCode, enterprise.LE_EnterpriseCode);
			AssertEquals(false, incident.EnterprisePKInfo.HasChanges);
			AssertEquals(false, incident.EnterpriseCodeInfo.HasChanges);

			incident.EnterprisePK = ZGuid.Empty;
			AssertEquals(incident.EnterprisePK, ZGuid.Empty);
			AssertEquals(incident.EnterpriseCode, "");
			AssertNoErrors(incident.EnterprisePKInfo);
			AssertNoErrors(incident.EnterpriseCodeInfo);

			incident.EnterprisePK = ZGuid.NewZGuid();
			AssertEquals(incident.EnterpriseCode, "");
			AssertHasErrors(incident.EnterprisePKInfo);
			AssertNoErrors(incident.EnterpriseCodeInfo);

			incident.EnterprisePK = enterprise.PK;
			AssertEquals(incident.EnterpriseCode, enterprise.LE_EnterpriseCode);
			AssertNoErrors(incident.EnterprisePKInfo);
			AssertNoErrors(incident.EnterpriseCodeInfo);

			incident.EnterpriseCode = "";
			AssertEquals(incident.EnterprisePK, ZGuid.Empty);
			AssertEquals(incident.EnterpriseCode, "");
			AssertNoErrors(incident.EnterprisePKInfo);
			AssertNoErrors(incident.EnterpriseCodeInfo);

			incident.EnterpriseCode = "@#$";
			AssertEquals(incident.EnterprisePK, ZGuid.Empty);
			AssertEquals(incident.EnterpriseCode, "@#$");
			AssertNoErrors(incident.EnterprisePKInfo);
			AssertHasErrors(incident.EnterpriseCodeInfo);

			incident.EnterpriseCode = enterprise.LE_EnterpriseCode;
			AssertEquals(incident.EnterprisePK, enterprise.PK);
			AssertEquals(incident.EnterpriseCode, enterprise.LE_EnterpriseCode);
			AssertNoErrors(incident.EnterprisePKInfo);
			AssertNoErrors(incident.EnterpriseCodeInfo);
		}

		public void TestClientCompanyCode_Visible()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_OA_BranchAddress = Org.MainAddress.PK;
			incident.IM_OC_Contact = Contact.PK;
			incident.IM_Description = "Help me";
			incident.DetailNoteText = "Don't know what i'm doing";
			incident.IM_LD = ClientCompany.Database.PK;
			incident.IM_LCC = ClientCompany.PK;
			incident.IM_ClientIncidentReference = "CL111111";

			ClientCompany.Database.LD_Product = ProductTypes.Codes.Enterprise;
			AssertEquals(true, incident.ClientCompanyCode_Visible);

			ClientCompany.Database.LD_Product = ProductTypes.Codes.ProductivityWise;
			AssertEquals(true, incident.ClientCompanyCode_Visible);

			ClientCompany.Database.LD_Product = ProductTypes.Codes.GLOW;
			AssertEquals(false, incident.ClientCompanyCode_Visible);

			ClientCompany.Database.LD_Product = ProductTypes.Codes.CargoWiseNext;
			AssertEquals(true, incident.ClientCompanyCode_Visible);

			ClientCompany.Database.LD_Product = ProductTypes.Codes.CargoWise;
			AssertEquals(true, incident.ClientCompanyCode_Visible);
		}

		public void TestChildrenParentRelatedItems()
		{
			var incidentParent = Factory.NewWithValidTestData<SupportIncident>();
			incidentParent.IM_Description = "Parent Incident";
			var incident = incidentParent.RelatedItems.AddNew(typeof(SupportIncident)) as SupportIncident;

			incident.RelatedItems.AddNew(typeof(WorkItem));
			incident.RelatedItems.AddNew(typeof(WorkItem));

			var model = new WorkTaskRelatedItemsTreeModel(incident);
			var mainNode = model.RootNodes;
			AssertEquals(1, mainNode.Count);

			var rootNodes = mainNode[0].ChildNodes.ToList();
			AssertEquals(2, rootNodes.Count);

			incident.RelatedItems.Load();
			AssertEquals("Should load all related items", 3, incident.RelatedItems.Count);
			incident.ChildrenOnlyRelatedItems.Load();
			AssertEquals("Should load just the children", 2, incident.ChildrenOnlyRelatedItems.Count);
			incident.ParentsOnlyRelatedItems.Load();
			AssertEquals("Should load just the parent", 1, incident.ParentsOnlyRelatedItems.Count);
			AssertEquals("Parent Incident", incident.ParentsOnlyRelatedItems.Cast<SupportIncident>().First().IM_Description);
		}

		public void TestParentBuildTree()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();

			var project = incident.RelatedItems.AddNew(typeof(Project)) as Project;
			incident.RelatedItems.AddNew(typeof(Project));
			project.RelatedItems.AddNew(typeof(WorkItem));
			project.RelatedItems.AddNew(typeof(WorkItem));

			var incidentParent1 = Factory.NewWithValidTestData<SupportIncident>();
			incidentParent1.IM_Description = "Parent1";
			incidentParent1.RelatedItems.Add(incident);
			var incidentParent2 = Factory.NewWithValidTestData<SupportIncident>();
			incidentParent2.IM_Description = "Parent2";
			incidentParent2.RelatedItems.Add(incident);
			var incidentParent3 = Factory.NewWithValidTestData<SupportIncident>();
			incidentParent3.IM_Description = "Parent3";
			incidentParent3.RelatedItems.Add(incident);

			var model = new WorkTaskRelatedItemsTreeModel(incident, true);
			var mainNode = model.RootNodes;
			AssertEquals(3, mainNode.Count);

			var parent1 = mainNode.First(a => a.BizObj.RelatedItem.ItemDescription == "Parent1");
			AssertNotNull(parent1);
			AssertEquals(0, parent1.ChildNodes.Count());

			var parent2 = mainNode.First(a => a.BizObj.RelatedItem.ItemDescription == "Parent2");
			AssertNotNull(parent2);
			AssertEquals(0, parent2.ChildNodes.Count());

			var parent3 = mainNode.First(a => a.BizObj.RelatedItem.ItemDescription == "Parent3");
			AssertNotNull(parent3);
			AssertEquals(0, parent3.ChildNodes.Count());
		}

		public void TestUniversalCopyAttribute()
		{
			var attribute = typeof(SupportIncident).GetCustomAttribute(typeof(UniversalCopyWithExtendedEntitiesAttribute), true) as UniversalCopyWithExtendedEntitiesAttribute;
			AssertNotNull(attribute);
			AssertEquals(typeof(UniversalCopyWithExtendedEntitiesAttribute), attribute.TypeId);
			AssertEquals(null, attribute.FinishCopyMethod);
			AssertEquals(false, attribute.IgnoreAllElementsExceptSpecificallyMarked);
		}

		public void TestProductChangeLogged()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("COR", "Core Product");
			areas.AddPair("DOM", "Domestic Logistics");
			areas.AddPair("INT", "International Logistics");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			var collection = new SystemProductCollection();
			var product1 = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product1.ModuleMappings.AddNew("SAA", "Super Module A", "COR", false);

			var product2 = collection.AddNew(ProductTypes.Codes.WiseTechAcademy, "test", true);
			product2.ModuleMappings.AddNew("SBB", "Super Module B", "DOM", false);

			var product3 = collection.AddNew(ProductTypes.Codes.CargoWiseOne, "test", true);
			product3.ModuleMappings.AddNew("SDD", "Super Module D", "INT", false);

			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = "CR8";
			incident.ProductArea = "COR";
			incident.IM_Module = "SAA";
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("The log should NOT be added : new incident", false, messageList.Any(msg => msg.Body == "Product changed from"));
			AssertEquals("The log should NOT be added : new incident", false, messageList.Any(msg => msg.Body == "Module changed from"));
			AssertEquals("The log should NOT be added : new incident", false, messageList.Any(msg => msg.Body == "Product Area changed from"));

			Factory.Save();
			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("The log should NOT be added : new incident", false, messageList.Any(msg => msg.Body == "Product changed from"));
			AssertEquals("The log should NOT be added : new incident", false, messageList.Any(msg => msg.Body == "Module changed from"));
			AssertEquals("The log should NOT be added : new incident", false, messageList.Any(msg => msg.Body == "Product Area changed from"));

			incident.IM_Product = ProductTypes.Codes.WiseTechAcademy;
			incident.ProductArea = "DOM";
			incident.IM_Module = "SBB";
			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("The log should NOT be added : new incident", false, messageList.Any(msg => msg.Body == "Product changed from"));
			AssertEquals("The log should NOT be added : changes not saved yet", false, messageList.Any(msg => msg.Body == "Module changed from"));
			AssertEquals("The log should NOT be added : changes not saved yet", false, messageList.Any(msg => msg.Body == "Product Area changed from"));

			Factory.Save();
			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("The log should be added : incident persistant and changes successfully saved", true, messageList.Any(msg => msg.Body == "Product changed from ENT (CargoWise) to WTA (test)"));
			AssertEquals("The log should be added : incident persistant and changes successfully saved", true, messageList.Any(msg => msg.Body == "Module changed from SAA (Super Module A) to SBB (Super Module B)"));
			AssertEquals("The log should be added : incident persistant and changes successfully saved", true, messageList.Any(msg => msg.Body == "Product Area changed from COR (Core Product) to DOM (Domestic Logistics)"));

			incident.SetLogTextForTest(ZString.Empty);
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Product = ProductTypes.Codes.WiseTechAcademy;
			incident.IM_Product = ProductTypes.Codes.CargoWiseOne;
			incident.ProductArea = "INT";
			incident.IM_Module = "SDD";
			AssertEquals("INT", incident.ProductArea);
			AssertEquals("SDD", incident.IM_Module);

			Factory.Save();
			messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals("The log should omit intermittent changes", false, messageList.Any(msg => msg.Body == "Product changed from WTA (test) to ETN (ediEnterprise / CargoWise One)"));
			AssertEquals("The log should omit intermittent changes", false, messageList.Any(msg => msg.Body == "Product changed from ETN (ediEnterprise / CargoWise One) to WTA (test)"));
			AssertEquals("The log should contain last changes", true, messageList.Any(msg => msg.Body == "Product changed from WTA (test) to CW1 (test)"));
			AssertEquals("The log should contain last changes", true, messageList.Any(msg => msg.Body == "Module changed from SBB (Super Module B) to SDD (Super Module D)"));
			AssertEquals("The log should contain last changes", true, messageList.Any(msg => msg.Body == "Product Area changed from DOM (Domestic Logistics) to INT (International Logistics)"));
		}

		public void TestDatabaseCurrentVersion()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			AssertNull(incident.Database);
			AssertEquals("", incident.DatabaseCurrentVersion);

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			incident.IM_LD = database.PK;
			AssertNotNull(incident.Database);
			AssertNull(incident.Database.CurrentVersion);
			AssertEquals("", incident.DatabaseCurrentVersion);

			var releaseBuild = Factory.NewWithValidTestData<ReleaseBuild>();
			database.LD_HL_CurrentRunningVersion = releaseBuild.PK;
			releaseBuild.HL_MajorVersion = 11;
			releaseBuild.HL_MinorVersion = 22;
			releaseBuild.HL_Release = 33;
			releaseBuild.HL_Patch = 44;
			AssertNotNull(incident.Database);
			AssertNotNull(incident.Database.CurrentVersion);
			AssertEquals("11.22.33.44", incident.DatabaseCurrentVersion);
		}

		public void TestDatabaseOrPrimaryProductionSystem()
		{
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			database.LD_LicenceType = DatabaseTypes.Codes.Production;
			database.LD_Product = ProductTypes.Codes.BorderWise;

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_LD = database.PK;
			AssertEquals(database.PK, incident.DatabaseOrPrimaryProductionSystem.PK);

			incident.IM_LD = ZGuid.Empty;
			AssertNull(incident.DatabaseOrPrimaryProductionSystem);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			incident.IM_OH_Client = org.PK;
			database.LD_OH_WebAccessOrg = org.PK;
			incident.IM_Product = ProductTypes.Codes.BorderWise;

			AssertEquals(database.PK, incident.DatabaseOrPrimaryProductionSystem.PK);
		}

		public void TestBindingOnlyProperties()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			ClientInvoiceDelivery delivery = org.LicCompany.InvoiceDeliveries.AddNew();
			delivery.L9_IsBilled = true;
			delivery.L9_RX_NKInvoiceCurrency = "IDR";
			Factory.Save();

			var incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			AssertNull(incident.EstimateForBinding);
			AssertNull(incident.QuoteForBinding);

			incident.FeatureRequestClientPK = org.PK;
			AssertEquals("IDR", incident.Estimate.CIE_RX_NKCurrency);
			AssertEquals("IDR", incident.Quote.CIQ_RX_NKCurrency);
			AssertEquals(incident.EstimateForBinding, incident.Estimate);
			AssertEquals(incident.QuoteForBinding, incident.Quote);
		}

		public void TestPopulateLicenceFromClient()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			var db = org.LicCompany.LicDatabases.AddNew();
			db.LD_Product = "SPH";
			db.LD_LicenceType = DatabaseTypes.Codes.Production;
			Factory.Save();

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "SPH";
			incident.IM_OH_Client = org.PK;

			AssertNotEquals(ZGuid.Empty, incident.IM_LD);
		}

		public void TestChangeClientCompay_DoNotChangeOrgAndContact()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "DDD", "COM", "SRV");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "TEST ORG 001";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact 1";
			contact.OC_Email = "c1@cargowise.com";
			Factory.Save();

			var oh = lic.Company.LC_OH;
			var oc = lic.Company.Header.Contacts[0].PK;

			var incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = oh;
			incident.IM_LD = lic.LA_LD;
			incident.IM_LCC = lic.ClientCompany.PK;
			incident.IM_OC_Contact = oc;
			Factory.Save();

			AssertEquals(incident.IM_OH_Client, oh);
			AssertEquals(incident.IM_OC_Contact, oc);
			AssertEquals(incident.ClientCompanyCode, "COM");

			var lcc2 = BillingTestHelper.CreateClientCompany(lic.Database, "C02");
			lcc2.LCC_OH = org.PK;
			Factory.Save();

			incident.ClientCompanyCode = "C02";
			AssertEquals(incident.IM_OH_Client, oh);
			AssertEquals(incident.IM_OC_Contact, oc);
			AssertEquals(incident.ClientCompanyCode, "C02");
			AssertEquals(incident.IM_LCC, lcc2.PK);
		}

		public void TestEnableWorkflowTemplateTriggersForIncidentDetailChanges()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "INC");
			template.P0_SubType1 = ProductTypes.Codes.Enterprise;

			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "track incident details";
			trigger.TriggerConditions.TriggerEventCode = Events.EditedARecord.Code;
			trigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			trigger.TemplateConditions.TemplateCondition2Value = "\"<IM_Description>\" == \"Test Trigger for Incident Detail Changes\"";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<IM_Priority>";
			action.PQ_FieldValue = "CR8";
			Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = "CR4";
			incident.ProductArea = "ARC";
			incident.IM_Module = "SAA";
			incident.IM_Language = "EN";
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			incident.Logs.AddNew(Events.EditedARecord, "test add Edit log");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			incident.IM_Description = "Test Trigger for Incident Detail Changes";
			Factory.Save();

			AssertEquals("CR8", incident.IM_Priority);
		}

		public void TestImportParentRelatedActivityInfoOnNew()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var supportIncident = Factory.NewWithValidTestData<SupportIncident>();

			var orgOpportunity = Factory.New<EDIOrgOpportunity>();
			orgOpportunity.P8_OH = orgHeader.PK;
			Factory.Save();

			var query = new ZQuery(GenPivotSchema.XX_Relation2ID, supportIncident.PK);
			var genPivots = Factory.Load<GenPivot>(query);
			AssertEquals(0, genPivots.Length);

			((IImportParentRelatedActivityInfoOnNew)supportIncident).ImportParentInfo(orgOpportunity, new ImportRelatedActivityNoDecisionFactory());

			genPivots = Factory.Load<GenPivot>(query);
			AssertEquals(1, genPivots.Length);

			var genPivot = genPivots.First();
			AssertEquals(genPivot.XX_RelationType, Core.Constants.GenPivotTypes.Opportunity);
			AssertEquals(genPivot.Relation1ID, orgOpportunity.PK);
			AssertEquals(genPivot.XX_Relation1TableCode, OrgOpportunitySchema.Constants.Prefix);
			AssertEquals(genPivot.Relation2ID, supportIncident.PK);
			AssertEquals(genPivot.XX_Relation2TableCode, IncidentMainSchema.Constants.Prefix);
		}

		public void TestDetachOpportunityParentRelatedActivity()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var supportIncident = Factory.NewWithValidTestData<SupportIncident>();

			var orgOpportunity = Factory.New<EDIOrgOpportunity>();
			orgOpportunity.P8_OH = orgHeader.PK;

			orgOpportunity.RelatedChildActivityPivotCollection.AddNewPivot(supportIncident);
			((IImportParentRelatedActivityInfoOnNew)supportIncident).ImportParentInfo(orgOpportunity, new ImportRelatedActivityNoDecisionFactory());
			Factory.Save();

			var genPivotQuery = new ZQuery(GenPivotSchema.XX_Relation2ID, supportIncident.PK);
			var genPivots = Factory.Load<GenPivot>(genPivotQuery);
			AssertEquals(1, genPivots.Length);

			var relatedActivityPivotQuery = new ZQuery(ViewRelatedActivityPivotSchema.RAP_ChildActivityID, supportIncident.PK);
			var relatedActivityPivots = Factory.Load<ViewRelatedActivityPivot>(relatedActivityPivotQuery);
			AssertEquals(1, relatedActivityPivots.Length);

			supportIncident.RelatedItems.Remove(orgOpportunity);

			genPivots = Factory.Load<GenPivot>(genPivotQuery);
			AssertEquals(0, genPivots.Length);

			relatedActivityPivots = Factory.Load<ViewRelatedActivityPivot>(relatedActivityPivotQuery);
			AssertEquals(0, relatedActivityPivots.Length);
		}

		public void TestDetachOpportunityChildRelatedActivity()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			var orgOpportunity = Factory.New<EDIOrgOpportunity>();
			orgOpportunity.P8_OH = orgHeader.PK;
			Factory.Save();

			var relatedActivityPivotQuery = new ZQuery(ViewRelatedActivityPivotSchema.RAP_ParentActivityID, supportIncident.PK);
			var relatedActivityPivots = Factory.Load<ViewRelatedActivityPivot>(relatedActivityPivotQuery);
			AssertEquals(0, relatedActivityPivots.Length);

			supportIncident.RelatedItems.Add(orgOpportunity);

			relatedActivityPivots = Factory.Load<ViewRelatedActivityPivot>(relatedActivityPivotQuery);
			AssertEquals(1, relatedActivityPivots.Length);

			supportIncident.RelatedItems.Remove(orgOpportunity);

			relatedActivityPivots = Factory.Load<ViewRelatedActivityPivot>(relatedActivityPivotQuery);
			AssertEquals(0, relatedActivityPivots.Length);
		}

		public void TestAttachOpportunityChildRelatedActivity()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			var orgOpportunity = Factory.New<EDIOrgOpportunity>();
			orgOpportunity.P8_OH = orgHeader.PK;
			Factory.Save();

			var relatedActivityPivotQuery = new ZQuery(ViewRelatedActivityPivotSchema.RAP_ParentActivityID, supportIncident.PK);
			var relatedActivityPivots = Factory.Load<ViewRelatedActivityPivot>(relatedActivityPivotQuery);
			AssertEquals(0, relatedActivityPivots.Length);

			supportIncident.RelatedItems.Add(orgOpportunity);
			Factory.Save();

			relatedActivityPivots = Factory.Load<ViewRelatedActivityPivot>(relatedActivityPivotQuery);
			AssertEquals(1, relatedActivityPivots.Length);
			AssertEquals("Sales Relation Tree exists", relatedActivityPivots[0].RAP_ParentActivityID, relatedActivityPivots[0].RAP_SalesRelationTreeID);
		}

		public void TestImportChildInfoOnAttach_CreateOpportunityPivot()
		{
			var supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			var orgOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			var query = new ZQuery(GenPivotSchema.XX_Relation1ID, supportIncident.PK);
			var genPivots = Factory.Load<GenPivot>(query);
			AssertEquals(0, genPivots.Length);

			((IImportChildRelatedActivityInfoOnAttach)supportIncident).ImportChildInfo(orgOpportunity, new ImportRelatedActivityNoDecisionFactory());
			genPivots = Factory.Load<GenPivot>(query);
			AssertEquals(1, genPivots.Length);

			var genPivot = genPivots.First();
			AssertEquals(genPivot.XX_RelationType, Core.Constants.GenPivotTypes.Opportunity);
			AssertEquals(genPivot.Relation1ID, supportIncident.PK);
			AssertEquals(genPivot.XX_Relation1TableCode, IncidentMainSchema.Constants.Prefix);
			AssertEquals(genPivot.Relation2ID, orgOpportunity.PK);
			AssertEquals(genPivot.XX_Relation2TableCode, OrgOpportunitySchema.Constants.Prefix);
		}

		public void TestImportChildInfoOnAttach_DeleteInverseOpportunityPivot()
		{
			var supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			var orgOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();

			var opportunityPivot = Factory.New<GenPivot>();
			opportunityPivot.XX_RelationType = Core.Constants.GenPivotTypes.Opportunity;
			opportunityPivot.XX_Relation1ID = orgOpportunity.PK;
			opportunityPivot.XX_Relation1TableCode = OrgOpportunitySchema.Constants.Prefix;
			opportunityPivot.XX_Relation2ID = supportIncident.PK;
			opportunityPivot.XX_Relation2TableCode = IncidentMainSchema.Constants.Prefix;

			Factory.Save();

			var pivotQuery = new ZQuery(GenPivotSchema.XX_Relation1ID, supportIncident.PK);
			var inversePivotQuery = new ZQuery(GenPivotSchema.XX_Relation2ID, supportIncident.PK);
			AssertEquals("Precondition: INC > OPP pivot should not have been created yet", 0, Factory.Load<GenPivot>(pivotQuery).Length);
			AssertEquals("Precondition: OPP > INC pivot should not have been deleted yet", 1, Factory.Load<GenPivot>(inversePivotQuery).Length);

			((IImportChildRelatedActivityInfoOnAttach)supportIncident).ImportChildInfo(orgOpportunity, new ImportRelatedActivityNoDecisionFactory());

			AssertEquals("INC > OPP pivot should have been created", 1, Factory.Load<GenPivot>(pivotQuery).Length);
			AssertEquals("OPP > INC pivot should have been deleted", 0, Factory.Load<GenPivot>(inversePivotQuery).Length);
		}

		public void TestImportChildInfoOnDetach_DeleteOpportunityPivot()
		{
			var supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			var orgOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();

			var newGenPivot = Factory.New<GenPivot>();
			newGenPivot.XX_RelationType = Core.Constants.GenPivotTypes.Opportunity;
			newGenPivot.XX_Relation1ID = supportIncident.PK;
			newGenPivot.XX_Relation1TableCode = IncidentMainSchema.Constants.Prefix;
			newGenPivot.XX_Relation2ID = orgOpportunity.PK;
			newGenPivot.XX_Relation2TableCode = OrgOpportunitySchema.Constants.Prefix;
			Factory.Save();

			var query = new ZQuery(GenPivotSchema.XX_Relation1ID, supportIncident.PK);
			var genPivots = Factory.Load<GenPivot>(query);
			AssertEquals(1, genPivots.Length);

			var genPivot = genPivots.First();
			AssertEquals(genPivot.XX_RelationType, Core.Constants.GenPivotTypes.Opportunity);
			AssertEquals(genPivot.Relation1ID, supportIncident.PK);
			AssertEquals(genPivot.XX_Relation1TableCode, IncidentMainSchema.Constants.Prefix);
			AssertEquals(genPivot.Relation2ID, orgOpportunity.PK);
			AssertEquals(genPivot.XX_Relation2TableCode, OrgOpportunitySchema.Constants.Prefix);

			((IImportChildRelatedActivityInfoOnDetach)supportIncident).ImportChildInfo(orgOpportunity, new ImportRelatedActivityNoDecisionFactory());

			genPivots = Factory.Load<GenPivot>(query);
			AssertEquals(0, genPivots.Length);
		}

		public void TestCustomFieldTriggersTemplate()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "INC");
			template.P0_SubType1 = ProductTypes.Codes.Enterprise;

			var templateWorkflow = bmTestHelper.CreateWorkflow(template);
			var templateTask1 = bmTestHelper.CreateTask(template, templateWorkflow, description: "avada");

			var templateTask2 = bmTestHelper.CreateTask(template, templateWorkflow, description: "Investigate");
			templateTask2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask2.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "avada kedavra";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			trigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			trigger.TemplateConditions.TemplateCondition2Value = "\"<GetCustomField(Service Type)>\" == \"Y\"";

			var rule = Factory.New<GenCustomAddOnRule>();
			rule.XR_Code = "Service Type";
			rule.XR_Description = "Service Type";
			rule.XR_SourceCode = @"<sourceCode>
  <rules>
	<rule code=""InvalidCode"" enabled=""true"">
	  <details>
		<codeDescriptionList>
		  <codeDescription code=""Y"" description=""Yes"" />
		  <codeDescription code=""N"" description=""No"" />
		</codeDescriptionList>
	  </details>
	</rule>
	<rule code=""CreateEvent"" enabled=""true"">
	  <details>
		<CreateEventRuleCode>Z00</CreateEventRuleCode>
		<CreateEventRuleReference>WINGARDIUM LEVIOSA</CreateEventRuleReference>
	  </details>
	</rule>
	<rule code=""DateTimeFormat"" enabled=""false"">
	  <details>
		<format>Short</format>
	  </details>
	</rule>
	<rule code=""CheckEntered"" enabled=""false"">
	  <details />
	</rule>
  </rules>
</sourceCode>";

			var custom = template.GenCustomColumnDefinitions.AddNew();
			custom.XC_Name = "Service Type";
			custom.XC_Type = AddOnColumnDataType.Codes.String;
			custom.XC_XR = rule.PK;

			Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = "CR4";
			incident.ProductArea = "ARC";
			incident.IM_Module = "SAA";
			incident.IM_Language = "EN";

			Factory.Save();

			AssertEquals(1, incident.WorkflowItems.Count);
			AssertEquals("Pre-condition", 0, incident.WorkflowItems.Triggers.Count);
			AssertEquals("Pre-condition", 1, incident.WorkflowItems.Tasks.Count);

			(incident as ICustomFieldProvider).GetCustomBusinessObject()["__SERVICE TYPE__prop__ZString"] = (ZString)"Y";
			Factory.Save();

			incident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
			AssertEquals("Should add a trigger", 1, incident.WorkflowItems.Triggers.Count);
			AssertEquals("Should not add new tasks", 1, incident.WorkflowItems.Tasks.Count);
		}

		#region IConversationBroadcastRecipient

		public void TestGenerateAndSendBroadcastEmailNotifications()
		{
			var incident = SupportIncidentEConversationTest.CreateIncidentWithoutSave(Factory);
			incident.IM_Description = "incy";
			var org = incident.Client;
			var contact1 = org.Contacts[0];
			contact1.OC_Email = "or@test.com.au";
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Bob";
			contact2.OC_Email = "bob@test.com.au";
			OrgContact contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Al";
			contact3.OC_Email = "al@test.com.au";

			var staffRecipient = Factory.NewWithValidTestData<GlbStaff>();
			staffRecipient.GS_EmailAddress = "arc@use.com.au";

			var convo = incident.EConversation.JobConversationForTest;
			var contactParticipant1 = convo.Participants.AddNew();
			contactParticipant1.JCP_ParticipantTableCode = contact1.TablePrefix;
			contactParticipant1.JCP_ParticipantID = contact1.PK;
			contactParticipant1.JCP_IsSubscribed = true;

			var contactParticipant2 = convo.Participants.AddNew();
			contactParticipant2.JCP_ParticipantTableCode = contact2.TablePrefix;
			contactParticipant2.JCP_ParticipantID = contact2.PK;
			contactParticipant2.JCP_IsSubscribed = true;

			var contactParticipant3 = convo.Participants.AddNew();
			contactParticipant3.JCP_ParticipantTableCode = contact3.TablePrefix;
			contactParticipant3.JCP_ParticipantID = contact3.PK;
			contactParticipant3.JCP_IsSubscribed = false;

			var staffParticipant = convo.Participants.AddNew();
			staffParticipant.JCP_ParticipantTableCode = staffRecipient.TablePrefix;
			staffParticipant.JCP_ParticipantID = staffRecipient.PK;
			staffParticipant.JCP_IsSubscribed = true;
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var newMessage1 = convo.Messages.AddNew();
			newMessage1.JCM_Body = "Onyaaaaa";
			newMessage1.JCM_PostedTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			newMessage1.JCM_JCP_Participant = contactParticipant1.PK;
			var newMessage2 = convo.Messages.AddNew();
			newMessage2.JCM_Body = "Alrightyy";
			newMessage2.JCM_PostedTimeUtc = ZDateTime.UtcNow;
			newMessage2.JCM_JCP_Participant = contactParticipant1.PK;
			((IConversationBroadcastRecipient)incident).GenerateAndSendBroadcastEmailNotifications();

			AssertEquals("Should have sent the email to the subscribed participants", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients[0].Email == contact1.OC_Email);
			AssertEquals("Only main recipient should be the incident contact", 1, email.Recipients.Count);
			AssertEquals("Should cc the subscribed participants", 1, email.CCRecipients.Count);
			AssertEquals("Should cc the subscribed participants", contact2.OC_Email, email.CCRecipients[0]);
			AssertEquals("Subject", FormattableString.Invariant($"Update on Incident: {incident.IM_IncidentNumber} - {incident.IM_Description}"), email.Subject);
			AssertContains("Should include first message body", newMessage1.JCM_Body, email.Body);
			AssertContains("Should include second message body", newMessage2.JCM_Body, email.Body);
		}

		public void TestGenerateAndSendBroadcastEmailNotificationsWhenNoRecipientEmailsShouldNotThrowException()
		{
			var incident = SupportIncidentEConversationTest.CreateIncidentWithoutSave(Factory);
			incident.IM_Description = "incy";
			var org = incident.Client;
			var contact1 = org.Contacts[0];
			contact1.OC_Email = string.Empty;
			incident.IM_OC_Contact = contact1.PK;
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Bob";
			contact2.OC_Email = string.Empty;

			var staffRecipient = Factory.NewWithValidTestData<GlbStaff>();
			staffRecipient.GS_EmailAddress = "arc@use.com.au";

			var convo = incident.EConversation.JobConversationForTest;
			var contactParticipant1 = convo.Participants.AddNew();
			contactParticipant1.JCP_ParticipantTableCode = contact1.TablePrefix;
			contactParticipant1.JCP_ParticipantID = contact1.PK;
			contactParticipant1.JCP_IsSubscribed = true;

			var contactParticipant2 = convo.Participants.AddNew();
			contactParticipant2.JCP_ParticipantTableCode = contact2.TablePrefix;
			contactParticipant2.JCP_ParticipantID = contact2.PK;
			contactParticipant2.JCP_IsSubscribed = true;

			var staffParticipant = convo.Participants.AddNew();
			staffParticipant.JCP_ParticipantTableCode = staffRecipient.TablePrefix;
			staffParticipant.JCP_ParticipantID = staffRecipient.PK;
			staffParticipant.JCP_IsSubscribed = true;
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var newMessage1 = convo.Messages.AddNew();
			newMessage1.JCM_Body = "Onyaaaaa";
			newMessage1.JCM_PostedTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			newMessage1.JCM_JCP_Participant = contactParticipant1.PK;
			var newMessage2 = convo.Messages.AddNew();
			newMessage2.JCM_Body = "Alrightyy";
			newMessage2.JCM_PostedTimeUtc = ZDateTime.UtcNow;
			newMessage2.JCM_JCP_Participant = contactParticipant1.PK;
			AssertNoExceptionThrown("Should not throw exception if there are no recipients to send an email to", () => ((IConversationBroadcastRecipient)incident).GenerateAndSendBroadcastEmailNotifications());

			AssertEquals("Should have sent the email to the subscribed staff participant", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email2 = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Only main recipient should be the incident staff", 1, email2.Recipients.Count);
			AssertEquals("Should be the staff email", staffRecipient.GS_EmailAddress, email2.Recipients[0].Email);
			AssertEquals("Should have no ccs", 0, email2.CCRecipients.Count);
			AssertEquals("Subject", FormattableString.Invariant($"New Messages in {incident.HumanReadableName}"), email2.Subject);
			AssertContains("Should include first message body", newMessage1.JCM_Body, email2.Body);
			AssertContains("Should include second message body", newMessage2.JCM_Body, email2.Body);
		}

		#endregion

		#region Service Type

		void ConfigServiceTypeRegistry()
		{
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "Enterprise", true);

			var module1 = product.ServiceTypeModuleMappings.AddNew("ARM", "Module ARC", "ARC", false);
			module1.ServiceTypeMappings.AddNew("SIM");
			module1.ServiceTypeMappings.AddNew("TEA");

			EDIDataRegistry.Instance.ServiceTypeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		public void TestServiceTypeProperty()
		{
			ConfigServiceTypeRegistry();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			incident.ProductArea = "ARC";
			incident.IM_Module = "ARM";

			incident.IM_ServiceType = "SIM";
			AssertNoErrors(incident.IM_ServiceTypeInfo);

			incident.IM_ServiceType = "PER";
			AssertHasErrors(incident.IM_ServiceTypeInfo);
		}

		public void TestServiceType_WorkflowIsTriggered()
		{
			ConfigServiceTypeRegistry();

			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "INC");
			var templateWorkflow = bmTestHelper.CreateWorkflow(template);
			var templateTask1 = bmTestHelper.CreateTask(template, templateWorkflow, description: "Marshmellow");
			var templateTask2 = bmTestHelper.CreateTask(template, templateWorkflow, description: "Happier");

			templateTask1.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask1.TemplateConditions.TemplateCondition2Value = "\"<P9_Description>\"!=\"\"";
			templateTask2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask2.TemplateConditions.TemplateCondition2Value = "\"<P9_Description>\"!=\"\"";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Priority = "CR4";
			incident.ProductArea = "ARC";
			incident.IM_Module = "ARM";

			incident.ClearAllNotifications();

			Factory.Save();

			AssertEquals(0, incident.WorkflowItems.Tasks.Count);

			incident.IM_ServiceType = "TEA";

			AssertEquals(2, incident.WorkflowItems.Tasks.Count);
			AssertSequencesEqual("Should apply the workflow", new[] { "Marshmellow", "Happier" }, incident.WorkflowItems.Tasks.Cast<ProcessTask>().Select(t => t.P9_Description.ToString()));
		}

		#endregion

		public void TestCloseIncidentPopupForm_IsCalledWhenIncidentCloses()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("ARC", "ARC");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("SAA", "Module AAA", "ARC", false);
			product.ModuleMappings.AddNew("SBB", "Module BBB", "ARC", false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var incident = Factory.NewWithValidTestData<SupportIncidentForTest>();
			incident.IM_IncidentNumber = "CS00098432";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			incident.IM_Priority = "CR6";
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.ProductArea = "ARC";
			incident.IM_Module = "SBB";
			incident.IM_Language = "EN";
			var task = incident.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Type = "UDF";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			AssertEquals(false, incident.CloseIncidentWasCalled);

			incident.IM_Module = "SAA";

			AssertEquals(true, incident.CloseIncidentWasCalled);
		}

		public void TestIncidentManagementLink()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_IncidentGroupNumber = "ING000001";
			var link = Factory.New<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;

			Factory.Save();
			AssertEquals("Should retrieve group from link", link, incident.IncidentManagementLink);
		}

		public void TestWorkflowPropertiesShouldNotBeRecalculatedIfIncidentIsGroupControlled()
		{
			var controlStage = "ZZZ";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(controlStage, "desc", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, controlIncidents: true);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "INC000001";
			var task1 = incident1.WorkflowItems.AddNew();
			task1.P9_Sequence = 10;
			task1.P9_Status = "CLS";

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_IncidentNumber = "INC000002";
			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group2.ING_IncidentGroupNumber = "ING000002";
			group2.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group2.ING_Status = controlStage;
			var link2 = Factory.New<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group2.PK;
			link2.INL_IsGroupControlled = false;
			var task2 = incident2.WorkflowItems.AddNew();
			task2.P9_Sequence = 10;
			task2.P9_Status = "CLS";

			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_IncidentNumber = "INC000003";
			var group3 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group3.ING_IncidentGroupNumber = "ING000003";
			group3.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group3.ING_Status = controlStage;
			var link3 = Factory.New<IncidentManagementLink>();
			link3.INL_IM_Incident = incident3.PK;
			link3.INL_ING_Group = group3.PK;

			var task3 = incident3.WorkflowItems.AddNew();
			task3.P9_Sequence = 10;
			task3.P9_Status = "CLS";
			Factory.Save();

			AssertEquals("Precondition", true, group2.NowStage.ControlIncidents);
			AssertEquals("Precondition", true, group3.NowStage.ControlIncidents);
			AssertEquals("Precondition", false, link2.INL_IsGroupControlled);
			AssertEquals("Precondition", false, incident1.IsGroupControlled);
			AssertEquals("Precondition", false, incident2.IsGroupControlled);
			AssertEquals("Precondition", true, incident3.IsGroupControlled);

			incident1.IM_Status = IncidentMainLookups.Status.Suspended;
			incident2.IM_Status = IncidentMainLookups.Status.Suspended;
			incident3.IM_Status = IncidentMainLookups.Status.Suspended;
			incident1.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Suspended.Deferred;
			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Suspended.Deferred;
			incident3.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Suspended.Deferred;

			Factory.Save();

			AssertEquals("Should be recalculated", IncidentMainLookups.Status.Closed, incident1.IM_Status);
			AssertEquals("Should be recalculated", IncidentMainLookups.Status.Closed, incident2.IM_Status);
			AssertEquals("Should not be recalculated since it's group controlled", IncidentMainLookups.Status.Suspended, incident3.IM_Status);
			AssertEquals("Should be recalculated", SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident1.IM_ResolutionCode);
			AssertEquals("Should be recalculated", SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident2.IM_ResolutionCode);
			AssertEquals("Should not be recalculated since it's group controlled", SupportIncidentLookups.DispositionList.Constants.Suspended.Deferred, incident3.IM_ResolutionCode);
		}

		public void TestIsGroupControlled_DeletedLink()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var link = Factory.New<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;

			group.ING_Status = "INV";
			AssertEquals(true, group.ControlIncidents);

			Factory.Save();
			AssertEquals("Precondition", link, incident.IncidentManagementLink);
			AssertEquals("Precondition", true, incident.IsGroupControlled);

			link.Delete();
			AssertNoExceptionThrown("Should not try to access deleted row", () => _ = incident.IsGroupControlled);
			AssertEquals("Should return false if the link is deleted", false, incident.IsGroupControlled);
			AssertNull(incident.IncidentManagementLink);
		}

		public void TestIsGroupControlled_DeletedAndRelinkedLink()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "INC000001";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = true;

			group.ING_Status = "INV";
			AssertEquals(true, group.ControlIncidents);

			Factory.Save();
			AssertEquals("Precondition", link1, incident.IncidentManagementLink);
			AssertEquals("Precondition", true, incident.IsGroupControlled);

			var link2 = Factory.New<IncidentManagementLink>();
			link2.INL_IM_Incident = incident.PK;
			link2.INL_ING_Group = group.PK;
			link2.INL_IsGroupControlled = false;
			link1.Delete();

			AssertNoExceptionThrown("Should not try to access deleted row", () => _ = incident.IsGroupControlled);
			AssertEquals("Should be linked to new row", link2, incident.IncidentManagementLink);
			AssertEquals(false, incident.IsGroupControlled);

			link2.INL_IsGroupControlled = true;
			AssertEquals(true, incident.IsGroupControlled);
		}

		public void TestRecalculateProductArea_IsControlledByGroup()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "AAA Enabled Module", ProductAreaList.Codes.ARC, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = "AAA";
			incident.IM_Priority = "CR8";

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = "MIM";
			group.ING_IncidentGroupNumber = "ING000001";
			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			group.ING_Status = "INV";
			AssertEquals("Precondition", true, group.ControlIncidents);
			AssertEquals("Precondition", false, link1.IsControlled);

			incident.ProductArea = "ZZZ";
			incident.RecalculateProductArea();
			AssertEquals("Should recalculate product area", ProductAreaList.Codes.ARC, incident.ProductArea);

			link1.INL_IsGroupControlled = true;
			AssertEquals("Precondition", true, link1.IsControlled);

			incident.ProductArea = "ZZZ";
			incident.RecalculateProductArea();
			AssertEquals("Should not recalculate product area since it's group controlled", "ZZZ", incident.ProductArea);
		}

		public void TestAddMessageFromSupport_Group_ShouldAddMessageSentEvent()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var message = "Test for AddMessageFromSupport";
			incident.AddMessageFromSupport(message);

			var incidentLog = incident.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent);
			AssertNull("Should not add log if not connected to a group", incidentLog);

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			Factory.Save();

			incident.AddMessageFromSupport(message);

			incidentLog = incident.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent);
			AssertNotNull("Should add log if connected to a group", incidentLog);
		}

		public void TestAddMessageFromSupport_Group_BroadcastShouldNotAddMessageSentEvent()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var message = "Test for AddMessageFromSupport";
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			Factory.Save();

			incident.AddMessageFromSupport(message, shouldAddMessageSentEvent: false);

			var incidentLog = incident.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent);
			AssertNull("Should not add message sent log if broadcasting", incidentLog);
		}

		public void TestAddStaffMessageToCustomer_Group_ShouldAddMessageSentEvent()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var message = "Test for AddMessageFromSupport";

			incident.AddStaffMessageToCustomer(message);
			var incidentLog = incident.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent);
			AssertNull("Should not add log if not connected to a group", incidentLog);

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			Factory.Save();

			incident.AddStaffMessageToCustomer(message);

			incidentLog = incident.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent);
			AssertNotNull("Should add log if connected to a group", incidentLog);
		}

		public void TestAddStaffMessageToCustomer_Group_BroadcastShouldNotAddMessageSentEvent()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var message = "Test for AddMessageFromSupport";

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			Factory.Save();

			incident.AddStaffMessageToCustomer(message, shouldAddMessageSentEvent: false);

			var incidentLog = incident.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent);
			AssertNull("Should not add message sent log if broadcasting", incidentLog);
		}

		[TestDate(2024, 10, 01, 08, 00, 00)]
		public void TestIncidentMetrics_IncidentCreated()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "CS11111111";
			Factory.Save();
			AssertEquals("Precondition: ", new ZDateTime(2024, 10, 1, 8, 0, 0), incident.IM_SystemCreateTimeUtc);

			var firstResponseTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS11111111", IncidentMetricConstants.FirstResponseTime);
			AssertMetrics(incident, firstResponseTimeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.FirstResponseTime, firstStartTimeUtc: new ZDateTime(2024, 10, 1, 8, 0, 0), firstEndTimeUtc: ZDateTime.Empty, firstCalculatedMetric: 0, firstMetricCount: 0);

			var totalERequestAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS11111111", IncidentMetricConstants.TotalERequestAge);
			AssertMetrics(incident, totalERequestAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.TotalERequestAge, firstStartTimeUtc: new ZDateTime(2024, 10, 1, 8, 0, 0), firstEndTimeUtc: ZDateTime.Empty, firstCalculatedMetric: 0, firstMetricCount: 0);

			var totalResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS11111111", IncidentMetricConstants.TotalResolutionAge);
			AssertMetrics(incident, totalResolutionAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.TotalResolutionAge, firstStartTimeUtc: new ZDateTime(2024, 10, 1, 8, 0, 0), firstEndTimeUtc: ZDateTime.Empty, firstCalculatedMetric: 0, firstMetricCount: 0);
		}

		[TestDate(2024, 10, 02, 08, 00, 00)]
		public void TestIncidentMetrics_HumanResponse()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			var contact = licence.Company.Header.Contacts.AddNew();
			contact.OC_ContactName = "Joe";
			contact.OC_Email = "joe@test.org";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "CS22222222";
			incident.IM_OC_Contact = contact.PK;
			Factory.Save();
			AssertEquals("Precondition: ", new ZDateTime(2024, 10, 2, 8, 0, 0), incident.IM_SystemCreateTimeUtc);

			TestDateAttribute.AddHours(1);
			incident.AddStaffMessageToCustomer("Support First Response.");
			Factory.Save();

			var firstResponseTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS22222222", IncidentMetricConstants.FirstResponseTime);
			AssertMetrics(incident, firstResponseTimeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.FirstResponseTime, firstStartTimeUtc: new ZDateTime(2024, 10, 2, 8, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 2, 9, 0, 0), firstCalculatedMetric: 3600, firstMetricCount: 1);

			TestDateAttribute.AddHours(1);
			incident.CloseIncident(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();

			TestDateAttribute.AddHours(1);
			AddMessageFromContact(incident, "Client First Reply.");
			Factory.Save();
			incident.EnsureOpenTask(false, true, false);
			Factory.Save();

			var mostRecentIROEvent = incident.GetMostRecentLogByEventCode(Events.IncidentReopenedCode);
			AssertEquals("Precondition:", new ZDateTime(2024, 10, 2, 11, 0, 0), mostRecentIROEvent.SL_PostedTimeUtc);
			AssertEquals("Precondition:", DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);
			var nextResponseTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS22222222", IncidentMetricConstants.NextResponseTime);
			AssertMetrics(incident, nextResponseTimeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.NextResponseTime, firstStartTimeUtc: new ZDateTime(2024, 10, 2, 11, 0, 0), firstEndTimeUtc: ZDateTime.Empty, firstCalculatedMetric: 0, firstMetricCount: 0);

			TestDateAttribute.AddHours(1);
			incident.AddStaffMessageToCustomer("Support Second Response.");
			incident.CloseIncident(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();

			var mostRecentAwaitingEvent = incident.GetMostRecentLogByEventCodes(incident.awaitingEventCodes);
			AssertEquals("Precondition:", new ZDateTime(2024, 10, 2, 12, 0, 0), mostRecentAwaitingEvent.SL_PostedTimeUtc);
			AssertEquals("Precondition:", DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
			nextResponseTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS22222222", IncidentMetricConstants.NextResponseTime);
			AssertMetrics(incident, nextResponseTimeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.NextResponseTime, firstStartTimeUtc: new ZDateTime(2024, 10, 2, 11, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 2, 12, 0, 0), firstCalculatedMetric: 3600, firstMetricCount: 1);

			TestDateAttribute.AddHours(1);
			AddMessageFromContact(incident, "Client Second Reply.");
			Factory.Save();
			incident.EnsureOpenTask(false, true, false);
			Factory.Save();

			nextResponseTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS22222222", IncidentMetricConstants.NextResponseTime);
			AssertMetrics(incident, nextResponseTimeMetrics, metricsLength: 2, firstMetricCode: IncidentMetricConstants.NextResponseTime, firstStartTimeUtc: new ZDateTime(2024, 10, 2, 13, 0, 0), firstEndTimeUtc: ZDateTime.Empty, firstCalculatedMetric: 0, firstMetricCount: 0);

			TestDateAttribute.AddHours(1);
			incident.AddStaffMessageToCustomer("Support Third Response.");
			TestDateAttribute.AddHours(1);
			incident.AddStaffMessageToCustomer("Support Forth Response.");
			TestDateAttribute.AddHours(1);
			incident.AddStaffMessageToCustomer("Support Fifth Response.");
			Factory.Save();

			AssertEquals("Precondition:", DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);
			nextResponseTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS22222222", IncidentMetricConstants.NextResponseTime);
			AssertMetrics(incident, nextResponseTimeMetrics, metricsLength: 2, firstMetricCode: IncidentMetricConstants.NextResponseTime, firstStartTimeUtc: new ZDateTime(2024, 10, 2, 13, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 2, 14, 0, 0), firstCalculatedMetric: 3600, firstMetricCount: 1);
			var additionalResponseTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS22222222", IncidentMetricConstants.AdditionalResponseTime);
			AssertMetrics(incident, additionalResponseTimeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.AdditionalResponseTime, firstStartTimeUtc: new ZDateTime(2024, 10, 2, 14, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 2, 16, 0, 0), firstCalculatedMetric: 7200, firstMetricCount: 2);

			TestDateAttribute.AddHours(1);
			incident.AddStaffMessageToCustomer("Support Sixth Response.");
			incident.CloseIncident(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
			Factory.Save();

			additionalResponseTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS22222222", IncidentMetricConstants.AdditionalResponseTime);
			AssertMetrics(incident, additionalResponseTimeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.AdditionalResponseTime, firstStartTimeUtc: new ZDateTime(2024, 10, 2, 14, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 2, 17, 0, 0), firstCalculatedMetric: 10800, firstMetricCount: 3);

			TestDateAttribute.AddHours(1);
			AddMessageFromContact(incident, "Client Third Reply.");
			Factory.Save();
			incident.EnsureOpenTask(false, true, false);
			Factory.Save();

			nextResponseTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS22222222", IncidentMetricConstants.NextResponseTime);
			AssertMetrics(incident, nextResponseTimeMetrics, metricsLength: 3, firstMetricCode: IncidentMetricConstants.NextResponseTime, firstStartTimeUtc: new ZDateTime(2024, 10, 2, 18, 0, 0), firstEndTimeUtc: ZDateTime.Empty, firstCalculatedMetric: 0, firstMetricCount: 0);

			TestDateAttribute.AddHours(1);
			incident.AddStaffMessageToCustomer("Support Seventh Response.");
			Factory.Save();

			nextResponseTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS22222222", IncidentMetricConstants.NextResponseTime);
			AssertMetrics(incident, nextResponseTimeMetrics, metricsLength: 3, firstMetricCode: IncidentMetricConstants.NextResponseTime, firstStartTimeUtc: new ZDateTime(2024, 10, 2, 18, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 2, 19, 0, 0), firstCalculatedMetric: 3600, firstMetricCount: 1);

			TestDateAttribute.AddHours(1);
			AddMessageFromContact(incident, "Client Forth Reply.");
			Factory.Save();

			AssertEquals("Precondition:", DispositionList.Constants.Working.WorkInProgress, incident.IM_ResolutionCode);
			nextResponseTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS22222222", IncidentMetricConstants.NextResponseTime);
			AssertMetrics(incident, nextResponseTimeMetrics, metricsLength: 4, firstMetricCode: IncidentMetricConstants.NextResponseTime, firstStartTimeUtc: new ZDateTime(2024, 10, 2, 20, 0, 0), firstEndTimeUtc: ZDateTime.Empty, firstCalculatedMetric: 0, firstMetricCount: 0);

			incident.MetricsCollection.RemoveAll(m => m.IME_MetricCode == IncidentMetricConstants.FirstResponseTime || m.IME_MetricCode == IncidentMetricConstants.NextResponseTime || m.IME_MetricCode == IncidentMetricConstants.AdditionalResponseTime);
			AssertNoExceptionThrown(() =>
			{
				TestDateAttribute.AddHours(1);
				incident.AddStaffMessageToCustomer("Support eighth Response.");
				incident.CloseIncident(DispositionList.Constants.Closed.ClosedAwaitingClientResponse, "");
				Factory.Save();
			});
		}

		[TestDate(2024, 10, 02, 08, 00, 00)]
		public void TestCR7IncidentMetricsHasNoException()
		{
			AssertNoExceptionThrown(() =>
			{
				var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
				var contact = licence.Company.Header.Contacts.AddNew();
				contact.OC_ContactName = "Joe";
				contact.OC_Email = "joe@test.org";

				var incident = Factory.NewWithValidTestData<SupportIncident>();
				incident.IM_IncidentNumber = "CS22222222";
				incident.IM_Priority = "CR7";
				incident.IM_OC_Contact = contact.PK;
				Factory.Save();

				TestDateAttribute.AddHours(1);
				incident.AddStaffMessageToCustomer("Support Response 1.");
				Factory.Save();
				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided, "");
				Factory.Save();

				TestDateAttribute.AddHours(1);
				AddMessageFromContact(incident, "Client Reply 1.");
				Factory.Save();
				incident.EnsureOpenTask(false, true, false);
				Factory.Save();

				TestDateAttribute.AddHours(1);
				incident.IM_ResolutionCode = "ADD";
				var workItem = Factory.NewWithValidTestData<NewWorkItem>();
				incident.RelatedItems.Add(workItem);
				incident.AddStaffMessageToCustomer("Support Response 2.");
				Factory.Save();

				TestDateAttribute.AddHours(1);
				incident.AddStaffMessageToCustomer("Support Response 3.");
				Factory.Save();
				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided, "");
				Factory.Save();

				TestDateAttribute.AddHours(1);
				AddMessageFromContact(incident, "Client Reply 2.");
				Factory.Save();
				incident.EnsureOpenTask(false, true, false);
				Factory.Save();

				TestDateAttribute.AddHours(1);
				incident.AddStaffMessageToCustomer("Support Response 4.");
				Factory.Save();
				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.DevelopmentEstimateProvided, "");
				Factory.Save();
			});
		}

		void AddMessageFromContact(SupportIncident incident, string msg)
		{
			var conversation = incident.EConversation.JobConversationForTest;
			var reportingContact = incident.Contact;

			var contactParticipant = conversation.Participants.FirstOrDefault(x => (x.Parent as OrgContact) == reportingContact);
			if (contactParticipant == null)
			{
				contactParticipant = conversation.Participants.AddNew();
				contactParticipant.JCP_ParticipantTableCode = reportingContact.TablePrefix;
				contactParticipant.JCP_ParticipantID = reportingContact.PK;
			}

			var webMsg1 = conversation.Messages.AddNew(contactParticipant, msg, false);
			webMsg1.JCM_IsLocal = false;
		}

		[TestDate(2024, 10, 3, 08, 00, 00)]
		public void TestIncidentMetrics_CloseAndReopen()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var allCode = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, allCode, allCode);
			var resolutionCode = "YYY";
			var resolutionDescription = "YYY is resolution";
			var nonResolutionCode = "ZZZ";
			var nonResolutionDescription = "ZZZ is not resolution";
			registryValue.AddSystemChildren(registryValue.Add(resolutionCode, (NoResString)resolutionDescription, supportParent, ZBool.True));
			registryValue.AddSystemChildren(registryValue.Add(nonResolutionCode, (NoResString)nonResolutionDescription, supportParent, ZBool.False));

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var incident = Factory.NewWithValidTestData<SupportIncident>();
				incident.IM_IncidentNumber = "CS33333333";
				incident.IM_Priority = "CR1";
				Factory.Save();
				AssertEquals("Precondition: ", new ZDateTime(2024, 10, 3, 8, 0, 0), incident.IM_SystemCreateTimeUtc);

				TestDateAttribute.AddHours(4);
				incident.CloseIncident(nonResolutionCode, "Closing as non resolution code.");
				Factory.Save();

				var mostRecentICLEvent = incident.GetMostRecentLogByEventCode(Events.IncidentClosedCode);
				var mostRecentIRSEvent = incident.GetMostRecentLogByEventCode(Events.IncidentResolvedCode);
				AssertEquals("Precondition: ", new ZDateTime(2024, 10, 3, 12, 0, 0), mostRecentICLEvent.SL_PostedTimeUtc);
				AssertEquals("Precondition: ", new ZDateTime(2024, 10, 3, 12, 0, 0), mostRecentIRSEvent.SL_PostedTimeUtc);

				var totalERequestAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.TotalERequestAge);
				var totalResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.TotalResolutionAge);
				var netResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.NetResolutionAge);
				AssertMetrics(incident, totalERequestAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.TotalERequestAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 3, 12, 0, 0), firstCalculatedMetric: 14400, firstMetricCount: 1);
				AssertMetrics(incident, totalResolutionAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.TotalResolutionAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 3, 12, 0, 0), firstCalculatedMetric: 14400, firstMetricCount: 1);
				AssertMetrics(incident, netResolutionAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.NetResolutionAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 3, 12, 0, 0), firstCalculatedMetric: 14400, firstMetricCount: 1);

				TestDateAttribute.AddHours(2);
				incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
				Factory.Save();

				var mostRecentIROEvent = incident.GetMostRecentLogByEventCode(Events.IncidentReopenedCode);
				AssertEquals("Precondition: ", new ZDateTime(2024, 10, 3, 14, 0, 0), mostRecentIROEvent.SL_PostedTimeUtc);

				totalERequestAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.TotalERequestAge);
				AssertEquals("No new TEA metric should be created when reopening incidents.", 1, totalERequestAgeMetrics.Length);
				AssertEquals("TEA metric EndTimeUtc should be wiped out when incident is reopened.", ZDateTime.Empty, totalERequestAgeMetrics[0].IME_EndTimeUtc);

				totalResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.TotalResolutionAge);
				netResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.NetResolutionAge);
				AssertEquals("No new TRA metric should be created when reopening incidents.", 1, totalResolutionAgeMetrics.Length);
				AssertEquals("TRA metric EndTimeUtc should be wiped out when incident is reopened.", ZDateTime.Empty, totalResolutionAgeMetrics[0].IME_EndTimeUtc);
				AssertEquals("TRA metric CalculatedMetric should be wiped out when incident is reopened.", 0, totalResolutionAgeMetrics[0].IME_CalculatedMetric);
				AssertEquals("NRA metric EndTimeUtc should be wiped out when incident is reopened.", ZDateTime.Empty, netResolutionAgeMetrics[0].IME_EndTimeUtc);
				AssertEquals("NRA metric CalculatedMetric should be wiped out when incident is reopened.", 0, netResolutionAgeMetrics[0].IME_CalculatedMetric);

				var awaitingClientTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.AwaitingClientTime);
				AssertMetrics(incident, awaitingClientTimeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.AwaitingClientTime, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 12, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 3, 14, 0, 0), firstCalculatedMetric: 7200, firstMetricCount: 1);

				TestDateAttribute.AddHours(2);
				incident.CloseIncident(nonResolutionCode, "Closing as non resolution code.");
				Factory.Save();

				mostRecentICLEvent = incident.GetMostRecentLogByEventCode(Events.IncidentClosedCode);
				mostRecentIRSEvent = incident.GetMostRecentLogByEventCode(Events.IncidentResolvedCode);
				AssertEquals("Precondition: ", new ZDateTime(2024, 10, 3, 16, 0, 0), mostRecentICLEvent.SL_PostedTimeUtc);
				AssertEquals("Precondition: ", new ZDateTime(2024, 10, 3, 16, 0, 0), mostRecentIRSEvent.SL_PostedTimeUtc);

				totalERequestAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.TotalERequestAge);
				totalResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.TotalResolutionAge);
				netResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.NetResolutionAge);
				AssertMetrics(incident, totalERequestAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.TotalERequestAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 3, 16, 0, 0), firstCalculatedMetric: 28800, firstMetricCount: 2);
				AssertMetrics(incident, totalResolutionAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.TotalResolutionAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 3, 16, 0, 0), firstCalculatedMetric: 28800, firstMetricCount: 2);
				AssertMetrics(incident, netResolutionAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.NetResolutionAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 3, 16, 0, 0), firstCalculatedMetric: 21600, firstMetricCount: 2);

				TestDateAttribute.AddHours(2);
				incident.CloseIncident(resolutionCode, "Closing as resolution code.");
				Factory.Save();

				mostRecentIRSEvent = incident.GetMostRecentLogByEventCode(Events.IncidentResolvedCode);
				AssertEquals("Precondition: ", new ZDateTime(2024, 10, 3, 18, 0, 0), mostRecentIRSEvent.SL_PostedTimeUtc);

				totalResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.TotalResolutionAge);
				netResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.NetResolutionAge);
				AssertMetrics(incident, totalResolutionAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.TotalResolutionAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 3, 18, 0, 0), firstCalculatedMetric: 36000, firstMetricCount: 3);
				AssertMetrics(incident, netResolutionAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.NetResolutionAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 3, 18, 0, 0), firstCalculatedMetric: 28800, firstMetricCount: 3);

				incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
				Factory.Save();

				totalResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.TotalResolutionAge);
				netResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.NetResolutionAge);
				AssertEquals("No new TRA metric should be created when reopening incidents.", 1, totalResolutionAgeMetrics.Length);
				AssertEquals("TRA metric EndTimeUtc should be wiped out when incident is reopened.", ZDateTime.Empty, totalResolutionAgeMetrics[0].IME_EndTimeUtc);
				AssertEquals("NRA metric CalculatedMetric should be wiped out when incident is reopened.", 0, netResolutionAgeMetrics[0].IME_CalculatedMetric);

				TestDateAttribute.AddHours(2);
				incident.CloseIncident(resolutionCode, "Closing as resolution code.");
				Factory.Save();

				mostRecentIRSEvent = incident.GetMostRecentLogByEventCode(Events.IncidentResolvedCode);
				AssertEquals("Precondition: ", new ZDateTime(2024, 10, 3, 20, 0, 0), mostRecentIRSEvent.SL_PostedTimeUtc);

				totalResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.TotalResolutionAge);
				netResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.NetResolutionAge);
				AssertMetrics(incident, totalResolutionAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.TotalResolutionAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 3, 20, 0, 0), firstCalculatedMetric: 43200, firstMetricCount: 4);
				AssertMetrics(incident, netResolutionAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.NetResolutionAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 3, 20, 0, 0), firstCalculatedMetric: 36000, firstMetricCount: 4);

				TestDateAttribute.AddHours(4);
				incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
				Factory.Save();

				incident.CloseViaCloseOnBehalfOfClient = true;
				var action = new SupportIncidentCloseAction(incident);
				incident.PopulateCloseAction(action);
				action.ResolutionMethod = nonResolutionCode;
				action.SynchroniseToIncident();
				Factory.Save();
				mostRecentIRSEvent = incident.GetMostRecentLogByEventCode(Events.IncidentResolvedCode);
				AssertEquals("Incident is closed", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
				AssertEquals("No new IRS was posted", new ZDateTime(2024, 10, 3, 20, 0, 0), mostRecentIRSEvent.SL_PostedTimeUtc);
				// Metrics should be calculated based on previous IRS
				totalResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.TotalResolutionAge);
				netResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.NetResolutionAge);
				awaitingClientTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.AwaitingClientTime);
				var expectedNRA = 43200 - awaitingClientTimeMetrics.Sum(x => x.IME_CalculatedMetric); // Previous TRA - current ACT
				AssertMetrics(incident, totalResolutionAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.TotalResolutionAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 3, 20, 0, 0), firstCalculatedMetric: 43200, firstMetricCount: 4);
				AssertMetrics(incident, netResolutionAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.NetResolutionAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 3, 20, 0, 0), firstCalculatedMetric: expectedNRA, firstMetricCount: 4);
				incident.CloseViaCloseOnBehalfOfClient = false;

				incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
				Factory.Save();
				mostRecentIROEvent = incident.GetMostRecentLogByEventCode(Events.IncidentReopenedCode);
				AssertEquals("Precondition: ", new ZDateTime(2024, 10, 4, 0, 0, 0), mostRecentIROEvent.SL_PostedTimeUtc);

				awaitingClientTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.AwaitingClientTime);
				AssertMetrics(incident, awaitingClientTimeMetrics, metricsLength: 4, firstMetricCode: IncidentMetricConstants.AwaitingClientTime, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 20, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 4, 0, 0, 0), firstCalculatedMetric: 14400, firstMetricCount: 1);

				TestDateAttribute.AddHours(2);
				incident.CloseIncident("CWR", "Closing as awaiting response.");
				Factory.Save();
				incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
				Factory.Save();
				awaitingClientTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.AwaitingClientTime);
				AssertEquals("A new ACT metric should be created when closing as awaiting response.", 5, awaitingClientTimeMetrics.Length);

				TestDateAttribute.AddHours(2);
				incident.CloseIncident("XXX", "Closed as non-awaiting response.");
				Factory.Save();
				incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
				Factory.Save();
				awaitingClientTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.AwaitingClientTime);
				AssertEquals("No new ACT metric should be created when closing as non-awaiting response.", 5, awaitingClientTimeMetrics.Length);
			}
		}

		[TestDate(2024, 10, 3, 08, 00, 00)]
		public void TestIncidentMetrics_FallBackRules()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var allCode = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, allCode, allCode);
			var resolutionCode = "YYY";
			var resolutionDescription = "YYY is resolution";
			registryValue.AddSystemChildren(registryValue.Add(resolutionCode, (NoResString)resolutionDescription, supportParent, ZBool.True));
			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var incident = Factory.NewWithValidTestData<SupportIncident>();
				incident.IM_IncidentNumber = "CS33333333";
				Factory.Save();
				AssertEquals("Precondition: ", new ZDateTime(2024, 10, 3, 8, 0, 0), incident.IM_SystemCreateTimeUtc);

				TestDateAttribute.AddHours(4);
				incident.CloseIncident(resolutionCode, resolutionDescription);
				Factory.Save();

				var mostRecentIRSEvent = incident.GetMostRecentLogByEventCode(Events.IncidentResolvedCode);
				AssertEquals("Precondition: ", new ZDateTime(2024, 10, 3, 12, 0, 0), mostRecentIRSEvent.SL_PostedTimeUtc);

				var totalResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.TotalResolutionAge);
				var netResolutionAgeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS33333333", IncidentMetricConstants.NetResolutionAge);
				AssertMetrics(incident, totalResolutionAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.TotalResolutionAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 3, 12, 0, 0), firstCalculatedMetric: 14400, firstMetricCount: 1);
				AssertMetrics(incident, netResolutionAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.NetResolutionAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 3, 12, 0, 0), firstCalculatedMetric: 14400, firstMetricCount: 1);

				incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
				incident.IM_ResolutionCode = "CLS";
				netResolutionAgeMetrics[0].IME_MetricCount = 0;
				AssertMetrics(incident, netResolutionAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.NetResolutionAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: ZDateTime.Empty, firstCalculatedMetric: 0, firstMetricCount: 0);

				Factory.Save();
				AssertMetrics(incident, netResolutionAgeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.NetResolutionAge, firstStartTimeUtc: new ZDateTime(2024, 10, 3, 8, 0, 0), firstEndTimeUtc: ZDateTime.Empty, firstCalculatedMetric: 0, firstMetricCount: 1);
			}
		}

		[TestDate(2024, 10, 4, 08, 00, 00)]
		public void TestIncidentMetrics_TotalDevelopmentTime()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var allCode = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Defect, allCode, allCode);
			var resolutionCode = "YYY";
			var resolutionDescription = "YYY is resolution";
			registryValue.AddSystemChildren(registryValue.Add(resolutionCode, (NoResString)resolutionDescription, supportParent, ZBool.True));

			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var workItem = Factory.NewWithValidTestData<NewWorkItem>();
				workItem.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
				workItem.WKI_WorkItemNumber = "WI00000001";
				workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Working;
				workItem.WorkflowItems.Tasks.AddNew();

				var incident = Factory.NewWithValidTestData<SupportIncident>();
				incident.IM_Priority = "AAA";
				incident.IM_IncidentNumber = "CS44444444";
				incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
				Factory.Save();

				TestDateAttribute.AddHours(1);
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var task = incident.WorkflowItems.AddNew();
				task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				task.P9_Type = "CH0";
				incident.RelatedItems.Add(workItem);
				Factory.Save();

				var atcReference = $"{workItem.Number} attached to {incident.Number}";
				AssertEquals("Precondition: ", 1, workItem.Logs.Find(x => x.SL_Reference == atcReference).Count());
				AssertEquals("Precondition: ", 1, incident.Logs.Find(x => x.SL_Reference == atcReference).Count());

				var totalDevelopmentTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS44444444", IncidentMetricConstants.TotalDevelopmentTime);
				AssertMetrics(incident, totalDevelopmentTimeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.TotalDevelopmentTime, firstStartTimeUtc: new ZDateTime(2024, 10, 4, 9, 0, 0), firstEndTimeUtc: ZDateTime.Empty, firstCalculatedMetric: 0, firstMetricCount: 1);

				TestDateAttribute.AddHours(1);
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Closed;
				incident.IM_Status = SupportIncidentLookups.Status.Closed;
				incident.CloseIncident(resolutionCode, string.Empty);
				Factory.Save();

				var mostRecentIRSEvent = incident.GetMostRecentLogByEventCode(Events.IncidentResolvedCode);
				AssertEquals("Precondition: ", new ZDateTime(2024, 10, 4, 10, 0, 0), mostRecentIRSEvent.SL_PostedTimeUtc);

				totalDevelopmentTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS44444444", IncidentMetricConstants.TotalDevelopmentTime);
				AssertMetrics(incident, totalDevelopmentTimeMetrics, metricsLength: 1, firstMetricCode: IncidentMetricConstants.TotalDevelopmentTime, firstStartTimeUtc: new ZDateTime(2024, 10, 4, 9, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 4, 10, 0, 0), firstCalculatedMetric: 3600, firstMetricCount: 1);

				AssertNoExceptionThrown(() =>
				{
					incident.RelatedItems.Remove(workItem);
					incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
					Factory.Save();
				});
				incident.RelatedItems.Add(workItem);
				incident.CloseIncident(resolutionCode, string.Empty);
				Factory.Save();

				TestDateAttribute.AddHours(1);
				var workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
				workItem2.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
				workItem2.WKI_WorkItemNumber = "WI00000002";
				incident.RelatedItems.Add(workItem2);
				Factory.Save();

				TestDateAttribute.AddHours(1);
				var workItem3 = Factory.NewWithValidTestData<NewWorkItem>();
				workItem3.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
				workItem3.WKI_WorkItemNumber = "WI00000003";
				incident.RelatedItems.Add(workItem3);
				Factory.Save();

				incident.RelatedItems.Remove(workItem);
				Factory.Save();
				var dtcReference = $"{workItem.Number} detached from {incident.Number}";
				AssertEquals("Precondition: ", 2, workItem.Logs.Find(x => x.SL_Reference == dtcReference).Count());
				AssertEquals("Precondition: ", 2, incident.Logs.Find(x => x.SL_Reference == dtcReference).Count());

				totalDevelopmentTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS44444444", IncidentMetricConstants.TotalDevelopmentTime);
				AssertMetrics(incident, totalDevelopmentTimeMetrics, metricsLength: 2, firstMetricCode: IncidentMetricConstants.TotalDevelopmentTime, firstStartTimeUtc: new ZDateTime(2024, 10, 4, 11, 0, 0), firstEndTimeUtc: new ZDateTime(2024, 10, 4, 12, 0, 0), firstCalculatedMetric: 3600, firstMetricCount: 2);

				TestDateAttribute.AddHours(1);
				incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.Resolved, string.Empty);
				Factory.Save();

				TestDateAttribute.AddHours(1);
				incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
				var workItem4 = Factory.NewWithValidTestData<NewWorkItem>();
				workItem4.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
				workItem4.WKI_WorkItemNumber = "WI00000004";
				workItem4.WorkflowItems.Tasks.AddNew();
				incident.RelatedItems.Add(workItem4);
				Factory.Save();

				totalDevelopmentTimeMetrics = LoadIncidentMetricsByIncidentNumberAndMetricCode("CS44444444", IncidentMetricConstants.TotalDevelopmentTime);
				AssertMetrics(incident, totalDevelopmentTimeMetrics, metricsLength: 3, firstMetricCode: IncidentMetricConstants.TotalDevelopmentTime, firstStartTimeUtc: new ZDateTime(2024, 10, 4, 14, 0, 0), firstEndTimeUtc: ZDateTime.Empty, firstCalculatedMetric: 0, firstMetricCount: 0);
			}
		}

		IncidentMetrics[] LoadIncidentMetricsByIncidentNumberAndMetricCode(string incidentNumber, string metricCode)
		{
			var incidentMetricsQuery = new ZQuery(IncidentMetricsSchema.IME_IncidentNumber, incidentNumber);
			incidentMetricsQuery.AddToFilter(IncidentMetricsSchema.IME_MetricCode, metricCode);
			incidentMetricsQuery.OrderBy = IncidentMetricsSchema.IME_SystemCreateTimeUtc.Name + OrderByClause.Descending;
			var incidentMetrics = Factory.Load<IncidentMetrics>(incidentMetricsQuery);
			return incidentMetrics;
		}

		void AssertMetrics(SupportIncident incident, IncidentMetrics[] incidentMetrics, int metricsLength, string firstMetricCode, ZDateTime firstStartTimeUtc, ZDateTime firstEndTimeUtc, int firstCalculatedMetric, int firstMetricCount)
		{
			AssertEquals(metricsLength, incidentMetrics.Length);
			var incidentMetric = incidentMetrics[0];
			AssertEquals(incident.IM_IncidentNumber, incidentMetric.IME_IncidentNumber);
			AssertEquals(firstMetricCode, incidentMetric.IME_MetricCode);
			AssertEquals(firstStartTimeUtc, incidentMetric.IME_StartTimeUtc);
			AssertEquals(firstEndTimeUtc, incidentMetric.IME_EndTimeUtc);
			AssertEquals(firstCalculatedMetric, incidentMetric.IME_CalculatedMetric);
			AssertEquals(firstMetricCount, incidentMetric.IME_MetricCount);
		}

		public void TestCommunicationAttachedToIncident()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "CS00001001";

			var trigger = incident.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "HelloWorld Trigger";
			trigger.TriggerConditions.TriggerEventCode = "ATC";
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger.TriggerConditions.TriggerConditionValue = "\"<SupportIncidentRelatedCommunicationsCollection.Find(\"{OQ_CommunicationID}\"==\"<SubString(\"<SL_Reference>\",19,10)>\").OQ_CallSummary>\"==\"CMSUM\"";
			Factory.Save();

			Assert(trigger.LastFiredTimeUtc.IsEmpty);

			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			communication.OQ_CommunicationID = "CM00001024";
			communication.OQ_TypeOfCall = "EML";
			communication.OQ_Category = "AAA";
			communication.OQ_CallSummary = "CMSUM";

			var collection = communication.RelatedParentActivityPivotCollection;
			collection.AddNewPivot(incident);

			Factory.Save();
			var log = incident.Logs.MostRecentLogByEventTime(AutoEvents.Attached);
			Assert("Trigger should be fired", !trigger.LastFiredTimeUtc.IsEmpty);
			AssertEquals("|DES=Communication CM00001024 attached to Incident CS00001001|JOB=CS00001001|MOD=EML|RES=AAA|RFN=CM00001024|TYP=COM", log.SL_Reference);
		}

		public void Test_ApplyWorkflowTemplatesOnCommunicationAttachedToIncident()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "CS00001001";

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "MyTemplate";
			template.P0_ProcessType = incident.WorkflowItems.WorkflowType;
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "HelloWorld Trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.AttachedCode;
			trigger.TemplateConditions.TemplateCondition2 = EventReferenceConditionList.Codes.UserDefined;
			trigger.TemplateConditions.TemplateCondition2Value = "\"<SupportIncidentRelatedCommunicationsCollection.Find(\"{OQ_CommunicationID}\"==\"CM00001024\").OQ_CallSummary>\"==\"CMSUM\"";
			Factory.Save();

			AssertEquals("Precondition: Template condition is not true yet", 0, incident.WorkflowItems.Triggers.Count);

			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			communication.OQ_CommunicationID = "CM00001024";
			communication.OQ_TypeOfCall = "EML";
			communication.OQ_Category = "AAA";
			communication.OQ_CallSummary = "CMSUM";

			var collection = communication.RelatedParentActivityPivotCollection;
			collection.AddNewPivot(incident);

			Factory.Save();

			AssertEquals("Template should be applied when the OrgSalesCall is attached", 1, incident.WorkflowItems.Triggers.Count);
		}
		public void TestConcurrencyPolicy()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			AssertEquals(ConcurrencyPolicy.Strict, incident.IM_CategoryInfo.ConcurrencyPolicy);
			AssertEquals(ConcurrencyPolicy.Strict, incident.IM_StatusInfo.ConcurrencyPolicy);
			AssertEquals(ConcurrencyPolicy.Strict, incident.IM_ResolutionCodeInfo.ConcurrencyPolicy);
		}

		public void TestAddContactToRelatedPartyWhenIncidentCreated()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OC_Contact = contact.PK;
			incident.IM_OC_Contact = contact1.PK;
			Factory.Save();
			AssertEquals("the related parties count should be 1", 1, incident.EConversation.Conversation.RelatedParties.Count);
			Assert("the related parties should not has contact", !incident.EConversation.Conversation.RelatedParties.HasParticipant(contact));
			Assert("the related parties should has contact1", incident.EConversation.Conversation.RelatedParties.HasParticipant(contact1));
		}

		public void TestCloseRelatedWorkItem_OnlyOneMessageShouldBeAdded()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_ReleaseRing = ReleaseRings.Codes.ALP;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			workItem.WKI_WorkItemNumber = "WI00000001";
			workItem.WorkflowItems.DeleteAll();

			var task1 = workItem.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;
			task1.P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(10);
			Factory.Save();

			Assert(!workItem.IsClosedOrCancelled);
			AssertEquals(1, workItem.WorkflowItems.Count);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = "AAA";
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			incident.RelatedItems.Add(workItem);

			Factory.Save();
			AssertEquals(false, incident.NeedUpgrade);

			var expectedMessage = "Work completed, no upgrade will be sent";

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Assert(!workItem.IsClosed);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Assert(workItem.IsClosed);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Assert(!workItem.IsClosed);
			Factory.Save();
			AssertEquals("If the status of work item is active when saving, this message should not be added", 0, incident.EConversation.Conversation.Messages.Count(x => x.Body == expectedMessage));

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals("This message should not be added before saving", 0, incident.EConversation.Conversation.Messages.Count(x => x.Body == expectedMessage));

			Factory.Save();
			incident.HasChanges = true;
			Factory.Save();
			AssertEquals("This message should be created only once on saving", 1, incident.EConversation.Conversation.Messages.Count(x => x.Body == expectedMessage));
		}

		[SupportIncidentSendingConversationMethod]
		void InvalidMethodForTestSendingMessageDelayer(ZString a)
		{
			testedObjectForTestSendingMessageDelayer = a;
			Factory.Save();
		}

		string testedObjectForTestSendingMessageDelayer;

		public void TestSendingMessageDelayer()
		{
			var incident = Factory.NewWithValidTestData<SupportIncidentForTest>();
			var testedObject = string.Empty;

			void InvalidSendingMethod(ZString message)
			{
				testedObject = message;
			}

			var delaySendingMessageMethod = typeof(SupportIncident).GetMethod("DelaySendingMessage", BindingFlags.NonPublic | BindingFlags.Instance);
			var removeDelayedMessageMethod = typeof(SupportIncident).GetMethod("RemoveDelayedMessage", BindingFlags.NonPublic | BindingFlags.Instance);

			AssertNotNull(delaySendingMessageMethod);
			AssertNotNull(removeDelayedMessageMethod);

			delaySendingMessageMethod.Invoke(incident, new object[] { "Invalid1", (Action<ZString>)InvalidSendingMethod, "hahahahahahahaha" });
			delaySendingMessageMethod.Invoke(incident, new object[] { "Invalid2", (Action<ZString>)InvalidMethodForTestSendingMessageDelayer, "ABC" });
			delaySendingMessageMethod.Invoke(incident, new object[] { "Test A", (Action<ZString>)incident.AddInternalSystemLogMessage, "Test A message" });
			delaySendingMessageMethod.Invoke(incident, new object[] { "Test A", (Action<ZString>)incident.AddInternalSystemLogMessage, "Test A_1 message" });
			delaySendingMessageMethod.Invoke(incident, new object[] { "Test B", (Action<ZString>)incident.AddSystemMessageToCustomer, "Test B message" });

			removeDelayedMessageMethod.Invoke(incident, new object[] { "TestB" });

			Factory.Save();

			AssertEquals("ABC", testedObjectForTestSendingMessageDelayer);
			AssertNullOrEmpty("The method which doesn't have Attribute should not be executed", testedObject);

			var messageA = incident.EConversation.Conversation.Messages.FirstOrDefault(x => x.Body == "Test A message");
			var messageA1 = incident.EConversation.Conversation.Messages.FirstOrDefault(x => x.Body == "Test A_1 message");
			var messageB = incident.EConversation.Conversation.Messages.FirstOrDefault(x => x.Body == "Test A_1 message");

			AssertNotNull("Message should be sent", messageA);
			Assert("Message should be sent", messageA.IsInDatabase);

			AssertNull("With the same KEY message only the first should be sent", messageA1);
			AssertNull("Message that are withdrawn before it is sent should not be sent", messageB);
		}

		public void TestAttachAndDetachWorkItem_AddEventLog()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			workItem.WKI_WorkItemNumber = "WI00000001";

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Priority = "AAA";
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");

			incident.RelatedItems.Add(workItem);
			var atcReference = $"{workItem.Number} attached to {incident.Number}";
			AssertEquals(1, workItem.Logs.Find(x => x.SL_Reference == atcReference).Count());
			AssertEquals(1, incident.Logs.Find(x => x.SL_Reference == atcReference).Count());

			incident.RelatedItems.Remove(workItem);
			var dtcReference = $"{workItem.Number} detached from {incident.Number}";
			AssertEquals(1, workItem.Logs.Find(x => x.SL_Reference == dtcReference).Count());
			AssertEquals(1, incident.Logs.Find(x => x.SL_Reference == dtcReference).Count());
		}

		public void TestCreateContentFinderJwtToken()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "newuser2";
			staff.GS_Code = "NE2";

			var supportStaff = Factory.NewWithValidTestData<GlbStaff>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_GS_NKCustServiceContact = supportStaff.GS_Code;
			incident.IM_Description = "New incident description 1";
			incident.DetailNoteText = "Detail note 2";
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.ProductArea = ProductAreaList.Codes.ARC;
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily("newuser2"))
			{
				var securityToken = incident.CreateContentFinderJwtToken();
				AssertNotNull(securityToken);

				var handler = new JwtSecurityTokenHandler();
				var token = handler.ReadToken(securityToken) as JwtSecurityToken;

				var payload = token.Payload;
				AssertEquals("Payload should contains logged in user code", payload["userStaffCode"], "NE2");
				AssertEquals("Payload should contains IM_IncidentNumber", payload["incidentNumber"], incident.IM_IncidentNumber);
				AssertEquals("Payload should contains IM_Description", payload["summary"], incident.IM_Description);
				AssertEquals("Payload should contains DetailNoteText", payload["details"], incident.DetailNoteText);
				AssertEquals("Payload should contains IM_Product", payload["product"], incident.IM_Product);
				AssertEquals("Payload should contains ProductArea", payload["productArea"], incident.ProductArea);
				AssertEquals("Payload should contains Criticality", payload["criticality"], incident.Criticality);
				AssertEquals("Payload should contains IM_Module", payload["module"], incident.IM_Module);
				AssertEquals("Payload should contains IM_IncidentType", payload["incidentType"], incident.IM_IncidentType);
				AssertEquals("Payload should contains IM_Language", payload["language"], incident.IM_Language);
			}
		}

		public void TestShouldShowClosedDispositions()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();

			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;
			AssertEquals(true, incident.ShouldShowClosedDispositions);

			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction;
			AssertEquals(false, incident.ShouldShowClosedDispositions);

			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;
			AssertEquals(true, incident.ShouldShowClosedDispositions);

			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress;
			AssertEquals(false, incident.ShouldShowClosedDispositions);

			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Suspended.Deferred;
			AssertEquals(false, incident.ShouldShowClosedDispositions);

			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
			AssertEquals(true, incident.ShouldShowClosedDispositions);
		}

		public void TestGetCustomerSystemStatusCodeV2_ShouldReturnClosedWhenResolutionCodeIsClosed()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;

			var snapshot = new SupportIncidentStatusSnapShot();
			snapshot.TakeSnapShot(incident, includeIncidentDetails: false);

			AssertEquals("Status code should be Closed when disposition (eRequest Status) is Closed", SupportIncidentLookups.LegacyStatusCodes.Closed, SupportIncident.GetCustomerSystemStatusCodeV2(snapshot));
		}

		public void TestGetCustomerSystemStatusCodeV2_ShouldReturnResolvedWhenResolutionCodeIsResolved()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;

			var snapshot = new SupportIncidentStatusSnapShot();
			snapshot.TakeSnapShot(incident, includeIncidentDetails: false);

			AssertEquals("Status code should be Resolved when disposition (eRequest Status) is Resolved", SupportIncidentLookups.LegacyStatusCodes.Resolved, SupportIncident.GetCustomerSystemStatusCodeV2(snapshot));
		}

		public void TestGetCustomerSystemStatusCodeV2_ShouldReturnAwaitingClientResponseWhenResolutionCodeIsAwaitingClientResponse()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse;

			var snapshot = new SupportIncidentStatusSnapShot();
			snapshot.TakeSnapShot(incident, includeIncidentDetails: false);

			AssertEquals("Status code should match disposition (eRequest Status) when Closed Awaiting Client Response", SupportIncidentLookups.LegacyStatusCodes.ClosedAwaitingResponse, SupportIncident.GetCustomerSystemStatusCodeV2(snapshot));
		}

		public void TestIncidentCanReOpen()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();

			incident.IM_RequestStatus = DispositionList.Constants.Closed.Resolved;
			Assert(incident.CanReOpen);

			incident.IM_RequestStatus = DispositionList.Constants.Closed.ResolvedAndClosed;
			Assert(incident.CanReOpen);

			incident.IM_RequestStatus = DispositionList.Constants.Closed.ClosedAwaitingClientResponse;
			Assert(incident.CanReOpen);

			incident.IM_RequestStatus = SupportIncidentLookups.LegacyStatusCodes.SupportTeam;
			Assert(!incident.CanReOpen);
		}

		public void TestGetResolutionAndClosureBehaviour()
		{
			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;

			AssertEquals("Precondition: Should have the ALL criticalities and ALL products default fallback", 2, resolutionAndClosureBehaviourCollection.Count);
			var resolutionAndClosureBehaviourArray = resolutionAndClosureBehaviourCollection.Cast<ResolutionAndClosureBehaviour>();

			var defaultCriticalityBehaviour = resolutionAndClosureBehaviourArray.FirstOrDefault(x => x.ParentID.IsEmpty);
			var defaultProductBehaviour = resolutionAndClosureBehaviourArray.FirstOrDefault(x => x.ParentID == defaultCriticalityBehaviour.PK);
			AssertNotNull("Precondition: Should have the ALL criticalities fallback", defaultCriticalityBehaviour);
			AssertNotNull("Precondition: Should have the ALL products fallback", defaultProductBehaviour);

			defaultProductBehaviour.DaysResolvedToClosed = 5;
			var defaultCriticalityENTBehaviour = resolutionAndClosureBehaviourCollection.AddNew();
			defaultCriticalityENTBehaviour.Code = ProductTypes.Codes.Enterprise;
			defaultCriticalityENTBehaviour.ParentID = defaultCriticalityBehaviour.PK;
			defaultCriticalityENTBehaviour.DaysResolvedToClosed = 7;

			var cr4Behaviour = resolutionAndClosureBehaviourCollection.AddNew();
			cr4Behaviour.Code = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			resolutionAndClosureBehaviourCollection.AddSystemChildren(cr4Behaviour);
			AssertEquals("Precondition: Should have the ALL products fallback for cr4", 5, resolutionAndClosureBehaviourCollection.Count);

			var cr4ALLProductBehaviour = resolutionAndClosureBehaviourCollection.Cast<ResolutionAndClosureBehaviour>().FirstOrDefault(x => x.ParentID == cr4Behaviour.PK);
			cr4ALLProductBehaviour.DaysResolvedToClosed = 9;

			var cr4ENTBehaviour = resolutionAndClosureBehaviourCollection.AddNew();
			cr4ENTBehaviour.Code = ProductTypes.Codes.Enterprise;
			cr4ENTBehaviour.ParentID = cr4Behaviour.PK;
			cr4ENTBehaviour.DaysResolvedToClosed = 11;

			var cr4ZZZBehaviour = resolutionAndClosureBehaviourCollection.AddNew();
			cr4ZZZBehaviour.Code = "ZZZ";
			cr4ZZZBehaviour.ParentID = cr4Behaviour.PK;
			cr4ZZZBehaviour.DaysResolvedToClosed = 13;

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Product = ProductTypes.Codes.Enterprise;

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				AssertEquals("Full match on criticality and product", cr4ENTBehaviour.DaysResolvedToClosed, incident.GetResolutionAndClosureBehaviour().DaysResolvedToClosed);

				incident.IM_Product = ProductTypes.Codes.GLOW;
				AssertEquals("Match on criticality, fallback for product", cr4ALLProductBehaviour.DaysResolvedToClosed, incident.GetResolutionAndClosureBehaviour().DaysResolvedToClosed);

				incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
				AssertEquals("Fallback for criticality and product", defaultProductBehaviour.DaysResolvedToClosed, incident.GetResolutionAndClosureBehaviour().DaysResolvedToClosed);

				incident.IM_Product = ProductTypes.Codes.Enterprise;
				AssertEquals("If criticality doesn't match, we must use the fallback", defaultCriticalityENTBehaviour.DaysResolvedToClosed, incident.GetResolutionAndClosureBehaviour().DaysResolvedToClosed);

				incident.IM_Product = "ZZZ";
				AssertEquals("If criticality doesn't match, we must use the fallback", defaultProductBehaviour.DaysResolvedToClosed, incident.GetResolutionAndClosureBehaviour().DaysResolvedToClosed);
			}
		}

		public void TestGetResolutionAndClosureBehaviour_InvalidData()
		{
			var resolutionAndClosureBehaviourCollection = EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.Value;

			AssertEquals("Precondition: Should have the ALL criticalities and ALL products default fallback", 2, resolutionAndClosureBehaviourCollection.Count);
			var resolutionAndClosureBehaviourArray = resolutionAndClosureBehaviourCollection.Cast<ResolutionAndClosureBehaviour>();

			var defaultCriticalityBehaviour = resolutionAndClosureBehaviourArray.FirstOrDefault(x => x.ParentID.IsEmpty);
			var defaultProductBehaviour = resolutionAndClosureBehaviourArray.FirstOrDefault(x => x.ParentID == defaultCriticalityBehaviour.PK);
			AssertNotNull("Precondition: Should have the ALL criticalities fallback", defaultCriticalityBehaviour);
			AssertNotNull("Precondition: Should have the ALL products fallback", defaultProductBehaviour);

			resolutionAndClosureBehaviourCollection.Remove(defaultProductBehaviour);
			AssertEquals("Precondition", 1, resolutionAndClosureBehaviourCollection.Count);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "CS00088801";
			incident.IM_Description = "Incident 1";
			incident.IM_Priority = Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Product = ProductTypes.Codes.Enterprise;

			var defaultBehaviour = new ResolutionAndClosureBehaviour();

			using (EDIDataRegistry.Instance.ResolutionAndClosureBehaviour.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, resolutionAndClosureBehaviourCollection))
			{
				AssertEquals("Should return default behaviour", defaultBehaviour.DaysResolvedToClosed, incident.GetResolutionAndClosureBehaviour().DaysResolvedToClosed);
				AssertEquals("Should report error", "Missing Resolution And Closure Behaviour Registry entry", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			}
		}

		public void TestContext()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("AAA", "AAA Enabled Module", ProductAreaList.Codes.ARC, false);
			EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = "AAA";
			incident.IM_Priority = "CR8";
			incident.ProductArea = "";
			incident.RecalculateProductArea();
			AssertEquals("ARC", incident.ProductArea);

			incident.ProductArea = "";
			using (incident.SetTempContext(SupportIncident.Context.OnOverrideProductClassification))
			{
				incident.RecalculateProductArea();
				AssertEquals("", incident.ProductArea);
			}

			var idx = 1;
			var log = new StringBuilder();
			Factory.Saving += (f) =>
			{
				var contexts = string.Join("-", Factory.GetContexts<SupportIncident.Context>().Select(x => x.ToString()));
				log.AppendLine($"{idx++} -> {contexts}");
			};
			Factory.Save();

			AssertEquals("1 -> \r\n2 -> OnSecondFactorySave\r\n", log.ToString());
		}

		public void TestRequestStatusSetToResolvedWhenRequestIsResolved()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Resolved;
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed;

			var result1 = new SupportIncidentStatusSnapShot();
			result1.TakeSnapShot(incident1, includeIncidentDetails: false);
			incident1.IM_RequestStatus = SupportIncident.GetCustomerSystemStatusCodeV2(result1);

			var result2 = new SupportIncidentStatusSnapShot();
			result2.TakeSnapShot(incident2, includeIncidentDetails: false);
			incident2.IM_RequestStatus = SupportIncident.GetCustomerSystemStatusCodeV2(result2);

			Factory.Save();

			AssertEquals("SLV", incident1.IM_RequestStatus);
			AssertEquals("SLV", incident1.Request.INC_Status);
			AssertEquals("Resolved", incident1.IM_ResolutionCodeDescription);

			AssertEquals("CLS", incident2.IM_RequestStatus);
			AssertEquals("CLS", incident2.Request.INC_Status);
			AssertEquals("Closed", incident2.IM_ResolutionCodeDescription);
		}

		public void TestRecalculateProductArea_ShouldNotOverrideTriageProductArea()
		{
			var productAreasList = new CodeDescriptionPairList();
			productAreasList.AddPair("AAA", "Apple");
			productAreasList.AddPair("BBB", "Banana");
			productAreasList.AddPair("CCC", "Carrot");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreasList);

			var collection = new SystemProductCollection();
			var products = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", isProductReadOnly: true);
			var redModuleMapping = products.ModuleMappings.AddNew("RED", "Red", "AAA", isModuleReadOnly: false);
			redModuleMapping.SourceModuleMappings.AddNew("SourceModule1", "BBB");
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_Type = IncidentTriageTypes.Codes.Support;
			triage.IMT_Module = "RED";
			triage.IMT_Product = "ENT";
			triage.IMT_ProductArea = "CCC";

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Priority = "CR4";
			incident.IM_Module = "RED";
			incident.IM_SourceModuleId = "SourceModule1";
			Factory.Save();

			incident.IM_IMT_Triage = triage.PK;
			incident.IM_ProgramArea = triage.IMT_ProductArea;

			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);

			incident.RecalculateProductArea();
			AssertEquals("Incident's product area should not be recalculated since it matches the triage", triage.IMT_ProductArea, incident.IM_ProgramArea);
		}

		public void TestSetProductAreaByTriageAndMenuItem_ShouldKeepLatestSet()
		{
			var productAreasList = new CodeDescriptionPairList();
			productAreasList.AddPair("AAA", "Apple");
			productAreasList.AddPair("BBB", "Banana");
			productAreasList.AddPair("CCC", "Carrot");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreasList);

			var collection = new SystemProductCollection();
			var products = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", isProductReadOnly: true);
			var redModuleMapping = products.ModuleMappings.AddNew("RED", "Red", "AAA", isModuleReadOnly: false);
			redModuleMapping.SourceModuleMappings.AddNew("SourceModule1", "BBB");
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_Type = IncidentTriageTypes.Codes.Support;
			triage.IMT_Module = "RED";
			triage.IMT_Product = "ENT";
			triage.IMT_ProductArea = "CCC";

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Priority = "CR4";
			incident.IM_Module = "RED";
			Factory.Save();

			incident.IM_IMT_Triage = triage.PK;
			incident.IM_ProgramArea = triage.IMT_ProductArea;

			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);

			incident.IM_SourceModuleId = "SourceModule1";
			AssertEquals("Incident's product area should be recalculated to match the menu item (source module id's) product area mapping", "BBB", incident.IM_ProgramArea);

			incident.RecalculateProductArea();
			AssertEquals("Incident's product area should still match menu item", "BBB", incident.IM_ProgramArea);

			incident.IM_ProgramArea = triage.IMT_ProductArea;
			incident.RecalculateProductArea();
			AssertEquals("Incident's product area should not be recalculated since it matches the triage", triage.IMT_ProductArea, incident.IM_ProgramArea);
		}

		public void TestClearingMenuItemShouldRecalculateProductAreaBasedOnTriage()
		{
			var productAreasList = new CodeDescriptionPairList();
			productAreasList.AddPair("AAA", "Apple");
			productAreasList.AddPair("BBB", "Banana");
			productAreasList.AddPair("CCC", "Carrot");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, productAreasList);

			var collection = new SystemProductCollection();
			var products = collection.AddNew(ProductTypes.Codes.Enterprise, "ZZ", isProductReadOnly: true);
			var redModuleMapping = products.ModuleMappings.AddNew("RED", "Red", "AAA", isModuleReadOnly: false);
			redModuleMapping.SourceModuleMappings.AddNew("SourceModule1", "BBB");
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_Type = IncidentTriageTypes.Codes.Support;
			triage.IMT_Module = "RED";
			triage.IMT_Product = "ENT";
			triage.IMT_ProductArea = "CCC";

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_Priority = "CR4";
			incident.IM_Module = "RED";
			Factory.Save();

			incident.IM_IMT_Triage = triage.PK;
			incident.IM_ProgramArea = triage.IMT_ProductArea;

			AssertEquals("Precondition", triage.IMT_ProductArea, incident.IM_ProgramArea);

			incident.IM_SourceModuleId = "SourceModule1";
			AssertEquals("Incident's product area should be recalculated to match the menu item (source module id's) product area mapping", "BBB", incident.IM_ProgramArea);

			incident.RecalculateProductArea();
			AssertEquals("Incident's product area should still match menu item", "BBB", incident.IM_ProgramArea);

			incident.IM_SourceModuleId = string.Empty;
			AssertEquals("Incident's product area should be recalculated to match the triage", triage.IMT_ProductArea, incident.IM_ProgramArea);
		}

		public void TestJobSpecificToken()
		{
			EDIDataRegistry.Instance.EnableIncidentAssignedStaffChangedEmailNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "NewUser@NewDomain.com";
			staff.GS_FullName = "Kermit the Frog";

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "NewUser@NewDomain.com";
			staff2.GS_FullName = "Beaker";

			incident.IM_Description = "I'm a test string";
			AssignWorkTaskToStaffForEmailTest(incident, staff);

			incident2.IM_Description = "Hi a test string, I'm Kyan";
			AssignWorkTaskToStaffForEmailTest(incident2, staff2);

			Factory.Save();

			AssertEquals("Should create one email per incident", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			var incident1Mail = Env.OutgoingMailManager.EmailsCreated[0];
			var incident2Mail = Env.OutgoingMailManager.EmailsCreated[1];

			AssertContains(incident.JobSpecificToken, incident1Mail.Body);
			AssertNotContains(incident2.JobSpecificToken, incident1Mail.Body);
			AssertContains(incident2.JobSpecificToken, incident2Mail.Body);
			AssertNotContains(incident.JobSpecificToken, incident2Mail.Body);
		}

		public void TestDoNotAutoRecordLogForSupportIncident()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Description = "I'm a test incident";
			Factory.Save();

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var incident1 = factory1.Load<SupportIncident>(incident.PK);

			AssertEquals(0, incident1.Logs.Find(log => log.SL_SE_NKEvent == "ADD").Count());
			AssertEquals(0, incident1.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());

			incident.IM_Description = "Do some changes";
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var incident2 = factory1.Load<SupportIncident>(incident.PK);

			AssertEquals(0, incident2.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
		}

		public void TestIncidentShouldStayIndependent()
		{
			var incident1 = (SupportIncident)GetNewBusinessObject();
			incident1.IM_Category = SupportIncidentCategoriesList.Codes.ContentDevelopment;

			var incident2 = (SupportIncident)GetNewBusinessObject();
			var project = Factory.NewWithValidTestData<EDIProject>();
			incident2.RelatedItems.Add(project);
			incident2.IM_Category = SupportIncidentCategoriesList.Codes.Support;

			var incident3 = (SupportIncident)GetNewBusinessObject();
			var workItem3 = Factory.NewWithValidTestData<NewWorkItem>();
			incident3.RelatedWorkItems.Add(workItem3);
			incident3.IM_Category = SupportIncidentCategoriesList.Codes.Support;

			var incident4 = (SupportIncident)GetNewBusinessObject();
			var project4 = Factory.NewWithValidTestData<EDIProject>();
			incident4.RelatedItems.Add(project4);
			incident4.IM_Category = SupportIncidentCategoriesList.Codes.ContentDevelopment;

			var incident5 = (SupportIncident)GetNewBusinessObject();
			incident5.IM_Category = SupportIncidentCategoriesList.Codes.ContentDevelopment;

			var incident6 = (SupportIncident)GetNewBusinessObject();
			incident6.IM_Category = SupportIncidentCategoriesList.Codes.Support;

			Factory.Save();

			Assert(incident1.ShouldStayIndependent);
			Assert(!incident2.ShouldStayIndependent);
			Assert(!incident3.ShouldStayIndependent);
			Assert(incident4.ShouldStayIndependent);
			Assert(incident5.ShouldStayIndependent);
			Assert(!incident6.ShouldStayIndependent);
		}

		public void TestAttachWorkItemHaveNoEffectOnResolutionCodeWhenIncidentShouldStayIndependent()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			incident1.SetupForNewCreatedFeatureRequest();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var task1 = incident1.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			var incident1Reload = factory2.Load<SupportIncident>(incident1.PK);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress, incident1Reload.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.Status.Working, incident1Reload.IM_Status);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			incident1.RelatedItems.Add(workItem);

			Assert(!incident1.ShouldStayIndependent);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident1.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.Status.Working, incident1.IM_Status);

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_Category = SupportIncidentCategoriesList.Codes.ContentDevelopment;
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var task2 = incident2.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress, incident2.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.Status.Working, incident2.IM_Status);

			var workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			incident2.RelatedItems.Add(workItem2);
			Factory.Save();

			var incident2Reload = factory2.Load<SupportIncident>(incident2.PK);
			Assert(incident2Reload.ShouldStayIndependent);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkInProgress, incident2Reload.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.Status.Working, incident2Reload.IM_Status);
		}

		public void TestAttachWorkItemHaveNoEffectOnStatusWhenIncidentShouldStayIndependent()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.SetupForNewCreatedFeatureRequest();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var task1 = incident1.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			incident1.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, string.Empty);
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var incident1Reload = factory2.Load<SupportIncident>(incident1.PK);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, incident1Reload.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident1Reload.IM_Status);

			var workItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem1.WKI_Status = "WRK";
			incident1.RelatedItems.Add(workItem1);
			Factory.Save();

			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };
			var incident1ReloadAgain = factory3.Load<SupportIncident>(incident1.PK);
			Assert(!incident1ReloadAgain.ShouldStayIndependent);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident1ReloadAgain.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.Status.Working, incident1ReloadAgain.IM_Status);

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_Category = SupportIncidentCategoriesList.Codes.ContentDevelopment;
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var task2 = incident2.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			incident2.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, string.Empty);
			Factory.Save();

			var incident2Reload = factory2.Load<SupportIncident>(incident2.PK);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, incident2Reload.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident2Reload.IM_Status);

			var workItem2 = Factory.NewWithValidTestData<NewWorkItem>();
			workItem2.WKI_Status = "WRK";
			incident2.RelatedItems.Add(workItem2);
			Factory.Save();

			var incident2ReloadAgain = factory3.Load<SupportIncident>(incident2.PK);
			Assert(incident2ReloadAgain.ShouldStayIndependent);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, incident2ReloadAgain.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.Status.Closed, incident2ReloadAgain.IM_Status);
		}

		public void TestCreateClientCommunicationTaskIfNeeded_NoOpenTasks()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = helper.GetJobHeaderForParent(incident1, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow 1");
			var workflow2 = helper.CreateWorkflow(jobHeader, "Workflow 2");
			var workflow3 = helper.CreateWorkflow(jobHeader, "Workflow 3");

			var task1 = incident1.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task1.P9_Sequence = 10;
			task1.P9_FH_ProcessHeader = workflow1.PK;
			var task2 = incident1.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task2.P9_Sequence = 20;
			task2.P9_FH_ProcessHeader = workflow2.PK;
			Factory.Save();

			incident1.CreateClientCommunicationTaskIfNeeded();

			AssertEquals("Should have added a Client Communication Task", 3, incident1.WorkflowItems.Tasks.Count);
			var newTask = incident1.WorkflowItems.Tasks[2];
			AssertEquals("Should have added 1 to the highest closed sequence number", task1.P9_Sequence + 1, newTask.P9_Sequence);
			AssertEquals("Workflow should match the task of highest closed sequence task", task1.P9_FH_ProcessHeader, newTask.P9_FH_ProcessHeader);
			newTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			var task4 = incident1.WorkflowItems.AddNew();
			task4.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task4.P9_Sequence = 30;
			task4.P9_FH_ProcessHeader = workflow1.PK;

			incident1.CreateClientCommunicationTaskIfNeeded();

			AssertEquals("Should have added a Client Communication Task", 5, incident1.WorkflowItems.Tasks.Count);
			var task5 = incident1.WorkflowItems.Tasks[4];
			AssertEquals("Should have added 1 to the highest closed sequence number", task4.P9_Sequence + 1, task5.P9_Sequence);
			AssertEquals("Workflow should match the task of highest closed sequence task", task4.P9_FH_ProcessHeader, task5.P9_FH_ProcessHeader);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task5.P9_FH_ProcessHeader = workflow3.PK;
			incident1.CreateClientCommunicationTaskIfNeeded();

			AssertEquals("Should have added a Client Communication Task", 6, incident1.WorkflowItems.Tasks.Count);
			newTask = incident1.WorkflowItems.Tasks[5];
			AssertEquals("Should have added 1 to the highest cancelled sequence number since there are no closed tasks", task5.P9_Sequence + 1, newTask.P9_Sequence);
			AssertEquals("Workflow should match the highest cancelled sequence task since there are no closed tasks", task5.P9_FH_ProcessHeader, newTask.P9_FH_ProcessHeader);
		}

		public void TestCreateClientCommunicationTaskIfNeeded_HasOpenTasks()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = helper.GetJobHeaderForParent(incident1, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow 1");
			var workflow2 = helper.CreateWorkflow(jobHeader, "Workflow 2");
			var workflow3 = helper.CreateWorkflow(jobHeader, "Workflow 3");

			var task1 = incident1.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task1.P9_Sequence = 10;
			task1.P9_FH_ProcessHeader = workflow1.PK;
			var task2 = incident1.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_Sequence = 20;
			task2.P9_FH_ProcessHeader = workflow2.PK;
			Factory.Save();

			AssertEquals("Precondition", task2.PK, incident1.CurrentTask.PK);
			incident1.CreateClientCommunicationTaskIfNeeded();

			AssertEquals("Should have added a Client Communication Task", 3, incident1.WorkflowItems.Tasks.Count);
			var task3 = incident1.WorkflowItems.Tasks[2];
			AssertEquals("Should have added 1 to the highest closed sequence number", task1.P9_Sequence + 1, task3.P9_Sequence);
			AssertEquals("Workflow should match the current task", task2.P9_FH_ProcessHeader, task3.P9_FH_ProcessHeader);
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			var task4 = incident1.WorkflowItems.AddNew();
			task4.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task4.P9_Sequence = 30;
			task4.P9_FH_ProcessHeader = workflow1.PK;

			AssertEquals("Precondition", task2.PK, incident1.CurrentTask.PK);
			incident1.CreateClientCommunicationTaskIfNeeded();

			AssertEquals("Should have added a Client Communication Task", 5, incident1.WorkflowItems.Tasks.Count);
			var task5 = incident1.WorkflowItems.Tasks[4];
			AssertEquals("Should have added 1 to the highest closed sequence number", task1.P9_Sequence + 1, task5.P9_Sequence);
			AssertEquals("Workflow should match the current task", task2.P9_FH_ProcessHeader, task5.P9_FH_ProcessHeader);

			task5.P9_FH_ProcessHeader = workflow3.PK;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task5.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			AssertEquals("Precondition", task5.PK, incident1.CurrentTask.PK);
			incident1.CreateClientCommunicationTaskIfNeeded();

			AssertEquals("Should have added a Client Communication Task", 6, incident1.WorkflowItems.Tasks.Count);
			var task6 = incident1.WorkflowItems.Tasks[5];
			AssertEquals("Should have added 1 to the highest cancelled sequence number since there are no close tasks", task4.P9_Sequence + 1, task6.P9_Sequence);
			AssertEquals("Workflow should match the current task", task5.P9_FH_ProcessHeader, task6.P9_FH_ProcessHeader);
		}

		public void TestCreateClientCommunicationTaskIfNeeded_HasNoTasks()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = helper.GetJobHeaderForParent(incident1, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow 1");
			Factory.Save();

			incident1.CreateClientCommunicationTaskIfNeeded();

			AssertEquals("Should have added a Client Communication Task", 1, incident1.WorkflowItems.Tasks.Count);
			var newTask = incident1.WorkflowItems.Tasks[0];
			AssertEquals("Should default sequence to 1", 1, newTask.P9_Sequence);
			AssertEquals("Should set workflow to first one it can find", workflow1.PK, newTask.P9_FH_ProcessHeader);
		}

		public void TestCreateClientCommunicationTaskIfNeeded_HasNoTasksOrWorkflows()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			incident1.CreateClientCommunicationTaskIfNeeded();

			AssertEquals("Should have added a Client Communication Task", 1, incident1.WorkflowItems.Tasks.Count);
			var newTask = incident1.WorkflowItems.Tasks[0];
			AssertEquals("Should default sequence to 1", 1, newTask.P9_Sequence);
		}

		public void TestCreateClientCommunicationTaskIfNeeded_HasNoClosedOrCancelledTasks()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = helper.GetJobHeaderForParent(incident1, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = helper.CreateWorkflow(jobHeader, "Workflow 1");

			var task1 = incident1.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 10;
			task1.P9_FH_ProcessHeader = workflow1.PK;
			Factory.Save();

			incident1.CreateClientCommunicationTaskIfNeeded();

			AssertEquals("Should have added a Client Communication Task", 2, incident1.WorkflowItems.Tasks.Count);
			var newTask = incident1.WorkflowItems.Tasks[0];
			AssertEquals("Should set sequence to first task", task1.P9_Sequence, newTask.P9_Sequence);
			AssertEquals("Should set workflow to same as first task", workflow1.PK, newTask.P9_FH_ProcessHeader);
		}

		public void TestProductAreaChangedViaTriageShouldHandleWorflowPropertyChange()
		{
			var triage = Factory.NewWithValidTestData<IncidentTriage>();
			triage.IMT_Type = IncidentTriageTypes.Codes.Support;
			triage.IMT_Module = "INT";
			triage.IMT_Product = "ENT";
			triage.IMT_ProductArea = "XRM";

			var incident = Factory.New<SupportIncident>();
			incident.IM_Product = "ENT";
			incident.IM_ProgramArea = "XRM";
			incident.IM_Module = "INT";
			incident.IM_SourceModuleId = "ABC";
			incident.IM_Priority = "CR4";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_RN_NKCountry = "AU";
			incident.IM_Language = "EN";
			Factory.Save();

			incident.IM_IMT_Triage = triage.PK;
			Factory.Save();

			var areas = new CodeDescriptionPairList();
			areas.AddPair("AXT", "AXTs");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			product.ModuleMappings.AddNew("INT", "Super Module A", "AXT", false);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "INC";
			template1.P0_SubType1 = ProductTypes.Codes.Enterprise;
			template1.P0_SubType2 = "SUP";
			template1.P0_SubType4 = "AXT";

			var header1 = template1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Level 1 Support";

			var task11 = template1.WorkflowItems.AddNew();
			task11.P9_FH_ProcessHeader = header1.PK;
			task11.P9_Sequence = 10;
			task11.P9_Description = "archie test task";
			task11.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();

			((ITriageAssistParent)incident).ProductArea = "AXT";
			AssertNotNull(incident.WorkflowItems.Cast<ProcessTask>().Any(t => t.P9_Description == "archie test task"));
		}

		#region Test HTML Properties

		public void TestHtmlProperty()
		{
			var supportIncident = (SupportIncident)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, supportIncident.FeatureRequestPrerequisitesAsBlob);
			AssertEquals(ZBlob.Empty, supportIncident.FeatureRequestPrerequisitesAsBlob_HTML);
			AssertEquals(ZBlob.Empty, supportIncident.FeatureRequestInternalNoteAsBlob);
			AssertEquals(ZBlob.Empty, supportIncident.FeatureRequestInternalNoteAsBlob_HTML);
			AssertEquals(ZBlob.Empty, supportIncident.BusinessRequirementsAsBlob);
			AssertEquals(ZBlob.Empty, supportIncident.BusinessRequirementsAsBlob_HTML);
			AssertEquals(ZBlob.Empty, supportIncident.TechnicalSpecificationAsBlob);
			AssertEquals(ZBlob.Empty, supportIncident.TechnicalSpecificationAsBlob_HTML);
			AssertEquals(ZBlob.Empty, supportIncident.SoftwareChangeAsBlob);
			AssertEquals(ZBlob.Empty, supportIncident.SoftwareChangeAsBlob_HTML);
			AssertEquals(ZBlob.Empty, supportIncident.IM_Details);
			AssertEquals(ZBlob.Empty, supportIncident.IM_Details_HTML);

			supportIncident.FeatureRequestPrerequisitesAsBlob_HTML = ZBlob.FromUTF8("<p>123</p>");
			supportIncident.FeatureRequestInternalNoteAsBlob_HTML = ZBlob.FromUTF8("<p>123</p>");
			supportIncident.BusinessRequirementsAsBlob_HTML = ZBlob.FromUTF8("<p>123</p>");
			supportIncident.TechnicalSpecificationAsBlob_HTML = ZBlob.FromUTF8("<p>123</p>");
			supportIncident.SoftwareChangeAsBlob_HTML = ZBlob.FromUTF8("<p>123</p>");
			supportIncident.IM_Details_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(supportIncident.FeatureRequestPrerequisitesAsBlob.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", supportIncident.FeatureRequestPrerequisitesAsBlob_HTML.ToUTF8());
			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(supportIncident.FeatureRequestInternalNoteAsBlob.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", supportIncident.FeatureRequestInternalNoteAsBlob_HTML.ToUTF8());
			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(supportIncident.BusinessRequirementsAsBlob.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", supportIncident.BusinessRequirementsAsBlob_HTML.ToUTF8());
			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(supportIncident.TechnicalSpecificationAsBlob.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", supportIncident.TechnicalSpecificationAsBlob_HTML.ToUTF8());
			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(supportIncident.SoftwareChangeAsBlob.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", supportIncident.SoftwareChangeAsBlob_HTML.ToUTF8());
			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(supportIncident.IM_Details.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", supportIncident.IM_Details_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			var supportIncident = (SupportIncident)GetNewBusinessObject();
			AssertEquals(ZBlob.Empty, supportIncident.FeatureRequestPrerequisitesAsBlob);
			AssertEquals(ZBlob.Empty, supportIncident.FeatureRequestPrerequisitesAsBlob_HTML);
			AssertEquals(ZBlob.Empty, supportIncident.FeatureRequestInternalNoteAsBlob);
			AssertEquals(ZBlob.Empty, supportIncident.FeatureRequestInternalNoteAsBlob_HTML);
			AssertEquals(ZBlob.Empty, supportIncident.BusinessRequirementsAsBlob);
			AssertEquals(ZBlob.Empty, supportIncident.BusinessRequirementsAsBlob_HTML);
			AssertEquals(ZBlob.Empty, supportIncident.TechnicalSpecificationAsBlob);
			AssertEquals(ZBlob.Empty, supportIncident.TechnicalSpecificationAsBlob_HTML);
			AssertEquals(ZBlob.Empty, supportIncident.SoftwareChangeAsBlob);
			AssertEquals(ZBlob.Empty, supportIncident.SoftwareChangeAsBlob_HTML);
			AssertEquals(ZBlob.Empty, supportIncident.IM_Details);
			AssertEquals(ZBlob.Empty, supportIncident.IM_Details_HTML);

			supportIncident.FeatureRequestPrerequisitesAsBlob = ZBlob.FromUTF8("1234\r\n5678");
			supportIncident.FeatureRequestInternalNoteAsBlob = ZBlob.FromUTF8("1234\r\n5678");
			supportIncident.BusinessRequirementsAsBlob = ZBlob.FromUTF8("1234\r\n5678");
			supportIncident.TechnicalSpecificationAsBlob = ZBlob.FromUTF8("1234\r\n5678");
			supportIncident.SoftwareChangeAsBlob = ZBlob.FromUTF8("1234\r\n5678");
			supportIncident.IM_Details = ZBlob.FromUTF8("1234\r\n5678");

			AssertEquals("<p>1234</p><p>5678</p>", supportIncident.FeatureRequestPrerequisitesAsBlob_HTML.ToUTF8());
			AssertEquals("<p>1234</p><p>5678</p>", supportIncident.FeatureRequestInternalNoteAsBlob_HTML.ToUTF8());
			AssertEquals("<p>1234</p><p>5678</p>", supportIncident.BusinessRequirementsAsBlob_HTML.ToUTF8());
			AssertEquals("<p>1234</p><p>5678</p>", supportIncident.TechnicalSpecificationAsBlob_HTML.ToUTF8());
			AssertEquals("<p>1234</p><p>5678</p>", supportIncident.SoftwareChangeAsBlob_HTML.ToUTF8());
			AssertEquals("<p>1234</p><p>5678</p>", supportIncident.IM_Details_HTML.ToUTF8());

			supportIncident.FeatureRequestPrerequisitesAsBlob = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");
			AssertEquals("<p>rtf</p>", supportIncident.FeatureRequestPrerequisitesAsBlob_HTML.ToUTF8());
			supportIncident.FeatureRequestInternalNoteAsBlob = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");
			AssertEquals("<p>rtf</p>", supportIncident.FeatureRequestInternalNoteAsBlob_HTML.ToUTF8());
			supportIncident.BusinessRequirementsAsBlob = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");
			AssertEquals("<p>rtf</p>", supportIncident.BusinessRequirementsAsBlob_HTML.ToUTF8());
			supportIncident.TechnicalSpecificationAsBlob = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");
			AssertEquals("<p>rtf</p>", supportIncident.TechnicalSpecificationAsBlob_HTML.ToUTF8());
			supportIncident.SoftwareChangeAsBlob = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");
			AssertEquals("<p>rtf</p>", supportIncident.SoftwareChangeAsBlob_HTML.ToUTF8());
			supportIncident.IM_Details = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");
			AssertEquals("<p>rtf</p>", supportIncident.IM_Details_HTML.ToUTF8());
		}

		#endregion

		public void TestReopenIncidentShouldMakeIsCustomerResolvedToFalse()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Description = "I'm a test incident";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			Factory.Save();

			incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.SelfResolved, string.Empty);
			incident.Request.INC_IsCustomerResolved = true;
			Factory.Save();
			Assert(incident.Request.INC_IsCustomerResolved);
			incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var requestReloadAfterReopen = factory2.Load<EdiIncidentRequest>(incident.Request.PK);
			Assert(!requestReloadAfterReopen.INC_IsCustomerResolved);
		}

		public void TestCloseIncident_ResolvedShouldNotChangeIsCustomerResolved()
		{
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			var resolvedDecription = "ZZZ Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)resolvedDecription, supportParent, ZBool.True));
			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var incident1 = Factory.New<SupportIncident>();
				incident1.IM_Category = "SUP";

				var org = Factory.NewWithValidTestData<OrgHeader>();
				var contact = org.Contacts.AddNew();
				contact.OC_Email = "contact@org.com";

				incident1.IM_OH_Client = org.PK;
				incident1.IM_OC_Contact = contact.PK;
				incident1.Request.INC_IsCustomerResolved = false;

				Factory.Save();

				var originalResolutionCode = incident1.IM_ResolutionCode;

				Assert("Precondition: incident1 should be unresolved", incident1.IM_ClosureResolution.IsEmpty);

				incident1.CloseIncident(resolvedCode, "Comment");

				AssertEquals("Disposition should now be set to resolved", DispositionList.Constants.Closed.Resolved, incident1.IM_ResolutionCode);
				AssertEquals("Closure resolution should be set to the code used for closing", resolvedCode, incident1.IM_ClosureResolution);
				AssertEquals("incidentRequest IsCustomerResolved should not be changed", false, incident1.Request.INC_IsCustomerResolved);
			}
		}

		class SupportIncidentForTimestampsTest : SupportIncident
		{
			public SupportIncidentForTimestampsTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public Action PreHookValidateTimestampsForTestAction;

			protected sealed override void PreHookValidateTimestampsForTest()
			{
				if (PreHookValidateTimestampsForTestAction != null)
				{
					PreHookValidateTimestampsForTestAction();
				}
			}
		}

		public void TestIncidentReportsErrorWhenSavingInvalidTimestamps()
		{
			ErrorReporter.Clear();
			var registryValue = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
			var any = CodeDescriptionBoolTreeNode.AllCode;
			var supportParent = registryValue.Find(SupportIncidentCategoriesList.Codes.Support, any, any);
			var resolvedCode = "ZZZ";
			var resolvedDecription = "ZZZ Description";
			registryValue.AddSystemChildren(registryValue.Add(resolvedCode, (NoResString)resolvedDecription, supportParent, ZBool.True));
			using (EDIDataRegistry.Instance.IncidentClosureDispositions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var incident = Factory.NewWithValidTestData<SupportIncident>();
				Factory.Save();
				incident.CloseIncident(DispositionList.Constants.Closed.SelfResolved, "");
				incident.IM_CloseTimeUtc = incident.IM_SystemCreateTimeUtc.AddDays(-1);
				Factory.Save();
				AssertEquals("Incident status is closed", SupportIncidentLookups.Status.Closed, incident.IM_Status);
				AssertEquals("Error is reported", "CloseTimeUtc shouldn't be less than Incident create time", ErrorReporter.LastMessageReported);

				var incident2 = Factory.NewWithValidTestData<SupportIncident>();
				Factory.Save();
				incident2.CloseIncident(resolvedCode, "");
				incident2.IM_ResolveTimeUtc = incident2.IM_SystemCreateTimeUtc.AddDays(-1);
				Factory.Save();
				AssertEquals("Disposition should be resolved", DispositionList.Constants.Closed.Resolved, incident2.IM_ResolutionCode);
				AssertEquals("Error is reported", "ResolveTimeUtc shouldn't be less than Incident create time", ErrorReporter.LastMessageReported);

				var tableName = $"{IncidentMainSchema.Constants.SqlSchemaName}.{IncidentMainSchema.Constants.TableName}";
				var incident3 = Factory.NewWithValidTestData<SupportIncidentForTimestampsTest>();
				Factory.Save();
				incident3.CloseIncident(resolvedCode, "");
				incident3.PreHookValidateTimestampsForTestAction = () =>
				{
					// Direct SQL to pretend that a bug has caused bad data to enter the db
					TestConnection.ExecuteNonQuery($"UPDATE {tableName} SET {IncidentMainSchema.IM_ResolveTimeUtc.Name} = NULL WHERE {IncidentMainSchema.PK.Name} = '{incident3.PK}'");
				};
				Factory.Save();
				AssertEquals("Disposition should be resolved", DispositionList.Constants.Closed.Resolved, incident3.IM_ResolutionCode);
				AssertEquals("Error is reported", "ResolveTimeUtc shouldn't be empty for a resolved incident", ErrorReporter.LastMessageReported);

				var incident4 = Factory.NewWithValidTestData<SupportIncidentForTimestampsTest>();
				Factory.Save();
				incident4.CloseIncident(DispositionList.Constants.Closed.SelfResolved, "");
				incident4.PreHookValidateTimestampsForTestAction = () =>
				{
					// Direct SQL to pretend that a bug has caused bad data to enter the db
					TestConnection.ExecuteNonQuery($"UPDATE {tableName} SET {IncidentMainSchema.IM_CloseTimeUtc.Name} = NULL WHERE {IncidentMainSchema.PK.Name} = '{incident4.PK}'");
				};
				Factory.Save();
				AssertEquals("Incident status is closed", SupportIncidentLookups.Status.Closed, incident4.IM_Status);
				AssertEquals("Error is reported", "CloseTimeUtc shouldn't be empty for a closed incident", ErrorReporter.LastMessageReported);
			}
			ErrorReporter.Clear();
		}

		public void TestIncidentValidateTimestampsFetchesDBValue()
		{
			ErrorReporter.Clear();
			var tableName = $"{IncidentMainSchema.Constants.SqlSchemaName}.{IncidentMainSchema.Constants.TableName}";
			var incident = Factory.NewWithValidTestData<SupportIncidentForTimestampsTest>();
			Factory.Save();
			incident.CloseIncident(DispositionList.Constants.Closed.SelfResolved, "");
			incident.PreHookValidateTimestampsForTestAction = () =>
			{
				// This shouldn't cause an error as the db value is not affected
				incident.IM_CloseTimeUtc = ZDateTime.Empty;
			};
			Factory.Save();
			AssertEquals("Incident status is closed", SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals("There should be no errors", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestCloseInternalIncidentWithNoUpgrade()
		{
			ReleaseBuildContentForLegacyTest.Enable();

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;

			var company = Factory.NewWithValidTestData<LicenceCompany>();
			company.LC_LE = enterprise.PK;
			company.LC_OH = org.PK;

			var database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			database1.LD_LE = enterprise.PK;
			database1.LD_ServerCode = "AAA";

			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			database2.LD_LE = enterprise.PK;
			database2.LD_ServerCode = "BBB";

			var header = Factory.NewWithValidTestData<LicenceHeader>();
			header.LA_LC = company.PK;
			header.LA_LD = database1.PK;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = company.LC_CompanyCode;
			clientCompany.LCC_LD = database1.PK;
			clientCompany.LCC_OH = org.PK;

			Factory.Save();

			InternalIncidentLicenceSettings settings = new InternalIncidentLicenceSettings();
			LicenceEnterpriseKey key = new LicenceEnterpriseKey();
			key.LE_PK = enterprise.PK;
			settings.LicenceEnterpriseKeys.Add(key);
			settings.EdiProd_LicencePK = header.PK;
			settings.UAT_ALP_LicencePK = header.PK;
			settings.UAT_DPR_LicencePK = header.PK;
			settings.UAT_GPC_LicencePK = header.PK;
			settings.UAT_GPR_LicencePK = header.PK;
			settings.UAT_STD_LicencePK = header.PK;
			EDIDataRegistry.Instance.InternalIncidentLicenceSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var incident = Factory.New<SupportIncident>();
			incident.SetupForInternalReportedIncident("", SupportIncident.InternalIncidentComment, header);
			incident.IM_IncidentNumber = "Z123";
			incident.IM_Description = "Test work ok";
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			var item = (IWorkItemRelatedItem)incident;
			incident.IM_GS_NKAssignedToCurrent = GlbStaff.CurrentUser.GS_Code;
			incident.IM_Status = SupportIncidentLookups.Status.Working;
			Factory.Save();

			var workItem1 = incident.RelatedWorkItems.AddNew();
			workItem1.WKI_Priority = ReleaseRings.Codes.STD;
			var task1 = workItem1.WorkflowItems.AddNew();
			task1.P9_Type = EDITaskTypes_ForTest.TaskCodingOfFunctionality;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var task2 = workItem1.WorkflowItems.AddNew();
			task2.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.STD).CheckinTask;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			item.OnRelatedWorkItemClosed(workItem1);

			Factory.Save();

			var message1 = incident.EConversation.GetTimeOrderedMessages().First(x => x.Body == "Work completed, no upgrade will be sent") as JobConversationMessage;
			AssertEquals("Should be a system message", true, message1.JCM_IsSystem);

			AssertEquals(SupportIncidentLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ClosedInternal, incident.IM_ClosureResolution);
		}

		public void TestShouldNotCalculatedStatusAndDispositionWhenSuspendTriggerCloseIncidentIsTrue()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Description = "I'm a test incident";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = helper.GetJobHeaderForParent(incident, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow 1");

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 10;
			task1.P9_FH_ProcessHeader = workflow.PK;

			incident.SuspendTriggerCloseIncident = false;

			var originalResolutionCode = incident.IM_ResolutionCode;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertNotEquals("ResolutionCode should be changed", originalResolutionCode, incident.IM_ResolutionCode);

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_Description = "I'm a test incident2";
			incident2.IM_Category = SupportIncidentCategoriesList.Codes.Support;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var helper2 = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader2 = helper2.GetJobHeaderForParent(incident2, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow2 = helper2.CreateWorkflow(jobHeader2, "Workflow 2");

			var task2 = incident2.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_Sequence = 10;
			task2.P9_FH_ProcessHeader = workflow2.PK;

			incident2.SuspendTriggerCloseIncident = true;

			var originalResolutionCode2 = incident2.IM_ResolutionCode;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals("ResolutionCode should not be changed", originalResolutionCode2, incident2.IM_ResolutionCode);
		}

		public void TestSuspendTriggerCloseIncidentShouldBeRevertedWhenSaveSucceeded()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_Description = "I'm a test incident";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			Factory.Save();

			incident.IM_Description = "changed IM_Description";
			incident.SuspendTriggerCloseIncident = true;

			AssertEquals("SuspendTriggerCloseIncident should be true", true, incident.SuspendTriggerCloseIncident);
			Factory.Save();
			AssertEquals("SuspendTriggerCloseIncident should revert to false", false, incident.SuspendTriggerCloseIncident);
		}

		public void TestAttachWorkItemWithCompletedCheckInTaskShouldReadyForUpgrade_NoOtherRelatedOpenCheckInTask()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			var buildForTest = Factory.NewWithValidTestData<ReleaseBuild>();
			buildForTest.HL_MajorVersion = 1;
			buildForTest.HL_MinorVersion = 1;
			buildForTest.HL_Release = 1937;
			buildForTest.HL_Patch = 0;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_ReleaseRing = ReleaseRings.Codes.ALP;
			database.LD_HL_CurrentRunningVersion = buildForTest.PK;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			var incident = Factory.New<SupportIncident>();
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			var openIncidentTask = incident.WorkflowItems.AddNew();
			openIncidentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var checkinWorkItemTask = workItem.WorkflowItems.AddNew();
			checkinWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinWorkItemTask.P9_Type = ReleaseRingsLookup.CheckInTaskTypes.FirstOrDefault();
			var openWorkItemTask = workItem.WorkflowItems.AddNew();
			openWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			incident.RelatedItems.Add(workItem);
			Factory.Save();

			AssertEquals("Should set waiting upgrade", DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);
		}

		public void TestAttachWorkItemWithCompletedCheckInTaskShouldNotReadyForUpgrade_HaveOtherRelatedOpenCheckInTask()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			var buildForTest = Factory.NewWithValidTestData<ReleaseBuild>();
			buildForTest.HL_MajorVersion = 1;
			buildForTest.HL_MinorVersion = 1;
			buildForTest.HL_Release = 1937;
			buildForTest.HL_Patch = 0;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_ReleaseRing = ReleaseRings.Codes.ALP;
			database.LD_HL_CurrentRunningVersion = buildForTest.PK;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			var incident = Factory.New<SupportIncident>();
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			var openIncidentTask = incident.WorkflowItems.AddNew();
			openIncidentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();

			var existingAttchedWorkItem = Factory.NewWithValidTestData<NewWorkItem>();
			var checkinWorkItemTask1 = existingAttchedWorkItem.WorkflowItems.AddNew();
			checkinWorkItemTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			checkinWorkItemTask1.P9_Type = ReleaseRingsLookup.CheckInTaskTypes.FirstOrDefault();
			var openWorkItemTask1 = existingAttchedWorkItem.WorkflowItems.AddNew();
			openWorkItemTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			incident.RelatedItems.Add(existingAttchedWorkItem);
			Factory.Save();

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var checkinWorkItemTask = workItem.WorkflowItems.AddNew();
			checkinWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinWorkItemTask.P9_Type = ReleaseRingsLookup.CheckInTaskTypes.FirstOrDefault();
			var openWorkItemTask = workItem.WorkflowItems.AddNew();
			openWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			incident.RelatedItems.Add(workItem);
			Factory.Save();

			AssertNotEquals("Should not set waiting upgrade", DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);
		}

		public void TestWorkItemClosedShouldNotUpdateIncidentToUPOWhenIncidentInUpgradeDelivered()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			var buildForTest = Factory.NewWithValidTestData<ReleaseBuild>();
			buildForTest.HL_MajorVersion = 1;
			buildForTest.HL_MinorVersion = 1;
			buildForTest.HL_Release = 1937;
			buildForTest.HL_Patch = 0;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_ReleaseRing = ReleaseRings.Codes.ALP;
			database.LD_HL_CurrentRunningVersion = buildForTest.PK;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			var incident = Factory.New<SupportIncident>();
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			var openIncidentTask = incident.WorkflowItems.AddNew();
			openIncidentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var checkinWorkItemTask = workItem.WorkflowItems.AddNew();
			checkinWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinWorkItemTask.P9_Type = ReleaseRingsLookup.CheckInTaskTypes.FirstOrDefault();
			var openWorkItemTask = workItem.WorkflowItems.AddNew();
			openWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			incident.RelatedItems.Add(workItem);
			Factory.Save();

			AssertEquals("Should set waiting upgrade", DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);
			incident.SetUpgradeDelivered();
			Factory.Save();
			AssertEquals("IM_ClosureResolution should be UpgradeDelivered", DispositionList.Constants.Closed.UpgradeDelivered, incident.IM_ClosureResolution);
			openWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("IM_ClosureResolution should keep UpgradeDelivered", DispositionList.Constants.Closed.UpgradeDelivered, incident.IM_ClosureResolution);
		}

		public void TestWorkItemClosedShouldNotUpdateIncidentToUPOWhenIncidentInUpgradeDelayed()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			var buildForTest = Factory.NewWithValidTestData<ReleaseBuild>();
			buildForTest.HL_MajorVersion = 1;
			buildForTest.HL_MinorVersion = 1;
			buildForTest.HL_Release = 1937;
			buildForTest.HL_Patch = 0;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_ReleaseRing = ReleaseRings.Codes.ALP;
			database.LD_HL_CurrentRunningVersion = buildForTest.PK;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			var incident = Factory.New<SupportIncident>();
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			var openIncidentTask = incident.WorkflowItems.AddNew();
			openIncidentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			var checkinWorkItemTask = workItem.WorkflowItems.AddNew();
			checkinWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			checkinWorkItemTask.P9_Type = ReleaseRingsLookup.CheckInTaskTypes.FirstOrDefault();
			var openWorkItemTask = workItem.WorkflowItems.AddNew();
			openWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			incident.RelatedItems.Add(workItem);
			Factory.Save();

			AssertEquals("Should set waiting upgrade", DispositionList.Constants.Closed.WaitingUpgrade, incident.IM_ResolutionCode);
			incident.DelayUpgrade();
			Factory.Save();
			AssertEquals("IM_ResolutionCode should be UpgradeDelayed", DispositionList.Constants.Closed.UpgradeDelayed, incident.IM_ResolutionCode);
			openWorkItemTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("IM_ResolutionCode should keep UpgradeDelayed", DispositionList.Constants.Closed.UpgradeDelayed, incident.IM_ResolutionCode);
		}
	}

	public sealed class SupportIncidentTestHelper
	{
		public SupportIncidentTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public SupportIncident CreateIncidentWithLicencedContact(string enterpriseCode = "DDD")
		{
			var lic = BillingTestHelper.CreateLicence(factory, enterpriseCode, "COM", "SRV");

			SupportIncident incident = factory.New<SupportIncident>();
			incident.IM_OH_Client = lic.Company.LC_OH;
			incident.IM_LD = lic.LA_LD;
			incident.IM_LCC = lic.ClientCompany.PK;
			incident.IM_OC_Contact = lic.Company.Header.Contacts[0].PK;
			return incident;
		}

		public SupportIncident CreateIncidentWithLegacyClient()
		{
			var incident = CreateIncidentWithLicencedContact("DDD");
			incident.IM_ClientIncidentReference = "SR00001001";

			ReleaseBuild build = factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.LegacyRelease_EHubSystemMessagesNotSupported);
			incident.Database.LD_HL_CurrentRunningVersion = build.PK;
			incident.Database.LD_PublicEmailAddressForUpdate = "test@test.com";
			factory.Save();

			return incident;
		}

		public SupportIncident CreateIncidentWithBidirectionalUpdateClient()
		{
			var incident = CreateIncidentWithLicencedContact("DD2");
			incident.IM_ClientIncidentReference = "SR00001002";

			ReleaseBuild build = factory.New<ReleaseBuild>();
			build.VersionNumber = new VersionNumber(LicenceDatabase.FirstReleaseCSBiDirectionMessage);
			incident.Database.LD_HL_CurrentRunningVersion = build.PK;

			factory.Save();

			return incident;
		}

		public SupportIncident CreateIncidentWithERequestV2()
		{
			var incident = CreateIncidentWithBidirectionalUpdateClient();
			var releaseBuilds = EDIDataRegistry.Instance.ERequestV2ReleaseBuilds.Value + "," + incident.ClientCompany.Database.CurrentVersion.ExeVersion;
			EDIDataRegistry.Instance.ERequestV2ReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, releaseBuilds);

			return incident;
		}

		#region Workflow

		internal static ProcessTask AddWorkflowTask(SupportIncident incident, ZString description, ZString taskType, ZString status)
		{
			return AddWorkflowTask(incident, description, taskType, status, (incident.ProductAreaAssignedStaff != null ? incident.ProductAreaAssignedStaff.GS_Code : ZString.Empty), ZString.Empty, ZDateTimeOffset.Invalid);
		}

		internal static ProcessTask AddWorkflowTask(SupportIncident incident, ZString description, ZString taskType, ZString status, ZString assignedStaffCode, ZString assignedGroupCode, ZDateTimeOffset schedulatedDate)
		{
			ProcessTask[] tasksByIncreasingSequence = incident.WorkflowItems.Tasks.ToArray<ProcessTask>();
			Array.Sort(tasksByIncreasingSequence, (x, y) => x.P9_Sequence.CompareTo(y.P9_Sequence));
			ProcessTask taskToAddAfter = null;
			foreach (ProcessTask task in tasksByIncreasingSequence)
			{
				if (task.IsCurrent)
				{
					taskToAddAfter = task;
					break;
				}
			}
			if (taskToAddAfter == null)
			{
				taskToAddAfter = (tasksByIncreasingSequence.Length > 0) ? tasksByIncreasingSequence[tasksByIncreasingSequence.Length - 1] : null;
			}

			var newTask = incident.WorkflowItems.AddNew();
			newTask.P9_Sequence = (taskToAddAfter != null) ? (taskToAddAfter.P9_Sequence + 1) : 1;

			SetupWorkflowTask(newTask, description, taskType, status, assignedStaffCode, assignedGroupCode, schedulatedDate);

			return newTask;
		}

		static void SetupWorkflowTask(ProcessTask task, ZString description, ZString taskType, ZString status, ZString assignedStaffCode, ZString assignedGroupCode, ZDateTimeOffset schedulatedDate)
		{
			task.P9_Description = description;
			task.P9_Type = taskType;

			if (!assignedStaffCode.IsEmpty)
			{
				task.P9_GS_NKAssignedStaffMember = assignedStaffCode;
			}

			if (!status.IsEmpty)
			{
				task.P9_Status = status;
			}

			if (!assignedGroupCode.IsEmpty)
			{
				var assignedGroup = task.Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, assignedGroupCode);
				if (assignedGroup != null)
				{
					task.P9_GG_AssignedGroup = assignedGroup.PK;
				}
			}

			if (schedulatedDate.IsValid)
			{
				task.SetMilestoneScheduledDateForTest(schedulatedDate);
			}
		}

		#endregion
	}

	#region Task Provider Test

	[TestedType(typeof(SupportIncident))]
	class SupportIncidentWorkflowProviderTest : WorkflowProviderTest<SupportIncident, SupportIncidentProcessTaskCollection>
	{
		public void TestGetTemplateFilterCriteria_ForIM_Product()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();
			Incident.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Incident.IM_ProductInfo, ProcessTaskTemplate.P0_SubType1Info,
				"AAA",
				ProductTypes.Codes.Enterprise,
				ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForStage()
		{
			Incident.Factory.Save();
			SetBusinessObjectPropertyValueDelegate<ZString> setter = new SetBusinessObjectPropertyValueDelegate<ZString>(
				value =>
				{
					Incident.IM_Category = value;
					Incident.Reopen(IncidentEventFactory.Codes.CargoWiseReopen);
				}
			);

			AssertGetTemplateFilterCriteria(setter, ProcessTaskTemplate.P0_SubType2Info,
				"SUP",
				"DEF",
				ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForIM_OH_Client()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			ZGuid matchingGuid = org1.PK;
			ZGuid nonMatchingGuid = org2.PK;

			Incident.Factory.Save();
			AssertGetTemplateFilterCriteria(Incident.IM_OH_ClientInfo, ProcessTaskTemplate.P0_OH_ClientInfo,
				matchingGuid,
				nonMatchingGuid,
				ZGuid.Empty);
		}

		public void TestGetTemplateFilterCriteria_ForIM_Source()
		{
			Incident.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(Incident.IM_SourceInfo, ProcessTaskTemplate.P0_SubType3Info,
				SupportIncidentLookups.SourceListConstants.ERequestPortal,
				SupportIncidentLookups.SourceListConstants.CreatedFromProject,
				ZString.Empty);
		}

		SupportIncident Incident
		{
			get { return BusinessObject; }
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return EDIJobInvoicingConsumerTypes.Incident.Code; }
		}
	}

	#endregion

	#region RelatableActivity Test

	[TestedType(typeof(SupportIncident))]
	class SupportIncidentRelatableActivityTest : RelatableActivityTestCase<SupportIncident>
	{
		protected override SupportIncident GetNewActivity()
		{
			return Factory.NewWithValidTestData<SupportIncident>();
		}
	}

	#endregion

	[TestedType(typeof(SupportIncidentWithCustomLookups))]
	sealed class SupportIncidentRelatedItemTest : IncidentMainBaseRelatedItemTest
	{
		protected override string ExpectedSelectionCriterion3 => "EYE - Eyeholes!";

		protected override IWorkTaskRelatedItem GetItemForSelectionCriteriaTest()
		{
			var incident = (IncidentMainBase)base.GetItemForSelectionCriteriaTest();

			incident.IM_Module = "EYE";

			var incidentDetailsSource = incident as IIncidentDetailsSource;
			Factory.ClearCachedValue<CodeDescriptionPairList>("ModuleList:" + incidentDetailsSource.ModuleType.ToString() + incidentDetailsSource.Product.PadRight(3) + incidentDetailsSource.ProductArea.PadRight(3));

			return (IWorkTaskRelatedItem)incident;
		}

		internal class SupportIncidentWithCustomLookups : SupportIncident
		{
			public SupportIncidentWithCustomLookups(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override IncidentMainLookups GetNewLookups()
			{
				return new SupportIncidentLookupsWithCustomModuleList(this);
			}

			class SupportIncidentLookupsWithCustomModuleList : SupportIncidentLookups
			{
				public SupportIncidentLookupsWithCustomModuleList(SupportIncident parent)
					: base(parent)
				{
				}

				protected override CodeDescriptionPairList GetModuleListCore(ModuleListType moduleListType, ZString product, ZString productArea)
				{
					return new CodeDescriptionPairList { new CodeDescriptionPair("EYE", "Eyeholes!") };
				}
			}
		}
	}

	[TestedType(typeof(SupportIncident))]
	sealed class SupportIncidentRelatedItemSourceTest : IncidentMainBaseRelatedItemSourceTest
	{
		protected override IWorkTaskRelatedItemSource GetNewSourceBusinessObject()
		{
			var contact = Factory.LoadTop1<OrgContact>(new ZQuery());
			contact.OC_OA_OrgAddress = Factory.NewWithValidTestData<OrgAddress>().PK;

			var incident = (IncidentMainBase)base.GetNewSourceBusinessObject();

			incident.IM_OH_Client = contact.OC_OH;
			incident.IM_OC_Contact = contact.PK;

			Factory.Save();

			return (IWorkTaskRelatedItemSource)incident;
		}
	}
}
