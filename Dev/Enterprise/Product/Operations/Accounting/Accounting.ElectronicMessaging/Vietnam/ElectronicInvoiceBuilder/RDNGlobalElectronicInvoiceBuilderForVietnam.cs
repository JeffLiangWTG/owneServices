using CargoWise.Common.JSON.Extensions;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public class RDNGlobalElectronicInvoiceBuilderForVietnam : GlobalElectronicInvoiceBuilderForVietnam
	{
		public RDNGlobalElectronicInvoiceBuilderForVietnam(string batchNumber, AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo) : base(batchNumber, additionalTransactionInfo)
		{
		}

		protected override ZString GetPayload(INotifications notifications)
		{
			return new VietnamEInvoiceDocumentCreator().Create(AdditionalTransactionInfo)?.ToJSON();
		}

		protected override string SchemaResourceName => "Enterprise.Accounting.ElectronicMessaging.Vietnam.EInvoice.RequestDocument.VietnamEInvoiceDocumentSchema.json";

		protected override ZString MessageType => VietnamEInvoiceAPICommandList.Codes.RequestDocumentForInvoice;
	}
}
