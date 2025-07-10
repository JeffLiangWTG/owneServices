using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	[ModuleID(ModuleId.ARTransaction)]
	public class UnApprovedFilteredTransactionHeaderCollectionView : FilteredTransactionHeaderCollectionView
	{
		public UnApprovedFilteredTransactionHeaderCollectionView(TransactionHeaderCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		#region IsThisPartOfTheCollection

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var result = true;
			var transactionHeader = element as TransactionHeader;

			if (transactionHeader != null && !Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed)
			{
				if (transactionHeader.AH_TransactionType == TransactionTypes.UAInvoice || transactionHeader.AH_TransactionType == TransactionTypes.UACreditNote)
				{
					if (transactionHeader.Branch != null && transactionHeader.Department != null && (transactionHeader.Branch != GlbBranch.CurrentBranch || transactionHeader.Department != GlbDepartment.CurrentDepartment))
					{
						result = AllowedToLogin(transactionHeader.Branch, transactionHeader.Department);
					}
				}
				else if (transactionHeader.AH_TransactionType == TransactionTypes.Invoice || transactionHeader.AH_TransactionType == TransactionTypes.CreditNote)
				{
					var receivingBranch = transactionHeader.ReceivingBranch.IsValid ? Factory.Load<GlbBranch>(transactionHeader.ReceivingBranch) : null;
					var receivingDepartment = transactionHeader.ReceivingDepartment.IsValid ? Factory.Load<GlbDepartment>(transactionHeader.ReceivingDepartment) : null;

					if (receivingBranch == null || receivingDepartment == null)
					{
						result = false;
					}
					else if (receivingBranch != GlbBranch.CurrentBranch || receivingDepartment != GlbDepartment.CurrentDepartment)
					{
						result = AllowedToLogin(receivingBranch, receivingDepartment);
					}
				}
			}
			return result;
		}

		#endregion
	}
}
