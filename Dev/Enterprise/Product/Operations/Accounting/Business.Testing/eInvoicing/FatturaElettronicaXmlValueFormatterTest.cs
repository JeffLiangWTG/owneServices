using System.Globalization;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	public class FatturaElettronicaXmlValueFormatterTest : TestCase
	{
		public void TestGetItalyZipCode()
		{
			AssertEquals("00000", FatturaElettronicaXmlValueFormatter.GetItalyPostCode(null));
			AssertEquals("00000", FatturaElettronicaXmlValueFormatter.GetItalyPostCode(""));
			AssertEquals("00001", FatturaElettronicaXmlValueFormatter.GetItalyPostCode("1"));
			AssertEquals("01234", FatturaElettronicaXmlValueFormatter.GetItalyPostCode("1234"));
			AssertEquals("12345", FatturaElettronicaXmlValueFormatter.GetItalyPostCode("12345"));
			AssertEquals("12345", FatturaElettronicaXmlValueFormatter.GetItalyPostCode("123456"));
			AssertEquals("12345", FatturaElettronicaXmlValueFormatter.GetItalyPostCode("1234567890"));
			AssertEquals("00000", FatturaElettronicaXmlValueFormatter.GetItalyPostCode("A"));
			AssertEquals("00000", FatturaElettronicaXmlValueFormatter.GetItalyPostCode("A234567890"));
		}

		public void TestToAmountDecimalType()
		{
			AssertEquals("10.00", FatturaElettronicaXmlValueFormatter.ToAmountDecimalType(10m));
			AssertEquals("-0.10", FatturaElettronicaXmlValueFormatter.ToAmountDecimalType(-0.1m));
			AssertEquals("10.12", FatturaElettronicaXmlValueFormatter.ToAmountDecimalType(10.12m));
			AssertEquals("10.123", FatturaElettronicaXmlValueFormatter.ToAmountDecimalType(10.123m));
			AssertEquals("-10.123456", FatturaElettronicaXmlValueFormatter.ToAmountDecimalType(-10.123456m));
			AssertEquals("-123456.123456", FatturaElettronicaXmlValueFormatter.ToAmountDecimalType(-123456.123456m));
			AssertEquals(null, FatturaElettronicaXmlValueFormatter.ToAmountDecimalType(null));

			var transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.TransactionType = TransactionType.CRD;
			AssertEquals("10.00", FatturaElettronicaXmlValueFormatter.ToAmountDecimalType(-10m, transaction));
			AssertEquals("-10.00", FatturaElettronicaXmlValueFormatter.ToAmountDecimalType(10m, transaction));
			transaction.TransactionType = TransactionType.ADJ;
			transaction.OSTotal = 0m;
			AssertEquals("10.00", FatturaElettronicaXmlValueFormatter.ToAmountDecimalType(10m, transaction));
			AssertEquals("-10.00", FatturaElettronicaXmlValueFormatter.ToAmountDecimalType(-10m, transaction));
			transaction.OSTotal = 0.1m;
			AssertEquals("10.00", FatturaElettronicaXmlValueFormatter.ToAmountDecimalType(10m, transaction));
			AssertEquals("-10.00", FatturaElettronicaXmlValueFormatter.ToAmountDecimalType(-10m, transaction));
			transaction.OSTotal = -0.1m;
			AssertEquals("10.00", FatturaElettronicaXmlValueFormatter.ToAmountDecimalType(-10m, transaction));
			AssertEquals("-10.00", FatturaElettronicaXmlValueFormatter.ToAmountDecimalType(10m, transaction));
		}

		public void TestToRateDecimalType()
		{
			AssertEquals("10.00", FatturaElettronicaXmlValueFormatter.ToRateDecimalType(10m));
			AssertEquals("-0.10", FatturaElettronicaXmlValueFormatter.ToRateDecimalType(-0.1m));
			AssertEquals("10.12", FatturaElettronicaXmlValueFormatter.ToRateDecimalType(10.12m));
			AssertEquals("10.123", FatturaElettronicaXmlValueFormatter.ToRateDecimalType(10.123m));
			AssertEquals("-10.123456", FatturaElettronicaXmlValueFormatter.ToRateDecimalType(-10.123456m));
			AssertEquals("-123456.123456", FatturaElettronicaXmlValueFormatter.ToRateDecimalType(-123456.123456m));
			AssertEquals(null, FatturaElettronicaXmlValueFormatter.ToRateDecimalType(null));
		}

		public void TestToDateType()
		{
			AssertEquals("2018-03-15", FatturaElettronicaXmlValueFormatter.ToDateType(new ZDateTime(2018, 3, 15, 8, 45, 10, 2)));
			AssertEquals("2018-07-24", FatturaElettronicaXmlValueFormatter.ToDateType(new ZDateTime(2018, 7, 24, 6, 15, 30)));
			AssertEquals("2018-09-25", FatturaElettronicaXmlValueFormatter.ToDateType(new ZDateTime(2018, 9, 25)));
			AssertEquals(string.Empty, FatturaElettronicaXmlValueFormatter.ToDateType(ZDateTime.Empty));
			AssertEquals("1900-01-01", FatturaElettronicaXmlValueFormatter.ToDateType(ZDateTime.MinSmallDateTimeValue));
			AssertEquals("2079-06-06", FatturaElettronicaXmlValueFormatter.ToDateType(ZDateTime.MaxSmallDateTimeValue));
			AssertEquals(string.Empty, FatturaElettronicaXmlValueFormatter.ToDateType(null));
		}

		public void TestEnsureComplianceWithBasicLatin()
		{
			AssertEquals("test test", FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatin("test€test"));
			AssertEquals("testtest", FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatin("testtest"));
			AssertEquals("test$test", FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatin("test$test"));
			AssertEquals(null, FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatin(null));
			AssertEquals(null, FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatin(""));
			AssertEquals("test", FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatin(new ZString("test")));
			AssertEquals("test", FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatin(new ZString?("test")));
			AssertEquals("test test", FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatin("test¥test"));
		}

		public void TestEnsureComplianceWithBasicLatinAndLatin1Supplement()
		{
			AssertEquals("test test", FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatinAndLatin1Supplement("test€test"));
			AssertEquals("testtest", FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatinAndLatin1Supplement("testtest"));
			AssertEquals("test$test", FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatinAndLatin1Supplement("test$test"));
			AssertEquals(null, FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatinAndLatin1Supplement(null));
			AssertEquals(null, FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatinAndLatin1Supplement(""));
			AssertEquals("test", FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatinAndLatin1Supplement(new ZString("test")));
			AssertEquals("test", FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatinAndLatin1Supplement(new ZString?("test")));
			AssertEquals("test¥test", FatturaElettronicaXmlValueFormatter.EnsureComplianceWithBasicLatinAndLatin1Supplement("test¥test"));
		}

		public static string FormatWithZerosAfterDecimalPoint(ZDecimal value)
		{
			return value.ToString("0.00", CultureInfo.InvariantCulture);
		}
	}
}
