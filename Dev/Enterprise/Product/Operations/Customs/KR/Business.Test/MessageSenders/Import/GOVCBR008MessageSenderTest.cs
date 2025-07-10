using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR008MessageSenderTest : CusEntryHeaderOriginalMessageSenderTest<GOVCBR008Sender>
	{
		protected override IEnumerable<CusEntryHeader> GetMessageParents() => EntriesToSend;

		protected override MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR008SenderForTest(EntriesToSend, Factory) : new GOVCBR008Sender(EntriesToSend, Factory);

		public override void TestStatusIsUpdated()
		{
			MessageSender.Send();
			foreach (CusEntryHeader entry in EntriesToSend)
			{
				AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, entry.CH_Status);
				AssertEquals(ZShort.Zero, entry.CH_VersionID);
			}
		}

		IEnumerable<CusEntryHeader> EntriesToSend
		{
			get
			{
				if (entriesToSend == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;
					var entry = declaration.CustomsEntryHeaders.AddNew();
					entry.CH_MessageType = ElectronicDocumentTypeList.Codes._008;
					var entryLine = entry.MergedLines.AddNew();
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					invoiceLine.JI_CL = entryLine.PK;
					invoiceLine.JI_InvoiceUQ = "1";
					entriesToSend = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>();

					var pornography = declaration.PersonalItemDecQuestions.AddNew();
					pornography.CY_Code = Import008DecQuestion.DecQuestion.PossessingPornography;
					pornography.CY_Data = Constants.YesNo.No;
					pornography.CY_ParentID = entry.PK;
				}
				return entriesToSend;
			}
		}

		IEnumerable<CusEntryHeader> entriesToSend;

		[TestDate(2022, 12, 27, 12, 30, 45)]
		public void TestCH_EntrySubmittedDateSaveWhenSend()
		{
			MessageSender.Send();
			foreach (CusEntryHeader entry in EntriesToSend)
			{
				AssertEquals(new ZDateTime(2022, 12, 27, 12, 30, 45), entry.CH_EntrySubmittedDate);
			}
		}

		class GOVCBR008SenderForTest : GOVCBR008Sender
		{
			public GOVCBR008SenderForTest(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory)
				: base(entries, factory)
			{
			}

			protected override Import008Header GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new Exception();
		}
	}
}
