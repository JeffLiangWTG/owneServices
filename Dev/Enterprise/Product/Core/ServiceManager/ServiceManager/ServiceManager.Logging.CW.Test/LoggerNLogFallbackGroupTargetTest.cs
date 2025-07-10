using System;
using System.Collections.Generic;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Shared.Testing
{
	public class LoggerNLogFallbackGroupTargetTest : TransactionedTestCase
	{
		public void TestAllEventsAreWrittenToFirstTarget_WhenNoExceptions()
		{
			var target1 = new TargetForTest();
			var target2 = new TargetForTest();

			using var wrapper = CreateAndInitializeFallbackGroupTarget(false, target1, target2);

			WriteAndAssertNoExceptions(wrapper);

			AssertEquals(10, target1.WriteCount);
			AssertEquals(0, target2.WriteCount);

			AssertNoFlushException(wrapper);
		}

		public void TestFirstTargetFails_Write_SecondTargetWritesAllEvents()
		{
			var target1 = new TargetForTest { FailCounter = 1 };
			var target2 = new TargetForTest();

			using var wrapper = CreateAndInitializeFallbackGroupTarget(false, target1, target2);

			WriteAndAssertNoExceptions(wrapper);

			AssertEquals(1, target1.WriteCount);
			AssertEquals(10, target2.WriteCount);
		}

		#region Implementation

		void AssertNoFlushException(FallbackGroupTarget wrapper)
		{
			Exception flushException = null;
			using var flushHit = new ManualResetEvent(false);
			wrapper.Flush(ex =>
			{
				flushException = ex;
				flushHit.Set();
			});

			flushHit.WaitOne();
			if (flushException != null)
			{
				AssertEquals(false, flushException.ToString());
			}
		}

		void WriteAndAssertNoExceptions(FallbackGroupTarget wrapper)
		{
			var exceptions = new List<Exception>();
			for (var i = 0; i < 10; ++i)
			{
				wrapper.WriteAsyncLogEvent(LogEventInfo.CreateNullEvent().WithContinuation(exceptions.Add));
			}

			AssertEquals(10, exceptions.Count);
			foreach (var e in exceptions)
			{
				AssertNull(e);
			}
		}

		FallbackGroupTarget CreateAndInitializeFallbackGroupTarget(bool returnToFirstOnSuccess, params Target[] targets)
		{
			var wrapper = new FallbackGroupTarget(targets)
			{
				ReturnToFirstOnSuccess = returnToFirstOnSuccess,
			};

			LogManager.Configuration = new LoggingConfiguration();

			var nameSuffix = 0;
			foreach (var target in targets)
			{
				target.Name = "target" + nameSuffix;
				LogManager.Configuration.AddTarget(target);
				nameSuffix++;
			}

			wrapper.Name = "wrapper";
			LogManager.Configuration.AddTarget(wrapper);

			LogManager.ReconfigExistingLoggers();

			return wrapper;
		}

		class TargetForTest : Target
		{
			public int FlushCount { get; set; }
			public int WriteCount { get; set; }
			public int FailCounter { get; set; }

			protected override void Write(LogEventInfo logEvent)
			{
				Assert(FlushCount <= WriteCount);
				WriteCount++;

				if (FailCounter > 0)
				{
					FailCounter--;
					throw new InvalidOperationException("Some failure.");
				}
			}
		}

		#endregion
	}
}
