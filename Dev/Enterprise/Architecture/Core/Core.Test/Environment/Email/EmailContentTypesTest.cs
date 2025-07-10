using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class EmailContentTypesTest : TestCase
	{
		public void TestEmailContentTypes()
		{
			AssertEquals("HTML Content type exists", typeof(HTMLContentType), EmailContentTypes.HTML.GetType());
			AssertEquals("Plain text Content type exists", typeof(PlainTextContentType), EmailContentTypes.PlainText.GetType());
			AssertEquals("Calendar Content type exists", typeof(CalendarContentType), EmailContentTypes.Calendar.GetType());

			AssertEquals("HTML Code", "HTM", EmailContentTypes.HTML.ContentTypeCode);
			AssertEquals("HTML Description", "HTML Document", EmailContentTypes.HTML.ContentTypeDescription);

			AssertEquals("Plain text Code", "PLN", EmailContentTypes.PlainText.ContentTypeCode);
			AssertEquals("Plain text Description", "Plain Text", EmailContentTypes.PlainText.ContentTypeDescription);

			AssertEquals("Calendar Code", "CAL", EmailContentTypes.Calendar.ContentTypeCode);
			AssertEquals("Calendar Description", "vCalendar", EmailContentTypes.Calendar.ContentTypeDescription);
		}
	}
}
