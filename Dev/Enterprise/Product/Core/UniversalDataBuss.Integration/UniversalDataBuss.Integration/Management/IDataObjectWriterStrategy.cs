namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IDataObjectWriterStrategy
	{
		bool IsAllowSet(string fieldName);
	}
}
