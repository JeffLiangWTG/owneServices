using System;
using NUnit.Framework;

namespace CargoWise.Data.Testing;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "We do not want simplify name here")]
class SqlExceptionWrapperTest : TestCase
{
	public void TestSqlExceptionWrapper()
	{
		var exception = SqlExceptionBuilder.CreateSqlException<SqlException>(5062, "DbErrorType.CannotAlterDbWhileInUse");
		var wrapper = new SqlExceptionWrapper(exception);

		AssertEquals(exception.ErrorCode, wrapper.ErrorCode);
		AssertEquals(exception.Message, wrapper.Message);
		AssertEquals(exception.Source, wrapper.Source);
		AssertEquals(exception.StackTrace, wrapper.StackTrace);
		AssertEquals(exception.Server, wrapper.Server);
		AssertEquals(exception.Procedure, wrapper.Procedure);
		AssertEquals(exception.LineNumber, wrapper.LineNumber);
		AssertEquals(exception.State, wrapper.State);
		AssertEquals(exception.Number, wrapper.Number);
		AssertEquals(exception.Class, wrapper.Class);
		AssertEquals(exception.Errors.Count, wrapper.Errors.Count);
		AssertEquals(exception.Errors[0].Message, wrapper.Errors[0].Message);
		AssertEquals(exception.Errors[0].Number, wrapper.Errors[0].Number);
		AssertEquals(exception.Errors[0].State, wrapper.Errors[0].State);
		AssertEquals(exception.Errors[0].Class, wrapper.Errors[0].Class);
		AssertEquals(exception.Errors[0].Server, wrapper.Errors[0].Server);
		AssertEquals(exception.Errors[0].Procedure, wrapper.Errors[0].Procedure);
		AssertEquals(exception.Errors[0].LineNumber, wrapper.Errors[0].LineNumber);
		AssertEquals(exception.Errors[0].Source, wrapper.Errors[0].Source);
	}

	public void TestSqlExceptionWrapperWithNullException()
	{
		AssertExceptionThrown<ArgumentException>(() => new SqlExceptionWrapper(null));
	}

	public void TestSqlExceptionWrapperWithAnotherWrapper()
	{
		var exception = SqlExceptionBuilder.CreateSqlException(5062, "DbErrorType.CannotAlterDbWhileInUse");
		var wrapper = new SqlExceptionWrapper(exception);
		AssertExceptionThrown<ArgumentException>(() => new SqlExceptionWrapper(wrapper));
	}
}
