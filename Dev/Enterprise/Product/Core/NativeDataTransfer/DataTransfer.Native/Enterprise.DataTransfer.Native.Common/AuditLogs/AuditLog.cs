using System;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Native.Common.AuditLogs
{
	public class AuditLog
	{
		public ZString Code { get; set; }
		public ZDateTime PostedTime { get; set; }
		public ZDateTime EventTime { get; set; }
		public ZString UserCode { get; set; }
		public ZString DepartmentCode { get; set; }
		public ZString BranchCode { get; set; }
		public Guid ParentId { get; set; }
		public ZString TableName { get; set; }
	}
}
