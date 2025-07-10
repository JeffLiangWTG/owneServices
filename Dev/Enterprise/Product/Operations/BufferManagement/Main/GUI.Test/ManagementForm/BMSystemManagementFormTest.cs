using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.TimeEngineScheduler.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(BMSystemManagementForm))]
	public class BMSystemManagementFormTest : ZFormBasherTest
	{
		#region BMSystem properties

		public void TestDescriptionIsOnlyUniqueForADInt()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var system = Factory.NewWithValidTestData<BMSystem>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "AGroupingOfIndividualsWhoAllLikeSomethingFun;)";
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var loadedSystem = anotherFactory.Load<BMSystem>(system.PK);

			using (BMSystemManagementForm form = new BMSystemManagementForm(loadedSystem))
			{
				var control = form.Menu.MenuItems.Find("ValidateMenuItem", true).Single();
				control.PerformClick();

				AssertNoErrors(loadedSystem.ReleaseGroups.Cast<BMSystemReleaseGroup>().First().Group.GG_DescInfo);
			}
		}

		#region System IsLive

		public void TestSystemIsLive_ChangesAsSystemDoes()
		{
			ZFormModaliser.ShowDialogsInTest = true;

			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_IsLive = false;
			system.FS_Description = "System has description";

			var component1 = system.Components.AddNew();
			component1.FC_Name = "Component1";
			component1.FC_Type = BMComponentTypeList.Codes.Bucket;

			var component2 = system.Components.AddNew();
			component2.FC_Name = "Component2";
			component2.FC_Type = BMComponentTypeList.Codes.Bucket;

			var linkFrom1To2 = component1.FromMeToOthersLinks.AddNew();
			linkFrom1To2.FL_FC_ComponentFrom = component1.PK;
			linkFrom1To2.FL_FC_ComponentTo = component2.PK;

			Factory.Save();

			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindAll<Control>(ctrl => ctrl.Text.Equals("Not Live"));
				AssertNotNull("We have a control that highlights the System Liveness", control);

				ClickLivenessToggleButton(form, DialogResult.Yes);

				AssertEquals("We successfully changed the system liveness", true, form.BMSystem.FS_IsLive);
				control = form.FindAll<Control>(ctrl => ctrl.Text.Equals("Not Live"));
				AssertNotNull("We have a control that highlights the System Liveness", control);

				ClickLivenessToggleButton(form, DialogResult.Yes);

				AssertEquals("We successfully changed the system liveness", false, form.BMSystem.FS_IsLive);
				control = form.FindAll<Control>(ctrl => ctrl.Text.Equals("Not Live"));
				AssertNotNull("We have a control that highlights the System Liveness", control);
			}
		}

		public void TestSystemIsLiveToggle_SavesOnLivening_NoSaveOnUnLivening()
		{
			ZFormModaliser.ShowDialogsInTest = true;

			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_Name = "aSociety";
			system.FS_Description = "System has description";

			var component1 = system.Components.AddNew();
			component1.FC_Name = "Component1";
			component1.FC_Type = BMComponentTypeList.Codes.Bucket;

			var component2 = system.Components.AddNew();
			component2.FC_Name = "Component2";
			component2.FC_Type = BMComponentTypeList.Codes.Bucket;

			var linkFrom1To2 = component1.FromMeToOthersLinks.AddNew();
			linkFrom1To2.FL_FC_ComponentFrom = component1.PK;
			linkFrom1To2.FL_FC_ComponentTo = component2.PK;

			Factory.Save();

			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				Application.DoEvents();

				system.FS_Name = "Joker";
				Application.DoEvents();

				AssertEquals("PRE: We changed the system", true, form.BMSystem.HasChanges);
				AssertSystemSaving_WhenLivenessToggled(form, BMSystemManagementForm.ToggledToLiveMessage, DialogResult.Yes, expectToBeLive: true);

				system.FS_Name = "Heath";
				Application.DoEvents();

				AssertEquals("PRE: We changed the system", true, form.BMSystem.HasChanges);
				AssertSystemSaving_WhenLivenessToggled(form, BMSystemManagementForm.ToggledToDeadMessage, DialogResult.Yes, expectToBeLive: false);

				system.FS_Name = "JokeroJoestar";
				Application.DoEvents();

				AssertEquals("PRE: We changed the system", true, form.BMSystem.HasChanges);
				AssertSystemSaving_WhenLivenessToggled(form, BMSystemManagementForm.ToggledToLiveMessage, DialogResult.Yes, expectToBeLive: true);
			}
		}

		public void TestSystemIsLiveToggle_CancelsProperly()
		{
			ZFormModaliser.ShowDialogsInTest = true;

			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_IsLive = false;
			system.FS_Name = "aSociety";
			system.FS_Description = "System has description";

			var component1 = system.Components.AddNew();
			component1.FC_Name = "Component1";
			component1.FC_Type = BMComponentTypeList.Codes.Bucket;

			var component2 = system.Components.AddNew();
			component2.FC_Name = "Component2";
			component2.FC_Type = BMComponentTypeList.Codes.Bucket;

			var linkFrom1To2 = component1.FromMeToOthersLinks.AddNew();
			linkFrom1To2.FL_FC_ComponentFrom = component1.PK;
			linkFrom1To2.FL_FC_ComponentTo = component2.PK;

			Factory.Save();

			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				Application.DoEvents();

				system.FS_Name = "Joker";
				Application.DoEvents();

				AssertEquals("PRE: We changed the system", true, form.BMSystem.HasChanges);

				ClickLivenessToggleButton(form, DialogResult.No);
				AssertEquals("We cancelled the toggle action and nothing saved", true, form.BMSystem.HasChanges);
				AssertEquals("We cancelled the toggle action and it's still dead", false, form.BMSystem.FS_IsLive);

				ClickLivenessToggleButton(form, DialogResult.Yes);
				AssertEquals("We performed the toggle action and everything saved", false, form.BMSystem.HasChanges);
				AssertEquals("We performed the toggle action and it's now live", true, form.BMSystem.FS_IsLive);

				system.FS_Name = "Joker2";
				Application.DoEvents();

				AssertEquals("PRE: We changed the system", true, form.BMSystem.HasChanges);

				ClickLivenessToggleButton(form, DialogResult.No);
				AssertEquals("We cancelled the toggle action and nothing saved", true, form.BMSystem.HasChanges);
				AssertEquals("We cancelled the toggle action and it's still live", true, form.BMSystem.FS_IsLive);
			}
		}

		public void TestSystemIsLiveToggle_ShouldProcessAllTransferRulesLinksOnNextBMSRun()
		{
			ZFormModaliser.ShowDialogsInTest = true;

			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_IsLive = false;
			system.FS_Name = "aSociety";
			system.FS_Description = "System has description";

			var component1 = system.Components.AddNew();
			component1.FC_Name = "Component1";
			component1.FC_Type = BMComponentTypeList.Codes.Bucket;

			var component2 = system.Components.AddNew();
			component2.FC_Name = "Component2";
			component2.FC_Type = BMComponentTypeList.Codes.Bucket;

			var linkFrom1To2 = component1.FromMeToOthersLinks.AddNew();
			linkFrom1To2.FL_FC_ComponentFrom = component1.PK;
			linkFrom1To2.FL_FC_ComponentTo = component2.PK;

			Factory.Save();

			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				Application.DoEvents();

				system.FS_Name = "Joker";
				Application.DoEvents();

				AssertEquals("PRE: BMS does not process all transfers on next run", false, BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.Value);

				ClickLivenessToggleButton(form, DialogResult.No);
				AssertEquals("BMS still does not process all transfers on next run", false, BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.Value);

				ClickLivenessToggleButton(form, DialogResult.Yes);
				AssertEquals("BMS now processes all transfers on next run", true, BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.Value);
			}
		}

		public void TestSystemIsLive_ShouldToggleWhenErrors()
		{
			ZFormModaliser.ShowDialogsInTest = true;

			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_IsLive = true;
			system.FS_Name = "CollapseSociety";
			system.FS_Description = "In few days";
			Factory.Save();

			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				Application.DoEvents();

				var nameTextBox = form.FindAll<ZTextBox>(ctrl => ctrl.Name.Equals("NameTextBox")).First();
				var descriptionTextBox = form.FindAll<ZTextBox>(ctrl => ctrl.Name.Equals("DescriptionTextBox")).First();
				descriptionTextBox.Focus();
				descriptionTextBox.Text = "";
				nameTextBox.Focus();
				Application.DoEvents();

				AssertEquals("PRE: We changed the system", true, form.BMSystem.HasChanges);
				AssertEquals("PRE: We changed the system, badly", true, form.BMSystem.HasErrors);

				ClickLivenessToggleButton(form, DialogResult.Yes);

				AssertEquals("We toggle action and save", false, form.BMSystem.HasChanges);
				AssertEquals("We toggle action and it's not live", false, form.BMSystem.FS_IsLive);
			}
		}

		public void TestSystemIsNotLive_ShouldNotToggleWhenThereAreErrors_ShouldShowValidationError()
		{
			ZFormModaliser.ShowDialogsInTest = true;

			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_IsLive = false;
			system.FS_Name = "Society";
			system.FS_Description = "Changed";
			Factory.Save();

			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				Application.DoEvents();

				var descriptionTextBox = form.FindAll<ZTextBox>(ctrl => ctrl.Name.Equals("DescriptionTextBox")).First();
				descriptionTextBox.Text = "";
				Application.DoEvents();

				AssertEquals("PRE: We changed the system, but validation has not triggered", false, form.BMSystem.HasChanges);
				AssertEquals("PRE: We changed the system, but validation has not triggered", false, form.BMSystem.HasErrors);

				ClickLivenessToggleButton(form, DialogResult.Yes);
				AssertEquals("We cancelled the toggle action and nothing saved", true, form.BMSystem.HasChanges);
				AssertEquals("Validation has triggered", true, form.BMSystem.HasErrors);
				AssertEquals("We cancelled the toggle action and it is still not live", false, form.BMSystem.FS_IsLive);

				AssertSystemSaving_WhenLivenessToggled(form, BMSystemManagementForm.ToggleWithErrorMessage, DialogResult.Yes, expectToBeLive: false);
			}
		}

		public void TestAddFilterButton_WhenLive_ShouldBeReadOnly()
		{
			ZFormModaliser.ShowDialogsInTest = true;

			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_IsLive = true;
			system.FS_Name = "aSociety";

			var entry = BMSTestHelper.CreateBuffer(system, "entry");
			var exit = BMSTestHelper.CreateBuffer(system, "exit");
			var link = Factory.NewWithValidTestData<BMComponentLink>();
			link.FL_FC_ComponentFrom = entry.PK;
			link.FL_FC_ComponentTo = exit.PK;

			FilterStripsTestHelper.AddFilterStrips(link.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.AutoAssignTasks,
				FilterStripValueSetter = f => ((ModuleFlagsFilter)f).Property0 = true,
			});

			entry.FromMeToOthersLinks.Add(link);

			Factory.Save();

			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("PRE: We should start with one filter strip", 1, form.FindAll<ZFilterStrip>().Count());

				var filterStripAddButton = form.FindAll<ZFilterStripAddButton>().First();
				AssertEquals("Our filterstrip 'Add' button should be disabled because we're in read-only mode", false, filterStripAddButton.Enabled);

				ClickLivenessToggleButton(form, DialogResult.Yes);

				AssertEquals("PRE: We toggled to an unlive system", false, system.FS_IsLive);
				filterStripAddButton = form.FindAll<ZFilterStripAddButton>().First();
				AssertEquals("Our filterstrip 'Add' button should be enabled because we're in write mode", true, filterStripAddButton.Enabled);

				filterStripAddButton.AddButton.PerformClick();
				form.Show();
				Application.DoEvents();

				AssertEquals("We should only have made 1 other filterstrip", 2, form.FindAll<ZFilterStrip>().Count());
			}
		}

		#endregion

		#endregion

		#region Related Entities

		public void TestAcceptabilityBandsDontBlockSave()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			BMSTestHelper.LinkComponents(bucket, buffer);

			AssertNoErrors(system);

			var acceptabilityBand = BMSTestHelper.CreateAcceptabilityBand(buffer, 0, 2, 4, 6, 8, 10, "Wheehoo");
			acceptabilityBand.BAB_SqlText = "AE Alexander Eagles Alex Eagles < Its silly to search for your own name in the code base. Not that silly if DNK Dink Daniel Keogh AKA The Dan has had at it though.";

			Factory.Save();

			var formSystem = Factory.CreateNewFactory().Load<BMSystem>(system.PK);
			using (var form = new BMSystemManagementForm(formSystem))
			{
				form.Show();
				Application.DoEvents();

				formSystem.FS_Name = "BeepBoop";
				form.FireSaveButton();
			}

			var finalSystem = Factory.CreateNewFactory().Load<BMSystem>(system.PK);
			AssertEquals("BeepBoop", finalSystem.FS_Name);
		}

		public void TestNewUnidentifiedFilterWrapperControl_ShouldThrowException()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_IsLive = false;
			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer 1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer 2");
			var link = BMSTestHelper.LinkComponents(buffer1, buffer2);

			Factory.Save();

			using (var form = new BMSystemManagementForm(system))
			{
				var control = form.FindAll<BMFilterStripWrapperControl>().Single();
				control.FilterControlIdentifier = "New Control";

				form.Show();

				var toolStrip = control.FindAll<ZToolStrip>().Single(x => x.Name == "ToolStrip");
				var previewButton = toolStrip.Items["ToolStripPreviewDropButton"];

				AssertExceptionThrown<UnidentifiedFilterControlException>("BMBoardForm's GetObjectForPreview method isn't set up to handle previewing from a control with the FilterControlIdentifier == 'New Control', so an exception should have been thrown, and yet...",
					() => previewButton.PerformClick());
			}
		}

		public void TestExperimentalSettingsButtonDisabledByDefault()
		{
			var system = Factory.New<BMSystem>();
			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				var button = form.GetControl<ZButton>("ExperimentalSettingsButton");
				AssertEquals(false, button.Visible);
			}
		}

		public void TestExperimentalSettingsButtonEnabledWithRegistryKeyon()
		{
			BMSRegistry.Instance.EnablePaveExperimentalFeatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = Factory.New<BMSystem>();
			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				var button = form.GetControl<ZButton>("ExperimentalSettingsButton");
				AssertEquals(true, button.Visible);
			}
		}

		public void TestExperimentalSettingsButtonDisabledWithRegistryKeyOnButNoPermission()
		{
			BMSRegistry.Instance.EnablePaveExperimentalFeatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.BMBoardEdit.IsAllowed = false;

			var system = Factory.New<BMSystem>();
			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				var button = form.GetControl<ZButton>("ExperimentalSettingsButton");
				AssertEquals(false, button.Visible);
			}
		}

		#endregion

		#region Is Live

		public void TestLiveOnlyControls_AreNotReadOnlyWhenDead()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_IsLive = false;

			using (var form = new BMSystemManagementForm(system))
			{
				var controlsToBeReadOnly = form.FindAll<Control>(cntrl => ReadOnlyControls.Contains(cntrl.Name));

				foreach (var control in controlsToBeReadOnly)
				{
					AssertNotNull("PRE: This control exists!", control);
					AssertEquals($"We should have {control.Name} be not readonly!", false, control.GetReadOnly());
				}
			}
		}

		public void TestLiveOnlyControls_AreReadOnlyWhenLive()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_IsLive = true;

			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				Application.DoEvents();
				var controlsToBeReadOnly = form.FindAll<Control>(cntrl => ReadOnlyControls.Contains(cntrl.Name));

				foreach (var control in controlsToBeReadOnly)
				{
					AssertNotNull("PRE: This control exists!", control);
					AssertEquals($"We should have {control.Name} be read only!", true, control.GetReadOnly());
				}
			}
		}

		readonly string[] ReadOnlyControls =
		{
			"BMComponentGrid",
			"ComponentOffsetTimeEdit",
			"DepartmentFindBox",
			"BranchFindBox",
			"NonCCRMultiplierCalcEdit",
			"AutoAssignAfterAgeEdit",
			"BufferLoadLimitCalcEdit",
			"BufferTimeSpanEdit",
			"ComponentLinkGrid",
			"FilterDescriptionDropEdit", // filterstrip controls are... weird, so this is how we check if the control as a whole is functionally "readonly"
		};

		public void TestShouldNotSwitchToLive_WhenThereAreValidationErrors()
		{
			var system = BMSTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);
			system.FS_Name = "Test";
			system.FS_IsLive = false;

			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				var button = form.GetControl<ZButton>("SystemIsLiveToggleButton");
				UnitTestUserNotification.Instance.AddYesAnswer();
				button.PerformClick();

				Assert("Should have validation errors", system.HasErrors);
				Assert("Should stay deactivated", !system.FS_IsLive);
			}
		}

		public void TestShouldNotSwitchToLive_WhenZSaveExceptionOccurs()
		{
			var system = BMSTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);
			system.FS_Name = "Test";
			system.FS_Description = "Description";
			system.FS_IsLive = false;
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket 2");
			var bucket3 = BMSTestHelper.CreateBucket(system, "bucket 3");
			BMSTestHelper.LinkComponents(bucket1, bucket2);
			BMSTestHelper.LinkComponents(bucket1, bucket3);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow", currentComponent: bucket3);

			Factory.Save();

			bucket3.Delete(); // should violate db constraints as the user cannot delete a component when there is a workflow in it 

			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				var button = form.GetControl<ZButton>("SystemIsLiveToggleButton");
				UnitTestUserNotification.Instance.AddYesAnswer();
				AssertExceptionThrown<ZSaveException>(() => button.PerformClick());

				Assert("Should have no validation errors", !system.HasErrors);
				Assert("Should stay deactivated", !system.FS_IsLive);
			}
		}

		public void TestUpdateRelatedWorkflowsButtonVisibility()
		{
			var system = BMSTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);
			system.FS_Name = "Test";
			system.FS_Description = "Description";
			system.FS_IsLive = false;
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket 2");
			BMSTestHelper.LinkComponents(bucket1, bucket2);

			Factory.Save();

			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				var button = form.GetControl<ZButton>("UpdateRelatedWorkflowsButton");
				AssertEquals(false, button.Visible);
			}

			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				var button = form.GetControl<ZButton>("UpdateRelatedWorkflowsButton");
				AssertNotNull(button);
				AssertEquals(true, button.Visible);
				AssertEquals(false, button.Enabled);

				var activateButton = form.GetControl<ZButton>("SystemIsLiveToggleButton");
				AssertNotNull(activateButton);
				AssertEquals(true, activateButton.Visible);
				AssertEquals(true, activateButton.Enabled);
				UnitTestUserNotification.Instance.AddYesAnswer();
				activateButton.PerformClick();

				Assert("Should be saved with no errors", !system.HasErrors);
				Assert("Should turn to live", system.FS_IsLive);

				AssertEquals(true, button.Visible);
				AssertEquals(true, button.Enabled);
			}
		}

		[TestDate(2025, 01, 23)]
		public void TestUpdateRelatedWorkflowsButton()
		{
			const int nudgeDelay = 2; // 2 minute delay for nudging the BMS service task
			BMSRegistry.Instance.DelayForResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, nudgeDelay);

			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var system1 = BMSTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);
			system1.FS_Name = "System 1";
			var component1_1 = BMSTestHelper.CreateBucket(system1, "bucket 1_1");
			var component1_2 = BMSTestHelper.CreateBucket(system1, "bucket 1_2");
			var component1_3 = BMSTestHelper.CreateBucket(system1, "bucket 1_3 (inactive)");
			component1_3.FC_IsActive = false;
			var component1_4 = BMSTestHelper.CreateBucket(system1, "bucket 1_4 (has just an inactive outgoing link)");
			var component1_5 = BMSTestHelper.CreateBucket(system1, "bucket 1_5 (sink)");
			BMSTestHelper.LinkComponents(component1_1, component1_2);
			BMSTestHelper.LinkComponents(component1_2, component1_3);
			BMSTestHelper.LinkComponents(component1_3, component1_4);
			BMSTestHelper.LinkComponents(component1_4, component1_5).FL_TransferRulesEnabled = false;

			var system2 = BMSTestHelper.CreateSystem(Factory, "ORG");
			system2.FS_Name = "System 2";
			var component2_1 = BMSTestHelper.CreateBucket(system2, "bucket 2_1");
			var component2_2 = BMSTestHelper.CreateBucket(system2, "bucket 2_2");
			BMSTestHelper.LinkComponents(component2_1, component2_2);

			new TimeActionScheduleCollection(Factory).DeleteAll();

			Factory.Save();

			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
			using (var form = new BMSystemManagementForm(system1))
			{
				form.Show();
				var button = form.GetControl<ZButton>("UpdateRelatedWorkflowsButton");
				AssertEquals(true, button.Visible);
				AssertEquals(true, button.Enabled);
				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddOKAnswer();
				button.PerformClick();

				var collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
				var scheduledComponentPKs = collection.Select(a => a.TAS_TargetPK);
				AssertContainsExactElementsInAnyOrder("Should schedule update actions for the components of the BMS", [component1_1.PK, component1_2.PK, component1_3.PK, component1_4.PK], scheduledComponentPKs);
				AssertCollectionContains("Should schedule for inactive components as well", component1_3.PK, scheduledComponentPKs);
				AssertCollectionNotContains("Should not schedule for components with no outgoing links", component1_5.PK, scheduledComponentPKs);
				AssertCollectionContains("Should still schedule for components that only have inactive outgoing links", component1_4.PK, scheduledComponentPKs);
				Assert("Should schedule actions targeting BMComponentLinks", collection.All(s => s.TAS_TargetTableCode == BMComponentSchema.Constants.Prefix));
				Assert("Should schedule actions with proper code", collection.All(s => s.TAS_ActionCode == "PHU"));
				Assert("Should schedule actions with proper parameter to update all properties", collection.All(s => s.TAS_JsonParameter == ProcessHeaderResponsiveActionConstants.UpdateAllExceptDedicatedBuffer));
				Assert("Should schedule actions as active", collection.All(s => s.TAS_ExecutionStatus == "SCH"));
				Assert("Should schedule without a token", collection.All(s => string.IsNullOrEmpty(s.TAS_Token)));
				Assert("Should schedule actions ready for execution", collection.All(s => s.TAS_ExecutionDateTimeUtc == ZDateTime.UtcNow));

				serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(TransferRuleRunnerServiceTask.Code, TimeSpan.FromMinutes(nudgeDelay)), Times.Once);
				AssertEquals("Updating related workflows will be initiated in 2 minutes (defined by the Delay For Responsive Workflow Updates On Related Object Changes registry item). It will take some time to process every related workflow.", UnitTestUserNotification.Instance.LastMessage.Text);

				new TimeActionScheduleCollection(new BusinessObjectFactory()).DeleteAll();

				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddOKAnswer();
				button.PerformClick();

				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddOKAnswer();
				button.PerformClick(); // let's do it twice to ensure we don't schedule updates multiple times when the user clicks the button multiple times

				collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
				AssertContainsExactElementsInAnyOrder("Should schedule update actions for the components of the BMS", [component1_1.PK, component1_2.PK, component1_3.PK, component1_4.PK], collection.Select(a => a.TAS_TargetPK));
				Assert("Should schedule actions targeting BMComponentLinks", collection.All(s => s.TAS_TargetTableCode == BMComponentSchema.Constants.Prefix));
				Assert("Should schedule actions with proper code", collection.All(s => s.TAS_ActionCode == "PHU"));
				Assert("Should schedule actions with proper parameter to update all properties", collection.All(s => s.TAS_JsonParameter == ProcessHeaderResponsiveActionConstants.UpdateAllExceptDedicatedBuffer));
				Assert("Should schedule actions as active", collection.All(s => s.TAS_ExecutionStatus == "SCH"));
				Assert("Should schedule without a token", collection.All(s => string.IsNullOrEmpty(s.TAS_Token)));
				Assert("Should schedule actions ready for execution", collection.All(s => s.TAS_ExecutionDateTimeUtc == ZDateTime.UtcNow));
			}
		}

		public void TestUpdateRelatedWorkflowsButton_WhenResponsiveDataProcessingIsDisabledInRegistry()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			BMSRegistry.Instance.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertUpdateRelatedWorkflowsButton_ShowsMessageAboutRegistryItems();
		}

		public void TestUpdateRelatedWorkflowsButton_WhenResponsiveWorkflowUpdatesAreDisabledInRegistry()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertUpdateRelatedWorkflowsButton_ShowsMessageAboutRegistryItems();
		}

		void AssertUpdateRelatedWorkflowsButton_ShowsMessageAboutRegistryItems()
		{
			var system = BMSTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);
			system.FS_Name = "Test System";

			new TimeActionScheduleCollection(Factory).DeleteAll();

			Factory.Save();

			using (var form = new BMSystemManagementForm(system))
			{
				form.Show();
				var button = form.GetControl<ZButton>("UpdateRelatedWorkflowsButton");
				AssertEquals(true, button.Visible);
				AssertEquals(true, button.Enabled);
				UnitTestUserNotification.Instance.AddOKAnswer();
				button.PerformClick();
				AssertEquals("This button is only available when both Workflow Manager > Buffer Management > Responsive PAVE Data Processing > Enable Responsive PAVE Data Processing and Enable Responsive Workflow Updates On Related Object Changes are set in the registry", UnitTestUserNotification.Instance.LastMessage.Text);

				var collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
				AssertContainsExactElementsInAnyOrder("Should not schedule update actions", [], collection.Select(a => a.TAS_TargetPK));
			}
		}

		[TestDate(2025, 01, 23)]
		public void TestUpdateRelatedWorkflowsButton_ConfirmationMessage()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var system1 = BMSTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);

			Factory.Save();

			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
			using (var form = new BMSystemManagementForm(system1))
			{
				form.Show();
				var button = form.GetControl<ZButton>("UpdateRelatedWorkflowsButton");
				AssertEquals(true, button.Visible);
				AssertEquals(true, button.Enabled);

				BMSRegistry.Instance.DelayForResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddOKAnswer();
				button.PerformClick();
				AssertEquals("Updating related workflows has been initiated. It will take some time to process every related workflow.", UnitTestUserNotification.Instance.LastMessage.Text);

				BMSRegistry.Instance.DelayForResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddOKAnswer();
				button.PerformClick();
				AssertEquals("Updating related workflows will be initiated in 1 minute (defined by the Delay For Responsive Workflow Updates On Related Object Changes registry item). It will take some time to process every related workflow.", UnitTestUserNotification.Instance.LastMessage.Text);

				BMSRegistry.Instance.DelayForResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddOKAnswer();
				button.PerformClick();
				AssertEquals("Updating related workflows will be initiated in 2 minutes (defined by the Delay For Responsive Workflow Updates On Related Object Changes registry item). It will take some time to process every related workflow.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Performance

		public void TestDbHits()
		{
			var system = CreateAComplexSystem(Factory);
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			var formSystem = newFactory.Load<BMSystem>(system.PK);
			using (var form = new BMSystemManagementForm(formSystem))
			{
				form.Show();
				Application.DoEvents();

				var tabs = form.FindAll<ZTabControl>();
				var topTabControl = tabs.Single(t => t.TabPages.Cast<ZTabPage>().Any(p => p.Text == "System Schematic"));
				var innerTabControl = tabs.Single(t => t.TabPages.Cast<ZTabPage>().Any(p => p.Text == "Component Links"));

				foreach (var page in innerTabControl.TabPages.Cast<ZTabPage>())
				{
					innerTabControl.SelectNextTabPage();
					Application.DoEvents();
				}

				foreach (var page in topTabControl.TabPages.Cast<ZTabPage>())
				{
					topTabControl.SelectNextTabPage();
					Application.DoEvents();
				}

				Application.DoEvents();
			}

			var expectedHitCounts = new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 3 },
				{ GlbGroupSchema.Constants.TableName, 1 },
				{ StmModuleFilterSchema.Constants.TableName, 2 },
				{ StmModuleFilterUserDataSchema.Constants.TableName, 2 },
				// { StmNoteSchema.Constants.TableName, 3 }, Including this will cause an amnesty for some reason.
				{ BMComponentLinkSchema.Constants.TableName, 2 },
				{ BMControlCustomisationLinkSchema.Constants.TableName, 2 },
				{ BMSystemSchema.Constants.TableName, 2 },
				{ BMComponentReleaseGroupLinkSchema.Constants.TableName, 1 },
				{ BMComponentResourceLinkSchema.Constants.TableName, 1 },
				{ BMControlCustomisationSchema.Constants.TableName, 1 },
				{ BMZoneCapacityMultiplierSchema.Constants.TableName, 1 },
			};

			AssertDbHits(expectedHitCounts, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true, ignoreHitsFromTablesCachedInUberFactory: true);
		}

		public void TestDbHitsValidateAll()
		{
			var system = CreateAComplexSystem(Factory);
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			var expectedHitCounts = new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 },
				{ BMComponentReleaseGroupLinkSchema.Constants.TableName, 1 },
				{ BMComponentResourceLinkSchema.Constants.TableName, 1 },
				{ BMControlCustomisationLinkSchema.Constants.TableName, 1 },
				{ BMZoneCapacityMultiplierSchema.Constants.TableName, 1 },
				{ GenCustomAddOnRuleAckSchema.Constants.TableName, 1 },
				{ GenCustomAddOnValueSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ StmModuleFilterSchema.Constants.TableName, 0 }, // we should never have to hit this table if we dont change filterstrips
				{ StmModuleFilterUserDataSchema.Constants.TableName, 0 } // we should never have to hit this table if we dont change filterstrips
			};

			var formSystem = newFactory.Load<BMSystem>(system.PK);
			using (var form = new BMSystemManagementForm(formSystem))
			{
				form.Show();
				Application.DoEvents();

				formSystem.FS_Name = "This is SPARTAAAAAA";

				var tabs = form.FindAll<ZTabControl>();
				var topTabControl = tabs.Single(t => t.TabPages.Cast<ZTabPage>().Any(p => p.Text == "System Schematic"));
				var innerTabControl = tabs.Single(t => t.TabPages.Cast<ZTabPage>().Any(p => p.Text == "Component Links"));

				foreach (var page in innerTabControl.TabPages.Cast<ZTabPage>())
				{
					innerTabControl.SelectNextTabPage();
					Application.DoEvents();
				}

				foreach (var page in topTabControl.TabPages.Cast<ZTabPage>())
				{
					topTabControl.SelectNextTabPage();
					Application.DoEvents();
				}

				newFactory.ResetDatabaseLoadCount();

				var validateAllMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[2];
				AssertEquals("&Validate All", validateAllMenuItem.Text);
				validateAllMenuItem.PerformClick();

				Application.DoEvents();
				AssertDbHits(expectedHitCounts, newFactory, ignoreHitsFromTablesCachedInUberFactory: true);
			}
		}

		#endregion

		#region Rebinding

		public void TestSortComponentLinkGrid_WhenGridIsReadOnly_ShouldUpdateFilterRuleBinding()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");
			var link1 = config.ComponentLink;
			var link2 = BMSTestHelper.LinkComponents(config.Bucket, config.Buffer, sequence: 100);
			AssertEquals(0, (int)link1.FL_Sequence);

			FilterStripsTestHelper.AddStartsWithFilter(link1.FilterRule, ProcessHeader.ModuleFilterConstants.CompletionStatement, "A");
			FilterStripsTestHelper.AddStartsWithFilter(link2.FilterRule, ProcessHeader.ModuleFilterConstants.CompletionStatement, "B");

			Factory.Save();

			AssertEquals("This problem happens when the system is live and the grid is readonly which allows you to deselect rows", true, config.System.FS_IsLive);

			using (var form = new BMSystemManagementForm(config.System))
			{
				form.Show();
				Application.DoEvents();

				// Swap to other link so that the dependent controls are rebound.
				var componentGrid = form.FindSingle<ZGrid>("BMComponentGrid");
				componentGrid.ListManager.Position = 1;
				Application.DoEvents();
				componentGrid.ListManager.Position = 0;
				Application.DoEvents();

				var grid = form.FindSingle<ZGrid>("ComponentLinkGrid");
				AssertEquals("Editable grids always have a selection which stops the problem from appearing, so for this test the grid must be readonly.", true, grid.ReadOnly);
				AssertEquals(0, (int)((BMComponentLink)grid.ListManager.Current).FL_Sequence);

				var filterStrip = form.FindSingle<ZFilterStrip>();
				var valueBox = filterStrip.FindSingle<ZTextBox>(x => x.GetBindingMember() == "Property");
				AssertEquals("A", valueBox.Text);

				var descriptor = grid.ListManager.GetItemProperties().Find(BMComponentLinkSchema.Constants.FL_Sequence, false);
				((IBindingListView)grid.ListManager.List).ApplySort(descriptor, ListSortDirection.Descending);
				Application.DoEvents();

				AssertEquals(100, (int)((BMComponentLink)grid.ListManager.Current).FL_Sequence);
				filterStrip = form.FindSingle<ZFilterStrip>();
				valueBox = filterStrip.FindSingle<ZTextBox>(x => x.GetBindingMember() == "Property");
				AssertEquals("The filter strips must rebind when the grid is sorted.", "B", valueBox.Text);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		protected override Form GetFormToBashCore()
		{
			return new BMSystemManagementForm(Factory.New<BMSystem>());
		}

		static BMSystem CreateAComplexSystem(BusinessObjectFactory factory)
		{
			var system = BMSTestHelper.CreateSystem(factory, "ORG", "WKI");
			system.FS_Name = "Washing Machine";

			const int numOfComponent = 10;
			const int numOfComponentLinks = 15;
			const int numOfZoneCapacityMultipliers = 20;
			const int numOfReleaseGroups = 25;
			const int numOfResources = 30;

			var groups = Enumerable.Range(0, numOfReleaseGroups).Select(i => factory.NewWithValidTestData<GlbGroup>()).ToArray();
			var releaseGroups = groups.Select(g => BMSTestHelper.CreateReleaseGroup(system, g)).ToArray();

			var resources = Enumerable.Range(0, numOfResources).Select(_ => BMSTestHelper.CreateStaffInCurrentBranchDept(factory)).ToArray();
			resources.ForEach(x => groups.ForEach(g => g.Staff.Add(x)));

			var detailedCards = releaseGroups.Select(r => BMSTestHelper.CreateControlCustomisationLink(factory, r, BMControlCustomisation.GetNewDefaultCardLayout(factory, CustomisedControlTypeList.Codes.DetailedCard)));
			var summaryCards = releaseGroups.Select(r => BMSTestHelper.CreateControlCustomisationLink(factory, r, BMControlCustomisation.GetNewDefaultCardLayout(factory, CustomisedControlTypeList.Codes.TaskCard)));

			var componentList = new BMComponent[numOfComponent];
			for (var i = 0; i < numOfComponent; i++)
			{
				var component = ((i % 2) == 0)
					? BMSTestHelper.CreateBucket(system, "Component " + i)
					: BMSTestHelper.CreateBuffer(system, "Component " + i);
				componentList[i] = component;
			}

			for (var i = 0; i < numOfComponent; i++)
			{
				for (var j = 0; j < numOfComponentLinks && j < numOfComponent; j++)
				{
					if (i == j)
					{
						continue;
					}
					// StmModuleFilterUserData
					var link = BMSTestHelper.LinkComponents(componentList[i], componentList[j]);

					FilterStripsTestHelper.AddFilterStrips(link.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
					{
						FilterStripName = ProcessHeader.ModuleFilterConstants.AutoAssignTasks,
						FilterStripValueSetter = f => ((ModuleFlagsFilter)f).Property0 = true,
					});
				}

				if (componentList[i].FC_Type == BMComponentTypeList.Codes.Buffer)
				{
					for (var j = 0; j < numOfZoneCapacityMultipliers; j++)
					{
						BMSTestHelper.CreateZoneMultiplier(componentList[i], 1, 1, 2, 3, groups[j].PK);
					}
				}

				for (var j = 0; j < numOfReleaseGroups; j++)
				{
					var releaseGroup = componentList[i].ReleaseGroupLinks.AddNew();
					releaseGroup.FO_GG_ReleaseGroup = groups[j].PK;
				}

				for (var j = 0; j < numOfResources; j++)
				{
					var resourceLink = componentList[i].ResourceLinks.AddNew();
					resourceLink.FD_GS_NKResource = resources[j].GS_Code;
					resourceLink.FD_IsCapacityConstrained = true;
				}
			}

			return system;
		}

		void AssertSystemSaving_WhenLivenessToggled(BMSystemManagementForm form, string expectedPrompt, DialogResult resultToExpect, bool expectToBeLive)
		{
			ClickLivenessToggleButton(form, resultToExpect);
			AssertEquals("We saved the system", expectToBeLive, form.BMSystem.FS_IsLive);
			AssertEquals(expectedPrompt, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		/// <param name="resultToTest">YES to toggle, anything else to NOT TOGGLE</param>
		void ClickLivenessToggleButton(BMSystemManagementForm form, DialogResult resultToTest)
		{
			ZFormModaliser.ShowDialogsInTest = true;

			var livenessControls = form.FindAll<ZButton>(cntrl => cntrl.Text.Equals("Configure Components") || cntrl.Text.Equals("Activate System"));
			AssertEquals("There should only be one liveness control at a time", 1, livenessControls.Count());

			var livenessControl = livenessControls.First();
			AssertNotNull("We have a control that changes the System Liveness", livenessControl);

			UnitTestUserNotification.Instance.AddAnswer(resultToTest);
			livenessControl.PerformClick();

			form.Show();
			Application.DoEvents();
		}

		#endregion
	}
}
