using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT015MessageManagerTest : BasePassarDepartureMessageManagerTest<NT015MessageManager>
{
	protected override string MessageSendingObjectMessageType => PassarMessageTypeList.Codes.NT015;

	protected override string ExpectedMovementHeaderPhase => "015";
	protected override string ExpectedMessageSubType => "015";
	protected override Event ExpectedEventType => Events.CustomsCommenced;
	protected override string ExpectedEventReference => string.Empty;

	protected override NT015MessageManager CreateMessageManager(NctsHeader nctsHeader) => new NT015MessageManager(CreateMessageSendingObject(nctsHeader));

	[TestDate(2024, 11, 18)]
	public void TestBeforeGenerateMessageSetsValuationDate()
	{
		NctsHeader.MovementHeader.BM_Phase = ExpectedMovementHeaderPhase;

		var manager = CreateMessageManager(NctsHeader);
		_ = manager.GenerateMessages();

		AssertEquals(ZDateTime.Now, NctsHeader.MovementHeader.BM_ValuationDate);
	}
}
