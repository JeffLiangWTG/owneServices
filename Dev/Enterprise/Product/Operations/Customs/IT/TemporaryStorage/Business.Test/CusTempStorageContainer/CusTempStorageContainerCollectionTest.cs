using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageContainerCollection))]
sealed class CusTempStorageContainerCollectionTest : CusCodeDataCollectionTest<CusTempStorageContainer>
{
	protected override CusCodeDataCollection<CusTempStorageContainer> GetCusCodeDataCollection()
	{
		return new CusTempStorageContainerCollection(StorageRegLine);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var storageContainer = StorageRegLine.Containers.AddNew();
		storageContainer.CY_ParentID = storageRegLine.PK;
		storageContainer.CY_ParentTableCode = CusTempStorageRegLineSchema.Constants.Prefix;
		return storageContainer;
	}

	CusTempStorageRegLine StorageRegLine => storageRegLine ??= Factory.New<CusTempStorageRegHeader>().CusTempStorageRegLines.AddNew();
	CusTempStorageRegLine storageRegLine;
}
