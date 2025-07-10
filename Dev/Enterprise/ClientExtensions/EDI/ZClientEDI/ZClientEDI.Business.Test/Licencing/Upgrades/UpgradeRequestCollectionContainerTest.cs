using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	[TestedType(typeof(UpgradeRequestCollectionContainer))]
	public class UpgradeRequestCollectionContainerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructorAndUpgrades()
		{
			AssertEquals("Two elements in collection", 2, BizObj.Upgrades.Count);
			AssertEquals("SaveToDisk enabled", false, BizObj.IsSaveToDiskInfo.ReadOnly);
			AssertUpgradeMethodBooleanPropertiesAndDescription(true, false, false, false);
			AssertEquals("SendNotification was set from the registry", EDIDataRegistry.Instance.SendUpgradeEmailNotificationAutomatically.Value, BizObj.SendEmailNotificationAutomatically);

			AssertEquals("Element 1 Organisation", Organisation.PK, BizObj.Upgrades[0].OrganisationPk);
			AssertEquals("Element 1 LicDatabase1", LicDatabase1, BizObj.Upgrades[0].LicDatabase);
			AssertEquals("Element 2 Organisation", Organisation.PK, BizObj.Upgrades[1].OrganisationPk);
			AssertEquals("Element 2 LicDatabase2", LicDatabase2, BizObj.Upgrades[1].LicDatabase);

			EDIOrgHeader org2 = Factory.NewWithValidTestData<EDIOrgHeader>();

			UpgradeRequestCollectionContainer container = new UpgradeRequestCollectionContainer(Factory, Organisation, org2);

			AssertEquals("Two elements in collection", 2, container.Upgrades.Count);
			AssertEquals("SaveToDisk disabled", true, container.IsSaveToDiskInfo.ReadOnly);
		}

		public void TestReleaseBuild()
		{
			BizObj.ReleaseBuildPK = TestBuild.PK;

			AssertEquals("ReleaseBuild", TestBuild, BizObj.SelectedReleaseBuild);
		}

		public void TestUpgradeMethodAndDescription()
		{
			AssertUpgradeMethodBooleanPropertiesAndDescription(true, false, false, false);

			BizObj.IsSendViaHttp = true;
			AssertUpgradeMethodBooleanPropertiesAndDescription(false, false, true, false);

			BizObj.IsSaveToDisk = true;
			AssertUpgradeMethodBooleanPropertiesAndDescription(false, false, false, true);
		}

		public void TestSendEmailNotificationAutomatically()
		{
			BizObj.SendEmailNotificationAutomatically = true;
			AssertEquals("SendNotification is true", true, BizObj.SendEmailNotificationAutomatically);

			BizObj.SendEmailNotificationAutomatically = false;
			AssertEquals("SendNotification is false", false, BizObj.SendEmailNotificationAutomatically);
		}

		public void TestAdditionalNotification()
		{
			EDIDataRegistry.Instance.UpgradeEmailDefaultAdditionalNotification = "Do not install this upgrade!!!";
			UpgradeRequestCollectionContainer container = new UpgradeRequestCollectionContainer(Factory);
			AssertEquals("AdditionalNotification", "Do not install this upgrade!!!", container.AdditionalNotification);

			container.SendEmailNotificationAutomatically = true;
			container.AdditionalNotification = "Some Additional Notification";
			AssertEquals("Assigned Value", "Some Additional Notification", container.AdditionalNotification);
		}

		public void TestNotificationSubjectPrefix()
		{
			UpgradeRequestCollectionContainer container = new UpgradeRequestCollectionContainer(Factory);
			AssertNull("NotificationSubjectPrefix", container.NotificationSubjectPrefix);

			container.NotificationSubjectPrefix = "Some Additional Subject";
			AssertEquals("Assigned Value", "Some Additional Subject", container.NotificationSubjectPrefix);
		}

		public void TestGetSubject()
		{
			UpgradeRequestCollectionContainer container = new UpgradeRequestCollectionContainer(Factory);
			AssertEquals("GetSubject", "A System Upgrade Package is Available for " + Organisation.OH_Code, container.GetSubject(Organisation, container.UpgradesForCurrentOrganisation));

			container.NotificationSubjectPrefix = "CS1234: ";
			AssertEquals("GetSubject", "CS1234: A System Upgrade Package is Available for " + Organisation.OH_Code, container.GetSubject(Organisation, container.UpgradesForCurrentOrganisation));
		}

		public void TestGetPreUpgradeStatusWithNoBuildSelected()
		{
			AssertEquals("ReleaseBuildPK.IsEmpty", true, BizObj.ReleaseBuildPK.IsEmpty);
			IPreUpgradeStatus status = BizObj.GetPreUpgradeStatus();
			AssertNotNull("Not Null", status);
			AssertEquals("IsError", true, status.IsError);
			AssertEquals("Message", "Error - ReleaseBuildPK: Please enter a value.", status.Message);
		}

		void SetAllowSaveUpgradePackageToDisk(bool value)
		{
			ZString code = GlbStaff.CurrentUser.GS_Code;
			GlbStaff.CurrentUser.GS_Code = "RJW";
			EDIDataRegistry.Instance.AllowSaveUpgradePackageToDisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			GlbStaff.CurrentUser.GS_Code = code;
		}

		public void TestGetPreUpgradeStatus_SaveToDisk()
		{
			SetAllowSaveUpgradePackageToDisk(false);
			BizObj.IsSaveToDisk = true;
			TestBuild.HL_Superceded = false;
			BizObj.ReleaseBuildPK = TestBuild.PK;
			IPreUpgradeStatus status = BizObj.GetPreUpgradeStatus();
			AssertNotNull("Not Null", status);
			AssertEquals("IsError", true, status.IsError);
			Assert("Message", status.Message.StartsWith("Save To Disk is disabled."));

			SetAllowSaveUpgradePackageToDisk(true);
			BizObj.IsSaveToDisk = true;
			status = BizObj.GetPreUpgradeStatus();
			AssertNotNull("Not Null", status);
			AssertEquals("IsError", false, status.IsError);
		}

		public void TestGetPreUpgradeStatus()
		{
			CreateExtraTestData();

			TestBuild.HL_Superceded = false;
			BizObj.ReleaseBuildPK = TestBuild.PK;
			BizObj.SendEmailNotificationAutomatically = true;

			IPreUpgradeStatus status = BizObj.GetPreUpgradeStatus();

			AssertNotNull("Not Null", status);
			AssertEquals("No Error", false, status.IsError);
			AssertEquals("Message is empty", "", status.Message);
			AssertEquals("OrganisationCount", 2, status.OrganisationCount);
			AssertEquals("FirstOrganisation", Organisation, status.GetOrganisations()[0]);
			AssertEquals("OrganisationWithoutNotificationCount", 0, status.OrganisationWithoutNotificationCount);
			AssertEquals("ToSendViaHttpServerCount", 3, status.GetRequestCountByStatusType(UpgradeStatusType.ViaHttp));
			AssertEquals("BlockedServerCount", 0, status.GetRequestCountByStatusType(UpgradeStatusType.Blocked));
			AssertEquals("UnspecifiedNotificationRecipientCount", 0, status.GetRequestCountByStatusType(UpgradeStatusType.WithoutNotificationAddress));
		}

		public void TestPlaceUpgradesToClientsWithNoBuildSelected()
		{
			Assert("ReleaseBuild not specified", BizObj.ReleaseBuildPK.IsEmpty);

			IPostUpgradeStatus status = BizObj.PlaceUpgradesToClients();

			AssertNotNull("Not Null", status);
			AssertEquals("IsError", true, status.IsError);
			AssertEquals("Meassage", UpgradeRequestCollectionContainer.NoBuildSelectedMessage, status.Message);
		}

		public void TestPlaceUpgradesToClients()
		{
			CreateExtraTestData();

			TestBuild.HL_ReleaseStatus = ReleaseRings.Codes.GPR;
			BizObj.ReleaseBuildPK = TestBuild.PK;
			BizObj.SendEmailNotificationAutomatically = true;
			BizObj.AdditionalNotification = "Some Additional Notification";

			IPostUpgradeStatus status = BizObj.PlaceUpgradesToClients();

			AssertNotNull("Not Null", status);
			AssertEquals("No Error", false, status.IsError);
			AssertEquals("Message is empty", "", status.Message);

			AssertEquals("CreatedUpgradesToClients Count", 3, status.CreatedUpgradesToClientsCount);
			AssertEquals("SentNotificationEmails Count", 2, status.NotificationEmailsCount);
			AssertEquals("Unsent NotificationEmails Count", 0, status.NotificationEmailsWithoutRecipientCount);
			AssertEquals("OrganisationWithoutNotification Count", 0, status.OrganisationWithoutNotificationCount);

			CustomerServiceEmail[] emails = status.GetNotificationEmails();
			AssertEquals("From Display Name", SupportIncident.SupportDisplayName, emails[0].FromDisplayName);
			AssertEquals("From Email Address", SupportIncident.SupportEmailAddress, emails[0].FromEmailAddress);
			AssertEquals("Email To Recipient", "Test.Contact@edi.com", emails[0].ToEmailAddress);
			UpgradesToClient[] createdUpgrades = status.GetCreatedUpgradesToClients();
			AssertEquals("Email Subject", "A System Upgrade Package is Available for " + createdUpgrades[0].Organisation.OH_Code, emails[0].Subject);
			string expectedBody = "A new upgrade package is available for your system with the following details:\r\n\r\nRelease: " +
				TestBuild.ReleaseDisplayText +
				"\r\nExe Date: " +
				TestBuild.HL_ExeVersionDate.ToLongTimeString() + "\r\nVersion Number: " +
				TestBuild.ExeVersion +
				"\r\n\r\nSome Additional Notification\r\n";

			AssertEquals("Message Body", expectedBody, emails[0].Body.ToString());
			AssertEquals("SaveAsNote", false, emails[0].SaveAsNote);

			StmALog lastEvent = Organisation.Logs.MostRecentLogByEventTime(Events.UpgradeSucceeded);
			AssertNotNull("Event should be logged", lastEvent);
			AssertEquals("Reference should match", TestBuild.ReleaseDisplayText + " (v1.2.3.4) will be sent to this client, SV1 via HTP, SV2 via HTP", lastEvent.SL_Reference);

			lastEvent = TestBuild.Logs.MostRecentLogByEventTime(Events.Delivered);
			AssertNotNull("Event should be logged", lastEvent);
			Assert("Reference should match one of the organisation codes", "ABCSYD" == lastEvent.SL_Reference || "CDESYD" == lastEvent.SL_Reference);
		}

		public void TestPlaceUpgradesToClients_WithIncidents()
		{
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "TESTSYD";
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test Contact";
			contact1.OC_Email = "Test.Contact@edi.com";
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Defect Contact";
			contact2.OC_Email = "Defect.Contact@edi.com";
			OrgContact contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "FR Contact";
			contact3.OC_Email = "FR.Contact@edi.com";

			LicenceCompany licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany.LC_OH = org.PK;
			LicenceEnterprise licEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			org.LicCompany.LC_LE = licEnt.PK;

			LicenceDatabase database1 = AddLicDatabaseToOrg(org, UpgradeMethods.Codes.Http);
			database1.ContractInstallerOrInternalTechContact.OC_Email = "Defect.Contact@edi.com";
			LicenceDatabase database2 = AddLicDatabaseToOrg(org, UpgradeMethods.Codes.Http);
			database2.ContractInstallerOrInternalTechContact.OC_Email = "Test.Contact@edi.com";

			bizObj = new UpgradeRequestCollectionContainerTestHelper(Factory, Organisation, org);

			TestBuild.HL_ReleaseStatus = ReleaseRings.Codes.GPR;
			BizObj.ReleaseBuildPK = TestBuild.PK;
			BizObj.SendEmailNotificationAutomatically = true;
			BizObj.AdditionalNotification = "Some Additional Notification";

			SupportIncident incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.SetupForNewCreatedDefect();
			incident1.IM_OH_Client = org.PK;
			incident1.IM_Priority = "CR4";
			incident1.IM_OC_Contact = contact2.PK;
			SupportIncident incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.SetupForNewCreatedFeatureRequest();
			incident2.IM_OH_Client = org.PK;
			incident2.IM_OC_Contact = contact3.PK;

			List<SupportIncident> incidents = new List<SupportIncident>();
			incidents.Add(incident1);
			incidents.Add(incident2);
			BizObj.Incidents = incidents;

			AssertEquals("Pre-condition", 0, incident1.DocManagerInfo.AllEDocs.Count);
			AssertEquals("Pre-condition", 0, incident2.DocManagerInfo.AllEDocs.Count);

			IPostUpgradeStatus status = BizObj.PlaceUpgradesToClients();

			AssertNotNull("Not Null", status);
			AssertEquals("No Error", false, status.IsError);
			AssertEquals("Message is empty", "", status.Message);

			AssertEquals("CreatedUpgradesToClients Count", 4, status.CreatedUpgradesToClientsCount);
			AssertEquals("SentNotificationEmails Count", 2, status.NotificationEmailsCount);
			AssertEquals("Unsent NotificationEmails Count", 0, status.NotificationEmailsWithoutRecipientCount);
			AssertEquals("OrganisationWithoutNotification Count", 0, status.OrganisationWithoutNotificationCount);

			AssertEquals(1, incident1.DocManagerInfo.AllEDocs.Count);
			AssertEquals(1, incident2.DocManagerInfo.AllEDocs.Count);

			ZString emailContent1 = incident1.DocManagerInfo.AllEDocs[0].GetImageDataReader().ConvertToAsciiStringAndCloseStream();
			AssertEquals("No duplicate recipients", 1, emailContent1.Occurrences("Defect.Contact@edi.com"));

			ZString emailContent2 = incident2.DocManagerInfo.AllEDocs[0].GetImageDataReader().ConvertToAsciiStringAndCloseStream();
			AssertEquals("No duplicate recipients", 1, emailContent2.Occurrences("FR.Contact@edi.com"));

			database2.ContractInstallerOrInternalTechContact.OC_Email = "";
			bizObj = new UpgradeRequestCollectionContainerTestHelper(Factory, org);
			BizObj.Incidents = incidents;
			BizObj.ReleaseBuildPK = TestBuild.PK;
			BizObj.SendEmailNotificationAutomatically = true;
			BizObj.AdditionalNotification = "Some Additional Notification 2";
			status = BizObj.PlaceUpgradesToClients();
			AssertEquals("CreatedUpgradesToClients Count", 2, status.CreatedUpgradesToClientsCount);
			AssertEquals("SentNotificationEmails Count", 1, status.NotificationEmailsCount);
			AssertEquals(2, incident1.DocManagerInfo.AllEDocs.Count);
			AssertEquals(2, incident2.DocManagerInfo.AllEDocs.Count);
			Assert(((BusinessObject)incident1.DocManagerInfo.AllEDocs[1]).IsInDatabase);
			emailContent1 = incident1.DocManagerInfo.AllEDocs[1].GetImageDataReader().ConvertToAsciiStringAndCloseStream();
			AssertEquals("No duplicate recipients", 1, emailContent1.Occurrences("Defect.Contact@edi.com"));
			Assert(((BusinessObject)incident2.DocManagerInfo.AllEDocs[1]).IsInDatabase);
			emailContent2 = incident2.DocManagerInfo.AllEDocs[1].GetImageDataReader().ConvertToAsciiStringAndCloseStream();
			AssertEquals("No duplicate recipients", 1, emailContent2.Occurrences("FR.Contact@edi.com"));
		}

		public void TestNotificationEmailsShouldAlwaysBeSentToTechContacts()
		{
			CreateExtraTestData();

			TestBuild.HL_ReleaseStatus = ReleaseRings.Codes.GPR;
			BizObj.ReleaseBuildPK = TestBuild.PK;
			BizObj.Upgrades[0].ClientContactEmailAddress = "somecustomer@customer.com";
			BizObj.Upgrades[1].LicDatabase.LD_OC_ContractInstallerOrInternalTechContact = Factory.New<OrgContact>().PK;
			BizObj.Upgrades[1].LicDatabase.ContractInstallerOrInternalTechContact.OC_Email = "someTechContact@cargowise.com";

			var org = BizObj.Factory.Load<EDIOrgHeader>(BizObj.Upgrades[0].OrganisationPk);
			EmailToContactBusinessObject email = BizObj.CreateNotificationEmailForOrganisation(org, BizObj.Upgrades.ToArray<UpgradeRequest>());
			AssertEquals("Test.Contact@edi.com;someTechContact@cargowise.com", email.ToEmailAddress);
			AssertEquals("somecustomer@customer.com", email.Cc);
		}

		public void TestPlaceUpgradesToClientsThrowExceptionOnCreatingUpgradesToClient()
		{
			CreateExtraTestData();

			BizObj.ReleaseBuildPK = TestBuild.PK;
			BizObj.ThrowExceptionOnCreatingUpgradesToClient = true;

			ErrorReporter.Clear();
			IPostUpgradeStatus status = BizObj.PlaceUpgradesToClients();

			AssertNotNull("Not Null", status);
			AssertEquals("Is Error", true, status.IsError);
			string expectedMessage = "An error occurred while creating upgrades:\r\nException on Creating UpgradesToClient";
			AssertEquals("Message contains Exception Msg", expectedMessage, status.Message);
			AssertEquals(typeof(InvalidOperationException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("An error occurred while creating upgrades", ErrorReporter.LastKeyReported);
			AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestPlaceUpgradesToClientsThrowExceptionOnSendingNotication()
		{
			CreateExtraTestData();

			BizObj.ReleaseBuildPK = TestBuild.PK;
			BizObj.SendEmailNotificationAutomatically = true;
			BizObj.ThrowExceptionOnSendNotificationEmails = true;

			ErrorReporter.Clear();
			IPostUpgradeStatus status = BizObj.PlaceUpgradesToClients();

			AssertNotNull("Not Null", status);
			AssertEquals("Is Error", true, status.IsError);
			string expectedMessage = "An error occurred while sending the upgrade notification emails:\r\nException on Sending Notification Emails";
			AssertEquals("Message contains Exception Msg", expectedMessage, status.Message);
			AssertEquals(typeof(InvalidOperationException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("An error occurred while sending the upgrade notification emails", ErrorReporter.LastKeyReported);
			AssertEquals(ErrorReporter.LastExceptionReported.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestSaveToDiskWithNoBuildSelected()
		{
			Assert("ReleaseBuild not specified", BizObj.ReleaseBuildPK.IsEmpty);

			IPostUpgradeStatus status = BizObj.SaveToDisk("");

			AssertNotNull("Not Null", status);
			AssertEquals("IsError", true, status.IsError);
			AssertEquals("Meassage", UpgradeRequestCollectionContainer.NoBuildSelectedMessage, status.Message);
		}

		public void TestSaveToDisk()
		{
			Assert("ReleaseBuild not specified", BizObj.ReleaseBuildPK.IsEmpty);

			string dummyPath = "DummyPath";
			BizObj.ReleaseBuildPK = TestBuild.PK;
			IPostUpgradeStatus status = BizObj.SaveToDisk(dummyPath);

			AssertNotNull("Not Null", status);
			AssertEquals("No Errors", false, status.IsError);
			string expectedMessage = "Package was successfully built and saved as " + Path.Combine(dummyPath, TestBuild.PackageName) + "\r\nWARNING: This must not be deployed to WiseCloud as it may not contain all necessary Winzor binaries.";
			AssertEquals("Meassage", expectedMessage, status.Message);
			AssertStartsWith("Output path", dummyPath, status.PackagePath);
		}

		public void TestValidateReleaseBuildPK()
		{
			BizObj.ReleaseBuildPK = ZGuid.Invalid;
			AssertHasError(BizObj.ReleaseBuildPKInfo, "Enter a valid selection.");

			BizObj.ReleaseBuildPK = ZGuid.Empty;
			AssertHasError(BizObj.ReleaseBuildPKInfo, "Please enter a value.");

			ReleaseBuild goodBuild = ReleaseBuild.NewForTesting(Factory, ReleaseRings.Codes.ALP, false);
			ReleaseBuild supersededBuild = ReleaseBuild.NewForTesting(Factory, ReleaseRings.Codes.ALP, true);

			EDISecurityCheckpoints.OrgLicenceModifySendSupersededUpgrade.IsAllowed = false;
			BizObj.IsSendViaDefault = true;
			BizObj.ReleaseBuildPK = supersededBuild.PK;
			AssertHasError(BizObj.ReleaseBuildPKInfo, "Superseded builds cannot be deployed to clients. Please select this only if you wish to save the build to disk.\r\n"
+ "If you do need to send superseded builds, please ask your administrator to change either your Staff or Group Security Rights to allow access to:\r\n\r\n"
+ EDISecurityCheckpoints.OrgLicenceModifySendSupersededUpgrade.DisplayTextPathToSecurityRight);

			EDISecurityCheckpoints.OrgLicenceModifySendSupersededUpgrade.IsAllowed = true;
			BizObj.ReleaseBuildPK = ZGuid.Empty;
			BizObj.ReleaseBuildPK = supersededBuild.PK;
			AssertHasWarning(BizObj.ReleaseBuildPKInfo, "Superseded builds should not be deployed to clients. Please select this only if:\r\n"
+ "1. The client has tested a specific version and wants to deploy that version to their production database\r\n"
+ "2. The client needs an intermediate upgrade to move from a very old version to a newer version");

			BizObj.ReleaseBuildPK = goodBuild.PK;
			AssertNoErrors(BizObj.ReleaseBuildPKInfo);

			BizObj.ReleaseBuildPK = supersededBuild.PK;
			BizObj.IsSaveToDisk = true;
			AssertNoErrors(BizObj.ReleaseBuildPKInfo);
		}

		public void TestValidateSelectedBuildIsActive()
		{
			TestBuild.HL_IsActive = false;
			BizObj.ReleaseBuildPK = TestBuild.PK;

			AssertHasError(BizObj.ReleaseBuildPKInfo, "This build has not been retained and cannot be deployed to clients.\r\nDeploy the latest patch on the same branch instead.");
		}

		public void TestValidateIsSaveToDisk()
		{
			SetAllowSaveUpgradePackageToDisk(true);
			SecurityCheckpoint checkpoint = EDISecurityCheckpoints.OrgLicenceModifySaveUpgradeToDisk;
			checkpoint.IsAllowed = true;
			BizObj.IsSaveToDiskInfo.ClearAllNotifications();
			BizObj.IsSaveToDisk = true;
			AssertHasWarningContaining(BizObj.IsSaveToDiskInfo, "Saving to disk function is still accessible to CargoWise staff");
			AssertNoErrors(BizObj.IsSaveToDiskInfo);

			BizObj.IsSaveToDisk = false;
			checkpoint.IsAllowed = false;
			BizObj.IsSaveToDisk = true;
			AssertHasError(BizObj.IsSaveToDiskInfo,
				"If you need to save upgrade packages to disk, please ask your administrator to change either your Staff or Group Security Rights to allow access to:\r\n\r\n"
				+ checkpoint.DisplayTextPathToSecurityRight);

			BizObj.IsSaveToDisk = false;
			AssertNoNotifications(BizObj.IsSaveToDiskInfo);

			checkpoint.IsAllowed = true;

			SetAllowSaveUpgradePackageToDisk(false);
			BizObj.IsSaveToDiskInfo.ClearAllNotifications();
			BizObj.IsSaveToDisk = true;
			AssertHasErrorContaining(BizObj.IsSaveToDiskInfo, "Save To Disk is disabled");

			BizObj.IsSaveToDisk = false;
			AssertNoNotifications(BizObj.IsSaveToDiskInfo);
		}

		public void TestSettingReleaseBuildPKValidatesUpgrades()
		{
			ReleaseBuild build1 = Factory.New<ReleaseBuild>();
			ReleaseBuild build2 = Factory.New<ReleaseBuild>();

			build1.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			build2.HL_ReleaseStatus = ReleaseRings.Codes.GPR;

			LicDatabase1.LD_ReleaseRing = ReleaseRings.Codes.STD;
			LicDatabase2.LD_ReleaseRing = ReleaseRings.Codes.GPR;

			BizObj.ReleaseBuildPK = build1.PK;
			AssertHasNotifications(BizObj.Upgrades[0].ReleaseRingInfo);
			AssertHasNotifications(BizObj.Upgrades[1].ReleaseRingInfo);

			BizObj.ReleaseBuildPK = build2.PK;
			AssertNoNotifications(BizObj.Upgrades[0].ReleaseRingInfo);
			AssertNoNotifications(BizObj.Upgrades[1].ReleaseRingInfo);
		}

		#region Implementation

		#region TestHelperClass

		public class UpgradeRequestCollectionContainerTestHelper : UpgradeRequestCollectionContainer
		{
			public UpgradeRequestCollectionContainerTestHelper(BusinessObjectFactory factory, params EDIOrgHeader[] organisationsToUpgrade)
				: base(factory, organisationsToUpgrade)
			{
			}

			protected override string BuildPackage(string targetDirectory, string licenceEnterpriseCode)
			{
				return Path.Combine(targetDirectory, SelectedReleaseBuild.PackageName);
			}

			protected override UpgradesToClient CreateUpgradesToClient(UpgradeRequest request)
			{
				if (ThrowExceptionOnCreatingUpgradesToClient)
				{
					throw new InvalidOperationException("Exception on Creating UpgradesToClient");
				}

				return base.CreateUpgradesToClient(request);
			}

			protected override void SendNotificationEmails(EmailToContactBusinessObject[] emails)
			{
				if (ThrowExceptionOnSendNotificationEmails)
				{
					throw new InvalidOperationException("Exception on Sending Notification Emails");
				}

				base.SendNotificationEmails(emails);
			}

			public bool ThrowExceptionOnCreatingUpgradesToClient;
			public bool ThrowExceptionOnSendNotificationEmails;
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UpgradeRequestCollectionContainer(Factory, Factory.NewWithValidTestData<EDIOrgHeader>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			bizObj = null;
		}

		#endregion

		#region Assertions

		void AssertUpgradeMethodBooleanPropertiesAndDescription(bool @default, bool email, bool http, bool saveToDisk)
		{
			AssertEquals("IsSendViaDefault", @default, BizObj.IsSendViaDefault);
			AssertEquals("IsSendViaHttp", http, BizObj.IsSendViaHttp);
			AssertEquals("IsSaveToDisk", saveToDisk, BizObj.IsSaveToDisk);

			if (@default)
			{
				AssertEquals("Description for Default", UpgradeRequestCollectionContainer.DefaultDescription, BizObj.Description);
			}

			if (http)
			{
				AssertEquals("Deescription for Http", UpgradeRequestCollectionContainer.HttpDescription, BizObj.Description);
			}

			if (saveToDisk)
			{
				AssertEquals("Deescription for SaveToDisk", UpgradeRequestCollectionContainer.SaveToDiskDescription, BizObj.Description);
				AssertEquals("SheduledTime is ReadOnly", true, BizObj.ScheduledDateTimeInfo.ReadOnly);
				AssertEquals("Upgrades are ReadOnly", true, BizObj.Upgrades.ReadOnly);
				AssertEquals("AdditionalNotification is ReadOnly", true, BizObj.AdditionalNotificationInfo.ReadOnly);
				AssertEquals("SendEmailNotificationAutomatically is ReadOnly", true, BizObj.SendEmailNotificationAutomaticallyInfo.ReadOnly);
			}
			else
			{
				AssertEquals("SheduledTime is not ReadOnly", false, BizObj.ScheduledDateTimeInfo.ReadOnly);
				AssertEquals("Upgrades are not ReadOnly", false, BizObj.Upgrades.ReadOnly);
				AssertEquals("AdditionalNotification is not ReadOnly", false, BizObj.AdditionalNotificationInfo.ReadOnly);
				AssertEquals("SendEmailNotificationAutomatically is not ReadOnly", false, BizObj.SendEmailNotificationAutomaticallyInfo.ReadOnly);
			}
		}

		#endregion

		#region Properties

		protected UpgradeRequestCollectionContainerTestHelper BizObj
		{
			get
			{
				if (bizObj == null)
				{
					bizObj = new UpgradeRequestCollectionContainerTestHelper(Factory, Organisation);
				}

				return bizObj;
			}
		}

		protected EDIOrgHeader Organisation
		{
			get
			{
				if (testOrganisation == null)
				{
					testOrganisation = Factory.New<EDIOrgHeader>();
					testOrganisation.OH_Code = "ABCSYD";
					testOrganisation.Contacts.AddNew();
					testOrganisation.Contacts[0].OC_ContactName = "Test Contact";
					testOrganisation.Contacts[0].OC_Email = "Test.Contact@edi.com";

					LicenceCompany licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
					licenceCompany.LC_OH = testOrganisation.PK;
					LicenceEnterprise licEnt = Factory.NewWithValidTestData<LicenceEnterprise>();

					testOrganisation.LicCompany.LC_LE = licEnt.PK;
					LicDatabase1.LD_LE = Organisation.LicEnterprise.PK;
					LicDatabase2.LD_LE = Organisation.LicEnterprise.PK;
					LicDatabase1.LD_OC_ContractInstallerOrInternalTechContact = testOrganisation.Contacts[0].PK;
					LicDatabase2.LD_OC_LicenseeAdminContact = testOrganisation.Contacts[0].PK;
					LicDatabase1.LD_ServerCode = "SV1";
					LicDatabase2.LD_ServerCode = "SV2";

					LicDatabase1.LD_PublicEmailAddressForUpdate = "test@edi.com";
					LicDatabase2.LD_PublicEmailAddressForUpdate = "test@edi.com";
					LicDatabase1.LD_HL_CurrentRunningVersion = TestHttpBuild.PK;
					LicDatabase2.LD_HL_CurrentRunningVersion = TestHttpBuild.PK;

					LicenceHeader header1 = Organisation.LicCompany.LicHeadersForAllDatabases.AddNew();
					header1.LA_LC = Organisation.LicCompany.PK;
					header1.LA_LD = LicDatabase1.PK;

					LicenceHeader header2 = Organisation.LicCompany.LicHeadersForAllDatabases.AddNew();
					header2.LA_LC = Organisation.LicCompany.PK;
					header2.LA_LD = LicDatabase2.PK;
				}

				return testOrganisation;
			}
		}

		protected LicenceDatabase LicDatabase1
		{
			get
			{
				if (licDatabase1 == null)
				{
					licDatabase1 = Factory.NewWithValidTestData<LicenceDatabase>();
					licDatabase1.LD_ReleaseRing = ReleaseRings.Codes.GPR;
				}
				return licDatabase1;
			}
		}

		protected LicenceDatabase LicDatabase2
		{
			get
			{
				if (licDatabase2 == null)
				{
					licDatabase2 = Factory.NewWithValidTestData<LicenceDatabase>();
					licDatabase1.LD_ReleaseRing = ReleaseRings.Codes.GPR;
				}
				return licDatabase2;
			}
		}

		protected ReleaseBuild TestBuild
		{
			get
			{
				if (testBuild == null)
				{
					testBuild = CreateReleaseBuild(1, 2, 3, 4);
					testBuild.HL_ExeVersionDate = new ZDateTime(2005, 1, 31, 12, 23, 0);
					testBuild.HL_IsActive = true;
				}
				return testBuild;
			}
		}

		protected ReleaseBuild TestEWABuild
		{
			get
			{
				if (testEWABuild == null)
				{
					testEWABuild = CreateReleaseBuild(1, 1, 1938, 0);
				}
				return testEWABuild;
			}
		}

		protected ReleaseBuild TestHttpBuild
		{
			get
			{
				if (testHttpBuild == null)
				{
					FirstSupportingVersion supportingVersion = new HttpDownload();
					testHttpBuild = CreateReleaseBuild(supportingVersion.VersionMajorNumber, supportingVersion.VersionMinorNumber, supportingVersion.VersionReleaseNumber, 0);
				}
				return testHttpBuild;
			}
		}

		protected OrgContact TestContact
		{
			get { return Organisation.Contacts[0]; }
		}

		UpgradeRequestCollectionContainerTestHelper bizObj;
		EDIOrgHeader testOrganisation;
		LicenceDatabase licDatabase1;
		LicenceDatabase licDatabase2;
		ReleaseBuild testBuild;
		ReleaseBuild testEWABuild;
		ReleaseBuild testHttpBuild;

		#endregion

		protected void CreateExtraTestData()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "CDESYD";
			org.Contacts.AddNew();
			org.Contacts[0].OC_ContactName = "Test Contact";
			org.Contacts[0].OC_Email = "Test.Contact@edi.com";

			LicenceCompany licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany.LC_OH = org.PK;
			LicenceEnterprise licEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			org.LicCompany.LC_LE = licEnt.PK;

			AddLicDatabaseToOrg(org, UpgradeMethods.Codes.Http);

			bizObj = new UpgradeRequestCollectionContainerTestHelper(Factory, Organisation, org);
			AssertEquals("UpgradeRequests Total Count", 3, BizObj.Upgrades.Count);
		}

		protected LicenceDatabase AddLicDatabaseToOrg(EDIOrgHeader org, string supportedUpgradeMethod)
		{
			var enterprise = org.LicCompany.LicEnterprise;
			LicenceDatabase db = Factory.NewWithValidTestData<LicenceDatabase>();
			enterprise.Databases.Add(db);
			db.LD_LE = enterprise.PK;
			db.LD_ServerCode = "S" + enterprise.Databases.Count.ToString();
			LicenceHeader header = org.LicCompany.LicHeadersForAllDatabases.AddNew();
			header.LA_LC = org.LicCompany.PK;
			header.LA_LD = db.PK;

			if (!string.IsNullOrEmpty(supportedUpgradeMethod) &&
				supportedUpgradeMethod != UpgradeMethods.Codes.Blocked)
			{
				db.LD_PublicEmailAddressForUpdate = "test@edi.com";

				if (supportedUpgradeMethod == UpgradeMethods.Codes.Http)
				{
					db.LD_HL_CurrentRunningVersion = TestHttpBuild.PK;
				}
			}
			else if (supportedUpgradeMethod == UpgradeMethods.Codes.Blocked)
			{
				db.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			}

			db.LD_OC_ContractInstallerOrInternalTechContact = org.Contacts[0].PK;

			return db;
		}

		ReleaseBuild CreateReleaseBuild(int majorVersion, int minorVersion, int release, int patch)
		{
			ReleaseBuild result = Factory.NewWithValidTestData<ReleaseBuild>();
			result.HL_MajorVersion = majorVersion;
			result.HL_MinorVersion = minorVersion;
			result.HL_Release = release;
			result.HL_Patch = patch;
			result.HL_ReleaseStatus = ReleaseRings.Codes.GPR;
			return result;
		}

		#endregion
	}
}
