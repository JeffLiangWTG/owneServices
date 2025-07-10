using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Module.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Test;
using Enterprise.Core;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.IncidentManager.Test
{
	[TestedType(typeof(IncidentBasedAutoDeploymentServiceTask))]
	class IncidentBasedAutoDeploymentServiceTaskTest : ServiceTaskTestCase<IncidentBasedAutoDeploymentServiceTask>
	{
		[TestDate(2017, 6, 5, 16, 39, 0)]
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));

			ZDateTime now = ZDateTime.Now;
			ReleaseBuild currentBuild = CreateReleaseBuild(ReleaseRings.Codes.DPR, now.AddDays(-2));
			ReleaseBuild oldBuild = CreateReleaseBuild(ReleaseRings.Codes.DPR, now.AddDays(-1));
			ReleaseBuild build = CreateReleaseBuild(ReleaseRings.Codes.DPR, now);
			oldBuild.HL_Patch = oldBuild.HL_Patch + 1;
			build.HL_Patch = build.HL_Patch + 2;
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.DPR);
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.DPR, now.AddDays(-1));
			((EDIOrgHeader)incident.Client).LicCompany.LicEnterprise.Databases[0].LD_HL_CurrentRunningVersion = currentBuild.PK;
			Factory.Save();
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(process.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => process.RunTask());
			}
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		#region Delay upgrade

		[TestDate(2006, 6, 7)]
		public void TestCR4DoesNotSendUpgradeForNotNCW()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			var cr4 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			cr4.IM_Priority = "CR4";
			cr4.IM_Description = "cr4 incident";

			var licenceClient1Licence1GprDefectIncident = AttachNewClientAndNewLicence(cr4, "EC1", ReleaseRings.Codes.GPR);
			var client1 = (EDIOrgHeader)cr4.Client;

			AttachNewCompletedWorkItem(cr4, ReleaseRings.Codes.GPR, now.AddDays(-1));

			var client1Contact1 = CreateContact(client1, "contact1@client1.com");
			licenceClient1Licence1GprDefectIncident.Database.LD_OC_LicenseeAdminContact = client1Contact1.PK;

			var currentBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddDays(-3));
			currentBuild.VersionNumber = new VersionNumber(23, 5, 16, 12);
			cr4.Database.LD_HL_CurrentRunningVersion = currentBuild.PK;
			cr4.Database.LD_HostedLocation = "SYD";

			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			gprBuild.VersionNumber = new VersionNumber(23, 5, 16, 20);

			var weeklyBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddDays(-7));
			weeklyBuild.VersionNumber = new VersionNumber(23, 5, 16, 10);

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			process.RunTask();

			var newFactory = new BusinessObjectFactory();
			SupportIncident loadedIncident = newFactory.Load<SupportIncident>(cr4.PK);
			AssertEquals("cr4." + IncidentMainSchema.IM_ResolutionCode.Name, SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed, loadedIncident.IM_ResolutionCode);

			var gprUpgrader = FindUpgrader(gprBuild, client1);
			AssertNull("gprUpgrader", gprUpgrader);

			AssertIsProcessed("cr4", cr4, true, "Upgrade has been delayed and will run on schedule");
			var last = loadedIncident.EConversation.GetTimeOrderedMessages().Last();
			AssertEquals("The next weekly release will contain changes to address your issue and will be delivered to your system when it becomes available.", last.Body);
			AssertEquals(1, loadedIncident.EConversation.GetTimeOrderedMessages().Count);
			AssertReporterCounts(1, 1, 0);

			process.RunTask();
			newFactory = new BusinessObjectFactory();
			loadedIncident = newFactory.Load<SupportIncident>(cr4.PK);
			AssertEquals("cr4." + IncidentMainSchema.IM_ResolutionCode.Name, SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed, loadedIncident.IM_ResolutionCode);

			gprUpgrader = FindUpgrader(gprBuild, client1);
			AssertNull("gprUpgrader", gprUpgrader);

			AssertIsProcessed("cr4", cr4, true, "Upgrade has been delayed and will run on schedule");
			var last2 = loadedIncident.EConversation.GetTimeOrderedMessages().Last();
			AssertEquals("The next weekly release will contain changes to address your issue and will be delivered to your system when it becomes available.", last2.Body);
			AssertEquals(last.Id, last2.Id);
			AssertEquals(1, loadedIncident.EConversation.GetTimeOrderedMessages().Count);
			AssertReporterCounts(1, 2, 0);
		}

		[TestDate(2023, 11, 30)]
		public void TestCR4SendWeeklyBuildUpgradeForNotNCW_WhenNotInternalHostedSystem()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			var cr4 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			cr4.IM_Priority = "CR4";
			cr4.IM_Description = "cr4 incident";

			var licenceClient1Licence1GprDefectIncident = AttachNewClientAndNewLicence(cr4, "EC1", ReleaseRings.Codes.GPR);
			var client1 = (EDIOrgHeader)cr4.Client;

			AttachNewCompletedWorkItem(cr4, ReleaseRings.Codes.GPR, now.AddDays(-1));

			var client1Contact1 = CreateContact(client1, "contact1@client1.com");
			licenceClient1Licence1GprDefectIncident.Database.LD_OC_LicenseeAdminContact = client1Contact1.PK;

			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			var weeklyBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddMinutes(-1));
			cr4.Database.LD_HostedLocation = "SYD";

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			process.RunTask();

			var newFactory = new BusinessObjectFactory();
			SupportIncident loadedIncident = newFactory.Load<SupportIncident>(cr4.PK);
			AssertEquals("cr4." + IncidentMainSchema.IM_ResolutionCode.Name, SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, loadedIncident.IM_ResolutionCode);

			var gprUpgrader = FindUpgrader(gprBuild, client1);
			var weeklyBuildUpgrader = FindUpgrader(weeklyBuild, client1);
			AssertNull("gprUpgrader", gprUpgrader);
			AssertNotNull("weeklyBuildUpgrader", weeklyBuildUpgrader);

			var first = loadedIncident.EConversation.GetTimeOrderedMessages().First();
			var last = loadedIncident.EConversation.GetTimeOrderedMessages().Last();
			AssertEquals("The next weekly release will contain changes to address your issue and will be delivered to your system when it becomes available.", first.Body);
			AssertHasEConversationMessage(@"The latest version has changes that address this eRequest and has been sent to your system.
Sent version: ediEnterprise 1.1.2083.0, GP Release 2005 Sep 14, Exe Date: 29-Nov-23 23:59.", loadedIncident.EConversation.ExistingConversation);
			AssertHasEConversationMessage("Closed As Upgrade Delivered", loadedIncident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", last.Body);

			AssertEquals(4, loadedIncident.EConversation.GetTimeOrderedMessages().Count);
			AssertReporterCounts(2, 0, 0);
		}

		[TestDate(2023, 11, 30)]
		public void TestCR4SendLatestBuildUpgradeForNotNCW_WhenInternalHostedSystem()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			var cr4 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			cr4.IM_Priority = "CR4";
			cr4.IM_Description = "cr4 incident";

			var licenceClient1Licence1GprDefectIncident = AttachNewClientAndNewLicence(cr4, "EC1", ReleaseRings.Codes.GPR);
			var client1 = (EDIOrgHeader)cr4.Client;

			AttachNewCompletedWorkItem(cr4, ReleaseRings.Codes.GPR, now.AddDays(-1));

			var client1Contact1 = CreateContact(client1, "contact1@client1.com");
			licenceClient1Licence1GprDefectIncident.Database.LD_OC_LicenseeAdminContact = client1Contact1.PK;
			var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.PK, licenceClient1Licence1GprDefectIncident.Database.LD_LE));
			licenceEnterprise.LE_IsInternal = true;
			Factory.Save();
			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			var weeklyBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddMinutes(-1));
			cr4.Database.LD_HostedLocation = "SYD";

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			process.RunTask();

			var newFactory = new BusinessObjectFactory();
			SupportIncident loadedIncident = newFactory.Load<SupportIncident>(cr4.PK);
			AssertEquals("cr4." + IncidentMainSchema.IM_ResolutionCode.Name, SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, loadedIncident.IM_ResolutionCode);

			var gprUpgrader = FindUpgrader(gprBuild, client1);
			var weeklyBuildUpgrader = FindUpgrader(weeklyBuild, client1);
			AssertNotNull("gprUpgrader", gprUpgrader);
			AssertNull("weeklyBuildUpgrader", weeklyBuildUpgrader);

			AssertHasEConversationMessage(@"The latest version has changes that address this eRequest and has been sent to your system.
Sent version: ediEnterprise 1.1.2083.0, GP Release 2005 Sep 14, Exe Date: 30-Nov-23 00:00.", loadedIncident.EConversation.ExistingConversation);
			AssertHasEConversationMessage("Closed As Upgrade Delivered", loadedIncident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", loadedIncident.EConversation.GetTimeOrderedMessages().Last().Body);

			AssertEquals(3, loadedIncident.EConversation.GetTimeOrderedMessages().Count);
			AssertReporterCounts(1, 0, 0);
		}

		[TestDate(2023, 11, 30)]
		public void TestSkipDeploy_CurrentProductIsCW1ButLatestBuildIsCWN_WhenCurrentRunningVersionIsNull()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			var cr4 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			cr4.IM_Priority = "CR4";
			cr4.IM_Description = "cr4 incident";

			var licenceClient1Licence1GprDefectIncident = AttachNewClientAndNewLicence(cr4, "EC1", ReleaseRings.Codes.GPR);
			var client1 = (EDIOrgHeader)cr4.Client;

			AttachNewCompletedWorkItem(cr4, ReleaseRings.Codes.GPR, now.AddDays(-1));

			var client1Contact1 = CreateContact(client1, "contact1@client1.com");
			licenceClient1Licence1GprDefectIncident.Database.LD_OC_LicenseeAdminContact = client1Contact1.PK;
			var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.PK, licenceClient1Licence1GprDefectIncident.Database.LD_LE));
			licenceEnterprise.LE_IsInternal = true;
			Factory.Save();
			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			gprBuild.HL_Product = ProductTypes.Codes.CargoWiseNext;
			var weeklyBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddMinutes(-1));
			weeklyBuild.HL_Product = ProductTypes.Codes.CargoWiseNext;
			cr4.Database.LD_HostedLocation = "SYD";

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			process.RunTask();

			var newFactory = new BusinessObjectFactory();
			SupportIncident loadedIncident = newFactory.Load<SupportIncident>(cr4.PK);

			AssertEquals("cr4." + IncidentMainSchema.IM_ResolutionCode.Name, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, loadedIncident.IM_ResolutionCode);
			AssertEquals("Information|CW1 systems cannot get CWN release build for incident CS00000001.", logger[1]);
		}

		[TestDate(2023, 11, 30)]
		public void TestSkipDeploy_CurrentProductIsCW1ButLatestBuildIsCWN_WhenCurrentRunningVersionIsNotNull()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			var cr4 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			cr4.IM_Priority = "CR4";
			cr4.IM_Description = "cr4 incident";

			var licenceClient1Licence1GprDefectIncident = AttachNewClientAndNewLicence(cr4, "EC1", ReleaseRings.Codes.GPR);
			var client1 = (EDIOrgHeader)cr4.Client;

			AttachNewCompletedWorkItem(cr4, ReleaseRings.Codes.GPR, now.AddDays(-1));

			var client1Contact1 = CreateContact(client1, "contact1@client1.com");
			licenceClient1Licence1GprDefectIncident.Database.LD_OC_LicenseeAdminContact = client1Contact1.PK;
			var licenceEnterprise = Factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.PK, licenceClient1Licence1GprDefectIncident.Database.LD_LE));
			licenceEnterprise.LE_IsInternal = true;
			Factory.Save();

			var currentBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddDays(-3));
			currentBuild.VersionNumber = new VersionNumber(23, 5, 16, 12);
			cr4.Database.LD_HL_CurrentRunningVersion = currentBuild.PK;

			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			gprBuild.VersionNumber = new VersionNumber(23, 5, 16, 20);
			gprBuild.HL_Product = ProductTypes.Codes.CargoWiseNext;

			var weeklyBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddDays(-7));
			weeklyBuild.VersionNumber = new VersionNumber(23, 5, 16, 10);
			weeklyBuild.HL_Product = ProductTypes.Codes.CargoWiseNext;
			cr4.Database.LD_HostedLocation = "SYD";

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			process.RunTask();

			var newFactory = new BusinessObjectFactory();
			SupportIncident loadedIncident = newFactory.Load<SupportIncident>(cr4.PK);

			AssertEquals("cr4." + IncidentMainSchema.IM_ResolutionCode.Name, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, loadedIncident.IM_ResolutionCode);
			AssertEquals("Information|CW1 systems cannot get CWN release build for incident CS00000001.", logger[1]);
		}

		[TestDate(2023, 7, 18)]
		public void TestCR4SendUpgradeForNCW()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			var cr4 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			cr4.IM_Priority = "CR4";
			cr4.IM_Description = "cr4 incident";

			var licenceClient1Licence1GprDefectIncident = AttachNewClientAndNewLicence(cr4, "EC1", ReleaseRings.Codes.GPR);
			var client1 = (EDIOrgHeader)cr4.Client;

			AttachNewCompletedWorkItem(cr4, ReleaseRings.Codes.GPR, now.AddDays(-1));

			var client1Contact1 = CreateContact(client1, "contact1@client1.com");
			licenceClient1Licence1GprDefectIncident.Database.LD_OC_LicenseeAdminContact = client1Contact1.PK;

			var currentBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddDays(-3));
			currentBuild.VersionNumber = new VersionNumber(23, 5, 16, 12);
			cr4.Database.LD_HL_CurrentRunningVersion = currentBuild.PK;
			cr4.Database.LD_HostedLocation = "NCW";

			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			gprBuild.VersionNumber = new VersionNumber(23, 5, 16, 20);

			var weeklyBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddDays(-7));
			weeklyBuild.VersionNumber = new VersionNumber(23, 5, 16, 10);

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			process.RunTask();

			var newFactory = new BusinessObjectFactory();
			SupportIncident loadedIncident = newFactory.Load<SupportIncident>(cr4.PK);
			AssertEquals("cr4." + IncidentMainSchema.IM_ResolutionCode.Name, SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, loadedIncident.IM_ResolutionCode);

			var gprUpgrader = FindUpgrader(gprBuild, client1);
			AssertNotNull("gprUpgrader", gprUpgrader);

			AssertIsProcessed("cr4", cr4, true, "Upgrade sent");
			AssertHasEConversationMessage(@"The latest version has changes that address this eRequest and has been sent to your system.
Sent version: CargoWise One 23.5.16.20, GP Release 2023 May 16 patch 20, Exe Date: 18-Jul-23 00:00.", loadedIncident.EConversation.ExistingConversation);
			AssertHasEConversationMessage("Closed As Upgrade Delivered", loadedIncident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", loadedIncident.EConversation.GetTimeOrderedMessages().Last().Body);
			AssertEquals(3, loadedIncident.EConversation.GetTimeOrderedMessages().Count);
			AssertReporterCounts(1, 0, 0);
		}

		[TestDate(2023, 5, 16)]
		public void TestCR4_AfterUpgrade()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			var cr4 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			cr4.IM_Priority = "CR4";
			cr4.IM_Description = "cr4 incident";

			var licenceClient1Licence1GprDefectIncident = AttachNewClientAndNewLicence(cr4, "EC1", ReleaseRings.Codes.GPR);
			var client1 = (EDIOrgHeader)cr4.Client;

			AttachNewCompletedWorkItem(cr4, ReleaseRings.Codes.GPR, now.AddDays(-1));
			cr4.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;

			var client1Contact1 = CreateContact(client1, "contact1@client1.com");
			licenceClient1Licence1GprDefectIncident.Database.LD_OC_LicenseeAdminContact = client1Contact1.PK;

			var currentBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddDays(-3));
			currentBuild.VersionNumber = new VersionNumber(23, 5, 16, 12);
			cr4.Database.LD_HL_CurrentRunningVersion = currentBuild.PK;
			cr4.Database.LD_HostedLocation = "SYD";

			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			gprBuild.VersionNumber = new VersionNumber(23, 5, 16, 20);

			var weeklyBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddDays(-7));
			weeklyBuild.VersionNumber = new VersionNumber(23, 5, 16, 10);

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			process.RunTask();

			var newFactory = new BusinessObjectFactory();
			SupportIncident loadedIncident = newFactory.Load<SupportIncident>(cr4.PK);
			AssertEquals("cr4." + IncidentMainSchema.IM_ResolutionCode.Name, SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed, loadedIncident.IM_ResolutionCode);

			var gprUpgrader = FindUpgrader(gprBuild, client1);
			AssertNull("gprUpgrader", gprUpgrader);

			AssertIsProcessed("cr4", cr4, true, "Upgrade has been delayed and will run on schedule");
			AssertEquals("The next weekly release will contain changes to address your issue and will be delivered to your system when it becomes available.", loadedIncident.EConversation.GetTimeOrderedMessages().Last().Body);

			//after upgrade
			cr4.Database.LD_HL_CurrentRunningVersion = gprBuild.PK;

			Factory.Save();

			process = new IncidentBasedAutoDeploymentServiceTaskForTest();
			logger = new TestServiceLogger();
			process.ServiceLogger = logger;

			process.RunTask();

			newFactory = new BusinessObjectFactory();
			loadedIncident = newFactory.Load<SupportIncident>(cr4.PK);

			AssertEquals("cr4." + IncidentMainSchema.IM_ResolutionCode.Name, SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, loadedIncident.IM_ResolutionCode);

			gprUpgrader = FindUpgrader(gprBuild, client1);
			AssertNull("gprUpgrader", gprUpgrader);

			AssertIsProcessed("cr4", cr4, true, "Upgrade not sent as the client is already on the same version");
			AssertHasEConversationMessage(@"No upgrade sent. Your system is already running a version with changes that address this eRequest.
Currently installed version: CargoWise One 23.5.16.20, GP Release 2023 May 16 patch 20, Exe Date: 16-May-23 00:00.", loadedIncident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", loadedIncident.EConversation.GetTimeOrderedMessages().Last().Body);
			AssertReporterCounts(1, 0, 0);
		}

		[TestDate(2006, 6, 7)]
		public void TestNeedUpgrade()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			var client1Licence1GprDefectIncident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var client1Licence2GprFeatureIncident = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var client1Licence3AlphaFeatureIncident = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var client2StdDefectIncident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var client2StdFeatureIncident = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var client3GprDefectIncident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);

			client1Licence1GprDefectIncident.IM_Description = "client1Licence1GprDefectIncident";
			client1Licence2GprFeatureIncident.IM_Description = "client1Licence2GprFeatureIncident";
			client1Licence3AlphaFeatureIncident.IM_Description = "client1Licence3AlphaFeatureIncident";
			client2StdDefectIncident.IM_Description = "client2StdDefectIncident";
			client2StdFeatureIncident.IM_Description = "client2StdFeatureIncident";
			client3GprDefectIncident.IM_Description = "client3GprDefectIncident";

			var licenceClient1Licence1GprDefectIncident = AttachNewClientAndNewLicence(client1Licence1GprDefectIncident, "EC1", ReleaseRings.Codes.GPR);
			var client1 = (EDIOrgHeader)client1Licence1GprDefectIncident.Client;
			var licenceClient1Licence2GprFeatureIncident = AttachClientAndNewLicence(client1Licence2GprFeatureIncident, client1, ReleaseRings.Codes.GPR);
			var licenceClient1Licence3AlphaFeatureIncident = AttachClientAndNewLicence(client1Licence3AlphaFeatureIncident, client1, ReleaseRings.Codes.ALP);

			var licenceClient2StdDefectIncident = AttachNewClientAndNewLicence(client2StdDefectIncident, "EC2", ReleaseRings.Codes.STD);
			var client2 = (EDIOrgHeader)client2StdDefectIncident.Client;
			AttachClientAndCompany(client2StdFeatureIncident, client2, client2StdDefectIncident.ClientCompany);

			var licenceClient3GprDefectIncident = AttachNewClientAndNewLicence(client3GprDefectIncident, "EC3", ReleaseRings.Codes.GPR);

			AttachNewCompletedWorkItem(client1Licence1GprDefectIncident, ReleaseRings.Codes.GPR, now.AddDays(-1));
			AttachNewCompletedWorkItem(client1Licence2GprFeatureIncident, ReleaseRings.Codes.GPR, now.AddDays(-1));
			AttachNewCompletedWorkItem(client1Licence3AlphaFeatureIncident, ReleaseRings.Codes.ALP, now.AddDays(-1));
			AttachNewCompletedWorkItem(client2StdDefectIncident, ReleaseRings.Codes.STD, now.AddDays(-1));
			AttachNewCompletedWorkItem(client2StdFeatureIncident, ReleaseRings.Codes.STD, now.AddDays(-1));
			AttachNewCompletedWorkItem(client3GprDefectIncident, ReleaseRings.Codes.GPR, now.AddDays(-1));

			var client1Contact1 = CreateContact(client1, "contact1@client1.com");
			var client1Contact2 = CreateContact(client1, "contact2@client1.com");
			var client1Contact3 = CreateContact(client1, "contact3@client1.com");
			var client2Contact = CreateContact(client2, "contact@client2.com");
			var client3Contact = CreateContact(client3GprDefectIncident.Client, "contact@client3.com");

			licenceClient1Licence1GprDefectIncident.Database.LD_OC_LicenseeAdminContact = client1Contact1.PK;
			licenceClient1Licence2GprFeatureIncident.Database.LD_OC_LicenseeAdminContact = client1Contact2.PK;
			client1Licence3AlphaFeatureIncident.IM_OC_Contact = client1Contact3.PK;
			licenceClient2StdDefectIncident.Database.LD_OC_ContractInstallerOrInternalTechContact = client2Contact.PK;
			licenceClient3GprDefectIncident.Database.LD_OC_ContractInstallerOrInternalTechContact = client3Contact.PK;

			var alphaBuild = CreateReleaseBuild(ReleaseRings.Codes.ALP, now);
			var dprBuild = CreateReleaseBuild(ReleaseRings.Codes.DPR, now);
			var stdBuild = CreateReleaseBuild(ReleaseRings.Codes.STD, now);
			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);

			licenceClient3GprDefectIncident.Database.LD_HL_CurrentRunningVersion = gprBuild.PK;
			client3GprDefectIncident.Quote.CIQ_DeliveredDateUTC = new ZDateTime(2006, 6, 1);

			// force creation of quote
			client1Licence1GprDefectIncident.Quote.CIQ_DeliveredDateUTC = ZDateTime.Empty;
			client1Licence2GprFeatureIncident.Quote.CIQ_DeliveredDateUTC = ZDateTime.Empty;
			client1Licence3AlphaFeatureIncident.Quote.CIQ_DeliveredDateUTC = ZDateTime.Empty;

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			process.AddNeedUpgrade(client1Licence1GprDefectIncident.PK, false);
			process.AddNeedUpgrade(client1Licence3AlphaFeatureIncident.PK, false);
			process.AddNeedUpgrade(client2StdFeatureIncident.PK, false);

			process.RunTask();

			var newFactory = new BusinessObjectFactory();
			AssertIncidentPostUpgradeStatus("client1Licence2GprFeatureIncident", now, newFactory, client1Licence2GprFeatureIncident);
			AssertIncidentPostUpgradeStatus("client2StdDefectIncident", now, newFactory, client2StdDefectIncident, false);

			var loadedClient3GprDefectIncident = newFactory.Load<SupportIncident>(client3GprDefectIncident.PK);
			AssertEquals("Client3GprDefectIncident." + IncidentMainSchema.IM_ResolutionCode.Name, SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, loadedClient3GprDefectIncident.IM_ResolutionCode);
			AssertEquals("Client3GprDefectIncident.IM_BugFixedDeployed", now, client3GprDefectIncident.IM_BugFixDeployed);
			AssertEquals("Client3GprDefectIncident.CIQ_DeliveredDateUTC", new ZDateTime(2006, 6, 1), client3GprDefectIncident.Quote.CIQ_DeliveredDateUTC);

			var loadedClient1Licence1GprDefectIncident = newFactory.Load<SupportIncident>(client3GprDefectIncident.PK);
			AssertEquals("Client1Licence1GprDefectIncident." + IncidentMainSchema.IM_ResolutionCode.Name, SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, loadedClient1Licence1GprDefectIncident.IM_ResolutionCode);
			AssertEquals("Client1Licence1GprDefectIncident.IM_BugFixedDeployed", now, client3GprDefectIncident.IM_BugFixDeployed);
			AssertEquals("Client1Licence1GprDefectIncident.CIQ_DeliveredDateUTC", new ZDateTime(2006, 6, 1), client3GprDefectIncident.Quote.CIQ_DeliveredDateUTC);

			var loadedClient1Licence3AlphaFeatureIncident = newFactory.Load<SupportIncident>(client3GprDefectIncident.PK);
			AssertEquals("Client1Licence3AlphaFeatureIncident." + IncidentMainSchema.IM_ResolutionCode.Name, SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, loadedClient1Licence3AlphaFeatureIncident.IM_ResolutionCode);
			AssertEquals("Client1Licence3AlphaFeatureIncident.IM_BugFixedDeployed", now, client3GprDefectIncident.IM_BugFixDeployed);
			AssertEquals("Client1Licence3AlphaFeatureIncident.CIQ_DeliveredDateUTC", new ZDateTime(2006, 6, 1), client3GprDefectIncident.Quote.CIQ_DeliveredDateUTC);

			var loadedClient2StdFeatureIncident = newFactory.Load<SupportIncident>(client3GprDefectIncident.PK);
			AssertEquals("Client2StdFeatureIncident." + IncidentMainSchema.IM_ResolutionCode.Name, SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, loadedClient3GprDefectIncident.IM_ResolutionCode);
			AssertEquals("Client2StdFeatureIncident.IM_BugFixedDeployed", now, client3GprDefectIncident.IM_BugFixDeployed);
			AssertEquals("Client2StdFeatureIncident.CIQ_DeliveredDateUTC", new ZDateTime(2006, 6, 1), loadedClient2StdFeatureIncident.Quote.CIQ_DeliveredDateUTC);

			AssertEquals("createdUpgraders.Count", 2, process.createdUpgraders.Count);
			var stdUpgrader = FindUpgrader(stdBuild, client2);
			var gprUpgrader = FindUpgrader(gprBuild, client1);

			AssertUpgrader("stdUpgrader", stdUpgrader, client2StdDefectIncident);
			AssertUpgrader("gprUpgrader", gprUpgrader, client1Licence2GprFeatureIncident);

			AssertEquals("stdUpgrader.Upgrades.Count", 1, stdUpgrader.Upgrades.Count);
			AssertUpgradeRequest("stdUpgrader.Upgrades[0]", stdUpgrader.Upgrades[0], client2StdDefectIncident, client2Contact);

			AssertEquals("gprUpgrader.Upgrades.Count", 1, gprUpgrader.Upgrades.Count);
			AssertUpgradeRequest("gprUpgrader.Upgrades[0]", FindUpgradeRequest(gprUpgrader, client1Licence2GprFeatureIncident), client1Licence2GprFeatureIncident, client1Contact2);

			AssertEquals("Logger.DebugLogStrings[0].Trim()", "Information|Processing incidents...", logger[0].Trim());
			AssertEquals("Logger.DebugLogStrings[1].Trim()", "Information|Incidents processed: 6", logger[1].Trim());
			AssertEquals("Emails Created", 3, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Email Body", process.Reporter.GetReportBody(), Env.OutgoingMailManager.EmailsCreated[2].Body);

			AssertReporterCounts(6, 0, 0);
			AssertIsProcessed("client1Licence1GprDefectIncident", client1Licence1GprDefectIncident, true, "Upgrade not required");
			AssertIsProcessed("client1Licence2GprFeatureIncident", client1Licence2GprFeatureIncident, true, "Upgrade sent");
			AssertIsProcessed("client1Licence3AlphaFeatureIncident", client1Licence3AlphaFeatureIncident, true, "Upgrade not required");
			AssertIsProcessed("client2StdDefectIncident", client2StdDefectIncident, true, "Upgrade sent");
			AssertIsProcessed("client2StdFeatureIncident", client2StdFeatureIncident, true, "Upgrade not required");
			AssertIsProcessed("client3GprDefectIncident", client3GprDefectIncident, true, "Upgrade not sent as the client is already on the same version");

			AssertHasEConversationMessage(@"No upgrade sent. Your system is already running a version with changes that address this eRequest.
Currently installed version: ediEnterprise 1.1.2083.0, GP Release 2005 Sep 14, Exe Date: 07-Jun-06 00:00.", loadedClient3GprDefectIncident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", loadedClient3GprDefectIncident.EConversation.GetTimeOrderedMessages().Last().Body);
		}

		[TestDate(2023, 5, 16)]
		public void TestGetDelayedIncidentsToSendWeeklyUpgrade_CR3()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;
			enterprise.LE_EnterpriseCode = "EC1";

			var database = CreateDatabase(enterprise, "SC1", ProductTypes.Codes.Enterprise, ReleaseRings.Codes.GPR, DatabaseTypes.Codes.Test, "SYD");
			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			var weeklyBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddMinutes(-1));
			var cr3Incident = CreateCR3Incident(now, database);
			cr3Incident.Database.LD_HostedLocation = "SYD";
			Factory.Save();

			process.RunTask();
			AssertEquals("createdUpgraders.Count", 1, process.createdUpgraders.Count);
			AssertEquals("the disposition of cr3Incident should be UpgradeDelivered after upgrade", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, cr3Incident.IM_ResolutionCode);
			AssertHasEConversationMessage(@"The latest version has changes that address this eRequest and has been sent to your system.
Sent version: ediEnterprise 1.1.2083.0, GP Release 2005 Sep 14, Exe Date: 16-May-23 00:00.", cr3Incident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", cr3Incident.EConversation.GetTimeOrderedMessages().Last().Body);
		}

		[TestDate(2023, 5, 16)]
		public void TestGetDelayedIncidentsToSendWeeklyUpgrade_CR4_WeeklyBuildIsReady()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;
			enterprise.LE_EnterpriseCode = "EC1";

			var database = CreateDatabase(enterprise, "SC1", ProductTypes.Codes.Enterprise, ReleaseRings.Codes.GPR, DatabaseTypes.Codes.Test, "SYD");
			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			var weeklyBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddMinutes(-1));
			var cr4Incident = CreateCR4Incident(now, database);
			cr4Incident.Database.LD_HostedLocation = "SYD";
			Factory.Save();

			process.RunTask();
			AssertEquals("createdUpgraders.Count", 1, process.createdUpgraders.Count);
			AssertEquals("the disposition of cr4Incident should be UpgradeDelivered after upgrade", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, cr4Incident.IM_ResolutionCode);
			AssertHasEConversationMessage(@"The latest version has changes that address this eRequest and has been sent to your system.
Sent version: ediEnterprise 1.1.2083.0, GP Release 2005 Sep 14, Exe Date: 15-May-23 23:59.", cr4Incident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", cr4Incident.EConversation.GetTimeOrderedMessages().Last().Body);
		}

		[TestDate(2023, 5, 16)]
		public void TestGetDelayedIncidentsToSendWeeklyUpgrade_CR4_WeeklyBuildIsNotReady()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;
			enterprise.LE_EnterpriseCode = "EC1";

			var database = CreateDatabase(enterprise, "SC1", ProductTypes.Codes.Enterprise, ReleaseRings.Codes.GPR, DatabaseTypes.Codes.Test, "SYD");
			var currentBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddDays(-3));
			currentBuild.VersionNumber = new VersionNumber(23, 5, 16, 12);
			database.LD_HL_CurrentRunningVersion = currentBuild.PK;

			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			gprBuild.VersionNumber = new VersionNumber(23, 5, 16, 20);

			var weeklyBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddDays(-7));
			weeklyBuild.VersionNumber = new VersionNumber(23, 5, 16, 10);

			var cr4Incident = CreateCR4Incident(now, database);
			cr4Incident.Database.LD_HostedLocation = "SYD";

			Factory.Save();

			process.RunTask();
			AssertNull("createdUpgraders should be null", process.createdUpgraders);
			AssertEquals("the disposition of cr4Incident should be UpgradeDelayed after upgrade", SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed, cr4Incident.IM_ResolutionCode);
			AssertEquals("The next weekly release will contain changes to address your issue and will be delivered to your system when it becomes available.", cr4Incident.EConversation.GetTimeOrderedMessages().Last().Body);
		}

		[TestDate(2023, 5, 16)]
		public void TestGetDelayedIncidentsToSendWeeklyUpgrade_CR3AndCR4_WeeklyBuildIsReady()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;
			enterprise.LE_EnterpriseCode = "EC1";

			var database = CreateDatabase(enterprise, "SC1", ProductTypes.Codes.Enterprise, ReleaseRings.Codes.GPR, DatabaseTypes.Codes.Test, "SYD");
			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			var weeklyBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddMinutes(-1));
			var cr3Incident = CreateCR3Incident(now, database);
			var cr4Incident = CreateCR4Incident(now, database);
			cr4Incident.Database.LD_HostedLocation = "SYD";
			Factory.Save();

			process.RunTask();
			AssertEquals("createdUpgraders.Count", 2, process.createdUpgraders.Count);
			AssertEquals("the disposition of cr3Incident should be UpgradeDelivered after upgrade", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, cr3Incident.IM_ResolutionCode);
			AssertHasEConversationMessage(@"The latest version has changes that address this eRequest and has been sent to your system.
Sent version: ediEnterprise 1.1.2083.0, GP Release 2005 Sep 14, Exe Date: 16-May-23 00:00.", cr3Incident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", cr3Incident.EConversation.GetTimeOrderedMessages().Last().Body);

			AssertEquals("the disposition of cr4Incident should be UpgradeDelivered after upgrade", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, cr4Incident.IM_ResolutionCode);
			AssertHasEConversationMessage(@"The latest version has changes that address this eRequest and has been sent to your system.
Sent version: ediEnterprise 1.1.2083.0, GP Release 2005 Sep 14, Exe Date: 15-May-23 23:59.", cr4Incident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", cr4Incident.EConversation.GetTimeOrderedMessages().Last().Body);
		}

		[TestDate(2023, 5, 16)]
		public void TestGetDelayedIncidentsToSendWeeklyUpgrade_CR3AndCR4_WeeklyBuildWillReady()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;
			enterprise.LE_EnterpriseCode = "EC1";

			var database = CreateDatabase(enterprise, "SC1", ProductTypes.Codes.Enterprise, ReleaseRings.Codes.GPR, DatabaseTypes.Codes.Test, "SYD");
			var currentBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddDays(-3));
			currentBuild.VersionNumber = new VersionNumber(23, 5, 16, 12);
			database.LD_HL_CurrentRunningVersion = currentBuild.PK;

			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			gprBuild.VersionNumber = new VersionNumber(23, 5, 16, 20);

			var weeklyBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddDays(-7));
			weeklyBuild.VersionNumber = new VersionNumber(23, 5, 16, 10);

			var cr3Incident = CreateCR3Incident(now, database);
			cr3Incident.Database.LD_HostedLocation = "SYD";
			var cr4Incident = CreateCR4Incident(now, database);
			cr4Incident.Database.LD_HostedLocation = "SYD";

			Factory.Save();

			process.RunTask();
			AssertEquals("createdUpgraders.Count", 1, process.createdUpgraders.Count);
			AssertEquals("the disposition of cr3Incident should be UpgradeDelivered after upgrade", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, cr3Incident.IM_ResolutionCode);
			AssertHasEConversationMessage(@"The latest version has changes that address this eRequest and has been sent to your system.
Sent version: CargoWise One 23.5.16.20, GP Release 2023 May 16 patch 20, Exe Date: 16-May-23 00:00.", cr3Incident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", cr3Incident.EConversation.GetTimeOrderedMessages().Last().Body);
			AssertEquals("the disposition of cr4Incident should be UpgradeDelayed after upgrade", SupportIncidentLookups.DispositionList.Constants.Closed.UpgradeDelayed, cr4Incident.IM_ResolutionCode);
			AssertEquals("The next weekly release will contain changes to address your issue and will be delivered to your system when it becomes available.", cr4Incident.EConversation.GetTimeOrderedMessages().Last().Body);

			weeklyBuild.VersionNumber = new VersionNumber(23, 5, 16, 18);
			weeklyBuild.HL_ExeVersionDate = now.AddDays(-1);
			Factory.Save();

			process = new IncidentBasedAutoDeploymentServiceTaskForTest();
			process.ServiceLogger = new TestServiceLogger();
			process.RunTask();
			AssertEquals("createdUpgraders.Count", 1, process.createdUpgraders.Count);
			AssertEquals("the disposition of cr4Incident should be UpgradeDelivered after upgrade", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, cr4Incident.IM_ResolutionCode);
			AssertHasEConversationMessage(@"The latest version has changes that address this eRequest and has been sent to your system.
Sent version: CargoWise One 23.5.16.18, GP Release 2023 May 16 patch 18, Exe Date: 15-May-23 00:00.", cr4Incident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", cr4Incident.EConversation.GetTimeOrderedMessages().Last().Body);
		}

		SupportIncident CreateCR3Incident(ZDateTime now, LicenceDatabase database)
		{
			var cr3Incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, IncidentMainLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			cr3Incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			cr3Incident.IM_Description = "CR3 incident";
			cr3Incident.IM_LD = database.PK;
			AttachNewClientAndNewLicence(cr3Incident, "EC1", ReleaseRings.Codes.GPR);
			AttachNewCompletedWorkItem(cr3Incident, ReleaseRings.Codes.GPR, now.AddDays(-1));
			return cr3Incident;
		}

		SupportIncident CreateCR4Incident(ZDateTime now, LicenceDatabase database)
		{
			var cr4Incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, IncidentMainLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			cr4Incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			cr4Incident.IM_Description = "CR4 incident";
			cr4Incident.IM_LD = database.PK;
			AttachNewClientAndNewLicence(cr4Incident, "EC1", ReleaseRings.Codes.GPR);
			AttachNewCompletedWorkItem(cr4Incident, ReleaseRings.Codes.GPR, now.AddDays(-1));
			return cr4Incident;
		}

		#endregion

		[TestDate(2006, 6, 7)]
		public void TestExecute()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			var client1Licence1GprDefectIncident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var client1Licence2GprFeatureIncident = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var client1Licence3AlphaFeatureIncident = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var client2StdDefectIncident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var client2StdFeatureIncident = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var client3GprDefectIncident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);

			client1Licence1GprDefectIncident.IM_Description = "client1Licence1GprDefectIncident";
			client1Licence2GprFeatureIncident.IM_Description = "client1Licence2GprFeatureIncident";
			client1Licence3AlphaFeatureIncident.IM_Description = "client1Licence3AlphaFeatureIncident";
			client2StdDefectIncident.IM_Description = "client2StdDefectIncident";
			client2StdFeatureIncident.IM_Description = "client2StdFeatureIncident";
			client3GprDefectIncident.IM_Description = "client3GprDefectIncident";

			var licenceClient1Licence1GprDefectIncident = AttachNewClientAndNewLicence(client1Licence1GprDefectIncident, "EC1", ReleaseRings.Codes.GPR);
			var client1 = (EDIOrgHeader)client1Licence1GprDefectIncident.Client;
			var licenceClient1Licence2GprFeatureIncident = AttachClientAndNewLicence(client1Licence2GprFeatureIncident, client1, ReleaseRings.Codes.GPR);
			var licenceClient1Licence3AlphaFeatureIncident = AttachClientAndNewLicence(client1Licence3AlphaFeatureIncident, client1, ReleaseRings.Codes.ALP);

			var licenceClient2StdDefectIncident = AttachNewClientAndNewLicence(client2StdDefectIncident, "EC2", ReleaseRings.Codes.STD);
			var client2 = (EDIOrgHeader)client2StdDefectIncident.Client;
			AttachClientAndCompany(client2StdFeatureIncident, client2, client2StdDefectIncident.ClientCompany);

			var licenceClient3GprDefectIncident = AttachNewClientAndNewLicence(client3GprDefectIncident, "EC3", ReleaseRings.Codes.GPR);

			AttachNewCompletedWorkItem(client1Licence1GprDefectIncident, ReleaseRings.Codes.GPR, now.AddDays(-1));
			AttachNewCompletedWorkItem(client1Licence2GprFeatureIncident, ReleaseRings.Codes.GPR, now.AddDays(-1));
			AttachNewCompletedWorkItem(client1Licence3AlphaFeatureIncident, ReleaseRings.Codes.ALP, now.AddDays(-1));
			AttachNewCompletedWorkItem(client2StdDefectIncident, ReleaseRings.Codes.STD, now.AddDays(-1));
			AttachNewCompletedWorkItem(client2StdFeatureIncident, ReleaseRings.Codes.STD, now.AddDays(-1));
			AttachNewCompletedWorkItem(client3GprDefectIncident, ReleaseRings.Codes.GPR, now.AddDays(-1));

			var client1Contact1 = CreateContact(client1, "contact1@client1.com");
			var client1Contact2 = CreateContact(client1, "contact2@client1.com");
			var client1Contact3 = CreateContact(client1, "contact3@client1.com");
			var client2Contact = CreateContact(client2, "contact@client2.com");
			var client3Contact = CreateContact(client3GprDefectIncident.Client, "contact@client3.com");

			licenceClient1Licence1GprDefectIncident.Database.LD_OC_LicenseeAdminContact = client1Contact1.PK;
			licenceClient1Licence2GprFeatureIncident.Database.LD_OC_LicenseeAdminContact = client1Contact2.PK;
			client1Licence3AlphaFeatureIncident.IM_OC_Contact = client1Contact3.PK;
			licenceClient2StdDefectIncident.Database.LD_OC_ContractInstallerOrInternalTechContact = client2Contact.PK;
			licenceClient3GprDefectIncident.Database.LD_OC_ContractInstallerOrInternalTechContact = client3Contact.PK;

			var alphaBuild = CreateReleaseBuild(ReleaseRings.Codes.ALP, now);
			var dprBuild = CreateReleaseBuild(ReleaseRings.Codes.DPR, now);
			var stdBuild = CreateReleaseBuild(ReleaseRings.Codes.STD, now);
			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);

			licenceClient3GprDefectIncident.Database.LD_HL_CurrentRunningVersion = gprBuild.PK;
			client3GprDefectIncident.Quote.CIQ_DeliveredDateUTC = new ZDateTime(2006, 6, 1);

			// force creation of quote
			client1Licence1GprDefectIncident.Quote.CIQ_DeliveredDateUTC = ZDateTime.Empty;
			client1Licence2GprFeatureIncident.Quote.CIQ_DeliveredDateUTC = ZDateTime.Empty;
			client1Licence3AlphaFeatureIncident.Quote.CIQ_DeliveredDateUTC = ZDateTime.Empty;

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			process.RunTask();

			var newFactory = new BusinessObjectFactory();
			AssertIncidentPostUpgradeStatus("client1Licence1GprDefectIncident", now, newFactory, client1Licence1GprDefectIncident);
			AssertIncidentPostUpgradeStatus("client1Licence2GprFeatureIncident", now, newFactory, client1Licence2GprFeatureIncident);
			AssertIncidentPostUpgradeStatus("client1Licence3AlphaFeatureIncident", now, newFactory, client1Licence3AlphaFeatureIncident);
			AssertIncidentPostUpgradeStatus("client2StdDefectIncident", now, newFactory, client2StdDefectIncident, false);
			AssertIncidentPostUpgradeStatus("client2StdFeatureIncident", now, newFactory, client2StdFeatureIncident);

			var loadedIncident = newFactory.Load<SupportIncident>(client3GprDefectIncident.PK);
			AssertEquals("client3GprDefectIncident." + IncidentMainSchema.IM_ResolutionCode.Name, SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, loadedIncident.IM_ResolutionCode);
			AssertEquals("client3GprDefectIncident.IM_BugFixedDeployed", now, client3GprDefectIncident.IM_BugFixDeployed);
			AssertEquals("client3GprDefectIncident.CIQ_DeliveredDateUTC", new ZDateTime(2006, 6, 1), client3GprDefectIncident.Quote.CIQ_DeliveredDateUTC);

			AssertEquals("createdUpgraders.Count", 3, process.createdUpgraders.Count);
			var alphaUpgrader = FindUpgrader(alphaBuild, client1);
			var stdUpgrader = FindUpgrader(stdBuild, client2);
			var gprUpgrader = FindUpgrader(gprBuild, client1);

			AssertUpgrader("alphaUpgrader", alphaUpgrader, client1Licence3AlphaFeatureIncident);
			AssertUpgrader("stdUpgrader", stdUpgrader, client2StdDefectIncident, client2StdFeatureIncident);
			AssertUpgrader("gprUpgrader", gprUpgrader, client1Licence1GprDefectIncident, client1Licence2GprFeatureIncident);

			AssertEquals("alphaUpgrader.Upgrades.Count", 1, alphaUpgrader.Upgrades.Count);
			AssertUpgradeRequest("alphaUpgrader.Upgrades[0]", alphaUpgrader.Upgrades[0], client1Licence3AlphaFeatureIncident, client1Contact3);

			AssertEquals("stdUpgrader.Upgrades.Count", 1, stdUpgrader.Upgrades.Count);
			AssertUpgradeRequest("stdUpgrader.Upgrades[0]", stdUpgrader.Upgrades[0], client2StdDefectIncident, client2Contact);

			AssertEquals("gprUpgrader.Upgrades.Count", 2, gprUpgrader.Upgrades.Count);
			AssertUpgradeRequest("gprUpgrader.Upgrades[0]", FindUpgradeRequest(gprUpgrader, client1Licence1GprDefectIncident), client1Licence1GprDefectIncident, client1Contact1);
			AssertUpgradeRequest("gprUpgrader.Upgrades[1]", FindUpgradeRequest(gprUpgrader, client1Licence2GprFeatureIncident), client1Licence2GprFeatureIncident, client1Contact2);

			AssertEquals("Logger.DebugLogStrings[0].Trim()", "Information|Processing incidents...", logger[0].Trim());
			AssertEquals("Logger.DebugLogStrings[1].Trim()", "Information|Incidents processed: 6", logger[1].Trim());
			AssertEquals("Emails Created", 4, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Email Body", process.Reporter.GetReportBody(), Env.OutgoingMailManager.EmailsCreated[3].Body);

			AssertReporterCounts(6, 0, 0);
			AssertIsProcessed("client1Licence1GprDefectIncident", client1Licence1GprDefectIncident, true, "Upgrade sent");
			AssertIsProcessed("client1Licence2GprFeatureIncident", client1Licence2GprFeatureIncident, true, "Upgrade sent");
			AssertIsProcessed("client1Licence3AlphaFeatureIncident", client1Licence3AlphaFeatureIncident, true, "Upgrade sent");
			AssertIsProcessed("client2StdDefectIncident", client2StdDefectIncident, true, "Upgrade sent");
			AssertIsProcessed("client2StdFeatureIncident", client2StdFeatureIncident, true, "Upgrade sent");
			AssertIsProcessed("client3GprDefectIncident", client3GprDefectIncident, true, "Upgrade not sent as the client is already on the same version");

			AssertHasEConversationMessage(@"No upgrade sent. Your system is already running a version with changes that address this eRequest.
Currently installed version: ediEnterprise 1.1.2083.0, GP Release 2005 Sep 14, Exe Date: 07-Jun-06 00:00.", loadedIncident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", loadedIncident.EConversation.GetTimeOrderedMessages().Last().Body);
		}

		[TestDate(2006, 6, 7)]
		public void TestExecute_DeploysGprIfClientIsOnGpcButIsUnavailable()
		{
			ZDateTime now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			SupportIncident clientLicenceGpcDefectIncident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			clientLicenceGpcDefectIncident.IM_Description = "clientLicence1GpcDefectIncident";

			var licence = AttachNewClientAndNewLicence(clientLicenceGpcDefectIncident, "EC1", ReleaseRings.Codes.GPC);
			AttachNewCompletedWorkItem(clientLicenceGpcDefectIncident, ReleaseRings.Codes.GPR, now.AddDays(-1));

			EDIOrgHeader client = (EDIOrgHeader)clientLicenceGpcDefectIncident.Client;
			OrgContact clientContact = CreateContact(client, "contact1@client.com");
			licence.Database.LD_OC_LicenseeAdminContact = clientContact.PK;

			ReleaseBuild gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			gprBuild.HL_MajorVersion = 1;
			gprBuild.HL_MinorVersion = 4;
			gprBuild.HL_Release = 3500;
			gprBuild.HL_Patch = 100;
			Factory.Save();

			// Test when no gpc build exists
			AssertEquals("Precondition: process.Builds.GpcReleaseBuildIsAvailable", false, process.Builds.GpcReleaseBuildIsAvailable);

			process.RunTask();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertIncidentPostUpgradeStatus("clientLicence1GpcDefectIncident", now, newFactory, clientLicenceGpcDefectIncident, false);

			AssertEquals("createdUpgraders.Count", 1, process.createdUpgraders.Count);
			UpgradeRequestCollectionContainer gprUpgrader = FindUpgrader(gprBuild, client);
			AssertUpgrader("gprUpgrader", gprUpgrader, clientLicenceGpcDefectIncident);
			AssertEquals("gprUpgrader.Upgrades.Count", 1, gprUpgrader.Upgrades.Count);
			AssertUpgradeRequest("gprUpgrader.Upgrades[0]", FindUpgradeRequest(gprUpgrader, clientLicenceGpcDefectIncident), clientLicenceGpcDefectIncident, clientContact);

			AssertReporterCounts(1, 0, 0);
			AssertIsProcessed("client1Licence1GprDefectIncident", clientLicenceGpcDefectIncident, true, "Upgrade sent");

			// Test when gpc build is older than gpc
			ReleaseBuild gpcBuild = CreateReleaseBuild(ReleaseRings.Codes.GPC, now);
			gpcBuild.HL_MajorVersion = gprBuild.HL_MajorVersion;
			gpcBuild.HL_MinorVersion = gprBuild.HL_MinorVersion;
			gpcBuild.HL_Release = 3000;
			gpcBuild.HL_Patch = 100;
			Factory.Save();

			AssertEquals("Precondition: process.Builds.GpcReleaseBuildIsAvailable", false, process.Builds.GpcReleaseBuildIsAvailable);

			process.RunTask();

			newFactory = new BusinessObjectFactory();
			AssertIncidentPostUpgradeStatus("clientLicence1GpcDefectIncident", now, newFactory, clientLicenceGpcDefectIncident, false);

			AssertEquals("createdUpgraders.Count", 1, process.createdUpgraders.Count);
			AssertEquals("gpcUpgrader", null, FindUpgrader(gpcBuild, client));
			gprUpgrader = FindUpgrader(gprBuild, client);
			AssertUpgrader("gprUpgrader", gprUpgrader, clientLicenceGpcDefectIncident);
			AssertEquals("gprUpgrader.Upgrades.Count", 1, gprUpgrader.Upgrades.Count);
			AssertUpgradeRequest("gprUpgrader.Upgrades[0]", FindUpgradeRequest(gprUpgrader, clientLicenceGpcDefectIncident), clientLicenceGpcDefectIncident, clientContact);

			AssertReporterCounts(1, 0, 0);
			AssertIsProcessed("client1Licence1GprDefectIncident", clientLicenceGpcDefectIncident, true, "Upgrade sent");
		}

		[TestDate(2006, 6, 7)]
		public void TestExecute_DoesNotSendToSameDatabaseMoreThanOnce()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();

			var client1Incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var client2Incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);

			client1Incident.IM_Description = "client1Incident";
			client2Incident.IM_Description = "client2Incident";
			client1Incident.Quote.CIQ_DeliveredDateUTC = ZDateTime.Empty;
			client2Incident.Quote.CIQ_DeliveredDateUTC = ZDateTime.Empty;

			var licence1 = AttachNewClientAndNewLicence(client1Incident, "EC1", ReleaseRings.Codes.GPR);
			var licence2 = AttachNewClientAndNewLicence(client2Incident, "EC2", ReleaseRings.Codes.GPR);
			licence1.ClientCompany.LCC_Code = "AAA";
			licence2.ClientCompany.LCC_Code = "BBB";
			licence1.ClientCompany.LCC_LD = licence2.ClientCompany.LCC_LD;
			client1Incident.IM_LD = client2Incident.IM_LD;

			AttachNewCompletedWorkItem(client1Incident, ReleaseRings.Codes.GPR, now.AddDays(-1));
			AttachNewCompletedWorkItem(client2Incident, ReleaseRings.Codes.GPR, now.AddDays(-1));

			var client1Contact = CreateContact(client1Incident.Client, "contact1@client1.com");
			var client2Contact = CreateContact(client2Incident.Client, "contact@client2.com");

			licence1.Database.LD_OC_LicenseeAdminContact = client1Contact.PK;
			client2Incident.IM_OC_Contact = client2Contact.PK;
			client1Incident.IM_OC_Contact = client1Contact.PK;

			CreateReleaseBuild(ReleaseRings.Codes.GPR, now);

			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			process.RunTask();

			var newFactory = new BusinessObjectFactory();
			AssertIncidentPostUpgradeStatus("client1Incident", now, newFactory, client1Incident);
			AssertIncidentPostUpgradeStatus("client2Incident", now, newFactory, client2Incident);

			AssertEquals("createdUpgraders.Count", 1, process.createdUpgraders.Count);

			var upgrader = process.createdUpgraders[0];
			AssertEquals("upgrader.Upgrades.Count", 1, upgrader.Upgrades.Count);
			AssertUpgrader("upgrader", upgrader, new SupportIncident[] { client1Incident, client2Incident });

			AssertEquals("Logger.DebugLogStrings[0].Trim()", "Information|Processing incidents...", logger[0].Trim());
			AssertEquals("Logger.DebugLogStrings[1].Trim()", "Information|Incidents processed: 2", logger[1].Trim());
			AssertEquals("Emails Created", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Email Body", process.Reporter.GetReportBody(), Env.OutgoingMailManager.EmailsCreated[1].Body);

			AssertReporterCounts(2, 0, 0);
			AssertIsProcessed("client1Incident", client1Incident, true, "Upgrade sent");
			AssertIsProcessed("client2Incident", client2Incident, true, "Upgrade sent");
		}

		[TestDate(2016, 10, 10)]
		public void TestExecute_ElevatedRing()
		{
			SetUpEnvironmentForExecute();

			// client has DPR version
			var dprBuild = CreateReleaseBuild(ReleaseRings.Codes.DPR, new ZDateTime(2016, 8, 8));
			dprBuild.VersionNumber = new VersionNumber(16, 8, 8, 0);
			dprBuild.HL_Superceded = true;

			// licenced GP1 release is older than clients DPR
			var gp1NewBuild = CreateReleaseBuild(ReleaseRings.Codes.GP1, new ZDateTime(2016, 10, 10));
			gp1NewBuild.VersionNumber = new VersionNumber(16, 6, 6, 0);
			gp1NewBuild.HL_Superceded = false;

			// current STD release is newer than clients DPR
			var stdNewBuild = CreateReleaseBuild(ReleaseRings.Codes.STD, new ZDateTime(2016, 10, 10));
			stdNewBuild.VersionNumber = new VersionNumber(16, 10, 10, 0);
			stdNewBuild.HL_Superceded = false;

			// incident reported by client in DPR version
			var incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			incident.IM_Description = "ClientDefectIncident";
			var licenceClient = AttachNewClientAndNewLicence_Sql2012(incident, "...", ReleaseRings.Codes.GP1);
			var client = (EDIOrgHeader)incident.Client;
			var clientContact = CreateContact(client, "contact@client.com");
			incident.Database.LD_OC_ContractInstallerOrInternalTechContact = clientContact.PK;
			licenceClient.Database.LD_HL_CurrentRunningVersion = dprBuild.PK;
			incident.Quote.CIQ_DeliveredDateUTC = ZDateTime.Empty;

			// incident fixed in STD version, but not GP1 version
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.STD, new ZDateTime(2016, 9, 9));

			Factory.Save();

			AssertEquals("Should deploy to STD because client version is older", "STD", process.GetReleaseBuildToDeployFor(incident.Database, isWeekly: false).HL_ReleaseStatus);
			AssertEquals("Should send upgrade because client is on DPR and STD is available", 1, process.GetIncidentsToSendUpgrade().Count);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			process.RunTask();

			AssertIncidentPostUpgradeStatus("ClientDefectIncident", ZDateTime.Now, new BusinessObjectFactory(), incident);

			AssertEquals("createdUpgraders.Count", 1, process.createdUpgraders.Count);
			var stdUpgrader = FindUpgrader(stdNewBuild, client);
			AssertUpgrader("stdUpgrader", stdUpgrader, incident);

			AssertEquals("stdUpgrader.Upgrades.Count", 1, stdUpgrader.Upgrades.Count);
			AssertUpgradeRequest("stdUpgrader.Upgrades[0]", stdUpgrader.Upgrades[0], incident, clientContact);

			AssertEquals("Logger.DebugLogStrings[0].Trim()", "Information|Processing incidents...", logger[0].Trim());
			AssertEquals("Logger.DebugLogStrings[1].Trim()", "Information|Incidents processed: 1", logger[1].Trim());

			AssertEquals("Emails Created", 2, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Email Body", process.Reporter.GetReportBody(), Env.OutgoingMailManager.EmailsCreated[1].Body);

			AssertReporterCounts(1, 0, 0);
			AssertIsProcessed("ClientDefectIncident", incident, true, "Upgrade sent");
		}

		[TestDate(2016, 10, 10)]
		public void TestExecute_ElevatedRing_BuildNotFound()
		{
			SetUpEnvironmentForExecute();

			// client has DPR version
			ReleaseBuild dprBuild = CreateReleaseBuild(ReleaseRings.Codes.DPR, new ZDateTime(2016, 8, 8));
			dprBuild.VersionNumber = new VersionNumber(16, 8, 8, 0);
			dprBuild.HL_Superceded = true;

			// licenced GP1 release is older than clients DPR
			ReleaseBuild gp1NewBuild = CreateReleaseBuild(ReleaseRings.Codes.GP1, new ZDateTime(2016, 10, 10));
			gp1NewBuild.VersionNumber = new VersionNumber(16, 6, 6, 0);
			gp1NewBuild.HL_Superceded = false;

			// current STD release is also older than clients DPR
			ReleaseBuild stdNewBuild = CreateReleaseBuild(ReleaseRings.Codes.STD, new ZDateTime(2016, 7, 7));
			stdNewBuild.VersionNumber = new VersionNumber(16, 7, 7, 0);
			stdNewBuild.HL_Superceded = false;

			// incident reported by client in DPR version
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			incident.IM_Description = "ClientDefectIncident";
			LicenceHeader licenceClient = AttachNewClientAndNewLicence_Sql2012(incident, "...", ReleaseRings.Codes.GP1);
			EDIOrgHeader client = (EDIOrgHeader)incident.Client;
			OrgContact clientContact = CreateContact(client, "contact@client.com");
			incident.Database.LD_OC_ContractInstallerOrInternalTechContact = clientContact.PK;
			licenceClient.Database.LD_HL_CurrentRunningVersion = dprBuild.PK;

			// incident completed in STD version, but not GP1 version
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.STD, new ZDateTime(2016, 9, 9));

			Factory.Save();

			// verify that all code paths cope with the error
			AssertEquals("Should deploy to same because client version is newest", "DPR", process.GetReleaseBuildToDeployFor(incident.Database, isWeekly: false).HL_ReleaseStatus);
			AssertEquals("Should not send upgrade because client is on DPR and STD is not available", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 1, 0);

			Env.OutgoingMailManager.EmailsCreated.Clear();

			process.RunTask();

			AssertEquals("createdUpgraders", null, process.createdUpgraders);

			AssertEquals("Logger.DebugLogStrings[0].Trim()", "Information|Processing incidents...", logger[0].Trim());
			AssertEquals("Logger.DebugLogStrings[1].Trim()", "Information|Incidents processed: 0", logger[1].Trim());

			AssertEquals("Emails Created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Email Body", process.Reporter.GetReportBody(), Env.OutgoingMailManager.EmailsCreated[0].Body);

			AssertReporterCounts(0, 2, 0);
			AssertIsProcessed("ClientDefectIncident", incident, false, "RelatedWorkItem's changes are not in latest ReleaseBuild yet");
		}

		[TestDate(2020, 4, 13)]
		public void TestExecute_SentBuildVersion()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();
			var incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licence = AttachNewClientAndNewLicence(incident, "EC1", ReleaseRings.Codes.GP1);
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.GP2, now.AddDays(-1));
			var client = (EDIOrgHeader)incident.Client;
			var contact = CreateContact(client, "contact1@client1.com");

			var gp1Build = CreateReleaseBuild(ReleaseRings.Codes.GP1, now);
			gp1Build.HL_MajorVersion = 21;
			gp1Build.HL_MinorVersion = 3;
			gp1Build.HL_Release = 1;
			gp1Build.HL_Patch = 5;

			var gp2Build = CreateReleaseBuild(ReleaseRings.Codes.GP2, now);
			gp2Build.HL_MajorVersion = 20;
			gp2Build.HL_MinorVersion = 12;
			gp2Build.HL_Release = 1;
			gp2Build.HL_Patch = 20;
			gp2Build.HL_Superceded = true;

			var gp2BuildLatest = CreateReleaseBuild(ReleaseRings.Codes.GP2, now.AddDays(1));
			gp2BuildLatest.HL_MajorVersion = 20;
			gp2BuildLatest.HL_MinorVersion = 12;
			gp2BuildLatest.HL_Release = 5;
			gp2BuildLatest.HL_Patch = 88;

			var db = licence.Database;
			db.LD_OC_LicenseeAdminContact = contact.PK;
			db.LD_HL_CurrentRunningVersion = gp2Build.PK;
			db.LD_HL_CurrentSentVersion = gp1Build.PK;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			process.RunTask();

			var newFactory = new BusinessObjectFactory();
			AssertIncidentPostUpgradeStatus("Upgrade should be sent for incident", now, newFactory, incident, quoteIsCreated: false);
		}

		[TestDate(2024, 12, 14)]
		public void TestExecute_IncidentsOnSameReleaseRingDifferentProduct()
		{
			//CW1
			var cw1ALP = CreateNewReleaseBuild(24, 11, 19, 10, superceded: true, ReleaseRings.Codes.ALP, ProductTypes.Codes.Enterprise, new ZDateTime(2024, 11, 19));
			var cw1DPR = CreateNewReleaseBuild(24, 10, 30, 115, superceded: true, ReleaseRings.Codes.DPR, ProductTypes.Codes.Enterprise, new ZDateTime(2024, 11, 19));
			var cw1STD = CreateNewReleaseBuild(24, 10, 30, 136, superceded: true, ReleaseRings.Codes.STD, ProductTypes.Codes.Enterprise, new ZDateTime(2024, 11, 19));
			var cw1GP1 = CreateNewReleaseBuild(24, 10, 30, 333, superceded: false, ReleaseRings.Codes.GP1, ProductTypes.Codes.Enterprise, new ZDateTime(2024, 12, 11));
			var cw1GP2 = CreateNewReleaseBuild(24, 7, 12, 940, superceded: false, ReleaseRings.Codes.GP2, ProductTypes.Codes.Enterprise, new ZDateTime(2024, 12, 9));

			//CWN
			var cwnALP = CreateNewReleaseBuild(24, 12, 12, 29, superceded: false, ReleaseRings.Codes.ALP, ProductTypes.Codes.CargoWiseNext, new ZDateTime(2024, 12, 12));
			var cwnDPR = CreateNewReleaseBuild(24, 12, 11, 9, superceded: false, ReleaseRings.Codes.DPR, ProductTypes.Codes.CargoWiseNext, new ZDateTime(2024, 12, 12));
			var cwnSTD = CreateNewReleaseBuild(24, 11, 27, 40, superceded: false, ReleaseRings.Codes.STD, ProductTypes.Codes.CargoWiseNext, new ZDateTime(2024, 12, 12));

			var incident1 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			incident1.IM_Description = "CW1 product incident";
			incident1.IM_Priority = "CR3";
			var licence1 = AttachNewClientAndNewLicence_Sql2012(incident1, "AAA", ReleaseRings.Codes.STD);
			var client1 = (EDIOrgHeader)incident1.Client;
			var contact1 = CreateContact(client1, "contact@AAA.com");
			var db1 = incident1.Database;
			var db1Version = CreateNewReleaseBuild(24, 10, 30, 100, superceded: true, ReleaseRings.Codes.STD, ProductTypes.Codes.Enterprise, new ZDateTime(2024, 11, 2));
			db1.LD_Product = ProductTypes.Codes.CargoWiseOne;
			db1.LD_OC_ContractInstallerOrInternalTechContact = contact1.PK;
			db1.LD_HL_CurrentRunningVersion = db1Version.PK;
			AttachNewCompletedWorkItem(incident1, ReleaseRings.Codes.GP1, new ZDateTime(2024, 12, 10)); //Latest CW1 branch is GP1

			var incident2 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			incident2.IM_Description = "CWN product incident";
			incident2.IM_Priority = "CR3";
			var licence2 = AttachNewClientAndNewLicence_Sql2012(incident2, "BBB", ReleaseRings.Codes.STD);
			var client2 = (EDIOrgHeader)incident2.Client;
			var contact2 = CreateContact(client2, "contact@BBB.com");
			var db2 = incident2.Database;
			var db2Version = CreateNewReleaseBuild(24, 10, 30, 100, superceded: true, ReleaseRings.Codes.STD, ProductTypes.Codes.Enterprise, new ZDateTime(2024, 11, 2));
			db2.LD_Product = ProductTypes.Codes.CargoWiseNext;
			db2.LD_OC_ContractInstallerOrInternalTechContact = contact2.PK;
			db2.LD_HL_CurrentRunningVersion = db2Version.PK;
			AttachNewCompletedWorkItem(incident2, ReleaseRings.Codes.STD, new ZDateTime(2024, 12, 10)); //Latest CWN branch is STD

			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			process.RunTask();

			AssertEquals("createdUpgraders.Count - CW1 and CWN should be processed seperatedly", 2, process.createdUpgraders.Count);
			var client1Upgrader = FindUpgrader(cw1GP1, client1);
			var client2Upgrader = FindUpgrader(cwnSTD, client2);

			AssertUpgrader("client1 Upgrader", client1Upgrader, incident1);
			AssertUpgrader("client2 Upgrader", client2Upgrader, incident2);

			incident1.Reload();
			AssertHasEConversationMessage(@"The latest version has changes that address this eRequest and has been sent to your system.
Sent version: CargoWise One 24.10.30.333, GP Release CW1 2024 Oct 30 patch 333, Exe Date: 11-Dec-24 00:00.", incident1.EConversation.ExistingConversation);

			incident2.Reload();
			AssertHasEConversationMessage(@"The latest version has changes that address this eRequest and has been sent to your system.
Sent version: CargoWise Next 24.11.27.40, 2024 Nov 27 patch 40, Exe Date: 12-Dec-24 00:00.", incident2.EConversation.ExistingConversation);
		}

		public void TestProcessIncidentInAnotherFactory()
		{
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			Factory.Save();
			AssertEquals("pre:", true, incident.IM_BugFixDeployed.IsEmpty);
			var task = new IncidentBasedAutoDeploymentServiceTaskForConcurrencyTest();
			task.ProcessIncidentInAnotherFactory(incident, "some reason", "some msg");
			AssertEquals("ProcessIncident called twice - retry after concurrency error", 2, task.CallsToProcessIncidentInAnotherFactoryWithoutRetry);
			AssertEquals("IM_BugFixDeployed.IsEmpty", false, incident.IM_BugFixDeployed.IsEmpty);
		}

		public void TestProcessIncidentWithNoContactEmailAddress()
		{
			ZDateTime now = ZDateTime.Now;
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_Code = "ZAC";
			staff.GS_EmailAddress = "bob@builders.com";
			EDIDataRegistry.Instance.IncidentsBatchProcessNotificationGroup = group.PK.ToGuid();

			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.GPR, now.AddDays(-1));
			CreateReleaseBuild(ReleaseRings.Codes.GPR, now);

			Factory.Save();

			process.RunTask();
			AssertReporterCounts(1, 0, 1);

			string errorInReport = "Incident " + incident.IM_IncidentNumber + " was processed but the client was not notified because there was no contact email address.";
			string error = "Error|" + errorInReport;
			AssertEquals("Logger.DebugLogStrings[1].Trim()", error, logger[1].Trim());
			AssertEquals("Reporter.HasError()", true, process.Reporter.HasError(errorInReport));
		}

		public void TestGetAdditionalNotification()
		{
			List<string> incidentDetails = new List<string>();
			UpgradeRequestCollectionContainer upgrader = new UpgradeRequestCollectionContainer(Factory, new UpgradeRequestCollection(Factory));
			upgrader.AdditionalNotification = "";

			incidentDetails.Add("x");
			AssertEquals("GetAdditionalNotification()", "The upgrade resolves the following incidents:\r\n\r\nx", process.GetAdditionalNotification(incidentDetails, upgrader));

			incidentDetails.Add("y");
			AssertEquals("GetAdditionalNotification()", "The upgrade resolves the following incidents:\r\n\r\nx\r\ny", process.GetAdditionalNotification(incidentDetails, upgrader));

			upgrader.AdditionalNotification = "z";
			AssertEquals("GetAdditionalNotification()", "The upgrade resolves the following incidents:\r\n\r\nx\r\ny\r\n\r\nz", process.GetAdditionalNotification(incidentDetails, upgrader));
		}

		public void TestProcessIncident()
		{
			ZDateTime now = ZDateTime.Now;
			SupportIncident supportIncident = CreateIncident(SupportIncidentCategoriesList.Codes.Support, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident defectIncident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident featureIncident = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident complianceRequirementIncident = CreateIncident(SupportIncidentCategoriesList.Codes.ComplianceRequirement, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident customerServiceIncident = CreateIncident(SupportIncidentCategoriesList.Codes.CustomerServiceRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);

			process.CloseAsUpgradeDeliveredAndAddMessageToClient(supportIncident, "");
			process.CloseAsUpgradeDeliveredAndAddMessageToClient(defectIncident, "");
			process.CloseAsUpgradeDeliveredAndAddMessageToClient(featureIncident, "");
			process.CloseAsUpgradeDeliveredAndAddMessageToClient(complianceRequirementIncident, "");
			process.CloseAsUpgradeDeliveredAndAddMessageToClient(customerServiceIncident, "");

			AssertEquals("supportStageIncident.IM_ResolutionCode", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, supportIncident.IM_ResolutionCode);
			AssertEquals("defectStageIncident.IM_ResolutionCode", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, defectIncident.IM_ResolutionCode);
			AssertEquals("featureStageIncident.IM_ResolutionCode", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, featureIncident.IM_ResolutionCode);
			AssertEquals("complianceRequirementIncident.IM_ResolutionCode", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, complianceRequirementIncident.IM_ResolutionCode);
			AssertEquals("customerServiceIncident.IM_ResolutionCode", SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, customerServiceIncident.IM_ResolutionCode);

			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			ReleaseBuild alphaBuild = CreateReleaseBuild(ReleaseRings.Codes.ALP, new ZDateTime(2008, 10, 03, 10, 22, 33));
			NewWorkItemTest.SetupVersionForBuild(alphaBuild, 1, 3, 3198, 0);

			process.CloseAsUpgradeDeliveredAndAddMessageToClient(incident, "Alpha Release 2008 Oct 03 (v1.3.3198.0) has been sent");
			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Alpha Release 2008 Oct 03 (v1.3.3198.0) has been sent"));

			SupportIncident incident2 = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			incident2.WorkflowItems.AddNew();
			process.CloseAsUpgradeDeliveredAndAddMessageToClient(incident2, "");
			Factory.Save();
			AssertEquals(0, incident2.WorkflowItems.Count);
		}

		public void TestAllRelatedWorkItemsAreChecked()
		{
			ZDateTime now = ZDateTime.Now;
			CreateReleaseBuild(ReleaseRings.Codes.DPR, now);
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.DPR);
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.DPR, now.AddDays(1));
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.DPR, now.AddDays(-1));
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.DPR, now.AddDays(1));
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.DPR, now.AddDays(-1));
			Factory.Save();
			AssertEquals("GetIncidentsToSendUpgrade().Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 1, 0);
			AssertIsProcessed(incident, false, "RelatedWorkItem's changes have NOT been patched to current ReleaseBuild");
		}

		public void TestAllRelatedWorkItemsAreChecked_EvenWhenAssignedToDAT()
		{
			ZDateTime now = ZDateTime.Now;
			CreateReleaseBuild(ReleaseRings.Codes.DPR, now.AddDays(1));
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.DPR);
			Factory.Save();
			var workItem = AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.DPR, now.AddDays(1));
			var checkInTask = workItem.WorkflowItems.Cast<ProcessTask>().Single();
			checkInTask.P9_GS_NKAssignedStaffMember = "DAT";
			checkInTask.P9_Status = "CLS";

			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = "UDF";
			task.P9_Status = "ASN";

			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;

			Factory.Save();
			AssertEquals("GetIncidentsToSendUpgrade().Count", 1, process.GetIncidentsToSendUpgrade().Count);
		}

		[TestDate(2008, 10, 03, 10, 0, 0)]
		public void TestPatchedRelatedWorkItems()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "alexander.korotun@cargowise.com";
			staff.GS_Code = "ZAC";
			EDIDataRegistry.Instance.IncidentsBatchProcessNotificationGroup = group.PK.ToGuid();

			ReleaseBuild dprBuild = CreateReleaseBuild(ReleaseRings.Codes.DPR, new ZDateTime(2008, 10, 03, 11, 22, 33));
			ReleaseBuild gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, new ZDateTime(2008, 10, 03, 09, 22, 33));

			SupportIncident incidentDpr = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licenceDpr = AttachNewClientAndNewLicence(incidentDpr, "...", ReleaseRings.Codes.DPR);

			// DPR incident
			OrgContact clientContact = CreateContact(incidentDpr.Client, "contact1@client1.com");
			licenceDpr.Database.LD_OC_LicenseeAdminContact = clientContact.PK;
			AttachNewCompletedWorkItem(incidentDpr, ReleaseRings.Codes.DPR, new ZDateTime(2008, 10, 03, 10, 0, 0));

			NewWorkItem workItem = incidentDpr.RelatedWorkItems[0];

			ProcessTask processTask = workItem.WorkflowItems.AddNew();
			processTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.DPR).CheckinTask;
			processTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			incidentDpr.IM_Status = SupportIncidentLookups.Status.Closed;
			incidentDpr.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			incidentDpr.IM_CloseTimeUtc = new ZDateTime(2008, 10, 03, 10, 0, 0);

			//GPR incident
			SupportIncident incidentGpr = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licenceGpr = AttachNewClientAndNewLicence(incidentGpr, "...", ReleaseRings.Codes.GPR);

			OrgContact clientContact2 = CreateContact(incidentGpr.Client, "contact1@client1.com");
			licenceGpr.Database.LD_OC_LicenseeAdminContact = clientContact2.PK;
			AttachNewCompletedWorkItem(incidentGpr, ReleaseRings.Codes.GPR, new ZDateTime(2008, 10, 04));

			NewWorkItem workItem2 = incidentGpr.RelatedWorkItems[0];

			ProcessTask processTask2 = workItem2.WorkflowItems.AddNew();
			processTask2.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GPR).CheckinTask;
			processTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			incidentGpr.IM_Status = SupportIncidentLookups.Status.Closed;
			incidentGpr.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			incidentGpr.IM_CloseTimeUtc = new ZDateTime(2008, 10, 4);

			Factory.Save();

			process.RunTask();

			AssertReporterCounts(1, 1, 0);
			AssertIsProcessed(incidentGpr, false, "RelatedWorkItem's changes have NOT been patched to current ReleaseBuild");
			AssertIsProcessed(incidentDpr, true, "Upgrade sent");
		}

		[TestDate(2021, 6, 3, 10, 0, 0)]
		public void TestPatchedRelatedWorkItems_NonCW()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "donAntonio@wtg.com";
			staff.GS_Code = "ZAC";
			EDIDataRegistry.Instance.IncidentsBatchProcessNotificationGroup = group.PK.ToGuid();

			ReleaseBuild alpBuild = CreateReleaseBuild(ReleaseRings.Codes.ALP, new ZDateTime(2008, 10, 03, 11, 22, 33));

			SupportIncident incidentAlp = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licenceAlp = AttachNewClientAndNewLicence(incidentAlp, "...", ReleaseRings.Codes.ALP);

			// DPR incident
			OrgContact clientContact = CreateContact(incidentAlp.Client, "contact1@client1.com");
			licenceAlp.Database.LD_OC_LicenseeAdminContact = clientContact.PK;
			AttachNewCompletedWorkItem(incidentAlp, ReleaseRings.Codes.DPR, new ZDateTime(2008, 10, 03, 10, 0, 0));

			NewWorkItem workItem = incidentAlp.RelatedWorkItems[0];

			ProcessTask processTask = workItem.WorkflowItems.AddNew();
			processTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			processTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			incidentAlp.IM_SystemCreateTimeUtc = new ZDateTime(2008, 10, 03, 10, 0, 0);
			incidentAlp.IM_Status = SupportIncidentLookups.Status.Closed;
			incidentAlp.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			incidentAlp.IM_CloseTimeUtc = new ZDateTime(2008, 10, 03, 10, 0, 0);

			Factory.Save();

			try
			{
				ReleaseBuildContentForLegacyTest.IsCargowiseOneChangeOverride = false;
				process.RunTask();

				AssertReporterCounts(1, 0, 0);
				AssertIsProcessed(incidentAlp, true, "Upgrade sent");
			}
			finally
			{
				ReleaseBuildContentForLegacyTest.IsCargowiseOneChangeOverride = true;
			}
		}

		[TestDate(2008, 10, 03, 10, 0, 0)]
		public void TestPromotedRelatedWorkItems()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "alexander.korotun@cargowise.com";
			staff.GS_Code = "ZAC";
			EDIDataRegistry.Instance.IncidentsBatchProcessNotificationGroup = group.PK.ToGuid();

			ReleaseBuild alphaBuild = CreateReleaseBuild(ReleaseRings.Codes.ALP, new ZDateTime(2008, 10, 03, 10, 22, 33));
			NewWorkItemTest.SetupVersionForBuild(alphaBuild, 1, 3, 3198, 0);
			ReleaseBuild dprBuild = CreateReleaseBuild(ReleaseRings.Codes.DPR, new ZDateTime(2008, 10, 03, 11, 22, 33));
			NewWorkItemTest.SetupVersionForBuild(dprBuild, 1, 3, 3198, 1);
			ReleaseBuild gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, new ZDateTime(2008, 10, 03, 12, 22, 33));
			NewWorkItemTest.SetupVersionForBuild(gprBuild, 1, 3, 3100, 123);

			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licence = AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.DPR);

			// DPR incident
			OrgContact clientContact = CreateContact(incident.Client, "contact1@client1.com");
			licence.Database.LD_OC_LicenseeAdminContact = clientContact.PK;
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.DPR, new ZDateTime(2008, 10, 03, 10, 0, 0));

			NewWorkItem workItem = incident.RelatedWorkItems[0];

			WorkItemProcessTask processTask = workItem.WorkflowItems.AddNew();
			processTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			processTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			processTask.Logs.AddNew(Events.JobClose, "1.3.3198.0|2008.10.03 10:22:33");

			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			incident.IM_CloseTimeUtc = new ZDateTime(2008, 10, 03, 10, 0, 0);

			Factory.Save();

			process.RunTask();

			AssertReporterCounts(1, 0, 0);
			AssertIsProcessed(incident, true, "Upgrade sent");
		}

		public void TestNoEmailNotificationGroup()
		{
			EDIDataRegistry.Instance.IncidentsBatchProcessNotificationGroup = Guid.Empty;
			ZDateTime now = ZDateTime.Now;

			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.GPR, now.AddDays(-1));
			CreateReleaseBuild(ReleaseRings.Codes.GPR, now);

			Factory.Save();

			AssertNoExceptionThrown(delegate
			{ process.RunTask(); });

			AssertReporterCounts(1, 0, 1);
			AssertEquals(null, ErrorReporter.LastExceptionReported);

			string errorInReport = "Incident " + incident.IM_IncidentNumber + " was processed but the client was not notified because there was no contact email address.";
			string error = "Error|" + errorInReport;
			AssertEquals("Logger.DebugLogStrings[1].Trim()", error, logger[1].Trim());
			AssertEquals("Reporter.HasError()", true, process.Reporter.HasError(errorInReport));
		}

		[TestDate(2025, 5, 18, 10, 8, 0)]
		public void TestShouldSentUpgradeWhenReleaseBuildIsReady()
		{
			var cw1GP2CurrentRunningRelease = CreateNewReleaseBuild(24, 10, 30, 882, superceded: true, ReleaseRings.Codes.GP2, ProductTypes.Codes.Enterprise, new ZDateTime(2025, 4, 21, 4, 38, 0));
			cw1GP2CurrentRunningRelease.HL_Product = "ENT";
			var cw1GP2Latest = CreateNewReleaseBuild(24, 10, 30, 972, superceded: false, ReleaseRings.Codes.GP2, ProductTypes.Codes.Enterprise, new ZDateTime(2025, 5, 18, 9, 49, 0));
			cw1GP2Latest.HL_Product = "ENT";
			var cgwGP1CurrentSentRelease = CreateNewReleaseBuild(25, 4, 7, 172, superceded: false, ReleaseRings.Codes.GP1, ProductTypes.Codes.CargoWise, new ZDateTime(2025, 5, 15, 11, 47, 0));
			cgwGP1CurrentSentRelease.HL_Product = "CGW";
			var cgwGP1Latest = CreateNewReleaseBuild(25, 4, 7, 178, superceded: false, ReleaseRings.Codes.GP1, ProductTypes.Codes.CargoWise, new ZDateTime(2025, 5, 18, 4, 36, 0));
			cgwGP1Latest.HL_Product = "CGW";

			var incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			incident.IM_Description = "CW1 product incident";
			incident.IM_Priority = "CR3";
			var licence = AttachNewClientAndNewLicence_Sql2012(incident, "AAA", ReleaseRings.Codes.GP2);
			var client = (EDIOrgHeader)incident.Client;
			var contact = CreateContact(client, "penney@AAA.com");
			var database = incident.Database;
			database.LD_Product = ProductTypes.Codes.CargoWiseOne;
			database.LD_OC_ContractInstallerOrInternalTechContact = contact.PK;
			database.LD_HL_CurrentRunningVersion = cw1GP2CurrentRunningRelease.PK;
			database.LD_HL_CurrentSentVersion = cgwGP1CurrentSentRelease.PK;
			database.LD_HostedLocation = "NCW";
			database.LD_ServerCode = "PRO";

			var workItem = AttachNewWorkItem(incident, "GP2", ProcessTaskStatusCodeList.Codes.Closed);
			workItem.WKI_WorkItemNumber = "WI00030001";
			var taskGP1 = workItem.WorkflowItems.AddNew();
			taskGP1.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GP1).CheckinTask;
			taskGP1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			taskGP1.CompletedTimeLocal = new ZDateTime(2025, 5, 17, 15, 2, 0);
			var taskGP2 = workItem.WorkflowItems.AddNew();
			taskGP2.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GP2).CheckinTask;
			taskGP2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			taskGP2.CompletedTimeLocal = new ZDateTime(2025, 5, 18, 0, 5, 0);
			Factory.Save();

			using (var cmd = Db.Connection.Command(@"
INSERT INTO dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_IsEstimate, SL_IsCancelled, SL_Reference, SL_PostedTimeUtc, SL_EventTime, SL_GS_NKUser, SL_SE_NKEvent, SL_GB_NKBranch, SL_GE_NKDepartment, SL_FireWorkflow, SL_DataSource, SL_EventTimeUtc)
VALUES ('1EC0732C-7CD4-4BEF-82F5-76C9C319C163', 'OrgHeader', @OrgHeaderPK, 'N', 'N', @Reference, '2005-05-15 10:01:39', '2005-05-15 20:01:39', 'ZZ', 'UPG', 'SDS', 'BRN', 0, 'C', '2005-05-15 10:01:39')"))
			{
				cmd.AddParameter("@OrgHeaderPK", SqlDbType.UniqueIdentifier, client.PK.ToGuid());
				cmd.AddParameter("@Reference", SqlDbType.Text, "GP Release CW1 2025 Apr 07 patch 172 (v25.4.7.172) will be sent to this client, PRO via HTP");
				cmd.ExecuteNonQuery();
			}

			process.RunTask();
			var loadedIncident = new BusinessObjectFactory().Load<SupportIncident>(incident.PK);
			AssertHasEConversationMessage(@"The latest version has changes that address this eRequest and has been sent to your system.
Sent version: CargoWise One 24.10.30.972, Previous GP Release 2024 Oct 30 patch 972, Exe Date: 18-May-25 09:49.", loadedIncident.EConversation.ExistingConversation);
			AssertIsProcessed(incident, processed: true, "Upgrade sent");
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Check Should Send Upgrade

		public void TestTryGetVersionNumberFromLog()
		{
			VersionNumber versionNumber;
			StmALog log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = "ReleaseBuild dated 01-Sep-06 00:00 (v1.2.3.4) will be sent to this client, SYD via EWA";
				AssertEquals("TryGetVersionNumberFromLog()", true, IncidentBasedAutoDeploymentServiceTask.TryGetVersionNumberFromLog(log, out versionNumber));
				AssertEquals("versionNumber", new VersionNumber(1, 2, 3, 4), versionNumber);

				log.SL_Reference = "DP Release 2006 Sep 02 patch 6 (v1.2.4.5) will be sent to this client, SYD via EWA";
				AssertEquals("TryGetVersionNumberFromLog()", true, IncidentBasedAutoDeploymentServiceTask.TryGetVersionNumberFromLog(log, out versionNumber));
				AssertEquals("versionNumber", new VersionNumber(1, 2, 4, 5), versionNumber);

				log.SL_Reference = "DP Release 2000 Boo 06 patch 6 will be sent to this client, SYD via EWA";
				AssertEquals("TryGetVersionNumberFromLog()", false, IncidentBasedAutoDeploymentServiceTask.TryGetVersionNumberFromLog(log, out versionNumber));

				log.SL_Reference = "ReleaseBuild dated 01-Sep-06 00:00 (vx.2.3.4) will be sent to this client, SYD via EWA";
				AssertEquals("TryGetVersionNumberFromLog()", false, IncidentBasedAutoDeploymentServiceTask.TryGetVersionNumberFromLog(log, out versionNumber));
			}
		}

		StmALog[] GetLogsToCheck(SupportIncident incident, ReleaseBuild build)
		{
			return IncidentBasedAutoDeploymentServiceTask.GetLogsToCheck(incident.Database, new[] { incident }, build);
		}

		[TestDate(2000, 1, 1)]
		public void TestGetLogsToCheck()
		{
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licence = AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			OrgHeader client = incident.Client;
			LicenceDatabase database = licence.Database;

			StmALog log1 = client.Logs.AddNew(Events.UpgradeSucceeded, "ReleaseBuild dated 01-Jan-00 00:00 (v1.2.3.4) will be sent to this client, BNE via EWA");
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2000, 1, 2);

			StmALog log2 = client.Logs.AddNew(Events.UpgradeFailed, "ReleaseBuild dated 02-Jan-00 00:00 (v1.2.3.5) will be sent to this client, SYD via EWA");
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2000, 1, 3);

			StmALog log3 = client.Logs.AddNew(Events.UpgradeSucceeded, "ReleaseBuild dated 03-Jan-00 00:00 (v1.2.3.6) will be sent to this client, BNE via EWA,SYD via EWA");
			Factory.Save();

			CreateReleaseBuild(ReleaseRings.Codes.GPR, ZDateTime.Now);
			TestDateAttribute.Date = new DateTime(2000, 1, 4);

			StmALog log4 = client.Logs.AddNew(Events.UpgradeSucceeded, "ReleaseBuild dated 04-Jan-00 00:00 (v1.2.3.7) will be sent to this client, SYD via EWA");
			Factory.Save();

			ReleaseBuild build = process.GetReleaseBuildToDeployFor(incident.Database, isWeekly: false);

			database.LD_ServerCode = "BNE";
			StmALog[] logs = GetLogsToCheck(incident, build);
			AssertEquals("Length", 2, logs.Length);
			AssertCollectionContains(log1, logs);
			AssertCollectionContains(log3, logs);

			database.LD_ServerCode = "SYD";
			logs = GetLogsToCheck(incident, build);
			AssertEquals("Length", 2, logs.Length);
			AssertCollectionContains(log3, logs);
			AssertCollectionContains(log4, logs);

			TestDateAttribute.Date = new DateTime(2000, 1, 30);
			ReleaseBuild gprBuild = process.Builds.GetLatestAvailableBuild(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GPR, takeWeeklyBuildsInsteadOfLatest: false);

			gprBuild.HL_ExeVersionDate = ZDateTime.Now;
			logs = GetLogsToCheck(incident, build);
			AssertEquals("Length", 1, logs.Length);
			AssertEquals("[0]", log4, logs[0]);

			gprBuild.HL_ExeVersionDate = new ZDateTime(2000, 1, 1);
			logs = GetLogsToCheck(incident, build);
			AssertEquals("Length", 2, logs.Length);
			AssertCollectionContains(log3, logs);
			AssertCollectionContains(log4, logs);
		}

		[TestDate(2000, 1, 1)]
		public void TestGetLogsDontLoadAllData()
		{
			SupportIncident incident;
			List<StmALog> expectedLogs;
			PrepareTestData(out incident, out expectedLogs);

			var expectedHitCounts = new Dictionary<string, int>
				{
					{ IncidentMainSchema.Constants.TableName, 1 },
					{ LicenceDatabaseSchema.Constants.TableName, 3 },
					{ LicenceEnterpriseSchema.Constants.TableName, 1 },
					{ LicenceHeaderSchema.Constants.TableName, 1 },
					{ LicenceCompanySchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ ClientCompanySchema.Constants.TableName, 0 }
				};

			var cleanFactory = new BusinessObjectFactory();
			incident = cleanFactory.Load<SupportIncident>(incident.PK);
			var build = process.GetReleaseBuildToDeployFor(incident.Database, isWeekly: false);
			incident.Database.LD_ServerCode = "BNE";
			AssertDbHits(expectedHitCounts, cleanFactory);

			// loading of incident.Client + searching for logs
			GetLogsToCheck(incident, build);
			expectedHitCounts[OrgHeaderSchema.Constants.TableName] = 1;
			expectedHitCounts[StmALogSchema.Constants.TableName] = 1;
			AssertDbHits(expectedHitCounts, cleanFactory);

			// No new hits
			GetLogsToCheck(incident, build);
			AssertDbHits(expectedHitCounts, cleanFactory);

			// first search gives 0 + second search
			TestDateAttribute.Date = new DateTime(2000, 1, 30);
			var gprBuild = process.Builds.GetLatestAvailableBuild(ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GPR, takeWeeklyBuildsInsteadOfLatest: false);
			gprBuild.HL_ExeVersionDate = ZDateTime.Now;
			GetLogsToCheck(incident, build);
			expectedHitCounts[StmALogSchema.Constants.TableName] += 2;
			AssertDbHits(expectedHitCounts, cleanFactory);

			// searching for logs
			gprBuild.HL_ExeVersionDate = new ZDateTime(2000, 1, 1);
			GetLogsToCheck(incident, build);
			expectedHitCounts[StmALogSchema.Constants.TableName] += 1;
			AssertDbHits(expectedHitCounts, cleanFactory);

			// should load logs, cause they are not loaded yet
			incident.Client.Logs.Find(new ZQuery());
			expectedHitCounts[StmALogSchema.Constants.TableName] += 1;
			AssertDbHits(expectedHitCounts, cleanFactory);

			// No new hits
			incident.Client.Logs.Find(new ZQuery());
			AssertDbHits(expectedHitCounts, cleanFactory);
		}

		void PrepareTestData(out SupportIncident incident, out List<StmALog> expectedLogs)
		{
			incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);

			expectedLogs = new List<StmALog>
				{
					incident.Client.Logs.AddNew(Events.UpgradeSucceeded, "ReleaseBuild dated 01-Jan-00 00:00 (v1.2.3.4) will be sent to this client, BNE via EWA")
				};
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2000, 1, 2);

			expectedLogs.Add(incident.Client.Logs.AddNew(Events.UpgradeFailed, "ReleaseBuild dated 02-Jan-00 00:00 (v1.2.3.5) will be sent to this client, SYD via EWA"));
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2000, 1, 3);

			expectedLogs.Add(incident.Client.Logs.AddNew(Events.UpgradeSucceeded, "ReleaseBuild dated 03-Jan-00 00:00 (v1.2.3.6) will be sent to this client, BNE via EWA,SYD via EWA"));
			Factory.Save();

			CreateReleaseBuild(ReleaseRings.Codes.GPR, ZDateTime.Now);
			TestDateAttribute.Date = new DateTime(2000, 1, 4);

			expectedLogs.Add(incident.Client.Logs.AddNew(Events.UpgradeSucceeded, "ReleaseBuild dated 04-Jan-00 00:00 (v1.2.3.7) will be sent to this client, SYD via EWA"));
			Factory.Save();
		}

		#endregion

		#region Get Incidents to Send Upgrade

		public void TestGetIncidentsToSendUpgrade_ClientOnHigherRing()
		{
			// client has DPR version
			ReleaseBuild dprBuild = CreateReleaseBuild(ReleaseRings.Codes.DPR, new ZDateTime(2016, 8, 8));
			dprBuild.VersionNumber = new VersionNumber(16, 8, 8, 0);
			dprBuild.HL_Superceded = true;

			// current GP1 release is older than clients DPR
			ReleaseBuild gp1NewBuild = CreateReleaseBuild(ReleaseRings.Codes.GP1, new ZDateTime(2016, 10, 10));
			gp1NewBuild.VersionNumber = new VersionNumber(16, 6, 6, 0);
			gp1NewBuild.HL_Superceded = false;

			// current STD release is newer than clients DPR
			ReleaseBuild stdNewBuild = CreateReleaseBuild(ReleaseRings.Codes.STD, new ZDateTime(2016, 10, 10));
			stdNewBuild.VersionNumber = new VersionNumber(16, 10, 10, 0);
			stdNewBuild.HL_Superceded = false;

			// incident completed in STD version after client version and before release version
			SupportIncident incident1 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			LicenceHeader licenceClient1 = AttachNewClientAndNewLicence_Sql2012(incident1, "...", ReleaseRings.Codes.GP1);
			AttachNewCompletedWorkItem(incident1, ReleaseRings.Codes.STD, new ZDateTime(2016, 9, 9));

			licenceClient1.Database.LD_HL_CurrentRunningVersion = dprBuild.PK;

			Factory.Save();

			var ring = process.GetReleaseBuildToDeployFor(incident1.Database, isWeekly: false).HL_ReleaseStatus;
			AssertEquals("Should deploy to STD because client is on DPR and STD is available", "STD", ring);
			AssertEquals("Should send upgrade because client is on DPR and STD is available", 1, process.GetIncidentsToSendUpgrade().Count);
		}

		public void TestGetIncidentsToSendUpgrade_GpcIncident()
		{
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPC);
			Factory.Save();

			AssertEquals("No GPC and GPR, therefore do not send upgrade", 0, process.GetIncidentsToSendUpgrade().Count);

			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, ZDateTime.Now.AddDays(-1));
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.GPR, gprBuild.ReleaseDate.AddDays(-1));
			var process2 = new IncidentBasedAutoDeploymentServiceTask();
			Factory.Save();

			AssertEquals("Should send upgrade because client is on GPC and GPR is available", 1, process2.GetIncidentsToSendUpgrade().Count);
		}

		public void TestGetIncidentsToSendUpgrade_IncidentProductNotEnterprise()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			incident.IM_Product = ProductTypes.Codes.EHub;
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			CreateReleaseBuild(ReleaseRings.Codes.GPR, ZDateTime.Now.AddDays(-1));
			Factory.Save();

			process.AddNeedUpgrade(incident.PK, false);
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(1, 0, 0);
			AssertEquals("CLS", incident.IM_Status);
		}

		public void TestGetIncidentsToSend_ShouldNotCloseTasks()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var incident = Factory.New<SupportIncident>();
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			incident.IM_IncidentType = IncidentConstants.IncidentType.SupportIncident;
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			CreateReleaseBuild(ReleaseRings.Codes.GPR, ZDateTime.Now.AddDays(-1));

			Factory.Save();

			var opnTask = incident.WorkflowItems.AddNew();
			var wrkTask = incident.WorkflowItems.AddNew();

			opnTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			wrkTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			wrkTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			opnTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			Factory.Save();

			process.AddNeedUpgrade(incident.PK, false);
			AssertEquals(0, process.GetIncidentsToSendUpgrade().Count);
			AssertEquals(1, process.Reporter.ProcessedIncidentCount);

			incident.Reload();
			incident.WorkflowItems.Reload(true);
			AssertEquals(2, incident.WorkflowItems.Cast<ProcessTask>().Count(x => x.IsCurrent));

			Assert(!incident.IsClosedOrCancelled);
			AssertEquals(incident.IM_ResolutionCode, SupportIncidentLookups.DispositionList.Constants.Working.Assigned);
		}

		public void TestGetIncidentsToSendUpgrade_OpenIncident()
		{
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Support, SupportIncidentLookups.Status.Open, SupportIncidentLookups.DispositionList.Constants.Open.AssignedAwaitingAction);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			Factory.Save();
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 0, 0);
		}

		public void TestGetIncidentsToSendUpgrade_IncidentClosedInSupport()
		{
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Support, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			Factory.Save();
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 0, 0);
		}

		public void TestGetIncidentsToSendUpgrade_IncidentClosedInContentDevelopment()
		{
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.ContentDevelopment, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			Factory.Save();
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 0, 0);
		}

		public void TestGetIncidentsToSendUpgrade_BadIncidentState()
		{
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			incident.IM_Status = SupportIncidentLookups.Status.Working;
			incident.IM_ResolutionCode = "";
			incident.IM_Category = "";
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			Factory.Save();
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 0, 0);
		}

		public void TestGetIncidentsToSendUpgrade_DatabaseBlocked()
		{
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licence = AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			licence.Database.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			Factory.Save();
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 1, 0);
			AssertIsProcessed(incident, false, "Client database has an upgrade method of Blocked.");
		}

		public void TestGetIncidentsToSendUpgrade_RelatedWorkItemWithNotCompletedShelfCheckInTask()
		{
			CreateReleaseBuild(ReleaseRings.Codes.GPR, ZDateTime.Now);
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			AttachNewOpenWorkItem(incident, ReleaseRings.Codes.GPR);
			WorkItemProcessTask processTask = incident.RelatedWorkItems[0].WorkflowItems.AddNew();
			processTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GPR).CheckinTask;
			processTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			Factory.Save();

			AssertEquals("Incident cannot be set to Waiting Upgrade because relate WI is not closed", SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, incident.IM_ResolutionCode);
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 0, 0);
			AssertIsProcessed(incident, false, null);
		}

		public void TestGetIncidentsToSendUpgrade_RelatedWorkItemWithCompletedShelfCheckInTask_DifferentRing()
		{
			var releaseBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, ZDateTime.Now);
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			incident.IM_CloseTimeUtc = ZDateTime.Now.AddHours(-1);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			AttachNewOpenWorkItem(incident, ReleaseRings.Codes.GPR);
			var processTask = incident.RelatedWorkItems[0].WorkflowItems.AddNew();
			processTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			processTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			processTask.CompletedTimeLocal = ZDateTime.Now.AddHours(-1);
			Factory.Save();

			Assert("Precondition:", releaseBuild.HL_ExeVersionDate >= processTask.CompletedTimeLocal.ToSmallDateTimeFloor());
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 1, 0);
			AssertIsProcessed(incident, false, "RelatedWorkItem's changes are not in latest ReleaseBuild yet");
		}

		public void TestGetIncidentsToSendUpgrade_RelatedWorkItemWithCompletedShelfCheckInTask_IncidentCloseDateAfterExeDate()
		{
			var releaseBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, ZDateTime.Now);
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			incident.IM_CloseTimeUtc = ZDateTime.Now.AddHours(1);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			AttachNewOpenWorkItem(incident, ReleaseRings.Codes.GPR);
			var processTask = incident.RelatedWorkItems[0].WorkflowItems.AddNew();
			processTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GPR).CheckinTask;
			processTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			processTask.CompletedTimeLocal = ZDateTime.Now.AddHours(1);
			Factory.Save();

			Assert("Precondition:", releaseBuild.HL_ExeVersionDate < processTask.CompletedTimeLocal.ToSmallDateTimeFloor());
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 1, 0);
			AssertIsProcessed(incident, false, "RelatedWorkItem's changes have NOT been patched to current ReleaseBuild");
		}

		public void TestGetIncidentsToSendUpgrade_RelatedWorkItemWithCompletedShelfCheckInTask_IncidentCloseDateBeforeExeDate()
		{
			var releaseBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, ZDateTime.Now);
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			incident.IM_CloseTimeUtc = ZDateTime.Now.AddHours(-1);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			AttachNewOpenWorkItem(incident, ReleaseRings.Codes.GPR);
			var processTask = incident.RelatedWorkItems[0].WorkflowItems.AddNew();
			processTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GPR).CheckinTask;
			processTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			incident.IM_Status = SupportIncidentLookups.Status.Closed;
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade;
			processTask.CompletedTimeLocal = ZDateTime.Now.AddHours(-1);
			Factory.Save();

			Assert("Precondition:", releaseBuild.HL_ExeVersionDate >= processTask.CompletedTimeLocal.ToSmallDateTimeFloor());
			AssertEquals("Count", 1, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 0, 0);
		}

		[TestDate(2017, 6, 5, 16, 39, 0)]
		public void TestGetIncidentsToSendUpgrade_ClientHasOlderButPatchedRelease()
		{
			ZDateTime now = ZDateTime.Now;
			ReleaseBuild gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			gprBuild.VersionNumber = new VersionNumber(1, 2, 3000, 5);
			ReleaseBuild superceededGprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			superceededGprBuild.VersionNumber = new VersionNumber(1, 2, 2000, 5);
			superceededGprBuild.HL_Superceded = true;

			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licence = AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.GPR, superceededGprBuild.ReleaseDate.AddDays(-1));
			licence.Database.LD_HL_CurrentRunningVersion = superceededGprBuild.PK;
			WorkItemProcessTask task = incident.RelatedWorkItems[0].WorkflowItems[0];
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.LPB).CheckinTask;

			Factory.Save();

			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(1, 0, 0);
			AssertIsProcessed(incident, true, "Upgrade not sent as the client is already on the same version");

			AssertHasEConversationMessage(@"No upgrade sent. Your system is already running a version with changes that address this eRequest.
Currently installed version: ediEnterprise 1.2.2000.5, GP Release 2005 Jun 23 patch 5, Exe Date: 05-Jun-17 16:39.", incident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", incident.EConversation.GetTimeOrderedMessages().Last().Body);
		}

		public void TestGetIncidentsToSendUpgrade_NoAvailableBuild()
		{
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.GPR, ZDateTime.Now);
			Factory.Save();
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 1, 0);
			AssertIsProcessed(incident, false, "ReleaseBuild not available");
			string errorInReport = "Build Not Found for client " + incident.Client.OH_Code + " in incident# " + incident.IM_IncidentNumber + ".";
			string error = "Warning|" + errorInReport;
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertEquals(error, logger[0].Trim());
		}

		public void TestGetIncidentsToSendUpgrade_NoWorkItem()
		{
			CreateReleaseBuild(ReleaseRings.Codes.GPR, ZDateTime.Now);
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			Factory.Save();
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 1, 0);
			AssertIsProcessed(incident, false, "No RelatedWorkItem");
		}

		public void TestGetIncidentsToSendUpgrade_IncidentHasNoClientOrLicence()
		{
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			Factory.Save();
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 0, 1);
			string errorInReport = "Incident " + incident.IM_IncidentNumber + " does not have a database or client.";
			string error = "Warning|" + errorInReport;
			AssertEquals("Reporter.HasError()", true, process.Reporter.HasError(errorInReport));
			AssertEquals(error, logger[0].Trim());
		}

		[TestDate(2017, 6, 5, 16, 39, 0)]
		public void TestGetIncidentsToSendUpgrade_ClientOnSameVersion()
		{
			ZDateTime now = ZDateTime.Now;
			ReleaseBuild build = CreateReleaseBuild(ReleaseRings.Codes.DPR, now);
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licence = AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.DPR);
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.DPR, now.AddDays(-1));
			licence.Database.LD_HL_CurrentRunningVersion = build.PK;
			Factory.Save();
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(1, 0, 0);
			AssertIsProcessed(incident, true, "Upgrade not sent as the client is already on the same version");
			AssertHasEConversationMessage(@"No upgrade sent. Your system is already running a version with changes that address this eRequest.
Currently installed version: ediEnterprise 1.1.2083.0, DP Release 2005 Sep 14, Exe Date: 05-Jun-17 16:39.", incident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", incident.EConversation.GetTimeOrderedMessages().Last().Body);
		}

		[TestDate(2017, 6, 5, 16, 39, 0)]
		public void TestGetIncidentsToSendUpgrade_ClientOnHigherRelease()
		{
			ZDateTime now = ZDateTime.Now;
			ReleaseBuild dprBuild = CreateReleaseBuild(ReleaseRings.Codes.DPR, now);
			dprBuild.VersionNumber = new VersionNumber(1, 2, 3000, 10);
			ReleaseBuild gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			gprBuild.VersionNumber = new VersionNumber(1, 2, 2000, 5);
			gprBuild.HL_Superceded = true;
			ReleaseBuild newerGprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddHours(1));
			newerGprBuild.VersionNumber = new VersionNumber(1, 2, 2000, 6);
			newerGprBuild.HL_Superceded = false;

			SupportIncident incidentDprRing = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licence1 = AttachNewClientAndNewLicence(incidentDprRing, "...", ReleaseRings.Codes.GPR);
			AttachNewCompletedWorkItem(incidentDprRing, ReleaseRings.Codes.GPR, now.AddDays(-1));
			licence1.Database.LD_HL_CurrentRunningVersion = dprBuild.PK;

			SupportIncident incidentGprRing = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licence2 = AttachNewClientAndNewLicence(incidentGprRing, "...", ReleaseRings.Codes.GPR);
			AttachNewCompletedWorkItem(incidentGprRing, ReleaseRings.Codes.GPR, now.AddDays(-1));
			licence2.Database.LD_HL_CurrentRunningVersion = newerGprBuild.PK;

			Factory.Save();
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(1, 1, 0);

			AssertIsProcessed(incidentDprRing, false, "RelatedWorkItem's changes are not in latest ReleaseBuild yet");
			AssertIsProcessed(incidentGprRing, true, "Upgrade not sent as the client is already on the same version");
			AssertHasEConversationMessage(@"No upgrade sent. Your system is already running a version with changes that address this eRequest.
Currently installed version: ediEnterprise 1.2.2000.6, GP Release 2005 Jun 23 patch 6, Exe Date: 05-Jun-17 17:39.", incidentGprRing.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", incidentGprRing.EConversation.GetTimeOrderedMessages().Last().Body);
		}

		public void TestGetIncidentsToSendUpgrade_ClientNotOnLowestSupportedSqlVersion()
		{
			ZDateTime now = ZDateTime.Now;
			ReleaseBuild dprBuild = CreateReleaseBuild(ReleaseRings.Codes.DPR, now);
			var bumpDate = SqlCutOverHelperTestHelper.DateWhenMinimumGenerationBump;
			dprBuild.VersionNumber = new VersionNumber(bumpDate.Year - 2000, bumpDate.Month, bumpDate.Day, 0);
			ReleaseBuild gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			gprBuild.VersionNumber = new VersionNumber(2, 0, 0, 0);

			var incident1 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licence1 = AttachNewClientAndNewLicence(incident1, "...", ReleaseRings.Codes.DPR);
			AttachNewCompletedWorkItem(incident1, ReleaseRings.Codes.DPR, now.AddDays(-1));
			licence1.Database.LD_SQLVersion = "Sql2008";

			var incident2 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licence2 = AttachNewClientAndNewLicence(incident2, "...", ReleaseRings.Codes.GPR);
			AttachNewCompletedWorkItem(incident2, ReleaseRings.Codes.GPR, now.AddDays(-1));
			licence2.Database.LD_SQLVersion = "Sql2012";

			var incident3 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licence3 = AttachNewClientAndNewLicence(incident3, "...", ReleaseRings.Codes.DPR);
			AttachNewCompletedWorkItem(incident3, ReleaseRings.Codes.DPR, now.AddDays(-1));
			licence3.Database.LD_SQLVersion = SqlServerVersionNumber.SupportedVersions.Max().Generation.Name;

			Factory.Save();

			AssertEquals("Count", 3, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 0, 0);
			AssertIsProcessed(incident1, false, null); // we no longer expect that old versions of sql to be the cause of process failure.
		}

		[TestDate(2017, 6, 5, 16, 39, 0)]
		public void TestGetIncidentsToSendUpgrade_ClientSentSameVersion_FromDatabase()
		{
			ZDateTime now = ZDateTime.Now;
			ReleaseBuild build = CreateReleaseBuild(ReleaseRings.Codes.DPR, now);
			build.VersionNumber = new VersionNumber(1, 2, 3, 4);

			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licence = AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.DPR);
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.DPR, now.AddDays(-1));
			licence.Database.LD_HL_CurrentSentVersion = build.PK;

			Factory.Save();

			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(1, 0, 0);
			AssertIsProcessed(incident, true, "Upgrade not sent as the client has already been sent the same version");
			AssertHasEConversationMessage(@"No upgrade sent. A version has already been sent to your system with changes that address this eRequest.
Previously sent version: ediEnterprise 1.2.3.4, DP Release 2000 Jan 04 patch 4, Exe Date: 05-Jun-17 16:39.", incident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", incident.EConversation.GetTimeOrderedMessages().Last().Body);
		}

		[TestDate(2017, 6, 5, 16, 39, 0)]
		public void TestGetIncidentsToSendUpgrade_ClientSentHigherVersion()
		{
			ZDateTime now = ZDateTime.Now;
			ReleaseBuild releaseBuild = CreateReleaseBuild(ReleaseRings.Codes.DPR, now.AddHours(-1));

			ReleaseBuild newerBuild = CreateReleaseBuild(ReleaseRings.Codes.DPR, now);
			newerBuild.HL_Patch++;
			newerBuild.HL_Superceded = true;

			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.DPR);
			AttachNewCompletedWorkItem(incident, ReleaseRings.Codes.DPR, now.AddDays(-1));
			((EDIOrgHeader)incident.Client).LicCompany.LicEnterprise.Databases[0].LD_HL_CurrentSentVersion = newerBuild.PK;

			Factory.Save();

			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(1, 0, 0);
			AssertIsProcessed(incident, true, "Upgrade not sent as the client has already been sent a newer version");
			AssertHasEConversationMessage(@"No upgrade sent. A newer version has already been sent to your system with changes that address this eRequest.
Previously sent version: ediEnterprise 1.1.2083.1, DP Release 2005 Sep 14 patch 1, Exe Date: 05-Jun-17 16:39.", incident.EConversation.ExistingConversation);
			AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", incident.EConversation.GetTimeOrderedMessages().Last().Body);
		}

		public void TestGetIncidentsToSendUpgrade_AllConditionsMet()
		{
			ZDateTime now = ZDateTime.Now;

			CreateReleaseBuild(ReleaseRings.Codes.ALP, now.AddDays(-2));
			CreateReleaseBuild(ReleaseRings.Codes.DPR, now.AddDays(-3));
			ReleaseBuild newStdBuild = CreateReleaseBuild(ReleaseRings.Codes.STD, now.AddDays(-4));
			ReleaseBuild oldStdBuild = CreateReleaseBuild(ReleaseRings.Codes.STD, now.AddDays(-6));
			newStdBuild.VersionNumber = new VersionNumber(1, 2, 3, 4);
			oldStdBuild.VersionNumber = new VersionNumber(1, 2, 3, 2);

			SupportIncident supportIncident = CreateIncident(SupportIncidentCategoriesList.Codes.Support, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident defectIncident1 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident defectIncident2 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident featureIncident1 = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident featureIncident2 = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);

			AttachNewClientAndNewLicence(supportIncident, "..1", ReleaseRings.Codes.ALP);
			var licenceDefectIncident1 = AttachNewClientAndNewLicence(defectIncident1, "..2", ReleaseRings.Codes.STD);
			AttachNewClientAndNewLicence(defectIncident2, "..3", ReleaseRings.Codes.STD);
			AttachNewClientAndNewLicence(featureIncident1, "..4", ReleaseRings.Codes.DPR);
			AttachClientAndCompany(featureIncident2, featureIncident1.Client, featureIncident1.ClientCompany);
			AttachNewCompletedWorkItem(defectIncident1, ReleaseRings.Codes.STD, now.AddDays(-5));
			AttachNewCompletedWorkItem(defectIncident2, ReleaseRings.Codes.STD, now.AddDays(-5));
			AttachNewCompletedWorkItem(featureIncident1, ReleaseRings.Codes.DPR, now.AddDays(-5));
			AttachNewCompletedWorkItem(featureIncident2, ReleaseRings.Codes.DPR, now.AddDays(-5));

			licenceDefectIncident1.Database.LD_HL_CurrentRunningVersion = oldStdBuild.PK;

			Factory.Save();

			List<SupportIncident> incidents = process.GetIncidentsToSendUpgrade();
			AssertEquals("Count", 4, incidents.Count);
			AssertReporterCounts(0, 0, 0);
			AssertEquals("Reporter.UnprocessedIncidentCount", 0, process.Reporter.UnprocessedIncidentCount);

			Dictionary<ZGuid, List<SupportIncident>> dprIncidents = process.SortIncidentsByClient(incidents, ReleaseRings.Codes.DPR);
			AssertEquals("dprIncidents.Keys.Count", 1, dprIncidents.Keys.Count);
			AssertIncidentsSortedByClientIncidentCount(dprIncidents, featureIncident1.Client, 2);
			AssertIncidentExistsInIncidentsSortedByClient(dprIncidents, featureIncident1);
			AssertIncidentExistsInIncidentsSortedByClient(dprIncidents, featureIncident2);

			Dictionary<ZGuid, List<SupportIncident>> stdIncidents = process.SortIncidentsByClient(incidents, ReleaseRings.Codes.STD);
			AssertEquals("stdIncidents.Keys.Count", 2, stdIncidents.Keys.Count);
			AssertIncidentsSortedByClientIncidentCount(stdIncidents, defectIncident1.Client, 1);
			AssertIncidentsSortedByClientIncidentCount(stdIncidents, defectIncident2.Client, 1);
			AssertIncidentExistsInIncidentsSortedByClient(stdIncidents, defectIncident1);
			AssertIncidentExistsInIncidentsSortedByClient(stdIncidents, defectIncident2);
		}

		public void TestGetIncidentsToSendUpgrade_AllConditionsMet_EntToCW1()
		{
			ZDateTime now = ZDateTime.Now;

			CreateReleaseBuild(ReleaseRings.Codes.ALP, now.AddDays(-2));
			CreateReleaseBuild(ReleaseRings.Codes.DPR, now.AddDays(-3));
			ReleaseBuild newStdBuild = CreateReleaseBuild(ReleaseRings.Codes.STD, now.AddDays(-4));
			ReleaseBuild oldStdBuild = CreateReleaseBuild(ReleaseRings.Codes.STD, now.AddDays(-6));
			newStdBuild.VersionNumber = new VersionNumber(14, 1, 1, 2);
			oldStdBuild.VersionNumber = new VersionNumber(1, 2, 3000, 2);

			SupportIncident supportIncident = CreateIncident(SupportIncidentCategoriesList.Codes.Support, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident defectIncident1 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident defectIncident2 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident featureIncident1 = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident featureIncident2 = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);

			AttachNewClientAndNewLicence_Sql2012(supportIncident, "..1", ReleaseRings.Codes.ALP);
			var licenceDefectIncident1 = AttachNewClientAndNewLicence_Sql2012(defectIncident1, "..2", ReleaseRings.Codes.STD);
			AttachNewClientAndNewLicence_Sql2012(defectIncident2, "..3", ReleaseRings.Codes.STD);
			AttachNewClientAndNewLicence_Sql2012(featureIncident1, "..4", ReleaseRings.Codes.DPR);
			AttachClientAndCompany(featureIncident2, featureIncident1.Client, featureIncident1.ClientCompany);
			AttachNewCompletedWorkItem(defectIncident1, ReleaseRings.Codes.STD, now.AddDays(-5));
			AttachNewCompletedWorkItem(defectIncident2, ReleaseRings.Codes.STD, now.AddDays(-5));
			AttachNewCompletedWorkItem(featureIncident1, ReleaseRings.Codes.DPR, now.AddDays(-5));
			AttachNewCompletedWorkItem(featureIncident2, ReleaseRings.Codes.DPR, now.AddDays(-5));

			licenceDefectIncident1.Database.LD_HL_CurrentRunningVersion = oldStdBuild.PK;

			Factory.Save();

			List<SupportIncident> incidents = process.GetIncidentsToSendUpgrade();
			AssertEquals("Count", 4, incidents.Count);
			AssertReporterCounts(0, 0, 0);
			AssertEquals("Reporter.UnprocessedIncidentCount", 0, process.Reporter.UnprocessedIncidentCount);

			Dictionary<ZGuid, List<SupportIncident>> dprIncidents = process.SortIncidentsByClient(incidents, ReleaseRings.Codes.DPR);
			AssertEquals("dprIncidents.Keys.Count", 1, dprIncidents.Keys.Count);
			AssertIncidentsSortedByClientIncidentCount(dprIncidents, featureIncident1.Client, 2);
			AssertIncidentExistsInIncidentsSortedByClient(dprIncidents, featureIncident1);
			AssertIncidentExistsInIncidentsSortedByClient(dprIncidents, featureIncident2);

			Dictionary<ZGuid, List<SupportIncident>> stdIncidents = process.SortIncidentsByClient(incidents, ReleaseRings.Codes.STD);
			AssertEquals("stdIncidents.Keys.Count", 2, stdIncidents.Keys.Count);
			AssertIncidentsSortedByClientIncidentCount(stdIncidents, defectIncident1.Client, 1);
			AssertIncidentsSortedByClientIncidentCount(stdIncidents, defectIncident2.Client, 1);
			AssertIncidentExistsInIncidentsSortedByClient(stdIncidents, defectIncident1);
			AssertIncidentExistsInIncidentsSortedByClient(stdIncidents, defectIncident2);
		}

		public void TestGetIncidentsToSendUpgrade_AllConditionsMet_CW1ToCW1()
		{
			ZDateTime now = ZDateTime.Now;

			CreateReleaseBuild(ReleaseRings.Codes.ALP, now.AddDays(-2));
			CreateReleaseBuild(ReleaseRings.Codes.DPR, now.AddDays(-3));
			ReleaseBuild newStdBuild = CreateReleaseBuild(ReleaseRings.Codes.STD, now.AddDays(-4));
			ReleaseBuild oldStdBuild = CreateReleaseBuild(ReleaseRings.Codes.STD, now.AddDays(-6));
			newStdBuild.VersionNumber = new VersionNumber(14, 3, 1, 4);
			oldStdBuild.VersionNumber = new VersionNumber(14, 2, 10, 2);

			SupportIncident supportIncident = CreateIncident(SupportIncidentCategoriesList.Codes.Support, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident defectIncident1 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident defectIncident2 = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident featureIncident1 = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			SupportIncident featureIncident2 = CreateIncident(SupportIncidentCategoriesList.Codes.FeatureRequest, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);

			AttachNewClientAndNewLicence_Sql2012(supportIncident, "..1", ReleaseRings.Codes.ALP);
			var licenceDefectIncident1 = AttachNewClientAndNewLicence_Sql2012(defectIncident1, "..2", ReleaseRings.Codes.STD);
			AttachNewClientAndNewLicence_Sql2012(defectIncident2, "..3", ReleaseRings.Codes.STD);
			AttachNewClientAndNewLicence_Sql2012(featureIncident1, "..4", ReleaseRings.Codes.DPR);
			AttachClientAndCompany(featureIncident2, featureIncident1.Client, featureIncident1.ClientCompany);
			AttachNewCompletedWorkItem(defectIncident1, ReleaseRings.Codes.STD, now.AddDays(-5));
			AttachNewCompletedWorkItem(defectIncident2, ReleaseRings.Codes.STD, now.AddDays(-5));
			AttachNewCompletedWorkItem(featureIncident1, ReleaseRings.Codes.DPR, now.AddDays(-5));
			AttachNewCompletedWorkItem(featureIncident2, ReleaseRings.Codes.DPR, now.AddDays(-5));

			licenceDefectIncident1.Database.LD_HL_CurrentRunningVersion = oldStdBuild.PK;

			Factory.Save();

			List<SupportIncident> incidents = process.GetIncidentsToSendUpgrade();
			AssertEquals("Count", 4, incidents.Count);
			AssertReporterCounts(0, 0, 0);
			AssertEquals("Reporter.UnprocessedIncidentCount", 0, process.Reporter.UnprocessedIncidentCount);

			Dictionary<ZGuid, List<SupportIncident>> dprIncidents = process.SortIncidentsByClient(incidents, ReleaseRings.Codes.DPR);
			AssertEquals("dprIncidents.Keys.Count", 1, dprIncidents.Keys.Count);
			AssertIncidentsSortedByClientIncidentCount(dprIncidents, featureIncident1.Client, 2);
			AssertIncidentExistsInIncidentsSortedByClient(dprIncidents, featureIncident1);
			AssertIncidentExistsInIncidentsSortedByClient(dprIncidents, featureIncident2);

			Dictionary<ZGuid, List<SupportIncident>> stdIncidents = process.SortIncidentsByClient(incidents, ReleaseRings.Codes.STD);
			AssertEquals("stdIncidents.Keys.Count", 2, stdIncidents.Keys.Count);
			AssertIncidentsSortedByClientIncidentCount(stdIncidents, defectIncident1.Client, 1);
			AssertIncidentsSortedByClientIncidentCount(stdIncidents, defectIncident2.Client, 1);
			AssertIncidentExistsInIncidentsSortedByClient(stdIncidents, defectIncident1);
			AssertIncidentExistsInIncidentsSortedByClient(stdIncidents, defectIncident2);
		}

		public void TestGetIncidentsToSendUpgrade_ClientInactive()
		{
			SupportIncident incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			var licence = AttachNewClientAndNewLicence(incident, "...", ReleaseRings.Codes.GPR);
			licence.Database.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			incident.Client.OH_IsActive = false;
			Factory.Save();
			AssertEquals("Count", 0, process.GetIncidentsToSendUpgrade().Count);
			AssertReporterCounts(0, 0, 0);
		}

		public void TestGetAllHostedLicenceDatabases()
		{
			var now = ZDateTime.Now;
			SetUpEnvironmentForExecute();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_OH = org.PK;
			enterprise.LE_EnterpriseCode = "EC1";

			var anotherEnterprise = Factory.New<LicenceEnterprise>();
			anotherEnterprise.LE_OH = org.PK;
			anotherEnterprise.LE_EnterpriseCode = "EC2";

			var database = CreateDatabase(enterprise, "AAA", ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GPR, DatabaseTypes.Codes.Test, "SYD");
			var databaseWithAnotherServerCode = CreateDatabase(enterprise, "BBB", ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GPR, DatabaseTypes.Codes.Test, "SYD");
			var databaseWithAnotherEnterprise = CreateDatabase(anotherEnterprise, "AAA", ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GPR, DatabaseTypes.Codes.Test, "SYD");
			var databaseWithAnotherProductType = CreateDatabase(enterprise, "CCC", ProductTypes.Codes.BorderWise, ReleaseRings.Codes.GPR, DatabaseTypes.Codes.Test, "SYD");
			var databaseWithAnotherReleaseRing = CreateDatabase(enterprise, "DDD", ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.ALP, DatabaseTypes.Codes.Test, "SYD");
			var databaseWithAnotherLicenceType = CreateDatabase(enterprise, "EEE", ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GPR, DatabaseTypes.Codes.Training, "SYD");
			var databaseNotHostedWithCargoWise = CreateDatabase(enterprise, "FFF", ProductTypes.Codes.CargoWiseOne, ReleaseRings.Codes.GPR, DatabaseTypes.Codes.Test, "NCW");
			var databaseWithCargoWiseNextProductType = CreateDatabase(enterprise, "GGG", ProductTypes.Codes.CargoWiseNext, ReleaseRings.Codes.GPR, DatabaseTypes.Codes.Test, "SYD");

			var currentBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddDays(-3));
			currentBuild.VersionNumber = new VersionNumber(23, 5, 16, 12);
			database.LD_HL_CurrentRunningVersion = currentBuild.PK;

			var gprBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now);
			gprBuild.VersionNumber = new VersionNumber(23, 5, 16, 20);

			var weeklyBuild = CreateReleaseBuild(ReleaseRings.Codes.GPR, now.AddDays(-7));
			weeklyBuild.VersionNumber = new VersionNumber(23, 5, 16, 10);

			var cr3Incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, IncidentMainLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			cr3Incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			cr3Incident.IM_Description = "CR3 incident";
			cr3Incident.IM_LD = database.PK;
			var cr3Licence = AttachNewClientAndNewLicence(cr3Incident, "EC1", ReleaseRings.Codes.GPR);
			AttachNewCompletedWorkItem(cr3Incident, ReleaseRings.Codes.GPR, now.AddDays(-1));
			var client = (EDIOrgHeader)cr3Incident.Client;
			var contact = CreateContact(client, "contact@client.com");
			cr3Incident.IM_OC_Contact = contact.PK;
			cr3Licence.Database.LD_OC_LicenseeAdminContact = contact.PK;
			cr3Licence.Database.LD_HostedLocation = "SYD";

			var cr4Incident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, IncidentMainLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			cr4Incident.IM_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			cr4Incident.IM_Description = "CR4 incident";
			cr4Incident.IM_LD = database.PK;
			var cr4Licence = AttachNewClientAndNewLicence(cr4Incident, "EC2", ReleaseRings.Codes.GPR);
			AttachNewCompletedWorkItem(cr4Incident, ReleaseRings.Codes.GPR, now.AddDays(-1));
			var client2 = (EDIOrgHeader)cr4Incident.Client;
			var contact2 = CreateContact(client2, "contact2@client.com");
			cr4Incident.IM_OC_Contact = contact2.PK;
			cr4Licence.Database.LD_OC_LicenseeAdminContact = contact2.PK;
			cr4Licence.Database.LD_HostedLocation = "SYD";

			var cr4incidentDatabase = CreateDatabase(cr4Incident.Database.LicEnterprise, "GGG", ProductTypes.Codes.Enterprise, ReleaseRings.Codes.GPR, DatabaseTypes.Codes.Test, "SYD");
			Factory.Save();

			var databases = process.GetAllHostedLicenceDatabases(cr3Incident);
			var databases2 = process.GetAllHostedLicenceDatabases(cr4Incident);
			AssertEquals("databases.Count should be 3.", 3, databases.Count);
			AssertEquals("databases2.Count should be 1.", 1, databases2.Count);

			process.RunTask();

			AssertEquals("createdUpgraders.Count", 1, process.createdUpgraders.Count);
			var gprUpgrader = FindUpgrader(gprBuild, client);
			AssertUpgrader("gprUpgrader", gprUpgrader, cr3Incident);
			AssertEquals("gprUpgrader.Upgrades.Count", 3, gprUpgrader.Upgrades.Count);
			AssertUpgradeRequest("gprUpgrader.Upgrades[0]", FindUpgradeRequest(gprUpgrader, cr3Incident), cr3Incident, contact);
			AssertUpgradeRequest("gprUpgrader.Upgrades[1]", FindUpgradeRequest(gprUpgrader, database), cr3Incident, database, contact);
			AssertUpgradeRequest("gprUpgrader.Upgrades[2]", FindUpgradeRequest(gprUpgrader, databaseWithAnotherServerCode), cr3Incident, databaseWithAnotherServerCode, contact);
			AssertNull("should not find upgradeRequest for databaseWithAnotherEnterprise", FindUpgradeRequest(gprUpgrader, databaseWithAnotherEnterprise));
			AssertNull("should not find upgradeRequest for databaseWithAnotherProductType", FindUpgradeRequest(gprUpgrader, databaseWithAnotherProductType));
			AssertNull("should not find upgradeRequest for databaseWithAnotherReleaseRing", FindUpgradeRequest(gprUpgrader, databaseWithAnotherReleaseRing));
			AssertNull("should not find upgradeRequest for databaseWithAnotherLicenceType", FindUpgradeRequest(gprUpgrader, databaseWithAnotherLicenceType));
			AssertNull("should not find upgradeRequest for databaseNotHostedWithCargoWise", FindUpgradeRequest(gprUpgrader, databaseNotHostedWithCargoWise));
			AssertNull("should not find upgradeRequest for databaseWithCargoWiseNextProductType", FindUpgradeRequest(gprUpgrader, databaseWithCargoWiseNextProductType));
			AssertNull("should not find upgrader for client2", FindUpgrader(gprBuild, client2));
			AssertIsProcessed("cr3Incident", cr3Incident, processed: true, "Upgrade sent");
		}

		public void TestGetIncidentsToSendUpgrade_GlowProduct()
		{
			ZDateTime now = ZDateTime.Now;
			CreateReleaseBuild(ReleaseRings.Codes.ALP, now.AddDays(-2));

			var glowIncident = CreateIncident(SupportIncidentCategoriesList.Codes.Defect, SupportIncidentLookups.Status.Closed, SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade);
			glowIncident.IM_Product = ProductTypes.Codes.GLOW;
			AttachNewClientAndNewLicence(glowIncident, "..1", ReleaseRings.Codes.ALP);
			AttachNewCompletedWorkItem(glowIncident, ReleaseRings.Codes.ALP, now.AddDays(-3));

			Factory.Save();

			var incidents = process.GetIncidentsToSendUpgrade();
			AssertEquals("Count", 1, incidents.Count);
		}

		void AssertIncidentsSortedByClientIncidentCount(Dictionary<ZGuid, List<SupportIncident>> incidents, OrgHeader client, int incidentCount)
		{
			foreach (ZGuid key in incidents.Keys)
			{
				if (client.PK == key)
				{
					AssertEquals("Incidents Count", incidentCount, incidents[key].Count);
					break;
				}
			}
		}

		void AssertIncidentExistsInIncidentsSortedByClient(Dictionary<ZGuid, List<SupportIncident>> incidents, SupportIncident incident)
		{
			bool foundMatch = false;
			foreach (ZGuid key in incidents.Keys)
			{
				if (incident.Client.PK == key)
				{
					foreach (SupportIncident incidentInCollection in incidents[key])
					{
						if (incidentInCollection.PK == incident.PK)
						{
							foundMatch = true;
							break;
						}
					}
				}
			}
			AssertEquals("Incident was not found.", true, foundMatch);
		}

		#endregion

		#region Implementation

		SupportIncident CreateIncident(string stage, string status, string disposition)
		{
			SupportIncident result = Factory.New<SupportIncident>();
			result.IM_Product = ProductTypes.Codes.Enterprise;
			result.IM_Category = stage;
			result.IM_Status = status;
			result.IM_ResolutionCode = disposition;

			return result;
		}

		ReleaseBuild CreateReleaseBuild(string releaseRing, ZDateTime exeDate)
		{
			var result = ReleaseBuild.NewForTesting(Factory, releaseRing, false);
			result.HL_Product = ProductTypes.Codes.Enterprise;
			var httpVersion = new HttpDownload();
			result.HL_MajorVersion = httpVersion.VersionMajorNumber;
			result.HL_MinorVersion = httpVersion.VersionMinorNumber;
			result.HL_Release = httpVersion.VersionReleaseNumber;
			result.HL_ExeVersionDate = exeDate;
			return result;
		}

		ReleaseBuild CreateNewReleaseBuild(int majorVersion, int minorVersion, int release, int patch, bool superceded, string releaseRing, string product, ZDateTime exeDate)
		{
			var releaseBuild = ReleaseBuild.NewForTesting(Factory, releaseRing, superceded);
			releaseBuild.HL_Product = product;
			releaseBuild.HL_MajorVersion = majorVersion;
			releaseBuild.HL_MinorVersion = minorVersion;
			releaseBuild.HL_Release = release;
			releaseBuild.HL_Patch = patch;
			releaseBuild.HL_ExeVersionDate = exeDate;

			return releaseBuild;
		}

		OrgContact CreateContact(OrgHeader client, string emailAddress)
		{
			OrgContact result = client.Contacts.AddNew();
			result.OC_Email = emailAddress;
			result.OC_ContactName = client.Contacts.Count.ToString();
			return result;
		}

		LicenceDatabase CreateDatabase(LicenceEnterprise enterprise, string serverCode, string product, string releaseRing, string licenceType, string hostedLocation)
		{
			var database = Factory.New<LicenceDatabase>();
			database.LD_LE = enterprise.PK;
			database.LD_ServerCode = serverCode;
			database.LD_Product = product;
			database.LD_ReleaseRing = releaseRing;
			database.LD_LicenceType = licenceType;
			database.LD_HostedLocation = hostedLocation;
			return database;
		}

		UpgradeRequestCollectionContainer FindUpgrader(ReleaseBuild build, EDIOrgHeader client)
		{
			if (process.createdUpgraders == null)
			{
				return null;
			}

			foreach (UpgradeRequestCollectionContainer upgrader in process.createdUpgraders)
			{
				if (upgrader.ReleaseBuildPK == build.PK && upgrader.Upgrades[0].OrganisationPk == client.PK)
				{
					return upgrader;
				}
			}
			return null;
		}

		UpgradeRequest FindUpgradeRequest(UpgradeRequestCollectionContainer upgrader, SupportIncident incident)
		{
			ZGuid databasePK = incident.Database.PK;
			foreach (UpgradeRequest upgradeRequest in upgrader.Upgrades)
			{
				if (upgradeRequest.LicDatabase.PK == databasePK)
				{
					return upgradeRequest;
				}
			}
			return null;
		}

		UpgradeRequest FindUpgradeRequest(UpgradeRequestCollectionContainer upgrader, LicenceDatabase database)
		{
			ZGuid databasePK = database.PK;
			foreach (UpgradeRequest upgradeRequest in upgrader.Upgrades)
			{
				if (upgradeRequest.LicDatabase.PK == databasePK)
				{
					return upgradeRequest;
				}
			}
			return null;
		}

		void SetUpEnvironmentForExecute()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "bob@builders.com";
			staff.GS_Code = "ZAC";
			EDIDataRegistry.Instance.IncidentsBatchProcessNotificationGroup = group.PK.ToGuid();
			Factory.Save();
		}

		LicenceHeader AttachNewClientAndNewLicence(SupportIncident incident, string enterpriseCode, string releaseRing)
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			client.LicenceEnterpriseCode = enterpriseCode;
			return AttachClientAndNewLicence(incident, client, releaseRing);
		}

		LicenceHeader AttachNewClientAndNewLicence_Sql2012(SupportIncident incident, string enterpriseCode, string releaseRing)
		{
			EDIOrgHeader client = Factory.NewWithValidTestData<EDIOrgHeader>();
			client.CreateAndLoadLicenceForOrg();
			client.LicenceEnterpriseCode = enterpriseCode;
			return AttachClientAndNewLicence_Sql2012(incident, client, releaseRing);
		}

		LicenceHeader AttachClientAndNewLicence(SupportIncident incident, EDIOrgHeader client, string releaseRing)
		{
			return AttachClientAndNewLicenceCore(incident, client, releaseRing, "Sql2005");
		}

		LicenceHeader AttachClientAndNewLicence_Sql2012(SupportIncident incident, EDIOrgHeader client, string releaseRing)
		{
			var latestSupportedSqlProduct = SqlServerVersionNumber.SupportedVersions.Max().Generation.Name;
			return AttachClientAndNewLicenceCore(incident, client, releaseRing, latestSupportedSqlProduct);
		}

		LicenceHeader AttachClientAndNewLicenceCore(SupportIncident incident, EDIOrgHeader client, string releaseRing, string sqlVersion)
		{
			var databases = client.LicCompany.LicEnterprise.Databases;
			LicenceDatabase database = databases.AddNew();
			client.LicCompany.LicDatabases.Add(database);
			database.LD_ReleaseRing = releaseRing;
			database.LD_ServerCode = databases.Count.ToString().PadLeft(3);
			database.LD_SQLVersion = sqlVersion;
			database.LD_PublicEmailAddressForUpdate = "test@cargowise.com";
			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_LD = database.PK;
			clientCompany.LCC_Code = "DDD";
			clientCompany.LCC_OH = client.PK;
			AttachClientAndCompany(incident, client, clientCompany);
			return client.LicCompany.GetHeader(database);
		}

		void AttachClientAndCompany(SupportIncident incident, OrgHeader client, ClientCompany company)
		{
			incident.IM_OH_Client = client.PK;
			incident.IM_LD = company.LCC_LD;
			incident.IM_LCC = company.PK;
		}

		NewWorkItem AttachNewCompletedWorkItem(SupportIncident incident, string patchTo, ZDateTime closeDate)
		{
			NewWorkItem workItem = AttachNewWorkItem(incident, patchTo, ProcessTaskStatusCodeList.Codes.Closed);
			var task = workItem.WorkflowItems.AddNew();
			task.P9_Type = ReleaseRings.Lookup(patchTo).CheckinTask;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task.CompletedTimeLocal = closeDate;
			return workItem;
		}

		void AttachNewOpenWorkItem(SupportIncident incident, string patchTo)
		{
			AttachNewWorkItem(incident, patchTo, ProcessTaskStatusCodeList.Codes.Open);
		}

		NewWorkItem AttachNewWorkItem(SupportIncident incident, string patchTo, string status)
		{
			NewWorkItem workItem = incident.RelatedWorkItems.AddNew();
			workItem.FillWithValidTestData();
			workItem.WKI_Priority = patchTo;
			workItem.WKI_Status = status;
			return workItem;
		}

		void AssertIncidentPostUpgradeStatus(string id, ZDateTime now, BusinessObjectFactory factory, SupportIncident incident, bool quoteIsCreated = true)
		{
			SupportIncident loadedIncident = factory.Load<SupportIncident>(incident.PK);
			AssertEquals(id + "." + IncidentMainSchema.IM_ResolutionCode.Name, SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, loadedIncident.IM_ResolutionCode);
			AssertEquals(id + ".IM_BugFixedDeployed", now, incident.IM_BugFixDeployed);
			if (quoteIsCreated)
			{
				AssertEquals(id + ".CIQ_DeliveredDateUTC", Env.Time.GetUtcFromLocalTime(now.ToDateTime()), incident.Quote.CIQ_DeliveredDateUTC);
			}
			else
			{
				AssertNull("quote should not be created", incident.Factory.LoadTop1<ClientIncidentQuote>(new ZQuery(ClientIncidentQuoteSchema.CIQ_IM, incident.PK)));
			}
		}

		void AssertUpgrader(string id, UpgradeRequestCollectionContainer upgrader, params SupportIncident[] incidents)
		{
			AssertEquals(id + ".IsSendViaDefault", true, upgrader.IsSendViaDefault);
			AssertEquals(id + ".SendEmailNotificationAutomatically", true, upgrader.SendEmailNotificationAutomatically);

			StringBuilder expectedAdditionalNotification = new StringBuilder("The upgrade resolves the following incidents:\r\n\r\n");
			List<string> incidentDetails = new List<string>();
			foreach (SupportIncident incident in incidents)
			{
				incidentDetails.Add(incident.IM_IncidentNumber + " - " + incident.IM_Description);
			}
			incidentDetails.Sort();
			foreach (string incidentDetailsLine in incidentDetails)
			{
				expectedAdditionalNotification.AppendLine(incidentDetailsLine);
			}
			AssertEquals(id + ".AdditionalNotification", expectedAdditionalNotification.ToString().TrimEnd(), upgrader.AdditionalNotification);
		}

		void AssertUpgradeRequest(string id, UpgradeRequest upgradeRequest, SupportIncident incident, OrgContact contact)
		{
			AssertEquals(id + ".Organisation.PK", incident.Client.PK, upgradeRequest.OrganisationPk);
			AssertEquals(id + ".LicDatabase.PK", incident.Database.PK, upgradeRequest.LicDatabase.PK);
			AssertEquals(id + ".ClientContactPK", contact.PK, upgradeRequest.ClientContactPK);
		}

		void AssertUpgradeRequest(string id, UpgradeRequest upgradeRequest, SupportIncident incident, LicenceDatabase database, OrgContact contact)
		{
			AssertEquals(id + ".Organisation.PK", incident.Client.PK, upgradeRequest.OrganisationPk);
			AssertEquals(id + ".LicDatabase.PK", database.PK, upgradeRequest.LicDatabase.PK);
			AssertEquals(id + ".ClientContactPK", contact.PK, upgradeRequest.ClientContactPK);
		}

		void AssertIsProcessed(SupportIncident incident, bool processed, string reason)
		{
			AssertIsProcessed("incident", incident, processed, reason);
		}

		void AssertIsProcessed(string id, SupportIncident incident, bool processed, string reason)
		{
			string actualReason;
			AssertEquals("Reporter.IsIncidentProcessed(" + id + ".IM_IncidentNumber)", processed, process.Reporter.IsIncidentProcessed(incident, out actualReason));
			AssertEquals("Reporter.IsIncidentProcessed(" + id + ".IM_IncidentNumber) reason", reason, actualReason);
		}

		void AssertReporterCounts(int processed, int unprocessed, int errors)
		{
			AssertEquals("Reporter.ProcessedIncidentCount", processed, process.Reporter.ProcessedIncidentCount);
			AssertEquals("Reporter.UnprocessedIncidentCount", unprocessed, process.Reporter.UnprocessedIncidentCount);
			AssertEquals("Reporter.ErrorCount", errors, process.Reporter.ErrorCount);
		}

		void AssertHasEConversationMessage(string expectedMessage, JobConversation eConversation) => AssertHasEConversationMessage(string.Empty, expectedMessage, eConversation);

		void AssertHasEConversationMessage(string message, string expectedMessage, JobConversation eConversation)
		{
			var hasExpectedMessage = eConversation.GetTimeOrderedMessages().Any(x => x.Body == expectedMessage);
			if (!hasExpectedMessage)
			{
				var messageList = eConversation.GetTimeOrderedMessages().Select(x => $"[{x.SendLocalDateTime}]{x.SenderCode}: {x.Body}");
				Assert(@$"{message}
The expected message cannot be found:
**Expected**:
{expectedMessage}
**Message List**:
{string.Join(System.Environment.NewLine, messageList)}", condition: false);
			}
		}

		protected override void SetUpCore()
		{
			ReleaseBuildContentForLegacyTest.Enable();
			process = new IncidentBasedAutoDeploymentServiceTaskForTest();
			logger = new TestServiceLogger();
			process.ServiceLogger = logger;
		}

		TestServiceLogger logger;

		IncidentBasedAutoDeploymentServiceTaskForTest process;

		class IncidentBasedAutoDeploymentServiceTaskForTest : IncidentBasedAutoDeploymentServiceTask
		{
			readonly Dictionary<ZGuid, bool> needUpgrades = new Dictionary<ZGuid, bool>();

			public void AddNeedUpgrade(ZGuid incidentPK, bool needUpgrade)
			{
				needUpgrades.Add(incidentPK, needUpgrade);
			}

			protected override SupportIncident[] LoadIncidents(ZQuery filter)
			{
				var incidents = Factory.Load<SupportIncidentForTest>(filter);
				foreach (var incident in incidents)
				{
					bool value;
					if (needUpgrades.TryGetValue(incident.PK, out value))
					{
						incident.NeedUpgradeOverride = value;
					}
				}

				return incidents;
			}
		}

		class SupportIncidentForTest : SupportIncident
		{
			public SupportIncidentForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool NeedUpgradeOverride = true;

			public override bool NeedUpgrade => NeedUpgradeOverride;
		}

		class IncidentBasedAutoDeploymentServiceTaskForConcurrencyTest : IncidentBasedAutoDeploymentServiceTaskForTest
		{
			bool throwConcurrencyException = true;
			public int CallsToProcessIncidentInAnotherFactoryWithoutRetry;

			protected override void ProcessIncidentInAnotherFactoryWithoutRetry(SupportIncident incidentInMainFactory, string eConversationMessage, bool awaitingDeployment = false)
			{
				++CallsToProcessIncidentInAnotherFactoryWithoutRetry;
				if (throwConcurrencyException)
				{
					throwConcurrencyException = false;
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new InvalidOperationException("Simulated Concurrency exception"),
						((IBusinessObjectInternals)incidentInMainFactory).Row, ((IDbConnected)Factory).Connection), Factory);
				}
				base.ProcessIncidentInAnotherFactoryWithoutRetry(incidentInMainFactory, eConversationMessage, awaitingDeployment);
			}
		}

		#endregion
	}
}
