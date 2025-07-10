using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZDateTimeLabelTest : TestCase
	{
		public void TestDateTimeFormat()
		{
			AssertEquals("Default value", ZDateTimePickerFormat.Short, new ZDateTimeLabel().DateTimeFormat);
		}

		public void TestAcceptsZDateTimeOffset()
		{
			var dateTimeOffset = new ZDateTimeOffset(1979, 12, 12, 12, 12, 12, new System.TimeSpan(-6, 0, 0));

			AssertEquals("Correct label for ZDateTimeOffset", "12-Dec-79", new ZDateTimeLabel().GetText(dateTimeOffset));
		}

		public void TestAcceptsZDate()
		{
			var date = new ZDate(1979, 12, 12);

			AssertEquals("Correct label for ZDate", "12-Dec-79", new ZDateTimeLabel().GetText(date));
		}
	}
}
