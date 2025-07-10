using System.Collections.Generic;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class SqlParameterList : List<SqlParameter>
	{
		public SqlParameterList()
			: base()
		{
		}

		public SqlParameterList(IEnumerable<SqlParameter> collection)
			: base(collection)
		{
		}
	}
}
