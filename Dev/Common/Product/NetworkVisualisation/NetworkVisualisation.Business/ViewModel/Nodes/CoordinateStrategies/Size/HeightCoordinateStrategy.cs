using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	class HeightCoordinateStrategy : SpanPropertyStrategy
	{
		internal HeightCoordinateStrategy(Action<INetworkEntity, double> entitySetter, Func<INetworkEntity, double> entityGetter, NetworkViewModel networkViewModel, bool shouldConsiderDescendants = true)
			: base(entitySetter, entityGetter, networkViewModel, shouldConsiderDescendants)
		{
		}

		protected override double DefaultValue
		{
			get { return networkViewModel.Network.EntityPositionStrategy.DefaultHeight; }
		}

		protected override double GetLowerBound(NodeViewModel node, IEnumerable<INetworkEntity> childEntities)
		{
			return considerDescendants && childEntities.Any()
				? childEntities.Max(e => e.Height + e.Y) - node.Entity.Y + networkViewModel.Network.EntityPositionStrategy.BottomMargin
				: NodeViewModel.MinAllowedNodeHeight;
		}

		protected override Range<double> GetHighRange(NodeViewModel node, INetworkEntity parentLayout)
		{
			return GetAcceptableHighYRange(node.Entity, parentLayout);
		}

		protected override double GetOffSet(INetworkEntity networkEntity)
		{
			return networkEntity.Y;
		}

		protected override Range<double> GetRangeForFixedDiagramSurface(double lowerBound, INetworkEntity parentLayout, double offset)
		{
			return networkViewModel.Network.DiagramEntity.IsDiagramSurfaceFixed && parentLayout.Height > 0d
				? new Range<double>(lowerBound, parentLayout.Height - offset)
				: new Range<double>(lowerBound, double.MaxValue);
		}
	}
}
