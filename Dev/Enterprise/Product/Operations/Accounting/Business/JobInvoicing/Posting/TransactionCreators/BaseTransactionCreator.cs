using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public abstract class BaseTransactionCreator
	{
		protected BaseTransactionCreator(Job job)
			: this(job, null, false)
		{
		}

		protected BaseTransactionCreator(Job job, IJobCostingPlugIn consol, bool consolHasJobOnHold)
		{
			this.Job = job;
			this.Factory = job.Factory;
			this.PostingTime = ZDateTime.Now;
			this.Consol = consol;
			this.ConsolHasJobOnHold = consolHasJobOnHold;
		}

		public void ChangeJob(Job job)
		{
			Job = job;
			ResetCharges();
		}

		protected readonly IJobCostingPlugIn Consol;
		protected readonly bool ConsolHasJobOnHold;

		protected bool IsConsol
		{
			get { return Consol != null; }
		}

		protected BusinessObjectFactory Factory;
		protected Job Job;
		protected ZDateTime PostingTime;

		List<Charge> fCharges;
		protected internal List<Charge> Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = LoadCharges();
				}

				return fCharges;
			}
		}

		void ResetCharges()
		{
			fCharges = null;
		}

		protected List<Charge> LoadCharges()
		{
			var charges = new List<Charge>();
			foreach (Charge charge in Job.Charges)
			{
				if (IsChargeApplicable(charge))
				{
					charges.Add(charge);
				}
			}
			return charges;
		}

		public bool CreateTransactions(TransactionCreatorHashtable transactions)
		{
			if (Job.IsWorkOnHold)
			{
				return false;
			}

			return CreateTransactionsCore(transactions, false);
		}

		protected abstract bool CreateTransactionsCore(TransactionCreatorHashtable transactions, bool isMultiJobOperationInProgress);

		#region Is Charge Applicable

		protected virtual bool IsChargeApplicable(Charge charge)
		{
			return !charge.JR_IsApportioned ||
				(IsConsol &&
				!ConsolHasJobOnHold &&
				charge.JR_JH == Job.PK &&
				charge.ParentConsolCost != null &&
				charge.ParentConsolCost.E6_ParentID == Consol.PK &&
				!CheckContainsJobIsReadyForFinancialClosureWithoutPostSecurity(charge));
		}

		bool CheckContainsJobIsReadyForFinancialClosureWithoutPostSecurity(Charge charge)
		{
			var result = false;
			var parentConsolCost = charge.ParentConsolCost;

			if (parentConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Any(x => x.JobIsReadyForFinancialClosureWithoutPostSecurity))
			{
				result = true;
			}
			else if (!parentConsolCost.E6_InvoiceNum.IsEmpty)
			{
				var consolCostCollection = ((IBusinessObjectInternals)parentConsolCost).ParentCollections.FirstOrDefault(x => x is JobConsolCostCollection) as JobConsolCostCollection;
				if (consolCostCollection != null)
				{
					foreach (JobConsolCost consolCost in consolCostCollection)
					{
						if (consolCost.PK != parentConsolCost.PK &&
							consolCost.E6_OH_Creditor == parentConsolCost.E6_OH_Creditor &&
							consolCost.E6_InvoiceNum == parentConsolCost.E6_InvoiceNum &&
							consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Any(x => x.JobIsReadyForFinancialClosureWithoutPostSecurity))
						{
							result = true;
							break;
						}
					}
				}
			}

			return result;
		}

		#endregion
	}
}
