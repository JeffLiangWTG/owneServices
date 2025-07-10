using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCINVSendExpMessageBuilderTest : DCSendMessageBuilderTest<DCINVSendExpMessageBuilder>
	{
		protected override ZBool IsImport => false;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.INV;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Gen>", "<Articles>", "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>7</codact>", "<Motivation>" };

		public override void TestAeroportembAndAertratag()
		{
			Assert("Export message doesn't populate Aeroportemb and Aertratag.", true);
		}
	}
}
