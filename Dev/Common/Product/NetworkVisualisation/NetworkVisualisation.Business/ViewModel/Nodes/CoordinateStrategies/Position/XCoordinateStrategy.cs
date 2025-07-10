using System;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	class XCoordinateStrategy : CoordinatePropertyStrategy
	{
		internal XCoordinateStrategy(Action<INetworkEntity, double> entitySetter, Func<INetworkEntity, double> entityGetter, NetworkViewModel networkViewModel, bool shouldConsiderDescendants = true)
			: base(entitySetter, entityGetter, networkViewModel, shouldConsiderDescendants)
		{
		}

		protected override void UpdateChildNodes(NodeViewModel child, double originalValue, double updatedValue)
		{
			child.SetX(child.X); // Reconstrain to boundaries after parent is moved.
		}

		protected override Range<double> GetValidRange(NodeViewModel node)
		{
			return GetValidRange(GetAcceptableLowXRange, GetAcceptableHighXRange, node.Width, node.Entity);
		}
	}
}
