using System.IO;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public class ABAFileGenerator : DDRFileGenerator
	{
		public ABAFileGenerator(TextWriter writer, DirectDebitBatchHeader header)
			: base(writer, header)
		{
		}

		protected override string GetInstitutionName(DirectDebitBatchHeader row)
		{
			return row.BankAccount.AB_BankAbbreviation;
		}
	}
}
