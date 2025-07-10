using System;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	sealed class FilterForTest : IFilter
	{
		public string WhereClause() => "";

		public SqlParameterList SqlParameters() => new SqlParameterList();

		public string GetXmlFragment() => "";

		void IFilter.SafeCopyValuesFrom(IFilter source) => throw new NotImplementedException();

		void IFilter.ClearValues() => throw new NotImplementedException();
	}
}
