using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(IncidentManagementGroupForm))]
	public class IncidentManagementGroupFormTest : ZFormBasherTest
	{
		protected override bool AllowHasChangesOnFormOpen => true;

		protected override Form GetFormToBashCore()
		{
			var incidentGroup = Factory.New<IncidentManagementGroup>();
			return new IncidentManagementGroupForm(incidentGroup);
		}

		public void TestFormCaption()
		{
			using (var form = GetFormToBashCore() as IncidentManagementGroupForm)
			{
				AssertEquals("Incident Management Group", form.FormCaption);
				form.IncidentManagementGroup.ING_IncidentGroupNumber = "G001";
				form.IncidentManagementGroup.ING_Description = "INC Group 123";
				AssertEquals("G001 - INC Group 123", form.FormCaption);
			}
		}

		public void TestRelatedItemsTabShouldContainUnifiedControlsForRelatedItems()
		{
			using (var form = (IncidentManagementGroupForm)GetFormToBashCore())
			{
				form.Show();
				var relatedTab = form.TopLevelTabControl_Exposed.GetTabPageByNameOrText("Related Items");
				AssertNotNull(relatedTab);
				form.TopLevelTabControl_Exposed.SelectedTab = relatedTab;
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("RelatedItemGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("NetworkDiagramGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("ParentWorkflowGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("ChildWorkflowGroupBox"));
			}
		}

		public void TestFormHasPlugIns()
		{
			var incidentGroup = Factory.New<IncidentManagementGroup>();
			using (var form = new IncidentManagementGroupForm(incidentGroup))
			{
				AssertNotNull("The form should contain the Documents PlugIn", form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				AssertNotNull("The form should contain the Documents PlugIn", form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertEquals("The form should hide the notes tab page", false, form.GetType().GetProperty("ShowNotesTab", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form));
			}
		}

		public void TestFormLoad_DbHits()
		{
			var wi1 = Factory.NewWithValidTestData<WorkItem>();
			wi1.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			wi1.WKI_WorkItemNumber = "WI00NTZ01";
			var task1 = wi1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org.PK;
			incident1.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org.PK;
			incident2.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;

			var incidentGroup = Factory.New<IncidentManagementGroup>();
			incidentGroup.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			incidentGroup.NowStage.ControlIncidents = true;
			incidentGroup.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			incidentGroup.AutoCascadeRelatedItems.Add(wi1);

			Factory.Save();

			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = incidentGroup.PK;
			link1.INL_IsGroupControlled = true;
			var link2 = Factory.New<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = incidentGroup.PK;
			link2.INL_IsGroupControlled = true;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var incidentGroupReloaded = newFactory.Load<IncidentManagementGroup>(incidentGroup.PK);

			using (var form = new IncidentManagementGroupForm(incidentGroupReloaded))
			{
				newFactory.ResetDatabaseLoadCount();

				using (AssertDbHitsWithUsefulQueryInformation(new Dictionary<string, int>
				{
					{ ProcessTasksSchema.Constants.TableName, 1 },
					{ JobConversationSchema.Constants.TableName, 4 },
					{ JobConversationMessageSchema.Constants.TableName, 1 },
					{ StmNoteSchema.Constants.TableName, 1 },
					{ GenCustomAddOnValueSchema.Constants.TableName, 1 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
#if WINZOR
					{ RefCountrySchema.Constants.TableName, 1 },
#endif
				}, newFactory))
				{
					form.Show();
					Application.DoEvents();
				}
			}
		}

		public void TestIncidentManagementGroupWithInvalidStageShouldPopupInvalidStageForm()
		{
			var incidentGroup = Factory.New<IncidentManagementGroup>();
			incidentGroup.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			incidentGroup.ING_Status = "INV";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var currentStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			currentStage.Enabled = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var incidentGroupReloaded = newFactory.Load<IncidentManagementGroup>(incidentGroup.PK);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			using (var form = new IncidentManagementGroupFormTestForm(incidentGroupReloaded))
			{
				form.Show();
				form.OnShownForTest(EventArgs.Empty);
				AssertEquals(typeof(InvalidStagePopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestIncidentManagementGroupWithValidStageShouldNotPopupInvalidStageForm()
		{
			var incidentGroup = Factory.New<IncidentManagementGroup>();
			incidentGroup.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			incidentGroup.ING_Status = "INV";

			Factory.Save();

			using (var form = new IncidentManagementGroupFormTestForm(incidentGroup))
			{
				form.Show();
				form.OnShownForTest(EventArgs.Empty);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestIncidentManagementGroupShouldNotChangeWhenCancelThePopupForm()
		{
			var incidentGroup = Factory.New<IncidentManagementGroup>();
			incidentGroup.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			incidentGroup.ING_Status = "INV";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var currentStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			currentStage.Enabled = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			using (var form = new IncidentManagementGroupFormTestForm(incidentGroup))
			{
				form.Show();
				form.OnShownForTest(EventArgs.Empty);
				InvalidStagePopupForm popup = (InvalidStagePopupForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals(typeof(InvalidStagePopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("IncidentManagementGroup status should not change", incidentGroup.ING_Status, "INV");
			}
		}

		class IncidentManagementGroupFormTestForm : IncidentManagementGroupForm
		{
			public IncidentManagementGroupFormTestForm(IncidentManagementGroup incidentManagementGroup) : base(incidentManagementGroup)
			{
			}

			public void OnShownForTest(EventArgs e)
			{
				base.OnShown(e);
			}
		}
	}
}
