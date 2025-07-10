using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5ASAmendmentDetailsManagerTest : AmendmentDetailsManagerTest
	{
		public override void TestAmendmentType()
		{
			AmendmentTypeAmendment();
		}

		void AmendmentTypeAmendment()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var entryLine1 = entry.MergedLines[0];
			entryLine1.CL_AdValoremTariff = "8429521022";
			entryLine1.InvoiceLines[0].JI_Tariff = "8429521022";
			var entryLine2 = entry.MergedLines[1];
			entryLine2.InvoiceLines[0].JI_Description = "USED EXCAVATOR";
			var sendingObj = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS);
			AssertEquals(_5ASAmendmentType.Codes.Amendment, sendingObj.AmendmentType);
		}

		public override void TestAmendedItems()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var declaration = entry.Declaration;
			declaration.JE_ExportGoodsType = "12";
			declaration.JE_MessageSubType = "A";
			declaration.InspectionDate = new ZDateTime(2023, 01, 01);

			var amendmentManager = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS);
			var amendedItems = amendmentManager.AmendedItems.ToArray();

			CombineAssertions(() =>
			{
				AssertEquals(EntityAmendType.Update, amendedItems[0].AmendType);
				AssertEquals("11", amendedItems[0].BeforeValue);
				AssertEquals("12", amendedItems[0].AfterValue);

				AssertEquals(EntityAmendType.Update, amendedItems[1].AmendType);
				AssertEquals("B", amendedItems[1].BeforeValue);
				AssertEquals("A", amendedItems[1].AfterValue);

				AssertEquals(EntityAmendType.Update, amendedItems[2].AmendType);
				AssertEquals("20210308", amendedItems[2].BeforeValue);
				AssertEquals("20230101", amendedItems[2].AfterValue);
			});
		}

		public override void TestAmendmentVersion()
		{
			var entry = new TestDataSetupHelper(Factory).GetExportEntryWithFullData();
			new GOVCBR830Sender(entry.Declaration.CustomsEntryHeaders, Factory).Send();
			AssertEquals(1, entry.Snapshots.Count);
			AssertEquals(1u, entry.Snapshots[0].CES_VersionNumber);
			AssertEquals("1", entry.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == "830").EM_ApplicationReference);
			AssertEquals(ZShort.Zero, entry.CH_VersionID);

			entry.CH_VersionID = 1;

			entry.MergedLines.Cast<CusEntryLine>().First(x => x.CL_LineNumber == 1).Delete();
			var amendmentMessageSendingObjectParent1 = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, "5AS");
			amendmentMessageSendingObjectParent1.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(1u, amendmentMessageSendingObjectParent1.SendingObjectsCollection[0].AmendmentVersion);
			new GOVCBR5ASAmendmentSender(amendmentMessageSendingObjectParent1.ObjectsToSend, Factory).Send();
			var outgoingMessages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == "5AS");
			AssertEquals(2, entry.Snapshots.Count);
			AssertEquals(2u, entry.Snapshots[1].CES_VersionNumber);
			AssertEquals(1, outgoingMessages.Count());
			AssertEquals("2", outgoingMessages.LastOrDefault().EM_ApplicationReference);
			AssertEquals(1u, entry.CH_VersionID);

			entry.CH_VersionID = 2;

			var miscMessageSendingObjectParentCancellation = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, "DKJ", MessageFunctions.MessageFunctionCode.Cancellation);
			miscMessageSendingObjectParentCancellation.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(2u, miscMessageSendingObjectParentCancellation.SendingObjectsCollection[0].AmendmentVersion);
			new GOVCBRDKJSender(miscMessageSendingObjectParentCancellation.ObjectsToSend, Factory).Send();
			outgoingMessages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == "DKJ");
			AssertEquals("SnapShot is created only when AmendType is Amendment.", 2, entry.Snapshots.Count);
			AssertEquals(1, outgoingMessages.Count());
			AssertEquals("3", outgoingMessages.LastOrDefault().EM_ApplicationReference);
			AssertEquals(2u, entry.CH_VersionID);

			entry.CH_VersionID = 3;

			entry.MergedLines.Cast<CusEntryLine>().First(x => x.CL_LineNumber == 2).Delete();
			var amendmentMessageSendingObjectParent2 = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, "5AS");
			amendmentMessageSendingObjectParent2.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(3u, amendmentMessageSendingObjectParent2.SendingObjectsCollection[0].AmendmentVersion);
			new GOVCBR5ASAmendmentSender(amendmentMessageSendingObjectParent2.ObjectsToSend, Factory).Send();
			outgoingMessages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == "5AS");
			AssertEquals(3, entry.Snapshots.Count);
			AssertEquals(4u, entry.Snapshots[2].CES_VersionNumber);
			AssertEquals(2, outgoingMessages.Count());
			AssertEquals("4", outgoingMessages.LastOrDefault().EM_ApplicationReference);
			AssertEquals(3u, entry.CH_VersionID);

			entry.CH_VersionID = 4;
			var miscMessageSendingObjectParentExtend = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, "5AS", MessageFunctions.MessageFunctionCode.Extend);
			miscMessageSendingObjectParentExtend.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(4u, miscMessageSendingObjectParentExtend.SendingObjectsCollection[0].AmendmentVersion);
			new GOVCBR5ASExtendOfPeriodSender(miscMessageSendingObjectParentExtend.ObjectsToSend, Factory).Send();
			outgoingMessages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == "5AS");
			AssertEquals("SnapShot is created only when AmendType is Amendment.", 3, entry.Snapshots.Count);
			AssertEquals(3, outgoingMessages.Count());
			AssertEquals("5", outgoingMessages.LastOrDefault().EM_ApplicationReference);
			AssertEquals(4u, entry.CH_VersionID);
		}

		public void TestDataItemIDList()
		{
			var list = Factory.GetCachedValue<ExportAmendmentDataItemIDList>();
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var amendmentManager = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS);
			AssertSame(list, amendmentManager.DataItemIDList);
		}

		public void TestAmendItemTypeOfDate()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("EN-US");
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			entry.Declaration.InspectionDate = new ZDateTime(2023, 01, 01);

			var amendmentManager = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS);
			var amendedItems = amendmentManager.AmendedItems.ToArray();
			AssertEquals("20210308", amendedItems[0].BeforeValue);
			AssertEquals("20230101", amendedItems[0].AfterValue);

			Thread.CurrentThread.CurrentCulture = new CultureInfo("KO-KR");
			amendmentManager = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS);
			amendedItems = amendmentManager.AmendedItems.ToArray();
			AssertEquals("20210308", amendedItems[0].BeforeValue);
			AssertEquals("20230101", amendedItems[0].AfterValue);
		}

		public void TestImportCargoManagementNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ProcedureType = "H";
			declaration.InspectionDate = new ZDateTime(2021, 03, 08);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = "EXP";
			entry.EntryNumber = "6N00221000025X";
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(entry.RegistryCompanyPK, Guid.Empty, Guid.Empty, "6N002");

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;
			var export830 = new ExportEntryHeaderCreator().Create(entry);
			export830.RoundDecimalValueRoundedWithDecimalPlaces();
			using (var stream = KRXmlObjectSerializer.Serialize(export830))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._830, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._830);
				Factory.Save();
			}
			entry.RandomHeader.JZ_ImportCargoManagementNumber = "20KE0E63JII00391222";

			var amendmentManager = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS);
			var amendedItems = amendmentManager.AmendedItems.ToArray();
			AssertEquals("", amendedItems[0].BeforeValue);
			AssertEquals("20KE0E63JII00391222", amendedItems[0].AfterValue);
			AssertEquals("F101", amendedItems[0].DataItemID);

			export830 = new ExportEntryHeaderCreator().Create(entry);
			export830.RoundDecimalValueRoundedWithDecimalPlaces();
			using (var stream = KRXmlObjectSerializer.Serialize(export830))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._830, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._830);
				Factory.Save();
			}
			entry.RandomHeader.JZ_ImportCargoManagementNumber = "40KE0E63JII00391222";
			amendmentManager = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS);
			amendedItems = amendmentManager.AmendedItems.ToArray();
			AssertEquals("20KE0E63JII00391222", amendedItems[0].BeforeValue);
			AssertEquals("40KE0E63JII00391222", amendedItems[0].AfterValue);
			AssertEquals("F101", amendedItems[0].DataItemID);

			entry.RandomHeader.JZ_ImportCargoManagementNumber = ZString.Empty;
			amendmentManager = new GOVCBR5ASAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5AS);
			amendedItems = amendmentManager.AmendedItems.ToArray();
			AssertEquals("20KE0E63JII00391222", amendedItems[0].BeforeValue);
			AssertEquals("", amendedItems[0].AfterValue);
			AssertEquals("F001", amendedItems[0].DataItemID);
		}
	}
}
