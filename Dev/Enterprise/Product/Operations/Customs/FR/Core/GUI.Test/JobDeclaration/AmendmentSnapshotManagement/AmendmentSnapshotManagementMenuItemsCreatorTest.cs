using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
	
namespace Enterprise.Customs.FR.GUI.Testing
{
	sealed class AmendmentSnapshotManagementMenuItemsCreatorTest : TestCaseWithFactory
	{
		public void TestShouldRevertToLastClearedMenuItemForTypicalEntryHeaderCore()
		{
			var ediMenu = new EDIMenu();
			var creator = new AmendmentSnapshotManagementMenuItemsCreatorForTest(ediMenu);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = Factory.NewWithValidTestData<CusEntryHeader>();
			declaration.ActiveEntryHeaders.Add(entry);
			Assert(!creator.ShouldRevertToLastClearedMenuItemForTypicalEntryHeaderCore_Exposed(entry));
			var outgoingMessage = AddOutgoingMessage(entry);
			outgoingMessage.EM_MessageSubType = EntryActionCodeList.Codes.REC;
			var incomingMessage = AddIncomingMessage(entry, "Enterprise.Customs.FR.GUI.Testing.DeltaCImportErreurResponseMessage.xml");

			Assert("Prerequisite: entry is in status of RectificationError", new DeltaGStatusResolver(entry, incomingMessage).CheckIsRectificationError());
			creator = new AmendmentSnapshotManagementMenuItemsCreatorForTest(ediMenu);
			Assert(creator.ShouldRevertToLastClearedMenuItemForTypicalEntryHeaderCore_Exposed(entry));

			FREDIMessage AddOutgoingMessage(CusEntryHeader entry)
			{
				var outgoingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
				outgoingInterchange.EI_InterchangeNum = "123";
				var outgoingMessage = Factory.NewWithValidTestData<FREDIMessage>();
				outgoingMessage.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
				entry.Messages.Add(outgoingMessage);
				outgoingMessage.EM_EI = outgoingInterchange.PK;
				outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				Factory.Save();
				return outgoingMessage;
			}

			FREDIMessage AddIncomingMessage(CusEntryHeader entry, string messageFile)
			{
				var incomingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
				incomingInterchange.EI_InterchangeNum = "123.";
				var incomingMessage = Factory.NewWithValidTestData<DeltaCImportFREDIMessage>();
				entry.Messages.Add(incomingMessage);
				incomingMessage.EM_EI = incomingInterchange.PK;
				incomingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				incomingMessage.EM_MessageText = resourceRetriever.Value.GetString(messageFile);
				return incomingMessage;
			}
		}

		class AmendmentSnapshotManagementMenuItemsCreatorForTest : AmendmentSnapshotManagementMenuItemsCreator
		{
			public AmendmentSnapshotManagementMenuItemsCreatorForTest(Customs.GUI.EDIMenu menu) : base(menu)
			{
			}

			public bool ShouldRevertToLastClearedMenuItemForTypicalEntryHeaderCore_Exposed(CusEntryHeader entryHeader ) => ShouldRevertToLastClearedMenuItemForTypicalEntryHeaderCore(entryHeader);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
