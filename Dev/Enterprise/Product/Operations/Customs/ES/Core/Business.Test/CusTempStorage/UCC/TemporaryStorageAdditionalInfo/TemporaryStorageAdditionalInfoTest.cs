using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageAdditionalInfo))]
	sealed class TemporaryStorageAdditionalInfoTest : CusSupportingInfoTest<TemporaryStorageAdditionalInfo>
	{
		public void TestLookups()
		{
			AssertType<TemporaryStorageAdditionalInfoLookups>(addInfo.Lookups);
		}

		protected override IEnumerable<TemporaryStorageAdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var addInfo = bill.AdditionalInfos.AddNew();
			yield return addInfo;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => addInfo;

		protected override BusinessObject GetNewBusinessObject() => addInfo;

		protected override void SetUp()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			addInfo = bill.AdditionalInfos.AddNew();
		}
		TemporaryStorageAdditionalInfo addInfo;
	}
}
