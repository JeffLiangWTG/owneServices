using System;
using NUnit.Framework;

namespace Enterprise.Accounting.Integration.Testing
{
	public static class AccountingTestHelper
	{
		public static bool IsAssertionFailedError(Exception ex)
		{
			return ex is AssertionFailedError;
		}

		public static void AssertNotNull(string message, object actual)
		{
			NUnit.Framework.Assertion.AssertNotNull(message, actual);
		}

		public static void AssertEquals(string message, object expected, object actual)
		{
			NUnit.Framework.Assertion.AssertEquals(message, expected, actual);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		public static string GetTestBaseSourcePath()
		{
			return NUnit.Framework.TestCase.BaseSourcePath;
		}
	}
}
