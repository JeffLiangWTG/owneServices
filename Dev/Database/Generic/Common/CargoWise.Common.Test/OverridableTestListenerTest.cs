using System;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class OverridableTestListenerTest : TestCase
	{
		public void TestOverrideIsReset()
		{
			var overridable = new Overridable<string>();
			overridable.Value = "override";
			AssertEquals("override", overridable.Value);
			new OverridableTestListener().AfterEachTest(DateTime.UtcNow);
			AssertNull(overridable.Value);
		}
	}
}
