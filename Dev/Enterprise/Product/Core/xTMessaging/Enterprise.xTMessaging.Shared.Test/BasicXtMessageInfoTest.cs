using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Enterprise.xTMessaging.Shared.Test
{
	class BasicXtMessageInfoTest : TestCase
	{
		public void TestAllGood()
		{
			var reader = new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz1234567890")));
			var trackingId = Guid.NewGuid().ToString();
			var xtMessageInfo = new BasicXtMessageInfo("ABC", "MTT", "SRC", "DES", trackingId, reader);
			AssertEquals(xtMessageInfo.ApplicationCode, "ABC");
			AssertEquals(xtMessageInfo.MessageType, "MTT");
			AssertEquals(xtMessageInfo.SourceParty, "SRC");
			AssertEquals(xtMessageInfo.DestinationParty, "DES");
			AssertEquals(xtMessageInfo.MessageTrackingID, trackingId);

			AssertEquals(reader, xtMessageInfo.GetMessageData());
		}

		public void TestNullReader()
		{
			var exception = AssertExceptionThrown<ArgumentException>(() => _ = new BasicXtMessageInfo("ABC", "MTT", "SRC", "DES", Guid.NewGuid().ToString(), null));
			AssertEquals(exception.Message, "Value cannot be null.\r\nParameter name: Message content reader");
		}

		public void TestEmptyApplicationCode()
		{
			var reader = new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz1234567890")));
			var exception = AssertExceptionThrown<ArgumentNullException>(() => _ = new BasicXtMessageInfo("", "MTT", "SRC", "DES", Guid.NewGuid().ToString(), reader));
			AssertEquals(exception.Message, "Value cannot be null.\r\nParameter name: ApplicationCode");
		}

		public void TestEmptyMessageType()
		{
			var reader = new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz1234567890")));
			var exception = AssertExceptionThrown<ArgumentNullException>(() => _ = new BasicXtMessageInfo("ABC", "", "SRC", "DES", Guid.NewGuid().ToString(), reader));
			AssertEquals(exception.Message, "Value cannot be null.\r\nParameter name: MessageType");
		}

		public void TestEmptySourceParty()
		{
			var reader = new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz1234567890")));
			var exception = AssertExceptionThrown<ArgumentNullException>(() => _ = new BasicXtMessageInfo("ABC", "MTT", "", "DES", Guid.NewGuid().ToString(), reader));
			AssertEquals(exception.Message, "Value cannot be null.\r\nParameter name: SourceParty");
		}

		public void TestEmptyDestinationParty()
		{
			var reader = new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz1234567890")));
			var exception = AssertExceptionThrown<ArgumentNullException>(() => _ = new BasicXtMessageInfo("ABC", "MTT", "SRC", "", Guid.NewGuid().ToString(), reader));
			AssertEquals(exception.Message, "Value cannot be null.\r\nParameter name: DestinationParty");
		}

		public void TestEmptyMessageTrackingID()
		{
			var reader = new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz1234567890")));
			var exception = AssertExceptionThrown<ArgumentException>(() => _ = new BasicXtMessageInfo("ABC", "MTT", "SRC", "DES", "", reader));
			AssertEquals(exception.Message, "Value is not a valid GUID.\r\nParameter name: MessageTrackingID");
		}

		public void TestEmptyGUIDMessageTrackingID()
		{
			var reader = new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz1234567890")));
			var exception = AssertExceptionThrown<ArgumentException>(() => _ = new BasicXtMessageInfo("ABC", "MTT", "SRC", "DES", Guid.Empty.ToString(), reader));
			AssertEquals(exception.Message, "Value is not a valid GUID.\r\nParameter name: MessageTrackingID");
		}

		public void TestWithExtraProperty()
		{
			var reader = new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz1234567890")));
			var trackingId = Guid.NewGuid().ToString();
			var xtMessageInfo = new BasicXtMessageInfo("ABC", "MTT", "SRC", "DES", trackingId, reader);
			xtMessageInfo.XTMessageAttributes.Add("aaaaa","aaaaa");
			xtMessageInfo.XTMessageAttributes.Add("bbbbb", "bbbbb");

			AssertEquals(xtMessageInfo.XTMessageAttributes["aaaaa"], "aaaaa");
			AssertEquals(xtMessageInfo.XTMessageAttributes["bbbbb"], "bbbbb");

			var exception = AssertExceptionThrown<Exception>(() => _ = xtMessageInfo.XTMessageAttributes["ccccc"]);
			AssertEquals(exception.Message, "The given key was not present in the dictionary.");
		}
	}
}
