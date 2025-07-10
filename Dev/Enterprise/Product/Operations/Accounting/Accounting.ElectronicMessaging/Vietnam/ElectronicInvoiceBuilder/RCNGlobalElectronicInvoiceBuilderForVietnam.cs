using CargoWise.Common.JSON.Extensions;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public class RCNGlobalElectronicInvoiceBuilderForVietnam : GlobalElectronicInvoiceBuilderForVietnam
	{
		public RCNGlobalElectronicInvoiceBuilderForVietnam(string batchNumber, AdditionalTransactionInfoForVietnamEInvoice additionalTransactionInfo)
			: base(batchNumber, additionalTransactionInfo)
		{
		}

		protected override ZString GetPayload(INotifications notifications)
		{
			return IsCircular78
				? new VietnamEInvoiceCancellationCircularCreator(AdditionalTransactionInfo).Create()?.ToJSON()
				: new VietnamEInvoiceCancellationCreator(AdditionalTransactionInfo).Create()?.ToJSON();
		}

		protected override string SchemaResourceName => IsCircular78
			? "Enterprise.Accounting.ElectronicMessaging.Vietnam.EInvoice.CancelCircularInvoice.VietnamEInvoiceCancellationCircularSchema.json"
			: "Enterprise.Accounting.ElectronicMessaging.Vietnam.EInvoice.CancelInvoice.VietnamEInvoiceCancellationSchema.json";
		protected override ZString MessageType => IsCircular78
			? VietnamEInvoiceAPICommandList.Codes.CancelReceivablesCircular78Invoice
			: VietnamEInvoiceAPICommandList.Codes.CancelReceivablesInvoice;

		bool IsCircular78
		{
			get
			{
				return new VietnamComplianceInfoEInvoicingExtension().IsCircular78(AdditionalTransactionInfo.ComplianceSequenceInfo?.OriginalComplianceSequenceMaximumNumberDigits ?? 0);
			}
		}
	}
}
