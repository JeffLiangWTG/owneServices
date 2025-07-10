using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class CustomisedControlRendererTest : BMSGUITestCase
	{
		#region Control Type

		public void TestCustomControls_ShouldRenderZCodeFindbox_WhenPropertyHasListAndRelatedBusinessObjectAttributes()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var processHeader = ProcessJobHeader.GetForParent(orgHeader, Factory).ProcessHeaders.AddNew();
			processHeader.FH_Status = WorkflowStatusList.Codes.Open;
			var processTask = processHeader.Parent.WorkflowItems.AddNew();
			processTask.P9_FH_ProcessHeader = processHeader.PK;
			var propertyName = nameof(OrgHeader.OH_RSL_ShippingLine);

			customisation.FM_JobType = "ORG";
			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation, new CellContent(0, 0, CellContentType.Cards));
			var lineWorkflow = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, propertyName, PropertyTypeList.Codes.Text);

			var property = CustomisationLineReflectionHelper.GetPropertyInfo(typeof(OrgHeader), lineWorkflow.PropertyName);
			var propertyHasAttributes = property.Name == propertyName
				&& property.CustomAttributes.Any(a => a.AttributeType == typeof(ListAttribute))
				&& property.CustomAttributes.Any(a => a.AttributeType == typeof(RelatedBusinessObjectAttribute));

			AssertEquals("Pre-condition", true, propertyHasAttributes);

			using (DisableAsyncBehaviour())
			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				control.SetDataBinding(processTask, string.Empty);
				form.Controls.Add(control);

				form.Show();
				Application.DoEvents();
				var lineWorkflowControl = control.Find(c => c.Tag == lineWorkflow).First() as CustomisedLayoutCodeFindBox;

				AssertNotNull("Should render ZCodeFindBox control", lineWorkflowControl);
				AssertEquals("Shouldn't show description box", false, lineWorkflowControl.ShowDescriptionBox);
				AssertEquals("Shouldn't auto size", false, lineWorkflowControl.AutoSize);
				AssertEquals("Shouldn't resize", false, lineWorkflowControl.ShouldResize);
			}

			AssertEquals("Shouldn't report error when render", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestCustomControls_ShouldRenderReadOnlyZTextBox_WhenFieldIsReadOnlyAndHasLabel()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var processHeader = ProcessJobHeader.GetForParent(orgHeader, Factory).ProcessHeaders.AddNew();
			var processTask = processHeader.Parent.WorkflowItems.AddNew();
			processTask.P9_FH_ProcessHeader = processHeader.PK;

			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, type: CustomisedControlTypeList.Codes.DetailedCard);
			customisation.FM_JobType = "ORG";

			var readOnlyJobLineWithLabel = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "<WorkflowItems.CardStatusDescription>", PropertyTypeList.Codes.Text);
			readOnlyJobLineWithLabel.IsReadOnly = true;
			readOnlyJobLineWithLabel.Label = "Status";

			var readOnlyJobLineWithoutLabel = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "<WorkflowItems.CardStatusDescription>", PropertyTypeList.Codes.Text);
			readOnlyJobLineWithoutLabel.IsReadOnly = true;

			var nonReadOnlyJobLineWithLabel = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "<WorkflowItems.CardStatusDescription>", PropertyTypeList.Codes.Text);
			nonReadOnlyJobLineWithLabel.IsReadOnly = false;
			nonReadOnlyJobLineWithLabel.Label = "Status";

			var nonReadOnlyJobLineWithoutLabel = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "<WorkflowItems.CardStatusDescription>", PropertyTypeList.Codes.Text);
			nonReadOnlyJobLineWithoutLabel.IsReadOnly = false;

			var readOnlyTransparentJobLineWithLabel = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "<WorkflowItems.CardStatusDescription>", PropertyTypeList.Codes.Text);
			readOnlyTransparentJobLineWithLabel.IsReadOnly = true;
			readOnlyTransparentJobLineWithLabel.Label = "Status";
			readOnlyTransparentJobLineWithLabel.BackgroundColor = "Transparent";

			var readOnlyWorkflowLineWithLabel = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, "FH_CompletionStatement", PropertyTypeList.Codes.Text);
			readOnlyWorkflowLineWithLabel.IsReadOnly = true;

			var readOnlyTaskLineWithLabel = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.ProcessTask, "P9_CardNote", PropertyTypeList.Codes.Text);
			readOnlyTaskLineWithLabel.IsReadOnly = true;

			var readOnlyWorkflowLineWithoutLabel = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, "FH_CompletionStatement", PropertyTypeList.Codes.Text);
			readOnlyWorkflowLineWithoutLabel.IsReadOnly = true;
			readOnlyWorkflowLineWithoutLabel.Label = string.Empty;

			var readOnlyTaskLineWithoutLabel = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.ProcessTask, "P9_CardNote", PropertyTypeList.Codes.Text);
			readOnlyTaskLineWithoutLabel.IsReadOnly = true;
			readOnlyTaskLineWithoutLabel.Label = string.Empty;

			Factory.Save();

			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation, new CellContent(0, 0, CellContentType.Cards));

			using (DisableAsyncBehaviour())
			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				control.SetDataBinding(processTask, string.Empty);
				form.Controls.Add(control);

				form.Show();
				Application.DoEvents();

				var readOnlyJobLineWithLabelControl = control.Find(c => c.Tag == readOnlyJobLineWithLabel).FirstOrDefault() as ZTextBox;
				var readOnlyJobLineWithoutLabelControl = control.Find(c => c.Tag == readOnlyJobLineWithoutLabel).FirstOrDefault() as DirectionalLabel;
				var nonReadOnlyJobLineWithLabelControl = control.Find(c => c.Tag == nonReadOnlyJobLineWithLabel).FirstOrDefault() as ZTextBox;
				var nonReadOnlyJobLineWithoutLabelControl = control.Find(c => c.Tag == nonReadOnlyJobLineWithoutLabel).FirstOrDefault() as ZTextBox;
				var readOnlyTransparentJobLineWithLabelControl = control.Find(c => c.Tag == readOnlyTransparentJobLineWithLabel).FirstOrDefault() as DirectionalLabel;
				var readOnlyWorkflowWithLabelControl = control.Find(c => c.Tag == readOnlyWorkflowLineWithLabel).FirstOrDefault() as ZTextBox;
				var readOnlyTaskWithLabelControl = control.Find(c => c.Tag == readOnlyTaskLineWithLabel).FirstOrDefault() as ZTextBox;
				var readOnlyWorkflowWithoutLabelControl = control.Find(c => c.Tag == readOnlyWorkflowLineWithoutLabel).FirstOrDefault() as DirectionalLabel;
				var readOnlyTaskWithoutLabelControl = control.Find(c => c.Tag == readOnlyTaskLineWithoutLabel).FirstOrDefault() as DirectionalLabel;

				AssertNotNull(readOnlyJobLineWithLabelControl);
				AssertNotNull(readOnlyJobLineWithoutLabelControl);
				AssertNotNull(nonReadOnlyJobLineWithLabelControl);
				AssertNotNull(nonReadOnlyJobLineWithoutLabelControl);
				AssertNotNull(readOnlyTransparentJobLineWithLabelControl);
				AssertNotNull(readOnlyWorkflowWithLabelControl);
				AssertNotNull(readOnlyTaskWithLabelControl);
			}

			foreach (BMControlCustomisationLine line in customisation.CustomisationLines)
			{
				line.BackgroundColor = Color.Transparent.Name;
			}
			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				control.SetDataBinding(processTask, string.Empty);
				form.Controls.Add(control);

				AssertNoExceptionThrown(() =>
				{
					form.Show();
					Application.DoEvents();
				});
			}
		}

		public void TestCustomControls_ShouldRenderZDropEdit_WhenPropertyHasListAttribute()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var processHeader = ProcessJobHeader.GetForParent(orgHeader, Factory).ProcessHeaders.AddNew();
			processHeader.FH_Status = WorkflowStatusList.Codes.Open;
			var processTask = processHeader.Parent.WorkflowItems.AddNew();
			processTask.P9_FH_ProcessHeader = processHeader.PK;

			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation, new CellContent(0, 0, CellContentType.Cards));
			var lineWorkflow = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, "FH_Status", PropertyTypeList.Codes.Text); //FH_Status has List Attribute check in

			var property = CustomisationLineReflectionHelper.GetPropertyInfo(typeof(ProcessHeader), lineWorkflow.PropertyName);
			var propertyHasListAttribute = property.Name == "FH_Status"
				&& property.CustomAttributes.Any(a => a.AttributeType == typeof(ListAttribute))
				&& property.CustomAttributes.Any(a => a.AttributeType == typeof(ReadOnlyAttribute));

			AssertEquals("Pre-condition", true, propertyHasListAttribute);

			using (DisableAsyncBehaviour())
			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				control.SetDataBinding(processTask, string.Empty);
				form.Controls.Add(control);

				form.Show();
				Application.DoEvents();
				var lineWorkflowControl = control.Find(c => c.Tag == lineWorkflow).First() as ZDropEdit;

				AssertNotNull("Should render ZDropEdit control", lineWorkflowControl);
				Assert("List should have items", lineWorkflowControl.List.Count > 0);
			}

			AssertEquals("Shouldn't report error when render", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestCustomControls_ShouldRenderZDropEdit_WhenPropertyIsNotReadOnlyAndHasListAttribute()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var processHeader = ProcessJobHeader.GetForParent(orgHeader, Factory).ProcessHeaders.AddNew();
			processHeader.FH_Status = WorkflowStatusList.Codes.Open;
			var processTask = processHeader.Parent.WorkflowItems.AddNew();
			processTask.P9_FH_ProcessHeader = processHeader.PK;

			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation, new CellContent(0, 0, CellContentType.Cards));
			var lineWorkflow = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, "FH_EarliestStartDateDefaultsFrom", PropertyTypeList.Codes.Text);

			var property = CustomisationLineReflectionHelper.GetPropertyInfo(typeof(ProcessHeader), lineWorkflow.PropertyName);
			var propertyHasListAndNoReadOnlyAttribute = property.Name == "FH_EarliestStartDateDefaultsFrom"
				&& property.CustomAttributes.Any(a => a.AttributeType == typeof(ListAttribute))
				&& property.CustomAttributes.Any(a => a.AttributeType != typeof(ReadOnlyAttribute));

			AssertEquals("Pre-condition", true, propertyHasListAndNoReadOnlyAttribute);

			using (DisableAsyncBehaviour())
			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				control.SetDataBinding(processTask, string.Empty);
				form.Controls.Add(control);

				form.Show();
				Application.DoEvents();
				var lineWorkflowControl = control.Find(c => c.Tag == lineWorkflow).First() as ZDropEdit;

				AssertNotNull("Should render ZDropEdit control", lineWorkflowControl);
			}

			AssertEquals("Shouldn't report error when render", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestCustomControlsRebind_Static()
		{
			var tag = BMSTestHelper.CreateTagDefinition(Factory, "JOO");
			var mag1 = BMSTestHelper.CreateTagMagnitude(tag, "WOO", color: Color.Aqua);
			var mag2 = BMSTestHelper.CreateTagMagnitude(tag, "COG", color: Color.Brown);

			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			var staff = CreateStaffInCurrentBranchDept("NON", "NonBon the Tron");

			var workflow1 = CreateWorkflow(CreateJobHeader<OrgHeader>(false), "Bung");
			var workflow2 = CreateWorkflow(CreateJobHeader<OrgHeader>(false), "Hole");
			workflow2.FH_DateAcceptability = DateAcceptabilityList.Codes.GraduatedStartSharpFinish;

			var task1 = CreateTask(workflow1, staff.GS_Code, 60, taskStatus: "WRK", description: "Gork");
			task1.P9_ActualDuration = new ZInt(1).GetDateTimeFromMinutes();

			CreateTask(workflow2, "", 120, description: "Girk");
			var task2 = CreateTask(workflow2, "", 120, description: "Shamoo");

			workflow1.AddTag(mag1);
			workflow2.AddTag(mag2);

			var cell = new CellContent(0, 0, CellContentType.Cards);

			CombineAssertions(() =>
			{
				foreach (var staticStatus in new StaticControlTypeList().Cast<CodeDescriptionPair>().Where(p => StaticControlCustomisation.IsReadonlyControlType(p.Code)))
				{
					var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, name: "Flowers", width: 500, height: 50);
					customisation.CustomisationLines.RemoveAll();
					var statusIndicatorLine = BMSTestHelper.CreateStaticControlCustomisation(customisation, staticStatus.Code, string.Empty, 0, 0, 500, 30, "Transperant", "Black", 8, false, false);

					section.SectionConfiguration.CustomisedLayoutLinks.DeleteAll();
					BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);
					Factory.Save();
					AssertConfigurationsRenderCorrectly(staticStatus.Description, section, task1, task2, cell);

					customisation.Delete();
				}
			});
		}

		void AssertConfigurationsRenderCorrectly(string description, BMBoardSection section, ProcessTask task1, ProcessTask task2, CellContent cell)
		{
			var viewModel = BMSTestHelper.CreateViewModel(section, (ProcessHeader)task1.ProcessHeader, (ProcessHeader)task2.ProcessHeader);

			var content1 = new TaskCardContent(task2, viewModel); // We go in reverse order to protect against status visibility bugs.
			var content2 = new TaskCardContent(task1, viewModel);

			using (var provider = new CachingTaskCardRenderer(new StaggeredCardRenderer()))
			using (var menuStrip = TaskCardControl.CreateMenuStrip())
			using (var card1 = provider.ConstructTaskCardControl(menuStrip, content1, viewModel, cell, canUseBitmapCache: true))
			using (var card1Old = new TaskCardControl(content1, viewModel, cell, canUseBitmapCache: false))
			using (var card2 = provider.ConstructTaskCardControl(menuStrip, content2, viewModel, cell, canUseBitmapCache: true))
			using (var card2Old = new TaskCardControl(content2, viewModel, cell, canUseBitmapCache: false))
			{
				AssertBitmapEquals(string.Format("The customisation for [{0}] wasn't right for the first pass.", description), card1Old, card1);
				AssertBitmapEquals(string.Format("The customisation for [{0}] wasn't right for the rebinder.", description), card2Old, card2);
			}
		}

		public void TestRenderDefaultFont_WhenSpecfiedFontIsNotInstalled()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var propertyLine = customisation.CustomisationLines.AddNew();
			propertyLine.ControlType = PropertyTypeList.Codes.Text;
			propertyLine.Font = "hwsask8rboi";

			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation);

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var lineControl = control.Find(c => c.Tag == propertyLine).First() as ZTextBox;
				AssertNotNull(lineControl);
#if !WINZOR
				AssertEquals("Microsoft Sans Serif", lineControl.Font.Name);
#else
				// The default font is new Font("Tahoma", 8) in the WinzorFramework Control.
				// Dev\Winzor\BArchitecture.GUI\System.Windows.Forms\Control.cs Control.DefaultFont
				AssertEquals("Tahoma", lineControl.Font.Name);
#endif
			}
		}

		public void TestRenderVerticalTextField_ShouldUseVerticalLabel()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var propertyLine = customisation.CustomisationLines.AddNew();
			propertyLine.IsReadOnly = true;
			propertyLine.ControlType = PropertyTypeList.Codes.Text;
			propertyLine.Orientation = "Vertical";

			var customisedLabel = customisation.CustomisedControls.AddNew();
			customisedLabel.ControlType = StaticControlTypeList.Codes.Label;
			customisedLabel.Label = "Something";
			customisedLabel.Orientation = "Vertical";

			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation);

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var lineControl = control.Find(c => c.Tag == propertyLine).First();
				AssertType<DirectionalLabel>(lineControl);

				var labelControl = control.Find(c => c.Tag == customisedLabel).First();
				AssertType<DirectionalLabel>(labelControl);
			}
		}

		public void TestRender_NullParentProperty()
		{
			var newFactory = Factory.CreateNewFactory();
			var task = newFactory.New<ProcessTask>();
			task.P9_ParentID = ZGuid.Empty;
			task.P9_ParentTableCode = ZString.Empty;
			newFactory.Save();

			var loadedTask = Factory.Load<ProcessTask>(task.PK);
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "OH_IsActive", PropertyTypeList.Codes.Boolean, "Blah", 20, 200, 40, 50, Color.Khaki.Name, Color.Lavender.Name, 10, true, true, false);
			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(customisation.Factory);

			var viewModel = new ControlCustomisationViewModel(customisation, sectionViewModel, new TaskCardContent(loadedTask, sectionViewModel), new CellContent(0, 0, CellContentType.Cards));

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var lineControl = control.Find(c => c.Tag == line).First();

				AssertControl(lineControl, typeof(ZCheckBox), string.Empty, "Blah", 20, 200, 40, 50, null, Color.Lavender, 10, true);
			}
		}

		public void TestRenderTextDropdown_ShouldNotUpdateDropEditDescriptionInternally()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var processTask = CreateTask(workflow, string.Empty, 60, taskStatus: "SUS");

			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation, new CellContent(0, 0, CellContentType.Cards));
			var lineProcessTask = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.ProcessTask, "P9_Status", PropertyTypeList.Codes.Text, "status label ree", 1, 1, 80, 20, Color.Khaki.Name, Color.Lavender.Name, 10, true, false, false);

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				control.SetDataBinding(processTask, string.Empty);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var dropEditControl = control.FindAll<ZDropEdit>().First();
				AssertEquals("Selected Index is default to -1 (UpdateSelectedIndexAndDescription not called)", -1, dropEditControl.SelectedIndex);
				AssertEquals("DropEdit text box displays correct status", "SUS", dropEditControl.Text);

				var dropCodeBoxControl = dropEditControl.FindAll<ZDropCodeBox>().First();
				KeySender.PostKeyDown(dropCodeBoxControl, Keys.W);
				Application.DoEvents();
				UserIdleWorker.Flush();

				AssertEquals("Selected Index is still at default -1", -1, dropEditControl.SelectedIndex);
				AssertEquals("Autocomplete selects predicted text", "WRK", dropEditControl.Text);
			}
		}

		public void TestStaticControls()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var customisedControl1 = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.Label, "Mai Label", 0, 10, 200, 40, Color.Green.Name, Color.Orange.Name, 8, false, false, ControlAlignmentList.Codes.Right);
			var customisedControl2 = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.CloseButton, "Cloze", 20, 10, 0, 0, Color.Green.Name, Color.Orange.Name, 8, true, false);
			var customisedControl3 = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.OpenJobButton, "Open Jahb", 40, 10, 0, 0, Color.Green.Name, Color.Orange.Name, 8, true, false);
			var customisedControl4 = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.SaveButton, "Save", 60, 10, 0, 0, Color.Green.Name, Color.Orange.Name, 8, false, false);
			var customisedControl5 = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.CapabilityAssignmentButton, string.Empty, 80, 10, 200, 40, Color.Green.Name, Color.Orange.Name, 8, false, false);
			var customisedControl6 = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.NudgeControls, string.Empty, 100, 10, 0, 0, Color.Green.Name, Color.Orange.Name, 8, false, false);
			var customisedControl7 = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.StatusButtons, string.Empty, 120, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			var customisedControl8 = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.TaskStatusIndicator, string.Empty, 140, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			var customisedControl9 = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.DateAcceptabilityPicture, string.Empty, 160, 10, 0, 0, Color.Green.Name, Color.Orange.Name, 8, false, false);
			var customisedControl11 = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.WorkingStatusButton, "Working", 220, 50, 0, 0, Color.Green.Name, Color.Orange.Name, 8, false, false);
			var customisedControl12 = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.SuspendStatusButton, "Suspend", 240, 50, 0, 0, Color.Green.Name, Color.Orange.Name, 8, false, false);
			var customisedControl13 = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.CompletedStatusButton, "Complete", 260, 50, 0, 0, Color.Green.Name, Color.Orange.Name, 8, false, false);
			var customisedControl14 = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.AttachedTagsIndicator, "TAG", 280, 20, 100, 10, Color.Green.Name, Color.Orange.Name, 8, false, false);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, string.Empty, 60);
			var viewModel = new ControlCustomisationViewModel(customisation, new TaskCardContent(task, BMSTestHelper.CreateDummyViewModel(Factory)));
			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				ControlTestHelper.AssertControlDimensions(300, 200, control);
				AssertColorEquals(Color.HotPink, control.BackColor);

				var line1Control = control.Find(c => c.Tag == customisedControl1).First();
				var line2Control = control.Find(c => c.Tag == customisedControl2).First();
				var line3Control = control.Find(c => c.Tag == customisedControl3).First();
				var line4Control = control.Find(c => c.Tag == customisedControl4).First();
				var line5Control = control.Find(c => c.Tag == customisedControl5).First();
				var line6Control = control.Find(c => c.Tag == customisedControl6).First();
				var line7Control = control.Find(c => c.Tag == customisedControl7).First();
				var line8Control = control.Find(c => c.Tag == customisedControl8).First();
				var line9Control = control.Find(c => c.Tag == customisedControl9).First();
				var line11Control = control.Find(c => c.Tag == customisedControl11).First();
				var line12Control = control.Find(c => c.Tag == customisedControl12).First();
				var line13Control = control.Find(c => c.Tag == customisedControl13).First();
				var line14Control = control.Find(c => c.Tag == customisedControl14).First();

				AssertControl(line1Control, typeof(DirectionalLabel), string.Empty, "Mai Label", 0, 10, 200, 40, Color.Green, Color.Orange, 8, false, anchor: AnchorStyles.Right | AnchorStyles.Top);
				AssertControl(line2Control, typeof(CloseCardButton), string.Empty, "Cloze", 20, 10, 25, 25, Color.Green, Color.Orange, 8, true);
				AssertControl(line3Control, typeof(OpenJobButton), string.Empty, "Open Jahb", 40, 10, 61, 22, Color.Green, Color.Orange, 8, true);
				AssertControl(line4Control, typeof(SaveButton), string.Empty, "Save", 60, 10, 40, 22, Color.Green, Color.Orange, 8, false);
				AssertControl(line5Control, typeof(CapabilityAssignmentButton), string.Empty, string.Empty, 80, 10, 200, 40, Color.Green, Color.Orange, 8, false);
				AssertControl(line6Control, typeof(NudgeControls), string.Empty, string.Empty, 100, 10, 84, 26, Color.Green, Color.Orange, 8, false);
				AssertControl(line7Control, typeof(TaskStatusControl), string.Empty, string.Empty, 120, 10, 211, 26, null, null, 8, false);
				AssertControl(line8Control, typeof(StatusIndicatorControl), string.Empty, string.Empty, 140, 10, 12, 12, null, null, 8, false);
				AssertControl(line9Control, typeof(DateAcceptabilityPicture), string.Empty, string.Empty, 160, 10, 58, 20, Color.Green, Color.Orange, 8, false);
				AssertControl(line11Control, typeof(GenericStatusChangeButton), string.Empty, string.Empty, 220, 50, 23, 23, Color.Green, Color.Orange, 8, false);
				AssertControl(line12Control, typeof(GenericStatusChangeButton), string.Empty, string.Empty, 240, 50, 23, 23, Color.Green, Color.Orange, 8, false);
				AssertControl(line13Control, typeof(GenericStatusChangeButton), string.Empty, string.Empty, 260, 50, 23, 23, Color.Green, Color.Orange, 8, false);
				AssertControl(line14Control, typeof(AttachedTagsIndicator), string.Empty, string.Empty, 280, 20, 100, 10, Color.Transparent, Color.Orange, 8, false);
			}
		}

#endregion

		#region Preview

		public void TestWhenCodeFindBoxInPreviewMode_PopupButtonShouldBeDisabled()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_FC_CurrentComponent, PropertyTypeList.Codes.Text);

			using (var control = CustomisedControlRenderer.RenderForPreview(customisation))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var lineControl = control.Find(c => c.Tag == line).First() as CustomisedLayoutCodeFindBox;
				AssertEquals("Should disable PopupButton", false, lineControl.PopupButton.Enabled);
			}
		}

		[ExpectNoExceptions]
		public void TestLookupsOnDummyObjectsAreAccessible()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_DateAcceptability, PropertyTypeList.Codes.Text, "Tag Me Baby One More Time", 1, 1, 100, 20, Color.White.Name, Color.Black.Name, 8, false, false, false);

			using (var control = CustomisedControlRenderer.RenderForPreview(customisation))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var lineControl = (ZDropEdit)control.Find(c => c.Tag == line).First();
				AssertEquals(new DateAcceptabilityList().Count, lineControl.List.Count);
			}
		}

		public void TestRenderInPreviewMode_JobProperty_ShouldJustSetPropertyNameWithoutTablePrefix()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "OH_MaiProperty", PropertyTypeList.Codes.Text, "IAmA Code - Ask Me Anything", 1, 1, 100, 20, Color.White.Name, Color.Black.Name, 8, false, false, false);

			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation);
			AssertEquals(true, viewModel.IsPreview);

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var lineControl = control.Find(c => c.Tag == line).First();
				AssertEquals("MaiProperty", lineControl.Text);
			}
		}

		public void TestRenderInPreviewMode_JobPropertyWithUnderscore_ShouldJustSetPropertyName()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "OHai_MaiProperty", PropertyTypeList.Codes.Text, "IAmA Code - Ask Me Anything", 1, 1, 100, 20, Color.White.Name, Color.Black.Name, 8, false, false, false);

			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation);
			AssertEquals(true, viewModel.IsPreview);

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var lineControl = control.Find(c => c.Tag == line).First();
				AssertEquals("OHai_MaiProperty", lineControl.Text);
			}
		}

		public void TestRenderInPreviewMode_JobPropertyNameEndsWithUnderscore_ShouldJustSetPropertyName()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "MaiProperty_", PropertyTypeList.Codes.Text, "IAmA Code - Ask Me Anything", 1, 1, 100, 20, Color.White.Name, Color.Black.Name, 8, false, false, false);

			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation);
			AssertEquals(true, viewModel.IsPreview);

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var lineControl = control.Find(c => c.Tag == line).First();
				AssertEquals("MaiProperty_", lineControl.Text);
			}
		}

		public void TestRenderInPreviewMode_TaskPropertyWithEmptyLabel()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.ProcessTask, "P9_Description", PropertyTypeList.Codes.Text, "Some Caption", 1, 1, 100, 20, Color.White.Name, Color.Black.Name, 8, false, false, false);
			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation);

			AssertEquals(true, viewModel.IsPreview);

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var lineControl = control.Find(c => c.Tag == line).First();

				AssertEquals(true, new LabelCaptionRenderProvider().GetLabelCaptionVisible(lineControl));

				line.Label = ZString.Empty;

				Factory.Save();
			}

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var lineControl = control.Find(c => c.Tag == line).First();

				AssertEquals(false, new LabelCaptionRenderProvider().GetLabelCaptionVisible(lineControl));
			}
		}

		#endregion

		#region Render Layout

		public void TestStaggeredRenderer_FrontItemIsFrontItem_IgnoreInvisibility()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = CreateBoardSection(config.Bucket);
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;

			var staff = CreateStaffInCurrentBranchDept("NON", "NonBon the Tron");

			var workflow1 = CreateWorkflow(CreateJobHeader<OrgHeader>(false), "Katherine", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);

			var task1 = CreateTask(workflow1, staff.GS_Code, 60, description: "LOTR");
			var task2 = CreateTask(workflow1, "", 120, taskStatus: "SUS", description: "HOBBIT");

			Factory.Save();

			var cell = new CellContent(0, 0, CellContentType.Cards);

			using (DisableAsyncBehaviour())
			using (var form = TaskPanelTest.GetAndShowVisualBoardFormWithTaskPanel(section.Board, height: 300))
			{
				var viewModel = (BMBoardSectionViewModel)form.BoardViewModel.GetSections().Single();
				var taskPanel = form.FindSingle<TaskPanel>();

				taskPanel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>());

				Application.DoEvents();

				var cards = taskPanel.FindAll<TaskCardControl>().Where(t => t.Visible).ToArray();
				AssertEquals(2, cards.Length);
				AssertEquals(task2.PK, cards[0].CardContent.TaskIdentifier);

				AssertEquals(viewModel.GetCardBitmaps(cards[0].CardContent).NormalBitmap, cards[0].BackgroundImage);
				AssertEquals(viewModel.GetCardBitmaps(cards[1].CardContent).DarkenedBitmap, cards[1].BackgroundImage);

				taskPanel.SetupTasksForTest(Factory, null, new[] { new CurrentTaskFilter() });
				Application.DoEvents();

				cards = taskPanel.FindAll<TaskCardControl>().Where(t => t.Visible).ToArray();
				AssertEquals(1, cards.Length);
				AssertEquals(task1.PK, cards[0].CardContent.TaskIdentifier);
				AssertEquals("The new front card has the right background", viewModel.GetCardBitmaps(cards[0].CardContent).NormalBitmap, cards[0].BackgroundImage);
			}
		}
		public void TestRenderInPreviewMode_CustomFieldProperty()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "<GetCustomField(schmeckels)>", PropertyTypeList.Codes.Text, "Some Caption", 1, 1, 100, 20, Color.White.Name, Color.Black.Name, 8, false, false, false);
			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation);

			AssertEquals(true, viewModel.IsPreview);

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var lineControl = control.Find(c => c.Tag == line).First() as ZTextBox;

				AssertEquals("<GetCustomField(schmeckels)>", lineControl.Text);
			}
		}

		public void TestRenderInPreviewMode_CustomFieldProperty_SansGreaterThan()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "<GetCustomField(schmeckels)", PropertyTypeList.Codes.Text, "Some Caption", 1, 1, 100, 20, Color.White.Name, Color.Black.Name, 8, false, false, false);
			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation);

			AssertEquals(true, viewModel.IsPreview);

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var lineControl = control.Find(c => c.Tag == line).First() as ZTextBox;

				AssertEquals("<GetCustomField(schmeckels)", lineControl.Text);
			}
		}

		public void TestRenderInPreviewMode_CustomFieldProperty_Checkbox()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };

			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var custom = template.GenCustomColumnDefinitions.AddNew();
			custom.XC_Name = "schquestion";
			custom.XC_Type = AddOnColumnDataType.Codes.Boolean;

			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "<GetCustomField(schquestion)>", PropertyTypeList.Codes.Boolean, "Some Caption", 1, 1, 100, 20, Color.White.Name, Color.Black.Name, 8, false, false, false);
			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation);

			AssertEquals(true, viewModel.IsPreview);

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var lineControl = control.Find(c => c.Tag == line).First() as ZCheckBox;

				AssertEquals("Some Caption", lineControl.Text);
			}
		}

		public void TestCustomFieldProperty()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };

			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var custom = template.GenCustomColumnDefinitions.AddNew();
			custom.XC_Name = "schmeckels";
			custom.XC_Type = AddOnColumnDataType.Codes.Integer;

			newFactory.Save();

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(newFactory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task = BMSTestHelper.CreateTask(workflow, "", description: "Task");
			var job = (OrgHeader)jobHeader.Parent;

			var customValue = newFactory.New<GenCustomAddOnValue>();
			customValue.XV_ParentID = jobHeader.FH_ParentId;
			customValue.XV_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			customValue.XV_Name = custom.XC_Name;
			customValue.XV_Type = custom.XC_Type;
			customValue.XV_Data = "99";

			newFactory.Save();

			var loadedTask = Factory.Load<ProcessTask>(task.PK);
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "<GetCustomField(schmeckels)>", PropertyTypeList.Codes.Number, "Some Caption", 1, 1, 100, 20, Color.White.Name, Color.Black.Name, 8, isBold: false, readOnly: true, autoSize: false);
			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(customisation.Factory);
			var viewModel = new ControlCustomisationViewModel(customisation, sectionViewModel, new TaskCardContent(loadedTask, sectionViewModel), new CellContent(0, 0, CellContentType.Cards));

			AssertEquals(false, viewModel.IsPreview);

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var calcEdit = control.Find(c => c.Tag == line).First() as ZCalcEdit;
				Application.DoEvents();

				AssertEquals("99", calcEdit.Text);
			}
		}

		public void TestCustomFieldPropertiesWithSimilarNames()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };

			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var custom1 = template.GenCustomColumnDefinitions.AddNew();
			custom1.XC_Name = "Triage 1";
			custom1.XC_Type = AddOnColumnDataType.Codes.String;

			var custom2 = template.GenCustomColumnDefinitions.AddNew();
			custom2.XC_Name = "Triage 1 Date";
			custom2.XC_Type = AddOnColumnDataType.Codes.Datetime;

			newFactory.Save();

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(newFactory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task = BMSTestHelper.CreateTask(workflow, "", description: "Task");
			var job = (OrgHeader)jobHeader.Parent;

			var customValue1 = newFactory.New<GenCustomAddOnValue>();
			customValue1.XV_ParentID = jobHeader.FH_ParentId;
			customValue1.XV_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			customValue1.XV_Name = custom1.XC_Name;
			customValue1.XV_Type = custom1.XC_Type;
			customValue1.XV_Data = "BlAh";

			var customValue2 = newFactory.New<GenCustomAddOnValue>();
			customValue2.XV_ParentID = jobHeader.FH_ParentId;
			customValue2.XV_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			customValue2.XV_Name = custom2.XC_Name;
			customValue2.XV_Type = custom2.XC_Type;
			customValue2.XV_Data = "24-FEB-2022 04:00:59";

			newFactory.Save();

			var loadedTask = Factory.Load<ProcessTask>(task.PK);
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var line1 = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "<GetCustomField(Triage 1)>", PropertyTypeList.Codes.Text, "Caption 1", 1, 1, 100, 20, Color.White.Name, Color.Black.Name, 8, isBold: false, readOnly: true, autoSize: false);
			var line2 = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "<GetCustomField(Triage 1 Date)>", PropertyTypeList.Codes.DateTime, "Caption 2", 1, 31, 100, 20, Color.White.Name, Color.Black.Name, 8, isBold: false, readOnly: true, autoSize: false);
			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(customisation.Factory);
			var viewModel = new ControlCustomisationViewModel(customisation, sectionViewModel, new TaskCardContent(loadedTask, sectionViewModel), new CellContent(0, 0, CellContentType.Cards));

			AssertEquals(false, viewModel.IsPreview);

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var edit1 = control.Find(c => c.Tag == line1).First() as ZTextBox;
				var edit2 = control.Find(c => c.Tag == line2).First() as ZDateEdit;
				Application.DoEvents();

				AssertEquals("BlAh", edit1.Text);
				AssertEquals("24-FEB-22 04:00", edit2.Text);
			}
		}

		public void TestRenderStatusControl_HorizontalOrientation_ShouldManageOwnSize()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var statusIndicatorLine = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.TaskStatusIndicator, string.Empty, 0, 0, 500, 500, "Black", "White", 8, false, false);

			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var viewModel = new ControlCustomisationViewModel(customisation, sectionViewModel, new TaskCardContent(task, sectionViewModel), new CellContent(0, 0, CellContentType.Cards));

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var statusControl = (StatusIndicatorControl)control.Find(c => c.Tag == statusIndicatorLine).First();
				ControlTestHelper.AssertControlDimensions(26, 12, statusControl);
			}
		}

		public void TestRenderStatusControl_VerticalOrientation_ShouldManageOwnSize()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var statusIndicatorLine = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.TaskStatusIndicator, string.Empty, 0, 0, 500, 500, "Black", "White", 8, false, false);
			statusIndicatorLine.Orientation = "Vertical";

			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var viewModel = new ControlCustomisationViewModel(customisation, sectionViewModel, new TaskCardContent(task, sectionViewModel), new CellContent(0, 0, CellContentType.Cards));

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var statusControl = (StatusIndicatorControl)control.Find(c => c.Tag == statusIndicatorLine).First();
				ControlTestHelper.AssertControlDimensions(12, 26, statusControl);
			}
		}

		public void TestRenderMultipartProperties()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Language = "333";

			var processHeader = ProcessJobHeader.GetForParent(orgHeader, Factory).ProcessHeaders.AddNew();
			processHeader.FH_CompletionStatement = "222";

			var processTask = processHeader.Parent.WorkflowItems.AddNew();
			processTask.P9_Status = "111";
			processTask.P9_FH_ProcessHeader = processHeader.PK;

			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation, new CellContent(0, 0, CellContentType.Cards));
			var lineProcessTask = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.ProcessTask, "<P9_Status>", PropertyTypeList.Codes.Text);
			var lineWorkflow = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, "<FH_CompletionStatement>", PropertyTypeList.Codes.Text);
			var lineJob = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, "<OH_Language>", PropertyTypeList.Codes.Text);

			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				control.SetDataBinding(processTask, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var lineWorkflowControl = control.Find(c => c.Tag == lineWorkflow).First();
				var lineProcessTaskControl = control.Find(c => c.Tag == lineProcessTask).First();
				var lineJobControl = control.Find(c => c.Tag == lineJob).First();
				Application.DoEvents();

				AssertEquals("111", lineProcessTaskControl.Text);
				AssertEquals("222", lineWorkflowControl.Text);
				AssertEquals("333", lineJobControl.Text);
			}
		}

		public void TestRender()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var line1 = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_CompletionStatement, PropertyTypeList.Codes.Text, "Desc.",
				10, 0, 0, 40, Color.Transparent.Name, Color.Ivory.Name, 8, true, true, true);
			var line2 = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_AgreedDeliveryDate, PropertyTypeList.Codes.DateTime, "Delivery Date",
				20, 50, 40, 20, Color.Khaki.Name, Color.Lavender.Name, 10, false, false, false);
			var line3 = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_IsCalendarItem, PropertyTypeList.Codes.Boolean, "Reminder",
				20, 100, 40, 50, Color.Khaki.Name, Color.Lavender.Name, 10, true, true, false);
			var line4 = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_Status, PropertyTypeList.Codes.Text, "Status",
				20, 150, 40, 20, Color.Khaki.Name, Color.Lavender.Name, 10, true, false, false);
			var line5 = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, OrgHeaderSchema.Constants.OH_IsActive, PropertyTypeList.Codes.Boolean, "Blah",
				20, 200, 40, 50, Color.Khaki.Name, Color.Lavender.Name, 10, true, true, false);
			var line6 = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_CompletionStatement, PropertyTypeList.Codes.Text, "Desc.",
				10, 250, 30, 20, Color.Indigo.Name, Color.Ivory.Name, 8, true, false, false);
			var line7 = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.ProcessTask, ProcessTasksSchema.Constants.P9_ActualDuration, PropertyTypeList.Codes.Duration, "Act Dur.",
				10, 300, 42, 20, Color.Indigo.Name, Color.Ivory.Name, 8, true, false, false);
			var line8 = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_DoNotStartBeforeDate, PropertyTypeList.Codes.Date, "Earliest Start",
				20, 350, 40, 20, Color.Khaki.Name, Color.Lavender.Name, 10, false, false, false);
			var line9 = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_FC_CurrentComponent, PropertyTypeList.Codes.Text, "Current Component",
				20, 400, 80, 20, Color.OrangeRed.Name, Color.PaleVioletRed.Name, 8, false, false, false);

			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation, new CellContent(0, 0, CellContentType.Cards));
			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				ControlTestHelper.AssertControlDimensions(300, 200, control);
				AssertColorEquals(Color.HotPink, control.BackColor);

				var line1Control = control.Find(c => c.Tag == line1).First();
				var line2Control = control.Find(c => c.Tag == line2).First();
				var line3Control = control.Find(c => c.Tag == line3).First();
				var line4Control = control.Find(c => c.Tag == line4).First();
				var line5Control = control.Find(c => c.Tag == line5).First();
				var line6Control = control.Find(c => c.Tag == line6).First();
				var line7Control = control.Find(c => c.Tag == line7).First();
				var line8Control = control.Find(c => c.Tag == line8).First();
				var line9Control = control.Find(c => c.Tag == line9).First();

				AssertControl(line1Control, typeof(DirectionalLabel), "ProcessHeader." + ProcessHeaderSchema.Constants.FH_CompletionStatement, "Desc.",
					10, 0, 40, 13, Color.Transparent, Color.Ivory, 8, true, true);

				AssertControl(line2Control, typeof(ZDateEdit), "ProcessHeader." + ProcessHeaderSchema.Constants.FH_AgreedDeliveryDate, "Delivery Date",
					20, 50, 40, 20, Color.Khaki, Color.Lavender, 10, false);
				AssertEquals(ZDateTimePickerFormat.Long, ((ZDateEdit)line2Control).DateTimeFormat);

				AssertControl(line3Control, typeof(ZCheckBox), ProcessTasksSchema.Constants.P9_IsCalendarItem, "Reminder",
					20, 100, 40, 50, Color.Khaki, Color.Lavender, 10, true);

				AssertControl(line4Control, typeof(ZDropEdit), ProcessTasksSchema.Constants.P9_Status, "Status",
					20, 150, 40, 20, Color.Khaki, Color.Lavender, 10, true);

				AssertControl(line5Control, typeof(ZCheckBox), "Parent." + OrgHeaderSchema.Constants.OH_IsActive, "Blah",
					20, 200, 40, 50, Color.Khaki, Color.Lavender, 10, true);

				AssertControl(line6Control, typeof(ZTextBox), "ProcessHeader." + ProcessHeaderSchema.Constants.FH_CompletionStatement, "Desc.",
					10, 250, 30, 20, Color.Indigo, Color.Ivory, 8, true);

				AssertControl(line7Control, typeof(ZTimeEditEx), ProcessTasksSchema.Constants.P9_ActualDuration, "Act Dur.",
					10, 300, 42, 20, Color.Indigo, Color.Ivory, 8, true);

				AssertControl(line8Control, typeof(ZDateEdit), "ProcessHeader." + ProcessHeaderSchema.Constants.FH_DoNotStartBeforeDate, "Earliest Start",
					20, 350, 40, 20, Color.Khaki, Color.Lavender, 10, false);
				AssertEquals(ZDateTimePickerFormat.Short, ((ZDateEdit)line8Control).DateTimeFormat);

				AssertControl(line9Control, typeof(CustomisedLayoutCodeFindBox), "ProcessHeader." + ProcessHeaderSchema.Constants.FH_FC_CurrentComponent, "Current Component",
					20, 400, 86, 20, Color.OrangeRed, Color.PaleVioletRed, 8, false);
			}
		}

		public void TestFieldAlignment()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Green");

			var lineLabelLeft = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_CompletionStatement, PropertyTypeList.Codes.Text, "Desc.",
				10, 0, 0, 100, Color.Transparent.Name, Color.Ivory.Name, 8, true, readOnly: true, true, ControlAlignmentList.Codes.Left);
			var lineLabelRight = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_CompletionStatement, PropertyTypeList.Codes.Text, "Desc.",
				10, 0, 0, 100, Color.Transparent.Name, Color.Ivory.Name, 8, true, readOnly: true, true, ControlAlignmentList.Codes.Right);
			var lineTextBoxLeft = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_CompletionStatement, PropertyTypeList.Codes.Text, "Desc.",
				10, 0, 0, 100, Color.Indigo.Name, Color.Ivory.Name, 8, true, readOnly: false, true, ControlAlignmentList.Codes.Left);
			var lineTextBoxRight = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_CompletionStatement, PropertyTypeList.Codes.Text, "Desc.",
				10, 0, 0, 100, Color.Indigo.Name, Color.Ivory.Name, 8, true, readOnly: false, true, ControlAlignmentList.Codes.Right);
			var lineCodeFindBoxLeft = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_FC_CurrentComponent, PropertyTypeList.Codes.Text, "Desc.",
				10, 0, 0, 100, Color.Indigo.Name, Color.Ivory.Name, 8, true, readOnly: false, true, ControlAlignmentList.Codes.Left);
			var lineCodeFindBoxRight = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Workflow, ProcessHeaderSchema.Constants.FH_FC_CurrentComponent, PropertyTypeList.Codes.Text, "Desc.",
				10, 0, 0, 100, Color.Indigo.Name, Color.Ivory.Name, 8, true, readOnly: false, true, ControlAlignmentList.Codes.Right);

			var viewModel = ControlCustomisationViewModelTest.GetViewModel(customisation, new CellContent(0, 0, CellContentType.Cards));
			using (var control = CustomisedControlRenderer.Render(viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(control);
				form.Show();

				var lineLabelLeftControl = control.Find(c => c.Tag == lineLabelLeft).First();
				var lineLabelRightControl = control.Find(c => c.Tag == lineLabelRight).First();
				var lineTextBoxLeftControl = control.Find(c => c.Tag == lineTextBoxLeft).First();
				var lineTextBoxRightControl = control.Find(c => c.Tag == lineTextBoxRight).First();
				var lineCodeFindBoxLeftControl = control.Find(c => c.Tag == lineCodeFindBoxLeft).First();
				var lineCodeFindBoxRightControl = control.Find(c => c.Tag == lineCodeFindBoxRight).First();

				AssertTextAlignment(lineLabelLeftControl, typeof(DirectionalLabel), alignedLeft: true);
				AssertTextAlignment(lineLabelRightControl, typeof(DirectionalLabel), alignedLeft: false);
				AssertTextAlignment(lineTextBoxLeftControl, typeof(ZTextBox), alignedLeft: true);
				AssertTextAlignment(lineTextBoxRightControl, typeof(ZTextBox), alignedLeft: false);
				AssertTextAlignment(lineTextBoxLeftControl, typeof(ZTextBox), alignedLeft: true);
				AssertTextAlignment(lineTextBoxRightControl, typeof(ZTextBox), alignedLeft: false);
				AssertTextAlignment(lineCodeFindBoxLeftControl, typeof(CustomisedLayoutCodeFindBox), alignedLeft: true);
				AssertTextAlignment(lineCodeFindBoxRightControl, typeof(CustomisedLayoutCodeFindBox), alignedLeft: false);
			}
		}

		public static void AssertControl(Control control, Type controlType, string bindingPath, string caption, int left, int top, int width, int height, Color? backColor, Color? foreColor, float fontSize, bool isBold, bool isAutoSize = false, AnchorStyles anchor = AnchorStyles.Left | AnchorStyles.Top)
		{
			AssertType(controlType, control);
			AssertEquals(bindingPath, ((ZUserControl)control.Parent).BindingSource.GetBindingMember(control));

			if (!string.IsNullOrEmpty(caption))
			{
				AssertNotNull("control should implement IResCaptionedControl", control as IResCaptionedControl);
				AssertEquals("Resource string caption", caption, ((IResCaptionedControl)control).CaptionResourceString.Caption);
			}

			// On these assertions we need to account for a range due to imprecisions when rounding at the moment of scaling to high DPI monitors
			var xVariation = ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
			var yVariation = ControlDpiScalingHelper.ScaleToCurrentDpiY(5);
			Assert("control.Left", ControlDpiScalingHelper.ScaleToCurrentDpiX(left) - xVariation <= control.Left && control.Left <= ControlDpiScalingHelper.ScaleToCurrentDpiX(left) + xVariation);
			Assert("control.Top", ControlDpiScalingHelper.ScaleToCurrentDpiY(top) - yVariation <= control.Top && control.Top <= ControlDpiScalingHelper.ScaleToCurrentDpiY(top) + yVariation);

			if (!control.AutoSize)
			{
				Assert("control.Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(width) - xVariation <= control.Width && control.Width <= ControlDpiScalingHelper.ScaleToCurrentDpiX(width) + xVariation);
				Assert("control.Height", ControlDpiScalingHelper.ScaleToCurrentDpiY(height) - yVariation <= control.Height && control.Height <= ControlDpiScalingHelper.ScaleToCurrentDpiY(height) + yVariation);
			}

			if (backColor != null)
			{
				AssertColorEquals("control.BackColor", backColor.Value, control.BackColor);
			}
			if (foreColor != null)
			{
				AssertColorEquals("control.ForeColor", foreColor.Value, control.ForeColor);
			}
			AssertEquals("control.Font.Size", fontSize, control.Font.Size);
			AssertEquals("control.Font.Bold", isBold, control.Font.Bold);

			if (isAutoSize)
			{
				AssertEquals(true, ((Label)control).AutoSize);
			}

			AssertEquals(anchor, control.Anchor);
		}

		public static void AssertTextAlignment(Control control, Type controlType, bool alignedLeft)
		{
			AssertType(controlType, control);

			switch (control)
			{
				case Label label:
					AssertEquals(alignedLeft ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleRight, label.TextAlign);
					break;
				case ZTextBox textBox:
					AssertEquals(alignedLeft ? HorizontalAlignment.Left : HorizontalAlignment.Right, textBox.TextAlign);
					break;
				case CustomisedLayoutCodeFindBox codeFindBox:
					AssertEquals(alignedLeft ? HorizontalAlignment.Left : HorizontalAlignment.Right, codeFindBox.CodeBox.TextAlign);
					break;
			}
		}

		#endregion
	}
}
