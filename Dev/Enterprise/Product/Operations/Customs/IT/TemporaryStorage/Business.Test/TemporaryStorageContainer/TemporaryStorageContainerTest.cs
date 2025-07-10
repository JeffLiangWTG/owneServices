using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageContainer))]
sealed class TemporaryStorageContainerTest : EnterpriseBusinessObjectTestCase
{
	public void TestHeader()
	{
		var container = (TemporaryStorageContainer)GetNewBusinessObject();
		AssertType<TemporaryStorageHeader>(container.Header);
	}

	public void TestReadOnlyPropertiesWhenCustomsStatusAMG()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		header.CustomsStatus = "AMG";
		var container = header.Containers.AddNew();

		CombineAssertions("When Customs status = AMG", () =>
		{
			AssertEquals("Conatiners should be read-only", true, container.ReadOnly);

			var additionalSeals = container.AdditionalSeals.AddNew();
			AssertEquals("Additional Seals should be read-only", true, additionalSeals.ReadOnly);
		});
	}

	public void TestAdditionalSeals()
	{
		var container = (TemporaryStorageContainer)GetNewBusinessObject();
		AssertType<CusSealCollection>(container.AdditionalSeals);
	}

	public void TestCusSealType()
	{
		var container = (TemporaryStorageContainer)GetNewBusinessObject();
		AssertType(((ICusSealTypeSupporter)container).CusSealType, container.AdditionalSeals.AddNew());
	}

	public void TestValidationType()
	{
		var container = (TemporaryStorageContainer)GetNewBusinessObject();
		AssertType<TemporaryStorageContainerValidation>(container.Validation);
	}

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = factory.NewWithValidTestData<TemporaryStorageHeader>();
		return header.Containers.AddNew();
	}
}
