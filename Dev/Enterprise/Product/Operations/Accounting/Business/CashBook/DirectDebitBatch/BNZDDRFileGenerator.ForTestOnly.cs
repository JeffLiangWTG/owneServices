#if DEBUG

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public partial class BNZDDRFileGenerator
	{
		public string GetFileHashingTotal_ForTestOnly(DirectDebitBatchHeader header)
		{
			return GetFileHashingTotal(header);
		}
	}
}

#endif
