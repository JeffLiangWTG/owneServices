using CargoWise.Types;
using CargoWise.Workflow;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Business
{
	[CodeAlive("Reference Parser loaded once using reflection")]
	public class WTE_Legacy_Parser : IReferenceParser<WorkflowTriggerEventData>
	{
		public int Version => 0;

		public Event Event => Events.WorkflowTriggerEvent;

		public void Deserialise(WorkflowTriggerEventData data, ZString reference)
		{
			var values = reference.Split(StmALog.ReferenceDelimiter);

			if (values.Length > 0)
			{
				ZGuid triggeringLogPK;
				if (ZGuid.TryParse(values[0], out triggeringLogPK))
				{
					data.TriggeringLogPK = triggeringLogPK;
				}
			}

			if (values.Length > 2)
			{
				data.TriggeringBranchCode = values[1];
				data.TriggeringDepartmentCode = values[2];
			}

			if (values.Length > 7)
			{
				// 8 Value WTE
				if (values.Length > 5)
				{
					data.ContextStaffCode = values[3];
					data.ContextBranchCode = values[4];
					data.ContextDepartmentCode = values[5];
				}

				if (values.Length > 6)
				{
					ZGuid triggerChainID;
					if (ZGuid.TryParse(values[6], out triggerChainID))
					{
						data.TriggerChainID = triggerChainID;
					}
				}

				if (values.Length > 7)
				{
					ZGuid triggerLineParentPK;
					if (ZGuid.TryParse(values[7], out triggerLineParentPK))
					{
						data.TriggeringLogParentPK = triggerLineParentPK;
					}
				}
			}
			else
			{
				// 5 Value WTE
				if (values.Length > 3)
				{
					ZGuid triggerChainID;
					if (ZGuid.TryParse(values[3], out triggerChainID))
					{
						data.TriggerChainID = triggerChainID;
					}
				}

				if (values.Length > 4)
				{
					ZGuid triggerLineParentPK;
					if (ZGuid.TryParse(values[4], out triggerLineParentPK))
					{
						data.TriggeringLogParentPK = triggerLineParentPK;
					}
				}
			}
		}

		public string Serialise(WorkflowTriggerEventData data)
		{
			return EventLogReferenceBuilder.New()
				.AddGuid(data.TriggeringLogPK)
				.AddIfValid(data.TriggeringBranchCode)
				.AddIfValid(data.TriggeringDepartmentCode)
				.AddIfValid(data.ContextStaffCode)
				.AddIfValid(data.ContextBranchCode)
				.AddIfValid(data.ContextDepartmentCode)
				.AddGuid(data.TriggerChainID)
				.AddGuid(data.TriggeringLogParentPK)
				.AddIfValid(WorkflowTriggerEventData.TriggeringSource, data.TriggeringSourceCode)
				.Build();
		}
	}

	static class WTE_Legacy_EventReferenceExtensions
	{
		internal static EventLogReferenceBuilder AddIfValid(this EventLogReferenceBuilder builder, ZString key, ZString value) => !value.IsEmpty ? builder.AddMandatory(key, value) : builder;
		internal static EventLogReferenceBuilder AddIfValid(this EventLogReferenceBuilder builder, ZString value) => !value.IsEmpty ? builder.AddMandatory(value) : builder;
	}
}
