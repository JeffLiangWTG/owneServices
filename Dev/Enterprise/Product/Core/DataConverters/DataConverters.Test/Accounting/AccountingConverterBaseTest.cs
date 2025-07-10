using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataConverters.Accounting;

namespace Enterprise.DataConverters.Testing.Accounting
{
	internal abstract class AccountingConverterBaseTest : TestCaseWithFactory
	{
		protected void WriteHeaderLine(StreamWriter writer)
		{
			var line =
				AccountingConverterBase.Account + "," +
				AccountingConverterBase.Reference + "," +
				AccountingConverterBase.InvoiceDate + "," +
				AccountingConverterBase.DueDate + "," +
				AccountingConverterBase.Currency + "," +
				AccountingConverterBase.ForeignAmount + "," +
				AccountingConverterBase.LocalAmount + "," +
				AccountingConverterBase.Branch + "," +
				AccountingConverterBase.Department;

			writer.WriteLine(line);
		}
	}
}
