using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class EntryActionHelperTest : TestCaseWithFactory
	{
		public void TestCanTriggerBilling()
		{
			var baes = new[]
			{
				EntryStatusDescriptionCodeList.Codes.ES100,
				EntryStatusDescriptionCodeList.Codes.ES101,
				EntryStatusDescriptionCodeList.Codes.ES130
			};

			foreach (var status in baes)
			{
				AssertEquals(true, EntryActionHelper.IsStatusClear(status));
			}
			foreach (var status in new EntryStatusDescriptionCodeList().GetAllCodes().Except(baes))
			{
				AssertEquals(false, EntryActionHelper.IsStatusClear(status));
			}
		}

		public void TestEntrySnapshotMustBeUpdated()
		{
			var message1 = Factory.New<DeltaDImportFREDIMessage>();
			message1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message1.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportREFResponseMessage.xml");

			AssertEquals(false, EntryActionHelper.IsRectificationAccepted(message1));
			AssertEquals("Snapshot must not be updated because reply to amendment request is negative or entry status >= 130.", false, EntryActionHelper.EntrySnapshotMustBeUpdated(message1, EntryStatusDescriptionCodeList.Codes.ES120));
			AssertEquals("Snapshot must not be updated because reply to amendment request is negative or entry status >= 130.", false, EntryActionHelper.EntrySnapshotMustBeUpdated(message1, EntryStatusDescriptionCodeList.Codes.ES130));

			var message2 = Factory.New<DeltaDImportFREDIMessage>();
			message2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message2.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportACCResponseMessage.xml");
			AssertEquals(true, EntryActionHelper.IsRectificationAccepted(message2));
			AssertEquals("Snapshot must not be updated because reply to amendment request is negative or entry status >= 130.", false, EntryActionHelper.EntrySnapshotMustBeUpdated(message2, EntryStatusDescriptionCodeList.Codes.ES130));
			AssertEquals("Snapshot must be updated because reply to amendment request is positive and entry status < 130.", true, EntryActionHelper.EntrySnapshotMustBeUpdated(message2, EntryStatusDescriptionCodeList.Codes.ES120));
		}

		public void TestEntryVALPendingSnapshotsMustBeConfirmed()
		{
			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");
			AssertEquals("Snapshot must not be confirmed because message doesn't grant VAL status", false, EntryActionHelper.EntryVALPendingSnapshotsMustBeConfirmed(message));

			var message2 = Factory.New<DeltaCImportFREDIMessage>();
			message2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message2.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			AssertEquals("Snapshot must not be confirmed because message doesn't grant VAL status", false, EntryActionHelper.EntryVALPendingSnapshotsMustBeConfirmed(message2));

			var message3 = Factory.New<DeltaCImportFREDIMessage>();
			message3.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message3.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportValidResponseMessage.xml");
			AssertEquals("Snapshot must be confirmed because message is a postive reply to VAL request", true, EntryActionHelper.EntryVALPendingSnapshotsMustBeConfirmed(message3));

			message3.EM_MessageText = message3.EM_MessageText.Replace("VALIDE", "VAL");
			AssertEquals("Snapshot must be confirmed because message is a postive reply to VAL request", true, EntryActionHelper.EntryVALPendingSnapshotsMustBeConfirmed(message3));

			message3.EM_MessageText = message3.EM_MessageText.Replace("VAL", "VALIDE-CONTINGENT-CRITIQUE");
			AssertEquals("Snapshot must be confirmed because message is a postive reply to VAL request", true, EntryActionHelper.EntryVALPendingSnapshotsMustBeConfirmed(message3));
		}

		public void TestHasVariousBAEStatus()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals(false, EntryActionHelper.HasVariousBAEStatus(entryHeader));
			entryHeader.Logs.AddNew(Events.CustomsEntryStatus, EntryStatusDescriptionCodeList.Codes.ES060);
			AssertEquals(false, EntryActionHelper.HasVariousBAEStatus(entryHeader));
		}

		public void TestIsVariousBAEStatus()
		{
			var baes = new[]
			{
				EntryStatusDescriptionCodeList.Codes.ES100,
				EntryStatusDescriptionCodeList.Codes.ES101
			};
			foreach (var status in baes)
			{
				AssertEquals(true, EntryActionHelper.IsVariousBAEStatus(status));
			}
			foreach (var status in new EntryStatusDescriptionCodeList().GetAllCodes().Except(baes))
			{
				AssertEquals(false, EntryActionHelper.IsVariousBAEStatus(status));
			}
		}

		public void TestIsOriginalClear()
		{
			var entry = Factory.New<CusEntryHeader>();

			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_InterchangeNum = "424";
			var outgoingMessage = entry.Messages.AddNew();
			outgoingMessage.EM_EI = interchange1.PK;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.INV;

			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_InterchangeNum = "424.";
			var incomingMessage = Factory.New<FREDIMessage>();
			entry.Messages.Add(incomingMessage);
			incomingMessage.EM_EI = interchange2.PK;
			incomingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			var baes = new[]
			{
				EntryStatusDescriptionCodeList.Codes.ES100,
				EntryStatusDescriptionCodeList.Codes.ES101
			};

			foreach (var status in baes)
			{
				outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.ANA;
				AssertEquals("Entry action is not original clear", false, EntryActionHelper.IsOriginalClear(status, EntryActionHelper.GetEntryAction(entry, incomingMessage)));
				outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.VAA;
				AssertEquals("Only true when status is BAE and entry action is original clear", true, EntryActionHelper.IsOriginalClear(status, EntryActionHelper.GetEntryAction(entry, incomingMessage)));
				outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.EAV;
				AssertEquals("Only true when status is BAE and entry action is original clear", true, EntryActionHelper.IsOriginalClear(status, EntryActionHelper.GetEntryAction(entry, incomingMessage)));
				outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.VAL;
				AssertEquals("Only true when status is BAE and entry action is original clear", true, EntryActionHelper.IsOriginalClear(status, EntryActionHelper.GetEntryAction(entry, incomingMessage)));
				Assert("Transaction must be confirmed when status is BAE and entry action is original clear", EntryActionHelper.TransactionMustBeConfirmed(status, entry, incomingMessage));
			}
			foreach (var status in new EntryStatusDescriptionCodeList().GetAllCodes().Except(baes))
			{
				outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.ANA;
				AssertEquals("Status is not BAE and entry action is not original clear", false, EntryActionHelper.IsOriginalClear(status, EntryActionHelper.GetEntryAction(entry, incomingMessage)));
				outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.VAA;
				AssertEquals("Status is not BAE", false, EntryActionHelper.IsOriginalClear(status, EntryActionHelper.GetEntryAction(entry, incomingMessage)));
				outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.EAV;
				AssertEquals("Status is not BAE", false, EntryActionHelper.IsOriginalClear(status, EntryActionHelper.GetEntryAction(entry, incomingMessage)));
				outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.VAL;
				AssertEquals("Status is not BAE", false, EntryActionHelper.IsOriginalClear(status, EntryActionHelper.GetEntryAction(entry, incomingMessage)));
			}
		}

		public void TestIsWithdrawnAccepted()
		{
			var entry = Factory.New<CusEntryHeader>();

			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_InterchangeNum = "424";
			var outgoingMessage = entry.Messages.AddNew();
			outgoingMessage.EM_EI = interchange1.PK;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.INV;

			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_InterchangeNum = "424.";
			var incomingMessage = Factory.New<FREDIMessage>();
			entry.Messages.Add(incomingMessage);
			incomingMessage.EM_EI = interchange2.PK;
			incomingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			Assert(EntryActionHelper.IsWithdrawnAccepted(EntryStatusDescriptionCodeList.Codes.ES150));
			Assert(EntryActionHelper.TransactionMustBeConfirmed(EntryStatusDescriptionCodeList.Codes.ES150, entry, incomingMessage));
		}

		public void TestIsWithdrawnRefused_HasError()
		{
			var entry = Factory.New<CusEntryHeader>();

			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_InterchangeNum = "424";
			var outgoingMessage = entry.Messages.AddNew();
			outgoingMessage.EM_EI = interchange1.PK;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.INV;

			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_InterchangeNum = "424.";
			var incomingMessage = Factory.New<DeltaDImportFREDIMessage>();
			entry.Messages.Add(incomingMessage);
			incomingMessage.EM_EI = interchange2.PK;
			incomingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			incomingMessage.EM_MessageText = @"<?xml version=""1.0"" encoding=""iso-8859-1"" standalone=""yes""?>
<Message>
	<EnveloppeMessage>
		<schemaID>MessageReponseCDecImp</schemaID>
		<schemaVersion>18122012</schemaVersion>
		<partyId>40218856900048</partyId>
		<transactionId>AAAAAAAAA+BBB+0000000001</transactionId>
		<numseq>0</numseq>
	</EnveloppeMessage>
	<ReponseDeclaration>
		<Entete>
			<refdos>9000-B00177613</refdos>
		</Entete>
		<ReponseDatas>
			<Erreur>
				<ErreurGen>
					<erreurCode>COR1500</erreurCode>
					<erreurDescription>This is error</erreurDescription>
				</ErreurGen>
			</Erreur>
		</ReponseDatas>
	</ReponseDeclaration>
</Message>";

			Assert("Withdrawn is refused if message has error", EntryActionHelper.IsWithdrawnRefused(entry, incomingMessage));
			Assert("Transaction must be deleted when withdrawn is refused", EntryActionHelper.TransactionMustBeDeleted(EntryStatusDescriptionCodeList.Codes.ES150, entry, incomingMessage));
		}

		public void TestIsWithdrawnRefused_InvalidationIsRefused()
		{
			var entry = Factory.New<CusEntryHeader>();

			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_InterchangeNum = "424";
			var outgoingMessage = entry.Messages.AddNew();
			outgoingMessage.EM_EI = interchange1.PK;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.INV;

			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_InterchangeNum = "424.";
			var incomingMessage = Factory.New<DeltaDImportFREDIMessage>();
			entry.Messages.Add(incomingMessage);
			incomingMessage.EM_EI = interchange2.PK;
			incomingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			incomingMessage.EM_MessageText = @"<?xml version=""1.0"" encoding=""iso-8859-1"" standalone=""yes""?>
<Message>
  <EnveloppeMessage>
    <schemaID>MessageReponseCDecImp</schemaID>
    <schemaVersion>01032013</schemaVersion>
    <partyId>33159700500064</partyId>
    <transactionId>HYEDFRCMT+DSI+0000007068</transactionId>
    <numseq>3</numseq>
  </EnveloppeMessage>
  <ReponseDeclaration>
    <Entete>
      <refdec>2300011256</refdec>
      <refdos>3FR33159700500064-B229379</refdos>
    </Entete>
    <ReponseDatas>
      <Notification>
        <Etat>
          <etat>REF</etat>
          <etatDate>20/06/2023</etatDate>
          <etatHeure>17:24</etatHeure>
          <etatprec>DRE</etatprec>
          <etatprecDate>20/06/2023</etatprecDate>
          <etatprecHeure>15:06</etatprecHeure>
          <evenement>invalidation d'une déclaration refusée par la douane</evenement>
          <ReponseDemande>
            <Motivation>
              <motiv>test AI2</motiv>
              <justifreg>Article 173 du CDU</justifreg>
            </Motivation>
            <motivservice>test KO</motivservice>
            <numdemande>2300000454</numdemande>
            <bureauagent>FR002300</bureauagent>
            <typedemande>RCT</typedemande>
          </ReponseDemande>
        </Etat>
      </Notification>
    </ReponseDatas>
  </ReponseDeclaration>
</Message>";

			Assert("Withdrawn is refused if evenement of message is 'invalidation d'une déclaration refusée par la douane'", EntryActionHelper.IsWithdrawnRefused(entry, incomingMessage));
			Assert("Transaction must be deleted when withdrawn is refused", EntryActionHelper.TransactionMustBeDeleted(EntryStatusDescriptionCodeList.Codes.ES150, entry, incomingMessage));
		}

		public void TestIsIntermediateStatus()
		{
			CombineAssertions(() =>
			{
				Assert(EntryActionHelper.IsIntermediateStatus(EntryStatusDescriptionCodeList.Codes.ES010));
				Assert(EntryActionHelper.IsIntermediateStatus(EntryStatusDescriptionCodeList.Codes.ES114));
				Assert(EntryActionHelper.IsIntermediateStatus(EntryStatusDescriptionCodeList.Codes.ES115));
				Assert(EntryActionHelper.IsIntermediateStatus(EntryStatusDescriptionCodeList.Codes.ES116));
				Assert(EntryActionHelper.IsIntermediateStatus(EntryStatusDescriptionCodeList.Codes.ES117));
				Assert(EntryActionHelper.IsIntermediateStatus(EntryStatusDescriptionCodeList.Codes.ES118));
				Assert(EntryActionHelper.IsIntermediateStatus(EntryStatusDescriptionCodeList.Codes.ES119));
				Assert(EntryActionHelper.IsIntermediateStatus(EntryStatusDescriptionCodeList.Codes.ES120));
			});
		}

		public void TestIsDecisionStatus()
		{
			CombineAssertions(() =>
			{
				Assert(EntryActionHelper.IsIntermediateStatus(EntryStatusDescriptionCodeList.Codes.ES118));
				Assert(EntryActionHelper.IsIntermediateStatus(EntryStatusDescriptionCodeList.Codes.ES119));
				Assert(EntryActionHelper.IsIntermediateStatus(EntryStatusDescriptionCodeList.Codes.ES120));
			});
		}

		public void TestUpdateStatusExceptionsWhenResponseWeightSuperiorToEntryStatusWeight()
		{
			AssertEquals(false, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES113, EntryStatusDescriptionCodeList.Codes.ES060));
			AssertEquals(false, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES118, EntryStatusDescriptionCodeList.Codes.ES100));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES118, EntryStatusDescriptionCodeList.Codes.ES116));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES118, EntryStatusDescriptionCodeList.Codes.ES117));
			AssertEquals(false, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES119, EntryStatusDescriptionCodeList.Codes.ES100));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES119, EntryStatusDescriptionCodeList.Codes.ES116));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES119, EntryStatusDescriptionCodeList.Codes.ES117));
		}

		public void TestUpdateStatusExceptionsWhenResponseWeightInferiorToEntryStatusWeight()
		{
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES060, EntryStatusDescriptionCodeList.Codes.ES062));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES060, EntryStatusDescriptionCodeList.Codes.ES063));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES060, EntryStatusDescriptionCodeList.Codes.ES116));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES060, EntryStatusDescriptionCodeList.Codes.ES117));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES060, EntryStatusDescriptionCodeList.Codes.ES118));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES060, EntryStatusDescriptionCodeList.Codes.ES119));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES060, EntryStatusDescriptionCodeList.Codes.ES120));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES061, EntryStatusDescriptionCodeList.Codes.ES062));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES061, EntryStatusDescriptionCodeList.Codes.ES063));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES061, EntryStatusDescriptionCodeList.Codes.ES116));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES061, EntryStatusDescriptionCodeList.Codes.ES117));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES061, EntryStatusDescriptionCodeList.Codes.ES118));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES061, EntryStatusDescriptionCodeList.Codes.ES119));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES061, EntryStatusDescriptionCodeList.Codes.ES120));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES070, EntryStatusDescriptionCodeList.Codes.ES116));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES070, EntryStatusDescriptionCodeList.Codes.ES117));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES070, EntryStatusDescriptionCodeList.Codes.ES118));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES070, EntryStatusDescriptionCodeList.Codes.ES119));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES070, EntryStatusDescriptionCodeList.Codes.ES120));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES080, EntryStatusDescriptionCodeList.Codes.ES081));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES080, EntryStatusDescriptionCodeList.Codes.ES082));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES080, EntryStatusDescriptionCodeList.Codes.ES116));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES080, EntryStatusDescriptionCodeList.Codes.ES117));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES080, EntryStatusDescriptionCodeList.Codes.ES118));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES080, EntryStatusDescriptionCodeList.Codes.ES119));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES080, EntryStatusDescriptionCodeList.Codes.ES120));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES083, EntryStatusDescriptionCodeList.Codes.ES116));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES083, EntryStatusDescriptionCodeList.Codes.ES117));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES083, EntryStatusDescriptionCodeList.Codes.ES118));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES083, EntryStatusDescriptionCodeList.Codes.ES119));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES083, EntryStatusDescriptionCodeList.Codes.ES120));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES090, EntryStatusDescriptionCodeList.Codes.ES120));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES100, EntryStatusDescriptionCodeList.Codes.ES101));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES100, EntryStatusDescriptionCodeList.Codes.ES111));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES100, EntryStatusDescriptionCodeList.Codes.ES112));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES100, EntryStatusDescriptionCodeList.Codes.ES113));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES100, EntryStatusDescriptionCodeList.Codes.ES116));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES100, EntryStatusDescriptionCodeList.Codes.ES117));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES100, EntryStatusDescriptionCodeList.Codes.ES118));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES100, EntryStatusDescriptionCodeList.Codes.ES119));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES100, EntryStatusDescriptionCodeList.Codes.ES120));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES101, EntryStatusDescriptionCodeList.Codes.ES111));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES101, EntryStatusDescriptionCodeList.Codes.ES112));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES101, EntryStatusDescriptionCodeList.Codes.ES113));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES101, EntryStatusDescriptionCodeList.Codes.ES116));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES101, EntryStatusDescriptionCodeList.Codes.ES117));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES101, EntryStatusDescriptionCodeList.Codes.ES118));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES101, EntryStatusDescriptionCodeList.Codes.ES119));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES101, EntryStatusDescriptionCodeList.Codes.ES120));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES130, EntryStatusDescriptionCodeList.Codes.ES131));
			AssertEquals(true, EntryActionHelper.ShouldUpdateEntryStatus(EntryStatusDescriptionCodeList.Codes.ES130, EntryStatusDescriptionCodeList.Codes.ES132));
		}

		public void TestGetEntryHeaderFromEntryNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var entryNumber = CusEntryNumber.New(entry, EU.Business.MessageTypeList.Codes.Import, Core.Constants.CountryCodes.France);
			entryNumber.CE_EntryNum = "0000000001";

			Factory.Save();

			var entryFound = EntryActionHelper.GetEntryHeaderFromEntryNumber(Factory, "0000000001", Core.Constants.CountryCodes.France);
			AssertSame(entry, entryFound);
		}

		public void TestGetEntryHeaderFromMessage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "9000-B00177613";

			var entryNumber = CusEntryNumber.New(entry, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.France);
			entryNumber.CE_EntryNum = "0000000001";

			var entryLine = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			Factory.Save();

			var incomingMessage = Factory.New<DeltaCImportFREDIMessage>();
			incomingMessage.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			incomingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			incomingMessage.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");

			var entryFound = EntryActionHelper.GetEntryHeaderFromMessage(Factory, (IResponseDataProvider)incomingMessage.MessageDataObject, incomingMessage.GetCountryCodeSafe());
			AssertSame(entry, entryFound);
		}

		public void TestGetOutgoingMessage()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();

			var interchange1 = Factory.NewWithValidTestData<EDIInterchange>();
			interchange1.EI_InterchangeNum = "423";
			var message1 = Factory.NewWithValidTestData<TestEDIMessage>();
			entryHeader.Messages.Add(message1);
			message1.EM_EI = interchange1.PK;
			message1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			Factory.Save();

			var interchange2 = Factory.NewWithValidTestData<EDIInterchange>();
			interchange2.EI_InterchangeNum = "424";
			var message2 = Factory.NewWithValidTestData<TestEDIMessage>();
			entryHeader.Messages.Add(message2);
			message2.EM_EI = interchange2.PK;
			message2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			Factory.Save();

			var interchange3 = Factory.NewWithValidTestData<EDIInterchange>();
			interchange3.EI_InterchangeNum = "425";
			var message3 = Factory.NewWithValidTestData<TestEDIMessage>();
			entryHeader.Messages.Add(message3);
			message3.EM_EI = interchange3.PK;
			message3.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			Factory.Save();

			var interchange4 = Factory.NewWithValidTestData<EDIInterchange>();
			var message4 = Factory.NewWithValidTestData<TestEDIMessage>();
			entryHeader.Messages.Add(message4);
			message4.EM_EI = interchange4.PK;
			message4.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			interchange4.EI_InterchangeNum = "424.115900";
			AssertEquals("GetOutgoingMessage should return the message matching the interchange with number 424", message2, EntryActionHelper.GetOutgoingMessage(entryHeader, message4));

			interchange4.EI_InterchangeNum = "424.";
			AssertEquals("GetOutgoingMessage should return the message matching the interchange with number 424", message2, EntryActionHelper.GetOutgoingMessage(entryHeader, message4));

			interchange4.EI_InterchangeNum = "424.BAE.BLABLA";
			AssertEquals("GetOutgoingMessage should return the message matching the interchange with number 424", message2, EntryActionHelper.GetOutgoingMessage(entryHeader, message4));

			interchange4.EI_InterchangeNum = "BLABLA.424.";
			AssertEquals("GetOutgoingMessage should return the last outgoing message found (in this case, the one with interchange number 425).", message3, EntryActionHelper.GetOutgoingMessage(entryHeader, message4));
		}

		public void TestGetEntryStatus()
		{
			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			var entryStatus = EntryActionHelper.GetEntryStatus(message);
			AssertEquals(EntryStatusDescriptionCodeList.Codes.ES100, entryStatus);

			var message2 = Factory.New<DeltaCImportFREDIMessage>();
			message2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message2.EM_MessageText = message.EM_MessageText.Replace("<etat>BAE</etat>", "<etat>MND</etat>");
			entryStatus = EntryActionHelper.GetEntryStatus(message2);
			AssertEquals("map MND to 085", EntryStatusDescriptionCodeList.Codes.ES085, entryStatus);
		}

		public void TestGetEntryAction_TRX()
		{
			var entry = Factory.New<CusEntryHeader>();
			var outgoingMessage = Factory.New<FREDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.ANT;
			entry.Messages.Add(outgoingMessage);
			AssertEquals("Entry action comes from sub type of outgoing message.", EntryActionCodeList.Codes.ANT, EntryActionHelper.GetEntryAction(entry, outgoingMessage));
		}

		public void TestGetEntryAction_RCV()
		{
			var entry = Factory.New<CusEntryHeader>();

			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_InterchangeNum = "424";
			var outgoingMessage = entry.Messages.AddNew();
			outgoingMessage.EM_EI = interchange1.PK;
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.MAP;

			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_InterchangeNum = "424.";
			var incomingMessage = entry.Messages.AddNew();
			incomingMessage.EM_EI = interchange2.PK;
			incomingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			AssertEquals("Entry action comes from sub type of outgoing message, because the incoming message won't tell us what action it is.", EntryActionCodeList.Codes.MAP, EntryActionHelper.GetEntryAction(entry, incomingMessage));
		}

		public void TestGetEntryAction_INT()
		{
			var entry = Factory.New<CusEntryHeader>();
			var message = entry.Messages.AddNew();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;
			AssertExceptionThrown<NotSupportedException>("Should not expect to get action from internal message.", () => EntryActionHelper.GetEntryAction(entry, message));
		}

		public void TestHasError_TRX()
		{
			var message = Factory.New<FREDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			AssertExceptionThrown<NotSupportedException>("Should not expect to detect errors on transmit message.", () => EntryActionHelper.HasError(message));
		}

		public void TestHasError_RCV()
		{
			var messageWithError = Factory.New<DeltaCImportFREDIMessage>();
			messageWithError.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			messageWithError.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportErreurResponseMessage.xml");
			AssertEquals("The message has error when we detect Erreur.", true, EntryActionHelper.HasError(messageWithError));

			var messageWithoutError = Factory.New<DeltaCImportFREDIMessage>();
			messageWithoutError.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			messageWithoutError.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportANNResponseMessage.xml");
			AssertEquals("The message has no error when we detect Etat.", false, EntryActionHelper.HasError(messageWithoutError));
		}

		public void TestHasError_INT()
		{
			var message = Factory.New<FREDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;
			AssertExceptionThrown<NotSupportedException>("Should not expect to detect errors on internal message.", () => EntryActionHelper.HasError(message));
		}

		public void TestIsGreaterThanOrEqualToBAE()
		{
			foreach (var code in new EntryStatusDescriptionCodeList().GetAllCodes())
			{
				if (int.Parse(code) < 100)
				{
					AssertEquals(false, EntryActionHelper.IsGreaterThanOrEqualToBAE(code));
				}
				else
				{
					AssertEquals(true, EntryActionHelper.IsGreaterThanOrEqualToBAE(code));
				}
			}
			AssertEquals(false, EntryActionHelper.IsGreaterThanOrEqualToBAE(""));
			AssertEquals(false, EntryActionHelper.IsGreaterThanOrEqualToBAE("HEY"));
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
