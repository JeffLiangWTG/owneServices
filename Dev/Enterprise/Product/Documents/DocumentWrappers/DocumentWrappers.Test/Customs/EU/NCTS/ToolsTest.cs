using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS.Testing
{
	sealed class ToolsTest : TestCase
	{
		public void TestGetNotReleasedCaption()
		{
			AssertEquals("GetNotReleasedCaption()", "NOT RELEASED", Tools.GetNotReleasedCaption());
		}

		public void TestToEuShortDateString()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ToEuShortDateString() for a valid date", "06/05/2022", new ZDateTime(2022, 05, 06).ToEuShortDateString());
				AssertEquals("ToEuShortDateString() for an empty date", "", ZDateTime.Empty.ToEuShortDateString());
				AssertEquals("ToEuShortDateString() for an invalid date", "", ZDateTime.Invalid.ToEuShortDateString());
			});
		}
	}
}
