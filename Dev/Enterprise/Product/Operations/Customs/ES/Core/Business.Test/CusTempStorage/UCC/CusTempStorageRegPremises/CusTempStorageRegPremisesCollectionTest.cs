using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(CusTempStorageRegPremisesCollection))]
sealed class CusTempStorageRegPremisesCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageRegPremisesCollection>
{
	public void TestTypeFiltering()
	{
		var premises1 = Factory.New<CusTempStorageRegPremises>();
		premises1.SRP_Code = "premises1";
		premises1.SRP_Type = EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;

		var premises2 = Factory.New<CusTempStorageRegPremises>();
		premises2.SRP_Code = "premises2";
		premises2.SRP_Type = EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;

		var premises3 = Factory.New<CusTempStorageRegPremises>();
		premises3.SRP_Code = "premises3";
		premises3.SRP_Type = EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;

		var premises4 = Factory.New<CusTempStorageRegPremises>();
		premises4.SRP_Code = "premises4";
		premises4.SRP_Type = EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;

		CombineAssertions(() =>
		{
			var list = new CusTempStorageRegPremisesCollection(Factory, EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse);
			AssertEquals("Module filter is set to TemporaryStorageWarehouse", EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse, list.FilterBusinessObjectDefaults["Type:Property"].Value);

			list = new CusTempStorageRegPremisesCollection(Factory, EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility);
			AssertEquals("Module filter is set to ExportStorageFacility", EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility, list.FilterBusinessObjectDefaults["Type:Property"].Value);

			list = new CusTempStorageRegPremisesCollection(Factory);
			AssertEquals("all saved premises are returned", 4, list.Count);
		});
	}

	protected override CusTempStorageRegPremisesCollection GetCollectionToTest() => new CusTempStorageRegPremisesCollection(Factory);
}
