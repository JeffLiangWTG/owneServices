using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegLine))]
sealed class CusTempStorageRegLineTest : EnterpriseBusinessObjectTestCase
{
	public void TestCusTempStorageContainers()
	{
		AssertType<CusTempStorageContainerCollection>(regLine.Containers);
		AssertNotNull("CusTempStorageContainers", regLine.Containers);
	}

	public void TestGetCusCodeDataTypes()
	{
		var dataTypes = regLine.GetCusCodeDataTypes();

		AssertNotNull("GetCusCodeDataTypes", dataTypes);
		AssertEquals("Number of data types", 1, dataTypes.Count);
		AssertEquals(typeof(CusTempStorageContainer), dataTypes[CusCodeDataTypeList.Codes.TemporaryStorageContainer]);
	}

	public void TestGetFetchStrategies()
	{
		var fetchStrategies = regLine.GetFetchStrategies();

		AssertNotNull("GetFetchStrategies", fetchStrategies);

		var strategiesList = fetchStrategies.ToList();
		AssertType<CusCodeDataTypeSupporterFetchStrategy>(strategiesList[0]);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<CusTempStorageRegHeader>();
		header.SRH_Reference = "TEST";
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		return line;
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_Reference = "TEST";
		regLine = header.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
	}
	CusTempStorageRegLine regLine;
}

