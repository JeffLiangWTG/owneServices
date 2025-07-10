using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	abstract class CoordinatePropertyStrategyBase
	{
		protected abstract bool SetPropertyCore(double value, NodeViewModel node, INetworkEntity entity);

		internal bool SetProperty(double value, NodeViewModel node, INetworkEntity entity)
		{
			var result = false;

			node.UpdatePositionCore(() =>
			{
				result = SetPropertyCore(value, node, entity);
			});

			return result;
		}
	}
}
