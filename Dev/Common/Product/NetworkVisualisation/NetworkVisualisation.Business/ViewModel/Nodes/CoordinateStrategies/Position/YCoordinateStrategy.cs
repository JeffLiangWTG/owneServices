using System;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	class YCoordinateStrategy : CoordinatePropertyStrategy
	{
		internal YCoordinateStrategy(Action<INetworkEntity, double> entitySetter, Func<INetworkEntity, double> entityGetter, NetworkViewModel networkViewModel, bool shouldConsiderDescendants = true)
			: base(entitySetter, entityGetter, networkViewModel, shouldConsiderDescendants)
		{
		}

		protected override void UpdateChildNodes(NodeViewModel child, double originalValue, double updatedValue)
		{
			child.SetY(child.Y); // Reconstrain to boundaries after parent is moved.
		}

		protected override Range<double> GetValidRange(NodeViewModel node)
		{
			return GetValidRange(GetAcceptableLowYRange, GetAcceptableHighYRange, node.Height, node.Entity);
		}
	}
}
