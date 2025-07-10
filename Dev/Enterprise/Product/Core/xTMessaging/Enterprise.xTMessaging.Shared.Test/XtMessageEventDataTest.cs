using System;
using NUnit.Framework;

namespace Enterprise.xTMessaging.Shared.Test
{
	class XtMessageEventDataTest : TestCase
	{
		public void TestEmptyJson()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new XtMessageEventData(1, 0, null));
		}

		public void TestSuccess()
		{
			var obj = new XtMessageEventData(1, 101, "{\"logevent\":\"2\",\"time\":\"2023-10-20T12:32:21Z\",\"logtext\":\"test logtext\"}");
			AssertEquals(101ul, obj.XtMsgId);
			AssertEquals(1, obj.EventIndex);
			AssertEquals(2, obj.LogEvent);
			AssertEquals("2023-10-20T12:32:21Z", obj.LogTime);
			AssertEquals("Sent to application: test logtext", obj.LogText);
		}

		public void TestWrongDataAndEmptyReturned()
		{
			var data = new XtMessageEventData(1, 101, "abcdefg");
			AssertEquals(null, data.LogText);
			AssertEquals(0, data.LogEvent);
			AssertEquals(1, data.EventIndex);
			AssertEquals(101ul, data.XtMsgId);
		}
	}
}
