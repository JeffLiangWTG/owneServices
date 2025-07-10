using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(AsycudaContainer))]
sealed class AsycudaContainerTest : EnterpriseBusinessObjectTestCase
{
	[ExpectNoExceptions]
	public void TestACN_SetPointTemperatureAttributes()
	{
		var container = CreateBusinessObject(Factory);
		var resData = DataBoundResourceStrings.GetDataForProperty(container.ACN_SetPointTemperatureInfo);
		NUnit.Framework.Assert.That(resData.Caption, Is.EqualTo("Temperature"), "Caption");
	}

	[ExpectNoExceptions]
	public void TestACN_SetPointTemperatureUnitAttributes()
	{
		var container = CreateBusinessObject(Factory);
		var resData = DataBoundResourceStrings.GetDataForProperty(container.ACN_SetPointTemperatureUnitInfo);
		NUnit.Framework.Assert.That(resData.Caption, Is.EqualTo("Temperature UQ"), "Caption");
	}

	[ExpectNoExceptions]
	public void TestAsycudaContainerLookups()
	{
		var container = CreateBusinessObject(Factory);
		NUnit.Framework.Assert.That(container.Lookups, Is.TypeOf<AsycudaContainerLookups>());
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return CreateBusinessObject(Factory);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<AsycudaManifestHeader>().Containers.AddNew();

	AsycudaContainer CreateBusinessObject(BusinessObjectFactory factory)
	{
		var manifestHeader = factory.New<AsycudaManifestHeader>();
		return manifestHeader.Containers.AddNew();
	}

	protected override BusinessObject GetBusinessObjectForFetchForLoad()
	{
		return GetNewBusinessObject();
	}
}
