using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(FTAAmendmentDetailsCollection))]
	sealed class FTAAmendmentDetailsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FTAAmendmentDetailsCollection>
	{
		protected override FTAAmendmentDetailsCollection GetCollectionToTest() => new FTAAmendmentDetailsCollection(Factory.New<CusEntryHeader>());
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return new FTAAmendmentDetails(new ImportFTAAmendmentHeaderCreator().Create(entry, System.Array.Empty<AmendedItem>()), Factory, ZString.Empty, entry.PK, ZString.Empty, null);
		}

		public void TestFTAAmendmentDetailsCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine1.JI_CL = entry.MergedLines.AddNew().PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine2.JI_CL = entry.MergedLines.AddNew().PK;
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_CountryOfOrigin = Core.Constants.CountryCodes.Japan;
			invoiceLine3.JI_CL = entry.MergedLines.AddNew().PK;
			var invoiceLine4 = invoice.InvoiceLines.AddNew();
			invoiceLine4.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine4.JI_CL = entry.MergedLines.AddNew().PK;
			AssertEquals(0, entry.Snapshots.Count);

			new GOVCBRDHRSender(declaration.CustomsEntryHeaders, Factory, ElectronicDocumentTypeList.Codes._DHR).Send();
			var snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._DHR, EntrySnapshotStatus.Current);
			snapshot.CES_Status = EntrySnapshotStatus.Deleted;
			AssertEquals(1u, snapshot.CES_VersionNumber);

			invoiceLine1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Mongolia;
			new GOVCBR5SCSender(declaration.CustomsEntryHeaders, Factory, ElectronicDocumentTypeList.Codes._5SC).Send();
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5SC, EntrySnapshotStatus.Current);
			snapshot.CES_Status = EntrySnapshotStatus.Lodged;
			AssertEquals(1u, snapshot.CES_VersionNumber);

			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5SC);
			entryNum.CE_EntryLineReference = "1";

			var amendmentMessageSendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._105);
			amendmentMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			amendmentMessageSendingObjectParent.SendingObjectsCollection[0].AmendmentReason = "First amend message is reject";
			new GOVCBR105AmendmentSender(amendmentMessageSendingObjectParent.ObjectsToSend, Factory, ElectronicDocumentTypeList.Codes._105).Send();
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5SC, EntrySnapshotStatus.Current);
			snapshot.CES_Status = EntrySnapshotStatus.Deleted;
			AssertEquals(2u, snapshot.CES_VersionNumber);
			var message = entry.Messages.LastOutgoingMessage;
			AssertEquals("2", message.EM_ApplicationReference);

			amendmentMessageSendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._105);
			amendmentMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			amendmentMessageSendingObjectParent.SendingObjectsCollection[0].AmendmentReason = "Second amend message";
			new GOVCBR105AmendmentSender(amendmentMessageSendingObjectParent.ObjectsToSend, Factory, ElectronicDocumentTypeList.Codes._105).Send();
			snapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5SC, EntrySnapshotStatus.Current);
			AssertEquals(2u, snapshot.CES_VersionNumber);
			message = entry.Messages.LastOutgoingMessage;
			AssertEquals("2", message.EM_ApplicationReference);

			var amendmentDetailsCollection = new FTAAmendmentDetailsCollection(entry);
			AssertEquals(1, amendmentDetailsCollection.Count);
			AssertEquals("Second amend message", amendmentDetailsCollection[0].AmendmentReason);
		}

		public void TestVersionOfAmendments105()
		{
			var entry = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot();
			AssertEquals(ElectronicDocumentTypeList.Codes._5SC, entry.GetOriginalFTAType());
			AssertVersionOfAmendments(entry, ElectronicDocumentTypeList.Codes._5SC, ElectronicDocumentTypeList.Codes._105);
		}

		public void TestVersionOfAmendmentsDHS()
		{
			var entry = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot(true);
			entry.MergedLines[0].InvoiceLines[0].JI_CountryOfOrigin = Core.Constants.CountryCodes.China;
			AssertEquals(ElectronicDocumentTypeList.Codes._DHR, entry.GetOriginalFTAType());
			AssertVersionOfAmendments(entry, ElectronicDocumentTypeList.Codes._DHR, ElectronicDocumentTypeList.Codes._DHS);
		}

		void AssertVersionOfAmendments(CusEntryHeader entry, ZString entryType, ZString messageType)
		{
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(entryType);
			entryNum.CE_EntryLineReference = "1";
			entry.MergedLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_LineNumber == 1).Delete();
			entry.ResetIsCustomsValueCalculated();

			var firstMessage = SetSnapShotData(entry, messageType, "First Amend Message (Amend Version '1')");
			AssertEquals("2", firstMessage.EM_ApplicationReference);
			var secondMessage = SetSnapShotData(entry, messageType, "Second Amend Message (Amend Version '1')");
			AssertEquals("2", secondMessage.EM_ApplicationReference);

			var amendmentDetailsCollection = new FTAAmendmentDetailsCollection(entry);
			AssertEquals(1, amendmentDetailsCollection.Count);
			AssertEquals("Second Amend Message (Amend Version '1')", amendmentDetailsCollection[0].AmendmentReason);

			entryNum.CE_EntryLineReference = "2";
			var thirdMessage = SetSnapShotData(entry, messageType, "Third Amend Message (Amend Version '2')");
			AssertEquals("3", thirdMessage.EM_ApplicationReference);

			amendmentDetailsCollection = new FTAAmendmentDetailsCollection(entry);
			AssertEquals(2, amendmentDetailsCollection.Count);
			AssertEquals("Second Amend Message (Amend Version '1')", amendmentDetailsCollection[0].AmendmentReason);
			AssertEquals("Third Amend Message (Amend Version '2')", amendmentDetailsCollection[1].AmendmentReason);
		}
		EDIMessage SetSnapShotData(CusEntryHeader entry, ZString messageType, ZString reason)
		{
			var declaration = entry.Declaration;
			var amendmentMessageSendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(declaration, messageType);
			amendmentMessageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			amendmentMessageSendingObjectParent.SendingObjectsCollection[0].AmendmentReason = reason;

			if (messageType == ElectronicDocumentTypeList.Codes._105)
			{
				new GOVCBR105AmendmentSender(amendmentMessageSendingObjectParent.ObjectsToSend, Factory, ElectronicDocumentTypeList.Codes._105).Send();
			}
			else
			{
				new GOVCBRDHSAmendmentSender(amendmentMessageSendingObjectParent.ObjectsToSend, Factory, ElectronicDocumentTypeList.Codes._DHS).Send();
			}

			return entry.Messages.Cast<EDIMessage>().LastOrDefault(x => x.EM_MessageType == messageType);
		}
	}
}
