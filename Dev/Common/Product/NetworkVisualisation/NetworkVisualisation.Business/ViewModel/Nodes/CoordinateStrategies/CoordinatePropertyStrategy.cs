using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	/// <summary>
	/// The purpose of this class is to implement common rules that restrict updates of Coordinate properties. (E.G. X, Y, Width, Height, Delta)
	/// </summary>
	abstract class CoordinatePropertyStrategy : CoordinatePropertyStrategyBase
	{
		internal CoordinatePropertyStrategy(Action<INetworkEntity, double> entitySetter, Func<INetworkEntity, double> entityGetter, NetworkViewModel networkViewModel, bool shouldConsiderDescendants)
		{
			this.networkViewModel = networkViewModel;
			this.entitySetter = entitySetter;
			this.entityGetter = entityGetter;
			considerDescendants = shouldConsiderDescendants;
		}

		protected readonly bool considerDescendants;
		protected readonly NetworkViewModel networkViewModel;
		protected readonly Action<INetworkEntity, double> entitySetter;
		protected readonly Func<INetworkEntity, double> entityGetter;

		protected override bool SetPropertyCore(double newVal, NodeViewModel node, INetworkEntity entity)
		{
			if (!entity.HasState(EntityState.Fixed))
			{
				var currentValue = entityGetter(entity);
				var range = GetValidRange(node);

				if (newVal < range.Minimum)
				{
					newVal = range.Minimum;
				}
				else if (newVal > range.Maximum)
				{
					newVal = range.Maximum;
				}

				if (currentValue != newVal)
				{
					entitySetter(entity, newVal);
					if (considerDescendants)
					{
						var descendants = entity.GetDescendants();
						var nodesToUpdate = networkViewModel.Nodes.Where(n => descendants.Any(d => d.Equals(n.Entity)));

						foreach (var nodeToUpdate in nodesToUpdate)
						{
							node.UpdateChildNodes(child => UpdateChildNodes(nodeToUpdate, currentValue, newVal));
						}
					}

					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		internal void RefreshProperty(NodeViewModel node, INetworkEntity entity)
		{
			SetProperty(entityGetter(entity), node, entity);
		}

		protected abstract void UpdateChildNodes(NodeViewModel child, double originalValue, double updatedValue);

		protected virtual Range<double> GetValidRange(NodeViewModel no)
		{
			return new Range<double>(double.MinValue, double.MaxValue);
		}

		protected virtual double DefaultValue
		{
			get { return default(double); }
		}

		#region Range Helpers

		protected Range<double> GetValidRange(Func<INetworkEntity, INetworkEntity, Range<double>> acceptableLowRangeGetter, Func<INetworkEntity, INetworkEntity, Range<double>> acceptableHighRangeGetter, double size, INetworkEntity entity)
		{
			var parentLayout = entity.Parent as INetworkEntity;

			if (IsParentLayoutDiagramEntity(parentLayout) && !networkViewModel.Network.DiagramEntity.IsDiagramSurfaceFixed)
			{
				parentLayout = null;
			}

			var acceptableLowRange = acceptableLowRangeGetter(entity, parentLayout);
			var acceptableHighRange = acceptableHighRangeGetter(entity, parentLayout);
			var lowestVal = Math.Max(acceptableLowRange.Minimum, acceptableHighRange.Minimum - size);
			var highestVal = Math.Min(acceptableLowRange.Maximum, acceptableHighRange.Maximum - size);

			return new Range<double>(lowestVal, highestVal);
		}

		bool IsParentLayoutDiagramEntity(INetworkEntity parentLayout)
		{
			return parentLayout != null && parentLayout.IsSameEntity(networkViewModel.Network.DiagramEntity);
		}

		internal Range<double> GetAcceptableLowXRange(INetworkEntity entity, INetworkEntity parentLayout)
		{
			var leftMargin = networkViewModel.Network.EntityPositionStrategy.LeftMargin;
			var parentLowerBound = parentLayout != null ? parentLayout.X + leftMargin : 0d;
			var fixedChildBound = GetFixedChildBound((e, depth) => e.X + depth * leftMargin, entity, upperBound: false) ?? double.MaxValue;

			return new Range<double>(parentLowerBound, fixedChildBound);
		}

		internal Range<double> GetAcceptableHighXRange(INetworkEntity entity, INetworkEntity parentLayout)
		{
			var rightMargin = networkViewModel.Network.EntityPositionStrategy.RightMargin;
			var fixedChildBound = GetFixedChildBound((e, depth) => e.X + e.Width - depth * rightMargin, entity, upperBound: true) ?? 0d;
			var parentUpperBound = parentLayout != null && parentLayout.Width > 0d ? parentLayout.X + parentLayout.Width - rightMargin : double.MaxValue;

			return new Range<double>(fixedChildBound, parentUpperBound);
		}

		internal Range<double> GetAcceptableLowYRange(INetworkEntity entity, INetworkEntity parentLayout)
		{
			var topMargin = networkViewModel.Network.EntityPositionStrategy.TopMargin;
			var parentLowerBound = parentLayout != null ? parentLayout.Y + topMargin : 0d;
			var fixedChildBound = GetFixedChildBound((e, depth) => e.Y - depth * topMargin, entity, upperBound: false) ?? double.MaxValue;

			return new Range<double>(parentLowerBound, fixedChildBound);
		}

		internal Range<double> GetAcceptableHighYRange(INetworkEntity entity, INetworkEntity parentLayout)
		{
			var bottomMargin = networkViewModel.Network.EntityPositionStrategy.BottomMargin;
			var fixedChildBound = GetFixedChildBound((e, depth) => e.Y + e.Height + depth * bottomMargin, entity, upperBound: true) ?? 0d;
			var parentUpperBound = parentLayout != null && parentLayout.Height > 0d ? parentLayout.Y + parentLayout.Height - bottomMargin : double.MaxValue;

			return new Range<double>(fixedChildBound, parentUpperBound);
		}

		double? GetFixedChildBound(Func<INetworkEntity, int, double> boundsGetter, INetworkEntity entity, bool upperBound)
		{
			Tuple<INetworkEntity, int> fixedChild = null;

			if (considerDescendants)
			{
				var nodes = GetFixedDescendentNodes(entity).ToArray();

				if (upperBound)
				{
					fixedChild = nodes.OrderByDescending(e => boundsGetter(e.Item1, e.Item2)).FirstOrDefault();
				}
				else
				{
					fixedChild = nodes.OrderBy(e => boundsGetter(e.Item1, e.Item2)).FirstOrDefault();
				}
			}

			return fixedChild != null ? boundsGetter(fixedChild.Item1, fixedChild.Item2) : new double?();
		}

		static IEnumerable<Tuple<INetworkEntity, int>> GetFixedDescendentNodes(INetworkEntity startingEntity)
		{
			return startingEntity.GetDescendantsWithDepth(entity => entity.HasState(EntityState.Fixed));
		}

		#endregion
	}
}
