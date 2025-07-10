using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR105AmendmentDetailsManagerTest : AmendmentDetailsManagerTest
	{
		public override void TestAmendedItems()
		{
			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA4", "BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED");
			var importerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "HKBOARAM0001A" }
			};
			TestOrgDataSetUpHelper.AddCustomsCode(importer, importerCodes);
			Factory.Save();
			var entry = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot();
			var declaration = entry.Declaration;
			declaration.JE_ExportDate = new ZDateTime(2023, 08, 18);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_RL_NKPortOfLoading = "KRPUS";
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;

			var invoice = declaration.Invoices[0];
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 6;
			var invoiceLine5 = invoice.InvoiceLines.AddNew();
			invoiceLine5.JI_CL = entryLine.PK;
			invoiceLine5.CusEntryLine.CL_FTASequenceNumber = 6;
			invoiceLine5.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine5.JI_COOSupportingDocType = "1";
			invoiceLine5.JI_SequenceNumber = 6;

			var amendmentManager = new GOVCBR105AmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._105);
			var amendedItems = amendmentManager.AmendedItems.ToArray();

			AssertEquals(EntityAmendType.Update, amendedItems[0].AmendType);
			AssertEquals("20230718", amendedItems[0].BeforeValue);
			AssertEquals("20230818", amendedItems[0].AfterValue);

			AssertEquals(EntityAmendType.Update, amendedItems[1].AmendType);
			AssertEquals("", amendedItems[1].BeforeValue);
			AssertEquals("KR", amendedItems[1].AfterValue);

			AssertEquals(EntityAmendType.Update, amendedItems[2].AmendType);
			AssertEquals("", amendedItems[2].BeforeValue);
			AssertEquals("Busan", amendedItems[2].AfterValue);

			AssertEquals(EntityAmendType.Add, amendedItems[3].AmendType);
			AssertEquals("", amendedItems[3].BeforeValue);
			AssertEquals("BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED", amendedItems[3].AfterValue);

			AssertEquals(EntityAmendType.Add, amendedItems[4].AmendType);
			AssertEquals("", amendedItems[4].BeforeValue);
			AssertEquals("#1", amendedItems[4].AfterValue);

			AssertEquals(EntityAmendType.Add, amendedItems[3].AmendType);
			AssertEquals("", amendedItems[5].BeforeValue);
			AssertEquals("6", amendedItems[5].AfterValue);

			AssertEquals(EntityAmendType.Add, amendedItems[4].AmendType);
			AssertEquals("", amendedItems[6].BeforeValue);
			AssertEquals("N", amendedItems[6].AfterValue);

			AssertEquals(EntityAmendType.Add, amendedItems[5].AmendType);
			AssertEquals("", amendedItems[7].BeforeValue);
			AssertEquals("1", amendedItems[7].AfterValue);
		}

		public override void TestAmendmentType()
		{
			var entry = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot();
			var entryLine1 = entry.MergedLines[0];
			entryLine1.RandomLine.JI_Tariff = "8429521022";
			var sendingObj = new GOVCBR105AmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._105);
			AssertEquals(FTAAmendmentType.Codes.UXX, sendingObj.AmendmentType);

			var entry2 = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot();
			entry2.MergedLines.Cast<CusEntryLine>().First(x => x.CL_LineNumber == 1).Delete();
			sendingObj = new GOVCBR105AmendmentDetailsManager(entry2, ElectronicDocumentTypeList.Codes._105);
			AssertEquals(FTAAmendmentType.Codes.XDX, sendingObj.AmendmentType);

			var entry3 = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot();
			var invoice = (JobComInvoiceHeader)entry3.Declaration.Invoices.FirstOrDefault();

			var entryLine2 = entry3.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 7;
			var invoiceLine6 = invoice.InvoiceLines.AddNew();
			invoiceLine6.JI_CL = entryLine2.PK;
			invoiceLine6.CusEntryLine.CL_FTASequenceNumber = 6;
			invoiceLine6.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine6.JI_COOSupportingDocType = "1";
			invoiceLine6.JI_SequenceNumber = 6;
			sendingObj = new GOVCBR105AmendmentDetailsManager(entry3, ElectronicDocumentTypeList.Codes._105);
			AssertEquals(FTAAmendmentType.Codes.XXI, sendingObj.AmendmentType);

			var entryLine4 = entry3.MergedLines[0];
			entryLine4.RandomLine.JI_Tariff = "8429521022";
			sendingObj = new GOVCBR105AmendmentDetailsManager(entry3, ElectronicDocumentTypeList.Codes._105);
			AssertEquals(FTAAmendmentType.Codes.UXI, sendingObj.AmendmentType);
		}

		public override void TestAmendmentVersion()
		{
			var entry = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot();
			AssertEquals(1, entry.Snapshots.Count);
			AssertEquals(1u, entry.Snapshots[0].CES_VersionNumber);

			var entryNum5SC = entry.EntryNumbers.Cast<CusEntryNumber>().Single(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5SC);
			entryNum5SC.CE_EntryLineReference = "1";
			entry.MergedLines.Cast<CusEntryLine>().First(x => x.CL_LineNumber == 1).Delete();
			var amendmentMessageSendingObjectParent1 = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, "105");
			amendmentMessageSendingObjectParent1.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(1u, amendmentMessageSendingObjectParent1.SendingObjectsCollection[0].AmendmentVersion);
			new GOVCBR105AmendmentSender(amendmentMessageSendingObjectParent1.ObjectsToSend, Factory, ElectronicDocumentTypeList.Codes._105).Send();
			var outgoingMessages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == "105");
			AssertEquals(1, outgoingMessages.Count());
			AssertEquals(2, entry.Snapshots.Count);
			AssertEquals("2", outgoingMessages.LastOrDefault().EM_ApplicationReference);
			AssertEquals(2u, entry.Snapshots[1].CES_VersionNumber);

			entryNum5SC.CE_EntryLineReference = "2";
			entry.MergedLines.AddNew();
			var amendmentMessageSendingObjectParent2 = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, "105");
			amendmentMessageSendingObjectParent2.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(2u, amendmentMessageSendingObjectParent2.SendingObjectsCollection[0].AmendmentVersion);
			new GOVCBR105AmendmentSender(amendmentMessageSendingObjectParent2.ObjectsToSend, Factory, ElectronicDocumentTypeList.Codes._105).Send();
			outgoingMessages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == "105");
			AssertEquals(2, outgoingMessages.Count());
			AssertEquals(3, entry.Snapshots.Count);
			AssertEquals("3", outgoingMessages.LastOrDefault().EM_ApplicationReference);
			AssertEquals((ZShort)3, entry.Snapshots[2].CES_VersionNumber);
		}
	}
}
