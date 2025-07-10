using System.Linq;
using CargoWise.NetworkVisualisation.Business.Test;
using Enterprise.BufferManagement.NetworkVisualisation.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	public class NetworkActionsMenuItemsTest : NetworkGUITestCase
	{
		public void TestShouldNotThrow_WhenShowingContextMenuForChildShapeOfShapeOpenedAsDiagram()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);

			networkViewModel.GetJobController().ViewDiagram(shape);
			using (var info = GetOpenedDiagramFormInfoAndCloseOnDispose())
			{
				info.JobNetwork.DiagramShape.SwitchToScaled();

				var openedEntity = info.JobNetwork.DiagramEntity;
				Assert("Opened shape should be in scaled mode", openedEntity.IsScaled);

				var newChildEntity = CreateShape(openedEntity);
				var newChildNode = NetworkVisualisationTestHelper.AddEntityToNetworkAndCreateNode(info.NetworkViewModel, newChildEntity);

				AssertNoExceptionThrown(() => newChildNode.MenuItems.ToArray());
				AssertNoExceptionThrown(() => info.NetworkViewModel.DiagramNodeViewModel.MenuItems.ToArray());
			}
		}
	}
}
