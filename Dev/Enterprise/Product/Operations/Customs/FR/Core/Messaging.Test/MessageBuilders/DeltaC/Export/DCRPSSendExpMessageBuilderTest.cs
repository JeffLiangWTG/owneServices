using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCRPSSendExpMessageBuilderTest : DCSendMessageBuilderTest<DCRPSSendExpMessageBuilder>
	{
		protected override ZBool IsImport => false;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.RPS;

		protected override ZString[] ItemsNotContains => new ZString[] { "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>9</codact>" };

		protected override void FillSomeFieldsForTheEntry(CusEntryHeader entry)
		{
			var rpsMessage = Factory.New<EDIMessage>();
			rpsMessage.EM_SystemCreateTimeUtc = new ZDateTime("01/01/2020");
			entry.Messages.Add(rpsMessage);
		}

		public override void TestAeroportembAndAertratag()
		{
			Assert("Export message doesn't populate Aeroportemb and Aertratag.", true);
		}
	}
}
