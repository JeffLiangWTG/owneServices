using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStoragePack))]
sealed class TemporaryStoragePackTest : EnterpriseBusinessObjectTestCase
{
	public void TestValidationType()
	{
		var pack = GetNewBusinessObject(Factory);
		AssertType<TemporaryStoragePackValidation>(pack.Validation);
	}

	public void TestReadOnlyPropertiesWhenCustomsStatusAMG()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		var pack = (TemporaryStoragePack)bill.Packs.AddNew();
		header.CustomsStatus = "TSA";
		AssertEquals("When Customs status != AMG", false, pack.ContainerPKInfo.ReadOnly);

		header.CustomsStatus = "AMG";
		AssertEquals("When Customs status = AMG", true, pack.ContainerPKInfo.ReadOnly);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	TemporaryStoragePack GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		var pack = (TemporaryStoragePack)bill.Packs.AddNew();
		return pack;
	}
}
