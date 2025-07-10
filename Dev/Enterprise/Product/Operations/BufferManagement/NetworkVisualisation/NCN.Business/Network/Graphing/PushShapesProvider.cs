using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public enum PushDirection
	{
		None,
		Early,
		Late
	}

	public static class PushShapesProvider
	{
		public static void PushAllEntities(IJobNetwork network, PushDirection direction)
		{
			network.RefreshSchedules();

			PushAllEntities(network.DiagramShape.Scale, network.Entities.ShapeEntities, direction);
		}

		static void PushAllEntities(ZDateTime scale, IEnumerable<ShapeNetworkEntity> shapes, PushDirection direction)
		{
			var list = new DisposableList(shapes.SelectMany(s => new[] { s.DelayPropertyChanged(), s.Shape.DelayPropertyChanged() }));
			try
			{
				foreach (var shape in shapes.Where(s => s.Shape.ScheduleBizo != null).OrderBy(s => s.GetOwnersUpHierarchy().Count()))
				{
					if (direction == PushDirection.Early)
					{
						shape.X = ShapeOffsetToDateConverter.GetSizeForMinutes(scale, shape.Shape.ScheduleBizo.BNC_EarliestStartOffsetMinutes);
					}
					else if (direction == PushDirection.Late)
					{
						shape.X = ShapeOffsetToDateConverter.GetSizeForMinutes(scale, shape.Shape.ScheduleBizo.BNC_LatestStartOffsetMinutes);
					}
					else
					{
						throw new InvalidOperationException("Entities can only be pushed early or late.");
					}
				}
			}
			finally
			{
				list.Dispose();
			}
		}
	}
}
