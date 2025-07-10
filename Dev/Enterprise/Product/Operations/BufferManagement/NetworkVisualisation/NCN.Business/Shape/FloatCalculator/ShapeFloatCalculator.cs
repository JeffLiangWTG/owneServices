using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public sealed class ShapeFloatCalculator : FloatCalculatorBase
	{
		public static void CalculateFloatForChildren(IJobNetwork network)
		{
			var scheduleNetwork = ShapeScheduleGenerator.CreateScheduleNetwork(network);
			var rootShape = network.DiagramShape;
			var startTime = rootShape.ScheduledStartTimeUtc;
			var finishTime = rootShape.ScheduledFinishTimeUtc;
			var rootNode = scheduleNetwork.SchedulesByEntity[rootShape.PK];

			var calculator = new ShapeFloatCalculator(startTime, finishTime, rootShape.Scale, rootShape.ExplicitDurationMinutes, rootNode, scheduleNetwork, rootShape.Factory);

			calculator.Calculate();

			AddCalculatedScheduleNodesToEntities(network, scheduleNetwork);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		internal ShapeFloatCalculator(ZDateTime scheduledStartTimeUtc, ZDateTime scheduledFinishTimeUtc, ZDateTime scale, int diagramDurationInMinutes, ScheduleNode root, ScheduleGraph scheduleNetwork, BusinessObjectFactory factory)
			: base(root, scheduleNetwork, WorkingTimeContext.Create((IBranchDepartmentProvider)root.Entity, null, factory), factory)
		{
			this.scale = scale;
			this.scheduledStartTimeUtc = scheduledStartTimeUtc;
			this.scheduledFinishTimeUtc = scheduledFinishTimeUtc;
			this.diagramDurationInMinutes = diagramDurationInMinutes;
		}

		#region Setup

		internal static void AddCalculatedScheduleNodesToEntities(IJobNetwork jobNetwork, ScheduleGraph scheduleGraph)
		{
			var scheduleScope = scheduleGraph.SchedulesByEntity;

			foreach (var entity in jobNetwork.Entities.ShapeEntities)
			{
				entity.Schedule = scheduleScope.ContainsKey(entity.PK) ? scheduleScope[entity.PK] : scheduleGraph.AddSchedule(entity, entity.Shape.ExplicitDurationHours);
			}
		}

		#endregion

		#region Offset to Time

		readonly ZDateTime scale;
		readonly ZDateTime scheduledStartTimeUtc;
		readonly ZDateTime scheduledFinishTimeUtc;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		readonly int diagramDurationInMinutes;

		protected override OffsetToDateConverter CreateOffsetConverter()
		{
			return new ShapeOffsetToDateConverter(scheduledStartTimeUtc, scheduledFinishTimeUtc, scale, diagramDurationInMinutes, Context, Factory);
		}

		#endregion
	}
}
