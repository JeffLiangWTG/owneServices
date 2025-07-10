using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCCMPSendExpMessageBuilderTest : DCSendMessageBuilderTest<DCCMPSendExpMessageBuilder>
	{
		protected override ZBool IsImport => false;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.CMP;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Gen>", "<Articles>", "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>8</codact>" };

		public override void TestAeroportembAndAertratag()
		{
			Assert("Export message doesn't populate Aeroportemb and Aertratag.", true);
		}
	}
}
