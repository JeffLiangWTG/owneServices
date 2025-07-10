using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;

namespace Enterprise.Accounting.Module
{
	public abstract partial class CreditNoteInvoiceController : InvoicingBaseController
	{
		protected virtual CreditNoteForm GetNewCreditNoteForm(CreditNote creditNote)
		{
			return new CreditNoteForm(creditNote);
		}

		protected virtual InvoiceForm GetNewInvoiceForm(Invoice invoice)
		{
			return new InvoiceForm(invoice);
		}

		protected override void BeforeBaseReversing(IBusiness transaction)
		{
			((InvoicingBase)transaction).SecurityOverrideProvider =
				IsMultipleReversing ? MultipleReversingProvider.GetSecurityOverrideProvider(GetSecurityOverrideProviderForReversing(MultipleReversingProvider))
				: GetSecurityOverrideProviderForReversing(transaction as InvoicingBase);
		}

		protected virtual InteractiveSecurityOverrideProvider GetSecurityOverrideProviderForReversing(MultipleReversingProviderForHeader multipleReversingProvider)
		{
			return new InvoicingSecurityOverrideProvider(multipleReversingProvider);
		}

		protected virtual InteractiveSecurityOverrideProvider GetSecurityOverrideProviderForReversing(InvoicingBase invoicingBase)
		{
			return new InvoicingSecurityOverrideProvider(invoicingBase);
		}

		protected override void AfterBaseReversing(IBusiness transaction1)
		{
			InvoicingBase transaction = (InvoicingBase)transaction1;
			if (transaction.ReverseTransaction != null)
			{
				((InvoicingBase)transaction.ReverseTransaction).SecurityOverrideProvider = transaction.SecurityOverrideProvider;
			}
		}

		protected virtual string AmendSecurityCheckPointCode
		{
			get { return null; }
		}

		public override SecurityCheckpoint GetCheckPointForNew(BusinessObject bizObject)
		{
			SecurityCheckpoint result = base.GetCheckPointForNew(bizObject);

			if (this.AmendSecurityCheckPointCode != null)
			{
				var invoice = bizObject as InvoicingBase;
				var amending = invoice as IAmending;

				if (invoice != null && amending != null && amending.IsAmendingTransaction)
				{
					var originalTransaction = amending.OriginalTransaction as InvoicingBase;

					if (invoice.InvoicingJob != null && invoice.InvoicingJob.PlugInData.InvoicingSupporter != null && invoice.InvoicingJob.PlugInData.InvoicingSupporter.JobInvoicingSecurity != null)
					{
						var pluginSecurity = invoice.InvoicingJob.PlugInData.InvoicingSupporter.JobInvoicingSecurity;
						JobInvoicingSecurityHelper securityTestHelper = new JobInvoicingSecurityHelper(pluginSecurity);
						result = securityTestHelper.GetInvSecurity(this.AmendSecurityCheckPointCode);
					}
					else if (originalTransaction != null)
					{
						if (originalTransaction.IsConsolInvoice)
						{
							var securityTestHelper = new JobInvoicingSecurityHelper(Env.Security.MaintainConsolJobInvoicing);
							result = securityTestHelper.GetInvSecurity(this.AmendSecurityCheckPointCode);
						}
						else if (originalTransaction.IsPeriodicInvoice && originalTransaction.IsBelongToMultipleJobs)
						{
							var foundCheckPoint = GetAmendingCheckPointFromMultipleJobs(originalTransaction);
							if (foundCheckPoint != null)
							{
								result = foundCheckPoint;
							}
						}
					}
				}
			}

			return result;
		}

		SecurityCheckpoint GetAmendingCheckPointFromMultipleJobs(InvoicingBase originalTransactions)
		{
			SecurityCheckpoint result = null;
			var invoicingJobs = originalTransactions.Lines.Cast<InvoicingLineBase>().Where(x => x.InvoicingJob != null).Select(z => z.InvoicingJob).Distinct();

			foreach (var job in invoicingJobs)
			{
				var invocingSupporter = job.GetInvoicingSupporter();
				var pluginSecurity = invocingSupporter != null ? invocingSupporter.JobInvoicingSecurity : null;
				if (pluginSecurity != null)
				{
					var securityHelper = new JobInvoicingSecurityHelper(pluginSecurity);
					result = securityHelper.GetInvSecurity(this.AmendSecurityCheckPointCode);
					if (!result.IsAllowed)	// Found a Job which does not allow amending for the current User
					{
						break;
					}
				}
			}

			return result;
		}
	}
}
