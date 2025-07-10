using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageRegLineCollection))]
	public class CusTempStorageRegLineCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageRegLineCollection>
	{
		protected override CusTempStorageRegLineCollection GetCollectionToTest() => new CusTempStorageRegLineCollection(Factory.New<CusTempStorageRegHeader>());
	}
}
