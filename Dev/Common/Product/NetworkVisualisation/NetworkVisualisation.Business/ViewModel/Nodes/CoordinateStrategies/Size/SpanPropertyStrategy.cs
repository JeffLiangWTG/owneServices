using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	abstract class SpanPropertyStrategy : CoordinatePropertyStrategy
	{
		public SpanPropertyStrategy(Action<INetworkEntity, double> entitySetter, Func<INetworkEntity, double> entityGetter, NetworkViewModel networkViewModel, bool shouldConsiderDescendants)
			: base(entitySetter, entityGetter, networkViewModel, shouldConsiderDescendants)
		{
		}

		protected override void UpdateChildNodes(NodeViewModel child, double originalValue, double updatedValue)
		{
			// Do nothing
		}

		protected abstract double GetLowerBound(NodeViewModel node, IEnumerable<INetworkEntity> childEntities);

		protected abstract Range<double> GetHighRange(NodeViewModel node, INetworkEntity parentLayout);

		protected abstract double GetOffSet(INetworkEntity networkEntity);

		protected abstract Range<double> GetRangeForFixedDiagramSurface(double lowerBound, INetworkEntity parentLayout, double offset);

		protected override Range<double> GetValidRange(NodeViewModel node)
		{
			var childEntities = node.Entity.Children ?? Enumerable.Empty<INetworkEntity>();
			var lowerBound = GetLowerBound(node, childEntities);
			lowerBound = Math.Max(lowerBound, 1d);

			var parentLayout = node.Entity.Parent as INetworkEntity;
			if (parentLayout == null)
			{
				return new Range<double>(lowerBound, double.MaxValue);
			}
			else if (parentLayout.IsSameEntity(networkViewModel.Network.DiagramEntity))
			{
				return GetRangeForFixedDiagramSurface(lowerBound, parentLayout, GetOffSet(node.Entity));
			}
			else
			{
				var highRange = GetHighRange(node, parentLayout);
				return new Range<double>(lowerBound, Math.Max(highRange.Maximum - GetOffSet(node.Entity), 1d));
			}
		}
	}
}
