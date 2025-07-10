namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IEntityID
	{
		DataContextType DataContextType { get; }
		string DataContextKey { get; }
	}
}
