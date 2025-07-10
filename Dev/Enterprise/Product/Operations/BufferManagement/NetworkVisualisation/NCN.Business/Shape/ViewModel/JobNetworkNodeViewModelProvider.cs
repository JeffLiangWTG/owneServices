using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class JobNetworkNodeViewModelProvider : NodeViewModelProvider
	{
		public override NodeViewModel Create(INetworkEntity entity, NetworkViewModel networkViewModel)
		{
			var bizo = entity as BusinessObject;
			if (bizo != null && bizo.IsDeleted)
			{
				return null;
			}

			switch (entity.ShapeType)
			{
				case ShapeTypeList.Codes.Shape:
					return new JobNetworkNodeViewModel(entity, networkViewModel);

				case ShapeTypeList.Codes.Buffer:
					return new BufferViewModel(entity, networkViewModel);

				default:
					return base.Create(entity, networkViewModel);
			}
		}
	}
}
