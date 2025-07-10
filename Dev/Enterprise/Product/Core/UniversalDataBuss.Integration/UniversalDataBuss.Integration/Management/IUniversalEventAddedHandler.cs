namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IUniversalEventAddedHandler
	{
		void UniversalEventAdded(IXmlImportLogger logger, IXmlEventValueObject eventDataObject);
	}
}
