using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>))]
sealed class CusTempStorageRegHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>>
{
	public void TestGetSumARegister()
	{
		var testHeader1 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
		testHeader1.SRH_AppCode = AppCode;
		var testHeader2 = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
		testHeader2.SRH_AppCode = AppCode;
		AssertContainsExactElementsInAnyOrder(new [] { testHeader1.PK, testHeader2.PK }, GetCollectionToTest().Select(x => x.PK).ToArray());
	}

	public void TestSetDefaultsForNewElementCore()
	{
		var collection1 = new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, "IST", "STO");
		var testHeader1 = collection1.AddNew();
		AssertEquals("If more than one AppCode is passed to the constructor of collection, SRH_AppCode of the new element will not be assigned.", ZString.Empty, testHeader1.SRH_AppCode);

		var collection2 = new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, "IST");
		var testHeader2 = collection2.AddNew();
		AssertEquals("If only one AppCode is passed to the constructor of collection, SRH_AppCode of the new element will not be assigned.", "IST", testHeader2.SRH_AppCode);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var testHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
		testHeader.SRH_AppCode = AppCode;
		Factory.Save();
		return testHeader;
	}

	protected override CusTempStorageRegHeaderCollection<CusTempStorageRegHeader> GetCollectionToTest() => new (Factory, AppCode);

	const string AppCode = "SUM";
}
