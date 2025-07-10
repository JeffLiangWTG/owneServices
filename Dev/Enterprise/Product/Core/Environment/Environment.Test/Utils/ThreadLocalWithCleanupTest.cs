using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	sealed class ThreadLocalWithCleanupTest : TransactionedTestCase
	{
		class IntBox
		{
			public int Value { get; set; }
		}

		public void TestIsLocal()
		{
			using (var local = new ThreadLocalWithCleanup<IntBox>(() => new IntBox { Value = 25 }))
			{
				var task = Task.Run(() =>
				{
					local.Value.Value++;
				});

				Task.WaitAll(task);
				AssertEquals(25, local.Value.Value);
			}
		}

		public void TestClearCurrentThread_DeleteLocal()
		{
			WeakReference<IntBox> reference = null;
			using (var local = new ThreadLocalWithCleanup<IntBox>(() => MakeIntBox(15, out reference)))
			{
				var task = Task.Run(() =>
				{
					local.Value.Value++;
					local.ClearCurrentThread();
				});

				Task.WaitAll(task);

				GC.Collect();
				GC.WaitForFullGCComplete();
				AssertEquals("Cleanup should do Garbage collection.", false, reference.TryGetTarget(out _));
			}
		}

		public void TestDisposeClearsBackgroundThreads()
		{
			WeakReference<IntBox> reference = null;
			using (var local = new ThreadLocalWithCleanup<IntBox>(() => MakeIntBox(15, out reference)))
			{
				var task = Task.Run(() =>
				{
					local.Value.Value++;
				});

				Task.WaitAll(task);
			}

			GC.Collect();
			GC.WaitForFullGCComplete();
			AssertEquals("Cleanup should do Garbage collection.", false, reference.TryGetTarget(out _));
		}

		IntBox MakeIntBox(int startingValue, out WeakReference<IntBox> weakResult)
		{
			var result = new IntBox { Value = startingValue };
			weakResult = new WeakReference<IntBox>(result);
			return result;
		}
	}
}
