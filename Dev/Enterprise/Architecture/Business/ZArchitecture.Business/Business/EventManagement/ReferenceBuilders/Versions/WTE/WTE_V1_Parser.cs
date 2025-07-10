using System.Text;
using CargoWise.Types;
using CargoWise.Workflow;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Business
{
	[CodeAlive("Reference Parser loaded once using reflection")]
	public class WTE_V1_Parser : IReferenceParser<WorkflowTriggerEventData>
	{
		public const int VERSION = 1;

		public int Version => VERSION;

		public Event Event => Events.WorkflowTriggerEvent;

		public void Deserialise(WorkflowTriggerEventData data, ZString reference)
		{
			var values = reference.SplitIgnoringEscapedDelimiter(StmALog.ReferenceDelimiter, StmALog.EscapeChar, false, true);
			if (values.Length > 8)
			{
				ZGuid triggeringLogPK;
				if (ZGuid.TryParse(values[1], out triggeringLogPK))
				{
					data.TriggeringLogPK = triggeringLogPK;
				}

				data.TriggeringBranchCode = values[2].Deserialise();
				data.TriggeringDepartmentCode = values[3].Deserialise();
				data.ContextStaffCode = values[4].Deserialise();
				data.ContextBranchCode = values[5].Deserialise();
				data.ContextDepartmentCode = values[6].Deserialise();

				ZGuid triggerChainID;
				if (ZGuid.TryParse(values[7], out triggerChainID))
				{
					data.TriggerChainID = triggerChainID;
				}

				ZGuid triggerLineParentPK;
				if (ZGuid.TryParse(values[8], out triggerLineParentPK))
				{
					data.TriggeringLogParentPK = triggerLineParentPK;
				}
			}
		}

		public string Serialise(WorkflowTriggerEventData data)
		{
			return EventLogReferenceBuilder.New()
				.AddMandatory(GetVersionAsString(Version))
				.AddGuid(data.TriggeringLogPK)
				.AddWithEscaped(data.TriggeringBranchCode)
				.AddWithEscaped(data.TriggeringDepartmentCode)
				.AddWithEscaped(data.ContextStaffCode)
				.AddWithEscaped(data.ContextBranchCode)
				.AddWithEscaped(data.ContextDepartmentCode)
				.AddGuidIfValid(data.TriggerChainID)
				.AddGuidIfValid(data.TriggeringLogParentPK)
				.AddMandatory(WorkflowTriggerEventData.TriggeringSource, data.TriggeringSourceCode)
				.Build();
		}

		string GetVersionAsString(int version)
		{
			return $"{ReferenceStrategy.VersionChar}{version}";
		}
	}

	static class WTE_V1_EventReferenceExtensions
	{
		internal static EventLogReferenceBuilder AddWithEscaped(this EventLogReferenceBuilder builder, ZString value) => !value.IsEmpty ? builder.AddMandatory(value.Serialise()) : builder.AddMandatory(value);

		internal static string Serialise(this ZString code)
		{
			var sb = new StringBuilder();
			foreach (char c in code)
			{
				switch (c)
				{
					case StmALog.ReferenceDelimiter:
						sb.Append(StmALog.EscapeChar);
						sb.Append(StmALog.ReferenceDelimiter);
						break;
					case StmALog.EscapeChar:
						sb.Append(StmALog.EscapeChar);
						sb.Append(StmALog.EscapeChar);
						break;
					default:
						sb.Append(c);
						break;
				}
			}
			return sb.ToString();
		}

		internal static string Deserialise(this ZString code)
		{
			var sb = new StringBuilder(code);
			sb.Replace($"{StmALog.EscapeChar}{StmALog.ReferenceDelimiter}", StmALog.ReferenceDelimiter.ToString());
			sb.Replace($"{StmALog.EscapeChar}{StmALog.EscapeChar}", StmALog.EscapeChar.ToString());
			return sb.ToString();
		}
	}
}
