using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionLineLookups : AccTransactionLinesLookups
	{
		public TransactionLineLookups(TransactionLine parent)
			: base(parent)
		{
		}

		public override AccTransactionHeaderCollection TransactionHeaders
		{
			get { return FindboxLookupCollections.GetTransactionHeaderCollection(Factory); }
		}
	}
}
