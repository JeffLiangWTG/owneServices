using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class OpenAsDiagramAction : JobNetworkAction
	{
		public OpenAsDiagramAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region Main Properties

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("f4eebb5a-bdf2-4a34-ab39-5b7dafc4768c", "Open as Diagram");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("f0d0a726-ff90-48ad-90da-f291074bd117", "Open this shape as a diagram in its own form");

		protected override string IconName => "PopOut";

		#endregion

		#region Accessibility

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape)
				.UnionIfAllowed(() => BMNetworkActionAccessibilityHelper.CheckEntityIsNotRoot(Network.Entities.GetInstance(shape))
					.Union(BMNetworkActionAccessibilityHelper.CheckIsShape(shape)));
		}

		#endregion

		#region Execution

		protected override bool RequiresSaveBeforeExecute => true;

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			Network.Controller.ViewDiagram(shape);
		}

		#endregion
	}
}
