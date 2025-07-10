using System.IO;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public class HSBCDDRFileGenerator : DDRFileGenerator
	{
		public HSBCDDRFileGenerator(TextWriter writer, DirectDebitBatchHeader header)
			: base(writer, header)
		{
		}

		protected override string GetDescriptionOfEntries()
		{
			return "PAY" + Header.AH_TransactionNum.PadLeft(9, '0');
		}
	}
}