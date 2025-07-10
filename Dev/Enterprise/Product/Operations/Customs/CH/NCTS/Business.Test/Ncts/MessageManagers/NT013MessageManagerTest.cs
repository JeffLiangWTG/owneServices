using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NT013MessageManagerTest : BasePassarDepartureMessageManagerTest<NT013MessageManager>
{
	protected override string MessageSendingObjectMessageType => PassarMessageTypeList.Codes.NT013;

	protected override string ExpectedMovementHeaderPhase => "013";
	protected override string ExpectedMessageSubType => "013";
	protected override Event ExpectedEventType => Events.DeclarationAmendmentSent;
	protected override string ExpectedEventReference => string.Empty;

	protected override NT013MessageManager CreateMessageManager(NctsHeader nctsHeader) => new NT013MessageManager(CreateMessageSendingObject(nctsHeader));

	[TestDate(2024, 11, 18)]
	public void TestBeforeGenerateMessageSetsValuationDate()
	{
		NctsHeader.MovementHeader.BM_Phase = ExpectedMovementHeaderPhase;

		var manager = CreateMessageManager(NctsHeader);
		_ = manager.GenerateMessages();

		AssertEquals(ZDateTime.Now, NctsHeader.MovementHeader.BM_ValuationDate);

		NctsHeader.MovementHeader.BM_ValuationDate = ZDateTime.Now.AddDays(-1);

		_ = manager.GenerateMessages();

		AssertEquals(ZDateTime.Now.AddDays(-1), NctsHeader.MovementHeader.BM_ValuationDate);
	}
}
