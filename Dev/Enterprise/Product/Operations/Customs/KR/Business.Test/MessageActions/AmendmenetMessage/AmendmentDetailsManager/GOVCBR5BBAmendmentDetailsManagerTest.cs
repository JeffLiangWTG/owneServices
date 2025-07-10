using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5BBAmendmentDetailsManagerTest : AmendmentDetailsManagerTest
	{
		public override void TestAmendmentType()
		{
			AmendmentTypeUpdate();
			AmendmentTypeAdd();
			AmendmentTypeDelete();
			AmendmentTypeMixed();
		}

		void AmendmentTypeUpdate()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var entryLine1 = entry.MergedLines[0];
			entryLine1.CL_AdValoremTariff = "8523292991";
			entryLine1.InvoiceLines[0].JI_Tariff = "8523292991";
			var entryLine2 = entry.MergedLines[1];
			entryLine2.InvoiceLines[0].JI_Description = "Those recorded video";
			var entryInstruction = entry.Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().SingleOrDefault();
			entryInstruction.CEI_AgreedDutyRate = 4.52m;
			entryInstruction.CEI_AgreedDutyRatePreferenceCode = "FAU1";
			var sendingObj = new GOVCBR5BBAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5BB);
			AssertEquals(_5BBAmendmentType.Codes.Update, sendingObj.AmendmentType);
		}

		void AmendmentTypeAdd()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 3;
			entryLine.CL_AdValoremTariff = "8523292991";
			var invoice = entry.Declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "Those recorded video";
			invoiceLine.JI_Tariff = "8523292991";
			invoiceLine.JI_CL = entryLine.PK;
			var amendmentManager = new GOVCBR5BBAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5BB);
			AssertEquals(_5BBAmendmentType.Codes.Add, amendmentManager.AmendmentType);
		}

		void AmendmentTypeDelete()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			entry.MergedLines.Cast<CusEntryLine>().First(x => x.CL_LineNumber == 1).Delete();
			var amendmentManager = new GOVCBR5BBAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5BB);
			AssertEquals(_5BBAmendmentType.Codes.Delete, amendmentManager.AmendmentType);
		}

		void AmendmentTypeMixed()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			entry.MergedLines.Cast<CusEntryLine>().First(x => x.CL_LineNumber == 1).Delete();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 3;
			entryLine.CL_AdValoremTariff = "8523292991";
			var invoiceLine = entryLine.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "Those recorded video";
			invoiceLine.JI_Tariff = "8523292991";
			var amendmentManager = new GOVCBR5BBAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5BB);
			AssertEquals(_5BBAmendmentType.Codes.Mix, amendmentManager.AmendmentType);
		}

		public override void TestAmendmentVersion()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5BA);
			AssertEquals(1, entry.Snapshots.Count);
			AssertEquals(1u, entry.Snapshots[0].CES_VersionNumber);

			var entryNum5BA = entry.EntryNumbers.Cast<CusEntryNumber>().Single(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5BA);
			entryNum5BA.CE_EntryLineReference = "1";
			entry.MergedLines.Cast<CusEntryLine>().First(x => x.CL_LineNumber == 1).Delete();
			var amendmentMessageSendingObjectParent1 = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, "5BB");
			amendmentMessageSendingObjectParent1.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(1u, amendmentMessageSendingObjectParent1.SendingObjectsCollection[0].AmendmentVersion);
			new GOVCBR5BBSender(amendmentMessageSendingObjectParent1.ObjectsToSend, Factory).Send();
			var outgoingMessages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == "5BB");
			AssertEquals(1, outgoingMessages.Count());
			AssertEquals(2, entry.Snapshots.Count);
			AssertEquals("2", outgoingMessages.LastOrDefault().EM_ApplicationReference);
			AssertEquals(2u, entry.Snapshots[1].CES_VersionNumber);

			entryNum5BA.CE_EntryLineReference = "2";
			entry.MergedLines.AddNew();
			var amendmentMessageSendingObjectParent2 = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, "5BB");
			amendmentMessageSendingObjectParent2.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(2u, amendmentMessageSendingObjectParent2.SendingObjectsCollection[0].AmendmentVersion);
			new GOVCBR5BBSender(amendmentMessageSendingObjectParent2.ObjectsToSend, Factory).Send();
			outgoingMessages = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == "5BB");
			AssertEquals(2, outgoingMessages.Count());
			AssertEquals(3, entry.Snapshots.Count);
			AssertEquals("3", outgoingMessages.LastOrDefault().EM_ApplicationReference);
			AssertEquals((ZShort)3, entry.Snapshots[2].CES_VersionNumber);
		}

		public override void TestAmendedItems()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var entryLine = entry.MergedLines.Cast<CusEntryLine>().First(x => x.CL_LineNumber == 1);
			var invoice = entryLine.RandomLine.InvoiceHeader;
			entryLine.RandomLine.Delete();
			entryLine.Delete();
			entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 3;
			entryLine.CL_AdValoremTariff = "8523292991";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8523292991";
			invoiceLine.JI_Description = "Those recorded video";
			invoiceLine.JI_CL = entryLine.PK;
			var entryInstruction = entry.Declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().SingleOrDefault();
			entryInstruction.CEI_AgreedDutyRate = 4.52m;
			entryInstruction.CEI_AgreedDutyRatePreferenceCode = "FAU1";
			var amendmentManager = new GOVCBR5BBAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5BB);
			var amendedItems = amendmentManager.AmendedItems.ToArray();

			CombineAssertions(() =>
			{
				AssertEquals(6, amendedItems.Length);

				var amendedItem = amendedItems[0];
				AssertEquals("1 - AmendType", EntityAmendType.Update, amendedItem.AmendType);
				AssertEquals("1 - BeforeValue", "8.25", amendedItem.BeforeValue);
				AssertEquals("1 - AfterValue", "4.52", amendedItem.AfterValue);

				amendedItem = amendedItems[1];
				AssertEquals("2 - AmendType", EntityAmendType.Update, amendedItem.AmendType);
				AssertEquals("2 - BeforeValue", "C1", amendedItem.BeforeValue);
				AssertEquals("2 - AfterValue", "FAU1", amendedItem.AfterValue);

				amendedItem = amendedItems[2];
				AssertEquals("3 - AmendType", EntityAmendType.Delete, amendedItem.AmendType);
				AssertEquals("3 - BeforeValue", string.Empty, amendedItem.BeforeValue);
				AssertEquals("3 - AfterValue", string.Empty, amendedItem.AfterValue);

				amendedItem = amendedItems[3];
				AssertEquals("4 - AmendType", EntityAmendType.Add, amendedItem.AmendType);
				AssertEquals("4 - BeforeValue", string.Empty, amendedItem.BeforeValue);
				AssertEquals("4 - AfterValue", "비디오 녹화된 것", amendedItem.AfterValue);

				amendedItem = amendedItems[4];
				AssertEquals("5 - AmendType", EntityAmendType.Add, amendedItem.AmendType);
				AssertEquals("5 - BeforeValue", string.Empty, amendedItem.BeforeValue);
				AssertEquals("5 - AfterValue", "Those recorded video", amendedItem.AfterValue);

				amendedItem = amendedItems[5];
				AssertEquals("6 - AmendType", EntityAmendType.Add, amendedItem.AmendType);
				AssertEquals("6 - BeforeValue", string.Empty, amendedItem.BeforeValue);
				AssertEquals("6 - AfterValue", "8523292991", amendedItem.AfterValue);
			});
		}

		public void TestDataItemIDList()
		{
			var list = Factory.GetCachedValue<GOVCBR5BADataItemIDList>();
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var amendmentManager = new GOVCBR5BBAmendmentDetailsManager(entry, ElectronicDocumentTypeList.Codes._5BB);
			AssertSame(list, amendmentManager.DataItemIDList);
		}
	}
}
