using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class CreateProjectBufferAction : JobNetworkAction
	{
		public CreateProjectBufferAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckDiagramIsScaled(Network.DiagramEntity, shape)
					.Union(BMNetworkActionAccessibilityHelper.CheckShapeCanHaveChildren(shape)));
		}

		protected override INetworkActionAccessibility IsEnabledCore(BMNCNShape shape)
		{
			var entity = Network.Entities.GetInstance(shape);
			var hasExistingProjectBuffer = Network.Shapes.OfType<BMNCNBufferShape>().Any(s => s.BufferType == BufferTypeList.Codes.Project);
			return new NetworkActionAccessibility(
					entity.IsRoot || entity.IsCriticalPath && !entity.PostRequisiteLinks.Any(),
					shape,
					() => Res.GetString("82FC09A1-8DCF-41E9-A0EB-82F1AD2D2146", "The shape should be either the root diagram or the last shape on the critical path."))
				.Union(new NetworkActionAccessibility(
					!hasExistingProjectBuffer,
					shape,
					() => Res.GetString("82abda0f-5ab4-47a2-a634-02b5f01c2ffd", "There is already a project buffer on this diagram.")));
		}

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			BufferCreator.AddProjectBuffer(Network);

			Network.Refresh(RefreshType.RedrawDiagram);
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("5b92dd7d-2533-429b-9753-74105fa35cdc", "Create Project Buffer");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("c307def1-25a9-45a8-9f50-f682e37cd630", "Creates a project buffer at the end of the Critical Chain on this diagram.");
		}

		protected override string IconName => (NoResString)"Buffer"; // resource name
	}
}
