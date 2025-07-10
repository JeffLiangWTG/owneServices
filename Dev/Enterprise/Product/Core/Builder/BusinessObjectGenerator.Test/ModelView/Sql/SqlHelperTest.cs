using NUnit.Framework;

namespace Enterprise.BusinessObjectGenerator.ModelView.Testing
{
	sealed class SqlHelperTest : TestCase
	{
		public void TestGetColumnScript()
		{
			var addInfo = new AddInfo
			{
				Name = "Z0_AddInfoString35",
				DataType = "String",
				Precision = null,
				Scale = null,
				MaxLength = 35,
				Indexed = false,
				IsUnicode = false
			};

			var columnScript = SqlHelper.GetColumnScript(addInfo, "Z0_AddInfo");

			var expectedValue =
				"CONVERT(VARCHAR(35), CASE WHEN CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) > 0 THEN REPLACE(SUBSTRING('*'+Z0_AddInfo, CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) + 17, CHARINDEX('*', '*'+Z0_AddInfo+'*', CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) + 1) - (CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) + 17)), '¤', '*') ELSE '' END)";

			AssertEquals(expectedValue, columnScript);
		}

		public void TestIsNullable()
		{
			var addInfo = new AddInfo
			{
				Name = "Z0_AddInfoInt",
				DataType = "Int32"
			};

			var columnScript = SqlHelper.GetColumnScript(addInfo, "Z0_AddInfo");

			var expectedValue =
				"TRY_CONVERT(INT, CASE WHEN CHARINDEX('*AddInfoInt=', '*'+Z0_AddInfo) > 0 THEN REPLACE(SUBSTRING('*'+Z0_AddInfo, CHARINDEX('*AddInfoInt=', '*'+Z0_AddInfo) + 12, CHARINDEX('*', '*'+Z0_AddInfo+'*', CHARINDEX('*AddInfoInt=', '*'+Z0_AddInfo) + 1) - (CHARINDEX('*AddInfoInt=', '*'+Z0_AddInfo) + 12)), '¤', '*') ELSE NULL END)";

			AssertEquals(expectedValue, columnScript);

			addInfo.IsNullable = false;
			columnScript = SqlHelper.GetColumnScript(addInfo, "Z0_AddInfo");
			expectedValue =
				"TRY_CONVERT(INT, CASE WHEN CHARINDEX('*AddInfoInt=', '*'+Z0_AddInfo) > 0 THEN REPLACE(SUBSTRING('*'+Z0_AddInfo, CHARINDEX('*AddInfoInt=', '*'+Z0_AddInfo) + 12, CHARINDEX('*', '*'+Z0_AddInfo+'*', CHARINDEX('*AddInfoInt=', '*'+Z0_AddInfo) + 1) - (CHARINDEX('*AddInfoInt=', '*'+Z0_AddInfo) + 12)), '¤', '*') ELSE '' END)";
			AssertEquals(expectedValue, columnScript);
		}

		public void TestGetColumnDefinition()
		{
			var addInfo = new AddInfo
			{
				Name = "Z0_AddInfoString35",
				DataType = "String",
				Precision = null,
				Scale = null,
				MaxLength = 35,
				Indexed = false,
				IsUnicode = false
			};

			var columnDefinition = SqlHelper.GetColumnDefinition(addInfo, "Z0_AddInfo");

			var columnDefinitionExpected =
				"Z0_AddInfoString35 = CONVERT(VARCHAR(35), CASE WHEN CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) > 0 THEN REPLACE(SUBSTRING('*'+Z0_AddInfo, CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) + 17, CHARINDEX('*', '*'+Z0_AddInfo+'*', CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) + 1) - (CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) + 17)), '¤', '*') ELSE '' END)";

			AssertEquals(columnDefinitionExpected, columnDefinition);
		}
	}
}
