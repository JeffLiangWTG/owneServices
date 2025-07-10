namespace CargoWise.EntityFramework
{
	public interface IColumnIndexer : IIndexer
	{
		void Delete();
		string TableName { get; }
	}

	public interface IIndexer
	{
		object this[string propertyName] { get; set; }
	}
}
