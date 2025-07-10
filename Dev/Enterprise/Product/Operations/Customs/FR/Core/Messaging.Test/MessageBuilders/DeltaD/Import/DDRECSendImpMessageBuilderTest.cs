using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Testing;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD.Testing
{
	class DDRECSendImpMessageBuilderTest : DDSendMessageBuilderTest<DDRECSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.REC;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Operateur><opedest>", "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>9</codact>", "<Motivation>", "<commentaire>." };

		public override void TestAeroportembAndAertratag()
		{
			Assert("REC message doesn't populate DSIComp where Aeroportemb and Aertratag exist.", true);
		}

		public void TestBuildWithEori()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(false, true, true);
			var message = deltaHelper.GetFlatMessage(MessageType, entry);
			AssertContains("<Operateur><opedest>FR32159700500065</opedest>", message);
		}

		public void TestPopulateDCSendMessageBuilderHeaderWithRectification()
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
