using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	[TestedType(typeof(ShapeEntityDetailsForm))]
	class ShapeEntityDetailsFormTest : ZFormBasherTest
	{
		public void TestEditShapeProcessHeaderProperties_ShouldSetHasChanges()
		{
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader2.ProcessHeaders.AddNew();

			var diagram = NetworkTestCase.CreateDiagram(jobHeader1);
			var subDiagram = NetworkTestCase.CreateShape(jobHeader2, diagram);
			var workflowShape = NetworkTestCase.CreateShape(workflow, subDiagram);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);

			var viewModel = new BMNetworkViewModel(loadedDiagram);
			var network = NetworkTestCase.CreateNetwork(loadedDiagram, viewModel);
			workflowShape = network.Shapes.Single(e => e.PK == workflowShape.PK);

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
			{
				var form = (ShapeEntityDetailsForm)dialog;

				AssertEquals(false, loadedDiagram.HasChanges);
				AssertEquals(false, form.BusinessEntity.ProcessHeader.HasChanges);

				form.BusinessEntity.ProcessHeader.FH_CompletionStatement = "You've got changes";

				AssertEquals(true, loadedDiagram.HasChanges);
				AssertEquals(true, form.BusinessEntity.ProcessHeader.HasChanges);
			});
			try
			{
				network.EditEntity(workflowShape);
				newFactory.Save();

				AssertEquals("You've got changes", workflow.FH_CompletionStatement);
			}
			finally
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			}
		}

		public void TestEditDiagramFormText()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader1 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader2, "Workflow");

			var diagram = NetworkTestCase.CreateDiagram(jobHeader1);
			var subDiagram = NetworkTestCase.CreateShape(jobHeader2, diagram);
			var workflowShape = NetworkTestCase.CreateShape(workflow, subDiagram);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);

			var viewModel = new BMNetworkViewModel(loadedDiagram);
			var network = NetworkTestCase.CreateNetwork(loadedDiagram, viewModel);
			workflowShape = network.Shapes.Single(e => e.PK == workflowShape.PK);

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
			{
				var form = (ShapeEntityDetailsForm)dialog;

				form.Shown += (s, e) => AssertStartsWith("The form title should begin with the word Edit.", "Edit New Diagram", form.Text);
			});

			try
			{
				network.EditEntity(workflowShape);
			}
			finally
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			}
		}

		public void TestRelationshipNavigatorButton_WouldYouLikeToSaveNow_ShouldNotThrowException()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "WKI");
			var job = Factory.New<IWorkItem>();
			var bizo = (BusinessObject)job;
			var workflowProvider = (IWorkflowProvider)job;
			bizo.FillWithValidTestData();
			var jobHeader = (ProcessJobHeader)BMSTestHelper.GetJobHeaderForParent(workflowProvider, Factory, addDefaultProcessHeaderIfNone: false);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader);

			Factory.Save();

			var viewModel = new BMNetworkViewModel(diagram);
			var network = NetworkTestCase.CreateNetwork(diagram, viewModel);
			var entity = network.Entities.GetInstance(diagram);

			job.WKI_Summary = "Shalala"; // Change a detail on the job so that the form requires a save when opening the Relationshavigator

			using (var form = new ShapeEntityDetailsForm(entity))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTabControl>(x => x.Name == "ShapeTabControl");
				tabControl.SelectTab("WorkflowDetailsTabPage");
				Application.DoEvents();

				var button = form.FindSingle<ZButton>(x => x.Name == "WorkflowRelationshipsButton");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals(true, bizo.HasChanges);

				AssertNoExceptionThrown(() =>
				{
					button.PerformClick();
				});

				AssertEquals("You must save this form before trying to edit this workflow's relationships. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The Factory.Save should have completed, and yet...", false, bizo.HasChanges);
			}
		}

		public void TestChannelsAndLevelingRulesTabs_ForScaledRootDiagram_ShouldBeVisible()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var subDiagram = NetworkTestCase.CreateShape(diagram);
			var shape = NetworkTestCase.CreateShape(diagram);

			Factory.Save();

			var viewModel = new BMNetworkViewModel(diagram);
			var network = NetworkTestCase.CreateNetwork(diagram, viewModel);
			var entity = network.Entities.GetInstance(diagram);

			AssertHasChannelsAndLevelingRulesTabs("The Channels tab should be shown for root diagrams. SAD!", entity, true);
		}

		public void TestChannelsAndLevelingRulesTabs_ForNonRootDiagramShape_ShouldNotBeVisible()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var subDiagram = NetworkTestCase.CreateShape(diagram);
			var shape = NetworkTestCase.CreateShape(diagram);

			Factory.Save();

			var viewModel = new BMNetworkViewModel(diagram);
			var network = NetworkTestCase.CreateNetwork(diagram, viewModel);
			var entity = network.Entities.GetInstance(subDiagram);

			AssertHasChannelsAndLevelingRulesTabs("The Channels tab should not be shown for sub diagrams. SAD!", entity, false);

			entity = network.Entities.GetInstance(shape);
			AssertHasChannelsAndLevelingRulesTabs("The Channels tab should not be shown for non-diagram shapes. SAD!", entity, false);
		}

#if !WINZOR
		public void TestChannelsAndLevelingRulesTabs_ForNonRootDiagramShape_WhenOpenedAsDiagram_ShouldNotBeVisible()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var shape = NetworkTestCase.CreateShape(diagram);

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				Application.DoEvents();

				using (var shapeForm = NetworkGUITestCase.FindAndClickOpenAsDiagramMenuItem(form, shape))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormClosing(dialog =>
					{
						var editForm = (ShapeEntityDetailsForm)dialog;
						AssertHasChannelsAndLevelingRulesTabs("The Channels tab should not be shown for a non-diagram shape, even if it's being shown as its own diagram. SAD!", editForm, false);
					});

					var control = NetworkGUITestCase.FindNetworkUserControl(shapeForm);

					control.ViewModel.MenuItems.Single(i => i?.Name == "Edit Properties").Action.Execute();
					NetworkGUITestCase.DoEventsThoroughly();
				}
			}
		}
#endif

		public void TestChannelsTab_ForNonScaledRootDiagram_ShouldNotBeVisible()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: false);
			var shape = NetworkTestCase.CreateShape(diagram);

			Factory.Save();

			var viewModel = new BMNetworkViewModel(diagram);
			var network = NetworkTestCase.CreateNetwork(diagram, viewModel);
			var entity = network.Entities.GetInstance(diagram);

			AssertHasChannelsAndLevelingRulesTabs("The Channels tab should not be shown for nonscaled diagrams. SAD!", entity, false);
		}

		static void AssertHasChannelsAndLevelingRulesTabs(string message, ShapeNetworkEntity entity, bool shouldIncludeTabs)
		{
			using (var form = new ShapeEntityDetailsForm(entity))
			{
				form.Show();
				Application.DoEvents();

				AssertHasChannelsAndLevelingRulesTabs(message, form, shouldIncludeTabs);
			}
		}

		static void AssertHasChannelsAndLevelingRulesTabs(string message, ShapeEntityDetailsForm form, bool shouldIncludeTabs)
		{
			var tabControl = form.FindSingle<ZTabControl>("ShapeTabControl");
			var tabs = tabControl.TabPages.Cast<ZTabPage>().Select(x => x.Text).ToArray();

			if (shouldIncludeTabs)
			{
				AssertCollectionContains(message, "Channels", tabs);
				AssertCollectionContains(message, "Leveling Rules", tabs);
			}
			else
			{
				AssertCollectionNotContains(message, "Channels", tabs);
				AssertCollectionNotContains(message, "Leveling Rules", tabs);
			}
		}

#if !WINZOR
		[TestDate(2018, 12, 12)]
		public void TestAdjustEntityLocationToChannelBounds_ResizeChannel_EntitiesShouldSnapWhenFormIsClosed()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagramShape);
			var network = viewModel.GetJobNetwork();

			var channel1 = NetworkTestCase.CreateChannel(diagramShape, "Bend", 1, "Blanched Almond", 500);
			var channel2 = NetworkTestCase.CreateChannel(diagramShape, "Snap", 2, "Olive Drab", 500);

			var childShape = viewModel.CreateNewShape(diagramShape);
			childShape.X = 10;
			childShape.Y = 300;
			childShape.Width = 300;
			childShape.Height = 150;

			Factory.Save();

			NodeViewModel childShapeNode = null;

			using (var form = new NetworkDiagramForm(diagramShape))
			{
				form.Show();
				Application.DoEvents();

				var networkUserControl = form.NetworkDiagramControl.NetworkUserControl;
				var networkView = (NetworkView)networkUserControl.MainDiagramControl.FindName("NetworkControl");
				childShapeNode = (NodeViewModel)networkView.Nodes.SingleOrDefault();
				AssertEquals("Shape should be in the Bend channel. HAPPY!", "Bend", childShapeNode.AppliedAttributesReadableText);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var editForm = (ShapeEntityDetailsForm)dialog;

					if (editForm != null)
					{
						editForm.Shown += (s, e) =>
						{
							var tabControl = editForm.FindSingle<ZTabControl>("ShapeTabControl");
							var tabPage = tabControl.TabPages.Cast<ZTabPage>().Single(x => x.Name == "ChannelsTabPage");
							tabControl.SelectTab(tabPage);
							Application.DoEvents();

							var grid = tabPage.FindSingle<ZGrid>("ChannelsGrid");

							AssertEquals(1, grid[0, 0]);
							AssertEquals("Bend", grid[0, 1]);
							AssertEquals(500, grid[0, 2]);

							AssertEquals(2, grid[1, 0]);
							AssertEquals("Snap", grid[1, 1]);
							AssertEquals(500, grid[1, 2]);

							grid[0, 2] = new ZInt(350);

							Application.DoEvents();

							var okButton = editForm.FindSingle<ZButton>("OKButton");
							okButton.PerformClick();
							Application.DoEvents();

							AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						};
					}
				});

				var control = NetworkGUITestCase.FindNetworkUserControl(form);
				control.ViewModel.MenuItems.Single(i => i?.Name == "Edit Properties").Action.Execute();
				NetworkGUITestCase.DoEventsThoroughly();

				childShapeNode = (NodeViewModel)networkView.Nodes.SingleOrDefault();
			}

			AssertEquals("The first channel should have shrunk. HAPPY!", 350, channel1.BNL_Height);
			AssertEquals("The second channel should be the same size. HAPPY!", 500, channel2.BNL_Height);
			AssertEquals(350d, childShape.Y);
			AssertEquals(150d, childShape.Height);
			AssertEquals("Shape should now be in the Snap channel. Awwww SNAP!", "Snap", childShapeNode.AppliedAttributesReadableText);
		}
#endif
		public void TestChannelsTab_Grid_ShouldBeBoundToDiagramChannelCollection()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channel1 = diagram.Channels.AddNew();
			var channel2 = diagram.Channels.AddNew();

			channel1.BNL_Name = "Shmlonathan";
			channel2.BNL_Name = "Shmlangela";
			channel1.BNL_Sequence = 1;
			channel2.BNL_Sequence = 2;
			channel1.Color = "Blanched Almond";
			channel2.Color = "Olive Drab";
			channel1.BNL_Height = 69;
			channel2.BNL_Height = 420;

			Factory.Save();

			var viewModel = new BMNetworkViewModel(diagram);
			var network = NetworkTestCase.CreateNetwork(diagram, viewModel);
			var entity = network.Entities.GetInstance(diagram);

			using (var form = new ShapeEntityDetailsForm(entity))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTabControl>("ShapeTabControl");
				var tabPage = tabControl.TabPages.Cast<ZTabPage>().Single(x => x.Name == "ChannelsTabPage");
				tabControl.SelectTab(tabPage);
				Application.DoEvents();

				var grid = tabPage.FindSingle<ZGrid>("ChannelsGrid");

				AssertEquals(1, grid[0, 0]);
				AssertEquals("Shmlonathan", grid[0, 1]);
				AssertEquals(69, grid[0, 2]);

				AssertEquals(2, grid[1, 0]);
				AssertEquals("Shmlangela", grid[1, 1]);
				AssertEquals(420, grid[1, 2]);
			}
		}

		public void TestChannelsTab_Grid_ShouldNotBeReadOnly()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			Factory.Save();

			var viewModel = new BMNetworkViewModel(diagram);
			var network = NetworkTestCase.CreateNetwork(diagram, viewModel);
			var entity = network.Entities.GetInstance(diagram);

			using (var form = new ShapeEntityDetailsForm(entity))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindSingle<ZTabControl>("ShapeTabControl");
				var tabPage = tabControl.TabPages.Cast<ZTabPage>().Single(x => x.Name == "ChannelsTabPage");
				tabControl.SelectTab(tabPage);
				Application.DoEvents();

				var grid = tabPage.FindSingle<ZGrid>("ChannelsGrid");
				AssertEquals(false, grid.ReadOnly);
			}
		}

		public void TestClickOk_WithValidationErrors_ShouldPromptUser_AndNotCloseForm()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			Factory.Save();

			using (var form = new ShapeEntityDetailsForm(network.DiagramEntity))
			{
				form.Show();
				Application.DoEvents();

				diagram.BNS_JobType = "SQU";
				AssertHasError(diagram.BNS_JobTypeInfo, "Enter a valid Job Type.");

				var okButton = form.FindSingle<ZButton>("OKButton");
				okButton.PerformClick();
				Application.DoEvents();

				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should not close because there are validation errors. SAD!", true, form.Visible);

				UnitTestUserNotification.Instance.ClearMessages();
				form.Close();
				Application.DoEvents();

				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should not close because there are validation errors. SAD!", true, form.Visible);

				UnitTestUserNotification.Instance.ClearMessages();
				diagram.BNS_JobType = "INQ";
				AssertNoErrors(diagram.BNS_JobTypeInfo);

				form.Close();
				okButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should close because there are no validation errors. SAD!", false, form.Visible);
			}
		}

		public void TestClickOk_WithSchedulingConflict_ShouldCloseFormWithoutNotification()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Maff");
			var diagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true, name: "Hath");
			var shape1 = NetworkTestCase.CreateShape(diagram, name: "Shape on Diagram 1");

			var network = NetworkTestCase.CreateNetwork(diagram);
			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var action = new CreateProjectBufferAction(viewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);

			network.LinkEntity(shape1, diagram2);
			NetworkTestCase.SetShapeSize(network, shape1, diagram, 400, 200);
			UnitTestUserNotification.Instance.ClearMessages();
			using (var form = new ShapeEntityDetailsForm(shape1.AsEntity(network)))
			{
				form.Show();
				Application.DoEvents();

				var okButton = form.FindSingle<ZButton>("OKButton");
				okButton.PerformClick();
				Application.DoEvents();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestClickOk_WithUnapprovedBufferError_ShouldCloseFormWithoutNotification()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var network = NetworkTestCase.CreateNetwork(diagram);
			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var action = new CreateProjectBufferAction(viewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);

			var shape = diagram.ChildShapes.SingleOrDefault();
			AssertNotNull("A project buffer shape should have been added", shape);
			AssertEquals(ShapeTypeList.Codes.Buffer, shape.BNS_ShapeType);

			AssertHasError(shape.ActiveInfo, "This buffer has not been accepted yet. Please accept the buffer or hide it from the diagram.");

			using (var form = new ShapeEntityDetailsForm(shape.AsEntity(network)))
			{
				form.Show();
				Application.DoEvents();

				var okButton = form.FindSingle<ZButton>("OKButton");
				okButton.PerformClick();
				Application.DoEvents();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should close because there are no *relevant* validation errors. SAD!", false, form.Visible);
			}
		}

		public void TestValidation_ShouldRunOnFormClose()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var channel = NetworkTestCase.CreateChannel(diagram, "Jeremy Bearimy", height: 69);
			AssertHasError(channel.BNL_HeightInfo, "Please enter a 'Height' greater than or equal to 100.");

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedDiagram = newFactory.Load<BMNCNRootDiagramShape>(diagram.PK);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(loadedDiagram);
			var network = networkViewModel.GetJobNetwork();

			using (var form = new ShapeEntityDetailsForm(network.DiagramEntity))
			{
				form.Show();
				Application.DoEvents();

				var loadedChannel = loadedDiagram.Channels.Single();
				AssertNoErrors("No validation has run this time so there shouldn't be any errors... yet.", loadedChannel.BNL_HeightInfo);

				form.Close();
				Application.DoEvents();

				AssertHasError("Attempting to close the form should have run validation on the underlying business object. SAD!", loadedChannel.BNL_HeightInfo, "Please enter a 'Height' greater than or equal to 100.");
				AssertEquals("Attempting to close the form should have actually validated the bizo and then prompted the user. SAD!", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should not close because there are validation errors. SAD!", true, form.Visible);

				UnitTestUserNotification.Instance.ClearMessages();
				loadedChannel.BNL_Height = 420;
				AssertNoErrors(loadedChannel.BNL_HeightInfo);

				form.Close();
				Application.DoEvents();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should close because there are no validation errors. SAD!", false, form.Visible);
			}
		}

		public void TestValidation_ShouldRunOnFormClose_ShouldNotValidateChildShapes()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "diagram!");
			var shape = NetworkTestCase.CreateShape(diagram, name: "shape!");
			var channel = NetworkTestCase.CreateChannel(diagram, "Fox", height: 70);
			AssertHasError(channel.BNL_HeightInfo, "Please enter a 'Height' greater than or equal to 100.");

			shape.Name = ""; // introduce an error into the shape

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedDiagram = newFactory.Load<BMNCNRootDiagramShape>(diagram.PK);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(loadedDiagram);
			var network = networkViewModel.GetJobNetwork();

			using (var form = new ShapeEntityDetailsForm(network.DiagramEntity))
			{
				var children = network.DiagramEntity.Children;

				AssertEquals("Our diagram has a child shape", 1, children.Count());

				AssertNoErrors("No validation has run this time so there shouldn't be any errors... yet.", children.First().Shape.BNS_NameInfo);

				form.Close();
				Application.DoEvents();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should close because there are no validation errors.", false, form.Visible);
			}

			shape.Validation.ValidateBNS_Name();
			AssertHasError("Shape actually does have an error that we can now see upon validating it!", shape.BNS_NameInfo, "Please enter a Name.");
		}

		public void TestValidation_ShouldRunOnFormClose_ShouldValidateRelevantChildObjects_ForShape()
		{
			const string RowError = "OwO We have a Rowo erroro";

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");

			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var shape = NetworkTestCase.CreateShape(jobHeader, diagram);
			shape.AddRowError(nameof(shape) + RowError);

			var schedule = shape.GetOrCreateSchedule();
			schedule.AddRowError(nameof(schedule) + RowError);

			var processHeader = shape.LinkedEntity;
			processHeader.AddRowError(nameof(processHeader) + RowError);

			Factory.Save();

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(shape);
			var network = networkViewModel.GetJobNetwork();

			using (var form = new ShapeEntityDetailsForm(network.DiagramEntity))
			{
				form.Show();
				Application.DoEvents();

				form.Close();
				Application.DoEvents();

				AssertEquals("Attempting to close the form should have actually validated the bizo and then prompted the user!", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should not close because there are row errors!", true, form.Visible);

				UnitTestUserNotification.Instance.ClearMessages();

				shape.RemoveRowError(nameof(shape) + RowError);
				schedule.RemoveRowError(nameof(schedule) + RowError);
				processHeader.RemoveRowError(nameof(processHeader) + RowError);

				form.Close();
				Application.DoEvents();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should close because there are no validation errors.", false, form.Visible);
			}
		}

		public void TestValidation_ShouldRunOnFormClose_ShouldValidateRelevantChildObjects_ForDiagram()
		{
			const string RowError = "OwO We have a Rowo erroro";

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var diagram = NetworkTestCase.CreateDiagram(jobHeader, name: "", isScaled: true);

			diagram.AddRowError(nameof(diagram) + RowError);

			var schedule = diagram.GetOrCreateSchedule();
			schedule.AddRowError(nameof(schedule) + RowError);

			var processHeader = diagram.LinkedEntity;
			processHeader.AddRowError(nameof(processHeader) + RowError);

			var levellingRule = NetworkTestCase.CreateLevelingRule(diagram, name: "38°");
			levellingRule.AddRowError(nameof(levellingRule) + RowError);

			Factory.Save();

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			using (var form = new ShapeEntityDetailsForm(network.DiagramEntity))
			{
				form.Show();
				Application.DoEvents();

				form.Close();
				Application.DoEvents();

				AssertEquals("Attempting to close the form should have actually validated the bizo and then prompted the user!", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should not close because there are row errors!", true, form.Visible);

				UnitTestUserNotification.Instance.ClearMessages();

				diagram.RemoveRowError(nameof(diagram) + RowError);
				schedule.RemoveRowError(nameof(schedule) + RowError);
				processHeader.RemoveRowError(nameof(processHeader) + RowError);
				levellingRule.RemoveRowError(nameof(levellingRule) + RowError);

				diagram.Name = "Name";

				form.Close();
				Application.DoEvents();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should close because there are no validation errors.", false, form.Visible);
			}
		}

		public void TestValidation_ShouldRunOnFormClose_ShouldValidateRelevantChildObjects_ShouldIgnoreExistingUnsavedErrors()
		{
			const string RowError = "OwO We have a Rowo erroro";

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var diagram = NetworkTestCase.CreateDiagram(jobHeader, name: "", isScaled: true);

			var schedule = diagram.GetOrCreateSchedule();
			var processHeader = diagram.LinkedEntity;
			var levellingRule = NetworkTestCase.CreateLevelingRule(diagram, name: "38°");

			Factory.Save();

			diagram.AddRowError(nameof(diagram) + RowError);
			schedule.AddRowError(nameof(schedule) + RowError);
			processHeader.AddRowError(nameof(processHeader) + RowError);
			levellingRule.AddRowError(nameof(levellingRule) + RowError);

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			using (var form = new ShapeEntityDetailsForm(network.DiagramEntity))
			{
				form.Show();
				Application.DoEvents();

				form.Close();
				Application.DoEvents();

				AssertEquals("Attempting to close the form should have actually validated the bizo and then prompted the user!", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should not close because there are row errors!", true, form.Visible);

				UnitTestUserNotification.Instance.ClearMessages();

				diagram.RemoveRowError(nameof(diagram) + RowError);
				schedule.RemoveRowError(nameof(schedule) + RowError);
				processHeader.RemoveRowError(nameof(processHeader) + RowError);
				levellingRule.RemoveRowError(nameof(levellingRule) + RowError);

				diagram.Name = "Name";

				form.Close();
				Application.DoEvents();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should close because there are no validation errors.", false, form.Visible);
			}
		}

		public void TestDetailsForm_ShouldHaveNameField_ShouldShowValidationErrorWhenEmpty()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagram.Name = "Hi";
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			Factory.Save();

			using (var form = new ShapeEntityDetailsForm(network.DiagramEntity))
			{
				form.Show();
				Application.DoEvents();

				var nameBindingMemberName = nameof(ShapeNetworkEntity.Shape) + "." + nameof(BMNCNShape.BNS_Name);
				var shapeNameControl = form.FindSingle<ZTextBox>(control => control.BindTo == nameBindingMemberName);

				AssertNotNull("We should have a control that binds to the Shape's Name field", shapeNameControl);

				diagram.Name = "";
				AssertEquals(string.Empty, shapeNameControl.Text);

				var okButton = form.FindSingle<ZButton>("OKButton");
				okButton.PerformClick();
				Application.DoEvents();

				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should not close because there are validation errors", true, form.Visible);

				UnitTestUserNotification.Instance.ClearMessages();
				diagram.Name = "Henlo";
				AssertEquals("Henlo", shapeNameControl.Text);

				okButton.PerformClick();
				Application.DoEvents();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The form should close because there are no validation errors.", false, form.Visible);
			}

			AssertEquals("We can change a shape's name through its Name field", "Henlo", diagram.Name);
		}

		public void TestDetailsFormShapeNameField_ShouldUseNameMaxLength()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			Factory.Save();

			using (var form = new ShapeEntityDetailsForm(network.DiagramEntity))
			{
				form.Show();
				Application.DoEvents();

				var nameBindingMemberName = nameof(ShapeNetworkEntity.Shape) + "." + nameof(BMNCNShape.BNS_Name);
				var shapeNameControl = form.FindSingle<ZTextBox>(control => control.BindTo == nameBindingMemberName);

				AssertEquals(AutoBMNCNShape.Schema.BNS_NameMaxLength, shapeNameControl.MaxLength);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		protected override Form GetFormToBashCore()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var network = NetworkTestCase.CreateNetwork(diagram);

			Factory.Save();

			return new ShapeEntityDetailsForm(network.DiagramEntity);
		}

		#endregion
	}
}
