using CargoWise.EntityFramework;

namespace Enterprise.Integration
{
	public interface IBusinessObjectReaderProvider
	{
		BusinessObjectReader BusinessObjectReaderWithFilter { get; }
	}
}
