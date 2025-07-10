using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class NetworkDiagramFormIntegrationTest : NetworkGUITestCase
	{
		public void TestCopyDiagram()
		{
			var diagramToImportShape = CreateDiagram(Factory, name: "Methods");
			var networkToImport = CreateNetwork(diagramToImportShape);
			var diagramToImport = networkToImport.DiagramEntity;
			var shape1 = CreateShape(diagramToImport, name: "Sock");
			var shape2 = CreateShape(diagramToImport, name: "Truck");
			var shape3 = CreateShape(diagramToImport, name: "Shower");

			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(shape3);

			shape1.SetCoordinates(width: 200d, height: 200d, x: 100d, y: 100d);
			shape2.SetCoordinates(width: 200d, height: 200d, x: 400d, y: 100d);
			shape3.SetCoordinates(width: 200d, height: 200d, x: 700d, y: 100d);

			var superDiagram = CreateDiagram(Factory, name: "Method Container");
			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(superDiagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				networkDiagram.NetworkUserControlHosted.WaitOne(TimeSpan.FromSeconds(1));
				var networkControlViewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;

				using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(diagramToImport.Shape))
				{
					networkDiagram.MainDiagramControl.ViewModel.ImportEntity(new System.Windows.Point(300, 300), networkControlViewModel.DiagramEntity);
				}

				Application.DoEvents();

				var importRoot = networkDiagram.FindNodeItem("Methods");
				var importShape1 = networkDiagram.FindNodeItem("Sock");
				var importShape2 = networkDiagram.FindNodeItem("Truck");
				var importShape3 = networkDiagram.FindNodeItem("Shower");

				AssertAttachmentExsists(Business.AttachmentTypeList.Codes.Dependency, importRoot, importShape1, importShape2);
				AssertAttachmentExsists(Business.AttachmentTypeList.Codes.Dependency, importRoot, importShape2, importShape3);

				AssertArrowCount(2, networkDiagram);

				AssertCoordinates(importRoot, x: 300, y: 300, width: 900, height: 300);
				AssertCoordinates(importShape1, x: 400, y: 395, width: 200, height: 200);
				AssertCoordinates(importShape2, x: 700, y: 395, width: 200, height: 200);
				AssertCoordinates(importShape3, x: 1000, y: 395, width: 200, height: 200);
			}
		}

		public void TestValidationHappensAfterSnapNotBefore()
		{
			var diagramShape = CreateDiagram(Factory, name: "Root");
			var network = CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;
			var shape1 = CreateShape(diagram, name: "Trunk1");
			var shape2 = CreateShape(diagram, name: "Trunk2");
			var arrow = shape1.MakeVisiblePrerequisiteOf(shape2).AsEntity(network);

			CreateNetwork(diagram.Shape).ScaleAndRefresh();

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				networkDiagram.NetworkUserControlHosted.WaitOne(TimeSpan.FromSeconds(1));
				var networkControlViewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;
				network = (JobNetwork)networkControlViewModel.NetworkModel;

				var node1 = networkDiagram.FindShapeControl("Trunk1");
				var node2 = networkDiagram.FindShapeControl("Trunk2");
				var node1View = node1.GetViewModel();
				var node2View = node2.GetViewModel();

				arrow = ((BusinessObject)form.DataSource).Factory.Load<BMNCNAttachment>(arrow.PK).AsEntity(network);
				var countNotificationChanges = 0;
				arrow.NotificationsChanged += (s, e) => countNotificationChanges++;

				node1View.Width = 300;
				node1View.X = 0;
				node2View.Width = 300;
				node2View.X = 300;
				Application.DoEvents();

				AssertEquals(0d, node1View.X);
				AssertEquals(300d, node1View.Width);
				AssertEquals(300d, node2View.X);
				AssertEquals(300d, node2View.Width);

				AssertNoRowWarnings(arrow);

				node1View.Width = 400;
				Application.DoEvents();

				AssertHasRowWarning(arrow, "The post-requisite is scheduled to begin before its pre-requisite is scheduled to complete.");
				AssertEquals(1, countNotificationChanges);

				node1View.Width = 315;
				node1View.AdjustSizeToScale();
				Application.DoEvents();

				AssertEquals("the shape snaps to the grid see?", 300d, node1View.Width);
				AssertNoRowWarnings(arrow);
				AssertEquals(2, countNotificationChanges);
			}
		}

		public void TestPinMenuItemIsNotShownForApprovedDiagram()
		{
			var diagram = CreateDiagram(Factory, name: "Rose");
			var shape = CreateShape(parentShape: diagram, name: "Verse");
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();
			diagram.ScheduledStartTimeLocal = ZDateTime.Today;
			networkViewModel.ToggleApproval();

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				networkDiagram.NetworkUserControlHosted.WaitOne(TimeSpan.FromSeconds(1));
				var formNetworkViewModel = networkDiagram.ViewModel.NetworkViewModel;
				var formNetwork = formNetworkViewModel.GetJobNetwork();

				var node = networkDiagram.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == "Verse");
				var shapeViewModel = node.DataContext as NodeViewModel;
				shape = shapeViewModel.Entity.AsShape();

				using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(formNetworkViewModel, shape))
				{
					var menuItemPin = formNetworkViewModel.GetSecondLevelNetworkActionsMenuItem_ForTesting(shapeViewModel.Entity, "Actions", "Pin Shape");
					AssertNull(menuItemPin);

					shape.PinShape(networkViewModel);
					menuItemPin = formNetworkViewModel.GetSecondLevelNetworkActionsMenuItem_ForTesting(shapeViewModel.Entity, "Actions", "Unpin Shape");
					AssertNull(menuItemPin);

					formNetworkViewModel.ToggleApproval();
					menuItemPin = formNetworkViewModel.GetSecondLevelNetworkActionsMenuItem_ForTesting(shapeViewModel.Entity, "Actions", "Unpin Shape");
					AssertNotNull(menuItemPin);
				}
			}
		}

		public void TestLinkManyWorkflows_ScrollingUpdates()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var diagram = CreateDiagram(Factory, name: "Root");
			var network = NetworkTestCase.CreateNetwork(diagram);

			var headers = new List<ProcessHeader>();

			for (var i = 0; i < 30; i++)
			{
				headers.Add(CreateWorkflow(jobHeader, "workflow" + i));
			}

			Factory.Save();
			var allHeaders = headers.Cast<BusinessObject>().ToArray();

			CopyFilterGridHyperlinkToClipboardMenuItemTest.CopyBizosToClipboard(ControllerIDs.ProcessHeader, allHeaders);

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				networkDiagram.NetworkUserControlHosted.WaitOne(TimeSpan.FromSeconds(1));
				var networkControlViewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;

				var scrollViewer = networkDiagram.MainDiagramControl.FindChildren<ScrollViewer>().Single(s => s.Name == "ScrollViewer");

				var initialScrollableHeight = scrollViewer.ScrollableHeight;

				var parentMenuItem = networkControlViewModel.MenuItems.WhereNotNull().Single(n => n.Name == "Create New...");
				var createFromClipboard = parentMenuItem.Items.WhereNotNull().Single(n => n.Name.StartsWith("Shapes for 30"));
				networkDiagram.MainDiagramControl.ExecuteContextMenuAction(createFromClipboard.Action);

				Application.DoEvents();

				AssertEquals("All of the nodes were added", 30, networkControlViewModel.NetworkModel.Entities.Count);

				AssertNotEquals(initialScrollableHeight, scrollViewer.ScrollableHeight);
			}
		}

		public void TestLinkJobHeaderFromClipboard_PasteActionAndTooltip_ClipboardEmpty()
		{
			var jobHeaderToLinkToRoot = CreateJobHeader<OrgHeader>();
			var jobToLinkToRoot = (OrgHeader)jobHeaderToLinkToRoot.Parent;

			var diagram = CreateDiagram(Factory, name: "Root");
			var subDiagram = CreateShape(parentShape: diagram, name: "Sub");

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				networkDiagram.NetworkUserControlHosted.WaitOne(TimeSpan.FromSeconds(1));
				var networkControlViewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;

				var formNetworkViewModel = networkDiagram.ViewModel.NetworkViewModel;

				formNetworkViewModel.SelectSingleEntity(null);
				var strategy = new TaskTrackingAsyncBaseStrategy();
				var pasteHandled = formNetworkViewModel.TryHandlePaste();

				AssertEquals(false, pasteHandled);
			}
		}

		public void TestLinkWorkflowFromClipboard()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "I saw a ping Pong");

			var diagram = CreateDiagram(Factory, name: "Root");
			var subDiagram = CreateShape(parentShape: diagram, name: "Sub");

			Factory.Save();

			ZMenuStrategyHelper.ShortcutCreator.CopyHyperlinkToClipboard("BingerBangBungoBoosh", ShowEditFormUrlHandler.Instance.Create(ControllerIDs.ProcessHeader, workflow.PK));

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				networkDiagram.NetworkUserControlHosted.WaitOne(TimeSpan.FromSeconds(1));
				var formNetworkViewModel = networkDiagram.ViewModel.NetworkViewModel;
				var formNetwork = formNetworkViewModel.GetJobNetwork();

				var node = networkDiagram.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == "Sub");
				var shapeViewModel = node.DataContext as NodeViewModel;

				formNetworkViewModel.SelectSingleEntity(shapeViewModel.Entity);
				AssertEquals("Paste works now!", true, formNetworkViewModel.TryHandlePaste());
				AssertEquals(workflow.PK, formNetwork.Shapes[0].BNS_RelatedEntityID);
			}
		}

		public void TestLinkDiagramToDiagramFromClipBoard()
		{
			var diagram1 = CreateDiagram(Factory, "Root1");
			var diagram2 = CreateDiagram(Factory, "Root2");

			Factory.Save();

			CopyHyperlinkToClipboard(ControllerIDs.NetworkDiagram, diagram1);

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram2))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				networkDiagram.NetworkUserControlHosted.WaitOne(TimeSpan.FromSeconds(1));
				var formNetworkViewModel = networkDiagram.ViewModel.NetworkViewModel;

				var diagram = formNetworkViewModel.ActiveNode.Entity.AsShape();

				using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(formNetworkViewModel, diagram))
				{
					var parentMenuItem = formNetworkViewModel.GetFirstLevelNetworkActionsMenuItem_ForTesting(formNetworkViewModel.ActiveNode.Entity, "Linked Entity");

					AssertMenuItems(parentMenuItem, "Link to Root1 (from clipboard)", "Select Workflow...", "Select Diagram...");
					AssertMenuItemTooltips(parentMenuItem,
						@"Link to the hyperlink currently in the clipboard. (Paste will also link from the clipboard)

This action cannot be executed for the given shape(s) due to the following reasons:
Root2: Only jobs can be linked to the diagram surface.",
						"Choose a record from the Job Workflows module to which this shape will be linked.",
						@"Choose a record from the Network Diagrams module to which this shape will be linked.

This action cannot be executed for the given shape(s) due to the following reasons:
Root2: The action can be applied to shapes only.");
				}
			}
		}

		public void TestLinkShapeToDiagramFromClipBoard()
		{
			var diagram1 = CreateDiagram(Factory, "Root1");
			var shape1 = CreateShape(diagram1, "Shape1");
			var diagram2 = CreateDiagram(Factory, "Root2");

			Factory.Save();

			CopyHyperlinkToClipboard(ControllerIDs.NetworkDiagram, shape1);

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram2))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				networkDiagram.NetworkUserControlHosted.WaitOne(TimeSpan.FromSeconds(1));
				var formNetworkViewModel = networkDiagram.ViewModel.NetworkViewModel;

				var diagram = formNetworkViewModel.ActiveNode.Entity.AsShape();

				using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(formNetworkViewModel, diagram))
				{
					var parentMenuItem = formNetworkViewModel.GetFirstLevelNetworkActionsMenuItem_ForTesting(formNetworkViewModel.ActiveNode.Entity, "Linked Entity");

					AssertMenuItems(parentMenuItem, "Link to Shape1 (from clipboard)", "Select Workflow...", "Select Diagram...");
					AssertMenuItemTooltips(parentMenuItem,
						@"Link to the hyperlink currently in the clipboard. (Paste will also link from the clipboard)

This action cannot be executed for the given shape(s) due to the following reasons:
Root2: Only jobs can be linked to the diagram surface.",
						"Choose a record from the Job Workflows module to which this shape will be linked.",
						@"Choose a record from the Network Diagrams module to which this shape will be linked.

This action cannot be executed for the given shape(s) due to the following reasons:
Root2: The action can be applied to shapes only.");
				}
			}
		}

		public void TestLinkJobHeaderFromClipboard_PasteActionAndTooltip()
		{
			var jobHeaderToLinkToRoot = CreateJobHeader<OrgHeader>();
			var jobToLinkToRoot = (OrgHeader)jobHeaderToLinkToRoot.Parent;

			var diagram = CreateDiagram(Factory, name: "Root");
			var subDiagram = CreateShape(parentShape: diagram, name: "Sub");

			Factory.Save();

			CopyHyperlinkToClipboard(ControllerIDs.Organisation, jobToLinkToRoot);

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				networkDiagram.NetworkUserControlHosted.WaitOne(TimeSpan.FromSeconds(1));
				var formNetworkViewModel = networkDiagram.ViewModel.NetworkViewModel;

				var node = networkDiagram.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == "Sub");
				var shapeViewModel = node.DataContext as NodeViewModel;

				formNetworkViewModel.SelectSingleEntity(shapeViewModel.Entity);
				AssertEquals("Paste works now!", true, formNetworkViewModel.TryHandlePaste());
			}
		}

		public void TestLinkJobHeader_JobNumberIsVisible()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();

			var diagram = CreateDiagram(Factory, name: "Root");
			var subDiagram = CreateShape(parentShape: diagram, name: "Trunk");
			var shape = CreateShape(parentShape: subDiagram, name: "Leaf");
			CreateNetwork(diagram).SwitchToScaled();

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				networkControl.NetworkUserControlHosted.WaitOne(TimeSpan.FromSeconds(1));
				var formNetworkViewModel = networkControl.ViewModel.NetworkViewModel;
				var formNetwork = formNetworkViewModel.GetJobNetwork();

				Assert(formNetwork.LinkEntity(formNetwork["Trunk"], jobHeader1));
				Assert(formNetwork.LinkEntity(formNetwork["Leaf"], jobHeader2));
				Application.DoEvents();

				var trunkNode = networkControl.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == "Trunk");
				var leafNode = networkControl.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == "Leaf");

				AssertEquals(true, trunkNode.FindChildren<TextBlock>().Any(t => t.IsVisible && t.Text == ((IProposedNetworkEntity)jobHeader1).JobNumber));
				AssertEquals(true, leafNode.FindChildren<TextBlock>().Any(t => t.IsVisible && t.Text == ((IProposedNetworkEntity)jobHeader2).JobNumber));
			}
		}

		public void TestElementHostShouldHaveTabStopSetToTrue()
		{
			var diagram = CreateDiagram(Factory, name: "Rose");
			var shape = CreateShape(parentShape: diagram, name: "Verse");
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
				AssertEquals(true, elementHost.TabStop);
			}
		}

		#region Linked Entity Menu

		public void TestLinkJobHeaderFromClipboard()
		{
			var jobHeaderToLinkToRoot = CreateJobHeader<OrgHeader>();
			var jobToLinkToRoot = (OrgHeader)jobHeaderToLinkToRoot.Parent;
			jobToLinkToRoot.OH_Code = "FROGGER";
			jobToLinkToRoot.OH_FullName = "Soniqua";

			var diagram = CreateDiagram(Factory, name: "Root");
			var subDiagram = CreateShape(parentShape: diagram, name: "Sub");
			var network = NetworkTestCase.CreateNetwork(diagram);

			Factory.Save();

			CopyHyperlinkToClipboard(ControllerIDs.Organisation, jobToLinkToRoot);

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				networkDiagram.NetworkUserControlHosted.WaitOne(TimeSpan.FromSeconds(1));
				var formNetworkViewModel = networkDiagram.ViewModel.NetworkViewModel;

				var node = networkDiagram.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == "Sub");
				var shapeViewModel = node.DataContext as NodeViewModel;
				var shape = shapeViewModel.Entity.AsShape();

				using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(formNetworkViewModel, shape))
				{
					var parentMenuItem = formNetworkViewModel.GetFirstLevelNetworkActionsMenuItem_ForTesting(shapeViewModel.Entity, "Linked Entity");

					AssertMenuItems(parentMenuItem, "Link to Soniqua (from clipboard)", "Select Workflow...", "Select Diagram...");
					AssertMenuItemTooltips(parentMenuItem,
						"Link to the hyperlink currently in the clipboard. (Paste will also link from the clipboard)",
						"Choose a record from the Job Workflows module to which this shape will be linked.",
						"Choose a record from the Network Diagrams module to which this shape will be linked.");

					parentMenuItem.Items.First().Action.Execute();

					parentMenuItem = formNetworkViewModel.GetFirstLevelNetworkActionsMenuItem_ForTesting(shapeViewModel.Entity, "Linked Entity");

					AssertMenuItems(parentMenuItem, "Link to Soniqua (from clipboard)", "Select Workflow...", "Select Diagram...", null, "Open Linked Workflow", "Un-link this Workflow");
					AssertMenuItemTooltips(parentMenuItem,
						@"Link to the hyperlink currently in the clipboard. (Paste will also link from the clipboard)

This action cannot be executed for the given shape(s) due to the following reasons:
Sub: The hyperlink in the clipboard points to the same job or workflow which is already linked to this shape.",

						"Choose a record from the Job Workflows module to which this shape will be linked.",
						"Choose a record from the Network Diagrams module to which this shape will be linked.",
						null,
						"Open the linked workflow in its job's form.",
						"Un-links the shape from its currently-linked workflow.");
					AssertEquals(false, parentMenuItem.Items.First().Enabled);

					AssertEquals(jobHeaderToLinkToRoot.PK, shape.BNS_RelatedEntityID);
				}
			}
		}

		public void TestLinkJobHeaderFromClipBoard_WhenShapeIsAlreadyLinkedToAnotherHeader()
		{
			var alreadyLinkedHeader = CreateJobHeader<OrgHeader>();
			var alreadyLinkedJob = (OrgHeader)alreadyLinkedHeader.Parent;

			var jobHeaderToLink = CreateJobHeader<OrgHeader>();
			var jobToLink = (OrgHeader)jobHeaderToLink.Parent;
			jobToLink.OH_Code = "INTERSLICE";
			jobToLink.OH_FullName = "Compuglobal Hyper-meganet";

			var diagram = CreateDiagram(Factory, name: "Root");
			var subDiagram = CreateShape(alreadyLinkedHeader, parentShape: diagram, name: "Sub");
			var network = NetworkTestCase.CreateNetwork(diagram);

			Factory.Save();

			CopyHyperlinkToClipboard(ControllerIDs.Organisation, jobToLink);

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				networkDiagram.NetworkUserControlHosted.WaitOne(TimeSpan.FromSeconds(1));
				var formNetworkViewModel = networkDiagram.ViewModel.NetworkViewModel;

				var node = networkDiagram.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == "Sub");
				var shapeViewModel = node.DataContext as NodeViewModel;
				var shape = shapeViewModel.Entity.AsShape();

				using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(formNetworkViewModel, shape))
				{
					var parentMenuItem = formNetworkViewModel.GetFirstLevelNetworkActionsMenuItem_ForTesting(shapeViewModel.Entity, "Linked Entity");

					AssertMenuItems(parentMenuItem, "Link to Compuglobal Hyper-meganet (from clipboard)", "Select Workflow...", "Select Diagram...", null, "Open Linked Workflow", "Un-link this Workflow");
					AssertMenuItemTooltips(parentMenuItem,
						"Link to the hyperlink currently in the clipboard. (Paste will also link from the clipboard)",
						"Choose a record from the Job Workflows module to which this shape will be linked.",
						"Choose a record from the Network Diagrams module to which this shape will be linked.",
						null,
						"Open the linked workflow in its job's form.",
						"Un-links the shape from its currently-linked workflow.");

					parentMenuItem.Items.First().Action.Execute();

					AssertEquals(jobHeaderToLink.PK, shape.ProcessHeader.PK);
				}
			}
		}

		public void TestLinkJobHeaderFromClipboard_WhenClipboardDoesNotContainHeader()
		{
			var orgHeaderWithoutJobHeader = Factory.NewWithValidTestData<OrgHeader>();
			var diagram = CreateDiagram(Factory, name: "Root");
			var subDiagram = CreateShape(parentShape: diagram, name: "Sub");
			var network = NetworkTestCase.CreateNetwork(diagram);

			Factory.Save();

			CopyHyperlinkToClipboard(ControllerIDs.Organisation, orgHeaderWithoutJobHeader);

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				networkDiagram.NetworkUserControlHosted.WaitOne(TimeSpan.FromSeconds(1));
				var formNetworkViewModel = networkDiagram.ViewModel.NetworkViewModel;

				var node = networkDiagram.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == "Sub");
				var shapeViewModel = node.DataContext as NodeViewModel;

				var parentMenuItem = formNetworkViewModel.GetFirstLevelNetworkActionsMenuItem_ForTesting(shapeViewModel.Entity, "Linked Entity");

				AssertMenuItems(parentMenuItem, "Link to Entity from Clipboard", "Select Workflow...", "Select Diagram...");
				AssertMenuItemTooltips(parentMenuItem,
					@"Link to the hyperlink currently in the clipboard. (Paste will also link from the clipboard)

This action cannot be executed for the given shape(s) due to the following reasons:
Sub: The hyperlink in the clipboard cannot be linked to this shape.",

					"Choose a record from the Job Workflows module to which this shape will be linked.",
					"Choose a record from the Network Diagrams module to which this shape will be linked.");
			}
		}

		public void TestLinkJobHeaderFromClipboard_WhenClipboardIsEmpty()
		{
			var diagram = CreateDiagram(Factory, name: "Root");
			var subDiagram = CreateShape(parentShape: diagram, name: "Sub");
			var network = NetworkTestCase.CreateNetwork(diagram);

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				networkDiagram.NetworkUserControlHosted.WaitOne(TimeSpan.FromSeconds(1));
				var formNetworkViewModel = networkDiagram.ViewModel.NetworkViewModel;

				var node = networkDiagram.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == "Sub");
				var shapeViewModel = node.DataContext as NodeViewModel;

				var parentMenuItem = formNetworkViewModel.GetFirstLevelNetworkActionsMenuItem_ForTesting(shapeViewModel.Entity, "Linked Entity");

				AssertMenuItems(parentMenuItem, "Link to Entity from Clipboard", "Select Workflow...", "Select Diagram...");
				AssertMenuItemTooltips(parentMenuItem,
					@"Link to the hyperlink currently in the clipboard. (Paste will also link from the clipboard)

This action cannot be executed for the given shape(s) due to the following reasons:
Sub: There is no hyperlink in the clipboard. If a hyperlink has been copied to the clipboard then this shape can be linked to the job.",

					"Choose a record from the Job Workflows module to which this shape will be linked.",
					"Choose a record from the Network Diagrams module to which this shape will be linked.");
			}
		}

		#endregion

		#region Delete is handled safely

		public void TestDeleteRootFromSameFactory_DoesntCrash()
		{
			var diagram = CreateDiagram(Factory, name: "Root");
			var subDiagram = CreateShape(parentShape: diagram, name: "Sub");
			var network = NetworkTestCase.CreateNetwork(diagram);

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var source = (BusinessObject)form.DataSource;
				source.Factory.Load<BMNCNShape>(diagram.PK).Delete();
				source.Factory.Save();

				AssertNoExceptionThrown(() => Application.DoEvents());
			}
		}

		public void TestDeleteRootViaDataBusRefresh_DoesntCrash()
		{
			var diagram = CreateDiagram(Factory, name: "Root");
			var subDiagram = CreateShape(parentShape: diagram, name: "Sub");
			var network = NetworkTestCase.CreateNetwork(diagram);

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var diagramOnNewFactory = Factory.CreateNewFactory().Load<BMNCNShape>(diagram.PK);
				diagram.Delete();
				diagram.Factory.Save();

				AssertNoExceptionThrown(() => Application.DoEvents());
				Assert(((BMNCNShape)form.DataSource).IsDeleted);
			}
		}

		#endregion

		#region Implementation

		ClipboardTestHelper clipboardTestHelper;

		protected override void SetUp()
		{
			base.SetUp();

			clipboardTestHelper = new ClipboardTestHelper();
			clipboardTestHelper.MockClipboard();
		}

		static void CopyHyperlinkToClipboard(ControllerID controllerId, BusinessObject bizo)
		{
			using (var orgForm = (ZForm)ZControllerFactory.Create(controllerId).ShowEditForm(bizo))
			{
				ZFormMenuStrategy.CopyHyperlinkToClipboard(orgForm);
			}
		}

		static void AssertMenuItems(NetworkActionMenuItem menuItem, params string[] expectedMenuLabels)
		{
			var actualMenuLabels = menuItem.Items.Select(i => i?.Name).ToArray();

			AssertArrayEqualsByElements(expectedMenuLabels, actualMenuLabels);
		}

		static void AssertMenuItemTooltips(NetworkActionMenuItem menuItem, params string[] expectedMenuTooltips)
		{
			var actualMenuTooltips = menuItem.Items.Select(i => i?.Tooltip).ToArray();

			AssertArrayEqualsByElements(expectedMenuTooltips, actualMenuTooltips);
		}

		#endregion
	}
}
