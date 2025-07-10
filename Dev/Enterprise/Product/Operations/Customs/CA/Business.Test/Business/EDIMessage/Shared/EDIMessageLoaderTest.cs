using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(EDIMessage.Loader))]
	sealed class EDIMessageLoaderTest : LoaderTestCase
	{
		public void TestLoadTop1WithDirection()
		{
			var message1 = Factory.New<EDIMessage>();
			message1.EM_ApplicationCode = "AAA";
			message1.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message1.EM_MessageNum = "123";

			var message2 = Factory.New<EDIMessage>();
			message2.EM_ApplicationCode = "AAA";
			message2.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			message2.EM_MessageNum = "123";

			AssertEquals(message1, new EDIMessage.Loader(Factory).LoadTop1WithDirection("AAA", "123", EDIInterchange.Direction.Receive));
			AssertEquals(message2, new EDIMessage.Loader(Factory).LoadTop1WithDirection("AAA", "123", EDIInterchange.Direction.Transmit));
		}

		public void TestLoad()
		{
			var message1 = Factory.New<EDIMessage>();
			message1.EM_ApplicationCode = "AAA";
			message1.EM_MessageType = "TTT";
			message1.EM_MessageNum = "123";

			var message2 = Factory.New<EDIMessage>();
			message2.EM_ApplicationCode = "AAA";
			message2.EM_MessageType = "TTT";
			message2.EM_MessageNum = "234";

			AssertEquals(message1, new EDIMessage.Loader(Factory).LoadTop1("AAA", "TTT", "123"));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new EDIMessage.Loader(Factory);
		}
	}
}
