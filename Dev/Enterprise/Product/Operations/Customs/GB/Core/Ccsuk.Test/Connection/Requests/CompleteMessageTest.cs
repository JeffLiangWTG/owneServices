using System.Text;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.Connection.Testing
{
	class CompleteMessageTest : TestCase
	{
		public void TestHeader()
		{
			var body = new CargoMessage("<foo>\u00A0m³</foo>");
			AssertEquals("Pre-req (sanity): Characterwise length of body", 14, body.PayloadAsString.Length);
			var bytes = Encoding.UTF8.GetBytes(body.PayloadAsString);
			AssertEquals("Pre-req (sanity): Bytewise length of body", 16, bytes.Length);

			var message = new CompleteMessage(body);
			AssertEquals("05210", message.Header);
		}
	}
}
