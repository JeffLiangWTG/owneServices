using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class CreateShapeAction : CreateShapeActionBase
	{
		public CreateShapeAction(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected override ResourceString GetDefaultNameCore()
		{
			return ResString.GetMultilingualString("736d1cca-ce16-489b-9ee2-0abf8a3f6f46", "Shape");
		}

		protected override ResourceString GetDefaultDescriptionCore()
		{
			return ResString.GetMultilingualString("dc7c82ce-14cd-4100-8d15-f9a20fee93cb", "Creates a new leaf shape that is not linked to a business entity");
		}

		protected override string IconName => (NoResString)"Shape"; // resource name

		protected override INetworkActionAccessibility IsApplicableCore(BMNCNShape shape)
		{
			return base.IsApplicableCore(shape).Union(BMNetworkActionAccessibilityHelper.CheckShapeDoesNotBelongToDefaultDiagram(shape));
		}

		protected override ProcessHeader GetNewProcessHeaderForShape(IJobNetwork network, BMNCNShape newShape, BMNCNShape parentShape)
		{
			return null;
		}

		protected override string GetDefaultName(IJobNetwork network, BMNCNShape newShape)
		{
			return Res.GetString("18c8bbab-e53c-4925-a76a-41dd21b5202b", "New Shape");
		}
	}
}
