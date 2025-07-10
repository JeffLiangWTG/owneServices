using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCANASendImpMessageBuilderTest : DCSendMessageBuilderTest<DCANASendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.ANA;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Gen>", "<Articles>", "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>4</codact>" };

		public override void TestAeroportembAndAertratag()
		{
			Assert("ANA message doesn't populate <Gen> where Aeroportemb and Aertratag exist.", true);
		}
	}
}
