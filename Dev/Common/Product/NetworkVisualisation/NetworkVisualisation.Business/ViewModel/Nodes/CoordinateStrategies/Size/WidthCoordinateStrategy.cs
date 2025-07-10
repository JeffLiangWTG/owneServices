using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	class WidthCoordinateStrategy : SpanPropertyStrategy
	{
		internal WidthCoordinateStrategy(Action<INetworkEntity, double> entitySetter, Func<INetworkEntity, double> entityGetter, NetworkViewModel networkViewModel, bool shouldConsiderDescendants = true)
			: base(entitySetter, entityGetter, networkViewModel, shouldConsiderDescendants)
		{
		}

		protected override double DefaultValue
		{
			get { return networkViewModel.Network.EntityPositionStrategy.DefaultWidth; }
		}

		protected override double GetLowerBound(NodeViewModel node, IEnumerable<INetworkEntity> childEntities)
		{
			return considerDescendants && childEntities.Any()
				? childEntities.Max(n => n.Width + n.X) - node.Entity.X + networkViewModel.Network.EntityPositionStrategy.RightMargin
				: NodeViewModel.MinAllowedNodeWidth;
		}

		protected override Range<double> GetHighRange(NodeViewModel node, INetworkEntity parentLayout)
		{
			return GetAcceptableHighXRange(node.Entity, parentLayout);
		}

		protected override double GetOffSet(INetworkEntity networkEntity)
		{
			return networkEntity.X;
		}

		protected override Range<double> GetRangeForFixedDiagramSurface(double lowerBound, INetworkEntity parentLayout, double offset)
		{
			return networkViewModel.Network.DiagramEntity.IsDiagramSurfaceFixed && parentLayout.Width > 0d
				? new Range<double>(lowerBound, parentLayout.Width - offset)
				: new Range<double>(lowerBound, double.MaxValue);
		}
	}
}
