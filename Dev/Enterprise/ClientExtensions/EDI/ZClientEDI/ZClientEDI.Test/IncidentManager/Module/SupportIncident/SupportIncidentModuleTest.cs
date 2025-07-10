using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(SupportIncidentModule))]
	public class SupportIncidentModuleTest : ZModuleBasherWithFetchHintsTest
	{
		public void TestNewMenuItems()
		{
			#region Test Data
			EDIOrgHeader ediorg = Factory.NewWithValidTestData<EDIOrgHeader>();
			ediorg.OH_Code = "CAREDISYDX";
			LicenceEnterprise enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "AAA";
			LicenceCompany company = Factory.NewWithValidTestData<LicenceCompany>();
			company.LC_LE = enterprise.PK;
			company.LC_CompanyCode = "BBB";
			company.LC_OH = ediorg.PK;
			LicenceDatabase database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			database1.LD_ServerCode = "XXX";
			database1.LD_LE = enterprise.PK;
			LicenceHeader licHeader1 = Factory.NewWithValidTestData<LicenceHeader>();
			licHeader1.LA_LD = database1.PK;
			licHeader1.LA_LC = company.PK;
			ClientCompany clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_Code = company.LC_CompanyCode;
			clientCompany1.LCC_LD = database1.PK;
			clientCompany1.LCC_OH = ediorg.PK;
			LicenceDatabase database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			database2.LD_ServerCode = "YYY";
			database2.LD_LE = enterprise.PK;
			LicenceHeader licHeader2 = Factory.NewWithValidTestData<LicenceHeader>();
			licHeader2.LA_LD = database2.PK;
			licHeader2.LA_LC = company.PK;
			ClientCompany clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_Code = company.LC_CompanyCode;
			clientCompany2.LCC_LD = database2.PK;
			clientCompany2.LCC_OH = ediorg.PK;
			GlbStaff.CurrentUser.GS_EmailAddress = "developer@cargowise.com";
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Samuel";
			contact.OC_Email = "developer@cargowise.com";
			contact.OC_OH = ediorg.PK;
			Factory.Save();
			InternalIncidentLicenceSettings licenceForInternalIncident = new InternalIncidentLicenceSettings(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			LicenceEnterpriseKey internalEnterprise = licenceForInternalIncident.LicenceEnterpriseKeys.AddNew();
			internalEnterprise.LE_PK = enterprise.PK;
			licenceForInternalIncident.EdiProd_LicencePK = licHeader1.PK;
			licenceForInternalIncident.UAT_ALP_LicencePK = licHeader2.PK;
			licenceForInternalIncident.UAT_DPR_LicencePK = licHeader2.PK;
			licenceForInternalIncident.UAT_STD_LicencePK = licHeader2.PK;
			licenceForInternalIncident.UAT_GPC_LicencePK = licHeader2.PK;
			licenceForInternalIncident.UAT_GPR_LicencePK = licHeader2.PK;
			EDIDataRegistry.Instance.InternalIncidentLicenceSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, licenceForInternalIncident);
			#endregion
			using (ZForm testForm = new ZForm())
			using (SupportIncidentModule mod = new SupportIncidentModule())
			{
				mod.SetFormsModalTo(testForm);
				AssertNotNull(mod.ToolBarButtons.FindByText("New"));
				AssertEquals(2, mod.NewMenuItem.MenuItems.Count);
				MenuItem custSvcIncidentMenuItem = mod.NewMenuItem.MenuItems[0];
				MenuItem internalIncidentMenuItem = mod.NewMenuItem.MenuItems[1];
				AssertEquals("Customer Service Incident", custSvcIncidentMenuItem.Text);
				AssertEquals("Internal Incident", internalIncidentMenuItem.Text);
				custSvcIncidentMenuItem.PerformClick();
				var form = ZFormModaliser.ActiveForm as SupportIncidentForm;
				AssertEquals(0, ((SupportIncidentForm)ZFormModaliser.ActiveForm).TopLevelTabControl_Exposed.SelectedIndex);
				form.Dispose();
				internalIncidentMenuItem.PerformClick();
				Application.DoEvents();
				form = ZFormModaliser.ActiveForm as SupportIncidentForm;
				AssertEquals("Main tab", 0, form.TopLevelTabControl_Exposed.SelectedIndex);
				var incident = form.BusinessEntity;
				AssertEquals("CAREDISYDX", incident.Client.OH_Code);
				AssertEquals("Samuel", incident.Contact.OC_ContactName);
				AssertEquals("EdiProd licence", "AAABBBXXX", incident.ClientCompany.LicenceCode);
				AssertEquals(SupportIncidentCategoriesList.Codes.Support, incident.IM_Category);
				AssertEquals("Should not be escalated to be a query", false, incident.EConversation.AnyLocalMessageContains("Passed as Query"));
				form.Dispose();
			}
		}

		public void TestAllowDelete()
		{
			using (SupportIncidentModule mod = new SupportIncidentModule())
			{
				AssertEquals("Can't delete customer service incidents", false, mod.AllowDelete);
			}
		}

		public void TestShouldSendERequestDocument()
		{
			using (ZForm testForm = new ZForm())
			using (SupportIncidentModule mod = new SupportIncidentModule())
			{
				EDIDataRegistry.Instance.GlowNewInternalIncident.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				mod.SetFormsModalTo(testForm);
				AssertNotNull(mod.ToolBarButtons.FindByText("New"));
				AssertEquals(2, mod.NewMenuItem.MenuItems.Count);
				MenuItem custSvcIncidentMenuItem = mod.NewMenuItem.MenuItems[0];
				MenuItem internalIncidentMenuItem = mod.NewMenuItem.MenuItems[1];
				AssertEquals("Customer Service Incident", custSvcIncidentMenuItem.Text);
				AssertEquals("Internal Incident", internalIncidentMenuItem.Text);
				internalIncidentMenuItem.PerformClick();
				Application.DoEvents();
				AssertEquals(false, Factory.Load<EDIInterchange>(new ZQuery()).Any());
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.SupportIncident;
		}

		#region Fetch hint db hits test

		void AddTestDataWithRelatedObjects(BusinessObjectFactory factory, bool alwaysPopulate)
		{
			var system = BMSTestHelper.CreateSystem(factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			for (var i = 0; i < 6; i++)
			{
				var shouldPopulateRelatedObject = alwaysPopulate || (i % 2 == 0);
				var incident = factory.New<SupportIncident>();

				#region Client

				var org = factory.New<OrgHeader>();
				org.OH_Code = $"ClientOrg{i}";
				org.OH_FullName = $"ntorg{i}";
				org.OH_RL_NKClosestPort = "AUSYD";

				incident.IM_OH_Client = org.PK;

				#endregion

				#region Client+MiscServ

				if (shouldPopulateRelatedObject)
				{
					org.MiscServ.OM_CMClientSize = "AAA";
				}

				#endregion

				#region Contact

				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = $"OCC{i}";
				contact.OC_Email = $"AOCCC{i}@123.com";
				incident.IM_OC_Contact = contact.PK;

				#endregion

				#region DataBase / DatabaseOrPrimaryProductionSystem

				var releaseBuild = factory.New<ReleaseBuild>();
				releaseBuild.HL_ExeVersionDate = DateTime.Now.AddDays(-1);
				var orgForDataBase = factory.New<OrgHeader>();
				orgForDataBase.OH_Code = $"ORGDB{i}";
				var licenceEnterprise = factory.New<LicenceEnterprise>();
				licenceEnterprise.LE_OH = orgForDataBase.PK;
				licenceEnterprise.LE_EnterpriseCode = (100 + i).ToString();

				if (shouldPopulateRelatedObject)
				{
					var database = factory.New<LicenceDatabase>();

					database.LD_LE = licenceEnterprise.PK;
					database.LD_HL_CurrentRunningVersion = releaseBuild.PK;
					database.LD_ServerCode = $"D{i}";
					database.LD_HostServerName = "localhost";
					database.LD_ReleaseRing = "ALP";
					database.LD_HL_CurrentRunningVersion = releaseBuild.PK;
					database.LD_HostedLocation = "SYD";

					incident.IM_LD = database.PK;
				}
				else
				{
					var orgForProductionSystem = factory.New<OrgHeader>();
					orgForProductionSystem.OH_Code = $"ORGPS{i}";
					orgForProductionSystem.OH_FullName = $"ORGPS{i}";

					incident.IM_LD = ZGuid.Empty;
					incident.IM_OH_Client = orgForProductionSystem.PK;
					incident.IM_Product = "ENT";

					var databaseForProductionSystem = factory.New<LicenceDatabase>();
					databaseForProductionSystem.LD_LicenceType = DatabaseTypes.Codes.Production;
					databaseForProductionSystem.LD_Product = incident.IM_Product;
					databaseForProductionSystem.LD_OH_WebAccessOrg = incident.IM_OH_Client;

					databaseForProductionSystem.LD_LE = licenceEnterprise.PK;
					databaseForProductionSystem.LD_HL_CurrentRunningVersion = releaseBuild.PK;
					databaseForProductionSystem.LD_ServerCode = $"P{i}";
					databaseForProductionSystem.LD_HostServerName = "localhost";
					databaseForProductionSystem.LD_ReleaseRing = "ALP";
					databaseForProductionSystem.LD_HL_CurrentRunningVersion = releaseBuild.PK;
					databaseForProductionSystem.LD_HostedLocation = "SYD";
				}

				#endregion

				#region WorkFlow

				var staffForWorkFlow = CreateStaff($"W{i}");

				var capabilityForWorkFlow = factory.New<GlbCapability>();
				capabilityForWorkFlow.G4_Code = $"G{i}";
				capabilityForWorkFlow.G4_Description = "Description_G4";

				var task1 = incident.WorkflowItems.AddNew();
				var task2 = incident.WorkflowItems.AddNew();

				task1.CapabilityCode = capabilityForWorkFlow.G4_Code;
				task2.CapabilityCode = ZString.Empty;
				task1.P9_Description = "Description";
				task2.P9_Description = "Description";
				task1.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(-3));
				task1.P9_ScheduledDateForBinding = ZDateTimeOffset.Now.AddDays(-4);
				task1.P9_GS_NKAssignedStaffMember = staffForWorkFlow.GS_Code;
				task2.P9_GS_NKAssignedStaffMember = staffForWorkFlow.GS_Code;
				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				var processHeader = BMSTestHelper.CreateJobHeader<SupportIncident>(Factory, addDefaultProcessHeaderIfNone: false);
				processHeader.FH_ParentId = incident.PK;
				var workflow = BMSTestHelper.CreateWorkflow(processHeader, "workflow");
				workflow.FH_ParentId = incident.PK;

				#endregion

				#region RelatedItems

				if (shouldPopulateRelatedObject)
				{
					var workItem = factory.New<NewWorkItem>();
					workItem.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
					workItem.WKI_WorkItemNumber = $"WI00NTZ0{i}";
					var wiTask = workItem.WorkflowItems.Tasks.AddNew();
					wiTask.P9_Description = $"Task {i}";
					wiTask.P9_Type = "UDF";
					wiTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
					wiTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
					wiTask.P9_Sequence = 1;

					var staffForProject = CreateStaff($"P{i}");

					var project = factory.New<EDIProject>();
					project.WKP_ProjectNumber = $"PJ000000{i}";
					project.WKP_GS_NKProjectManager = staffForProject.GS_Code;

					var project2 = factory.New<EDIProject>();
					project2.WKP_ProjectNumber = $"PJ100000{i}";
					project2.WKP_GS_NKProjectManager = ZString.Empty;

					var group = Factory.New<IncidentManagementGroup>();
					var issue = Factory.New<EdiHelpErrorLog>();
					var opportunity = Factory.New<OrgOpportunity>();

					incident.RelatedItems.Add(workItem);
					incident.RelatedItems.Add(project);
					incident.RelatedItems.Add(group);
					incident.RelatedItems.Add(issue);
					incident.RelatedItems.Add(opportunity);
				}

				#endregion

				#region FK GlbStaff

				if (shouldPopulateRelatedObject)
				{
					var staffForServiceContact = CreateStaff($"S{i}");
					var staffForAssignedToCurrent = CreateStaff($"A{i}");
					var staffForDefectCausedBy = CreateStaff($"D{i}");

					incident.IM_GS_NKAssignedToCurrent = staffForAssignedToCurrent.GS_Code;
					incident.IM_GS_NKCustServiceContact = staffForServiceContact.GS_Code;
					incident.IM_GS_NKCustDefectCausedBy = staffForDefectCausedBy.GS_Code;
				}

				#endregion

				#region EstimateForBinding

				if (shouldPopulateRelatedObject)
				{
					var estimate = factory.New<ClientIncidentEstimate>();
					estimate.CIE_IM = incident.PK;
					estimate.CIE_EstimateSentDateLocal = new ZDateTime(2000, 1, 1);
					estimate.CIE_EstimateExpiryDateLocal = new ZDateTime(2000, 1, 10);
					estimate.CIE_QuoteRequestedLocal = new ZDateTime(2000, 1, 4);
				}

				#endregion

				#region QuoteForBinding

				if (shouldPopulateRelatedObject)
				{
					var quote = factory.New<ClientIncidentQuote>();
					quote.CIQ_IM = incident.PK;
					quote.CIQ_QuoteSentDateLocal = new ZDateTime(2000, 1, 1);
					quote.CIQ_QuoteExpiryDateLocal = new ZDateTime(2000, 1, 10);
					quote.CIQ_QuoteAcceptedDateLocal = new ZDateTime(2000, 1, 4);
				}

				#endregion

				#region Triage
				var triage = factory.NewWithValidTestData<IncidentTriage>();
				triage.IMT_TriageNumber = $"TR00{i}";
				triage.IMT_SupportDescription = $"TriageSupportDescription{i}";
				incident.IM_IMT_Triage = triage.PK;
				#endregion

				incident.IM_Status = "CLS";
				incident.IM_CloseTimeUtc = ZDateTime.Now;

				incident.Request.INC_ClientReference = $"222{i}";
				incident.Request.INC_IncidentNumber = $"222{i}";

				incident.IM_Language = "CN";
				incident.IM_Description = $"SUMMARY {i}";
				incident.DetailNoteText = $"DETAIL {i}";
				incident.IncidentComment = $"COMMENT {i}";
				incident.IM_ClientBugSeverity = "LOW";
				incident.IM_ActualHoursWorked = new ZDecimal(10);
				incident.IM_QuoteAmount = new ZDecimal(1);
				incident.IM_ResolutionCode = "CLS";
				incident.IM_ClosureResolution = "DUP";
				incident.IM_InstallDate = ZDateTime.Now;
				incident.IM_PlannedInstall = ZDateTime.Now;
				incident.IM_ResolveTimeUtc = ZDateTime.Now;

				incident.IM_Priority = "CR5";
				incident.IM_Product = "ENT";
				incident.IM_ProgramArea = "ARC";
				incident.IM_Module = "CEC";
				incident.IM_SourceModuleId = $"INC";
				incident.IM_RN_NKCountry = "AU";
				incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
				incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
				incident.IM_ServiceStatus = AutoEvents.ServiceSuspendedCode;
			}

			GlbStaff CreateStaff(string code)
			{
				var quickCreatedStaff = factory.New<GlbStaff>();
				quickCreatedStaff.GS_Code = code;
				quickCreatedStaff.GS_FullName = $"NTZ{code}";
				quickCreatedStaff.GS_EmailAddress = $"NTZ{code}@123.com";
				quickCreatedStaff.GS_LoginName = quickCreatedStaff.GS_Code;

				return quickCreatedStaff;
			}
		}

		void AddTestDataWithoutRelatedObjects(BusinessObjectFactory factory)
		{
			var group = factory.New<IncidentManagementGroup>();
			group.ING_IncidentGroupNumber = "ING000001";
			group.ING_Description = "ManagementGroupDescription";
			for (var i = 0; i < 6; i++)
			{
				var incident = factory.New<SupportIncident>();

				var link = group.LinkedIncidents.AddNew();
				link.INL_IM_Incident = incident.PK;
				link.INL_ING_Group = group.PK;

				incident.IM_Status = "CLS";
				incident.IM_CloseTimeUtc = ZDateTime.Now;

				incident.Request.INC_ClientReference = $"111{i}";
				incident.Request.INC_IncidentNumber = $"111{i}";

				incident.IM_Language = "CN";
				incident.IM_Description = $"SUMMARY {i}";
				incident.DetailNoteText = $"DETAIL {i}";
				incident.IncidentComment = $"COMMENT {i}";
				incident.IM_ClientBugSeverity = "LOW";
				incident.IM_ActualHoursWorked = new ZDecimal(10);
				incident.IM_QuoteAmount = new ZDecimal(1);
				incident.IM_ResolutionCode = "CLS";
				incident.IM_ClosureResolution = "DUP";
				incident.IM_InstallDate = ZDateTime.Now;
				incident.IM_PlannedInstall = ZDateTime.Now;
				incident.IM_ResolveTimeUtc = ZDateTime.Now;

				incident.IM_Priority = "CR8";
				incident.IM_Product = "ENT";
				incident.IM_ProgramArea = "ARC";
				incident.IM_Module = "CEC";
				incident.IM_SourceModuleId = $"INC";
				incident.IM_RN_NKCountry = "AU";
				incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
				incident.IM_Source = SupportIncidentLookups.SourceListConstants.ERequestPortal;
				incident.IM_ServiceStatus = AutoEvents.ServiceSuspendedCode;
			}
		}

		[TestDate(2023, 07, 07)]
		public void TestAllColumnsDbHits()
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			AddTestDataWithRelatedObjects(factory, alwaysPopulate: false);
			factory.Save();

			var expectedDBHits = new Dictionary<string, int>
			{
				{ IncidentMainSchema.Constants.TableName, 1 },
				{ GenPivotSchema.Constants.TableName, 1 },
				{ WorkItemSchema.Constants.TableName, 1 },
				{ WorkProjectSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 2 },	// From add fetch hint method
				{ ProcessHeaderSchema.Constants.TableName, 1 },
				{ GlbCapabilitySchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 2 },	// From add fetch hint method - P9_GS_NKAssignedStaffMember, IM_SystemCreateUser
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ IncidentRequestSchema.Constants.TableName, 1 },
				{ OrgContactSchema.Constants.TableName, 1 },
				{ LicenceDatabaseSchema.Constants.TableName, 2 },	// from add fetch hint method - IM_LD is not empty and is empty
				{ ClientIncidentEstimateSchema.Constants.TableName, 1 },
				{ ClientIncidentQuoteSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 1 },
				{ LicenceEnterpriseSchema.Constants.TableName, 1 },
				{ HelpErrorLogSchema.Constants.TableName, 1 },
				{ IncidentManagementGroupSchema.Constants.TableName, 1 },
				{ ReleaseBuildSchema.Constants.TableName, 1 },
				{ IncidentTriageSchema.Constants.TableName, 1 },
				{ IncidentManagementLinkSchema.Constants.TableName, 1 },
			};

			var searchFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			using (var module = new SupportIncidentModuleForTest() { FactoryForDbHitsTest = searchFactory })
			using (var form = new ZForm())
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, searchFactory))
			{
				var filterControl = (ZFilterStripControl)module.EmbeddedControl;
				filterControl.Dock = DockStyle.Fill;
				form.Controls.Add(filterControl);
				form.Show();
				var filteredGrid = filterControl.FilteredGrid;
				filteredGrid.SetAllColumnsVisible(true);
				foreach (var col in filteredGrid.Columns)
				{
					col.ColumnStyle.Width = 1;
				}

				var moduleForTesting = (IFilterModuleInternalsForTesting)module;
				moduleForTesting.FilterBusinessObject.ResetToDefaultValues();
				moduleForTesting.PerformSearch();
				Application.DoEvents();

				var collection = moduleForTesting.GridCollection;
				AssertNotEquals(0, collection.Count);
				AssertEquals("All rows should be visible", collection.Count, filteredGrid.VisibleRowCount);
				AssertEquals("All columns should be visible", filteredGrid.Columns.Count, filteredGrid.VisibleColumnCount);
			}
		}

		protected override void SetupDataForFetchHintsTest()
		{
			Factory.CleanUp();

			var factory = new BusinessObjectFactory();
			AddTestDataWithRelatedObjects(factory, alwaysPopulate: true);
			AddTestDataWithoutRelatedObjects(factory);
			factory.Save();
			
			Factory.ResetDatabaseLoadCount();
		}

		protected override ZFilterModule CreateModuleForFetchHintsTest() => new SupportIncidentModuleForTest();

		class SupportIncidentModuleForTest : SupportIncidentModule
		{
			public BusinessObjectFactory FactoryForDbHitsTest { get; set; }

			protected override BusinessObjectFactory GetNewFactory()
			{
				return FactoryForDbHitsTest ?? base.GetNewFactory();
			}
		}

		#endregion
	}
}
