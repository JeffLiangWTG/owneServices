using CargoWise.Common.JSON.Extensions;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Vietnam.EInvoice;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public class RAPGlobalElectronicInvoiceBuilderForVietnam : GlobalElectronicInvoiceBuilderForVietnam
	{
		public RAPGlobalElectronicInvoiceBuilderForVietnam(string batchNumber, AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo)
			: base(batchNumber, additionalTransactionInfo)
		{
		}

		protected override ZString GetPayload(INotifications notifications)
		{
			return new VietnamEInvoiceApproveCreator(AdditionalTransactionInfo).Create()?.ToJSON();
		}

		protected override string SchemaResourceName => "Enterprise.Accounting.ElectronicMessaging.Vietnam.EInvoice.ApproveInvoice.VietnamEInvoiceApproveSchema.json";
		protected override ZString MessageType => VietnamEInvoiceAPICommandList.Codes.ApproveReceivablesInvoice;
	}
}
