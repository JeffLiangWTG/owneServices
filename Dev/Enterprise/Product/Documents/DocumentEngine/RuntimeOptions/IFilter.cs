namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public interface IFilter
	{
		string WhereClause();
		SqlParameterList SqlParameters();
		void SafeCopyValuesFrom(IFilter source);
		void ClearValues();
	}
}
