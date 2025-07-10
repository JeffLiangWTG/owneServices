using System;
using CargoWise.Database.Abstractions;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class EnterpriseSystemClockTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestReturnsCurrentTime()
		{
			AssertCloseEnough(ZDateTime.UtcNow.ToDateTime(), clock.UtcNow, 1);
		}

		[TestDate(2123, 04, 05, 06, 07, 08)]
		public void TestReturnsMockedTime()
		{
			AssertEquals(new DateTime(2123, 04, 05, 06, 07, 08), clock.UtcNow);
		}

		ISystemClock clock;

		protected override void SetUp()
		{
			base.SetUp();
			clock = new EnterpriseSystemClock();
		}
	}
}
