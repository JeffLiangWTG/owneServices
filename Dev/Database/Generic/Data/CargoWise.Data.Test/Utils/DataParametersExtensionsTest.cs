using System;
using System.Data;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DataParametersExtensionsTest : TestCase
	{
		public void TestGetFormattedDbType()
		{
			var parameterVarChar = new SqlParameter("@Name", SqlDbType.VarChar);
			AssertEquals("VarChar", parameterVarChar.GetFormattedDbType());

			parameterVarChar.Size = 20;
			AssertEquals("VarChar (20)", parameterVarChar.GetFormattedDbType());

			var parameterMoney = new SqlParameter("@Name", SqlDbType.Money);
			AssertEquals("Money", parameterMoney.GetFormattedDbType());

			parameterMoney.Precision = 16;
			parameterMoney.Scale = 3;
			AssertEquals("Money", parameterMoney.GetFormattedDbType());

			var parameterDecimal = new SqlParameter("@Name", SqlDbType.Decimal);
			AssertEquals("Decimal", parameterDecimal.GetFormattedDbType());

			parameterDecimal.Precision = 18;
			parameterDecimal.Scale = 9;
			AssertEquals("Decimal (18, 9)", parameterDecimal.GetFormattedDbType());
		}

		public void TestFormattedValue()
		{
			var parameter = new SqlParameter("@Name", SqlDbType.VarChar, 20) { Value = "Dummy Name" };
			AssertEquals("'Dummy Name'", parameter.GetFormattedValue());

			parameter = new SqlParameter("@Name", SqlDbType.VarChar, 20) { Value = "Dummy 'contains' Name" };
			AssertEquals("'Dummy ''contains'' Name'", parameter.GetFormattedValue());

			parameter = new SqlParameter("@Name", SqlDbType.VarChar, 20) { Value = DBNull.Value };
			AssertEquals("null", parameter.GetFormattedValue());

			parameter = new SqlParameter("@Name", SqlDbType.VarChar, 20) { Value = null };
			AssertEquals("null", parameter.GetFormattedValue());

			parameter = new SqlParameter("@Name", SqlDbType.Bit) { Value = true };
			AssertEquals("1", parameter.GetFormattedValue());
			parameter.Value = false;
			AssertEquals("0", parameter.GetFormattedValue());

			var time = new DateTime(2015, 5, 4, 01, 30, 45);
			parameter = new SqlParameter("@Name", SqlDbType.DateTime, 20) { Value = time };
			AssertEquals("'05/04/2015 01:30:45'", parameter.GetFormattedValue());

			parameter = new SqlParameter("@Name", SqlDbType.DateTime2, 20) { Value = time };
			AssertEquals("'05/04/2015 01:30:45'", parameter.GetFormattedValue());

			parameter = new SqlParameter("@Name", SqlDbType.SmallDateTime, 20) { Value = time };
			AssertEquals("'05/04/2015 01:30:45'", parameter.GetFormattedValue());

			var timeOffset = new DateTimeOffset(time, new TimeSpan(1, 0, 0));
			parameter = new SqlParameter("@Name", SqlDbType.DateTimeOffset, 20) { Value = timeOffset };
			AssertEquals("'05/04/2015 01:30:45 +01:00'", parameter.GetFormattedValue());

			time = new DateTime(2015, 5, 4);
			parameter = new SqlParameter("@Name", SqlDbType.Date, 20) { Value = time };
			AssertEquals("'05/04/2015 00:00:00'", parameter.GetFormattedValue());

			var guid = Guid.NewGuid();
			parameter = new SqlParameter("@Name", SqlDbType.UniqueIdentifier, 20) { Value = guid };
			AssertEquals(string.Format("'{0}'", guid), parameter.GetFormattedValue());

			var geography = SqlGeography.STGeomFromText(new SqlChars("POINT (-121 48)"), 4326);
			parameter = new SqlParameter("@Name", SqlDbType.NVarChar, 20) { Value = geography };
			AssertEquals(string.Format("'{0}'", geography.AsTextZM().ToSqlString().ToString()), parameter.GetFormattedValue());
		}
	}
}
