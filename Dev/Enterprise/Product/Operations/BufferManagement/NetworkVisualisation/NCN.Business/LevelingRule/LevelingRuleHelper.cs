using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public static class LevelingRuleHelper
	{
		static bool AppliesToChannels(BMNCNShape shape, IEnumerable<ZGuid> applicableChannelPKs)
		{
			var hasApplicableChannels = !applicableChannelPKs.Any();

			if (!hasApplicableChannels)
			{
				var diagram = (shape.FindTopmostDiagramShape() as BMNCNRootDiagramShape);
				if (diagram != null)
				{
					var diagramChannelPKs = diagram.Channels.Where(c => c.Shapes.Contains(shape)).Select(s => s.PK);
					hasApplicableChannels = applicableChannelPKs.Intersect(diagramChannelPKs).Any();
				}
			}

			return hasApplicableChannels;
		}

		static bool ShouldIncludeShape(BMNCNShape shape, IEnumerable<ZGuid> applicableChannelPKs)
		{
			return !shape.IsNonScheduled
				&& !((IBufferedItem)shape).IsClosed
				&& !shape.IsBufferShape
				&& !shape.IsAnnotation
				&& shape.ParentShape.PK == shape.RootShapePK
				&& AppliesToChannels(shape, applicableChannelPKs);
		}

		static ShapeNetworkEntity[] GetIntersectingEntities(int index, IEnumerable<ShapeNetworkEntity> entities, BMNCNShape diagramShape)
		{
			var cellRectangle = new Rectangle(index * diagramShape.ScaleUnitPixelSize, 1, diagramShape.ScaleUnitPixelSize, 1);
			return entities.Where(e => e.GetScaleRectangle().IntersectsWith(cellRectangle)).ToArray();
		}

		static ShapeNetworkEntity[] EntitiesThatAreAffected(int index, int value, bool considerStartOfEntity, ShapeNetworkEntity[] allEntities, IEnumerable<ShapeNetworkEntity> intersectingEntities, BMNCNShape diagramShape)
		{
			var result = new List<ShapeNetworkEntity>();
			var max = intersectingEntities.Max(e => e.X);
			var entitiesToCompare = allEntities.Where(e => e.X == max).ToArray();
			var entityToCompare = entitiesToCompare.FirstOrDefault();

			if (entityToCompare != null)
			{
				var affectedEntities =
					(from entity in allEntities
					 let shape = entity.AsShape()
					 let entityStartOrEnd = considerStartOfEntity ? entity.X : entity.X + entity.Width
					 where !ReferenceEquals(entity, entityToCompare)
					 where entityStartOrEnd >= (index - value + 1) * diagramShape.ScaleUnitPixelSize && entityStartOrEnd <= index * diagramShape.ScaleUnitPixelSize
					 where entityToCompare.X >= entityStartOrEnd
					 select entity).ToArray();

				if (affectedEntities.Any())
				{
					if (considerStartOfEntity)
					{
						result.AddRange(affectedEntities.Where(e => e.X == entityToCompare.X));
						result.Add(entityToCompare);
					}
					else
					{
						result.AddRange(entitiesToCompare);
					}
				}
			}

			return result.ToArray();
		}

		public static ShapeNetworkEntity[] GetEntitiesThatViolateConcurrencyLevelingRule(int index, int value, IEnumerable<ShapeNetworkEntity> entities, BMNCNShape diagramShape, IEnumerable<ZGuid> applicableChannelPKs)
		{
			var result = new List<ShapeNetworkEntity>();
			var cellRectangle = new Rectangle(index * diagramShape.ScaleUnitPixelSize, 1, diagramShape.ScaleUnitPixelSize, 1);

			var shapesInsideCell =
				(from entity in entities
				 let shape = entity.AsShape()
				 where ShouldIncludeShape(shape, applicableChannelPKs)
				 where entity.GetScaleRectangle().IntersectsWith(cellRectangle)
				 select entity).ToArray();

			if (shapesInsideCell.Length > value)
			{
				result.AddRange(shapesInsideCell);
			}

			return result.ToArray();
		}

		public static ShapeNetworkEntity[] GetEntitiesThatViolateStartLevelingRule(int index, int value, IEnumerable<ShapeNetworkEntity> entities, BMNCNShape diagramShape, IEnumerable<ZGuid> applicableChannelPKs)
		{
			var result = new List<ShapeNetworkEntity>();
			var entitiesToConsider = entities.Where(e => ShouldIncludeShape(e.Shape, applicableChannelPKs)).ToArray();
			var intersectingEntities = GetIntersectingEntities(index, entitiesToConsider, diagramShape);

			if (intersectingEntities.Any())
			{
				result.AddRange(EntitiesThatAreAffected(index, value, true, entitiesToConsider, intersectingEntities, diagramShape));
			}

			return result.ToArray();
		}

		public static ShapeNetworkEntity[] GetEntitiesThatViolateGapLevelingRule(int index, int value, IEnumerable<ShapeNetworkEntity> entities, BMNCNShape diagramShape, IEnumerable<ZGuid> applicableChannelPKs)
		{
			var result = new List<ShapeNetworkEntity>();
			var entitiesToConsider = entities.Where(e => ShouldIncludeShape(e.Shape, applicableChannelPKs)).ToArray();
			var intersectingEntities = GetIntersectingEntities(index, entitiesToConsider, diagramShape);

			if (intersectingEntities.Any())
			{
				result.AddRange(EntitiesThatAreAffected(index, value, false, entitiesToConsider, intersectingEntities, diagramShape));
			}

			return result.ToArray();
		}
	}
}
