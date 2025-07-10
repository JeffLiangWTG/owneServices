using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD.Testing
{
	class DDINVSendImpMessageBuilderTest : DDSendMessageBuilderTest<DDINVSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.INV;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Gen>", "<Articles>", "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>10</codact>", "<Motivation>", "<commentaire>." };

		public override void TestAeroportembAndAertratag()
		{
			Assert("INV message doesn't populate DSIComp where Aeroportemb and Aertratag exist.", true);
		}
	}
}
