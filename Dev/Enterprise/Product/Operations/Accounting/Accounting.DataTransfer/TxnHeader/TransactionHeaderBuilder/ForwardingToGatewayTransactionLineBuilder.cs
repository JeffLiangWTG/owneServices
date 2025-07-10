using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	internal class ForwardingToGatewayTransactionLineBuilder : TransactionLineBuilder
	{
		public ForwardingToGatewayTransactionLineBuilder(INotificationManager notifier, TransactionBuilderConfig config) : base(notifier, config)
		{
		}

		protected override bool ShouldResetJobAndChargeCode(InvoicingLineBase invoiceLine) => !invoiceLine.ChargeCode.IsGatewayRelated();

		protected override void PrepareToResetJobAndChargeCode(TxnLine xmlInvoiceLine)
		{
			xmlInvoiceLine.ConsolOrJobNo = xmlInvoiceLine.OriginalShipmentJobNumber;
			xmlInvoiceLine.ConsolOrJobType = TxnLineConsolOrJobType.SHP;
			xmlInvoiceLine.RelatedJobID = ZString.Empty;
			xmlInvoiceLine.RelatedJobNumber = ZString.Empty;
		}

		protected override void ResetJobAndChargeCode(InvoicingLineBase invoiceLine, bool shouldResetChargeCode)
		{
			base.ResetJobAndChargeCode(invoiceLine, shouldResetChargeCode);
			if (shouldResetChargeCode)
			{
				invoiceLine.AL_Desc = ZString.Empty;
			}
		}
	}
}
