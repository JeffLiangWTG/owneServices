using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing;

[TestedType(typeof(ErrorMessageProcessingStrategy))]
sealed class ErrorMessageProcessingStrategyTest : TestCaseWithFactory
{
	public void TestProcessMessage()
	{
		var header = Factory.New<CusEntryHeader>();
		var strategy = ((IErrorMessageProcessingStrategyParent)header).ProcessingStrategy;

		var message = Factory.New<EDIMessage>();
		message.EM_LinkedObject = header;

		strategy.ProcessMessage(message);
		AssertEquals(CustomsStatusList.Codes.Error, header.CustomsStatus);
		AssertEquals(CustomsStatusList.Codes.Error, header.MessageStatus);
		AssertEquals(EDIMessage.Status.ProcessedOK, message.EM_Status);
	}
}
