using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public class InvoicingLineBaseForOtherTaxesDisplay : TransactionLineForOtherTaxesDisplay
	{
		internal InvoicingLineBaseForOtherTaxesDisplay(InvoicingLineBase invoice)
		{
			parent = invoice;
		}
		readonly InvoicingLineBase parent;

		public override ZString ChargeCode => parent.GenericChargeBizO.VC_Code;

		public override ZString ChargeCodeDescription => parent.AL_Desc;

		public override ZString Branch => parent.Branch.GB_Code;

		public override ZString Department => parent.Department.GE_Code;

		public override ZString Job => parent.Job != null ? parent.Job.JH_JobNum : ZString.Empty;

		public override ZString Currency => parent.AL_RX_NKTransactionCurrency;

		public override ZDecimal OSExTaxAmount => parent.AL_OSExTaxAmount;

		public override ZDecimal OSTaxAmount => parent.AL_OSTaxAmount;

		public override ZDecimal LocalExTaxAmount => parent.AL_LocalExTaxAmount;

		public override ZDecimal LocalTaxAmount => parent.AL_LocalTaxAmount;

		public override ZDecimal OSTotalAmount => parent.AL_OverseasTotal;

		public override ZDecimal LocalTotalAmount => parent.AL_LocalTotalAmount;

		public override ZString GovtChargeCode => parent.AL_GovtChargeCode;

		public override ZInt OSCurrencyDecimals => parent.CurrencyDecimals;

		public override ZInt LocalCurrencyDecimals => parent.LocalDecimals;

		public override ZString TaxBranch => parent.TaxBranch?.GB_Code ?? ZString.Empty;

		public override ZString SupplyType => parent.AL_SupplyType;

		public override OtherTaxesLinkedToTransactionLinesCollection OtherTaxesLinkedToTransactionLinesCollection
		{
			get
			{
				if (otherTaxesLinkedToTransactionLinesCollection == null)
				{
					otherTaxesLinkedToTransactionLinesCollection = new OtherTaxesLinkedToTransactionLinesCollection(parent, TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(parent.InvoiceBase));
				}
				return otherTaxesLinkedToTransactionLinesCollection;
			}
		}
		OtherTaxesLinkedToTransactionLinesCollection otherTaxesLinkedToTransactionLinesCollection;
	}
}
