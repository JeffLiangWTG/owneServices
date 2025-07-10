using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCINVSendImpMessageBuilderTest : DCSendMessageBuilderTest<DCINVSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.INV;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Gen>", "<Articles>", "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>7</codact>", "<Motivation>" };

		public override void TestAeroportembAndAertratag()
		{
			Assert("INV message doesn't populate <Gen> where Aeroportemb and Aertratag exist.", true);
		}
	}
}
