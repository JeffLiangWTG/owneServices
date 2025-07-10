using System;
using CargoWise.Database.TestFramework.ObjectModel;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	public sealed class AssertionProviderTestListener : BaseTestListener
	{
		public override void StartTest(TestCase test, DateTime startTime)
		{
			SQLDataObject.AssertionProvider = NUnitAssertionProvider.Instance;
			base.StartTest(test, startTime);
		}

		public override void EndAllTests(DateTime endTime)
		{
			base.EndAllTests(endTime);
			SQLDataObject.AssertionProvider = null;
		}
	}

	public class AssertionProviderTestListenerTest : TestCase
	{
		public void TestAssertionProviderSetup()
		{
			AssertSame("Test listener should setup the assertion provider.", NUnitAssertionProvider.Instance, SQLDataObject.AssertionProvider);
		}
	}
}
