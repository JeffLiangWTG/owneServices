using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[DoNotAddToTestTree]
	sealed class MockTestForListener : TestCaseWithFactory
	{
		[TestDate(2017, 10, 10)]
		public void TestWithDate()
		{
			Assert(true);
		}

		public void TestWithInlineDate()
		{
			TestDateAttribute.Date = DateTime.Now.AddDays(1);
			Assert(true);
		}

		[TestDate]
		public void TestWithActiveDate()
		{
			Assert(true);
		}

		public void TestWithNoDate()
		{
			Assert(true);
		}
	}
}
