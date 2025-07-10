using System.IO;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public class ABAFileGeneratorWithDetailLine : DDRFileGeneratorWithDetailLine
	{
		public ABAFileGeneratorWithDetailLine(TextWriter writer, DirectDebitBatchHeader header)
			: base(writer, header)
		{
		}

		protected override string GetInstitutionName(DirectDebitBatchHeader row)
		{
			return row.BankAccount.AB_BankAbbreviation;
		}
	}
}
