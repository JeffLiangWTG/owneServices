using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageContainer))]
	class CusTempStorageContainerTest : CusCodeDataTest<CusTempStorageContainer>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(CusCodeDataTypeList.Codes.TempStorageContainer, tempStorageContainer.CY_Type);
		}

		public void TestLookupsType()
		{
			CusTempStorageContainer newCusTempStorageContainer = GetNewTempStorageContainer();
			Assertion.AssertType(GetLookupType(), newCusTempStorageContainer.Lookups);
		}

		protected Type GetLookupType() => typeof(CusTempStorageContainerLookups);

		#region Implementation

		protected override IEnumerable<CusTempStorageContainer> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return GetNewTempStorageContainer(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewTempStorageContainer(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewTempStorageContainer();
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			tempStorageContainer = GetNewTempStorageContainer();
		}
		CusTempStorageContainer tempStorageContainer;

		CusTempStorageContainer GetNewTempStorageContainer(BusinessObjectFactory factory = null)
		{
			var currentFactory = factory ?? Factory;
			var storageHeader = currentFactory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_OH_Customer = currentFactory.NewWithValidTestData<OrgHeader>().PK;
			var storageDec = ISTCusTempStorageDec.New(storageHeader);
			return storageDec.CusTempStorageContainers.AddNew();
		}
	}
}
