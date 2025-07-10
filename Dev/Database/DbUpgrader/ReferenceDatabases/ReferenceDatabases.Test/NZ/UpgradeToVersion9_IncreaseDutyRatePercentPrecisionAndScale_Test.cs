using System;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.ReferenceDatabases.NZ.Testing
{
	sealed class UpgradeToVersion9_IncreaseDutyRatePercentPrecisionAndScale_Test : NZTariffReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 9;

		protected override void AssertUpgradeResult()
		{
			string dataType = "";
			int numericPrecision = 0;
			int numericScale = 0;

			string script = @"
				SELECT TOP 1 DATA_TYPE, NUMERIC_PRECISION, NUMERIC_SCALE
				FROM INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'NZCClassificationDutyRate'
					AND COLUMN_NAME = 'U1_DutyRatePercent'";

			string sqlTextWithEscapedSingleQuotes = DataUtils.EscapeSingleQuotes(script);
			string sql = String.Format("EXEC [{0}]..sp_executesql N'{1}'", refDbUpgrader.DbName, sqlTextWithEscapedSingleQuotes);
			using (var command = testConnection.Command(sql))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					dataType = (string)reader["DATA_TYPE"];
					numericPrecision = (byte)reader["NUMERIC_PRECISION"];
					numericScale = (int)reader["NUMERIC_SCALE"];
				}
			}

			AssertEquals("DATA_TYPE", "decimal", dataType);
			AssertEquals("NUMERIC_PRECISION", 12, numericPrecision);
			AssertEquals("NUMERIC_SCALE", 6, numericScale);
		}
	}
}
