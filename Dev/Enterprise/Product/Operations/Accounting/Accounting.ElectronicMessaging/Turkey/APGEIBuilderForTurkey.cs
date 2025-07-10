using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	public class APGEIBuilderForTurkey : GlobalElectronicInvoiceBuilderBaseForTurkey
	{
		public APGEIBuilderForTurkey(string batchNumber, string messageType, GlbCompany company, string invoiceId) : base(batchNumber, messageType)
		{
			Argument.NotNull(company, nameof(company));
			Argument.NotNull(invoiceId, nameof(invoiceId));

			this.company = company;
			InvoiceId = invoiceId;
		}

		protected string InvoiceId { get; }

		protected override GlbCompany GEIMessageCompany => company;
		GlbCompany company { get; }

		protected override BusinessObjectFactory Factory => GEIMessageCompany.Factory;

		protected override ZString GetPayloadXML(INotifications notifications)
		{
			switch (MessageType)
			{
				case TurkeyEInvoiceAPICommandList.Codes.GetInboxInvoiceList:
					return GetPILPayloadXML();

				case TurkeyEInvoiceAPICommandList.Codes.SetInvoiceTaken:
					return GetPSTPayloadXML();

				case TurkeyEInvoiceAPICommandList.Codes.SendApproveDocumentResponse:
					return GetPAPPayloadXML();

				case TurkeyEInvoiceAPICommandList.Codes.SendRejectDocumentResponse:
					return GetPRJPayloadXML();

				default:
					return ZString.Empty;
			}
		}

		protected string GetRejectReason() => string.Empty;

		protected ZString GetPILPayloadXML()
			=> $@"<GetInboxInvoiceList xmlns:tem=""http://tempuri.org/"" xmlns=""http://tempuri.org/""><query PageIndex=""0"" PageSize=""{AccountingConfigurationRegistry.Instance.SizeOfRequestedAPTransactionList.GetValueWithoutFallback(GEIMessageCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)}"" OnlyNewestInvoices=""true""></query></GetInboxInvoiceList>"; // Payload String

		protected ZString GetPSTPayloadXML()
			=> $@"<SetInvoicesTaken xmlns:tem=""http://tempuri.org/"" xmlns=""http://tempuri.org/""><invoices><string IsNew=""true"">{InvoiceId}</string></invoices></SetInvoicesTaken>"; // Payload String

		protected ZString GetPAPPayloadXML()
			=> $@"<SendDocumentResponse xmlns:tem=""http://tempuri.org/"" xmlns=""http://tempuri.org/""><responses><DocumentResponseInfo><InvoiceId>{InvoiceId}</InvoiceId><ResponseStatus>Approved</ResponseStatus><Reason>{GetRejectReason()}</Reason></DocumentResponseInfo></responses></SendDocumentResponse>"; // Payload String

		protected ZString GetPRJPayloadXML()
			=> $@"<SendDocumentResponse xmlns:tem=""http://tempuri.org/"" xmlns=""http://tempuri.org/""><responses><DocumentResponseInfo><InvoiceId>{InvoiceId}</InvoiceId><ResponseStatus>Declined</ResponseStatus><Reason>{GetRejectReason()}</Reason></DocumentResponseInfo></responses></SendDocumentResponse>"; // Payload String
	}
}
