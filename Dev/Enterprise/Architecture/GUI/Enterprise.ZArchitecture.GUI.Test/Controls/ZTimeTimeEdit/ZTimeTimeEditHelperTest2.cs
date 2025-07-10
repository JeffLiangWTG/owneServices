using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZTimeTimeEditHelperTest2 : TestCase
	{
		public virtual void TestGetTextFromTime()
		{
			AssertGetTextFromTime("10:30", new ZTime(10, 30));
			AssertGetTextFromTime("10:03", new ZTime(10, 3));
			AssertGetTextFromTime("01:03", new ZTime(1, 3));
			AssertGetTextFromTime("00:01", new ZTime(0, 1));
			AssertGetTextFromTime("00:00", new ZTime(0, 0));
			AssertGetTextFromTime("23:59", new ZTime(23, 59));
		}

		public void TestGetTextFromEmptyOrInvalidTime()
		{
			AssertEquals("GetTextFromTime(ZTime.Empty)", ZTime.Empty.ToString(), TimeHelper.GetTextFromTime(ZTime.Empty, true));
			AssertEquals("GetTextFromTime(ZTime.Invalid)", ZTime.Invalid.ToString(), TimeHelper.GetTextFromTime(ZTime.Invalid, true));
		}

		protected virtual ZTimeTimeEditHelper TimeHelper
		{
			get { return new ZTimeTimeEditHelper(); }
		}

		public void AssertGetTextFromTime(string expectedText, ZTime time)
		{
			AssertEquals(expectedText, TimeHelper.GetTextFromTime(time, true));
		}
	}
}
