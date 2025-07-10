using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5SMMessageSenderTest : CusEntryHeaderOriginalMessageSenderTest<GOVCBR5SMSender>
	{
		protected override IEnumerable<CusEntryHeader> GetMessageParents() => EntriesToSend;

		protected override MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR5SMSenderForTest(EntriesToSend, Factory) : new GOVCBR5SMSender(EntriesToSend, Factory);

		IEnumerable<CusEntryHeader> EntriesToSend
		{
			get
			{
				if (entriesToSend == null)
				{
					var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
					var entryLine = entryHeader1.AllEntryLines.AddNew();
					declaration.InvoiceLines[0].JI_CL = entryLine.PK;

					var entryNum = entryHeader1.EntryNumbers.AddNew();
					entryNum.CE_EntryNum = "88888211002U";
					entryNum.CE_EntryLineReference = "1";
					entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5SM;
					entryNum.CE_IssueDate = ZDateTime.Today;

					var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
					entryLine = entryHeader2.AllEntryLines.AddNew();
					declaration.InvoiceLines[1].JI_CL = entryLine.PK;

					entryNum = entryHeader2.EntryNumbers.AddNew();
					entryNum.CE_EntryNum = "88888211003U";
					entryNum.CE_EntryLineReference = "1";
					entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5SM;
					entryNum.CE_IssueDate = ZDateTime.Today;

					var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
					entryLine = entryHeader3.AllEntryLines.AddNew();
					declaration.InvoiceLines[2].JI_CL = entryLine.PK;

					entryNum = entryHeader3.EntryNumbers.AddNew();
					entryNum.CE_EntryNum = "88888211004U";
					entryNum.CE_EntryLineReference = "1";
					entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5SM;
					entryNum.CE_IssueDate = ZDateTime.Today;

					entriesToSend = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>();
				}
				return entriesToSend;
			}
		}

		IEnumerable<CusEntryHeader> entriesToSend;
		JobDeclaration declaration;

		[TestDate(2022, 12, 27, 12, 30, 45)]
		public void TestCH_EntrySubmittedDateSaveWhenSend()
		{
			MessageSender.Send();
			foreach (CusEntryHeader entry in EntriesToSend)
			{
				AssertEquals(new ZDateTime(2022, 12, 27, 12, 30, 45), entry.CH_EntrySubmittedDate);
			}
		}

		protected override ZString GetStatusField(CusEntryHeader entry)
		{
			return entry.CH_Status;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "899999999", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "Tariff description");

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._5SM;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "899999999";

			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "899999999";

			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "899999999";
		}
	}

	class GOVCBR5SMSenderForTest : GOVCBR5SMSender
	{
		public GOVCBR5SMSenderForTest(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory)
			: base(entries, factory)
		{
		}

		protected override Import5SMHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new System.Exception();
	}
}
