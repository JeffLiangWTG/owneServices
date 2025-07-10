#if DEBUG

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public partial class TransactionHeaderValidation
	{
		public TransactionHeader Parent_ForTestOnly
		{
			get { return Parent; }
			set { Parent = value; }
		}

		public void CheckAH_IsCancelled_ForTestOnly() => CheckAH_IsCancelled();
	}
}

#endif
