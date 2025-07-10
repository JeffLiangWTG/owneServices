using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public class InvoicingLineBaseTaxable : ITaxableTransactionLine
	{
		internal InvoicingLineBaseTaxable(InvoicingLineBase invoice)
		{
			parent = invoice;
		}
		readonly InvoicingLineBase parent;

		ZString ITaxableTransactionLineBase.Currency => parent.AL_RX_NKTransactionCurrency;

		ZDecimal ITaxableTransactionLineBase.BaseOSAmount => parent.AL_OSExTaxAmount_DBSigned;

		ZDecimal ITaxableTransactionLineBase.LocalAmount => parent.AL_LineAmount;

		ZDate ITaxableTransactionLineBase.TaxDate => !parent.AL_TaxDate.IsEmpty ? parent.AL_TaxDate : ZDate.Today;

		ZGuid ITaxableTransactionLineBase.PK => parent.PK;

		BusinessObjectFactory ITaxableTransactionLineBase.Factory => parent.Factory;

		GlbBranch ITaxableTransactionLineBase.Branch => parent.TaxBranch ?? parent.Branch;

		AccChargeCode ITaxableTransactionLineBase.ChargeCode => parent.ChargeCode;

		ZGuid ITaxableTransactionLineBase.JobPK => parent.AL_JH;

		ZGuid ITaxableTransactionLine.CopiedFromPK => parent.CopiedFromPK;

		ZString ITaxableTransactionLine.SupplyType => parent.AL_SupplyType;

		AccChargeTaxOverrideMatcher.TaxCalculationParameters ITaxableTransactionLineBase.GetTaxCalculationParameters() => parent.GetTaxCalculationParameters();
	}
}
