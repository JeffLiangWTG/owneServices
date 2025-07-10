using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Interfaces;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public abstract class AutoSendCustomsMessageProcessorTest<T> : Customs.Business.Testing.AutoSendCustomsMessageProcessorTest where T : IAutoSendCustomsMessageRule, new()
	{
		protected override void PrepareInvoiceLine(BaseJobComInvoiceLine invoiceLine)
		{
			base.PrepareInvoiceLine(invoiceLine);
			var cei = invoiceLine.InvoiceHeader.JobDeclaration.CustomsEntryInstructions.AddNew();
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine.JI_Tariff = "2203001010";
			invoiceLine.JI_Description = "Unit Test";
			invoiceLine.JI_LinePrice = 50;
			invoiceLine.JI_CEI = cei.PK;
			invoiceLine.JI_Weight = 10;
			invoiceLine.JI_NetWeight = 10;
			invoiceLine.JI_ValuationCode = "A";
			invoiceLine.JI_CustomsUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		}

		protected override void SetEntryClearedStatus(Customs.Business.CusEntryHeader entry)
		{
			entry.EntryNumber = "123";
			entry.CH_EntryStatus = OriginalEntryStatusForProcessing;
		}

		protected override Customs.Business.CusEntryHeader GetEntryHeader(BaseJobDeclaration declaration)
		{
			return declaration.ActiveEntryHeaders.Cast<Customs.Business.CusEntryHeader>().FirstOrDefault();
		}

		protected override void AssertEntryAndMessageResultForEndToEndTest(Customs.Business.CusEntryHeader entry)
		{
			AssertEquals(1, entry.Declaration.CustomsEntryHeaders.Count);
			AssertEquals(UpdatedEntryStatusAfterProcessed, entry.CH_EntryStatus);

			AssertEquals(1, entry.Messages.Count);
			var message = entry.Messages[0];
			AssertEquals(ApplicationCodeList.Codes.FRCustomsMessage, message.EM_ApplicationCode);
			AssertEquals(new T().newMessageType, message.EM_MessageSubType);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIMessageStatusList.Codes.Queued, message.EM_Status);
			Assert("Message should be held for later sending.", message.EM_SystemCreateTimeUtc < message.EM_HeldUntilDate);
		}

		protected override ZString ExpectedMessageDescription => "France Customs Declaration";

		protected abstract ZString OriginalEntryStatusForProcessing { get; }

		protected abstract ZString UpdatedEntryStatusAfterProcessed { get; }
	}
}
