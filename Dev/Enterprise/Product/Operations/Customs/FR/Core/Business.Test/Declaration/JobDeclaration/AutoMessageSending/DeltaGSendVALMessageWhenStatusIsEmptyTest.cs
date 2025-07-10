using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class DeltaGSendVALMessageWhenStatusIsEmptyTest : AutoSendCustomsMessageRuleTest<SendVALMessageWhenStatusIsEmpty>
	{
		protected override ZString ExpectedMessageType => EntryActionCodeList.Codes.VAL;

		protected override ZString EntryStatusThatCanSendMessage => ZString.Empty;

		protected override ZString EntryStatusThatCanNotSendMessage => EntryStatusDescriptionCodeList.Codes.ES100;
	}
}
