using Enterprise.Customs.IT.Messaging.SAD;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class SadMessageFixedPartFactoryTest : TestCase
{
	public void TestFactoryMethod()
	{
		var fallbackProcedureMessageSendingObject = new Mock<ISadMessageSendingObject>();
		fallbackProcedureMessageSendingObject.Setup(m => m.FallbackProcedure).Returns(true);
		fallbackProcedureMessageSendingObject.Setup(m => m.DeclarantTaxNumber).Returns("");

		var fallbackProcedureFixedPart = SadMessageFixedPartFactory.GetSadMessageFixedPart(false, fallbackProcedureMessageSendingObject.Object, "", "", 0);
		AssertType<SadMessageFallbackProcedureFixedPart>("Expected fallback procedure fixed part", fallbackProcedureFixedPart);

		var standardProcedureMessageSendingObject = new Mock<ISadMessageSendingObject>();
		standardProcedureMessageSendingObject.Setup(m => m.FallbackProcedure).Returns(false);
		standardProcedureMessageSendingObject.Setup(m => m.DeclarantTaxNumber).Returns("");
		var standardProcedureFixedPart = SadMessageFixedPartFactory.GetSadMessageFixedPart(false, standardProcedureMessageSendingObject.Object, "", "", 0);
		AssertType<SadMessageFixedPart>("Expected 'standard' procedure fixed part", standardProcedureFixedPart);
	}
}
