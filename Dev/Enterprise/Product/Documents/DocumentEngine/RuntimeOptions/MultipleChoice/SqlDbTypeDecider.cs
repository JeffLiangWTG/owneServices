using System.Data;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	static class SqlDbTypeDecider
	{
		internal static void UpdateParamSqlDbTypeBasedOnStringValue(SqlParameter param)
		{
			if (param.Value is string stringValue)
			{
				if (new ZString(stringValue).IsWindows1252OrEmpty)
				{
					switch (param.SqlDbType)
					{
						case SqlDbType.NChar:
							param.SqlDbType = SqlDbType.Char;
							break;
						case SqlDbType.NVarChar:
							param.SqlDbType = SqlDbType.VarChar;
							break;
					}
				}
				else
				{
					switch (param.SqlDbType)
					{
						case SqlDbType.Char:
							param.SqlDbType = SqlDbType.NChar;
							break;
						case SqlDbType.VarChar:
							param.SqlDbType = SqlDbType.NVarChar;
							break;
					}
				}
			}
		}
	}
}
