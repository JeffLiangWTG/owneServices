using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	public class MessageCreatorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCreateMessage()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.CustomsWare;
			interchange.EI_BodyText = "BODYTEXT";
			interchange.EI_From = "from";
			interchange.EI_To = "to";
			IInboundMessageCreator messageCreator = new MessageCreator();
			messageCreator.CreateMessagesForInterchange(interchange);
			Factory.Save();
			NUnit.Framework.Assert.That(interchange.ContainedMessages.Count, Is.EqualTo(1));
			var message = interchange.ContainedMessages[0];
			NUnit.Framework.Assert.That(message.EM_ApplicationCode, Is.EqualTo(interchange.EI_ApplicationCode));
			NUnit.Framework.Assert.That(message.EM_MessageType, Is.EqualTo(ApplicationCodeList.Codes.CustomsWare).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.EM_MessageText, Is.EqualTo("BODYTEXT").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.EM_Status, Is.EqualTo(EDIMessageStatusList.Codes.Queued).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.EM_ReceiveTransmit, Is.EqualTo(EDIInterchange.Direction.Receive).Using(CustomComparers.TypeComparison));
		}
	}
}
