using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class MutexErrorEventArgs : EventArgs
	{
		public MutexErrorEventArgs(string errorMessage)
			: base()
		{
			this.ErrorMessage = errorMessage;
		}

		public readonly string ErrorMessage;
	}

	public delegate void MutexErrorEventHandler(InvoicingLineBase invoiceLine, MutexErrorEventArgs e);

	public class InvoicingLineBaseCollection : DependentTransactionLineCollection
	{
		public InvoicingLineBaseCollection(BusinessObject parent)
			: base(parent, new ZQuery())
		{
		}

		public new InvoicingLineBase this[int index]
		{
			get { return (InvoicingLineBase)Elements[index]; }
		}

		public void SetGSTReadOnlyStateForAllLines()
		{
			using (SuspendListChanged())
			{
				foreach (InvoicingLineBase line in this)
				{
					line.UpdateGSTReadOnlyState();
				}
			}
		}

		public void UpdateTaxAndTotalReadOnlyStateForAllLines()
		{
			using (SuspendListChanged())
			{
				foreach (InvoicingLineBase line in this)
				{
					line.AL_OverseasTotalInfo.RefreshBinding();

					if (CountrySpecificValidationHelper.ShouldValidateAL_OSTaxAmount(line))
					{
						var validation = (InvoicingLineBaseValidation)line.Validation;
						validation.ValidateAL_OSTaxAmount();
					}
					line.AL_OSTaxAmountInfo.RefreshBinding();
				}
			}
		}

		public void RaiseMutexError(InvoicingLineBase invoiceLine, string errorMessage)
		{
			if (MutexError != null)
			{
				MutexError(invoiceLine, new MutexErrorEventArgs(errorMessage));
			}
		}

		public void CalculateGSTAndWHTForAllLines()
		{
			using (SuspendListChanged())
			{
				foreach (InvoicingLineBase line in this)
				{
					if (!line.IsPopulatedFromImportedApportionment)
					{
						line.CalculateGST(line.IsGSTMandatory);
						line.RecalculateWHTRate();
					}
				}
			}
		}

		public InvoicingBase InvoicingBase
		{
			get { return Master as InvoicingBase; }
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			if (Master != null)
			{
				using (Master.GetValidationSuspender())
				{
					base.RemoveAndDelete(elementToDelete);
				}
			}
			else
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}

		protected override bool AllowNewCore
		{
			get { return !(InvoicingBase != null && (InvoicingBase.IsInvoiceApproving || InvoicingBase.IsAmendingARCreditNoteForComplianceDocument || InvoicingBase.IsPostingMiscARCreditNoteLinkedToARInvoiceForComplianceDocument)) && base.AllowNewCore; }
		}

		protected override bool AllowRemoveCore
		{
			get { return !(InvoicingBase?.IsInvoiceApproving ?? false) && base.AllowRemoveCore; }
		}

		protected override void SetDefaultsForNewChildCore(BusinessObject child)
		{
			InvoicingLineBase newLine = (InvoicingLineBase)child;

			if (!JobDefaultingSuspender.IsSuspended)
			{
				if (Count > 0)
				{
					InvoicingLineBase previousLine = this[Count - 1];
					if (previousLine != null)
					{
						newLine.SetDefaultAL_JH(previousLine.AL_JH);
					}
				}
			}

			if (InvoicingBase != null &&
				InvoicingBase.DefaultChargeCodeForLines.IsValid &&
				!InvoicingBase.DefaultChargeCodeForLines.IsEmpty)
			{
				newLine.GenericCharge = InvoicingBase.DefaultChargeCodeForLines;
			}

			if (InvoicingBase.SupportMultiPeriodApportionment)
			{
				newLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Default;
			}
			newLine.AL_OSAmountInfo.ValueChanged += AL_OSAmountInfo_ValueChanged;
			base.SetDefaultsForNewChildCore(child);
		}

		protected override void SetDefaultExchangeRate(DependentTransactionLine line)
		{
			((InvoicingLineBase)line).SetExchangeRate();
		}

		void AL_OSAmountInfo_ValueChanged(object sender, EventArgs e)
		{
			if (AmountChanged != null)
			{
				AmountChanged(sender, e);
			}
		}

		public event EventHandler AmountChanged;
		public event MutexErrorEventHandler MutexError;

		protected override DisposableList GetAdditionalListChangedSuspenders()
		{
			var disposableList = base.GetAdditionalListChangedSuspenders();
			disposableList.Add(JobDefaultingSuspender.GetSuspender());
			if (InvoicingBase != null)
			{
				disposableList.Add(InvoicingBase.ValidateAH_OSTotalAmountSuspenderForBatchLineChanges.GetSuspender());
				disposableList.Add(InvoicingBase.SetLinesAL_GSTVATBasisSuspender.GetSuspender());
			}
			return disposableList;
		}

		FunctionalitySuspender JobDefaultingSuspender
		{
			get { return jobDefaultingSuspender ?? (jobDefaultingSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender jobDefaultingSuspender;

		#region Apportionment

		public event ApportionedLineRemovedEventHander ApportionedLineRemoved;

		protected void RaiseApportionedLineRemoved(InvoicingLineBase removedLine)
		{
			if (ApportionedLineRemoved != null)
			{
				ApportionedLineRemoved(this, new ApportionedLineRemovedEventArgs(removedLine));
			}
		}

		public event EventHandler ShowJobChargesForImportEvent;
		public event ApportionedInvoiceLineModifiedEventHandler ApportionedInvoiceLineModified;

		internal bool ShouldShowJobChargesForImport
		{
			get { return ShowJobChargesForImportEvent != null; }
		}

		public void OnApportionedInvoiceLineModified(InvoicingLineBase modifiedLine)
		{
			if (ApportionedInvoiceLineModified != null)
			{
				ApportionedInvoiceLineModified(modifiedLine, EventArgs.Empty);
			}
		}

		protected void OnShowJobChargesForImportEvent(object sender, EventArgs e)
		{
			if (ShowJobChargesForImportEvent != null)
			{
				ShowJobChargesForImportEvent(sender, e);
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			InvoicingLineBase removedLine = bizO as InvoicingLineBase;
			if (removedLine != null)
			{
				InvoicingBase.TransactionLinesDefaultExRates.Remove(removedLine.PK);

				if (IsDeletingForDataRefresh && (InvoicingBase.AH_TransactionType == TransactionTypes.IncompleteInvoice || InvoicingBase.AH_TransactionType == TransactionTypes.IncompleteCreditNote))
				{
					return;
				}

				if (removedLine.IsPopulatedFromImportedApportionment || (!removedLine.IsPopulatedFromImportedApportionment && !removedLine.ImportedApportionmentID.IsEmpty))
				{
					RaiseApportionedLineRemoved(removedLine);
				}
			}
		}

		#endregion
	}
}
