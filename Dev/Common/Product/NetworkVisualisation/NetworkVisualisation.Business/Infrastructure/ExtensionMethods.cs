using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;

namespace CargoWise.NetworkVisualisation.Business
{
	public static class ExtensionMethods
	{
		#region String Transforms

		public static string ShortenLineLengths(this string content, int maxLineLength)
		{
			if (string.IsNullOrEmpty(content))
			{
				return content;
			}

			var result = new StringBuilder();
			var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				if (line.Length > maxLineLength)
				{
					if (maxLineLength <= 3)
					{
						result.AppendLine(line.Substring(0, maxLineLength));
					}
					else
					{
						result.AppendLine(line.Substring(0, maxLineLength - 3) + "...");
					}
				}
				else
				{
					result.AppendLine(line);
				}
			}

			return result.ToString().Trim('\r', '\n');
		}

		public static string GetDisplayName(this IEntityRelationship relationship)
		{
			var from = relationship.From;
			var to = relationship.To;

			return from != null && to != null ? string.Format(CultureInfo.InvariantCulture, "{0} -> {1}", from.Name, to.Name) : string.Empty;
		}

		#endregion

		#region Actions / State

		public static bool Supports(this INetworkEntity entity, NetworkActions action)
		{
			return (entity.SupportedActions & action) == action;
		}

		public static bool HasState(this INetworkEntity entity, EntityState state)
		{
			return entity.EntityState.HasFlag(state);
		}

		#endregion

		#region Scale

		public static double GetScaledPixelIncrements(this IDiagramEntity diagramEntity)
		{
			var resolutionIncrementFactor = diagramEntity.ResolutionIncrement / (double)diagramEntity.Scale;
			var scaleSizeInPixels = diagramEntity.ScaleUnitPixelSize;

			return scaleSizeInPixels * resolutionIncrementFactor;
		}

		[SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "We're not in ZArchitecture land")]
		public static double GetScaleWidth(this INetworkEntity entity, IDiagramEntity scaledDiagram)
		{
			var pixelIncrements = scaledDiagram.GetScaledPixelIncrements();

			var result = Math.Round(entity.Width / pixelIncrements) * pixelIncrements;
			if (scaledDiagram.IsDiagramSurfaceFixed && entity.X + result > scaledDiagram.Width)
			{
				return Math.Floor(entity.Width / pixelIncrements) * pixelIncrements;
			}
			else
			{
				return result;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "We're not in ZArchitecture anymore")]
		public static bool AdjustLocationToScale(this INetworkEntity entity, IDiagramEntity scaledDiagram)
		{
			if (!entity.IsNonScheduled && CheckIsScalable(entity, scaledDiagram))
			{
				var pixelIncrements = scaledDiagram.GetScaledPixelIncrements();
				entity.X = Math.Round(entity.X / pixelIncrements) * pixelIncrements;

				return true;
			}

			return false;
		}

		public static bool AdjustLocationToChannels(this INetworkEntity entity, IDiagramEntity diagram, NetworkViewModel networkViewModel)
		{
			if (diagram.DiagramChannels.Any())
			{
				var yAxisRanges = GetYAxisRangesSpanningEntity(entity, networkViewModel);
				var height = entity.Height;
				var y = entity.Y;

				entity.Y = GetNewEntityLocationWithinChannelBounds(entity, yAxisRanges);
				entity.Height = GetHeightAdjustedForChannel(entity, yAxisRanges, isAdjustingLocation: true);

				return !(y == entity.Y && height == entity.Height);
			}

			return false;
		}

		static double GetNewEntityLocationWithinChannelBounds(INetworkEntity entity, IEnumerable<Range<int>> yAxisRanges)
		{
			if (yAxisRanges.Count() >= 2)
			{
				if (EntityMostlySpansFirstChannel(entity, yAxisRanges))
				{
					var range = yAxisRanges.First();
					var rangeHeight = range.Maximum - range.Minimum;
					var diff = entity.Height > rangeHeight ? rangeHeight : entity.Height;
					var top = range.Maximum - diff;

					return top < 0.0d ? 0.0d : top;
				}
				else
				{
					return yAxisRanges.Last().Minimum;
				}
			}

			return entity.Y;
		}

		static bool EntityMostlySpansFirstChannel(INetworkEntity entity, IEnumerable<Range<int>> yAxisRanges)
		{
			return (yAxisRanges.First().Maximum - entity.Y) >= ((entity.Y + entity.Height) - yAxisRanges.Last().Minimum);
		}

		public static double GetHeightAdjustedForChannel(this INetworkEntity entity, IDiagramEntity scaledDiagram, NetworkViewModel networkViewModel)
		{
			if (scaledDiagram.DiagramChannels.Any())
			{
				var yAxisRanges = GetYAxisRangesSpanningEntity(entity, networkViewModel);

				return GetHeightAdjustedForChannel(entity, yAxisRanges, isAdjustingLocation: false);
			}

			return entity.Height;
		}

		static bool EntityExceedsChannelHeight(INetworkEntity entity, Range<int> range)
		{
			return range.Maximum - range.Minimum < entity.Height;
		}

		static double GetHeightAdjustedForChannel(INetworkEntity entity, IEnumerable<Range<int>> yAxisRanges, bool isAdjustingLocation)
		{
			var range = yAxisRanges.FirstOrDefault();

			if (range != default)
			{
				var isInNonChanneledRegion = range.Minimum == 0 && range.Maximum == 0;

				if (isAdjustingLocation && !isInNonChanneledRegion && EntityExceedsChannelHeight(entity, range))
				{
					return range.Maximum - range.Minimum;
				}
			}

			if (yAxisRanges.Count() > (isAdjustingLocation ? 2 : 1))
			{
				return range.Maximum - entity.Y;
			}

			return entity.Height;
		}

		static IEnumerable<Range<int>> GetYAxisRangesSpanningEntity(INetworkEntity entity, NetworkViewModel networkViewModel)
		{
			var y = networkViewModel.GetChannelYAxisRanges();

			return y.Select(r => r.Item2).Where(r => entity.Y + entity.Height > r.Minimum).Where(r => entity.Y < r.Maximum);
		}

		[SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "We're not in ZArchitecture anymore")]
		public static bool AdjustEntityLocationToDiagramBounds(this INetworkEntity entity, IDiagramEntity scaledDiagram, bool useOffset = false)
		{
			if (scaledDiagram.IsDiagramScaled && scaledDiagram.Scale > 0 && scaledDiagram.Width > entity.Width)
			{
				var pixelIncrements = scaledDiagram.GetScaledPixelIncrements();

				if (useOffset)
				{
					entity.X = Math.Round(entity.X / pixelIncrements) * pixelIncrements;
				}
				else
				{
					var entityX = (scaledDiagram.X + scaledDiagram.Width) - entity.Width;
					entity.X = Math.Floor(entityX / pixelIncrements) * pixelIncrements;
				}
				return true;
			}
			return false;
		}

		public static void RoundUpToResolutionIncrement(this INetworkEntity entity, IDiagramEntity diagram)
		{
			var pixelsPerResolutionIncrement = diagram.GetScaledPixelIncrements();
			var excessPixels = entity.Width % pixelsPerResolutionIncrement;

			if (excessPixels > 0)
			{
				entity.Width += pixelsPerResolutionIncrement - excessPixels;
			}
		}

		public static bool IsScalable(this string shapeType)
		{
			switch (shapeType)
			{
				case ShapeTypes.Annotation:
					return false;

				default:
					return true;
			}
		}

		#endregion

		#region Size / Location

		public static void LocateAfterShape(this INetworkEntity entity, INetworkEntity otherEntity)
		{
			entity.X = otherEntity.X + otherEntity.Width;
			entity.Y = ((otherEntity.Height - entity.Height) / 2) + otherEntity.Y;
		}

		public static Location GetLocation(this INetworkEntity entity)
		{
			return new Location(entity.X, entity.Y);
		}

		#endregion

		#region Notifications

		public static bool HasErrors(this INetworkEntity entity)
		{
			return HasNotificationsOfType(entity, EntityNotifcationType.Error);
		}

		public static bool HasWarnings(this INetworkEntity entity)
		{
			return HasNotificationsOfType(entity, EntityNotifcationType.Warning);
		}

		public static bool HasMessages(this INetworkEntity entity)
		{
			return HasNotificationsOfType(entity, EntityNotifcationType.Message);
		}

		static bool HasNotificationsOfType(INetworkEntity entity, EntityNotifcationType notificationType)
		{
			return entity.EntityNotifications.Any(en => en.NotificationType == notificationType);
		}

		#endregion

		#region IProposedNetworkEntity

		/// <summary>
		/// Calculates prerequisite depth for a given entity.
		/// Returns -1 if the entity is involved into a circular dependency.
		/// </summary>
		public static int GetPreRequisiteDepth(this IProposedNetworkEntity entity, Dictionary<IProposedNetworkEntity, int> depthCache = null) => GetPreRequisiteDepthCore(entity, entity, depthCache ?? new Dictionary<IProposedNetworkEntity, int>());

		static int GetPreRequisiteDepthCore(IProposedNetworkEntity currentEntity, IProposedNetworkEntity initialEntity, Dictionary<IProposedNetworkEntity, int> depthCache)
		{
			var prereqDepths = currentEntity.PreRequisiteLinks.Select(link => link.From == initialEntity ? -1 : depthCache.GetFromCacheOrCalculateDepth(link.From, initialEntity)).ToArray(); // -1 means circular dependency - infinite loop
			return !prereqDepths.Any() ? 0 :
				prereqDepths.Any(depth => depth == -1) ? -1 : prereqDepths.Max() + 1;
		}

		static int GetFromCacheOrCalculateDepth(this Dictionary<IProposedNetworkEntity, int> depthCache, IProposedNetworkEntity currentEntity, IProposedNetworkEntity initialEntity)
		{
			if (!depthCache.TryGetValue(currentEntity, out int depth))
			{
				depth = GetPreRequisiteDepthCore(currentEntity, initialEntity, depthCache);
				depthCache.Add(currentEntity, depth);
			}
			return depth;
		}

		public static bool IsRoot(this IProposedNetworkEntity entity)
		{
			return entity.Parent == null;
		}

		#endregion

		#region INetworkEntity

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IEnumerable<Tuple<INetworkEntity, int>> GetDescendantsWithDepth(this INetworkEntity entity, Predicate<INetworkEntity> filter)
		{
			return entity.FilteredTraversalWithDepth(e => e.Children, filter, new[] { entity });
		}

		static IEnumerable<Tuple<T, int>> FilteredTraversalWithDepth<T>(this T item, Func<T, IEnumerable<T>> selector, Predicate<T> filter, IEnumerable<T> previous)
		{
			foreach (var child in selector(item))
			{
				if (filter(child))
				{
					yield return Tuple.Create(child, previous.Count());
				}

				foreach (var descendant in FilteredTraversalWithDepth(child, selector, filter, previous.Concat(new[] { child })))
				{
					yield return descendant;
				}
			}
		}

		public static IEnumerable<INetworkEntity> GetDescendants(this INetworkEntity entity)
		{
			foreach (var child in entity.Children)
			{
				yield return child;
				foreach (var grandchild in child.GetDescendants())
				{
					yield return grandchild;
				}
			}
		}

		public static IEnumerable<INetworkEntity> GetAncestors(this INetworkEntity entity)
		{
			var parent = entity.Parent as INetworkEntity;
			if (parent != null)
			{
				yield return parent;
				foreach (var ancestor in parent.GetAncestors())
				{
					yield return ancestor;
				}
			}
		}

		public static IDiagramChannel GetMemberChannel(this INetworkEntity entity, NetworkViewModel viewModel)
		{
			var entityTopOffset = entity.Y;
			var result = default(IDiagramChannel);

			foreach (var tuple in viewModel.GetChannelYAxisRanges())
			{
				result = tuple.Item1;

				if (tuple.Item2.Maximum > entityTopOffset)
				{
					break;
				}
			}

			return result;
		}

		#endregion

		#region WorkStatus

		public static bool IsClosed(this WorkStatus status)
		{
			return status == WorkStatus.Cancelled || status == WorkStatus.Complete;
		}

		#endregion

		#region Utilities

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static Tuple<IEnumerable<T>, IEnumerable<T>> Split<T>(this IEnumerable<T> collection, Func<T, bool> comparisonOperator)
		{
			var truthy = new List<T>();
			var falsy = new List<T>();

			foreach (var element in collection)
			{
				if (comparisonOperator(element))
				{
					truthy.Add(element);
				}
				else
				{
					falsy.Add(element);
				}
			}

			return Tuple.Create<IEnumerable<T>, IEnumerable<T>>(truthy, falsy);
		}

		public static bool CheckIsScalable(this INetworkEntity entity, IDiagramEntity scaledDiagram)
		{
			return entity.ShapeType.IsScalable() && scaledDiagram.IsDiagramScaled && scaledDiagram.Scale > 0;
		}

		#endregion
	}
}
