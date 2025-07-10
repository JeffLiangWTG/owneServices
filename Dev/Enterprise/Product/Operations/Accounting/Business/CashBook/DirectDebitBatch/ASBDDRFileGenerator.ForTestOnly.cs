#if DEBUG

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public partial class ASBDDRFileGenerator
	{
		public string GetFileHashingTotal_ForTestOnly(DirectDebitBatchHeader row)
		{
			return GetFileHashingTotal(row);
		}
	}
}

#endif
