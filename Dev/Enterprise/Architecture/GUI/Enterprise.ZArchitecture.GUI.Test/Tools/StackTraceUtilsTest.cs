namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class StackTraceUtilsTest : NUnit.Framework.TestCase
	{
		public void TestIsCalledFrom()
		{
			AssertEquals(false, IsCalledFromFrame1(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame1), -1));
			AssertEquals(false, IsCalledFromFrame1(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame1), 0));
			AssertEquals(true, IsCalledFromFrame1(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame1), 1));
			AssertEquals(true, IsCalledFromFrame1(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame1), 2));
			AssertEquals(false, IsCalledFromFrame2(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame1), -1));
			AssertEquals(false, IsCalledFromFrame2(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame1), 0));
			AssertEquals(true, IsCalledFromFrame2(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame1), 1));
			AssertEquals(true, IsCalledFromFrame2(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame1), 2));
			AssertEquals(false, IsCalledFromFrame2(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame2), 0));
			AssertEquals(false, IsCalledFromFrame2(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame2), 1));
			AssertEquals(true, IsCalledFromFrame2(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame2), 2));
			AssertEquals(true, IsCalledFromFrame2(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame2), 3));
			AssertEquals(false, IsCalledFromFrame3(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame3), 0));
			AssertEquals(false, IsCalledFromFrame3(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame3), 1));
			AssertEquals(false, IsCalledFromFrame3(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame3), 2));
			AssertEquals(true, IsCalledFromFrame3(nameof(StackTraceUtilsTest), nameof(IsCalledFromFrame3), 3));
			AssertEquals(false, StackTraceUtils.IsCalledFrom("Unknown Type", "Unknown Method", 999999));
		}

		bool IsCalledFromFrame3(string className, string methodName, int maxStackLength)
		{
			return IsCalledFromFrame2(className, methodName, maxStackLength);
		}

		bool IsCalledFromFrame2(string className, string methodName, int maxStackLength)
		{
			return IsCalledFromFrame1(className, methodName, maxStackLength);
		}

		bool IsCalledFromFrame1(string className, string methodName, int maxStackLength)
		{
			return StackTraceUtils.IsCalledFrom(className, methodName, maxStackLength);
		}
	}
}