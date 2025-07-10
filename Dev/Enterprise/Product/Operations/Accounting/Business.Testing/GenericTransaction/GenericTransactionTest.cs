using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GenericTransaction
{
	[TestedType(typeof(GenericTransaction))]
	public class GenericTransactionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDelete()
		{
			GenericTransaction gT = new GenericTransaction(Factory);
			try
			{
				gT.Delete();
				string expectedError = "Cannot delete accounting object " + nameof(GenericTransaction);
				AssertEquals("LastMessageReported", expectedError, ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesGenerictransaction()
		{
			GenericTransaction genericTransaction = new GenericTransaction(Factory);

			var localList = new List<string>
			{
				nameof(genericTransaction.VT_Amount),
				nameof(genericTransaction.VT_GST),
				nameof(genericTransaction.VT_Total)
			};

			var osList = new List<string>
			{
				nameof(genericTransaction.VT_OSTotal),
				nameof(genericTransaction.VT_OSAmount),
				nameof(genericTransaction.VT_OSGST)
			};

			var exList = new List<string>
			{
				nameof(genericTransaction.VT_ExchangeRate)
			};

			var tester = new DecimalPlacesAttributeTester(genericTransaction);
			tester.CheckLocalCurrency(localList, nameof(genericTransaction.Decimals));
			tester.CheckNonLocalCurrency(osList, nameof(genericTransaction.CurrencyDecimals), nameof(genericTransaction.VT_RX_NKCurrency), genericTransaction);
			tester.CheckExchangeRate(exList, nameof(genericTransaction.ExchangeRateDecimals));
		}
	}
}
