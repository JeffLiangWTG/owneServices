using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ScheduleNodeSnapshot
	{
		public ScheduleNodeSnapshot(IJobNetwork network)
		{
			factory = network.DiagramShape.Factory;

			ScheduleNetwork = ShapeScheduleGenerator.CreateScheduleNetwork(network);
			RootSchedule = ScheduleNetwork.SchedulesByEntity[network.DiagramShape.PK];

			scheduledStartTimeUtc = network.DiagramShape.ScheduledStartTimeUtc;
			scheduledFinishTimeUtc = network.DiagramShape.ScheduledFinishTimeUtc;
			scale = network.DiagramShape.Scale;
			diagramDurationInMinutes = network.DiagramShape.ExplicitDurationMinutes;
		}

		readonly BusinessObjectFactory factory;
		readonly ZDateTime scale;
		readonly ZDateTime scheduledStartTimeUtc;
		readonly ZDateTime scheduledFinishTimeUtc;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanForDuration", Justification = "Baseline")]
		readonly int diagramDurationInMinutes;

		public ScheduleGraph ScheduleNetwork { get; }
		public ScheduleNode RootSchedule { get; }

		public void Calculate()
		{
			new ShapeFloatCalculator(scheduledStartTimeUtc, scheduledFinishTimeUtc, scale, diagramDurationInMinutes, RootSchedule, ScheduleNetwork, factory).Calculate();
		}
	}
}
