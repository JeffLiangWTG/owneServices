using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Workflow;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public class EventSource : IEventSource
	{
		public EventSource(IWorkflowTriggerSource log)
		{
			Log = log ?? throw new ArgumentNullException(nameof(log));
			if (log is BusinessObject bizo && (bizo.IsDeleted || bizo.IsDeleting))
			{
				var message = StmALog.LogMessages.DeletedStmALogError(log.Identifier);
				ErrorReporter.ReportOnceWithAdditionalInfo(null, message,
					StmALog.ErrorReportKeys.Category.WorkflowGun,
					StmALog.ErrorReportKeys.Category.DeletedStmALog);
				throw new ArgumentException(message);
			}

			if (log.Identifier.IsValid)
			{
				Identifier = log.Identifier.ToGuid();
			}
			else
			{
				Identifier = Guid.Empty;
			}

			EventTime = log.EventTimeOffset.ToDateTimeOffset();
			ParentID = log.ParentID.ToGuid();
			SourceType = log.SourceType;
			Reference = log.Reference;
		}

		public IWorkflowTriggerSource Log { get; }
		public Guid Identifier { get; }
		public Guid ParentID { get; }
		public DateTimeOffset EventTime { get; }
		public string SourceType { get; }
		public string Reference { get; }
		public CargoWise.Workflow.IPropagationSettings PropagationSettings { get; } = new DefaultPropagationSettings();

		sealed class DefaultPropagationSettings : CargoWise.Workflow.IPropagationSettings
		{
			public bool PropagateOnParameterChange { get; } = true;
		}
	}
}
