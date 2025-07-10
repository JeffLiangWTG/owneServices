using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.DB.Helpers
{
	class ColumnNameHelperTest : TestCase
	{
		public void TestRemovePrefix()
		{
			var result = ColumnNameHelper.RemovePrefix("");
			AssertEquals(string.Empty, result);

			result = ColumnNameHelper.RemovePrefix("_XX_YY");
			AssertEquals("YY", result);

			result = ColumnNameHelper.RemovePrefix("XX_YY");
			AssertEquals("YY", result);

			result = ColumnNameHelper.RemovePrefix("XX_YY_ZZ");
			AssertEquals("ZZ", result);

			result = ColumnNameHelper.RemovePrefix("XX_Y_ZZ");
			AssertEquals("Y_ZZ", result);

			result = ColumnNameHelper.RemovePrefix("XX_YYY_ZZ");
			AssertEquals("ZZ", result);

			result = ColumnNameHelper.RemovePrefix("XX_YYYY_ZZ");
			AssertEquals("YYYY_ZZ", result);

			result = ColumnNameHelper.RemovePrefix("XX_YYYY_ZZ_KKK");
			AssertEquals("YYYY_ZZ_KKK", result);
		}

		public void TestGetParentTablePrefix()
		{
			var result = ColumnNameHelper.GetParentTablePrefix("");
			AssertEquals(string.Empty, result);

			result = ColumnNameHelper.GetParentTablePrefix("XX_YY");
			AssertEquals("YY", result);

			result = ColumnNameHelper.GetParentTablePrefix("XX_YY_Something");
			AssertEquals("YY", result);
		}

		public void TestTransformNumberToString()
		{
			var result = ColumnNameHelper.TransformNumberToString("1A");
			AssertEquals("OneA", result);

			result = ColumnNameHelper.TransformNumberToString("1A1");
			AssertEquals("OneA1", result);

			result = ColumnNameHelper.TransformNumberToString("OneA1");
			AssertEquals("OneA1", result);
		}
	}
}
