using System;
using System.Data;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{ // for ZQuery
	sealed class ZSqlConnectionInfoTest : TransactionedTestCase
	{
		public void TestSelectGetsCorrectLengthOnSmall()
		{
			ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(null, null);
			using (var command = connectionInfo.GetNewDbCommandForSelect("", ZSqlParameter.New("@p", "1", DummyBizoSchema.Z0_Code)))
			{
				var p = command.GetParameter("@p");
				AssertEquals(DummyBizoSchema.Z0_Code.MaxLength, p.Size);
			}
		}

		public void TestSelectGetsCorrectLengthOnTooBig()
		{
			var value = "1234567890123456789012345678901234567890";
			ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(null, null);
			using (var command = connectionInfo.GetNewDbCommandForSelect("", ZSqlParameter.New("@p", value, DummyBizoSchema.Z0_Code)))
			{
				var p = command.GetParameter("@p");
				AssertEquals(value.Length, p.Size);
			}
		}

		public void TestSelectGetsCorrectLengthOnLike()
		{
			var value = "[0-9][A-z][A-z][A-z]";
			ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(null, null);
			using (var command = connectionInfo.GetNewDbCommandForSelect("select * from dbo.orgheader where OH_Code like @p", ZSqlParameter.New("@p", value, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Like)))
			{
				var p = command.GetParameter("@p");
				command.ExecuteNonQuery();
				AssertEquals(value.Length, p.Size);
			}
		}

		public void TestSelectGetsCorrectLengthOnContains()
		{
			var value = "this is a really long field that is actually too long";
			ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(null, null);
			using (var command = connectionInfo.GetNewDbCommandForSelect("", ZSqlParameter.New("@p", value, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Contains)))
			{
				var p = command.GetParameter("@p");
				AssertEquals("%" + value + "%", p.Value);
				AssertEquals(value.Length + 2, p.Size);
			}

			AssertEquals("Field maximum length exceeded on Z0_Code, Value = 'this is a really long field that is actually too long'", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestEachThreadGetsADifferentConnectionByDefault()
		{
			object instance1 = null, instance2 = null, instance3 = null;

			ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(null, null);
			instance1 = connectionInfo.DbConnection;

			AssertEquals(instance1, Db.Connection);

			ThreadStart threadstart2 = new ThreadStart(delegate
			{
				using (Db.DisposableActionForDbConnection())
				{
					instance2 = connectionInfo.DbConnection;
					AssertEquals(instance2, Db.Connection);
				}
			});

			ThreadStart threadstart3 = new ThreadStart(delegate
			{
				using (Db.DisposableActionForDbConnection())
				{
					instance3 = connectionInfo.DbConnection;
					AssertEquals(instance3, Db.Connection);
				}
			});

			Thread thread2 = new Thread(threadstart2);
			Thread thread3 = new Thread(threadstart3);

			thread2.Start();
			thread3.Start();

			thread2.Join(1000);
			thread3.Join(1000);

			AssertNotNull(instance1);
			AssertNotNull(instance2);
			AssertNotNull(instance3);
			AssertNotEquals("Each thread should get it's own instance", instance1, instance2);
			AssertNotEquals("Each thread should get it's own instance", instance2, instance3);
			AssertNotEquals("Each thread should get it's own instance", instance1, instance3);

			((DbConnection)instance1).Dispose();
		}

		public void TestEachThreadGetsTheSameConnectionIfAssigned()
		{
			object instance1 = null, instance2 = null, instance3 = null;

			using (DbConnection connection = Db.NewExtraConnectionToMainDb())
			{
				ZSqlConnectionInfo connectionInfo = new ZSqlConnectionInfo(connection, null);
				instance1 = connectionInfo.DbConnection;

				ThreadStart threadstart2 = new ThreadStart(delegate
				{ instance2 = connectionInfo.DbConnection; });
				ThreadStart threadstart3 = new ThreadStart(delegate
				{ instance3 = connectionInfo.DbConnection; });

				Thread thread2 = new Thread(threadstart2);
				Thread thread3 = new Thread(threadstart3);

				thread2.Start();
				thread3.Start();

				thread2.Join(1000);
				thread3.Join(1000);

				AssertEquals("Each thread should get the same instance", connection, instance1);
				AssertEquals("Each thread should get the same instance", connection, instance2);
				AssertEquals("Each thread should get the same instance", connection, instance3);
			}

			((DbConnection)instance1).Dispose();
			((DbConnection)instance2).Dispose();
			((DbConnection)instance3).Dispose();
		}

		public void TestEscapingOfSquareBrackets()
		{
			ZSqlConnectionInfo info = new ZSqlConnectionInfo(Db.Connection, "NonDefault");
			ZQuery filter;

			filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Contains, "[");
			DbCommand cmd = info.GetNewDbCommandForUpdate("select blah from blah where param1 like...", filter.Params);
			AssertEquals("Escaped correctly", "%~[%", cmd.GetParameterValue(filter.Params[0].ParameterName));

			filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, "[");
			cmd = info.GetNewDbCommandForUpdate("select blah from blah where param1 = ...", filter.Params);
			AssertEquals("No escaping", "[", cmd.GetParameterValue(filter.Params[0].ParameterName));
		}

		public void TestConstructor()
		{
			ZSqlConnectionInfo info = new ZSqlConnectionInfo(Db.Connection, "NonDefault");
			AssertEquals(Db.Connection, info.DbConnection);
			AssertEquals(info.NonDefaultDatabaseName, "NonDefault");
		}

		public void TestConstructorWithNullDbConnection()
		{
			ZSqlConnectionInfo info = new ZSqlConnectionInfo(null, "NonDefault");
			AssertEquals(info.DbConnection, Db.Connection);
			AssertEquals(info.NonDefaultDatabaseName, "NonDefault");
		}

		public void TestConstructorWithNullNonDefaultDbName()
		{
			ZSqlConnectionInfo info = new ZSqlConnectionInfo(Db.Connection, null);
			AssertEquals(info.DbConnection, Db.Connection);
			AssertEquals(info.NonDefaultDatabaseName, "");
			AssertEquals(info.PathToTables, "");
		}

		public void TestPathToTables()
		{
			ZSqlConnectionInfo info = new ZSqlConnectionInfo(Db.Connection, "NonDefault");
			AssertEquals("NonDefault.dbo.", info.PathToTables);

			ZSqlConnectionInfo info2 = new ZSqlConnectionInfo(Db.Connection, "");
			AssertEquals("", info2.PathToTables);
		}

		public void TestDecimalPrecisionAndScaleAreCopiedToParameter()
		{
			ZSqlConnectionInfo info = new ZSqlConnectionInfo(Db.Connection, "");

			SchemaDecimalColumn column = DummyBizoSchema.Z0_AnotherDecimal;
			Assert("Precondition", column.Precision > 0);
			Assert("Precondition", column.Scale > 0);

			ZSqlParameter parameter = ZSqlParameter.New("@gar", 0, column, SQLComparisonOperator.Equal);
			ZSqlParameter[] parameters = new ZSqlParameter[] { parameter };
			DbCommand cmd = info.GetNewDbCommandForSelect("Select * from blah where far > @gar", parameters);
			var garParameter = cmd.GetParameter("@gar") as SqlParameter;
			AssertEquals("Precision", column.Precision, garParameter.Precision);
			AssertEquals("Scale", column.Scale, garParameter.Scale);
		}

		[ExpectNoExceptions]
		public void TestConstructorDoesNotThrowExceptionOnNullConnection()
		{
			ZSqlConnectionInfo info = new ZSqlConnectionInfo(null, "");
		}

		public void TestGetNewDbCommandForUpdate()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			ZSqlConnectionInfo info = new ZSqlConnectionInfo(Db.Connection, "");
			DbCommand cmd = info.GetNewDbCommandForUpdate(Query, Params);

			object result = cmd.ExecuteScalar();
			AssertEquals("Should have got back one row since query param should be chopped at field length", "COMRADE", result.ToString().Trim());
		}

		public void TestGetNewDbCommandForSelect()
		{
			DummyTableCreator.AddDummyBusinessObjectsToDB(Guid.NewGuid(), Guid.NewGuid());
			ZSqlConnectionInfo info = new ZSqlConnectionInfo(Db.Connection, "");
			DbCommand cmd = info.GetNewDbCommandForSelect(Query, Params);

			AssertNull("Should have got no rows back since is doing search with full long query string", cmd.ExecuteScalar());
		}

		public void TestGetNewDbCommandForSelectWithTimeout()
		{
			ZSqlConnectionInfo info = new ZSqlConnectionInfo(Db.Connection, "");
			DbCommand cmd = info.GetNewDbCommandForSelect(Query, 15, Params);
			AssertEquals(15, cmd.CommandTimeout);
		}

		public void TestGetNewDbCommandForSelectWithoutTimeout()
		{
			ZSqlConnectionInfo info = new ZSqlConnectionInfo(Db.Connection, "");
			DbCommand cmd = info.GetNewDbCommandForSelect(Query, Params);
			AssertEquals(info.DbConnection.DefaultCommandTimeOutInSeconds, cmd.CommandTimeout);
		}

		public void TestGetNewDbCommandForSelectForBool()
		{
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Bool, false);
			ZSqlConnectionInfo info = new ZSqlConnectionInfo(Db.Connection, "");
			DbCommand cmd = info.GetNewDbCommandForSelect(query.GetAsWhereClause(false), query.Params);

			AssertEquals("N", cmd.GetParameter(query.Params[0].ParameterName).Value);
		}

		public void TestGetNewDbCommandForSelectForZBool()
		{
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Bool, ZBool.False);
			ZSqlConnectionInfo info = new ZSqlConnectionInfo(Db.Connection, "");
			DbCommand cmd = info.GetNewDbCommandForSelect(query.GetAsWhereClause(false), query.Params);

			AssertEquals("N", cmd.GetParameter(query.Params[0].ParameterName).Value);
		}

		public void TestGetNewDbCommandForSelectForZBlob()
		{
			AssertZBlobParamDbType(new ZBlob(new byte[] { 1, 2, 3 }), DbType.Binary);
			AssertZBlobParamDbType("ABC", DbType.String);
		}

		public void TestCommandTypeAndTextForStoredProcedure()
		{
			var storedProcedureQuery = "EXEC TestStoredProcedure";
			var parameters = new ZSqlParameter[1] { ZSqlParameter.New("@CW_Test", "TST", DummyBizoSchema.Z0_Code) };
			ZSqlConnectionInfo info = new ZSqlConnectionInfo(Db.Connection, "");
			DbCommand cmd = info.GetNewDbCommandForStoredProcedure(storedProcedureQuery, parameters);
			AssertEquals(CommandType.StoredProcedure, cmd.CommandType);
			AssertEquals("EXEC TestStoredProcedure", cmd.CommandText);
			AssertEquals(1, cmd.ParameterCount);
		}

		void AssertZBlobParamDbType(object value, DbType expectedDbType)
		{
			var info = new ZSqlConnectionInfo(Db.Connection, "");
			var query = new ZQuery(DummyBizoSchema.Z0_VarBinaryMax, SQLComparisonOperator.Equal, value);
			var command = info.GetNewDbCommandForSelect(query.GetAsWhereClause(false), query.Params);
			AssertEquals(1, command.ParameterCount);
			Assert(command.ContainParameter(query.Params[0].ParameterName));
			var param = command.GetParameter(query.Params[0].ParameterName);
			AssertEquals(value, param.Value);
			AssertEquals(expectedDbType, param.DbType);
		}

		string Query
		{
			get
			{
				return
					" select " + DummyBizoSchema.Z0_Description.Name +
					" from " + DummyBizoSchema.Constants.TableName +
					" where " + DummyBizoSchema.Z0_Description.Name +
					" like @param";
			}
		}

		ZSqlParameter[] Params
		{
			get { return new[] { ZSqlParameter.New("@param", "%COMRADE%" + LongString, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Like) }; }
		}

		string LongString
		{
			get
			{
				string result = "";
				for (int i = 0; i < 100; i++)
				{
					result += "%";
				}
				return result + "JUNK!";
			}
		}
	}
}
