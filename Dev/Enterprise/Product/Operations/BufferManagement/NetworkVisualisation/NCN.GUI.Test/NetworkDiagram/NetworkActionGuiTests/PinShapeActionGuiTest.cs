using CargoWise.NetworkVisualisation.Business.Test;
using Enterprise.BufferManagement.NetworkVisualisation.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	public class PinShapeActionGuiTest : NetworkGUITestCase
	{
		public void TestShouldBeEnabled_WhenShapeOpenedAsDiagram()
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

				var action = new PinShapeAction(info.NetworkViewModel);
				NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableToEntity(newChildEntity));
				NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledForEntity(newChildEntity));
			}
		}
	}
}
