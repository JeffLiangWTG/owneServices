using eServices.eHubAdmin.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace eServices.eHubAdmin.Tests
{
	[TestClass]
	public class HelpersTests
	{
		[TestMethod]
		public void TestStringExtension()
		{
			const string testValue = "value";
			const string nullValue = null;
			const string defaultValue = "default";
			Assert.AreEqual(testValue, testValue.GetValueOrDefault(), "StringExtension.GetValueOrDefault()");
			Assert.AreEqual("", nullValue.GetValueOrDefault(), "StringExtension.GetValueOrDefault()");
			Assert.AreEqual(defaultValue, nullValue.GetValueOrDefault(defaultValue), "StringExtension.GetValueOrDefault()");

			const string shortName = "123456";
			const string longName = "123456789012345678901234567890";
			Assert.AreEqual(shortName, shortName.ShortName(), "StringExtension.ShortName");
			Assert.AreEqual("1234567890123456789012...", longName.ShortName(), "StringExtension.ShortName");
		}

		[TestMethod]
		public void TestStringExtension_NullName()
		{
			const string nullName = null;
			Assert.AreEqual(null, nullName.ShortName(), "StringExtension.ShortName");
		}
	}
}
