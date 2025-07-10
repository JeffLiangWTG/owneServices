using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class CloneDiagramAction : SpawningIndependentNetworkAction
	{
		public CloneDiagramAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return BMNetworkActionAccessibilityHelper.CheckEntityIsRoot(Network.Entities.GetInstance(shape));
		}

		protected override INetworkActionAccessibility PerformPreExecutionChecksForShape(BMNCNShape shape) => NetworkActionAccessibility.Allowed;

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			if (!ShouldProceedWithNoBuffers())
			{
				return;
			}

			shape.RequireDiagram();

			var factoryForSpawnedNetwork = CreateNewFactoryForSpawnedNetwork();
			var clone = Network.DiagramEntity.CloneDiagramAndAllDescendants(factoryForClone: factoryForSpawnedNetwork);
			clone.BNS_Name += " " + Res.GetString("76c17125-865c-49cd-84c5-385ba0cf6df0", "(copy)");
			clone.BNS_ShapeType = ShapeTypeList.Codes.Diagram;

			var cloneNetwork = CreateSpawnedTemporaryNetwork(clone);
			cloneNetwork.RefreshSchedules();

			Controller.ViewDiagram(clone);
		}

		bool ShouldProceedWithNoBuffers()
		{
			if (Network.Shapes.Any(s => s.IsBufferShape))
			{
				var caption = Res.GetString("68C196F7-62B0-4262-803E-45D93702D836", "Buffers cannot be recreated on diagram copies");
				var message = Res.GetString("D155E786-643D-4CD4-8CC4-736647F79374", "The original diagram contains buffers which will not be created on the diagram copy. Do you want to proceed?");
				return Controller.UserInteractionImplementor.HasUserAnsweredYes(message, caption);
			}
			return true;
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("f04a3be1-5f9d-4630-9b55-e25c017ee267", "Clone Diagram");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("bbd8da10-e6ef-43b3-85e4-f7f73535eb16", "Clones the current diagram and opens it in a new form");
		}

		protected override string IconName => (NoResString)"Copy"; // resource name

		protected override bool RequiresUserConfirmation => true;

		protected override string UserConfirmationMessage => Res.GetString("6c595ee7-5bbc-4457-91e6-4166b23240a5", "Are you sure you want to create a new copy of this diagram?");

		protected override bool RequiresValidateBeforeExecute => true;
	}
}
