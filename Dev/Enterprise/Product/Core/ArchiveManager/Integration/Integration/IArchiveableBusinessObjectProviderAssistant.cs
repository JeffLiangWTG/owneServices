using CargoWise.EntityFramework;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveableBusinessObjectProviderAssistant : IArchiveableBusinessObjectProviderBase
	{
		string CountryCode { get; }
		IArchiveableBusinessObject LoadArchiveableBusinessObject(BusinessObject businessObjectLoadedByProvider);
	}
}
