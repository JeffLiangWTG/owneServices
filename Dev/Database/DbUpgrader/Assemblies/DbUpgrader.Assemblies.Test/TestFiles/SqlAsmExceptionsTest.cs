using System;
using Microsoft.SqlServer.Server;

public class SqlAsmExceptionsTest
{
	[SqlFunction(Name = "TestAmbiguousMatch")]
	public static string TestAmbiguousMatch()
	{
		return "";
	}

	public static string TestAmbiguousMatch(string param)
	{
		return "";
	}

	[SqlFunction(Name = "TestTypeMapping")]
	public static object TestTypeMapping()
	{
		return "";
	}

	public static string NoSqlFunctionAttribute()
	{
		return "";
	}

	[SqlFunction()]
	public static string NoSqlFunctionAttributeName()
	{
		return "";
	}
}
