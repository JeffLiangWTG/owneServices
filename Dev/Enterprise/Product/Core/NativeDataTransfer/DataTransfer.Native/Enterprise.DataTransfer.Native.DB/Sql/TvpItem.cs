using System.Collections;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.DB
{
	public class TvpItem
	{
		public TvpItem(string tableName, string columnName, string paramName, IEnumerable values)
		{
			this.tableName = tableName;
			this.columnName = columnName;
			Values = values;
			ParamName = paramName;
		}
		readonly string tableName;
		readonly string columnName;
		public IEnumerable Values { get; }
		public string ParamName { get; }

		public void AddTvpParamToCommand(DbCommand command)
		{
			command.AddTableValuedParameter(ParamName, EnterpriseSchema.GetTableSchema(tableName).GetSchemaColumn(columnName), Values);
		}
	}
}
