using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5DQMessageSenderTest : CusEntryHeaderOriginalMessageSenderTest<GOVCBR5DP5DQSender>
	{
		protected override IEnumerable<CusEntryHeader> GetMessageParents() => EntriesToSend;

		protected override MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR5DQSenderForTest(EntriesToSend, ElectronicDocumentTypeList.Codes._5DQ, Factory) : new GOVCBR5DP5DQSender(EntriesToSend, ElectronicDocumentTypeList.Codes._5DQ, Factory);
		IEnumerable<CusEntryHeader> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<CusEntryHeader> parents;

		IEnumerable<CusEntryHeader> EntriesToSend
		{
			get
			{
				if (entriesToSend == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = "LEX";
					declaration.JE_MessageSubType = "07";
					var invHeader = declaration.Invoices.AddNew();
					invHeader.JZ_RX_NKInvoice_Currency = "USD";
					var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
					var entryLine1 = entryHeader1.AllEntryLines.AddNew();
					var invLine1 = invHeader.InvoiceLines.AddNew();
					invLine1.JI_CL = entryLine1.PK;
					var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
					var entryLine2 = entryHeader2.AllEntryLines.AddNew();
					var invLine2 = invHeader.InvoiceLines.AddNew();
					invLine2.JI_CL = entryLine2.PK;
					var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
					var entryLine3 = entryHeader3.AllEntryLines.AddNew();
					var invLine3 = invHeader.InvoiceLines.AddNew();
					invLine3.JI_CL = entryLine3.PK;
					entriesToSend = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>();
				}
				return entriesToSend;
			}
		}

		IEnumerable<CusEntryHeader> entriesToSend;

		public override void TestStatusIsUpdated()
		{
			MessageSender.Send();
			foreach (CusEntryHeader entry in Parents)
			{
				AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, entry.CH_Status);
				AssertEquals(ZShort.Zero, entry.CH_VersionID);
			}
		}

		[TestDate(2022, 12, 27, 12, 30, 45)]
		public void TestCH_EntrySubmittedDateSaveWhenSend()
		{
			MessageSender.Send();
			foreach (CusEntryHeader entry in Parents)
			{
				AssertEquals(new ZDateTime(2022, 12, 27, 12, 30, 45), entry.CH_EntrySubmittedDate);
			}
		}

		public void TestRoundDecimalPlaces()
		{
			var entry = new TestDataSetupHelper(Factory).GetLocalExportEntry5DQWithFullData();
			entry.MergedLines[1].InvoiceLines[0].JI_NetWeight = 1.1233m;
			entry.MergedLines[1].InvoiceLines[0].JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			entry.MergedLines[1].InvoiceLines[1].JI_NetWeight = 1.0005m;
			entry.MergedLines[1].InvoiceLines[1].JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			entry.MergedLines[1].InvoiceLines[0].JI_InvoiceQuantity = 0.8765m;
			entry.MergedLines[1].InvoiceLines[1].JI_InvoiceQuantity = 2m;
			entry.MergedLines[0].CL_CustomsValue = 1000.123m;
			entry.MergedLines[1].CL_CustomsValue = 3000.12345m;
			entry.InvoiceHeaders()[0].JZ_Weight = 900.12345m;

			entriesToSend = entry.Declaration.CustomsEntryHeaders.Cast<CusEntryHeader>();
			MessageSender.Send();

			var message = Parents.Single().Messages[0];
			using (var textReader = message.GetEM_MessageTextReader())
			{
				var result = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5DQ.Declaration>(textReader);
				AssertEquals(2.124m, result.Consignment.ConsignmentItem[0].GoodsMeasure.NetNetWeightMeasure.Value);
				AssertEquals(2.877m, result.Consignment.ConsignmentItem[0].Commodity.CountQuantity.Value);
				AssertEquals(3000m, result.Consignment.ConsignmentItem[0].Commodity.ValueAmount.Value);
				AssertEquals(4000.25m, result.InvoiceAmount.Value);
				AssertEquals(900.123m, result.TotalGrossMassMeasure.Value);
			}
		}

		protected override ZString GetStatusField(CusEntryHeader entry) => entry.CH_Status;
	}

	class GOVCBR5DQSenderForTest : GOVCBR5DP5DQSender
	{
		public GOVCBR5DQSenderForTest(IEnumerable<CusEntryHeader> entries, string messageType, BusinessObjectFactory factory)
		: base(entries, messageType, factory)
		{
		}

		protected override LocalExportEntryHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new System.Exception();
	}
}
