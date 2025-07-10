using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionLineSubAccountLookups : AccTransactionLineSubAccountLookups
	{
		public TransactionLineSubAccountLookups(TransactionLineSubAccount parent) : base(parent)
		{
			Parent = parent;
		}

		protected new TransactionLineSubAccount Parent;

		public IBusinessObjectCollection SubAccountList => SubAccountHelper.GetSubAccountList(Factory, Parent.AL1_SubClassParentTableCode);

		public CodeDescriptionPairList SubAccountTypeList => SubAccountHelper.GetSubAccountTypeList(Factory);
	}
}
