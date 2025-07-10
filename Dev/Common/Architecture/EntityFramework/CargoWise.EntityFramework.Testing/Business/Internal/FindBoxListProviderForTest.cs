namespace CargoWise.EntityFramework.Testing
{
	public class FindBoxListProviderForTest : FindBoxListProvider
	{
		public FindBoxListProviderForTest(IBusinessObjectCollection collection)
			: base(collection)
		{ }

		public ZQuery LastIsActiveQuery;
		protected override void AddIsActiveFilter(ZQuery query, string code)
		{
			base.AddIsActiveFilter(query, code);
			LastIsActiveQuery = query;
		}
	}
}
