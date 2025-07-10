using System.Text;
using NUnit.Framework;

namespace Enterprise.Messaging.Integration.Testing
{
	sealed class MessageEncodingTest : TestCase
	{
		public void TestUTF8WithoutBOM()
		{
			var encoding = MessageEncoding.UTF8WithoutBOM as UTF8Encoding;
			AssertEquals("encoding.GetPreamble()", System.Array.Empty<byte>(), encoding.GetPreamble());
		}
	}
}
