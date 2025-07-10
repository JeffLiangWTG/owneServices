using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay.Testing
{
	public class CFEHelperTest : TestCaseWithFactory
	{
		public void TestGetInvoiceSerieAndNumber_NullChecks()
		{
			(var serie, var numero) = (new CFEHelper() as ICFEHelper).GetInvoiceSerieAndNumber(null);

			AssertNullOrEmpty(serie);
			AssertNullOrEmpty(numero);
		}

		public void TestGetInvoiceSerieAndNumber()
		{
			var testList = new List<(string transactionReference, string expectedSerie, string expectedNumber)>() {
				("A1007", "A", "1007"),
				("AA10007", "AA", "10007"),
				("AA1A0007", "AA", "1A0007"),
				("1A0007", "", "1A0007"),
				("AAA10007", "AAA", "10007"),
				("10007", "", "10007"),
				("A1B0007", "A", "1B0007"),
				("A10007B", "A", "10007B"),
				("ABCD", "ABCD", ""),
				("1", "", "1"),
				("A", "A", ""),
				("", "", "")
			};

			foreach (var test in testList)
			{
				(var serie, var numero) = (new CFEHelper() as ICFEHelper).GetInvoiceSerieAndNumber(test.transactionReference);

				AssertEquals(test.expectedSerie, serie);
				AssertEquals(test.expectedNumber, numero);
			}
		}
	}
}
