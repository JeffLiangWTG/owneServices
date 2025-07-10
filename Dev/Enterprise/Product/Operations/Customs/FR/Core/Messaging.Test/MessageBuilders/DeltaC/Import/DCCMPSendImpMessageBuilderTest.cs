using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCCMPSendImpMessageBuilderTest : DCSendMessageBuilderTest<DCCMPSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.CMP;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Gen>", "<Articles>", "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>8</codact>" };

		public override void TestAeroportembAndAertratag()
		{
			Assert("CMP message doesn't populate <Gen> where Aeroportemb and Aertratag exist.", true);
		}
	}
}
