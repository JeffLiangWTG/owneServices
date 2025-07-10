using System;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	class ResizeStrategy : CoordinatePropertyStrategyBase
	{
		internal ResizeStrategy(Action<INetworkEntity, double> entitySetter, Func<INetworkEntity, double> entityGetter, bool shouldConsiderDescendants)
		{
			this.entitySetter = entitySetter;
			this.entityGetter = entityGetter;
			this.shouldConsiderDescendants = shouldConsiderDescendants;
		}

		readonly Action<INetworkEntity, double> entitySetter;
		readonly Func<INetworkEntity, double> entityGetter;
		readonly bool shouldConsiderDescendants;

		protected override bool SetPropertyCore(double value, NodeViewModel node, INetworkEntity entity)
		{
			var curValue = entityGetter(entity);
			var diff = value - curValue;
			entitySetter(entity, value);

			if (shouldConsiderDescendants)
			{
				node.UpdateChildNodes(child => CompensateRelativePositionOfChildNode(diff, child));
			}

			return true;
		}

		void CompensateRelativePositionOfChildNode(double diff, NodeViewModel node)
		{
			// we need to compensate the immediate children for the parent move, since these children are positioned relative to the parent
			var curValue = entityGetter(node.Entity);
			var newValue = curValue - diff;
			entitySetter(node.Entity, newValue);
		}
	}
}