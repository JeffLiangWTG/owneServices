using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
#if !WINZOR
using System.Windows.Media;
#endif
using CargoWise.NetworkVisualisation.GUI;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class NetworkDiagramFormFactoryTest : NetworkTestCase
	{
		public void TestShowForm_ForWorkflowShape()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow");

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram);

			Factory.Save();

			using (var form = NetworkDiagramFormFactory.ShowNetworkFormForDiagramContainingShape(shape) as NetworkDiagramForm)
			{
				AssertNotNull("Should open a network diagram form", form);
				var boundDiagram = (BMNCNShape)form.BusinessEntity;
				AssertEquals(diagram.PK, boundDiagram.PK);
			}
		}

#if !WINZOR
		public void TestFormattedTextHeightIsAppropriate()
		{
			var superDiagram = CreateDiagram(Factory, name: "Method Container");
			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(superDiagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var networkControlViewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;

				var textcontrol = networkDiagram.FindChildren<TextBoxWithPlaceholder>().LastOrDefault();
				AssertNotNull(textcontrol);
				var foreground = System.Windows.SystemColors.GrayTextBrush.Clone();
				var typeface = new Typeface(textcontrol.FontFamily, textcontrol.FontStyle, textcontrol.FontWeight,
					textcontrol.FontStretch);
#pragma warning disable CS0618 // Type or member is obsolete (.NET Framework 4.6.2)
				var formattedText = new FormattedText("someting to show", CultureInfo.CurrentCulture,
					textcontrol.FlowDirection, typeface, textcontrol.FontSize, foreground);
#pragma warning restore CS0618 // Type or member is obsolete
				var proposedHeight = textcontrol.RenderSize.Height - textcontrol.BorderThickness.Top - textcontrol.BorderThickness.Bottom - textcontrol.Padding.Top - textcontrol.Padding.Bottom;

				AssertEquals("formattedText.Height should not be larger than proposed Height", true, formattedText.Height <= proposedHeight);
			}
		}

		public void TestCompletionCriteriaTextWrapped()
		{
			var diagram = CreateDiagram(Factory, name: "Main Diagram");
			var shape = CreateShape(diagram, "MyShape");
			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.NetworkDiagram).ShowEditForm(diagram))
			{
				Application.DoEvents();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var networkDiagram = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var networkControlViewModel = (NetworkUserControlViewModel)networkDiagram.DataContext;

				var shapeNode = networkDiagram.FindNodeItem("MyShape");
				var textcontrol = shapeNode.FindChildren<TextBoxWithPlaceholder>().LastOrDefault();
				AssertNotNull(textcontrol);
				textcontrol.Text = "This is a long Completion Criteria text to test the wrapping, text should be moved to another row of the text control. This is a long Completion Criteria text to test the wrapping, text should be moved to another row of the text control.";
				Application.DoEvents();
				AssertEquals("TextBox wrapped", System.Windows.TextWrapping.Wrap, textcontrol.TextWrapping);
			}
		}
#endif
		public void TestShowForm_ForWorkflowShape_AndSelectShape()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow3");

			var diagram = CreateDiagram(jobHeader);
			var shape1 = CreateShape(workflow1, diagram);
			var shape2 = CreateShape(workflow2, diagram);
			var shape3 = CreateShape(workflow3, diagram);

			Factory.Save();

			using (var form = NetworkDiagramFormFactory.ShowNetworkFormForDiagramContainingShape(shape3) as NetworkDiagramForm)
			{
				Application.DoEvents(); // need to pump the queue now to let NetworkUserControl create a NetworkViewModel in SetDataContext() to get it ready for selection operations

				AssertNotNull("Should open a network diagram form", form);
				var boundDiagram = (BMNCNShape)form.BusinessEntity;
				AssertEquals(diagram.PK, boundDiagram.PK);

				var networkDiagramUserControl = (NetworkDiagramUserControl)form.Controls.Find("NetworkDiagramControl", true)[0];
				AssertNotNull(networkDiagramUserControl);

				var elementHost = (ZElementHost)form.Controls.Find("WPFElementHost", true)[0];
#pragma warning disable CS0618 // Type or member is obsolete
				var networkUserControl = (NetworkUserControl)elementHost?.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				AssertNotNull(networkUserControl);

				var networkViewModel = networkDiagramUserControl.NetworkViewModel;
				AssertNotNull(networkViewModel);
				AssertEquals(3, networkViewModel.Nodes.Count());

				var message = "\n" +
					"Given a shape,\n " +
					"When opening a diagram for the shape, \n" +
					"Then the NCN Diagram should open \n " +
					"And the passed in shape should be selected";
				AssertEquals(message, shape3.PK, networkViewModel.FirstSelectedEntity.AsShape().PK);
			}
		}

		public void TestShowForm_ForWorkflowShape_AndSelectShape_ButShapeIsNotInTheDb()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();

			var diagram = CreateDiagram(jobHeader);

			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow1");
			Factory.Save();

			var shape = CreateShape(workflow1, diagram);

			using (var form = NetworkDiagramFormFactory.ShowNetworkFormForDiagramContainingShape(shape) as NetworkDiagramForm)
			{
				Application.DoEvents();
				AssertNotNull("Should open a network diagram form", form);
			}
		}

		public void TestShowForm_ForDiagram()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow");

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram);

			Factory.Save();

			using (var form = NetworkDiagramFormFactory.ShowNetworkFormForDiagramContainingShape(diagram) as NetworkDiagramForm)
			{
				AssertNotNull("Should open a network diagram form", form);
				var boundDiagram = (BMNCNShape)form.BusinessEntity;
				AssertEquals(diagram.PK, boundDiagram.PK);
			}
		}

		public void TestShowForm_WhenMultipleOwnersPresent_ShouldPreferApprovedShape()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow");

			var diagram1 = CreateDiagram(jobHeader);
			var diagram2 = CreateDiagram(jobHeader);
			((IApprovable)diagram2).Approve(GlbStaff.CurrentUser.GS_Code);

			var shape = CreateShape(workflow, diagram1);
			AttachChildToParent(shape, diagram2);

			Factory.Save();

			using (var form = NetworkDiagramFormFactory.ShowNetworkFormForDiagramContainingShape(shape) as NetworkDiagramForm)
			{
				AssertNotNull("Should open a network diagram form", form);
				var boundDiagram = (BMNCNShape)form.BusinessEntity;
				AssertEquals("Should open form for approved diagram", diagram2.PK, boundDiagram.PK);
			}
		}
	}
}
