using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public abstract class CreateShapeActionBase : CreateEntityActionBase
	{
		protected CreateShapeActionBase(INetworkViewModel networkViewModel, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(networkViewModel, group, groupIndex, shouldUpdateOnNetworkEvents)
		{
		}

		protected void SetDefaultsForNewShapeCore(IJobNetwork network, BMNCNShape newShape, BMNCNShape parentShape)
		{
			newShape.BackColor = (NoResString)"Bisque"; // It's a color name

			var workflow = GetNewProcessHeaderForShape(network, newShape, parentShape);

			if (workflow != null)
			{
				network.LinkEntity(newShape, workflow);
			}
		}

		protected override void SetDefaultsForNewShape(IJobNetwork network, BMNCNShape newShape, BMNCNShape parentShape)
		{
			newShape.BNS_ShapeType = ShapeTypeList.Codes.Shape;

			SetDefaultsForNewShapeCore(network, newShape, parentShape);

			var defaultName = GetDefaultName(network, newShape);

			if (defaultName != null)
			{
				newShape.Name = defaultName;

				if (newShape.ProcessHeader is ProcessHeader processHeader)
				{
					processHeader.Name = defaultName;
				}
			}
		}

		protected abstract ProcessHeader GetNewProcessHeaderForShape(IJobNetwork network, BMNCNShape newShape, BMNCNShape parentShape);

		protected virtual string GetDefaultName(IJobNetwork network, BMNCNShape newShape) => null;
	}
}
