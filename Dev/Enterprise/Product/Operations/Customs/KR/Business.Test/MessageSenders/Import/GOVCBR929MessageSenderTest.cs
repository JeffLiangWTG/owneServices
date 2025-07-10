using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR929MessageSenderTest : CusEntryHeaderOriginalMessageSenderTest<GOVCBR929Sender>
	{
		protected override IEnumerable<CusEntryHeader> GetMessageParents() => EntriesToSend;

		protected override MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR929SenderForTest(EntriesToSend, Factory) : new GOVCBR929Sender(EntriesToSend, Factory);

		IEnumerable<CusEntryHeader> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<CusEntryHeader> parents;

		IEnumerable<CusEntryHeader> EntriesToSend
		{
			get
			{
				if (entriesToSend == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var entry = declaration.CustomsEntryHeaders.AddNew();
					entry.CH_MessageType = JobMessageTypeList.Codes.Import;
					var entryLine = entry.MergedLines.AddNew();
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.JI_CL = entryLine.PK;
					entriesToSend = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>();

					var chargeVersion1 = entry.Charges.AddNew();
					chargeVersion1.C1_ChargeType = Universal.Constants.RateTypes.Duty;
					chargeVersion1.C1_ChargeAmount = 1000m;
					entryLine.CL_LineNumber = 1;
					var feeVersion1 = entryLine.Fees.AddNew();
					feeVersion1.CF_ChargeType = Universal.Constants.RateTypes.Duty;
					feeVersion1.CF_ChargeAmount = 1000m;
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

		public void TestChargesVersionUpdate()
		{
			MessageSender.Send();
			foreach (CusEntryHeader entry in Parents)
			{
				AssertEquals("1", entry.Charges[0].C1_RateOverrideReasonCode);
				AssertEquals("1", entry.MergedLines[0].Fees[0].CF_RateOverrideReasonCode);
			}
		}

		protected override ZString GetStatusField(CusEntryHeader entry) => entry.CH_Status;
	}

	class GOVCBR929SenderForTest : GOVCBR929Sender
	{
		public GOVCBR929SenderForTest(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory)
			: base(entries, factory)
		{
		}

		protected override ImportEntryHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new Exception();
	}
}
