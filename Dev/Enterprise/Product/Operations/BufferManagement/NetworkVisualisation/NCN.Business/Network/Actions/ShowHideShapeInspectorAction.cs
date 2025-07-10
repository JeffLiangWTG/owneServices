using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ShowHideShapeInspectorAction : JobNetworkAction
	{
		public ShowHideShapeInspectorAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, executionStrategy: null, group: group, groupIndex: groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override ResourceString GetDefaultNameCore() => ResString.GetMultilingualString("111DA07E-70BE-47A8-9E43-62F7CB526E94", "Shape Inspector");

		protected override ResourceString GetDefaultDescriptionCore() => ResString.GetMultilingualString("1CFD7F05-7C30-4F28-9160-D39AB3579D71", "Show / Hide Shape Inspector");

		protected override string IconName => "PictureInPicture"; // resource name

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return new NetworkActionAccessibility(true, shape, () => Res.GetString("77633375-DB7E-4C35-924A-8548A31F63D4", "Should be a single shape or an annotation."));
		}

		protected override bool IsActivatedCore(BMNCNShape shape)
		{
			var diagram = shape.FindTopmostDiagramShape();
			if (!diagram.IsDiagram)
			{
				return false;
			}

			return diagram.ShapeInspectorVisible;
		}

		protected override void ExecuteForShape(BMNCNShape shape)
		{
			var diagram = shape.FindTopmostDiagramShape();
			if (!diagram.IsDiagram)
			{
				return;
			}

			diagram.ShapeInspectorVisible = !diagram.ShapeInspectorVisible;

			if (shape.ShapeType == ShapeTypeList.Codes.Shape || shape.ShapeType == ShapeTypeList.Codes.Annotation)
			{
				Network.Refresh(RefreshType.ShapeInspectorVisibilityChanged, shape);
			}
			else
			{
				Network.Refresh(RefreshType.ShapeInspectorVisibilityChanged);
			}
		}
	}
}
