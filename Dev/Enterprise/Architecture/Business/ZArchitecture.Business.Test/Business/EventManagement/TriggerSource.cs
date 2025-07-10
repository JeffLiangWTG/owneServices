using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business.EventManagement.Testing
{
	sealed class TriggerSource : IWorkflowTriggerSource
	{
		public ZGuid ParentID { get; set; }

		public ZDateTime EventTime { get; set; }

		public ZDateTimeOffset EventTimeOffset { get; set; }

		public ZDateTime PostedTimeUtc { get; set; }

		public ZString FriendlyTableName { get; set; }

		public ZString Reference { get; set; }

		public ZString SourceType { get; set; }

		public ZString StaffCode { get; set; }

		public ZString DepartmentCode { get; set; }

		public ZString BranchCode { get; set; }

		public ZString CompanyCode { get; set; }

		public ZBool IsEstimate { get; set; }

		public ZGuid Identifier { get; set; }

		public ZDateTime PostedTimeLocal { get; set; }

		public ZDateTime EventTimeUtc { get; set; }

		public ZString Source => FriendlyTableName;

		public ZBool IsCancelled { get; set; }

		public ZString UserCode => StaffCode;

		public IPropagationSettings PropagationSettings => new DefaultPropagationSettings();
	}
}
