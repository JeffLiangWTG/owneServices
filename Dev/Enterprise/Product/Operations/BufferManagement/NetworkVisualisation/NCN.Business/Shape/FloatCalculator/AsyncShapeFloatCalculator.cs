using System.Linq;
using System.Threading;
using CargoWise.Async;
using CargoWise.Common;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class AsyncShapeFloatCalculator : ValidatedAsyncOperator<IJobNetwork, ScheduleNodeSnapshot>
	{
		protected override ScheduleNodeSnapshot GetSnapshot(IJobNetwork source)
		{
			return new ScheduleNodeSnapshot(source);
		}

		protected override ScheduleNodeSnapshot TransformSnapshot(ScheduleNodeSnapshot snapshot, CancellationTokenSource cancellationTokenSource = null)
		{
			snapshot.Calculate();

			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Token.ThrowIfCancellationRequested();
			}

			return snapshot; // Mutating this in place is ok, because none of the original data changes
		}

		protected override bool AssumptionsOfInitialSnapShotValid(IJobNetwork network, ScheduleNodeSnapshot initialSnapshot)
		{
			var schedulableShapes = network.Entities.ShapeEntities.Where(ShapeScheduleGenerator.IsSchedulable).Append(network.DiagramEntity).ToArray();
			if (schedulableShapes.Length != initialSnapshot.ScheduleNetwork.SchedulesByEntity.Count)
			{
				return false;
			}
			else
			{
				foreach (var shape in schedulableShapes)
				{
					if (!ShapeMatchesInitialNode(initialSnapshot, shape))
					{
						return false;
					}
				}

				return true;
			}
		}

		static bool ShapeMatchesInitialNode(ScheduleNodeSnapshot initialSnapshot, ShapeNetworkEntity shape)
		{
			ScheduleNode node;
			if (initialSnapshot.ScheduleNetwork.SchedulesByEntity.TryGetValue(shape.PK, out node))
			{
				return shape.IsRoot || DurationsMatch(shape, node);
			}

			return false;
		}

		static bool DurationsMatch(ShapeNetworkEntity shape, ScheduleNode node)
		{
			return node.EstimatedDurationHoursIncludingChildren == shape.Shape.ExplicitDurationHours;
		}

		protected override void MapSnapshot(IJobNetwork network, ScheduleNodeSnapshot snapshot)
		{
			ShapeFloatCalculator.AddCalculatedScheduleNodesToEntities(network, snapshot.ScheduleNetwork);
		}
	}
}
