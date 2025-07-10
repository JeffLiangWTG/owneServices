using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Shared.Test;

namespace Enterprise.xTMessaging.Business.Test
{
	public class MessageCreatorFactoryTest : TestCaseWithFactory
	{
		public void TestGetMessageCreator()
		{
			var logger = new TestUtils.TestLogger();
			var interchange = Factory.New<EDIInterchange>();

			AssertExceptionThrown<ArgumentNullException>(() => MessageCreatorFactory.GetMessageCreator("", null, logger));
			AssertExceptionThrown<ArgumentNullException>(() => MessageCreatorFactory.GetMessageCreator("", interchange, null));

			AssertNull("No Handler is found", MessageCreatorFactory.GetMessageCreator("", interchange, logger));
			AssertNull("No Handler is found", MessageCreatorFactory.GetMessageCreator("XXX", interchange, logger));

			var messageCreator = MessageCreatorFactory.GetMessageCreator("XDC", interchange, logger);
			AssertNotNull("MessageCreator is found", messageCreator);
			AssertEquals("UniversalXmlMessageCreator returned", typeof(UniversalXmlMessageCreator), messageCreator.GetType());

			interchange.EI_ApplicationCode = "NDM";
			interchange.EI_InterchangeType = "XDC";
			messageCreator = MessageCreatorFactory.GetMessageCreator("", interchange, logger);
			AssertNotNull("MessageCreator is found", messageCreator);
			AssertEquals("UniversalXmlMessageCreator returned", typeof(UniversalXmlMessageCreator), messageCreator.GetType());

			interchange.EI_ApplicationCode = "UDM";
			messageCreator = MessageCreatorFactory.GetMessageCreator("", interchange, logger);
			AssertNotNull("MessageCreator is found", messageCreator);
			AssertEquals("UniversalXmlMessageCreator returned", typeof(UniversalXmlMessageCreator), messageCreator.GetType());
		}
	}
}
