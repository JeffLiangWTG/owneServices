using NUnit.Framework;

namespace CargoWise.Types.Tests
{
	class ZDateTimeGeneralHelperTest : TestCase
	{
		public void TestGetTextFromTimeOffset_2Digits()
		{
			AssertGetTextFromTimeOffset_2Digits("10:30", new ZDateTime(2006, 1, 1, 10, 30, 0));
			AssertGetTextFromTimeOffset_2Digits("10:03", new ZDateTime(2006, 1, 1, 10, 3, 0));
			AssertGetTextFromTimeOffset_2Digits("01:03", new ZDateTime(2006, 1, 1, 1, 3, 0));
			AssertGetTextFromTimeOffset_2Digits("00:01", new ZDateTime(2006, 1, 1, 0, 1, 0));
			AssertGetTextFromTimeOffset_2Digits("00:00", new ZDateTime(2006, 1, 1, 0, 0, 0));
			AssertGetTextFromTimeOffset_2Digits("23:59", new ZDateTime(2006, 1, 1, 23, 59, 0));
			AssertGetTextFromTimeOffset_2Digits("-00:01", new ZDateTime(2006, 1, 1).AddMinutes(-1));
			AssertGetTextFromTimeOffset_2Digits("-23:59", new ZDateTime(2006, 1, 1).AddHours(-23).AddMinutes(-59));
		}

		public void TestGetTextFromTimeOffset_3Digits()
		{
			AssertGetTextFromTimeOffset_3Digits("010:30", new ZDateTime(2006, 1, 1, 10, 30, 0));
			AssertGetTextFromTimeOffset_3Digits("010:03", new ZDateTime(2006, 1, 1, 10, 3, 0));
			AssertGetTextFromTimeOffset_3Digits("001:03", new ZDateTime(2006, 1, 1, 1, 3, 0));
			AssertGetTextFromTimeOffset_3Digits("000:01", new ZDateTime(2006, 1, 1, 0, 1, 0));
			AssertGetTextFromTimeOffset_3Digits("000:00", new ZDateTime(2006, 1, 1, 0, 0, 0));
			AssertGetTextFromTimeOffset_3Digits("023:59", new ZDateTime(2006, 1, 1, 23, 59, 0));
			AssertGetTextFromTimeOffset_3Digits("-000:01", new ZDateTime(2006, 1, 1).AddMinutes(-1));
			AssertGetTextFromTimeOffset_3Digits("-023:59", new ZDateTime(2006, 1, 1).AddHours(-23).AddMinutes(-59));

			AssertGetTextFromTimeOffset_3Digits("110:30", new ZDateTime(2006, 1, 1, 10, 30, 0).AddHours(100));
			AssertGetTextFromTimeOffset_3Digits("210:03", new ZDateTime(2006, 1, 1, 10, 3, 0).AddHours(200));
			AssertGetTextFromTimeOffset_3Digits("301:03", new ZDateTime(2006, 1, 1, 1, 3, 0).AddHours(300));
			AssertGetTextFromTimeOffset_3Digits("400:01", new ZDateTime(2006, 1, 1, 0, 1, 0).AddHours(400));
			AssertGetTextFromTimeOffset_3Digits("500:00", new ZDateTime(2006, 1, 1, 0, 0, 0).AddHours(500));
			AssertGetTextFromTimeOffset_3Digits("623:59", new ZDateTime(2006, 1, 1, 23, 59, 0).AddHours(600));
			AssertGetTextFromTimeOffset_3Digits("-700:01", new ZDateTime(2006, 1, 1).AddMinutes(-1).AddHours(-700));
			AssertGetTextFromTimeOffset_3Digits("-823:59", new ZDateTime(2006, 1, 1).AddHours(-23).AddMinutes(-59).AddHours(-800));
		}

		public void TestGetTextFromEmptyOrInvalidTimeOffset()
		{
			AssertEquals("GetTextFromTimeOffset(ZDateTime.Empty)", ZDateTime.Empty.ToString(), ZDateTimeGeneralHelper.GetTextFromTimeOffset(ZDateTime.Empty, 23, true));
			AssertEquals("GetTextFromTimeOffset(ZDateTime.Invalid)", ZDateTime.Invalid.ToString(), ZDateTimeGeneralHelper.GetTextFromTimeOffset(ZDateTime.Invalid, 23, true));
		}

		void AssertGetTextFromTimeOffset_2Digits(string expectedText, ZDateTime time)
		{
			AssertEquals(expectedText, ZDateTimeGeneralHelper.GetTextFromTimeOffset(time, 23, true));
		}

		void AssertGetTextFromTimeOffset_3Digits(string expectedText, ZDateTime time)
		{
			AssertEquals(expectedText, ZDateTimeGeneralHelper.GetTextFromTimeOffset(time, 999, true));
		}
	}
}
