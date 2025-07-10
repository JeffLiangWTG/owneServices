using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.DocumentParsing;

namespace Enterprise.Integration
{
	public interface IWorkflowTriggerSource : IEventUserContextSource, IIdentified
	{
		[DocumentFieldExcludeFromMap]
		ZGuid ParentID { get; }
		ZDateTime EventTime { get; }
		ZDateTime EventTimeUtc { get; }
		ZDateTimeOffset EventTimeOffset { get; }
		ZDateTime PostedTimeUtc { get; }
		[DocumentFieldExcludeFromMap]
		ZString FriendlyTableName { get; }
		ZString Source { get; }
		ZString Reference { get; }
		[DocumentFieldExcludeFromMap]
		ZString SourceType { get; }
		ZString DepartmentCode { get; }
		ZString BranchCode { get; }
		ZString CompanyCode { get; }
		ZBool IsEstimate { get; }
		ZBool IsCancelled { get; }
		[DocumentFieldExcludeFromMap]
		IPropagationSettings PropagationSettings { get; }
	}
}
