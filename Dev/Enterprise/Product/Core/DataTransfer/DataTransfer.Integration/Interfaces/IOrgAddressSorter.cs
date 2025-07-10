using CargoWise.EntityFramework;

namespace Enterprise.DataTransfer.Integration
{
	public interface IOrgAddressSorter
	{
		StringCollectionX GetReferences(string ediCode);
	}
}
