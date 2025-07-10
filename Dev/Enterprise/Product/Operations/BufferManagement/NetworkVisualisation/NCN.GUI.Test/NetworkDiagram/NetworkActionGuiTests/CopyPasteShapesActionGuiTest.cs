using System.Linq;
using System.Windows.Forms.Integration;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Network.NetworkEntity;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test.NetworkDiagram.NetworkActionGuiTests
{
#if !WINZOR
	class CopyPasteShapesActionGuiTest : NetworkGUITestCase
	{
		public void TestIfStateContainerIsEmpty()
		{
			//arrange
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			var network = CreateNetwork(diagram);

			Factory.Save();

			//act
			using (var form = new NetworkDiagramForm(diagram) { Size = ControlDpiScalingHelper.NewScaledSize(1600, 1000) })
			{
				form.Show();
				DoEventsThoroughly();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var diagramControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				ShapeStateContainer.Clear();
				diagramControl.NonScheduledDiagramControl.CtrlVIsPressed();
				DoEventsThoroughly();
				var noShapeIsInDiagram = !diagramControl.NonScheduledDiagramControl.NetworkViewModel.Nodes.Any();

				//assert
				AssertEquals("new node must be inserted wit the same properties", noShapeIsInDiagram, true);
			}
		}

		public void TestCopyShapeAndPasteShapeInASingleDiagram()
		{
			//arrange
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			var network = CreateNetwork(diagram);

			var shapeToBeCopied = CreateShape(diagram, "shapeToBeCopied");
			var shapeToBeCopiedEntity = shapeToBeCopied.AsEntity(network);

			shapeToBeCopiedEntity.X = 100;
			shapeToBeCopiedEntity.Y = 100;
			shapeToBeCopiedEntity.Width = 600;
			shapeToBeCopiedEntity.Height = 600;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram) { Size = ControlDpiScalingHelper.NewScaledSize(1600, 1000) })
			{
				form.Show();
				DoEventsThoroughly();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var diagramControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				//act
				diagramControl.NetworkViewModel.Nodes.FirstOrDefault().IsSelected = true;
				DoEventsThoroughly();
				diagramControl.NonScheduledDiagramControl.CtrlCIsPressed();
				diagramControl.NetworkViewModel.Nodes.FirstOrDefault().IsSelected = false;
				diagramControl.NonScheduledDiagramControl.CtrlVIsPressed();
				DoEventsThoroughly();
				var twoNodesInDiagram = diagramControl.NonScheduledDiagramControl.NetworkViewModel.Nodes.Count() > 1;

				//assert
				AssertEquals("new node must be inserted wit the same properties", true, twoNodesInDiagram);
			}
		}

		public void TestCopyShapeAndPasteShapeBetweenTwoDiagrams()
		{
			//arrange
			var diagram1 = CreateDiagram(Factory, isScaled: true);
			var diagram2 = CreateDiagram(Factory, isScaled: true);

			diagram1.ShouldShowNonScheduledSection = true;
			diagram2.ShouldShowNonScheduledSection = true;

			var network1 = CreateNetwork(diagram1);
			var network2 = CreateNetwork(diagram2);

			var shapeToBeCreatedInDiagram1 = CreateShape(diagram1, "shapeToBeCopied");
			var shapeToBeCreatedInDiagram1Entity = shapeToBeCreatedInDiagram1.AsEntity(network1);

			shapeToBeCreatedInDiagram1Entity.X = 100;
			shapeToBeCreatedInDiagram1Entity.Y = 100;
			shapeToBeCreatedInDiagram1Entity.Width = 600;
			shapeToBeCreatedInDiagram1Entity.Height = 600;

			Factory.Save();

			using (var form1 = new NetworkDiagramForm(diagram1) { Size = ControlDpiScalingHelper.NewScaledSize(1600, 1000) })
			{
				form1.Show();
				DoEventsThoroughly();

				var elementHost = form1.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var diagramControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				// creating aShape in Diagram1
				diagramControl.NetworkViewModel.Nodes.FirstOrDefault().IsSelected = true;
				DoEventsThoroughly();
				diagramControl.NonScheduledDiagramControl.CtrlCIsPressed();
				diagramControl.NetworkViewModel.Nodes.FirstOrDefault().IsSelected = false;
				DoEventsThoroughly();

				using (var form2 = new NetworkDiagramForm(diagram2) { Size = ControlDpiScalingHelper.NewScaledSize(1600, 1000) })
				{
					form2.Show();
					DoEventsThoroughly();

					var elementHost2 = form2.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
					var diagramControl2 = (NetworkUserControl)elementHost2.Child;
#pragma warning restore CS0618 // Type or member is obsolete

					// act
					diagramControl2.NonScheduledDiagramControl.CtrlVIsPressed();
					DoEventsThoroughly();
					var nodeIsAddedToDiagram2 = diagramControl2.NonScheduledDiagramControl.NetworkViewModel.Nodes.Count() == 1;

					//assert
					AssertEquals("new node must be inserted wit the same properties", true, nodeIsAddedToDiagram2);
				}
			}
		}
	}
#endif
}
