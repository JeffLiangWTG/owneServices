using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRDHSAmendmentDetailsManagerTest : AmendmentDetailsManagerTest
	{
		public override void TestAmendedItems()
		{
			var importer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA4", "BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED");
			var importerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.ForeignCompanyID, Number = "HKBOARAM0001A" }
			};
			importer.MainAddress.Address1 = "Test Address";
			TestOrgDataSetUpHelper.AddCustomsCode(importer, importerCodes);
			Factory.Save();
			var entry = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot(true);
			var declaration = entry.Declaration;
			declaration.JE_ExportDate = new ZDateTime(2023, 08, 18);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_RL_NKPortOfLoading = "KRPUS";
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;

			var entryLine6 = entry.MergedLines[5];
			var invoiceLine6 = entryLine6.RandomLine;
			invoiceLine6.JI_CustomsFifthQuantity = 999;
			invoiceLine6.CertificateOfOriginUQ = "KG";
			invoiceLine6.CreateCertificateOfOriginDataIfRequired();
			invoiceLine6.CertificateOfOriginData.CSI_ReferenceNumber = "AAA123";
			invoiceLine6.CertificateOfOriginData.CSI_LineNo = 6;

			var amendmentManager = new GOVCBRDHSAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._DHS);
			var amendedItems = amendmentManager.AmendedItems.ToArray();

			AssertEquals(EntityAmendType.Update, amendedItems[0].AmendType);
			AssertEquals("14", amendedItems[0].DataItemID);
			AssertEquals("20230718", amendedItems[0].BeforeValue);
			AssertEquals("20230818", amendedItems[0].AfterValue);

			AssertEquals(EntityAmendType.Update, amendedItems[1].AmendType);
			AssertEquals("13A", amendedItems[1].DataItemID);
			AssertEquals("", amendedItems[1].BeforeValue);
			AssertEquals("KR", amendedItems[1].AfterValue);

			AssertEquals(EntityAmendType.Update, amendedItems[2].AmendType);
			AssertEquals("13B", amendedItems[2].DataItemID);
			AssertEquals("", amendedItems[2].BeforeValue);
			AssertEquals("Busan", amendedItems[2].AfterValue);

			AssertEquals(EntityAmendType.Update, amendedItems[3].AmendType);
			AssertEquals("41", amendedItems[3].DataItemID);
			AssertEquals("", amendedItems[3].BeforeValue);
			AssertEquals("AAA123", amendedItems[3].AfterValue);

			AssertEquals(EntityAmendType.Update, amendedItems[4].AmendType);
			AssertEquals("42", amendedItems[4].DataItemID);
			AssertEquals("0", amendedItems[4].BeforeValue);
			AssertEquals("6", amendedItems[4].AfterValue);

			AssertEquals(EntityAmendType.Update, amendedItems[5].AmendType);
			AssertEquals("43", amendedItems[5].DataItemID);
			AssertEquals("0", amendedItems[5].BeforeValue);
			AssertEquals("999", amendedItems[5].AfterValue);

			AssertEquals(EntityAmendType.Update, amendedItems[6].AmendType);
			AssertEquals("44", amendedItems[6].DataItemID);
			AssertEquals("", amendedItems[6].BeforeValue);
			AssertEquals("KG", amendedItems[6].AfterValue);

			AssertEquals(EntityAmendType.Add, amendedItems[7].AmendType);
			AssertEquals("03A", amendedItems[7].DataItemID);
			AssertEquals("", amendedItems[7].BeforeValue);
			AssertEquals("BOA RAM HONGKONG INTERNATIONAL TRADING CO LIMITED", amendedItems[7].AfterValue);

			AssertEquals(EntityAmendType.Add, amendedItems[8].AmendType);
			AssertEquals("03F", amendedItems[8].DataItemID);
			AssertEquals("", amendedItems[8].BeforeValue);
			AssertEquals("Test Address", amendedItems[8].AfterValue);

			AssertEquals(EntityAmendType.Update, amendedItems[9].AmendType);
			AssertEquals("11E", amendedItems[9].DataItemID);
			AssertEquals("", amendedItems[9].BeforeValue);
			AssertEquals("AAA123", amendedItems[9].AfterValue);
		}

		public override void TestAmendmentType()
		{
			var entry = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot(true);
			UpdateInvoiceLine(entry);
			var sendingObj = new GOVCBRDHSAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._DHS);
			AssertEquals(DHSAmendmentTypeForInvoiceLine.Codes.CXX, sendingObj.AmendmentTypeForInvoiceLine);

			var entry2 = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot(true);
			DeleteInvoiceLine(entry2);
			sendingObj = new GOVCBRDHSAmendmentDetailsManager(entry2, ElectronicDocumentTypeList.Codes._DHS);
			AssertEquals(DHSAmendmentTypeForInvoiceLine.Codes.XRX, sendingObj.AmendmentTypeForInvoiceLine);

			ZShort lineNo = 6;
			var entry3 = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot(true);
			AddToInvoiceLine(entry3, ++lineNo);
			sendingObj = new GOVCBRDHSAmendmentDetailsManager(entry3, ElectronicDocumentTypeList.Codes._DHS);
			AssertEquals(DHSAmendmentTypeForInvoiceLine.Codes.XXA, sendingObj.AmendmentTypeForInvoiceLine);

			var entry4 = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot(true);
			UpdateInvoiceLine(entry4);
			AddToInvoiceLine(entry4, ++lineNo);
			sendingObj = new GOVCBRDHSAmendmentDetailsManager(entry4, ElectronicDocumentTypeList.Codes._DHS);
			AssertEquals(DHSAmendmentTypeForInvoiceLine.Codes.CXA, sendingObj.AmendmentTypeForInvoiceLine);

			var entry5 = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot(true);
			DeleteInvoiceLine(entry5);
			AddToInvoiceLine(entry5, ++lineNo);
			sendingObj = new GOVCBRDHSAmendmentDetailsManager(entry5, ElectronicDocumentTypeList.Codes._DHS);
			AssertEquals(DHSAmendmentTypeForInvoiceLine.Codes.XRA, sendingObj.AmendmentTypeForInvoiceLine);

			var entry6 = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot(true);
			UpdateInvoiceLine(entry6);
			DeleteInvoiceLine(entry6);
			sendingObj = new GOVCBRDHSAmendmentDetailsManager(entry6, ElectronicDocumentTypeList.Codes._DHS);
			AssertEquals(DHSAmendmentTypeForInvoiceLine.Codes.CRX, sendingObj.AmendmentTypeForInvoiceLine);

			var entry7 = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot(true);
			UpdateInvoiceLine(entry7);
			AddToInvoiceLine(entry7, ++lineNo);
			DeleteInvoiceLine(entry7);
			sendingObj = new GOVCBRDHSAmendmentDetailsManager(entry7, ElectronicDocumentTypeList.Codes._DHS);
			AssertEquals(DHSAmendmentTypeForInvoiceLine.Codes.CRA, sendingObj.AmendmentTypeForInvoiceLine);
		}

		void AddToInvoiceLine(CusEntryHeader entry, ZShort no)
		{
			var invoice = entry.Declaration.Invoices[0];
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = no;
			entryLine.CL_FTASequenceNumber = no;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.CusEntryLine.CL_FTASequenceNumber = no;
			invoiceLine.JI_RN_NKSecondCommercialInvoiceCountry = ZString.Empty;
			invoiceLine.JI_CustomsFifthQuantity = 111;
			invoiceLine.JI_SequenceNumber = no;
		}

		void DeleteInvoiceLine(CusEntryHeader entry)
		{
			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)entry.MergedLines[0].InvoiceLines[0];
			invoiceLine.CusEntryLine.CL_FTASequenceNumber = 0;
		}

		void UpdateInvoiceLine(CusEntryHeader entry)
		{
			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)entry.MergedLines[1].InvoiceLines[0];
			invoiceLine.JI_CustomsFifthQuantity = 999;
		}

		public override void TestAmendmentVersion()
		{
			var entry = new TestDataSetupHelper(Factory).GetFTAEntrySnapshot(true);
			AssertEquals(1, entry.Snapshots.Count);
			AssertEquals(1u, entry.Snapshots[0].CES_VersionNumber);

			var entryNumDHR = entry.EntryNumbers.Cast<CusEntryNumber>().Single(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._DHR);
			entryNumDHR.CE_EntryLineReference = "1";
			entry.MergedLines.Cast<CusEntryLine>().First(x => x.CL_LineNumber == 1).Delete();
			var amendmentMessageSendingObjectParent1 = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, "DHS");
			amendmentMessageSendingObjectParent1.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(1u, amendmentMessageSendingObjectParent1.SendingObjectsCollection[0].AmendmentVersion);
			new GOVCBRDHSAmendmentSender(amendmentMessageSendingObjectParent1.ObjectsToSend, Factory, ElectronicDocumentTypeList.Codes._DHS).Send();
			var outgoingMessages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == "DHS");
			AssertEquals(1, outgoingMessages.Count());
			AssertEquals(2, entry.Snapshots.Count);
			AssertEquals("2", outgoingMessages.LastOrDefault().EM_ApplicationReference);
			AssertEquals(2u, entry.Snapshots[1].CES_VersionNumber);

			entryNumDHR.CE_EntryLineReference = "2";
			entry.MergedLines.AddNew();
			var amendmentMessageSendingObjectParent2 = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, "DHS");
			amendmentMessageSendingObjectParent2.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(2u, amendmentMessageSendingObjectParent2.SendingObjectsCollection[0].AmendmentVersion);
			new GOVCBRDHSAmendmentSender(amendmentMessageSendingObjectParent2.ObjectsToSend, Factory, ElectronicDocumentTypeList.Codes._DHS).Send();
			outgoingMessages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == "DHS");
			AssertEquals(2, outgoingMessages.Count());
			AssertEquals(3, entry.Snapshots.Count);
			AssertEquals("3", outgoingMessages.LastOrDefault().EM_ApplicationReference);
			AssertEquals((ZShort)3, entry.Snapshots[2].CES_VersionNumber);
		}
	}
}
