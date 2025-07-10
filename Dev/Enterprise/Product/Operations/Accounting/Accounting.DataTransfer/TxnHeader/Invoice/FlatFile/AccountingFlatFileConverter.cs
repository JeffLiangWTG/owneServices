using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile
{
	public abstract class AccountingFlatFileConverter : FlatFileConverter
	{
		protected AccountingFlatFileConverter(INotifications notification, BusinessObjectFactory factory) : base(notification, factory)
		{
		}

		public ZBool CheckThatAllTransactionsAreExported
		{
			get { return fCheckThatAllTransactionsAreExported; }
		}

		protected abstract ZBool fCheckThatAllTransactionsAreExported
		{
			get;
		}

		public void ResetCounters()
		{
			fNumberOfInvoicesProcessed = 0;
			fNumberOfCreditNotesProcessed = 0;
			fNumberOfAdjustmentNotesProcessed = 0;
			fNumberOfWipPostingProcessed = 0;
			fNumberOfWipReversingProcessed = 0;
			fNumberOfAccrualPostingProcessed = 0;
			fNumberOfAccrualReversingProcessed = 0;
		}

		public ZInt NumberOfInvoicesProcessed
		{
			get { return fNumberOfInvoicesProcessed; }
		}

		protected ZInt fNumberOfInvoicesProcessed;

		public ZInt NumberOfCreditNotesProcessed
		{
			get { return fNumberOfCreditNotesProcessed; }
		}

		protected ZInt fNumberOfCreditNotesProcessed;

		public ZInt NumberOfAdjustmentNotesProcessed
		{
			get { return fNumberOfAdjustmentNotesProcessed; }
		}

		protected ZInt fNumberOfAdjustmentNotesProcessed;

		public ZInt NumberOfWipPostingProcessed
		{
			get { return fNumberOfWipPostingProcessed; }
		}

		protected ZInt fNumberOfWipPostingProcessed;

		public ZInt NumberOfWipReversingProcessed
		{
			get { return fNumberOfWipReversingProcessed; }
		}

		protected ZInt fNumberOfWipReversingProcessed;

		public ZInt NumberOfAccrualPostingProcessed
		{
			get { return fNumberOfAccrualPostingProcessed; }
		}

		protected ZInt fNumberOfAccrualPostingProcessed;

		public ZInt NumberOfAccrualReversingProcessed
		{
			get { return fNumberOfAccrualReversingProcessed; }
		}

		protected ZInt fNumberOfAccrualReversingProcessed;

		/// <summary>
		/// Only use this method if you treat all TxnHeaders the same way in your converter.
		/// If you differentiate between the different types (INV, CRD, ADJ), then you should increase the number
		/// for each type individually as it is processed.
		/// </summary>
		/// <param name="txnHeader">The TxnHeader that has been processed by your converter</param>
		protected void TxnHeaderHasBeenProcessed(Xsd.TxnHeader txnHeader)
		{
			switch (txnHeader.TxnType)
			{
				case Xsd.TxnType.INV:
					fNumberOfInvoicesProcessed++;
					break;

				case Xsd.TxnType.CRD:
					fNumberOfCreditNotesProcessed++;
					break;

				case Xsd.TxnType.ADJ:
					fNumberOfAdjustmentNotesProcessed++;
					break;
			}
		}

		/// <summary>
		/// Only use this method if you treat all WipOrAccruals the same way in your converter.
		/// If you differentiate between the different types, then you should increase the number
		/// for each type individually as it is processed.
		/// </summary>
		/// <param name="wipOrAccrual">The WipOrAccrual that has processed by your converter</param>
		protected void WipAccrualHasBeenProcessed(Xsd.WipOrAccrual wipOrAccrual)
		{
			if (wipOrAccrual.LineType == Xsd.WipOrAccrualLineType.REV)
			{
				if (wipOrAccrual.PostOrReverse == Xsd.WipOrAccrualPostOrReverse.P)
				{
					fNumberOfWipPostingProcessed++;
				}
				else if (wipOrAccrual.PostOrReverse == Xsd.WipOrAccrualPostOrReverse.R)
				{
					fNumberOfWipReversingProcessed++;
				}
			}
			else if (wipOrAccrual.LineType == Xsd.WipOrAccrualLineType.CST)
			{
				if (wipOrAccrual.PostOrReverse == Xsd.WipOrAccrualPostOrReverse.P)
				{
					fNumberOfAccrualPostingProcessed++;
				}
				else if (wipOrAccrual.PostOrReverse == Xsd.WipOrAccrualPostOrReverse.R)
				{
					fNumberOfAccrualReversingProcessed++;
				}
			}
		}
	}
}
