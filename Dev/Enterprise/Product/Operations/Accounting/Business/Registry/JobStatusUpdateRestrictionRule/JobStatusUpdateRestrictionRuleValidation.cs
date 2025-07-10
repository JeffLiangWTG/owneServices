using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Registry.Business
{
	public class JobStatusUpdateRestrictionRuleValidation : ZValidation
	{
		public JobStatusUpdateRestrictionRuleValidation(JobStatusUpdateRestrictionRule parent) : base(parent)
		{
			Parent = parent;
		}

		public override Type AutoValidationType => typeof(JobStatusUpdateRestrictionRuleValidation);

		readonly JobStatusUpdateRestrictionRule Parent;

		public override void ValidateAll()
		{
			ValidateWorking();
			ValidateWorkOnHold();
			ValidateInvoiceOnHold();
			ValidateCustomsProcessActive();
			ValidateJobReadyForRevenuePosting();
			ValidateJobReadyForCostPosting();
			ValidateJobReadyForRevenueAndCostPosting();
			ValidateJobInvoiced();
			ValidateJobReadyForDelivery();
			ValidateComplete();
			ValidateJobReadyForFinancialClosure();
			ValidateScheduledForArchive();
		}

		void ValidateProperty(ZPropertyInfo info)
		{
			ListValidation.ErrorIfInvalidCode(info);
			if (!info.ReadOnly)
			{
				MandatoryValidation.CheckEntered(info);
			}
			else
			{
				MandatoryValidation.CheckNotEntered(info);
			}
		}

		#region Working

		public void ValidateWorking()
		{
			ValidateCalculatedProperty(Parent.WorkingInfo);
		}

		protected void CheckWorking()
		{
			ValidateProperty(Parent.WorkingInfo);
		}

		#endregion

		#region WorkOnHold

		public void ValidateWorkOnHold()
		{
			ValidateCalculatedProperty(Parent.WorkOnHoldInfo);
		}

		protected void CheckWorkOnHold()
		{
			ValidateProperty(Parent.WorkOnHoldInfo);
		}

		#endregion

		#region InvoiceOnHold

		public void ValidateInvoiceOnHold()
		{
			ValidateCalculatedProperty(Parent.InvoiceOnHoldInfo);
		}

		protected void CheckInvoiceOnHold()
		{
			ValidateProperty(Parent.InvoiceOnHoldInfo);
		}

		#endregion

		#region CustomsProcessActive

		public void ValidateCustomsProcessActive()
		{
			ValidateCalculatedProperty(Parent.CustomsProcessActiveInfo);
		}

		protected void CheckCustomsProcessActive()
		{
			ValidateProperty(Parent.CustomsProcessActiveInfo);
		}

		#endregion

		#region JobReadyForRevenuePosting

		public void ValidateJobReadyForRevenuePosting()
		{
			ValidateCalculatedProperty(Parent.JobReadyForRevenuePostingInfo);
		}

		protected void CheckJobReadyForRevenuePosting()
		{
			ValidateProperty(Parent.JobReadyForRevenuePostingInfo);
		}

		#endregion

		#region JobReadyForCostPosting

		public void ValidateJobReadyForCostPosting()
		{
			ValidateCalculatedProperty(Parent.JobReadyForCostPostingInfo);
		}

		protected void CheckJobReadyForCostPosting()
		{
			ValidateProperty(Parent.JobReadyForCostPostingInfo);
		}

		#endregion

		#region JobReadyForRevenueAndCostPosting

		public void ValidateJobReadyForRevenueAndCostPosting()
		{
			ValidateCalculatedProperty(Parent.JobReadyForRevenueAndCostPostingInfo);
		}

		protected void CheckJobReadyForRevenueAndCostPosting()
		{
			ValidateProperty(Parent.JobReadyForRevenueAndCostPostingInfo);
		}

		#endregion

		#region JobInvoiced

		public void ValidateJobInvoiced()
		{
			ValidateCalculatedProperty(Parent.JobInvoicedInfo);
		}

		protected void CheckJobInvoiced()
		{
			ValidateProperty(Parent.JobInvoicedInfo);
		}

		#endregion

		#region JobReadyForDelivery

		public void ValidateJobReadyForDelivery()
		{
			ValidateCalculatedProperty(Parent.JobReadyForDeliveryInfo);
		}

		protected void CheckJobReadyForDelivery()
		{
			ValidateProperty(Parent.JobReadyForDeliveryInfo);
		}

		#endregion

		#region Complete

		public void ValidateComplete()
		{
			ValidateCalculatedProperty(Parent.CompleteInfo);
		}

		protected void CheckComplete()
		{
			ValidateProperty(Parent.CompleteInfo);
		}

		#endregion

		#region JobReadyForFinancialClosure

		public void ValidateJobReadyForFinancialClosure()
		{
			ValidateCalculatedProperty(Parent.JobReadyForFinancialClosureInfo);
		}

		protected void CheckJobReadyForFinancialClosure()
		{
			ValidateProperty(Parent.JobReadyForFinancialClosureInfo);
		}

		#endregion

		#region ScheduledForArchive

		public void ValidateScheduledForArchive()
		{
			ValidateCalculatedProperty(Parent.ScheduledForArchiveInfo);
		}

		protected void CheckScheduledForArchive()
		{
			ValidateProperty(Parent.ScheduledForArchiveInfo);
		}

		#endregion
	}
}
