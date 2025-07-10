namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IDataContextCoordinator
	{
		// Enforces uniqueness for matching
		string GetUniqueContextIdentifier(IXmlEventValueObject xmlEvent);
	}
}
