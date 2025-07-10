using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public interface IQueryProvider
	{
		ZQuery GetQuery(SQLComparisonOperator @operator, object value);
	}
}
