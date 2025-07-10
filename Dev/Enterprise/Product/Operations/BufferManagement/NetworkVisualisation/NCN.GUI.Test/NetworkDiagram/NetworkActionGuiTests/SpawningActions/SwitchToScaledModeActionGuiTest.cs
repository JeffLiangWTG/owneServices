using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class SwitchToScaledModeActionGuiTest : SpawningIndependentNetworkActionGuiTestCase<SwitchToScaledModeAction>
	{
		#region Test Overrides

		protected override INetworkViewModel GetNetworkViewModel(MockRepository mocks, BusinessObjectFactory factory)
		{
			var diagram = CreateDiagram(factory);
			var interactionImplementor = new Mock<IBMNetworkUserInteractionImplementor>();
			var controller = CreateController(factory, CreateRefresher(), interactionImplementor.Object);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller);

			interactionImplementor.Setup(m => m.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ConfirmationNotification[]>())).Returns(true);

			factory.Save();

			return networkViewModel;
		}

		protected override SwitchToScaledModeAction GetAction(INetworkViewModel networkViewModel) => new SwitchToScaledModeAction(networkViewModel);

		#endregion
	}
}
