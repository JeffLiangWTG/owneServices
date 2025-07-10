using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Testing;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD.Testing
{
	class DDRECSendExpMessageBuilderTest : DDSendMessageBuilderTest<DDRECSendExpMessageBuilder>
	{
		protected override ZBool IsImport => false;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.REC;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Destinatairefinal><tin>", "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>9</codact>", "<Motivation>", "<commentaire>." };

		public override void TestAeroportembAndAertratag()
		{
			Assert("Export message doesn't populate Aeroportemb and Aertratag.", true);
		}

		public void TestBuildWithEori()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(false, false, true);
			var message = deltaHelper.GetFlatMessage(MessageType, entry);
			AssertContains("<Destinatairefinal><tin>FR32159700500065</tin>", message);
		}

		public void TestPopulateBuilderHeaderWithRectification()
		{
			CreateDeclarationMock(MessageType, IsImport);

			var messageHeaderPart = @"<Entete><codact>10</codact><Motivation><motiv>Rectification explanation</motiv><commentaire>.</commentaire></Motivation><refdos>8461132</refdos></Entete>";
			var messageBuilder = CreateMessageBuilder(MessageType, IsImport);
			var message = messageBuilder.GetMessage();
			message = Extensions.GetFlatXml(message);
			AssertContains(messageHeaderPart, message);
		}
	}
}
