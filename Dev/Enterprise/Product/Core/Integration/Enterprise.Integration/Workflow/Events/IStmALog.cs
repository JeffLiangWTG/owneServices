using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.DocumentParsing;

namespace Enterprise.Integration
{
	public interface IStmALog : IAllowMacroAccessToAllPublicProperties, IWorkflowTriggerSource, IBusiness
	{
		[DocumentFieldExcludeFromMap]
		BusinessObject Master { get; }
		[DocumentFieldExcludeFromMap]
		ZGuid PK { get; }
		[DocumentFieldExcludeFromMap]
		ZDateTime SL_EventTime { get; }
		[DocumentFieldExcludeFromMap]
		ZDateTime SL_EventTimeUtc { get; }
		[DocumentFieldExcludeFromMap]
		ZDateTimeOffset SL_EventTimeOffset { get; }
		[DocumentFieldExcludeFromMap]
		ZGuid SL_Parent { get; }
		[DocumentFieldExcludeFromMap]
		ZString SL_GS_NKUser { get; }
		[DocumentFieldExcludeFromMap]
		ZString SL_Reference { get; }
		[DocumentFieldExcludeFromMap]
		ZString SL_ReferenceForBinding { get; }
		[DocumentFieldExcludeFromMap]
		ZString SL_SE_NKEvent { get; }
		[DocumentFieldExcludeFromMap]
		ZString SL_Table { get; }
		[DocumentFieldExcludeFromMap]
		ZString SL_TableFriendlyName { get; }
		[DocumentFieldExcludeFromMap]
		ZString SL_UserNameAndInitials { get; }
		[DocumentFieldExcludeFromMap]
		ZString DisplayEventReference { get; }
		[DocumentFieldExcludeFromMap]
		ZDateTime PostedLocalBranchTime { get; }
		[DocumentFieldExcludeFromMap]
		ZBool SL_IsEstimate { get; }
		[DocumentFieldExcludeFromMap]
		ZBool SL_IsCancelled { get; }
		[DocumentFieldExcludeFromMap]
		ZBool SL_FireWorkflow { get; }
		[DocumentFieldExcludeFromMap]
		ZString SL_GB_NKBranch { get; }
		[DocumentFieldExcludeFromMap]
		ZString SL_GE_NKDepartment { get; }
		[DocumentFieldExcludeFromMap]
		IDictionary<string, string> Parameters { get; }
		IStaff User { get; }
		IKeyDataPairCollection SourceInfoItems { get; }
		ILogsParams Params { get; }
		ZDateTime PostedTimeLocal { get; }

		void Cancel();
		IStmALog WeakCopy();
	}
}
