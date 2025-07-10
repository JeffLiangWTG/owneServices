namespace Enterprise.UniversalDataBuss.Integration
{
	public interface ISettableWriterStrategy
	{
		void SetWriterStrategy(IDataObjectWriterStrategy strategy);
	}
}
