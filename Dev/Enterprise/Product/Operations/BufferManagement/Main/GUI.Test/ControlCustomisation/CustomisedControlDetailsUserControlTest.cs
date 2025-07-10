using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI.Test
{
	class CustomisedControlDetailsUserControlTest : BMSTestCaseWithFactory
	{
		#region Property Control Tests - Show MacroFindBox or DropEdit

		public void TestPropertyControl_WhenControlTypeNotSet()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();

			BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, ProcessHeaderSchema.Constants.FH_CompletionStatement, string.Empty, "Job Line", 0, 0, 100, 20, Color.Transparent.Name, Color.Black.Name, 10, false, false, false);

			using (var customisedControlDetail = new CustomisedControlDetailsUserControl())
			using (var form = new ZForm())
			{
				customisedControlDetail.SetDataBinding(customisation, string.Empty);

				form.Controls.Add(customisedControlDetail);
				form.Show();

				Application.DoEvents();

				AssertPropertyName("GIVEN 1st row has source=Job, WHEN displaying property SHOULD show macro-find-box @ grid and disable property-name @ detail-panel",
					customisedControlDetail,
					propertyIsJob: true);
			}
		}

		public void TestPropertyControl_InitialRowSourceIsJob()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();

			BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, ProcessHeaderSchema.Constants.FH_CompletionStatement, PropertyTypeList.Codes.Text, "Job Line", 0, 0, 100, 20, Color.Transparent.Name, Color.Black.Name, 10, false, false, false);
			BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_CompletionStatement, PropertyTypeList.Codes.Text, "Workflow Line", 0, 0, 100, 20, Color.Transparent.Name, Color.Black.Name, 10, false, false, false);

			using (var customisedControlDetail = new CustomisedControlDetailsUserControl())
			using (var form = new ZForm())
			{
				customisedControlDetail.SetDataBinding(customisation, string.Empty);

				form.Controls.Add(customisedControlDetail);
				form.Show();

				Application.DoEvents();

				AssertPropertyName("GIVEN 1st row has source=Job, WHEN displaying property SHOULD show macro-find-box @ grid and disable property-name @ detail-panel",
					customisedControlDetail,
					propertyIsJob: true);

				var propertiesGrid = customisedControlDetail.Find(c => c.Name == "PropertiesGrid").First() as ZGrid;
				propertiesGrid.ListManager.Position = 1;
				propertiesGrid.ListManager.Refresh();

				AssertPropertyName("GIVEN user selected 2nd row that has property=Workflow, WHEN displaying property SHOULD show drop-edit @ grid and enable property-name @ detail-panel",
					customisedControlDetail,
					propertyIsJob: false);
			}
		}

		public void TestPropertyControl_InitialRowSourceIsNotJob()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();

			BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_CompletionStatement, PropertyTypeList.Codes.Text, "Workflow Line", 0, 0, 100, 20, Color.Transparent.Name, Color.Black.Name, 10, false, false, false);
			BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, ProcessHeaderSchema.Constants.FH_CompletionStatement, PropertyTypeList.Codes.Text, "Job Line", 0, 0, 100, 20, Color.Transparent.Name, Color.Black.Name, 10, false, false, false);

			using (var customisedControlDetail = new CustomisedControlDetailsUserControl())
			using (var form = new ZForm())
			{
				customisedControlDetail.SetDataBinding(customisation, string.Empty);

				form.Controls.Add(customisedControlDetail);
				form.Show();

				Application.DoEvents();

				AssertPropertyName("GIVEN 1st row has source=Workflow, WHEN displaying property SHOULD show drop-edit @ grid and enable property-name @ detail-panel",
					customisedControlDetail,
					propertyIsJob: false);

				var propertiesGrid = customisedControlDetail.Find(c => c.Name == "PropertiesGrid").First() as ZGrid;
				propertiesGrid.ListManager.Position = 1;
				propertiesGrid.ListManager.Refresh();

				AssertPropertyName("GIVEN user selected 2nd row that has property=Job, WHEN displaying property SHOULD show macro-find-box @ grid and disable property-name @ detail-pane",
					customisedControlDetail,
					propertyIsJob: true);
			}
		}

		public void TestPropertyControl_ShowMapTreeForm()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();

			BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, ProcessHeaderSchema.Constants.FH_CompletionStatement, PropertyTypeList.Codes.Text, "Job Line", 0, 0, 100, 20, Color.Transparent.Name, Color.Black.Name, 10, false, false, false);

			using (var customisedControlDetail = new CustomisedControlDetailsUserControl())
			using (var form = new ZForm())
			{
				customisedControlDetail.SetDataBinding(customisation, string.Empty);

				form.Controls.Add(customisedControlDetail);
				form.Show();

				Application.DoEvents();

				AssertPropertyName(@"GIVEN 1st row has source=Job WHEN displaying property SHOULD show macro-find-box @ grid and disable property-name @ detail-panel",
					customisedControlDetail,
					propertyIsJob: true);
			}
		}

		void AssertPropertyName(string message, CustomisedControlDetailsUserControl customisedControlDetail, bool propertyIsJob)
		{
			var propertyNameDropEdit = customisedControlDetail.Find(c => c.Name == "PropertyNameDropEdit").First();
			AssertEquals("Property-name @ detail-panel: " + message, propertyIsJob, propertyNameDropEdit.GetReadOnly());

			var propertiesGrid = customisedControlDetail.Find(c => c.Name == "PropertiesGrid").First() as ZGrid;
			var propertyColumnStyle = propertiesGrid.GetColumnStyle("PropertyNameDescription") as ZMultiControlColumnStyleInfo;

			var selectedBizObj = propertiesGrid.ListManager.Current as BusinessObject;

			var styleInfo = selectedBizObj[propertyColumnStyle.FieldTypeColumnName];

			var propertyNameColumnFieldType = styleInfo.ToString();

			AssertEquals("Display-field grid: ",
				propertyIsJob ? "TextMacro" : "TextDropEdit",
				propertyNameColumnFieldType);

			if (propertyIsJob)
			{
				AssertEquals("GIVEN property=Job, WHEN displaying mapTreeform should only allow data-fields-only", true, propertyColumnStyle.DataFieldsOnly);
				AssertEquals("GIVEN property=Job, WHEN displaying mapTreeform should allow for multiple macros", true, propertyColumnStyle.AllowMultipleMacroses);
			}
		}

		#endregion

		public void TestSetValues_ShouldUpdatePreview()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.Width = 100;

			using (var control = new CustomisedControlDetailsUserControl())
			using (var form = new ZForm())
			{
				control.SetDataBinding(customisation, string.Empty);

				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				var previewControl = control.PreviewGroupBox.Controls.OfType<ZUserControl>().First();
				AssertEquals(false, previewControl.IsDisposed);

				customisation.Width = 200;
				AssertEquals("Should post preview action to the message queue - doing it immediately is risky due to objects getting disposed whilst still having pending messages", false, previewControl.IsDisposed);

				Application.DoEvents();

				AssertEquals(true, previewControl.IsDisposed);

				AssertNotNull(control.PreviewGroupBox.Controls.OfType<ZUserControl>().First());
			}
		}

		public void TestExceptionDuringPreview_ShouldShowErrorLabel()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation.Width = 100;

			// This has an invalid value for font size, which will trigger an exception.
			var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_CompletionStatement, PropertyTypeList.Codes.Text, "Blah", 0, 0, 100, 20, Color.White.Name, Color.Black.Name, -999, false, false, false);

			Factory.Save();

			var loadedCustomisation = new BusinessObjectFactory().Load<BMControlCustomisation>(customisation.PK);

			using (var control = new CustomisedControlDetailsUserControl())
			using (var form = new ZForm())
			{
				control.SetDataBinding(loadedCustomisation, string.Empty);

				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				var errorLabel = control.PreviewGroupBox.Controls.OfType<ZLabel>().First();
				AssertEquals("There was an error previewing the control. Please check the property values.", errorLabel.Text);
				AssertStartsWith("Error text should be available in the tag for use in other tests", "System.ArgumentException: Value of '-999", (string)errorLabel.Tag);
			}
		}

		public void TestValidationErrors_ShouldShowErrorLabel()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation.Width = 100;

			var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_CompletionStatement, PropertyTypeList.Codes.Text, "Blah", 0, 0, 100, 20, Color.Transparent.Name, Color.Black.Name, 10, false, false, false);

			using (var control = new CustomisedControlDetailsUserControl())
			using (var form = new ZForm())
			{
				control.SetDataBinding(customisation, string.Empty);

				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				var errorLabel = control.PreviewGroupBox.Controls.OfType<ZLabel>().First();
				AssertEquals("Please correct the validation errors.", errorLabel.Text);
			}
		}

		public void TestInvalidPropertiesShouldNotBeBound()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation.Width = 100;

			var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, "ChocopuddingXTREME", PropertyTypeList.Codes.Text, "Blah", 0, 0, 100, 20, Color.Tomato.Name, Color.Black.Name, 10, false, false, false);

			Factory.Save();

			var loadedCustomisation = new BusinessObjectFactory().Load<BMControlCustomisation>(customisation.PK);

			using (var control = new CustomisedControlDetailsUserControl())
			using (var form = new ZForm())
			{
				control.SetDataBinding(loadedCustomisation, string.Empty);

				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				var errorLabel = control.PreviewGroupBox.Controls.OfType<ZLabel>().First();
				AssertEquals("A property on this Visual Layout has been removed or renamed. \r\nPlease delete or re-set the value before saving.", errorLabel.Text);
			}
		}

		public void TestRenderLayoutOnTheWebCheckbox_ShouldExist_ShouldBeReadOnly()
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var control = new CustomisedControlDetailsUserControl())
			{
				var checkBox = (ZCheckBox)control.Controls.Find("RenderOnTheWebCheckBox", true).First();

				AssertNotNull("The control should contain our checkbutton", checkBox);
				AssertEquals("The checkbox should be visible dependent on WAVE registry item", true, checkBox.Visible);
				AssertEquals("The checkbox should be readonly", true, checkBox.ReadOnly);
				AssertEquals("The checkbox should be default false (for now)", false, checkBox.Checked);
			}

			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var control = new CustomisedControlDetailsUserControl())
			{
				var checkBox = (ZCheckBox)control.Controls.Find("RenderOnTheWebCheckBox", true).First();

				AssertNotNull("The control should contain our checkbutton", checkBox);
				AssertEquals("The checkbox should be visible dependent on WAVE registry item", false, checkBox.Visible);
				AssertEquals("The checkbox should be readonly", true, checkBox.ReadOnly);
				AssertEquals("The checkbox should be default false (for now)", false, checkBox.Checked);
			}
		}

		public void TestClickRestoreDefaultButton()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var custom = template.GenCustomColumnDefinitions.AddNew();
			custom.XC_Name = "Sneakers";
			custom.XC_Type = AddOnColumnDataType.Codes.String;

			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			customisation.FM_JobType = "ORG";
			var taskLine = customisation.CustomisationLines.AddNew();
			taskLine.PropertySource = PropertySourceList.Codes.ProcessTask;
			taskLine.PropertyName = ProcessTasksSchema.P9_Description.Name;
			taskLine.Label = "Cubic Zirconia";

			var workflowLine = customisation.CustomisationLines.AddNew();
			workflowLine.PropertySource = PropertySourceList.Codes.Workflow;
			workflowLine.PropertyName = ProcessHeaderSchema.FH_CompletionStatement.Name;
			workflowLine.Label = "Cat burgling";

			var jobLine = customisation.CustomisationLines.AddNew();
			jobLine.PropertySource = PropertySourceList.Codes.Job;
			jobLine.PropertyName = OrgHeaderSchema.OH_FullName.Name;
			jobLine.Label = "Molloy";

			var customFieldLine = customisation.CustomisationLines.AddNew();
			customFieldLine.PropertySource = PropertySourceList.Codes.Job;
			customFieldLine.PropertyName = "<GetCustomField(Sneakers)>";
			customFieldLine.Label = "For sneaking!";

			Factory.Save();

			using (var form = new BMControlCustomisationForm(customisation) { Size = ControlDpiScalingHelper.NewScaledSize(800, 800) })
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTabControl>("PropertiesTabControl");
				var grid = tabControl.FindSingle<ZGrid>("PropertiesGrid");
				Application.DoEvents();

				grid.CurrentCell = new DataGridCell(0, 0);
				form.FindAndClickButton("RestoreDefaultButton");
				Application.DoEvents();

				AssertEquals("Description", taskLine.Label);

				grid.CurrentCell = new DataGridCell(1, 0);
				form.FindAndClickButton("RestoreDefaultButton");
				Application.DoEvents();

				AssertEquals("Description", workflowLine.Label);

				grid.CurrentCell = new DataGridCell(2, 0);
				form.FindAndClickButton("RestoreDefaultButton");
				Application.DoEvents();

				AssertEquals("OH_FullName", jobLine.Label);

				grid.CurrentCell = new DataGridCell(3, 0);
				form.FindAndClickButton("RestoreDefaultButton");
				Application.DoEvents();

				AssertEquals("Sneakers", customFieldLine.Label);
			}
		}

		public void TestPlayButtonBehaviorDropList_WhenLineTypeIsSTS_ShouldBeVisible()
		{
			AssertButtonBehaviorDropListVisibility(StaticControlTypeList.Codes.StatusButtons, "PlayButtonDropEdit", true);
		}

		public void TestPlayButtonBehaviorDropList_WhenLineTypeIsWRK_ShouldBeVisible()
		{
			AssertButtonBehaviorDropListVisibility(StaticControlTypeList.Codes.WorkingStatusButton, "PlayButtonDropEdit", true);
		}

		public void TestPlayButtonBehaviorDropList_WhenLineTypeIsNotSTSOrWRK_ShouldNotBeVisible()
		{
			foreach (var controlType in new StaticControlTypeList().GetAllCodes().Where(x => x != StaticControlTypeList.Codes.StatusButtons && x != StaticControlTypeList.Codes.WorkingStatusButton))
			{
				AssertButtonBehaviorDropListVisibility(controlType, "PlayButtonDropEdit", false);
			}
		}

		public void TestSuspendButtonBehaviorDropList_WhenLineTypeIsSTS_ShouldBeVisible()
		{
			AssertButtonBehaviorDropListVisibility(StaticControlTypeList.Codes.StatusButtons, "SuspendButtonDropEdit", true);
		}

		public void TestSuspendButtonBehaviorDropList_WhenLineTypeIsSUS_ShouldBeVisible()
		{
			AssertButtonBehaviorDropListVisibility(StaticControlTypeList.Codes.SuspendStatusButton, "SuspendButtonDropEdit", true);
		}

		public void TestSuspendButtonBehaviorDropList_WhenLineTypeIsNotSTSOrSUS_ShouldNotBeVisible()
		{
			foreach (var controlType in new StaticControlTypeList().GetAllCodes().Where(x => x != StaticControlTypeList.Codes.StatusButtons && x != StaticControlTypeList.Codes.SuspendStatusButton))
			{
				AssertButtonBehaviorDropListVisibility(controlType, "SuspendButtonDropEdit", false);
			}
		}

		public void TestCloseTaskButtonBehaviorDropList_WhenLineTypeIsSTS_ShouldBeVisible()
		{
			AssertButtonBehaviorDropListVisibility(StaticControlTypeList.Codes.StatusButtons, "CloseTaskButtonDropEdit", true);
		}

		public void TestCloseTaskButtonBehaviorDropList_WhenLineTypeIsCOM_ShouldBeVisible()
		{
			AssertButtonBehaviorDropListVisibility(StaticControlTypeList.Codes.CompletedStatusButton, "CloseTaskButtonDropEdit", true);
		}

		public void TestCloseTaskButtonBehaviorDropList_WhenLineTypeIsNotSTSOrCOM_ShouldNotBeVisible()
		{
			foreach (var controlType in new StaticControlTypeList().GetAllCodes().Where(x => x != StaticControlTypeList.Codes.StatusButtons && x != StaticControlTypeList.Codes.CompletedStatusButton))
			{
				AssertButtonBehaviorDropListVisibility(controlType, "CloseTaskButtonDropEdit", false);
			}
		}

		void AssertButtonBehaviorDropListVisibility(string controlType, string dropEditName, bool shouldControlBeVisible)
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.Label, "For rebinding", 0, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			BMSTestHelper.CreateStaticControlCustomisation(customisation, controlType, string.Empty, 120, 40, 0, 0, string.Empty, string.Empty, 8, false, false);

			using (var form = new BMControlCustomisationForm(customisation) { Size = ControlDpiScalingHelper.NewScaledSize(800, 800) })
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTabControl>("PropertiesTabControl");
				tabControl.SelectNextTabPage();
				Application.DoEvents();
				AssertEquals("Actions and Information", tabControl.SelectedTab.Text);

				// We've added a dummy row at the start of the grid, to prove that the hiding/showing of the drop edits happens when the grid selection changes.
				var dropEdit = tabControl.SelectedTab.FindSingle<ZDropEdit>(dropEditName);
				AssertEquals("The Label control row should be selected by default, so no relevant drop edits should be visible. SAD!", false, dropEdit.Visible);

				var grid = tabControl.FindSingle<ZGrid>("ControlsGrid");
				grid.CurrentCell = new DataGridCell(1, 0);
				Application.DoEvents();

				AssertEquals($"Rebinding the common control to the {controlType} row should update the relevant drop edits' visibility. SAD!", shouldControlBeVisible, dropEdit.Visible);

				grid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();

				AssertEquals("Going back to the label row should hide any relevant controls again. SAD!", false, dropEdit.Visible);
			}
		}

		public void TestChangeLineControlType_ShouldUpdateDropEditVisibility()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.Label, "For changing", 0, 10, 0, 0, string.Empty, string.Empty, 8, false, false);

			using (var form = new BMControlCustomisationForm(customisation) { Size = ControlDpiScalingHelper.NewScaledSize(800, 800) })
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTabControl>("PropertiesTabControl");
				tabControl.SelectNextTabPage();
				Application.DoEvents();
				AssertEquals("Actions and Information", tabControl.SelectedTab.Text);

				var playDropEdit = tabControl.SelectedTab.FindSingle<ZDropEdit>("PlayButtonDropEdit");
				var suspendDropEdit = tabControl.SelectedTab.FindSingle<ZDropEdit>("SuspendButtonDropEdit");
				var closeTaskDropEdit = tabControl.SelectedTab.FindSingle<ZDropEdit>("CloseTaskButtonDropEdit");
				AssertEquals("The Label control row should be selected by default, so no relevant drop edits should be visible. SAD!", false, playDropEdit.Visible);
				AssertEquals("The Label control row should be selected by default, so no relevant drop edits should be visible. SAD!", false, suspendDropEdit.Visible);
				AssertEquals("The Label control row should be selected by default, so no relevant drop edits should be visible. SAD!", false, closeTaskDropEdit.Visible);

				var grid = tabControl.FindSingle<ZGrid>("ControlsGrid");
				var line = (StaticControlCustomisation)grid.GetCurrent();
				line.ControlType = StaticControlTypeList.Codes.StatusButtons;
				Application.DoEvents();

				AssertEquals("Changing the control type should update the relevant drop edits' visibility. SAD!", true, playDropEdit.Visible);
				AssertEquals("Changing the control type should update the relevant drop edits' visibility. SAD!", true, suspendDropEdit.Visible);
				AssertEquals("Changing the control type should update the relevant drop edits' visibility. SAD!", true, closeTaskDropEdit.Visible);

				line.ControlType = StaticControlTypeList.Codes.WorkingStatusButton;
				Application.DoEvents();

				AssertEquals("Changing the control type should update the relevant drop edits' visibility. SAD!", true, playDropEdit.Visible);
				AssertEquals("Changing the control type should update the relevant drop edits' visibility. SAD!", false, suspendDropEdit.Visible);
				AssertEquals("Changing the control type should update the relevant drop edits' visibility. SAD!", false, closeTaskDropEdit.Visible);

				line.ControlType = StaticControlTypeList.Codes.SuspendStatusButton;
				Application.DoEvents();

				AssertEquals("Changing the control type should update the relevant drop edits' visibility. SAD!", false, playDropEdit.Visible);
				AssertEquals("Changing the control type should update the relevant drop edits' visibility. SAD!", true, suspendDropEdit.Visible);
				AssertEquals("Changing the control type should update the relevant drop edits' visibility. SAD!", false, closeTaskDropEdit.Visible);

				line.ControlType = StaticControlTypeList.Codes.CompletedStatusButton;
				Application.DoEvents();

				AssertEquals("Changing the control type should update the relevant drop edits' visibility. SAD!", false, playDropEdit.Visible);
				AssertEquals("Changing the control type should update the relevant drop edits' visibility. SAD!", false, suspendDropEdit.Visible);
				AssertEquals("Changing the control type should update the relevant drop edits' visibility. SAD!", true, closeTaskDropEdit.Visible);
			}
		}
	}
}
