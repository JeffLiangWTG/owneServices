using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Messaging.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DeltaD.Testing
{
	class DDRPSSendImpMessageBuilderTest : DDSendMessageBuilderTest<DDRPSSendImpMessageBuilder>
	{
		protected override ZBool IsImport => true;

		protected override ZString MessageType => Business.EntryActionCodeList.Codes.RPS;

		protected override ZString[] ItemsNotContains => new ZString[] { "<Operateur><opedest>", "<MetaData>", "<Motivation>" };

		protected override ZString[] ItemsContains => new ZString[] { "<Entete><codact>5</codact>", "<datdepot>" };

		protected override void FillSomeFieldsForTheEntry(CusEntryHeader entry)
		{
			var rpsMessage = Factory.New<EDIMessage>();
			rpsMessage.EM_SystemCreateTimeUtc = new ZDateTime("01/01/2020");
			entry.Messages.Add(rpsMessage);
		}

		public override void TestAeroportembAndAertratag()
		{
			Assert("RPS message doesn't populate DSIComp where Aeroportemb and Aertatag exist.", true);
		}

		public void TestBuildWithEori()
		{
			var deltaHelper = new DeltaMessageBuilderHelper();

			var entry = deltaHelper.CreateEntryDeclarationForTest(false, true, true);

			var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			sendingObject.MessageType = MessageType;

			var rpsMessage = Factory.New<EDIMessage>();
			rpsMessage.EM_SystemCreateTimeUtc = new ZDateTime("01/01/2020");
			sendingObject.Header.Messages.Add(rpsMessage);

			var errCollector = new EU.Business.ErrorCollector();
			var messageBuilderManager = new DeltaGMessageBuilderManager(errCollector);
			var messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
			var rpsFinalMessage = messageBuilder.GetMessage();

			AssertContains("<Operateur><opedest>FR32159700500065</opedest>", rpsFinalMessage);
		}
	}
}
