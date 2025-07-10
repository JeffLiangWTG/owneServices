using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class CloneDiagramActionGuiTest : SpawningIndependentNetworkActionGuiTestCase<CloneDiagramAction>
	{
		#region Test Overrides

		protected override INetworkViewModel GetNetworkViewModel(MockRepository mocks, BusinessObjectFactory factory)
		{
			var diagram = CreateDiagram(factory);
			var interactionImplementor = new Mock<IBMNetworkUserInteractionImplementor>();
			var controller = CreateController(factory, CreateRefresher(), interactionImplementor.Object);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller);

			interactionImplementor
				.Setup(m => m.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<ConfirmationNotification[]>()))
				.Returns(true);

			return networkViewModel;
		}

		protected override CloneDiagramAction GetAction(INetworkViewModel networkViewModel) => new CloneDiagramAction(networkViewModel);

		#endregion
	}
}
