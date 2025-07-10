using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.EventManagement.Testing
{
	sealed class EventSourceTest : TestCaseWithFactory
	{
		public void TestNotNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new EventSource(null));
		}

		public void TestEventSourceMustHaveValidEventTime()
		{
			AssertExceptionThrown(typeof(Exception), () => new EventSource(new TriggerSource()));
			AssertNoExceptionThrown(() => new EventSource(new TriggerSource
			{
				ParentID = ZGuid.NewZGuid(),
				Identifier = ZGuid.NewZGuid(),
				EventTime = ZDateTime.Now,
				EventTimeOffset = ZDateTimeOffset.Now
			}));
		}

		public void TestEventSourceProvidedDeletedLog()
		{
			var log = Factory.New<StmALog>();
			log.Delete();

			using (((IBusinessObjectInternals)log).SuppressReportRowDeletedError())
			{
				AssertExceptionThrown<ArgumentException>(() =>
				{
					new EventSource(log);
				});
			}

			AssertContains(StmALog.LogMessages.DeletedStmALogError(log.PK), ErrorReporter.LastMessageReported);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Instance.Clear();
		}
	}
}
