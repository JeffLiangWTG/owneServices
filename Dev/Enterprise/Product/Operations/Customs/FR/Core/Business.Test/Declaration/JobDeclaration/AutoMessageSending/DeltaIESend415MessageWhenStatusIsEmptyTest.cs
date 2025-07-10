using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class DeltaIESend415MessageWhenStatusIsEmptyTest : AutoSendCustomsMessageRuleTest<Send415MessageWhenStatusIsEmpty>
	{
		protected override ZString ExpectedMessageType => DeltaIESendMessageSubTypeList.Codes.ImportDeclaration;

		protected override ZString EntryStatusThatCanSendMessage => ZString.Empty;

		protected override ZString EntryStatusThatCanNotSendMessage => EntryStatusDescriptionCodeList.Codes.ES100;
	}
}
