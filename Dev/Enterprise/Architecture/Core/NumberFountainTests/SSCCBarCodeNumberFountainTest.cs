#if DEBUG

namespace Enterprise.NumberFountain.Testing
{
	using System;
	using System.Data;
	using CargoWise.Data;
	using NUnit.Framework;

	public class SSCCBarCodeNumberFountainFactoryTest : TransactionedTestCase
	{
		#region TestGetNext

		public void TestGetNext()
		{
			// fountain should be unique per SSCC prefix
			AssertEquals("012345670000000015", new SSCCBarCodeNumberFountainFactory("1234567").New().GetNextFormatted(Connection, Transaction));
			AssertEquals("Numbering should NOT continue with a different SSCC Prefix.", "012345678900000012", new SSCCBarCodeNumberFountainFactory("123456789").New().GetNextFormatted(Connection, Transaction));

			// fudge the fountain forward to test that the first digit is incremented (also tests that check-digit calculation considers the first digit)
			var fountain = new SSCCBarCodeNumberFountainFactory("123456789").New();
			fountain.SetNext(Connection, Transaction, 10000000);
			AssertEquals("The first Fountain digit should be appended to the front of the SSCC BarCode.", "112345678900000002", fountain.GetNextFormatted(Connection, Transaction));

			// get a fountain with specified min and max value						
			fountain = new SSCCBarCodeNumberFountainFactory("0012345").New();
			fountain.SetValues(Connection, Transaction, minValue: 1001, nextValue: 1001, maxValue: 2001);
			AssertEquals("000123450000010019", fountain.GetNextFormatted(Connection, Transaction));
			AssertEquals("000123450000010026", fountain.GetNextFormatted(Connection, Transaction));

			// ensure check-digit is correctly calculated
			AssertEquals("062345670000000010", new SSCCBarCodeNumberFountainFactory("6234567").New().GetNextFormatted(Connection, Transaction)); // testing check digit 0 is important, see implementation for details.
			AssertEquals("056618910000000019", new SSCCBarCodeNumberFountainFactory("5661891").New().GetNextFormatted(Connection, Transaction));
			AssertEquals("012345678000000011", new SSCCBarCodeNumberFountainFactory("12345678").New().GetNextFormatted(Connection, Transaction));
			AssertEquals("016765167700000011", new SSCCBarCodeNumberFountainFactory("167651677").New().GetNextFormatted(Connection, Transaction));
			AssertEquals("004649488900000015", new SSCCBarCodeNumberFountainFactory("046494889").New().GetNextFormatted(Connection, Transaction));
			AssertEquals("012345678900000012", new SSCCBarCodeNumberFountainFactory("1234567890").New().GetNextFormatted(Connection, Transaction));
			AssertEquals("012345678901000011", new SSCCBarCodeNumberFountainFactory("12345678901").New().GetNextFormatted(Connection, Transaction));
		}

		#endregion

		#region TestInvalidSSCCPrefixThrowsException

		public void TestInvalidSSCCPrefixThrowsException()
		{
			AssertExceptionThrown(typeof(ArgumentException), "SSCC Prefix must be 7-11 digits.", delegate
			{ new SSCCBarCodeNumberFountainFactory(""); });
			AssertExceptionThrown(typeof(ArgumentException), "SSCC Prefix must be 7-11 digits.", delegate
			{ new SSCCBarCodeNumberFountainFactory("123456"); });
			AssertExceptionThrown(typeof(ArgumentException), "SSCC Prefix must be 7-11 digits.", delegate
			{ new SSCCBarCodeNumberFountainFactory("123456789012"); });
			AssertExceptionThrown(typeof(ArgumentException), "SSCC Prefix must be 7-11 digits.", delegate
			{ new SSCCBarCodeNumberFountainFactory("123456A"); });
			AssertExceptionThrown(typeof(ArgumentException), "SSCC Prefix must be 7-11 digits.", delegate
			{ new SSCCBarCodeNumberFountainFactory("12345 6"); });
		}

		#endregion

		#region Implementation

		static IDbTransaction Transaction => GetTransaction(Db.Connection);

		static IDbConnection Connection => GetConnection(Db.Connection);

		static System.Data.Common.DbTransaction GetTransaction(DbConnection connection)
		{
			return ((IDbConnectionInternals)connection).ADOTransaction;
		}

		static IDbConnection GetConnection(DbConnection connection)
		{
			return ((IDbConnectionInternals)connection).InternalDbConnection;
		}

		#endregion
	}

	public class SSCCBarCodeCheckerTest : TestCase
	{
		#region TestIsSSCCBarCodePrefix

		public void TestIsSSCCBarCodePrefix()
		{
			AssertEquals(false, SSCCBarCodeChecker.IsSSCCBarCodePrefix(null));
			AssertEquals(false, SSCCBarCodeChecker.IsSSCCBarCodePrefix(""));
			AssertEquals(false, SSCCBarCodeChecker.IsSSCCBarCodePrefix("123456"));
			AssertEquals(false, SSCCBarCodeChecker.IsSSCCBarCodePrefix("123456A"));
			AssertEquals(false, SSCCBarCodeChecker.IsSSCCBarCodePrefix("12345 6"));

			AssertEquals(true, SSCCBarCodeChecker.IsSSCCBarCodePrefix("1234567"));
			AssertEquals(true, SSCCBarCodeChecker.IsSSCCBarCodePrefix("12345678"));
			AssertEquals(true, SSCCBarCodeChecker.IsSSCCBarCodePrefix("123456789"));
			AssertEquals(true, SSCCBarCodeChecker.IsSSCCBarCodePrefix("1234567890"));
			AssertEquals(true, SSCCBarCodeChecker.IsSSCCBarCodePrefix("12345678901"));
		}

		#endregion

		#region TestIsValidSSCC

		public void TestIsValidSSCC()
		{
			var ssccPrefix = "6234567";
			AssertEquals(false, SSCCBarCodeChecker.IsValidSSCC("", ssccPrefix)); // 
			AssertEquals(false, SSCCBarCodeChecker.IsValidSSCC("012345670000000011", ssccPrefix)); // Prefix is not correct
			AssertEquals(false, SSCCBarCodeChecker.IsValidSSCC("062345670000000011", ssccPrefix)); // invalid check-digit

			AssertEquals(true,  SSCCBarCodeChecker.IsValidSSCC("062345670000000010", ssccPrefix));
			AssertEquals(true,  SSCCBarCodeChecker.IsValidSSCC("00062345670000000010", ssccPrefix));
		}

		#endregion

		#region TestIsSSCCBarCode

		public void TestIsSSCCBarCode()
		{
			AssertEquals(false, SSCCBarCodeChecker.IsSSCCBarCode(null));
			AssertEquals(false, SSCCBarCodeChecker.IsSSCCBarCode(""));
			AssertEquals(false, SSCCBarCodeChecker.IsSSCCBarCode("06234567000000001"));
			AssertEquals(false, SSCCBarCodeChecker.IsSSCCBarCode("06234567A000000010"));
			AssertEquals(false, SSCCBarCodeChecker.IsSSCCBarCode("062345670000000011")); // invalid check-digit

			AssertEquals(true, SSCCBarCodeChecker.IsSSCCBarCode("062345670000000010"));
		}

		public void TestGetCheckDigit()
		{
			AssertEquals(3, SSCCBarCodeChecker.GetCheckDigit("34012345123458789"));
			AssertEquals(0, SSCCBarCodeChecker.GetCheckDigit("34012345678901234"));
			AssertNoExceptionThrown("No Exception is thrown when input is non-numeric", () => { SSCCBarCodeChecker.GetCheckDigit("abcde"); });
		}

		#endregion

		#region TestGetSSCCFromRawBarcode

		public void TestGetSSCCFromRawBarcode()
		{
			AssertEquals("", SSCCBarCodeChecker.GetSSCCFromRawBarcode(null));
			AssertEquals("", SSCCBarCodeChecker.GetSSCCFromRawBarcode(""));
			AssertEquals("", SSCCBarCodeChecker.GetSSCCFromRawBarcode("06234567000000001"));
			AssertEquals("", SSCCBarCodeChecker.GetSSCCFromRawBarcode("06234567A000000010"));
			AssertEquals("", SSCCBarCodeChecker.GetSSCCFromRawBarcode("062345670000000011")); // invalid check-digit

			AssertEquals("", SSCCBarCodeChecker.GetSSCCFromRawBarcode("062345670000000010"));
			AssertEquals("062345670000000010", SSCCBarCodeChecker.GetSSCCFromRawBarcode("00" + "062345670000000010"));
		}

		#endregion

	}
}

#endif
