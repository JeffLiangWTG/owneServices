using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI.Test
{
	class BMComponentsUserControlTest : BMSTestCaseWithFactory
	{
		#region Filter Rules

		public void TestFilterStripsLabel_ShouldUpdateWhenSwitchingComponents()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1", sequence: 1);
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2", sequence: 2);
			var bucket3 = BMSTestHelper.CreateBucket(system, "bucket3", sequence: 3);
			var bucket4 = BMSTestHelper.CreateBucket(system, "bucket4", sequence: 4);

			var link1_2 = BMSTestHelper.LinkComponents(bucket1, bucket2, sequence: 1);
			var link2_3 = BMSTestHelper.LinkComponents(bucket2, bucket3, sequence: 1);
			var link3_4 = BMSTestHelper.LinkComponents(bucket3, bucket4, sequence: 1);
			var link4_2 = BMSTestHelper.LinkComponents(bucket4, bucket2, sequence: 1);
			var link4_3 = BMSTestHelper.LinkComponents(bucket4, bucket3, sequence: 2);

			var filter1 = link1_2.FilterRule;
			var filter2 = link2_3.FilterRule;
			var filter3 = link3_4.FilterRule;
			var filter4 = link4_2.FilterRule;
			var filter5 = link4_3.FilterRule;

			FilterStripsTestHelper.AddFilterStrips(filter1, new FilterStripsTestHelper.FilterStripDefinition { });
			FilterStripsTestHelper.AddFilterStrips(filter2, new FilterStripsTestHelper.FilterStripDefinition { });
			FilterStripsTestHelper.AddFilterStrips(filter3, new FilterStripsTestHelper.FilterStripDefinition { });
			FilterStripsTestHelper.AddFilterStrips(filter4, new FilterStripsTestHelper.FilterStripDefinition { });
			FilterStripsTestHelper.AddFilterStrips(filter5, new FilterStripsTestHelper.FilterStripDefinition { });

			Factory.Save();

			using (var form = (BMSystemManagementForm)ZControllerFactory.Create(ControllerIDs.BMSystems).ShowEditForm(system))
			{
				var control = form.FindAll<BMComponentsUserControl>().Single();
				AssertNotNull(control);

				var filterStripsLabel = control.FindAll<ZLabel>().Single(l => l.Name == "FilterStripsLabelWithBinding");
				AssertNotNull(filterStripsLabel);
				control.BMComponentGrid.CurrentRowIndex = 0;
				AssertEquals("Workflows in the Component [bucket1] will move to the Component [bucket2] when they match the below filters:", filterStripsLabel.Text);

				control.BMComponentGrid.CurrentRowIndex = 1;
				Application.DoEvents();
				AssertEquals("Workflows in the Component [bucket2] will move to the Component [bucket3] when they match the below filters:", filterStripsLabel.Text);

				control.BMComponentGrid.CurrentRowIndex = 2;
				Application.DoEvents();
				AssertEquals("Workflows in the Component [bucket3] will move to the Component [bucket4] when they match the below filters:", filterStripsLabel.Text);

				control.BMComponentGrid.CurrentRowIndex = 3;
				Application.DoEvents();
				AssertEquals("Workflows in the Component [bucket4] will move to the Component [bucket2] when they match the below filters:", filterStripsLabel.Text);

				control.BMComponentGrid.CurrentRowIndex = 4;
				Application.DoEvents();
				AssertEquals("Workflows in the Component [bucket4] will move to the Component [bucket2] when they match the below filters:", filterStripsLabel.Text);

				control.ComponentLinkGrid.CurrentRowIndex = 1;
				Application.DoEvents();
				AssertEquals("Workflows in the Component [bucket4] will move to the Component [bucket3] when they match the below filters:", filterStripsLabel.Text);
			}
		}

		public void TestFilterRulesBinding()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1", sequence: 1);
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2", sequence: 2);
			var bucket3 = BMSTestHelper.CreateBucket(system, "bucket3", sequence: 3);
			var bucket4 = BMSTestHelper.CreateBucket(system, "bucket4", sequence: 4);

			var link1_2 = BMSTestHelper.LinkComponents(bucket1, bucket2, sequence: 1);
			var link2_3 = BMSTestHelper.LinkComponents(bucket2, bucket3, sequence: 1);
			var link3_4 = BMSTestHelper.LinkComponents(bucket3, bucket4, sequence: 1);
			var link4_2 = BMSTestHelper.LinkComponents(bucket4, bucket2, sequence: 1);
			var link4_3 = BMSTestHelper.LinkComponents(bucket4, bucket3, sequence: 2);

			var filter1 = link1_2.FilterRule;
			var filter2 = link2_3.FilterRule;
			var filter3 = link3_4.FilterRule;
			var filter4 = link4_2.FilterRule;
			var filter5 = link4_3.FilterRule;

			FilterStripsTestHelper.AddFilterStrips(filter1,
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover }
				);
			FilterStripsTestHelper.AddFilterStrips(filter2,
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover }
				);
			FilterStripsTestHelper.AddFilterStrips(filter3,
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover }
				);
			FilterStripsTestHelper.AddFilterStrips(filter4,
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover }
				);
			FilterStripsTestHelper.AddFilterStrips(filter5,
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover }
				);

			Factory.Save();

			using (var form = (BMSystemManagementForm)ZControllerFactory.Create(ControllerIDs.BMSystems).ShowEditForm(system))
			{
				var control = form.FindAll<BMComponentsUserControl>().Single();

				AssertFilterStripsShown("1 filter strip shown for link1_2", 1, control);

				control.BMComponentGrid.CurrentRowIndex = 1;
				Application.DoEvents();

				AssertFilterStripsShown("2 filter strips shown for link2_3", 2, control);

				control.BMComponentGrid.CurrentRowIndex = 2;
				Application.DoEvents();

				AssertFilterStripsShown("3 filter strips shown for link3_4", 3, control);

				control.BMComponentGrid.CurrentRowIndex = 3;
				Application.DoEvents();

				AssertFilterStripsShown("4 filter strips shown for link4_2", 4, control);

				control.ComponentLinkGrid.CurrentRowIndex = 1;
				Application.DoEvents();

				AssertFilterStripsShown("5 filter strips shown for link4_3", 5, control);
			}
		}

		public void TestFilterRulesBinding_ShouldNotActivateHasChanges()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1", sequence: 1);
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2", sequence: 2);
			var bucket3 = BMSTestHelper.CreateBucket(system, "bucket3", sequence: 3);

			var link1_2 = BMSTestHelper.LinkComponents(bucket1, bucket2, sequence: 1);
			var link2_3 = BMSTestHelper.LinkComponents(bucket2, bucket3, sequence: 1);

			var filter1 = link1_2.FilterRule;

			FilterStripsTestHelper.AddFilterStrips(filter1,
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover, FilterStripValueSetter = f => ((ModuleFlagsFilter)f).Property0 = true },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CriticalHandover }
				);

			var userData = Factory.LoadTop1<StmModuleFilterUserData>(new ZQuery(StmModuleFilterUserDataSchema.S0_S9, filter1.PK));
			userData.Delete();

			Factory.Save();

			AssertNull(Factory.CreateNewFactory().LoadTop1<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_ParentID, link2_3.PK)));

			using (var form = (BMSystemManagementForm)ZControllerFactory.Create(ControllerIDs.BMSystems).ShowEditForm(system))
			{
				Application.DoEvents();
				var control = form.FindAll<BMComponentsUserControl>().Single();

				AssertFilterStripsShown("Existing filter strips should be shown", 2, control);
				AssertEquals("No changes have been made", false, form.BusinessEntity.HasChanges);

				control.BMComponentGrid.CurrentRowIndex = 1;
				Application.DoEvents();

				AssertFilterStripsShown("New empty filter strip should be shown", 1, control);
				AssertEquals("New filter bizo should be created, but this shouldn't trigger HasChanges", false, form.BusinessEntity.HasChanges);

				var filterStrip = control.FindAll<ZFilterStrip>().Single();
				BMSGUITestCase.SetFilterStripName(filterStrip, ProcessHeader.ModuleFilterConstants.CriticalHandover);

				AssertEquals("New filter strip has been added, so this should trigger HasChanges", true, form.BusinessEntity.HasChanges);

				var saveResult = form.FireSaveButton();

				AssertNoErrors((BusinessObject)form.BusinessEntity);
				AssertSaved(saveResult);
			}

			var loadedLink2_3 = Factory.CreateNewFactory().Load<BMComponentLink>(link2_3.PK);

			AssertNotNull(loadedLink2_3.FilterRule);
			AssertEquals(string.Empty, loadedLink2_3.FilterRule.S9_FilterName);
		}

		public void TestFilterStripControlPreview_ShouldShowActualResults_ForUserWithSecurityRights()
		{
			var staffWithPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithPermissions.GS_FullName = "Almighty";

			Factory.Save();

			using (Env.SetTemporaryUserContext(staffWithPermissions.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Instance.Security.BMSystemsView.IsAllowed = true;
				Env.Instance.Security.BMFilterRule.IsAllowed = true;

				AssertFilterStripControlPreview();
			}
		}

		public void TestFilterStripControlPreview_ShouldShowActualResults_ForUserWithoutSecurityRights()
		{
			var staffWithoutPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutPermissions.GS_FullName = "Poor guy";

			Factory.Save();

			using (Env.SetTemporaryUserContext(staffWithoutPermissions.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Instance.Security.BMSystemsView.IsAllowed = true;
				Env.Instance.Security.BMFilterRule.IsAllowed = false;

				AssertFilterStripControlPreview();
			}
		}

		public void AssertFilterStripControlPreview()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_IsLive = false;
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			var bucket3 = BMSTestHelper.CreateBucket(system, "Bucket 3");
			BMSTestHelper.LinkComponents(bucket1, bucket2, sequence: 1);
			BMSTestHelper.LinkComponents(bucket2, bucket3, sequence: 2);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = BMSTestHelper.CreateJobHeader(orgHeader);
			var jobHeader2 = BMSTestHelper.CreateJobHeader(orgHeader2);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Shalala", bucket1);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Uslurp", bucket1);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader2, "Shalala", bucket2);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader2, "Uslurp", bucket2);

			Factory.Save();

			using (var form = (BMSystemManagementForm)ZControllerFactory.Create(ControllerIDs.BMSystems).ShowEditForm(system))
			{
				form.Show();
				Application.DoEvents();

				var filterControl = form.FindAll<BMFilterStripWrapperControl>().Single();
				AssertEquals(true, filterControl.IsPreviewAllowed);

				var stripControl = filterControl.FindAll<FilterRuleFilterStripControl>().Single();
				var strip = stripControl.AddNewFilterStrip();
				strip.CurrentDataItem.FilterDescription = "Completion Statement";
				((ModuleTextFilter)strip.CurrentDataItem.CurrentModuleFilter).Property = "Shalala";

				BusinessObject[] result = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((o) =>
				{
					var popupForm = o as EmbeddedModulePopup;
					if (popupForm != null)
					{
						popupForm.Closing += (s, e) =>
						{
							result = popupForm.Module_ForTest.GridCollection.ToArray();
						};
					}
				});

				var toolStrip = form.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip");
				var previewButton = toolStrip.Items["ToolStripPreviewDropButton"];
				form.Show();
				Application.DoEvents();
				previewButton.PerformClick();
				form.Show();
				Application.DoEvents();

				AssertContainsExactElementsInAnyOrder("The result should reflect the added filter strip AND the auto added component-related filter strip, and yet...", new[] { workflow1.PK }, result.Select(x => x.PK));

				var componentControl = form.FindAll<BMComponentsUserControl>().Single();
				componentControl.BMComponentGrid.CurrentRowIndex = 1;

				toolStrip = form.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip");
				previewButton = toolStrip.Items["ToolStripPreviewDropButton"];
				previewButton.PerformClick();

				AssertContainsExactElementsInAnyOrder("Should have shown results for headers from the second bucket since that one was selected, and yet...", new[] { workflow3.PK, workflow4.PK }, result.Select(x => x.PK));

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest();
			}
		}

		public void TestFilterRuleDisplay_WhenComponentLinkDoesNotExist_ShouldInformUser()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var component1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var component2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			var componentLink = BMSTestHelper.LinkComponents(component1, component2, sequence: 1);
			FilterStripsTestHelper.AddFilterStrips(componentLink.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.AutoAssignTasks,
				FilterStripValueSetter = f => ((ModuleFlagsFilter)f).Property0 = true,
			});

			Factory.Save();

			using (var form = (BMSystemManagementForm)ZControllerFactory.Create(ControllerIDs.BMSystems).ShowEditForm(system))
			{
				form.Show();
				Application.DoEvents();

				SelectComponent(form, 0, component1);
				var componentGrid = form.FindAll<ZGrid>().First(grid => grid.Name == "BMComponentGrid");
				var filterStripsLabel_WithBinding = form.Find(control => control.Name.Equals("FilterStripsLabelWithBinding")).First();
				var filterStripsLabel_WithoutBinding = form.Find(control => control.Name.Equals("FilterStripsLabelWithoutBinding")).First();

				CombineAssertions("We are displaying the correct filter label text", () =>
				{
					AssertEquals(true, filterStripsLabel_WithBinding.Visible);
					AssertEquals(false, filterStripsLabel_WithoutBinding.Visible);
				});

				AssertFirstComponentLinkFound(form, componentLink.PK, "We selected the first componentLink");

				var filterStrips = form.FindAll<ZFilterStrip>();
				AssertNotNull("PRE: We have a filter strip displaying for the componentLink's filter rule", filterStrips.Where(strip => strip.CurrentDataItem.CurrentModuleFilter is ModuleFlagsFilter));

				SelectComponent(form, 1, component2);
				// find the components again, as they've been rebound
				filterStripsLabel_WithBinding = form.Find(control => control.Name.Equals("FilterStripsLabelWithBinding")).First();
				filterStripsLabel_WithoutBinding = form.Find(control => control.Name.Equals("FilterStripsLabelWithoutBinding")).First();

				CombineAssertions("We are displaying the correct filter label", () =>
				{
					AssertEquals(false, filterStripsLabel_WithBinding.Visible);
					AssertEquals(true, filterStripsLabel_WithoutBinding.Visible);
				});

				AssertFirstComponentLinkFound(form, null, "We should have an empty component link grid");
			}
		}

		void SelectComponent(ZForm form, int componentIndexToSelect, BMComponent componentToSelect)
		{
			var componentGrid = form.FindAll<ZGrid>().First(grid => grid.Name == "BMComponentGrid");
			var componentLinkGrid = form.FindAll<ZGrid>().First(grid => grid.Name == "ComponentLinkGrid");
			componentGrid.IsWholeRowSelectedOnClick = true;

			componentLinkGrid.UnSelectAll();
			componentGrid.UnSelectAll();
			form.Show();
			Application.DoEvents();

			componentGrid.SelectSingleElement(componentToSelect);

			form.Show();
			Application.DoEvents();
		}

		void AssertFirstComponentLinkFound(ZForm form, object expectedValue, string message)
		{
			var componentLinkGrid = form.FindAll<ZGrid>().First(grid => grid.Name == "ComponentLinkGrid");
			componentLinkGrid.UnSelectAll();
			componentLinkGrid.SelectAllElements();

			form.Show();
			Application.DoEvents();

			AssertEquals(message, expectedValue, componentLinkGrid.SelectedElements.FirstOrDefault()?.PK);
		}

		static void AssertFilterStripsShown(string message, int expectedCount, BMComponentsUserControl control)
		{
			var strips = control.FindAll<ZFilterStrip>().ToArray();

			AssertEquals(message, expectedCount, strips.Length);
		}

		#endregion

		#region Schematic Visualisation

		[UseSnapshotProtection]
		public void TestSchematicImagePreview_ShouldShowMessageWhenSchematicPreviewOversized()
		{
			var bms = BMSTestHelper.CreateSystem(Factory, "DEF");
			using (var form = (BMSystemManagementForm)ZControllerFactory.Create(ControllerIDs.BMSystems).ShowFormForNewEntity(bms))
			{
				var schematicImage = (KPictureBox)(form.Controls.Find("schematicPictureBox", true)[0]);
				var labelInvalidSchematicMessage = (ZLabel)(form.Controls.Find("labelInvalidSchematicMessage", true)[0]);

				AssertNull("GIVEN the BMS has no schematic dimensions, WHEN a schematic is requested for generation, THEN nothing is generated",
										schematicImage.Image);
				AssertEquals("GIVEN the BMS has no schematic dimensions, WHEN a schematic is requested for generation, THEN labelInvalidSchematicMessage shows its default message",
											"Enter a valid component combination to display the schematic", labelInvalidSchematicMessage.Text);

				BMSTestHelper.AddMultipleBucketsBuffersAndLinks(bms, 12);
				Factory.Save();
				form.ComponentsUserControl.BindingContext = new BindingContext(); // This is currently the only reasonable way to access DrawSchematic() within BMComponentsUserControl.

				schematicImage = (KPictureBox)(form.Controls.Find("schematicPictureBox", true)[0]);
				labelInvalidSchematicMessage = (ZLabel)(form.Controls.Find("labelInvalidSchematicMessage", true)[0]);

				AssertNotNull("GIVEN the BMS has appropriate schematic width and height dimensions, WHEN a schematic is requested for generation, THEN schematicPictureBox holds an image",
											schematicImage.Image);

				BMSTestHelper.AddMultipleBucketsBuffersAndLinks(bms, 20);
				Factory.Save();
				form.ComponentsUserControl.BindingContext = new BindingContext();

				schematicImage = (KPictureBox)(form.Controls.Find("schematicPictureBox", true)[0]);
				labelInvalidSchematicMessage = (ZLabel)(form.Controls.Find("labelInvalidSchematicMessage", true)[0]);

				AssertNull("GIVEN the BMS has oversized schematic width and height dimensions, WHEN a schematic is requested for generation, THEN schematicPictureBox shows no image",
										schematicImage.Image);
				AssertEquals("GIVEN the BMS has oversized schematic width and height dimensions, WHEN a schematic is requested for generation, THEN labelInvalidSchematicMessage shows an altered message",
											"Schematic is too large to preview. Please select 'View Schematic Full-Screen' to view the schematic.", labelInvalidSchematicMessage.Text);
			}
		}

		#endregion

		#region Column Visibility

		public void TestDefaultAndInvisibleColumns_ForReleaseGroupLinks()
		{
			var bms = BMSTestHelper.CreateSystem(Factory, "DEF");
			BMSTestHelper.AddMultipleBucketsBuffersAndLinks(bms, 12);

			using (var form = (BMSystemManagementForm)ZControllerFactory.Create(ControllerIDs.BMSystems).ShowFormForNewEntity(bms))
			{
				var allColumns = form.ComponentsUserControl.ReleaseGroupLinksGrid_ExposedForTest.ColumnStyles.Cast<ZGridColumnInfo>().ToList();

				var visibleColumns = allColumns.Where(c => c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder([
					"FO_GG_ReleaseGroup",
					"ReleaseGroup+GG_Desc",
					"FO_IsConstrainedMode",
					"FO_AutoAssignTasksAge",
				], visibleColumns);

				var invisibleColumns = allColumns.Where(c => !c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder([], invisibleColumns);
			}
		}

		public void TestDefaultAndInvisibleColumns_ForReleaseGroupLinks_WhenNewReleaseGateRegistryEnabled()
		{
			var bms = BMSTestHelper.CreateSystem(Factory, "DEF");
			BMSTestHelper.AddMultipleBucketsBuffersAndLinks(bms, 12);

			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var form = (BMSystemManagementForm)ZControllerFactory.Create(ControllerIDs.BMSystems).ShowFormForNewEntity(bms))
			{
				var allColumns = form.ComponentsUserControl.ReleaseGroupLinksGrid_ExposedForTest.ColumnStyles.Cast<ZGridColumnInfo>().ToList();

				var visibleColumns = allColumns.Where(c => c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder([
					"FO_GG_ReleaseGroup",
					"ReleaseGroup+GG_Desc",
					"FO_IsConstrainedMode",
					"FO_AutoAssignTasksAge",
					"FO_ReleaseGateMode",
					"FO_ReleaseWhenInProgress",
				], visibleColumns);

				var invisibleColumns = allColumns.Where(c => !c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder([], invisibleColumns);
			}
		}

		public void TestDefaultAndInvisibleColumns_ForResourceMembership()
		{
			var bms = BMSTestHelper.CreateSystem(Factory, "DEF");
			BMSTestHelper.AddMultipleBucketsBuffersAndLinks(bms, 12);

			using (var form = (BMSystemManagementForm)ZControllerFactory.Create(ControllerIDs.BMSystems).ShowFormForNewEntity(bms))
			{
				var allColumns = form.ComponentsUserControl.ResourceMembershipGrid_ExposedForTest.ColumnStyles.Cast<ZGridColumnInfo>().ToList();

				var visibleColumns = allColumns.Where(c => c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder([
					"FD_GS_NKResource",
					"Resource+GS_FullName",
					"FD_CapacityLimitPercent",
					"TimeConsideredCCR",
					"FD_IsCapacityConstrained",
					"FD_GS_NKDesignatedAsCapacityConstrainedBy",
					"MarkedAsCapacityConstrainedByFullName",
				], visibleColumns);

				var invisibleColumns = allColumns.Where(c => !c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder([
					"CapacityConstraintDetectedLocal",
					"FD_CapacityConstraintDetectedUtc",
					"FD_IsPersistentlyOverloaded",
				], invisibleColumns);
			}
		}

		public void TestDefaultAndInvisibleColumns_ForResourceMembership_WhenNewReleaseGateRegistryEnabled()
		{
			var bms = BMSTestHelper.CreateSystem(Factory, "DEF");
			BMSTestHelper.AddMultipleBucketsBuffersAndLinks(bms, 12);

			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var form = (BMSystemManagementForm)ZControllerFactory.Create(ControllerIDs.BMSystems).ShowFormForNewEntity(bms))
			{
				var allColumns = form.ComponentsUserControl.ResourceMembershipGrid_ExposedForTest.ColumnStyles.Cast<ZGridColumnInfo>().ToList();

				var visibleColumns = allColumns.Where(c => c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder([
					"FD_GS_NKResource",
					"Resource+GS_FullName",
					"FD_CapacityLimitPercent",
					"TimeConsideredCCR",
					"FD_IsCapacityConstrained",
					"FD_GS_NKDesignatedAsCapacityConstrainedBy",
					"MarkedAsCapacityConstrainedByFullName",
					"FD_StaffStartableWorkflowLimit",
				], visibleColumns);

				var invisibleColumns = allColumns.Where(c => !c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder([
					"CapacityConstraintDetectedLocal",
					"FD_CapacityConstraintDetectedUtc",
					"FD_IsPersistentlyOverloaded",
				], invisibleColumns);
			}
		}

		public void TestTaskPenetrationSetupColumnsVisibilityIsDefinedByRegitryItem()
		{
			var bms = BMSTestHelper.CreateSystem(Factory, "DEF");
			BMSTestHelper.AddMultipleBucketsBuffersAndLinks(bms, 12);

			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var form = (BMSystemManagementForm)ZControllerFactory.Create(ControllerIDs.BMSystems).ShowFormForNewEntity(bms))
			{
				form.ComponentsUserControl.BindAllComponentLinksTabs();
				AssertEquals(false, form.ComponentsUserControl.ReleaseGroupLinksGrid_ExposedForTest.Columns.Contains(BMComponentReleaseGroupLinkSchema.Constants.FO_ResetTaskPenetrationOutsideGroup));
				AssertEquals(false, form.ComponentsUserControl.ReleaseGroupLinksGrid_ExposedForTest.Columns.Contains(BMComponentReleaseGroupLinkSchema.Constants.FO_ResetTaskPenetrationInsideGroup));
				AssertEquals(false, form.ComponentsUserControl.ReleaseGroupLinksGrid_ExposedForTest.Columns.Contains(BMComponentReleaseGroupLinkSchema.Constants.FO_ResetTaskPenetrationThrottleFactor));
			}

			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var form = (BMSystemManagementForm)ZControllerFactory.Create(ControllerIDs.BMSystems).ShowFormForNewEntity(bms))
			{
				form.ComponentsUserControl.BindAllComponentLinksTabs();
				AssertEquals(true, form.ComponentsUserControl.ReleaseGroupLinksGrid_ExposedForTest.Columns.Contains(BMComponentReleaseGroupLinkSchema.Constants.FO_ResetTaskPenetrationOutsideGroup));
				AssertEquals(true, form.ComponentsUserControl.ReleaseGroupLinksGrid_ExposedForTest.Columns.Contains(BMComponentReleaseGroupLinkSchema.Constants.FO_ResetTaskPenetrationInsideGroup));
				AssertEquals(true, form.ComponentsUserControl.ReleaseGroupLinksGrid_ExposedForTest.Columns.Contains(BMComponentReleaseGroupLinkSchema.Constants.FO_ResetTaskPenetrationThrottleFactor));
			}
		}

		#endregion
	}
}
