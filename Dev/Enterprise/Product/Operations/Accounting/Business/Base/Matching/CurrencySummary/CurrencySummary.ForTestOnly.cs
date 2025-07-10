#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public partial class CurrencySummary
	{
		[BusinessObjectTestExclude]
		public Transaction.IMatchingCollection FMatchingTransactions_ForTestOnly
		{
			get { return fMatchingTransactions; }
			set { fMatchingTransactions = value; }
		}
	}
}

#endif
