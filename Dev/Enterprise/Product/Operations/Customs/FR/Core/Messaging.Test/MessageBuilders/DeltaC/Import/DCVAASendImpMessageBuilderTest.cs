using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCVAASendImpMessageBuilderTest : DCSendMessageBuilderTest<DCVAASendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.VAA;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Gen>", "<Articles>", "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>5</codact>", "<Declaration><Liquidations>" };

		public override void TestAeroportembAndAertratag()
		{
			Assert("VAA message doesn't populate <Gen> where Aeroportemb and Aertratag exist.", true);
		}
	}
}
