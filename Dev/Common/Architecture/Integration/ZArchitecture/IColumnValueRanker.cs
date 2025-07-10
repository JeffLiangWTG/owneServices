namespace CargoWise.Integration
{
	public interface IColumnValueRanker
	{
		void Add(object schemaColumn, params object[] values);
	}
}
