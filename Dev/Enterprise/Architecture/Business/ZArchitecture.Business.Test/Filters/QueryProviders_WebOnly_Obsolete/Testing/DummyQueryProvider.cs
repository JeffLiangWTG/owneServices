using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyQueryProvider : IQueryProvider
	{
		public ZQuery GetQuery(SQLComparisonOperator @operator, object value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(OrgHeaderSchema.OH_Code, @operator, value);
			return query;
		}
	}
}
