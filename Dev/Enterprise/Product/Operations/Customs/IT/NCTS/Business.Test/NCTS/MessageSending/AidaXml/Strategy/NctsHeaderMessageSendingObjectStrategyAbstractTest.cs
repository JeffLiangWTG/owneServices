using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

abstract class NctsHeaderMessageSendingObjectStrategyAbstractTest<T> : TestCaseWithFactory
	where T : NctsHeaderMessageSendingObjectStrategy
{
	public void TestDoesStatusAllowSending()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;

		var nctsHeaderMessageSendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
		nctsHeaderMessageSendingObject.MessageType = GetMessageType();
		var strategy = NctsHeaderMessageSendingObjectStrategyFactory.CreateStrategy(nctsHeaderMessageSendingObject);
		AssertType(typeof(T), strategy);

		CombineAssertions("When NCTS5 Departure", () =>
		{
			var scenarios = GetTestCases();

			foreach (var scenario in scenarios)
			{
				nctsHeader.MovementHeader.BM_CustomsStatus = scenario.DepartureStatus;
				nctsHeader.MovementHeader.BM_MessageStatus = scenario.MessageStatus;
				nctsHeader.MovementHeader.BM_Phase = scenario.PhaseStatus;
				var allowsSendingExpected = scenario.AllowsSendingExpected;

				AssertDoesStatusAllowSending(nctsHeaderMessageSendingObject, strategy, allowsSendingExpected);
			}
		});
	}

	void AssertDoesStatusAllowSending(NctsHeaderMessageSendingObject nctsHeaderMessageSendingObject, INctsHeaderMessageSendingObjectStrategy strategy, bool expectedResult)
	{
		var movementHeader = nctsHeaderMessageSendingObject.NctsHeader.MovementHeader;

		AssertEquals($"When MessageType: '{nctsHeaderMessageSendingObject.MessageType}', " +
			$"BM_CustomsStatus: '{movementHeader.BM_CustomsStatus}', " +
			$"BM_MessageStatus: '{movementHeader.BM_MessageStatus}', " +
			$"BM_Phase: '{movementHeader.BM_Phase}' " +
			$"DoesStatusAllowSending", expectedResult, strategy.DoesStatusAllowSending());
	}

	protected abstract string GetMessageType();
	protected abstract List<(string PhaseStatus, string MessageStatus, string DepartureStatus, bool AllowsSendingExpected)> GetTestCases();

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
	}

	NctsHeader nctsHeader;
}
