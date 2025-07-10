using System;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	[Serializable]
	internal class DummyFilter : IFilter
	{
		public DummyFilter(string whereClause)
		{
			fWhereClause = whereClause;
		}

		public string WhereClause()
		{
			return fWhereClause;
		}

		public SqlParameterList SqlParameters()
		{
			SqlParameterList dummy = new SqlParameterList();
			dummy.Add(new SqlParameter(fWhereClause, fWhereClause));
			return dummy;
		}

		public string GetXmlFragment()
		{
			return "";
		}

		void IFilter.SafeCopyValuesFrom(IFilter source)
		{
		}

		void IFilter.ClearValues()
		{
		}

		readonly string fWhereClause;
	}
}
