using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class NodeViewModelProvider
	{
		public virtual NodeViewModel Create(INetworkEntity entity, NetworkViewModel networkViewModel)
		{
			switch (entity.ShapeType)
			{
				case ShapeTypes.Annotation:
					return new AnnotationViewModel(entity, networkViewModel);

				default:
					return new NodeViewModel(entity, networkViewModel);
			}
		}
	}
}
