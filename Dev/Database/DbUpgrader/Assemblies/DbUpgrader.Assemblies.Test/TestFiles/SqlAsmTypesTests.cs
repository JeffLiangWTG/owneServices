using System;
using Microsoft.SqlServer.Server;

public class SqlAsmTypesTest
{
	[SqlFunction(Name = "TestBinToGuid")]
	public static Guid TestBinToGuid(byte[] input)
	{
		return new Guid(input);
	}

	[SqlFunction(Name = "TestGuidToBin")]
	public static byte[] TestGuidToBin(Guid input)
	{
		return input.ToByteArray();
	}

	[SqlFunction(Name = "TestInts")]
	public static Int64 TestInts(Int16 oneInt, Int32 twoInt)
	{
		return ((Int64)oneInt + (Int64)twoInt) * 2;
	}

	[SqlFunction(Name = "TestDateTime")]
	public static DateTime TestDateTime(DateTime dateTime)
	{
		return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
	}

	[SqlFunction(Name = "TestDecimalBoolean")]
	public static bool TestDecimalBoolean(decimal decimalValueOne, decimal decimalValueTwo)
	{
		return decimalValueOne == decimalValueTwo;
	}

	[SqlFunction(Name = "TestDouble")]
	public static double TestDouble(int valueOne, int valueTwo)
	{
		return (double)valueOne / (double)valueTwo;
	}
}
