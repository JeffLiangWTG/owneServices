using System.IO;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public class BBLDDRFileGenerator : DDRFileGenerator
	{
		public BBLDDRFileGenerator(TextWriter writer, DirectDebitBatchHeader header)
			: base(writer, header)
		{
		}

		#region WriteDetailRecord

		protected override string GetDetailRecordWHTField(TransactionHeader row)
		{
			return GetFormattedAmountStringCore(row.AH_OSWHTAmount.ToString(), 6, '0');
		}

		#endregion
	}
}