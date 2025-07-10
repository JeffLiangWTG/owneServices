using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.FR.DocumentWrappers.TempStorageHeader;

public class DocTempStorageLineCollection : DocBaseWrapperCollection<DocTempStorageLine>
{
	public DocTempStorageLineCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	public DocTempStorageLineCollection(IBusinessObjectCollection collectionToWrap, BusinessObjectFactory factory) : base(collectionToWrap, factory)
	{
	}
}
