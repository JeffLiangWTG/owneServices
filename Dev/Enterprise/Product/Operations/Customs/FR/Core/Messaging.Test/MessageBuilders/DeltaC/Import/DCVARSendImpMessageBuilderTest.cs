using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCVARSendImpMessageBuilderTest : DCSendMessageBuilderTest<DCVARSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.VAR;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Gen>", "<Articles>", "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>11</codact>", "<Declaration><Liquidations>" };

		public override void TestAeroportembAndAertratag()
		{
			Assert("VAR message doesn't populate <Gen> where Aeroportemb and Aertratag exist.", true);
		}
	}
}
