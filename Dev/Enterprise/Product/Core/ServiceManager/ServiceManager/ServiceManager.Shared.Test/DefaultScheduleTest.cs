using System;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ServiceManager.Shared.Testing
{
	class DefaultScheduleTest
	{
		[Test]
		public void TestDefaultScheduleMapsFromHostedServiceAttribute()
		{
			var attribute = new HostedServiceAttribute("~~T", "#DESC#", "TST", typeof(object))
			{
				DefaultScheduleRunEvery = "15minutes",
				DefaultScheduleDayOfMonth = 1,
				DefaultScheduleDaysOfWeek = new[] { DayOfWeek.Friday, DayOfWeek.Saturday, },
				DefaultScheduleStartAtLocal = "2hours",
				DefaultScheduleEndAtLocal = "4hours",
				DefaultScheduleStartAtUtc = "6hours",
				DefaultScheduleEndAtUtc = "8hours",
				DefaultScheduleRandomStartOffset = "20minutes",
				DefaultScheduleDoNotRunTillNextDueTimeIfOverdue = "10hours"
			};

			Assert.Multiple(() =>
			{
				Assert.That(attribute.DefaultSchedule.RunEvery, Is.EqualTo("15minutes"));
				Assert.That(attribute.DefaultSchedule.DayOfMonth, Is.EqualTo(1));
				Assert.That(attribute.DefaultSchedule.DaysOfWeek, Is.EqualTo(new[] { DayOfWeek.Friday, DayOfWeek.Saturday, }));
				Assert.That(attribute.DefaultSchedule.StartAtLocal, Is.EqualTo("2hours"));
				Assert.That(attribute.DefaultSchedule.EndAtLocal, Is.EqualTo("4hours"));
				Assert.That(attribute.DefaultSchedule.StartAtUtc, Is.EqualTo("6hours"));
				Assert.That(attribute.DefaultSchedule.EndAtUtc, Is.EqualTo("8hours"));
				Assert.That(attribute.DefaultSchedule.RandomStartOffset, Is.EqualTo("20minutes"));
				Assert.That(attribute.DefaultSchedule.DoNotRunTillNextDueTimeIfOverdue, Is.EqualTo("10hours"));
			});
		}
	}
}
