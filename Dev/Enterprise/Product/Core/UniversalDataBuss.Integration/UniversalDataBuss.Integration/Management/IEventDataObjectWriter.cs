namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IEventDataObjectWriter : ITopLevelDataObjectWriter
	{
		bool PopulateAdditionalContexts { get; set; }
	}
}