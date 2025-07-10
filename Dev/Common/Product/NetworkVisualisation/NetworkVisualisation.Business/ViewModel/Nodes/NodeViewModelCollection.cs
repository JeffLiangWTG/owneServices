using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class NodeViewModelCollection : KeyedObservableCollection<INetworkEntity, NodeViewModel>
	{
		public NodeViewModelCollection()
			: base(n => n.Entity)
		{
		}
	}
}
