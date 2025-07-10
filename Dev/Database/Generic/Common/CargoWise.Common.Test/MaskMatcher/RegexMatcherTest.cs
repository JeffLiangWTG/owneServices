using System;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class RegexMatcherTest : TestCase
	{
		public void TestRegexMatcher()
		{
			string[] orderedRegularExpressions = new[] { "^[a-z]{3}(\\d{4})[a-z]{3}", "^(\\d{4})[a-z]{5}", "^\\d{8}" };
			var matcher = new RegexMatcher();
			AssertEquals("1234", matcher.GetMatch(orderedRegularExpressions, "abc1234abc"));
			AssertEquals("1234", matcher.GetMatch(orderedRegularExpressions, "1234abcde"));
			AssertEquals("12345678", matcher.GetMatch(orderedRegularExpressions, "1234567890"));
			AssertEquals("a", matcher.GetMatch(orderedRegularExpressions, "a"));
			AssertEquals("", matcher.GetMatch(orderedRegularExpressions, ""));
			AssertExceptionThrown(typeof(ArgumentException), () => matcher.GetMatch(new[] { "(a)(b)" }, "ab"));
			AssertExceptionThrown(typeof(ArgumentException), () => matcher.GetMatch(new[] { "(a)(b)" }, "c"));
		}
	}
}