using System.Linq;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PeriodicInvoiceItemValidation : TransactionHeaderWithLinesValidation
	{
		public PeriodicInvoiceItemValidation(TransactionHeader parent)
			: base(parent)
		{
			fParent = parent;
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			string cantReverseErrorMessage = Res.GetString("55a55b4a-e252-4dd1-8f2e-2d50d6f20f80", "The transaction can't be reversed and therefore can't be included in Periodic Invoice");
			ParentAsInvoicingBase.RemoveRowError(cantReverseErrorMessage);
			if (ParentAsInvoicingBase != null)
			{
				ReversingBase reverser = new ReversingFactory().NewReversing(ParentAsInvoicingBase);
				if (ParentAsInvoicingBase.IncludeInThePeriodicInvoice && !reverser.CanReverseTransaction)
				{
					ParentAsInvoicingBase.AddRowError(cantReverseErrorMessage);
				}
			}

			if (ParentAsInvoicingBase.IncludeInThePeriodicInvoice && AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.Value.EnableBranchLevelPosting)
			{
				if (!ParentAsInvoicingBase.HasErrors
					&& !BranchLevelPostingHelper.DoesAllBranchesBelongToSamePostingGroup(AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting
																						, ParentAsInvoicingBase.Lines.Cast<DependentTransactionLine>().Select(x => x.AL_GB).ToHashSet()))
				{
					ParentAsInvoicingBase.AddRowError(Res.GetString("ab7b6be9-d034-48ad-bfbb-a1e91e1a14b9", "This Invoice cannot be included in Periodic Invoice as it has lines using branches with different Posting Groups."));
				}
			}
		}

		InvoicingBase ParentAsInvoicingBase
		{
			get { return fParent as InvoicingBase; }
		}

		readonly TransactionHeader fParent;
	}
}