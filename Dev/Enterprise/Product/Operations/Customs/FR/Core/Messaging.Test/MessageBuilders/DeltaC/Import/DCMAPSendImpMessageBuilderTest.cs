using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaC.Testing
{
	public class DCMAPSendImpMessageBuilderTest : DCSendMessageBuilderTest<DCMAPSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => EntryActionCodeList.Codes.MAP;

		protected override ZString[] ItemsNotContains => new ZString[] { "<MetaData>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>3</codact>", "<Preval><datpreval>31/01/2029</datpreval><heurpreval>20:12</heurpreval></Preval>" };

		protected override void FillSomeFieldsForTheEntry(CusEntryHeader entry)
		{
			entry.EntryInstruction.CEI_DateForDuty = new ZDateTime(2029, 01, 31, 20, 12, 12);
		}

		public void TestPopulateDeclEmergencyProcDate()
		{
			HelpTestingPopulateDeclEmergencyProcDate();
		}
	}
}
