using System;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class FormUserStatisticsTest : TestCase
	{
		[TestDate(2006, 1, 1, 9, 0, 0)]
		public void TestShowAndClose()
		{
			FormUserStatistics stats = new FormUserStatisticsWithEnvTime();
			stats.NotifyFormShownUtc("Hello World", "TestModule", EnvProxy.Instance.Time.CurrentUtcDateTime);
			AssertEquals("Hello World", stats.FormCaption);
			AssertEquals("TestModule", stats.ModuleName);
			AssertEquals(EnvProxy.Instance.Time.CurrentUtcDateTime, stats.ShownDateTimeUtc);

			Guid someGuid = Guid.NewGuid();
			stats.NotifyFormClosed(someGuid, "OH");
			AssertEquals("OH", stats.BusinessObjectTableCode);
			AssertEquals(someGuid, stats.BusinessObjectPK);
			AssertEquals(EnvProxy.Instance.Time.CurrentUtcDateTime, stats.CloseDateTimeUtc);
		}

		[TestDate(2006, 1, 1, 9, 0, 0)]
		public void TestActiveInactive()
		{
			var stats = new FormUserStatisticsWithEnvTime();
			stats.NotifyFormShownUtc("Hello World", "TestModule", EnvProxy.Instance.Time.CurrentUtcDateTime);
			stats.KeyPresses++;
			AssertEquals((double)0, stats.InactiveDuration.TotalSeconds);

			TestDateAttribute.Date = new DateTime(2006, 1, 1, 9, 2, 0);
			stats.KeyPresses++;
			AssertEquals((double)2, stats.InactiveDuration.TotalMinutes);

			stats.MouseClicks++;
			AssertEquals((double)2, stats.InactiveDuration.TotalMinutes);

			stats.ControlFocusChanges++;
			AssertEquals((double)2, stats.InactiveDuration.TotalMinutes);

			TestDateAttribute.Date = new DateTime(2006, 1, 1, 9, 5, 0);
			stats.ControlFocusChanges++;
			AssertEquals((double)5, stats.InactiveDuration.TotalMinutes);

			TestDateAttribute.Date = new DateTime(2006, 1, 1, 9, 10, 0);
			stats.NotifyFormDeactivate();
			stats.ControlFocusChanges++;
			AssertEquals("If focus is lost, then we stop counting inactive time", (double)5, stats.InactiveDuration.TotalMinutes);

			AssertEquals(2, stats.KeyPresses);
			AssertEquals(1, stats.MouseClicks);
			AssertEquals(3, stats.ControlFocusChanges);

			TestDateAttribute.Date = new DateTime(2006, 1, 1, 9, 10, 0);
			stats.NotifyFormClosed(Guid.Empty, string.Empty);
			AssertEquals((double)5, stats.ActiveDuration.TotalMinutes);
		}
	}

	class FormUserStatisticsWithEnvTime : FormUserStatistics
	{
		public override DateTime GetCurrentUtcTime()
		{
			return EnvProxy.Instance.Time.CurrentUtcDateTime;
		}
	}
}
