using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class DeltaGSendEAVMessageWhenStatusIs055Test : AutoSendCustomsMessageRuleTest<SendEAVMessageWhenStatusIs055>
	{
		protected override ZString ExpectedMessageType => EntryActionCodeList.Codes.EAV;

		protected override ZString EntryStatusThatCanSendMessage => EntryStatusDescriptionCodeList.Codes.ES055;

		protected override ZString EntryStatusThatCanNotSendMessage => EntryStatusDescriptionCodeList.Codes.ES050;
	}
}
