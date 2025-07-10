using System;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class FactorySaveAlerterOverrideTest : TestCase
	{
		public void TestValidateThrowsWhenNotRegistered()
		{
			string invalidType = null;
			AssertExceptionThrown<InvalidOperationException>(() => FactorySaveAlerterOverride.TemporarilyOverride(invalidType).Dispose());
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestValidateNoThrowWhenTypeRegistered()
		{
			TestType validType = null;
			AssertNoExceptionThrown(() => FactorySaveAlerterOverride.TemporarilyOverride(validType).Dispose());
		}

		class TestType
		{
		}
	}
}
