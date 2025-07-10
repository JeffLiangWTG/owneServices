using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Testing
{
	[TestedType(typeof(CalculateTimeZoneOffset))]
	internal sealed class CalculateTimeZoneOffsetTest : DbCreateScriptTest
	{
		[TestDate(2014, 3, 1)]
		public void TestCalculateTime()
		{
			var referenceDate = new DateTime(2014, 3, 1);

			var result = Run("UAIEV", referenceDate);
			AssertEquals((decimal)2, result);

			result = Run("AUSYD", referenceDate);
			AssertEquals((decimal)11, result);

			result = Run("USLAX", referenceDate);
			AssertEquals((decimal)(-8), result);
		}

		[TestDate(2014, 7, 1)]
		public void TestCalculateTime_DaylightSavings()
		{
			var referenceDate = new DateTime(2014, 7, 1);

			var result = Run("UAIEV", referenceDate);
			AssertEquals((decimal)3, result);

			result = Run("AUSYD", referenceDate);
			AssertEquals((decimal)10, result);

			result = Run("USLAX", referenceDate);
			AssertEquals((decimal)(-7), result);
		}

		#region Implementation

		decimal Run(string portCode, DateTime utcTime)
		{
			using (var command = Db.Connection.Command("CalculateTimeZoneOffset"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@UNLOCO", SqlDbType.VarChar, portCode);
				command.AddParameter("@utctime", SqlDbType.DateTime, utcTime);
				command.AddParameter("@resultOffset", SqlDbType.Decimal, decimal.MinValue);
				command.GetParameter("@resultOffset").Direction = ParameterDirection.Output;

				command.ExecuteNonQuery();
				var result = command.GetParameterValue("@resultOffset");
				return (decimal)result;
			}
		}
		#endregion
	}
}

