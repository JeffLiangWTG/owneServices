using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class CreateAnnotationAction : CreateEntityActionBase
	{
		public CreateAnnotationAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		#region CreateShapeActionBase Overrides

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("4f94365b-443e-4335-9ade-231e78ad8cd9", "Annotation");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("b1366796-eb15-4aa6-bd20-412f6d613255", "Creates a new annotation and adds it to the diagram");
		}

		protected override string IconName => (NoResString)"Bubble"; // resource name

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return base.IsApplicableCore(shape).Union(BMNetworkActionAccessibilityHelper.CheckShapeIsNotDefaultDiagramChild(shape));
		}

		protected override void SetDefaultsForNewShape(IJobNetwork network, BMNCNShape newShape, BMNCNShape parentShape)
		{
			newShape.BNS_ShapeType = ShapeTypeList.Codes.Annotation;
			newShape.BackColor = "AliceBlue"; // It's a color name
			newShape.BNS_Name = Res.GetString("ec546cbf-b758-4070-b939-c00d24b0ace9", "New Annotation");
			var bizo = newShape.ScheduleBizo;
			if (bizo != null)
			{
				bizo.Delete(); // TODO: Remove this when we have BMNCNAttachmentShape. Prevents Events from creating SB.
			}
		}

		#endregion
	}
}
