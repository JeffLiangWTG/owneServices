//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCollectionOrderValidation
//
//    This class should be used for overriding validation in AutoAccCollectionOrderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.Riba
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Environment;
	using MasterFiles.Business;
	using static Enterprise.Accounting.Business.Riba.AccCollectionOrder;

	public class AccCollectionOrderValidation : AutoAccCollectionOrderValidation
	{
		public AccCollectionOrderValidation(AutoAccCollectionOrder parent) : base(parent)
		{
		}

		AccCollectionOrder CollectionOrder
		{
			get	{ return Parent as AccCollectionOrder; }
		}

		protected override void CheckACO_Amount()
		{
			base.CheckACO_Amount();
			if (!Parent.ACO_AmountInfo.HasErrors() && CollectionOrder.IncludeInBatch)
			{
				if (Parent.ACO_AmountInfo.HasChanges && Parent.ACO_DepositedDate.IsValid)
				{
					Parent.ACO_AmountInfo.AddError(AccountingConstants.OrderAlreadyCompletedErrorMessage);
				}
				else if (Parent.ACO_Amount == 0)
				{
					Parent.ACO_AmountInfo.AddError(Res.GetString("8b9dbbc7-46ad-40b7-a463-83316a44f9a4", "Order total amount can not be 0."));
				}
				else if (Parent.ACO_Amount < 0)
				{
					Parent.ACO_AmountInfo.AddError(Res.GetString("0b5a27c6-b3e3-43ff-9a12-79923dc38637", "Order total amount can not be negative."));
				}
				else if (AccountingMasterFilesRegistry.Instance.CollectionOrderMinimumAmount.Value > Parent.ACO_Amount
					&& !Env.Security.CollectionOrderAllowBelowMinAmount.IsAllowed)
				{
					Parent.ACO_AmountInfo.AddError(Res.GetString("7d9d3241-68d2-4104-b9b2-30460d106b6f", "Please revise or exclude this collection order from this batch.The amount for this collection order is less than the minimum amount '{0}' in Local Currency stipulated in the ‘Minimum Collection Order Amount’ registry and you do not have access right to ‘Manage > Receivables > Collection Batch > Collection Order > Allow Order Below Minimum  Amount’.",
						AccountingMasterFilesRegistry.Instance.CollectionOrderMinimumAmount.Value));
				}
			}
		}

		protected override void CheckACO_CollectionDate()
		{
			base.CheckACO_CollectionDate();
			if (!Parent.ACO_CollectionDateInfo.HasErrors() && !Parent.ACO_IsCancelled && CollectionOrder.IncludeInBatch)
			{
				if (Parent.ACO_CollectionDateInfo.HasChanges && Parent.ACO_DepositedDate.IsValid)
				{
					Parent.ACO_CollectionDateInfo.AddError(AccountingConstants.OrderAlreadyCompletedErrorMessage);
				}
				else if (!CollectionOrder.IsInDatabase || Parent.ACO_CollectionDateInfo.HasChanges)
				{
					if (Parent.ACO_CollectionDate < ZDate.Today)
					{
						Parent.ACO_CollectionDateInfo.AddError(Res.GetString("f787a011-7bf2-40fe-aa88-ff72b58c466b", "Collection date can not less than today."));
					}
				}
			}
		}

		protected override void CheckACO_OH_Debtor()
		{
			if (CollectionOrder.DebtorValidationType == DebtorValidation.DebtorIsRequired)
			{
				base.CheckACO_OH_Debtor();
				MandatoryValidation.CheckEntered(Parent.ACO_OH_DebtorInfo);
			}
			else if (CollectionOrder.DebtorValidationType == DebtorValidation.DebtorShouldBeEmpty && !Parent.ACO_OH_Debtor.IsEmpty)
			{
				Parent.ACO_OH_DebtorInfo.AddWarning(Res.GetString("606ABCDD-C9D3-428A-B83A-60D265EB5316", "This collection order contains transactions belonging to more than one debtor, this value must be empty."));
			}
		}
	}
}

