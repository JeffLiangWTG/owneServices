using System;
using CargoWise.Main.Navigation;
using NUnit.Framework;

namespace CargoWise.Main.Test.Navigation;

public class TimeExtensionsTest : TestCase
{
	public void TestToFriendlyTimeAgoString_WhenDateTime()
	{
		var now = DateTime.UtcNow;

		AssertEquals("just now", now.ToFriendlyTimeAgoString());
		AssertEquals("just now", now.AddSeconds(1).ToFriendlyTimeAgoString());

		AssertEquals("1 minute ago", now.AddMinutes(-1).ToFriendlyTimeAgoString());
		AssertEquals("2 minutes ago", now.AddMinutes(-2).ToFriendlyTimeAgoString());

		AssertEquals("2 hours ago", now.AddHours(-2).ToFriendlyTimeAgoString());

		AssertEquals("1 day ago", now.AddDays(-1).ToFriendlyTimeAgoString());
		AssertEquals("2 days ago", now.AddDays(-2).ToFriendlyTimeAgoString());

		AssertEquals("1 week ago", now.AddDays(-7).ToFriendlyTimeAgoString());
		AssertEquals("2 weeks ago", now.AddDays(-14).ToFriendlyTimeAgoString());

		AssertEquals("1 month ago", now.AddDays(-30).ToFriendlyTimeAgoString());
		AssertEquals("2 months ago", now.AddDays(-60).ToFriendlyTimeAgoString());

		AssertEquals("1 year ago", now.AddDays(-365).ToFriendlyTimeAgoString());
		AssertEquals("2 years ago", now.AddDays(-730).ToFriendlyTimeAgoString());
	}
	public void TestToFriendlyTimeAgoString_WhenTimeSpan()
	{
		AssertEquals(string.Empty, TimeSpan.FromMinutes(-2).ToFriendlyTimeAgoString());
		AssertEquals("just now", TimeSpan.FromSeconds(0).ToFriendlyTimeAgoString());
		AssertEquals("just now", TimeSpan.FromSeconds(1).ToFriendlyTimeAgoString());

		AssertEquals("1 minute ago", TimeSpan.FromMinutes(1).ToFriendlyTimeAgoString());
		AssertEquals("2 minutes ago", TimeSpan.FromMinutes(2).ToFriendlyTimeAgoString());

		AssertEquals("1 hour ago", TimeSpan.FromHours(1).ToFriendlyTimeAgoString());
		AssertEquals("2 hours ago", TimeSpan.FromHours(2).ToFriendlyTimeAgoString());

		AssertEquals("1 day ago", TimeSpan.FromDays(1).ToFriendlyTimeAgoString());
		AssertEquals("2 days ago", TimeSpan.FromDays(2).ToFriendlyTimeAgoString());

		AssertEquals("1 week ago", TimeSpan.FromDays(8).ToFriendlyTimeAgoString());
		AssertEquals("2 weeks ago", TimeSpan.FromDays(15).ToFriendlyTimeAgoString());

		AssertEquals("1 month ago", TimeSpan.FromDays(32).ToFriendlyTimeAgoString());
		AssertEquals("2 months ago", TimeSpan.FromDays(64).ToFriendlyTimeAgoString());

		AssertEquals("1 year ago", TimeSpan.FromDays(380).ToFriendlyTimeAgoString());
		AssertEquals("2 years ago", TimeSpan.FromDays(800).ToFriendlyTimeAgoString());
	}
}
