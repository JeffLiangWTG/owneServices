using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class AccTaxRecordTransactionLinePivotForDisplay : NonPersistentBusinessObject
	{
		public AccTaxRecordTransactionLinePivotForDisplay(ITransactionWithLinesForDisplayOtherTaxes transactionWithLines, ITaxRecordParent taxRecordParent) : base(transactionWithLines.Transaction.Factory)
		{
			this.transactionWithLines = transactionWithLines;
			this.taxRecordParent = taxRecordParent;
		}

		readonly ITransactionWithLinesForDisplayOtherTaxes transactionWithLines;
		readonly ITaxRecordParent taxRecordParent;

#if DEBUG
		public ITransactionWithLinesForDisplayOtherTaxes TransactionWithLines_ForTestOnly => transactionWithLines;
#endif

		public AccTaxTransactionCollection TaxTransactionCollection
		{
			get
			{
				if (taxTransactionCollection == null)
				{
					taxTransactionCollection = new AccTaxTransactionCollection(Factory, transactionWithLines.Transaction, taxRecordParent);
				}

				return taxTransactionCollection;
			}
		}
		AccTaxTransactionCollection taxTransactionCollection;

		public TransactionLineForOtherTaxesDisplayCollection TransactionLinesForOtherTaxesDisplay
		{
			get
			{
				if (transactionLinesForOtherTaxesDisplay == null)
				{
					transactionLinesForOtherTaxesDisplay = new TransactionLineForOtherTaxesDisplayCollection(Factory);
					if (transactionWithLines.IsTaxTransactionsCalculatedBeforePosting || TaxTransactionCollection.Any())
					{
						transactionLinesForOtherTaxesDisplay.AddRange(transactionWithLines.Lines);
					}
					if (transactionWithLines.ShouldShowTransactionLines)
					{
						transactionWithLines.OnOtherTaxesCalculatedBeforePosting_Changed += OnOtherTaxesCalculatedBeforePosting_Changed;
					}
				}
				return transactionLinesForOtherTaxesDisplay;
			}
		}

#if DEBUG
		public TransactionLineForOtherTaxesDisplayCollection TransactionLinesForOtherTaxesDisplay_ForTestOnly => transactionLinesForOtherTaxesDisplay;
#endif
		TransactionLineForOtherTaxesDisplayCollection transactionLinesForOtherTaxesDisplay;

		void OnOtherTaxesCalculatedBeforePosting_Changed(object sender, EventArgs e)
		{
			if (transactionWithLines.IsTaxTransactionsCalculatedBeforePosting)
			{
				TransactionLinesForOtherTaxesDisplay.AddRange(transactionWithLines.Lines);
			}
			else if (!TaxTransactionCollection.Any())
			{
				TransactionLinesForOtherTaxesDisplay.RemoveAll();
			}
		}

#if DEBUG
		public void UnhookEventHandler_OnOtherTaxesCalculatedBeforePosting_Changed_ForTestOnly()
		{
			if (transactionWithLines != null)
			{
				transactionWithLines.OnOtherTaxesCalculatedBeforePosting_Changed -= OnOtherTaxesCalculatedBeforePosting_Changed;
			}
		}
#endif
	}
}
