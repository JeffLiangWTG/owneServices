using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public class InvoicingBaseForDisplayOtherTaxes : ITransactionWithLinesForDisplayOtherTaxes
	{
		internal InvoicingBaseForDisplayOtherTaxes(InvoicingBase invoice)
		{
			parent = invoice;
		}
		readonly InvoicingBase parent;

		InvoicingBaseTaxRecordParent TaxRecordParent => TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(parent);

		event EventHandler ITransactionWithLinesForDisplayOtherTaxes.OnOtherTaxesCalculatedBeforePosting_Changed
		{
			add
			{
				TaxRecordParent.OnOtherTaxesCalculatedBeforePosting_Changed += value;
			}
			remove
			{
				TaxRecordParent.OnOtherTaxesCalculatedBeforePosting_Changed -= value;
			}
		}

		AccTransactionHeader ITransactionWithLinesForDisplayOtherTaxes.Transaction => parent;

		bool ITransactionWithLinesForDisplayOtherTaxes.ShouldShowTransactionLines => TaxRecordParent.ShouldCalculateTaxTransactions;

		IEnumerable<TransactionLineForOtherTaxesDisplay> ITransactionWithLinesForDisplayOtherTaxes.Lines =>
			parent.Lines.Cast<InvoicingLineBase>()
			.Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(l));

		bool ITransactionWithLinesForDisplayOtherTaxes.IsTaxTransactionsCalculatedBeforePosting => TaxRecordParent.IsTaxTransactionsCalculatedBeforePosting;

		public AccTaxRecordTransactionLinePivotForDisplay TaxRecordTransactionLinePivotForDisplay
		{
			get
			{
				if (taxRecordTransactionLinePivotForDisplay == null)
				{
					taxRecordTransactionLinePivotForDisplay = new AccTaxRecordTransactionLinePivotForDisplay(this, TaxRecordParent);
					parent.RegisterTaxTransactionCollectionAsEditableChild();
				}
				return taxRecordTransactionLinePivotForDisplay;
			}
		}
		AccTaxRecordTransactionLinePivotForDisplay taxRecordTransactionLinePivotForDisplay;
	}
}
