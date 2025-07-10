using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

abstract class TemporaryStorageMessageSendingObjectStrategyAbstractTest<T> : TestCaseWithFactory
	where T : TemporaryStorageMessageSendingObjectStrategy
{
	public void TestDoesStatusAllowSending()
	{
		var tempStorageMessageSendingObject = new TemporaryStorageMessageSendingObject(tempHeader);
		tempStorageMessageSendingObject.MessageType = GetMessageType();
		var strategy = TemporaryStorageMessageSendingObjectStrategyFactory.CreateStrategy(tempStorageMessageSendingObject);
		AssertType(typeof(T), strategy);

		var scenarios = GetTestCases();

		foreach (var scenario in scenarios)
		{
			tempHeader.CustomsStatus = scenario.CustomsStatus;
			tempHeader.AMA_MessageStatus = scenario.MessageStatus;
			var allowsSendingExpected = scenario.AllowsSendingExpected;

			AssertDoesStatusAllowSending(tempStorageMessageSendingObject, strategy, allowsSendingExpected);
		}
	}

	void AssertDoesStatusAllowSending(TemporaryStorageMessageSendingObject tempStorageMessageSendingObject, ITemporaryStorageMessageSendingObjectStrategy strategy, bool expectedResult)
	{
		AssertEquals($"When MessageType: '{tempStorageMessageSendingObject.MessageType}', " +
			$"CustomsStatus: '{tempHeader.CustomsStatus}', " +
			$"MessageStatus: '{tempHeader.AMA_MessageStatus}', " +
			$"DoesStatusAllowSending", expectedResult, strategy.DoesStatusAllowSending());
	}

	protected abstract string GetMessageType();
	protected abstract List<(string CustomsStatus, string MessageStatus, bool AllowsSendingExpected)> GetTestCases();

	protected override void SetUp()
	{
		base.SetUp();
		tempHeader = Factory.New<TemporaryStorageHeader>();
	}

	TemporaryStorageHeader tempHeader;
}
