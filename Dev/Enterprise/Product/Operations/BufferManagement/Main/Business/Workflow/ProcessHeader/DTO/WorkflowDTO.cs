using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowDTO : IEstimatable, IBufferedItem, IWorkflow
	{
		public WorkflowDTO(Guid pk, IBuffer relatedBuffer = null, IBuffer relatedSubBuffer = null)
		{
			PK = pk;
			tasks = new List<TaskDTO>();
			this.relatedBuffer = relatedBuffer;
			this.relatedSubBuffer = relatedSubBuffer;
		}

		readonly List<TaskDTO> tasks;
		readonly IBuffer relatedBuffer;
		readonly IBuffer relatedSubBuffer;

		public Guid PK { get; private set; }
		public Guid ReleaseGroup { get; set; }
		public ZDateTime ReleaseDate { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal EstimateHours { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int PlannedDurationMinutes { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int PenetrationMinutesWithoutAging => this.CalculatePenetrationMinutesWithoutAging();

		public ZGuid ShapeForZoneCalculation { get; set; }
		public ZGuid ParentWorkflowForZoneCalculation { get; set; }

		public ICollection<TaskDTO> Tasks
		{
			get { return tasks; }
		}

		#region IBufferedItem Members

		string IBufferedItem.Name
		{
			get { return string.Empty; }
		}

		IReadOnlyCollection<IBuffer> IBufferedItem.GetRelatedBuffers(bool includeNetworkBuffers)
		{
			var buffers = new List<IBuffer>();
			if (relatedBuffer != null)
			{
				buffers.Add(relatedBuffer);
			}
			return buffers;
		}

		IReadOnlyCollection<IBuffer> IBufferedItem.RelatedSubBuffers
		{
			get
			{
				var subBuffers = new List<IBuffer>();
				if (relatedSubBuffer != null)
				{
					subBuffers.Add(relatedSubBuffer);
				}
				return subBuffers;
			}
		}

		DateTime IBufferedItem.StartableTime
		{
			get { return ReleaseDate.IsValid && !ReleaseDate.IsEmpty ? ReleaseDate.ToDateTime() : default(DateTime); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int IBufferedItem.PlannedDurationInMinutes
		{
			get { return PlannedDurationMinutes; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int IBufferedItem.RemainingEstimateInMinutes
		{
			get { return (int)(EstimateHours * 60); }
		}

		WorkStatus IBufferedItem.WorkStatus
		{
			get { return WorkStatus.None; }
		}

		bool IBufferedItem.IsClosed => false;

		public bool HasBufferPenetrationWhenClosed => true;

		#endregion

		#region IIdentified Members

		ZGuid IIdentified.Identifier
		{
			get { return PK; }
		}

		#endregion

		#region IWorkflow Members

		Guid IWorkflow.ReleaseGroupPK
		{
			get { return ReleaseGroup; }
		}

		public string Description { get; set; }

		DateTime IWorkflow.EarliestStartDateUtc
		{
			get { return default(DateTime); }
		}

		DateTime IWorkflow.JobEarliestStartDateUtc
		{
			get { return default(DateTime); }
		}

		string IWorkflow.JobDescription
		{
			get { return string.Empty; }
		}

		string IWorkflow.WorkflowType
		{
			get { return string.Empty; }
		}

		string IWorkflow.Status
		{
			get { return string.Empty; }
		}

		IEnumerable<IWorkflowTask> IWorkflow.Tasks
		{
			get { yield break; }
		}

		public Guid CurrentComponentPK { get; set; }

		public Guid JobLevelWorkflowPK { get; set; }

		public Guid ParentId { get; set; }

		public string ParentTableCode { get; set; }

		public DateTime LastTransferDateUtc { get; set; }

		public string LastTransferType { get; set; }

		public bool IsCcpmScheduleReleasable { get; set; }

		#endregion
	}
}
