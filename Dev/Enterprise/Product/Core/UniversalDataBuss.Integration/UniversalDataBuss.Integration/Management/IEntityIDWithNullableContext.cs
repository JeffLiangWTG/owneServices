namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IEntityIDWithNullableContext : IEntityID
	{
		DataContextType? NullableDataContextType { get; }
	}
}
