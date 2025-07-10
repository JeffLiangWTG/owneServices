using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageRegHeaderCollection))]
	public class CusTempStorageRegHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageRegHeaderCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var testHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			testHeader.SRH_AppCode = AppCode;
			Factory.Save();
			return testHeader;
		}

		protected override CusTempStorageRegHeaderCollection GetCollectionToTest() => new CusTempStorageRegHeaderCollection(Factory);

		const string AppCode = FRConstants.TemporaryStorage.AppCodeIST;
	}
}
