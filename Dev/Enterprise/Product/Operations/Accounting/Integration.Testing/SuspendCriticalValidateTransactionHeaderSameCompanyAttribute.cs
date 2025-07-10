using System;
using CargoWise.Common;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.Integration.Testing
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class SuspendCriticalValidateTransactionHeaderSameCompanyAttribute : TestSetupAttribute
	{
		public override void SetUp(TestCase testCase)
		{
			IsActive = true;
		}

		public override void TearDown(TestCase testCase)
		{
			IsActive = false;
		}

		[ThreadSafe]
		public static bool IsActive;

		public static IDisposable ActivateTemporary()
		{
			if (IsActive)
			{
				throw new InvalidOperationException("Someone has already activated it. This action will cause unexpected deactivation.");
			}

			return new DisposableAction(() => IsActive = true, () => IsActive = false);
		}
	}
}
