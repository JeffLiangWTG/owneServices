using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5SCMessageSenderTest : CusEntryHeaderOriginalMessageSenderTest<GOVCBR5SCSender>
	{
		protected override IEnumerable<CusEntryHeader> GetMessageParents() => EntriesToSend;

		protected override MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR5SCSenderForTest(EntriesToSend, Factory, messageType) : new GOVCBR5SCSender(EntriesToSend, Factory, messageType);
		readonly ZString messageType = ElectronicDocumentTypeList.Codes._5SC;
		IEnumerable<CusEntryHeader> EntriesToSend
		{
			get
			{
				if (entriesToSend == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

					var invoice1 = declaration.Invoices.AddNew();
					var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
					entryHeader1.CH_MessageType = messageType;
					var entryLine1 = entryHeader1.AllEntryLines.AddNew();
					entryLine1.CL_FTASequenceNumber = 1;
					var invoiceLine1 = entryLine1.InvoiceLines.AddNew();
					invoiceLine1.JI_JZ = invoice1.PK;

					var invoice2 = declaration.Invoices.AddNew();
					var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
					entryHeader2.CH_MessageType = messageType;
					entryHeader2.AllEntryLines.AddNew();
					var entryLine2 = entryHeader2.AllEntryLines.AddNew();
					entryLine2.CL_FTASequenceNumber = 2;
					var invoiceLine2 = entryLine2.InvoiceLines.AddNew();
					invoiceLine2.JI_JZ = invoice2.PK;

					var invoice3 = declaration.Invoices.AddNew();
					var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
					entryHeader3.AllEntryLines.AddNew();
					entryHeader3.CH_MessageType = messageType;
					var entryLine3 = entryHeader3.AllEntryLines.AddNew();
					entryLine3.CL_FTASequenceNumber = 3;
					var invoiceLine3 = entryLine3.InvoiceLines.AddNew();
					invoiceLine3.JI_JZ = invoice3.PK;

					entriesToSend = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>();
				}
				return entriesToSend;
			}
		}

		public void TestVersionIDFromEntryNum()
		{
			var entry = GetMessageParents().Cast<CusEntryHeader>().First();
			entry.CH_VersionID = 10;
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = messageType;
			entryNum.CE_EntryLineReference = "20";
			MessageSender.Send();
			var message = entry.Messages[0];

			AssertEquals("21", message.EM_ApplicationReference);
		}

		IEnumerable<CusEntryHeader> entriesToSend;
	}

	class GOVCBR5SCSenderForTest : GOVCBR5SCSender
	{
		public GOVCBR5SCSenderForTest(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory, ZString messageType)
			: base(entries, factory, messageType)
		{
		}

		protected override ImportFTAHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new System.Exception();
	}
}
