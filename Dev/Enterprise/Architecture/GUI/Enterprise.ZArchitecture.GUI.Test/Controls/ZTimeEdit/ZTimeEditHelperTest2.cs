using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZTimeEditHelperTest2 : TestCase
	{
		public virtual void TestGetTextFromTime()
		{
			AssertGetTextFromTime("10:30", new ZDateTime(2006, 1, 1, 10, 30, 0));
			AssertGetTextFromTime("10:03", new ZDateTime(2006, 1, 1, 10, 3, 0));
			AssertGetTextFromTime("01:03", new ZDateTime(2006, 1, 1, 1, 3, 0));
			AssertGetTextFromTime("00:01", new ZDateTime(2006, 1, 1, 0, 1, 0));
			AssertGetTextFromTime("00:00", new ZDateTime(2006, 1, 1, 0, 0, 0));
			AssertGetTextFromTime("23:59", new ZDateTime(2006, 1, 1, 23, 59, 0));
			AssertGetTextFromTime("-00:01", new ZDateTime(2006, 1, 1).AddMinutes(-1));
			AssertGetTextFromTime("-23:59", new ZDateTime(2006, 1, 1).AddHours(-23).AddMinutes(-59));
		}

		public void TestGetTextFromEmptyOrInvalidTime()
		{
			AssertEquals("GetTextFromTime(ZDateTime.Empty)", ZDateTime.Empty.ToString(), TimeHelper.GetTextFromTime(ZDateTime.Empty, true));
			AssertEquals("GetTextFromTime(ZDateTime.Invalid)", ZDateTime.Invalid.ToString(), TimeHelper.GetTextFromTime(ZDateTime.Invalid, true));
		}

		protected virtual ZTimeEditHelper TimeHelper
		{
			get { return new ZTimeEditHelper(); }
		}

		public void AssertGetTextFromTime(string expectedText, ZDateTime time)
		{
			if (TimeHelper.MaximumHours.ToString().Length == 3)
			{
				expectedText = expectedText.PadLeft(6, '0');
			}

			AssertEquals(expectedText, TimeHelper.GetTextFromTime(time, true));
		}
	}
}
