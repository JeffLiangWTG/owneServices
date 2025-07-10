using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class QueryTextAndParameters
	{
		public string QueryText { get; }

		public ZSqlParameterCollection Parameters { get; }

		public QueryTextAndParameters(string queryText, ZSqlParameterCollection parameters)
		{
			QueryText = queryText;
			Parameters = parameters;
		}

		public QueryTextAndParameters(string queryText, ZSqlParameter[] parameters)
		{
			QueryText = queryText;
			Parameters = new ZSqlParameterCollection();

			foreach (var param in parameters)
			{
				Parameters.Add(param);
			}
		}
	}
}
