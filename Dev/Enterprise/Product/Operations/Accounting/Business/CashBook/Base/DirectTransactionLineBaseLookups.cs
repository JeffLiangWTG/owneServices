using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook
{
	public class DirectTransactionLineBaseLookups : TransactionLineLookups
	{
		public DirectTransactionLineBaseLookups(DirectTransactionLineBase directTransactionLine)
			: base(directTransactionLine)
		{
		}

		public override AccGLHeaderCollection GLHeaders
		{
			get
			{
				return FindboxLookupCollections.GetBSH_PnL_Active_DirectPosting_GLHeaderCollection(Factory, ParentLine.ShowGLAccountsForImportAction);
			}
		}

		public override GlbBranchCollection Branches
		{
			get
			{
				return FindboxLookupCollections.GetActiveBranchForCompanyCollection(Factory);
			}
		}

		public override GlbDepartmentCollection Departments
		{
			get
			{
				return FindboxLookupCollections.GetDepartmentCollection_ActiveOnly(Factory);
			}
		}

		public override AccTransactionHeaderCollection TransactionHeaders
		{
			get
			{
				if ((ParentLine).ParentTransactionHeader is BankReconDirectPayment)
				{
					return new BankReconDirectPaymentCollection(Factory);
				}
				else if ((ParentLine).ParentTransactionHeader is BankReconDirectReceipt)
				{
					return new BankReconDirectReceiptCollection(Factory);
				}
				else
				{
					return base.TransactionHeaders;
				}
			}
		}

		DirectTransactionLineBase ParentLine => (DirectTransactionLineBase)Parent;
	}
}
