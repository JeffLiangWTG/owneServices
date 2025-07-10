using System.Linq;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public static class BoundsFitting
	{
		public static void FitToParent(this NodeViewModel node)
		{
			var entity = node.Entity;

			if (entity != null)
			{
				var parent = node.GetParent();
				var parentEntity = parent != null ? parent.Entity : null;
				if (parentEntity != null)
				{
					var parentFixed = parentEntity.HasState(EntityState.Fixed);
					var nodeFixed = entity.HasState(EntityState.Fixed);
					if (parentFixed && nodeFixed)
					{
						// Do nothing. Fixed shapes don't change anyway.
					}
					else
					{
						var fixedNode = parent.TryGrowToFit(node);
						if (fixedNode != null)
						{
							fixedNode.UpdateChildNodes(n => n.TryShrinkToFit(fixedNode));
						}
					}
				}
			}
		}

		internal static bool TryShrinkToFit(this NodeViewModel node, NodeViewModel parentNode, double? widthOverride = null, double? heightOverride = null)
		{
			var positionStrategy = node.Network.EntityPositionStrategy;
			var maxWidth = (widthOverride ?? parentNode.Width) - positionStrategy.RightMargin - positionStrategy.LeftMargin;
			var maxHeight = (heightOverride ?? parentNode.Height) - positionStrategy.TopMargin - positionStrategy.BottomMargin;
			var widthNeedsShrinking = node.Width > maxWidth + 1; // Adding 1 to get around pixel rounding.
			var heightNeedsShrinking = node.Height > maxHeight + 1; // Adding 1 to get around pixel rounding.

			if (widthNeedsShrinking || heightNeedsShrinking)
			{
				if (node.Entity.HasState(EntityState.Fixed) || maxWidth < 0 || maxHeight < 0)
				{
					return false;
				}
				else if (!node.ChildNodes.All(child => child.TryShrinkToFit(node, maxWidth, maxHeight)))
				{
					return false;
				}
			}

			if (widthNeedsShrinking)
			{
				node.SetX(parentNode.X + positionStrategy.LeftMargin);
				node.Width = maxWidth;
			}

			if (heightNeedsShrinking)
			{
				node.SetY(parentNode.Y + positionStrategy.TopMargin);
				node.Height = maxHeight;
			}

			return true;
		}

		internal static NodeViewModel TryGrowToFit(this NodeViewModel node, NodeViewModel childNode, double? widthOverride = null, double? heightOverride = null)
		{
			NodeViewModel result = null;
			var positionStrategy = node.Network.EntityPositionStrategy;
			var minWidth = (widthOverride ?? childNode.Width) + positionStrategy.LeftMargin + positionStrategy.RightMargin;
			var minHeight = (heightOverride ?? childNode.Height) + positionStrategy.TopMargin + positionStrategy.BottomMargin;
			var changeWidth = node.Width < minWidth + 1; // Adding 1 to get around pixel rounding.
			var changeHeight = node.Height < minHeight + 1; // Adding 1 to get around pixel rounding.

			if (changeWidth || changeHeight)
			{
				if (node.Entity.HasState(EntityState.Fixed))
				{
					result = node;
				}
				else
				{
					var parent = node.GetParent();
					if (parent != null)
					{
						result = parent.TryGrowToFit(node, minWidth, minHeight);
					}
				}
			}

			if (changeWidth)
			{
				var childXOffset = childNode.X - node.X;
				var xDiff = minWidth - node.Width;
				node.SetX(node.X - xDiff);
				node.Width = minWidth;
				childNode.SetX(node.X + childXOffset, true);
			}

			if (changeHeight)
			{
				var childYOffset = childNode.Y - node.Y;
				var yDiff = minHeight - node.Height;
				node.SetY(node.Y - yDiff);
				node.Height = minHeight;
				childNode.SetY(node.Y + childYOffset, true);
			}

			return result;
		}

		internal static bool TryMoveToFit(this NodeViewModel node)
		{
			node.SetX(node.X);
			node.SetY(node.Y);

			var widthIsConstrained = false;
			var heightIsConstrained = false;

			var entity = node.Entity;
			new WidthCoordinateStrategy((e, v) => widthIsConstrained = e.Width != v, e => e.Width, node.NetworkViewModel, false).SetProperty(node.Width, node, entity);
			new HeightCoordinateStrategy((e, v) => heightIsConstrained = e.Height != v, e => e.Height, node.NetworkViewModel, false).SetProperty(node.Height, node, entity);

			return !widthIsConstrained && !heightIsConstrained;
		}
	}
}
