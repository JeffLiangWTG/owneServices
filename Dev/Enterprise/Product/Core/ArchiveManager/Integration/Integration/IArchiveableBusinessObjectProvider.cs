using CargoWise.EntityFramework;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveableBusinessObjectProvider : IArchiveableBusinessObjectProviderBase
	{
		IArchiveableBusinessObject[] LoadArchiveableBusinessObjects(IArchiveItem item, BusinessObjectFactory factory);
	}
}
