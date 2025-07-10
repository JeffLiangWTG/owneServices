using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(CalculateTimeZoneOffsetInMinutes))]
	internal sealed class CalculateTimeZoneOffsetInMinutesTest : DbCreateScriptTest
	{
		[TestDate(2017, 3, 1)]
		public void TestCalculateTime()
		{
			var referenceDate = new DateTime(2017, 3, 1);

			var result = Run("UAIEV", referenceDate);
			AssertEquals((short)120, result);

			result = Run("AUSYD", referenceDate);
			AssertEquals((short)660, result);

			result = Run("USLAX", referenceDate);
			AssertEquals((short)(-480), result);
		}

		[TestDate(2017, 7, 1)]
		public void TestCalculateTime_DaylightSavings()
		{
			var referenceDate = new DateTime(2017, 7, 1);

			var result = Run("UAIEV", referenceDate);
			AssertEquals((short)180, result);

			result = Run("AUSYD", referenceDate);
			AssertEquals((short)600, result);

			result = Run("USLAX", referenceDate);
			AssertEquals((short)(-420), result);
		}

		#region Implementation

		short Run(string portCode, DateTime localTime)
		{
			using (var command = Db.Connection.Command("CalculateTimeZoneOffsetInMinutes"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@UNLOCO", SqlDbType.VarChar, portCode);
				command.AddParameter("@localtime", SqlDbType.DateTime, localTime);
				command.AddParameter("@resultOffset", SqlDbType.SmallInt, 0);
				command.GetParameter("@resultOffset").Direction = ParameterDirection.Output;

				command.ExecuteNonQuery();
				var result = command.GetParameterValue("@resultOffset");
				return (short)result;
			}
		}
		#endregion
	}
}
