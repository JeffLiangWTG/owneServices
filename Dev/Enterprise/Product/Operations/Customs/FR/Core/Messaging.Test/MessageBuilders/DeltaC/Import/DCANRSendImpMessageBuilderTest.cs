using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCANRSendImpMessageBuilderTest : DCSendMessageBuilderTest<DCANRSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.ANR;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Gen>", "<Articles>", "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>12</codact>" };

		public override void TestAeroportembAndAertratag()
		{
			Assert("ANR message doesn't populate <Gen> where Aeroportemb and Aertratag exist.", true);
		}
	}
}
