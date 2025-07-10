using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageContainerCollection))]
	class CusTempStorageContainerCollectionTest : CusCodeDataCollectionTest<CusTempStorageContainer>
	{
		protected override CusCodeDataCollection<CusTempStorageContainer> GetCusCodeDataCollection()
		{
			return new CusTempStorageContainerCollection(StorageDec);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var storageContainer = StorageDec.CusTempStorageContainers.AddNew();
			storageContainer.CY_ParentID = StorageDec.PK;
			storageContainer.CY_ParentTableCode = CusTempStorageDecSchema.Constants.Prefix;
			return storageContainer;
		}

		public void TestUpdateParentContainerCount()
		{
			var storageHeader = StorageDec.StorageHeader;
			var container1 = StorageDec.CusTempStorageContainers.AddNew();
			var container2 = StorageDec.CusTempStorageContainers.AddNew();
			var container3 = StorageDec.CusTempStorageContainers.AddNew();
			AssertEquals("StorageHeader.SJH_ContainerCount", 3, storageHeader.SJH_ContainerCount);

			StorageDec.CusTempStorageContainers.Remove(container1);
			AssertEquals("StorageHeader.SJH_ContainerCount", 2, storageHeader.SJH_ContainerCount);
		}

		CusTempStorageDec StorageDec
		{
			get
			{
				if (storageDec == null)
				{
					var storageHeader = CusTempStorageJobHeader.New(Factory);
					storageHeader.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
					storageDec = storageHeader.CusTempStorageDec;
				}
				return storageDec;
			}
		}
		CusTempStorageDec storageDec;
	}
}
