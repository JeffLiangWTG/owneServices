using System;
using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	class SuspendedBoundsCoordinatePropertyStrategy : CoordinatePropertyStrategyBase
	{
		internal SuspendedBoundsCoordinatePropertyStrategy(Action<INetworkEntity, double> entitySetter)
		{
			this.entitySetter = entitySetter;
			this.modifiedEntities = new HashSet<Tuple<NodeViewModel, INetworkEntity>>();
		}

		readonly Action<INetworkEntity, double> entitySetter;
		readonly HashSet<Tuple<NodeViewModel, INetworkEntity>> modifiedEntities;

		protected override bool SetPropertyCore(double value, NodeViewModel node, INetworkEntity entity)
		{
			entitySetter(entity, value);
			modifiedEntities.Add(Tuple.Create(node, entity));
			return true;
		}

		internal void UpdateSuspended(CoordinatePropertyStrategy strategy)
		{
			foreach (var tuple in modifiedEntities)
			{
				var node = tuple.Item1;
				var entity = tuple.Item2;

				strategy.RefreshProperty(node, entity);
			}
		}
	}
}
