using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Utilities.Testing
{
	internal class JASCsvLineTest : TestCase
	{
		public void TestGetFieldValues()
		{
			AssertEquals("Empty string if index is invalid", "", Line.GetFieldValue(-1));
			AssertEquals("TEST", Line.GetFieldValue(0));
			AssertEquals("12309", Line.GetFieldValue(1));
			AssertEquals("HAHA", Line.GetFieldValue(3));
			AssertEquals("28.23", Line.GetFieldValue(4));
			AssertEquals("", Line.GetFieldValue(8));
			AssertEquals("Empty string if index is not valid", "", Line.GetFieldValue(88));
		}

		public void TestGetByteFieldValue()
		{
			AssertEquals("Zero if index is invalid", ZByte.Zero, Line.GetIntFieldValue(-1));
			AssertEquals("Zero if string cannot be parsed", ZByte.Zero, Line.GetIntFieldValue(0));
			AssertEquals(12309, Line.GetIntFieldValue(1));
			AssertEquals(8888, Line.GetIntFieldValue(2));
		}

		public void TestGetIntFieldValue()
		{
			AssertEquals("Zero if index is invalid", 0, Line.GetIntFieldValue(-1));
			AssertEquals("Zero if string cannot be parsed", 0, Line.GetIntFieldValue(0));
			AssertEquals(12309, Line.GetIntFieldValue(1));
			AssertEquals(8888, Line.GetIntFieldValue(2));
		}

		public void TestGetDecimalFieldValue()
		{
			AssertEquals("Zero if index is invalid", 0m, Line.GetDecimalFieldValue(-1));
			AssertEquals("Zero if string cannot be parsed", 0m, Line.GetDecimalFieldValue(0));
			AssertEquals(12309m, Line.GetDecimalFieldValue(1));
			AssertEquals(28.23m, Line.GetDecimalFieldValue(4));
			AssertEquals(100.2938m, Line.GetDecimalFieldValue(6));
		}

		public void TestGetDateTimeFieldValue()
		{
			AssertEquals("ZDateTime.Empty if index is invalid", ZDateTime.Empty, Line.GetDateTimeFieldValue(-1));
			AssertEquals("ZDateTime.Empty if string cannot be parsed", ZDateTime.Empty, Line.GetDateTimeFieldValue(0));
			AssertEquals(new ZDateTime(1981, 10, 3), Line.GetDateTimeFieldValue(7));
		}

		JASCsvLine Line
		{
			get
			{
				if (fLine == null)
				{
					fLine = new JASCsvLine("TEST;12309;8888;HAHA;28.23;sadf;100.2938;03/10/1981;;", ';');
				}

				return fLine;
			}
		}

		JASCsvLine fLine;
	}
}
