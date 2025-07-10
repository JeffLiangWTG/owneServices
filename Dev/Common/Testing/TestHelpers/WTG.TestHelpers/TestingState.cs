using NUnit.Framework;

namespace WTG.TestHelpers
{
	public static class TestingState
	{
		public static string TempPath => NUnit.Framework.TestingState.TempPath;
		public static void Setup() => NUnit.Framework.TestingState.Setup();

		public static bool IsRunningOnDAT
		{
			get
			{
				return NUnit.Framework.TestingState.IsRunningOnDAT;
			}
			set
			{
				NUnit.Framework.TestingState.IsRunningOnDAT = value;
			}
		}

		public static bool IsTest
		{
			get
			{
				if (NUnit.Framework.TestingState.IsRunningTests)
				{
					return true;
				}

				return false;
			}
		}

		public static bool InTransactionedTestCase
		{
			get
			{
				if (TransactionedTestCase.InTransactionedTestCase)
				{
					return TransactionedTestCase.InTransactionedTestCase;
				}

				return false;
			}
		}
	}
}
