#if DEBUG

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public partial class WNZDDRFileGenerator
	{
		public static string Separator_ForTestOnly => Separator;

		public static string HeaderRecordType_ForTestOnly => HeaderRecordType;

		public static string DetailRecordType_ForTestOnly => DetailRecordType;
	}
}

#endif
