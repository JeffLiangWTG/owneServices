using System;
using System.Globalization;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	[Serializable]
	public class WorkingTimeNotAvailableException : Exception
	{
		public WorkingTimeNotAvailableException(GlbBranch branch, GlbDepartment department, IBranchDepartmentProvider provider)
			: base(GetMessage(branch, department, provider))
		{
		}

#if NETFRAMEWORK
		protected WorkingTimeNotAvailableException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		static string GetMessage(GlbBranch branch, GlbDepartment department, IBranchDepartmentProvider provider)
		{
			var branchString = branch?.GB_Code ?? "null";
			var departmentString = department?.GE_Code ?? "null";
			var providerValue = provider?.ToString() ?? "null";
			var systemName = (provider as BMComponent)?.System?.FS_Name ?? "null";
			var boardName = (provider as BMBoardSection)?.Board?.MB_Name ?? "null";

			return string.Format(CultureInfo.InvariantCulture, (NoResString)@"Could not create a working time context. Both Branch and Department must be specified.

Branch: {0}
Department: {1}
System: {2},
Board: {3}
Provider Value: {4}.",
			branchString, departmentString, systemName, boardName, providerValue); // exception message should not be translated
		}
	}
}
