using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public delegate void AddToQueryDelegate(ZQuery query, SQLComparisonOperator @operator, object value);

	public class ZDelegateQueryProvider : IQueryProvider
	{
		public ZDelegateQueryProvider(AddToQueryDelegate @delegate)
		{
			this.Delegate = @delegate;
		}

		public readonly AddToQueryDelegate Delegate;

		public ZQuery GetQuery(SQLComparisonOperator @operator, object value)
		{
			ZQuery result = new ZQuery();
			Delegate(result, @operator, value);
			return result;
		}
	}
}
