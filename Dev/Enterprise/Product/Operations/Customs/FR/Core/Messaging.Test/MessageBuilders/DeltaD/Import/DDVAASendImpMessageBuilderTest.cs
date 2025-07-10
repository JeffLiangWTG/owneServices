using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD.Testing
{
	class DDVAASendImpMessageBuilderTest : DDSendMessageBuilderTest<DDVAASendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.VAA;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Preval>", "<Articles>", "<MetaData>", "<Motivation>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>7</codact>", "<Declaration><DSI><Entete>" };

		public override void TestAeroportembAndAertratag()
		{
			Assert("VAA message doesn't populate DSIComp where Aeroportemb and Aertatag exist.", true);
		}
	}
}
