using System;
using System.Runtime.CompilerServices;
using CargoWise.Common;

namespace Enterprise.Environment.Testing
{
	internal class DisposableEnvironmentForTest : DisposableEnvironment
	{
		public DisposableEnvironmentForTest(
			Guid branchPk,
			string loginName,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1,
			bool reportMissingBranch = true) : base(branchPk, loginName, callerFilePath, callerMemberName,
			callerLineNumber, reportMissingBranch)
		{
		}
		public static IDisposable CleanupDepartment_ForTest()
		{
			var prevBrnDepartment = brnDepartment;
			brnDepartment = Guid.Empty;

			return new DisposableAction(() => brnDepartment = prevBrnDepartment);
		}
	}
}
