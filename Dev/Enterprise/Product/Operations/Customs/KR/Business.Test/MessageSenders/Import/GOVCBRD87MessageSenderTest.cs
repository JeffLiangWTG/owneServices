using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRD87MessageSenderTest : CusEntryHeaderOriginalMessageSenderTest<GOVCBRD87Sender>
	{
		protected override IEnumerable<CusEntryHeader> GetMessageParents() => EntriesToSend;

		protected override MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBRD87SenderForTest(EntriesToSend, Factory) : new GOVCBRD87Sender(EntriesToSend, Factory);

		IEnumerable<CusEntryHeader> EntriesToSend
		{
			get
			{
				if (entriesToSend == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
					var entry = declaration.CustomsEntryHeaders.AddNew();
					entry.CH_MessageType = ElectronicDocumentTypeList.Codes._D87;
					var entryLine = entry.MergedLines.AddNew();
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.JI_CL = entryLine.PK;
					entriesToSend = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>();
				}
				return entriesToSend;
			}
		}

		[TestDate(2022, 12, 27, 12, 30, 45)]
		public void TestCH_EntrySubmittedDateSaveWhenSend()
		{
			MessageSender.Send();
			foreach (CusEntryHeader entry in EntriesToSend)
			{
				AssertEquals(new ZDateTime(2022, 12, 27, 12, 30, 45), entry.CH_EntrySubmittedDate);
			}
		}

		public override void TestStatusIsUpdated()
		{
			MessageSender.Send();
			foreach (CusEntryHeader entry in EntriesToSend)
			{
				AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, entry.CH_Status);
				AssertEquals(ZShort.Zero, entry.CH_VersionID);
			}
		}

		IEnumerable<CusEntryHeader> entriesToSend;

		class GOVCBRD87SenderForTest : GOVCBRD87Sender
		{
			public GOVCBRD87SenderForTest(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory)
				: base(entries, factory)
			{
			}

			protected override ImportD87Header GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new Exception();
		}
	}
}
