using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.Testing
{
	public class CustomsDateTimeExtensionTest : TestCaseWithFactory
	{
		public void TestToCustomsFormatString()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ToCustomsFormatString returns the correct format: yyMMdd", "201013", testDate.ToCustomsFormatString("yyMMdd"));
				AssertEquals("ToCustomsFormatString returns the correct format: MM/dd/yyyy", "10/13/2020", testDate.ToCustomsFormatString("MM/dd/yyyy"));
				AssertEquals("ToCustomsFormatString returns the correct format: dddd, dd MMMM yyyy HH:mm:ss", "Tuesday, 13 October 2020 14:30:20", testDate.ToCustomsFormatString("dddd, dd MMMM yyyy HH:mm:ss"));
				AssertEquals("ToCustomsFormatString returns the correct format: MM/dd/yyyy hh:mm tt", "10/13/2020 02:30 PM", testDate.ToCustomsFormatString("MM/dd/yyyy hh:mm tt"));
			});
		}

		public void TestToShortCustomsFormatDateString()
		{
			AssertEquals("ToShortCustomsFormatDateString returns the correct format: yyMMdd", "201013", testDate.ToShortCustomsFormatDateString());
		}

		public void TestToCustomsFormatTimeString()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ToCustomsFormatTimeString returns the correct format: HHmm", "1430", testDate.ToCustomsFormatTimeString());

				testDate = new ZDateTime(2020, 10, 13, 7, 30, 20);
				AssertEquals("ToCustomsFormatTimeString returns the correct format: HHmm", "0730", testDate.ToCustomsFormatTimeString());
			});
		}

		public void TestToLongCustomsFormatDateTimeString()
		{
			AssertEquals("ToLongCustomsFormatDateTimeString returns the correct format: yyyyMMddHHmm", "202010131430", testDate.ToLongCustomsFormatDateTimeString());
		}

		public void TestToCustomsFormatDateString()
		{
			AssertEquals("ToCustomsFormatDateString returns the correct format: yyyyMMdd", "20201013", testDate.ToCustomsFormatDateString());
		}

		protected override void SetUp()
		{
			base.SetUp();

			testDate = new ZDateTime(2020, 10, 13, 14, 30, 20);
		}
		ZDateTime testDate;
	}
}
