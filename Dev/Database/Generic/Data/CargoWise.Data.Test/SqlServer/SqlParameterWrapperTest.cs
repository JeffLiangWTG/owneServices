using System.Data;
using System.Data.Common;
using NUnit.Framework;

namespace CargoWise.Data.Testing;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "We do not want simplify name here")]
class SqlParameterWrapperTest : TestCase
{
	public void TestSqlParameterWrapperReturnBaseValue()
	{
		var baseValue = new SqlParameter()
		{
			DbType = DbType.String,
			Direction = ParameterDirection.Input,
			IsNullable = true,
			ParameterName = "TestParameter",
			Size = 50,
			SourceColumn = "TestColumn",
			SourceColumnNullMapping = false,
			Value = "TestValue",

			SqlDbType = SqlDbType.NVarChar,
			Precision = 9,
			Scale = 2
		};

		var wrapper = new SqlParameterWrapper(baseValue);

		AssertEquals("Wrapper should be valid", false, wrapper.InvalidAsThisIsNotSqlParameter);
		AssertEquals(baseValue, wrapper);
		AssertNotEquals(null, wrapper);

		AssertEquals(baseValue.DbType, wrapper.DbType);
		AssertEquals(baseValue.Direction, wrapper.Direction);
		AssertEquals(baseValue.IsNullable, wrapper.IsNullable);
		AssertEquals(baseValue.ParameterName, wrapper.ParameterName);
		AssertEquals(baseValue.Size, wrapper.Size);
		AssertEquals(baseValue.SourceColumn, wrapper.SourceColumn);
		AssertEquals(baseValue.SourceColumnNullMapping, wrapper.SourceColumnNullMapping);
		AssertEquals(baseValue.Value, wrapper.Value);

		AssertEquals(baseValue.SqlDbType, wrapper.SqlDbType);
		AssertEquals(baseValue.Precision, wrapper.Precision);
		AssertEquals(baseValue.Scale, wrapper.Scale);
		AssertEquals(baseValue.SourceVersion, wrapper.SourceVersion);

		AssertEquals(baseValue.ToString(), wrapper.ToString());
		AssertEquals(baseValue.GetHashCode(), wrapper.GetHashCode());
	}

	public void TestSqlParameterWrapperCanSetValues()
	{
		var baseValue = new System.Data.SqlClient.SqlParameter();
		var wrapper = new SqlParameterWrapper(baseValue);

		wrapper.DbType = DbType.String;
		wrapper.Direction = ParameterDirection.Input;
		wrapper.IsNullable = true;
		wrapper.ParameterName = "TestParameter";
		wrapper.Size = 50;
		wrapper.SourceColumn = "TestColumn";
		wrapper.SourceColumnNullMapping = false;
		wrapper.Value = "TestValue";

		wrapper.SqlDbType = SqlDbType.NVarChar;
		wrapper.Precision = 9;
		wrapper.Scale = 2;
		wrapper.UdtTypeName = "TestUdtTypeName";

		AssertEquals(DbType.String, baseValue.DbType);
		AssertEquals(ParameterDirection.Input, baseValue.Direction);
		AssertEquals(true, baseValue.IsNullable);
		AssertEquals("TestParameter", baseValue.ParameterName);
		AssertEquals(50, baseValue.Size);
		AssertEquals("TestColumn", baseValue.SourceColumn);
		AssertEquals(false, baseValue.SourceColumnNullMapping);
		AssertEquals("TestValue", baseValue.Value);
		AssertEquals(SqlDbType.NVarChar, baseValue.SqlDbType);
		AssertEquals((byte)9, baseValue.Precision);
		AssertEquals((byte)2, baseValue.Scale);
		AssertEquals("TestUdtTypeName", wrapper.UdtTypeName);
	}

#if NET
	public void TestSqlParameterWrapperCanSetValuesOnMS()
	{
		var baseValue = new Microsoft.Data.SqlClient.SqlParameter();
		var wrapper = new SqlParameterWrapper(baseValue);

		wrapper.DbType = DbType.String;
		wrapper.Direction = ParameterDirection.Input;
		wrapper.IsNullable = true;
		wrapper.ParameterName = "TestParameter";
		wrapper.Size = 50;
		wrapper.SourceColumn = "TestColumn";
		wrapper.SourceColumnNullMapping = false;
		wrapper.Value = "TestValue";

		wrapper.SqlDbType = SqlDbType.NVarChar;
		wrapper.Precision = 9;
		wrapper.Scale = 2;
		wrapper.UdtTypeName = "TestUdtTypeName";

		AssertEquals(DbType.String, baseValue.DbType);
		AssertEquals(ParameterDirection.Input, baseValue.Direction);
		AssertEquals(true, baseValue.IsNullable);
		AssertEquals("TestParameter", baseValue.ParameterName);
		AssertEquals(50, baseValue.Size);
		AssertEquals("TestColumn", baseValue.SourceColumn);
		AssertEquals(false, baseValue.SourceColumnNullMapping);
		AssertEquals("TestValue", baseValue.Value);
		AssertEquals(SqlDbType.NVarChar, baseValue.SqlDbType);
		AssertEquals((byte)9, baseValue.Precision);
		AssertEquals((byte)2, baseValue.Scale);
		AssertEquals("TestUdtTypeName", wrapper.UdtTypeName);
	}
#endif

	public void TestSqlParameterWrapperCanCompareWhenInvalid()
	{
		var invalidBase = new NotSqlparameter();
		var invalidWrapper = new SqlParameterWrapper(invalidBase);
		AssertEquals(true, invalidWrapper.InvalidAsThisIsNotSqlParameter);
		AssertEquals(invalidBase, invalidWrapper);
		AssertEquals("GetHashCode return 0", 0, invalidWrapper.GetHashCode());

		// Equals
		Assert("True when: invalidWrapper.Equals(null)			",	 invalidWrapper.Equals(null)			);
		Assert("True when: invalidWrapper.Equals(invalidBase)	",	 invalidWrapper.Equals(invalidBase)		);
		Assert("True when: invalidWrapper.Equals(invalidWrapper)",	 invalidWrapper.Equals(invalidWrapper)	);

		// ==
		#pragma warning disable CS1718 // Comparison made to same variable
		Assert("True when: wrapper == null		",	invalidWrapper == null);
		Assert("True when: wrapper == wrapper	",	invalidWrapper == invalidWrapper);

		// !=
		Assert("True when: invalidWrapper != new object()",		invalidWrapper != new object());

		var validBase = new SqlParameter();
		var validWrapper = new SqlParameterWrapper(validBase);

		Assert("False when: validWrapper == invalidBase		", !(validWrapper == invalidBase));
		Assert("False when: validWrapper == invalidWrapper	", !(validWrapper == invalidWrapper));

		Assert("True when: validWrapper != invalidBase		", validWrapper != invalidBase);
		Assert("True when: validWrapper != invalidWrapper	", validWrapper != invalidWrapper);
	}

	public void TestSqlParameterWrapperCanCompareWhenValid()
	{
		var validBase = new SqlParameter();
		var validWrapper = new SqlParameterWrapper(validBase);
		AssertEquals(false, validWrapper.InvalidAsThisIsNotSqlParameter);
		AssertEquals(validBase, validWrapper);

		// Equals
		Assert("False when: validWrapper.Equals(null)		", !validWrapper.Equals(null));
		Assert("True when: validWrapper.Equals(validBase)	", validWrapper.Equals(validBase));
		Assert("True when: validWrapper.Equals(validWrapper)", validWrapper.Equals(validWrapper));

		// ==
		#pragma warning disable CS1718 // Comparison made to same variable
		Assert("False when: wrapper == null		", !(validWrapper == null));
		Assert("True when: wrapper == wrapper	", validWrapper == validWrapper);

		// !=
		Assert("True when: validWrapper != new object()", validWrapper != new object());
	}

	public void TestSqlParameterWrapperCanNotAcceptWrapperItself()
	{
		var validBase = new SqlParameter();
		var validWrapper = new SqlParameterWrapper(validBase);
		AssertExceptionThrown<System.ArgumentException>(() => new SqlParameterWrapper(validWrapper));
	}

	public class NotSqlparameter : DbParameter
	{
		public override DbType DbType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		public override ParameterDirection Direction { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		public override bool IsNullable { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		public override string ParameterName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		public override int Size { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		public override string SourceColumn { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		public override bool SourceColumnNullMapping { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		public override object Value { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

		public override void ResetDbType()
		{
			throw new System.NotImplementedException();
		}
	}
}
