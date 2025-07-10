using System;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.BufferManagement.Business
{
	[Immutable]
	public class RoadRunnerDetails
	{
		internal RoadRunnerDetails(ZString resourceCode, RoadRunnerStatus status, ZGuid relevantTask, ZString relevantTaskDescription, TimeSpan relevantTime, bool overtime, ZDateTime awayUntil)
		{
			this.resourceCode = resourceCode;
			this.status = status;
			this.relevantProcessTaskPK = relevantTask;
			this.relevantTaskDescription = relevantTaskDescription;
			this.relevantActivityTime = relevantTime;
			this.isOvertime = overtime;
			this.awayUntil = awayUntil;
		}

		public ZString ResourceCode => resourceCode;
		public RoadRunnerStatus Status => status;
		public ZString RelevantTaskDescription => relevantTaskDescription;
		public TimeSpan RelevantActivityTime => relevantActivityTime;
		public bool IsOvertime => isOvertime;
		public ZDateTime AwayUntil => awayUntil;
		public ZGuid RelevantProcessTaskPK => relevantProcessTaskPK;

		readonly ZString resourceCode;
		readonly RoadRunnerStatus status;
		readonly ZString relevantTaskDescription;
		readonly TimeSpan relevantActivityTime;
		readonly bool isOvertime;
		readonly ZDateTime awayUntil;
		readonly ZGuid relevantProcessTaskPK;
	}

	internal class RoadRunnerDetailsDto
	{
		internal RoadRunnerDetailsDto() { }

		internal RoadRunnerDetails ConvertToRoadRunnerDetailsObject()
		{
			return new RoadRunnerDetails(ResourceCode, Status, RelevantTask != null ? RelevantTask.PK : ZGuid.Empty, RelevantTaskDescription, RelevantActivityTime, IsOvertime, AwayUntil);
		}

		internal ZString ResourceCode { get; set; }
		internal RoadRunnerStatus Status { get; set; }
		internal TimeSpan RelevantActivityTime { get; set; }
		internal bool IsOvertime { get; set; }
		internal ZDateTime AwayUntil { get; set; }
		internal ProcessTask RelevantTask { get; set; }
		internal ZString RelevantTaskDescription { get; set; } // RoadRunnerDetail descriptions are longer than ProcessTask descriptions will allow
	}
}
