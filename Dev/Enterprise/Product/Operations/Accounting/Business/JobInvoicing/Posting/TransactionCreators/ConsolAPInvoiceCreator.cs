using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Accounting.Business.JobInvoicing.BranchLevelPostingHelper;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ConsolAPInvoiceCreator : ConsolBaseTransactionCreator
	{
		public ConsolAPInvoiceCreator(BusinessObjectFactory fallbackFactory, IEnumerable<Job> jobs, bool hasJobOnHold, IJobCostingPlugIn consol, JobConsolCostCollection consolCosts, bool createConsolCostOnly = false, IPostingJobTransactionsApprovalGUIProvider apInvoicePostGUIProvider = null)
			: base(fallbackFactory, jobs, hasJobOnHold, consol)
		{
			this.ConsolCosts = consolCosts;
			this.CreateConsolCostOnly = createConsolCostOnly;
			this.apInvoicePostGUIProvider = apInvoicePostGUIProvider;
		}

		readonly bool CreateConsolCostOnly;
		readonly JobConsolCostCollection ConsolCosts;
		protected readonly IPostingJobTransactionsApprovalGUIProvider apInvoicePostGUIProvider;

		public override bool CreateTransactions(TransactionCreatorHashtable transactions)
		{
			bool result = false;

			if (Jobs.Any())
			{
				var invoiceCreator = GetInvoiceCreator(Jobs.First());
				using (invoiceCreator.InitializeMultiJobOperation(transactions))
				{
					foreach (Job job in Jobs)
					{
						if (!job.IsWorkOnHold)
						{
							invoiceCreator.ChangeJob(job);
							invoiceCreator.CreateTransactionsForMultiJobOperation();
						}
					}
				}
				result |= invoiceCreator.IsInvoiceCreatedAndWontBeRepalcedByCreditNote;

				SetJobHeaderAndBranch(transactions);
			}

			return result;
		}

		protected virtual APInvoiceCreator GetInvoiceCreator(Job job)
		{
			return new APInvoiceCreator(job, Consol, HasJobOnHold, ConsolCosts, CreateConsolCostOnly, apInvoicePostGUIProvider);
		}

		#region Job / Invoice References

		protected void SetJobHeaderAndBranch(TransactionCreatorHashtable transactions)
		{
			foreach (object value in transactions.Values)
			{
				APInvoice invoice = value as APInvoice;
				if (invoice != null)
				{
					if (!invoice.LinesHaveSameJobHeader)
					{
						invoice.AH_Desc += " " + Res.GetString("813abe75-0f12-4326-955a-7eb01078a031", "Multiple Jobs");

						invoice.AH_JH = ZGuid.Empty;
					}

					var branch = ObjectFactory.Get<IAccountingDependencyFactory>().GetJobCostingPlugInHelpers().FindBranchFromConsolAgentsAndJobHeaders(Consol, invoice.AH_GC, Factory) ?? GlbBranch.CurrentBranch;
					invoice.AH_GB = branch.PK;
					ObjectFactory.Get<IAccountingDependencyFactory>().GetBranchLevelPostingHelper().SetTransactionHeaderBranch(invoice,	InvoiceProcessingLevelIsAllowingToResetBranch.Creation);
				}
			}
		}

		#endregion
	}
}
