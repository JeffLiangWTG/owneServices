using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Testing;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD.Testing
{
	class DDMDASendExpMessageBuilderTest : DDSendMessageBuilderTest<DDMDASendExpMessageBuilder>
	{
		protected override ZBool IsImport => false;

		protected override ZString MessageType => EntryActionCodeList.Codes.MDA;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Destinatairefinal><tin>", "<MetaData>", "<Motivation>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>6</codact>", "<Preval><datpreval>31/01/2029</datpreval><heurpreval>20:12</heurpreval></Preval>" };

		protected override void FillSomeFieldsForTheEntry(CusEntryHeader entry)
		{
			entry.EntryInstruction.CEI_DateForDuty = new ZDateTime(2029, 01, 31, 20, 12, 12);
		}

		public override void TestAeroportembAndAertratag()
		{
			Assert("Export message doesn't populate Aeroportemb and Aertratag.", true);
		}

		public void TestBuildWithEori()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();
			var entry = deltaHelper.CreateEntryDeclarationForTest(false, false, true);
			entry.Declaration.JE_ExportDate = new ZDateTime("31/01/2029");
			var message = deltaHelper.GetFlatMessage(MessageType, entry);
			AssertContains("<Destinatairefinal><tin>FR32159700500065</tin>", message);
		}

		public void TestPopulateDeclEmergencyProcDate()
		{
			HelpTestingPopulateDeclEmergencyProcDate();
		}
	}
}
