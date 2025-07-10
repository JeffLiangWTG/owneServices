using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.HttpXmlMessaging;
using Enterprise.xTMessaging.Shared.Test;

namespace Enterprise.xTMessaging.Business.Test
{
	public class EDIMessageCreatorTest : TestCaseWithFactory
	{
		public void TestContructorParameters()
		{
			var logger = new TestUtils.TestLogger();
			var exception = AssertExceptionThrown<ArgumentNullException>(() => new MessageCreatorForTest(null, logger));
			AssertContains("Exception related to Parameter - Interchange", "Value cannot be null.\r\nParameter name: interchange", exception.Message);

			exception = AssertExceptionThrown<ArgumentNullException>(() => new MessageCreatorForTest(DummyInterchange, null));
			AssertContains("Exception related to Parameter - Logger", "Value cannot be null.\r\nParameter name: logger", exception.Message);
		}
		public void TestCreateEDIMessage()
		{
			var logger = new TestUtils.TestLogger();
			var messageCreator = new MessageCreatorForTest(DummyInterchange, logger);
			var messageCreated = messageCreator.CreateEDIMessageForTest(Factory);
			AssertNotNull("EDIMessage should be created", messageCreated);

			CombineAssertions(() =>
			{
				AssertEquals("Type of EDIMessage", typeof(HttpXmlEDIMessage), messageCreated.GetType());
				AssertEquals("EM_EI", DummyInterchange.PK, messageCreated.EM_EI);
				AssertEquals("EM_GB", DummyInterchange.EI_GB, messageCreated.EM_GB);
				AssertEquals("EM_GE", GlbDepartment.CurrentDepartment.PK, messageCreated.EM_GE);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, messageCreated.EM_Status);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, messageCreated.EM_ReceiveTransmit);
				AssertEquals("EM_ApplicationCode", DummyInterchange.EI_ApplicationCode, messageCreated.EM_ApplicationCode);
				AssertEquals("EM_MessageType", DummyInterchange.EI_InterchangeType, messageCreated.EM_MessageType);
				AssertEquals("EM_MessageSubType", DummyInterchange.EI_InterchangeType, messageCreated.EM_MessageSubType);
			}
			);

			messageCreated = messageCreator.CreateEDIMessageForTest(
				Factory,
				() => "IEC",
				() => "SYS",
				() => "XXX"
				);

			CombineAssertions(() =>
			{
				AssertEquals("Type of EDIMessage", typeof(HttpXmlEDIMessage), messageCreated.GetType());
				AssertEquals("EM_EI", DummyInterchange.PK, messageCreated.EM_EI);
				AssertEquals("EM_GB", DummyInterchange.EI_GB, messageCreated.EM_GB);
				AssertEquals("EM_GE", GlbDepartment.CurrentDepartment.PK, messageCreated.EM_GE);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, messageCreated.EM_Status);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, messageCreated.EM_ReceiveTransmit);
				AssertEquals("EM_ApplicationCode", "IEC", messageCreated.EM_ApplicationCode);
				AssertEquals("EM_MessageType", "SYS", messageCreated.EM_MessageType);
				AssertEquals("EM_MessageSubType", "XXX", messageCreated.EM_MessageSubType);
			}
		);
		}

		EDIInterchange DummyInterchange
		{
			get
			{
				if (dummyInterchange == null)
				{
					dummyInterchange = Factory.NewWithValidTestData<EDIInterchange>();
					dummyInterchange.EI_ApplicationCode = "UDM";
					dummyInterchange.EI_InterchangeType = "XDC";
					dummyInterchange.EI_GB = GlbBranch.CurrentBranch.PK;
				}
				return dummyInterchange;
			}
		}
		EDIInterchange dummyInterchange;

		class MessageCreatorForTest : EDIMessageCreator
		{
			public MessageCreatorForTest(EDIInterchange interchange, ILogger logger) : base(interchange, logger)
			{ }

			protected override ZString CreateEDIMessagesForInterchangeCore(Stream payload, BusinessObjectFactory factory) => "";

			public EDIMessage CreateEDIMessageForTest(BusinessObjectFactory factory, Func<ZString> getApplicationCode = null, Func<ZString> getMessageType = null, Func<ZString> getMessageSubType = null)
			{
				return base.CreateEDIMessage(factory, getApplicationCode, getMessageType, getMessageSubType);
			}

			protected override Type EDIMessageType => typeof(HttpXmlEDIMessage);
		}
	}
}
