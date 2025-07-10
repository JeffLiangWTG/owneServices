using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace CargoWise.Common.MemoryManagement.Internal.Testing
{
	class LambdaTarget
	{
		public bool CleanedUp { get; set; }
	}

	class ReclaimableTest : TestCase
	{
		public void TestReclaimWithNoObject()
		{
			var reclaimer = new Reclaimer();
			bool cleanedUp = false;
			var reclaimable = reclaimer.Register("Bodgy test", FlushCallback.OnAnyThread, action =>
			{
				cleanedUp = true;
				return FlushResult.Exhausted;
			});
			Assert(!cleanedUp);
			reclaimable.CleanUp(FlushAction.Full);
			Assert(cleanedUp);
		}

		public void TestReclaimWithNullObject()
		{
			var reclaimer = new Reclaimer();
			LambdaTarget target = null;
			var reclaimable = reclaimer.Register("Bodgy test", target, FlushCallback.OnAnyThread, (lambdaTarget, action) =>
			{
				return FlushResult.Exhausted;
			});
			AssertEquals(FlushResult.Exhausted, reclaimable.CleanUp(FlushAction.Full));
		}

		public void TestReclaimWithObject()
		{
			var reclaimer = new Reclaimer();
			var target = new LambdaTarget();
			var reclaimable = reclaimer.Register("Bodgy test", target, FlushCallback.OnAnyThread, (lambdaTarget, action) =>
			{
				lambdaTarget.CleanedUp = true;
				return FlushResult.Exhausted;
			});
			Assert(!target.CleanedUp);
			reclaimable.CleanUp(FlushAction.Full);
			Assert(target.CleanedUp);
		}

		public void TestReclaimWithCollectedTarget()
		{
			var reclaimable = CreateReclaimableWithLocalVariable();
			GC.Collect();
			AssertEquals(FlushResult.NotRequired, reclaimable.CleanUp(FlushAction.Full));
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		Reclaimable CreateReclaimableWithLocalVariable()
		{
			var reclaimer = new Reclaimer();
			return reclaimer.Register("Bodgy test", new LambdaTarget(), FlushCallback.OnAnyThread, (lambdaTarget, action) =>
			{
				lambdaTarget.ToString();
				return FlushResult.Exhausted;
			});
		}
	}
}
