using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Testing;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD.Testing
{
	class DDMDVSendImpMessageBuilderTest : DDSendMessageBuilderTest<DDMDVSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.MDV;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Operateur><opedest>", "<MetaData>", "<Motivation>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>4</codact>" };

		protected override void FillSomeFieldsForTheEntry(CusEntryHeader entry)
		{
			entry.Declaration.JE_DateOfArrival = new ZDateTime("31/01/2029");
		}

		public override void TestAeroportembAndAertratag()
		{
			Assert("MDV message doesn't populate DSIComp where Aeroportemb and Aertratag exist.", true);
		}

		public void TestBuildWithEori()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(false, true, true);
			entry.Declaration.JE_DateOfArrival = new ZDateTime("31/01/2029");
			var message = deltaHelper.GetFlatMessage(MessageType, entry);
			AssertContains("<Operateur><opedest>FR32159700500065</opedest>", message);
		}
	}
}
