namespace CargoWise.EntityFramework
{
	public static class OrderByClause
	{
		public const string Ascending = " ASC ";
		public const string Descending = " DESC ";

		public static string Get(OrderDirection direction)
		{
			return direction == OrderDirection.Ascending ? Ascending : Descending;
		}
	}
}
