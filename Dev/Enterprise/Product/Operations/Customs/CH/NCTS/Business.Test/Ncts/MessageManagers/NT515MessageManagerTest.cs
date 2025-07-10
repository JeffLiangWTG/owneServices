using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT515MessageManagerTest : BasePassarDepartureMessageManagerTest<NT515MessageManager>
{
	protected override string MessageSendingObjectMessageType => PassarMessageTypeList.Codes.NT515;

	protected override string ExpectedMovementHeaderPhase => "015";
	protected override string ExpectedMessageSubType => "515";
	protected override Event ExpectedEventType => Events.CustomsCommenced;
	protected override string ExpectedEventReference => string.Empty;

	protected override NT515MessageManager CreateMessageManager(NctsHeader nctsHeader) => new NT515MessageManager(CreateMessageSendingObject(nctsHeader));

	[TestDate(2024, 11, 18)]
	public void TestBeforeGenerateMessageSetsValuationDate()
	{
		NctsHeader.MovementHeader.BM_Phase = ExpectedMovementHeaderPhase;

		var manager = CreateMessageManager(NctsHeader);
		_ = manager.GenerateMessages();

		AssertEquals(ZDateTime.Now, NctsHeader.MovementHeader.BM_ValuationDate);
	}
}
