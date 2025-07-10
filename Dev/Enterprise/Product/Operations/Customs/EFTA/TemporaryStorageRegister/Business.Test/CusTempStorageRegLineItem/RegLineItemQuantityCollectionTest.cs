using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing
{
	[TestedType(typeof(RegLineItemQuantityCollection))]
	sealed class RegLineItemQuantityCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RegLineItemQuantityCollection>
	{
		protected override RegLineItemQuantityCollection GetCollectionToTest()
		{
			return new RegLineItemQuantityCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var regLine = Factory.New<CusTempStorageRegLine>();
			var regItem = Factory.New<CusTempStorageRegLineItem>();
			var collection = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(regLine);
			var pivot = collection.AddChild(regItem);
			return RegLineItemQuantity.LoadNew(pivot);
		}
	}
}
