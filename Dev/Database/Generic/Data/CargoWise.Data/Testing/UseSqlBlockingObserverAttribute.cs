#if DEBUG
using System;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public sealed class UseSqlBlockingObserverAttribute : TestSetupAttribute
	{
		public UseSqlBlockingObserverAttribute()
		{
			IntervalMs = 100;
		}

		public int IntervalMs { get; set; }

		public override void SetUp(TestCase testCase)
		{
			SqlBlockingObserver.Start(TimeSpan.FromMilliseconds(IntervalMs));
		}

		public override void TearDown(TestCase testCase)
		{
			SqlBlockingObserver.Stop();

			var lockingInfo = SqlBlockingObserver.GetLockingInfo();
			if (!string.IsNullOrEmpty(lockingInfo))
			{
				Assertion.Fail($@"{lockingInfo}

{SqlBlockingObserver.GetExtraDebugInformation()}
");
			}
		}
	}
}
#endif
