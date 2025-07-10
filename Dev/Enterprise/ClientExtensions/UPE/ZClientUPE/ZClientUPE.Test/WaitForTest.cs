using System;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Testing
{
	sealed class WaitForTest : TestCaseWithFactory
	{
		public void TestWaitFor()
		{
			NUnit.Framework.Assert.That(WaitFor<DateTime>.Run(TimeSpan.FromSeconds(1), () => DateTime.Now), Is.EqualTo(DateTime.Now).Within(1).Seconds, "WaitFor with return value");
			AssertNoExceptionThrown("WaitFor with no exception on void method", () => WaitFor<Object>.Run(TimeSpan.FromSeconds(1), () => { Thread.Sleep(1); return null; }));
			AssertExceptionThrown<TimeoutException>("WaitFor with exception", () => WaitFor<Object>.Run(TimeSpan.FromMilliseconds(1), () => { Thread.Sleep(1000); return null; }));
		}

		public void TestWaitFor_Timer()
		{
			var startTime = DateTime.Now;
			var timer = TimeSpan.FromSeconds(5);
			AssertExceptionThrown<TimeoutException>(() => WaitFor<Object>.Run(timer, () => { Thread.Sleep(30000); return null; }));
			AssertCloseEnough(startTime.Add(timer), DateTime.Now);

			startTime = DateTime.Now;
			timer = TimeSpan.FromSeconds(10);
			AssertExceptionThrown<TimeoutException>(() => WaitFor<Object>.Run(timer, () => { Thread.Sleep(30000); return null; }));
			AssertCloseEnough(startTime.Add(timer), DateTime.Now);
		}
	}
}
