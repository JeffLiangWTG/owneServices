using System;
using System.Text;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class WorkflowTriggerEventData : IReferenceVersion
	{
		#region Create

		const int CurrentReferenceVersion = WTE_V1_Parser.VERSION;

		public WorkflowTriggerEventData(IWorkflowTriggerSource triggeringLog, ZGuid triggeringLogParent, ZString branchCode, ZString departmentCode, ZString contextStaffCode, ZString contextBranchCode, ZString contextDepartmentCode)
			: this(new EventSource(triggeringLog), triggeringLogParent, branchCode, departmentCode, contextStaffCode, contextBranchCode, contextDepartmentCode)
		{
		}

		public WorkflowTriggerEventData(IEventSource triggeringLog, ZGuid triggeringLogParent, ZString branchCode, ZString departmentCode, ZString contextStaffCode, ZString contextBranchCode, ZString contextDepartmentCode)
		{
			if (triggeringLog != null)
			{
				this.TriggeringLogPK = triggeringLog.Identifier;
				this.TriggeringSourceCode = triggeringLog.SourceType;
			}
			this.TriggeringLogParentPK = triggeringLogParent;
			this.TriggeringBranchCode = branchCode;
			this.TriggeringDepartmentCode = departmentCode;
			this.ContextStaffCode = contextStaffCode;
			this.ContextBranchCode = contextBranchCode;
			this.ContextDepartmentCode = contextDepartmentCode;
			this.TriggerChainID = WorkflowChainManager.ChainID;
			this.Version = CurrentReferenceVersion;
		}

		public WorkflowTriggerEventData() { }

		#endregion

		#region Deserialise

		public WorkflowTriggerEventData(StmALog log) : this(log.SL_SE_NKEvent, log.SL_Reference) { }

		public WorkflowTriggerEventData(IQueuedLog log) : this(log.SJ_SE_NKEvent, log.SJ_Reference) { }

		WorkflowTriggerEventData(ZString eventCode, ZString reference)
		{
			if (eventCode != Events.WorkflowTriggerEventCode)
			{
				throw new ArgumentException("Can only construct with a 'WTE' Event.");
			}
			ReferenceStrategy.Instance.Deserialise(this, reference);
		}

		#endregion

		public static readonly ZString TriggeringSource = "TBS";

		public ZGuid TriggeringLogPK { get; set; } = ZGuid.Invalid;
		public ZString TriggeringSourceCode { get; set; }
		public ZGuid TriggeringLogParentPK { get; set; }
		public ZString TriggeringBranchCode { get; set; }
		public ZString TriggeringDepartmentCode { get; set; }
		public ZString ContextStaffCode { get; set; }
		public ZString ContextBranchCode { get; set; }
		public ZString ContextDepartmentCode { get; set; }
		internal ZGuid TriggerChainID { get; set; }

		public int Version { get; set; }

		public string ToReference()
		{
			return ReferenceStrategy.Instance.Serialise(this);
		}

		public int GetVersion(string reference)
		{
			if (reference.Length >= 3 && reference[0] == ReferenceStrategy.VersionChar)
			{
				var delimiterIndex = reference.IndexOf(StmALog.ReferenceDelimiter);
				if (delimiterIndex == -1)
				{
					return 0;
				}
				if (Int32.TryParse(reference.Substring(1, delimiterIndex - 1), out int version))
				{
					return version;
				}
			}
			return 0;
		}

		public string GetDiagnosticLogInfo()
		{
			var result = new StringBuilder();

			result.Append((NoResString)"TriggeringBranchCode: ");
			result.AppendLine(TriggeringBranchCode);
			result.Append((NoResString)"TriggeringDepartmentCode: ");
			result.AppendLine(TriggeringDepartmentCode);
			result.Append((NoResString)"ContextStaffCode: ");
			result.AppendLine(ContextStaffCode);
			result.Append((NoResString)"ContextBranchCode: ");
			result.AppendLine(ContextBranchCode);
			result.Append((NoResString)"ContextDepartmentCode: ");
			result.AppendLine(ContextDepartmentCode);

			return result.ToString();
		}
	}
}
