using System.Data;

namespace Enterprise.DocumentEngine
{
	internal class ReplacementWithSqlDbType
	{
		internal ReplacementWithSqlDbType(object macroValue, SqlDbType sqlDbType)
		{
			MacroValue = macroValue;
			SqlDbType = sqlDbType;
		}

		internal object MacroValue;
		internal SqlDbType SqlDbType;
	}
}