using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class ZComparisonQueryProvider : IQueryProvider
	{
		public ZComparisonQueryProvider(SchemaColumn column)
		{
			this.Column = column;
		}

		public ZQuery GetQuery(SQLComparisonOperator @operator, object value)
		{
			ZQuery result = new ZQuery();
			if (Column != null)
			{
				result.AddToFilter(JoinCondition.Or, Column, @operator, value);
			}
			return result;
		}

		#region Implementation

		protected readonly SchemaColumn Column;

		#endregion
	}
}
