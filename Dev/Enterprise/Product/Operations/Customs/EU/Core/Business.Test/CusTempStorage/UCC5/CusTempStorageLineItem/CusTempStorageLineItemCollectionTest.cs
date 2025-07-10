using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageLineItemCollection<CusTempStorageLineItem>))]
	public class CusTempStorageLineItemCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageLineItemCollection<CusTempStorageLineItem>>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusTempStorageLineItem>();

		protected override CusTempStorageLineItemCollection<CusTempStorageLineItem> GetCollectionToTest() => new CusTempStorageLineItemCollection<CusTempStorageLineItem>(Factory.New<CusTempStorageLine>());
	}
}
